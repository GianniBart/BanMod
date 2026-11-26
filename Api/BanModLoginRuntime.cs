//credits and licenses in the resources folder/
using System;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using UnityEngine;

namespace BanMod
{
    internal static class BanModLoginSubmitBridge
    {
        private static readonly object Sync = new object();
        private static Action<string> _callback;

        public static void SetCallback(Action<string> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            lock (Sync)
                _callback = callback;
        }

        public static void Submit(string json)
        {
            Action<string> callback;
            lock (Sync)
                callback = _callback;

            if (callback == null)
                throw new InvalidOperationException("The login submission callback is not available.");

            callback(json ?? "{}");
        }

        public static void ClearCallback()
        {
            lock (Sync)
                _callback = null;
        }
    }

    public sealed class BanModLoginRuntimeHost
    {
        private static readonly object UiSync = new object();
        private static readonly object DisableSync = new object();
        private static bool _disableApplied;
        private static string _pendingModelJson = "";
        private static Action<string> _pendingSubmit;

        internal static bool HasPendingLoginMenu
        {
            get
            {
                if (BanModLoginRuntime.IsReady)
                    return false;

                lock (UiSync)
                    return _pendingSubmit != null && !string.IsNullOrWhiteSpace(_pendingModelJson);
            }
        }

        internal static void TryPresentPendingLoginMenu()
        {
            string modelJson;
            Action<string> callback;
            lock (UiSync)
            {
                modelJson = _pendingModelJson;
                callback = _pendingSubmit;
            }

            if (callback == null || string.IsNullOrWhiteSpace(modelJson))
                return;

            try
            {
                BanModLoginUi.EnsureCreated();
                if (BanModLoginUi.Instance == null)
                    return;

                BanModLoginSubmitBridge.SetCallback(callback);
                if (!BanModLoginUi.IsOpen)
                {
                    BanModLoginUi.Instance.ShowFromJson(modelJson);
                    Debug.Log("[BANMOD][LOGIN] Login UI opened.");
                }
            }
            catch (Exception ex)
            {
                try { Debug.LogError("[BANMOD][LOGIN] Pending UI presentation failed: " + ex); } catch { }
            }
        }

        private static void ClearPendingLoginMenu()
        {
            lock (UiSync)
            {
                _pendingModelJson = "";
                _pendingSubmit = null;
            }
        }
        public string GetClientToken()
        {
            return BanModCore.GetCurrentClientToken();
        }

        public string GetSnapshotJson()
        {
            var payload = new
            {
                api_base_url = BanModCore.PublicApiBaseUrl,
                friend_code = BanModCore.GetCurrentFriendCode(),
                client_token = BanModCore.GetCurrentClientToken(),
                player_name = BanModCore.GetCurrentPlayerName(),
                banmod_sha256 = BanModCore.GetCurrentBanModSha256(),
                build_id = BanModCore.GetCurrentBuildId(),
                login_bin_sha256 = BanModLoginRuntime.LoginBinSha256,
                login_bin_version = BanModLoginRuntime.LoginBinVersion,
                mod_disabled = BanMod.IsBanModDisabled
            };

            return JsonSerializer.Serialize(payload);
        }

        public void ShowLoginMenu(string modelJson, Action<string> onSubmit)
        {
            if (onSubmit == null)
                throw new ArgumentNullException(nameof(onSubmit));

            lock (UiSync)
            {
                _pendingModelJson = modelJson ?? "{}";
                _pendingSubmit = onSubmit;
            }

            try { Debug.Log("[BANMOD][LOGIN] Login UI queued."); } catch { }
            TryPresentPendingLoginMenu();
        }

        public void SetLoginStatus(string message, bool isError)
        {
            try { BanModLoginUi.Instance?.SetStatus(message, isError); } catch { }
        }

        public void ShowLoginLoading(string message)
        {
            try
            {
                BanModLoginUi.EnsureCreated();
                BanModLoginUi.Instance?.ShowLoading(message);
            }
            catch { }
        }

        public void SetLoginBusy(bool busy, string message)
        {
            try { BanModLoginUi.Instance?.SetBusy(busy, message); } catch { }
        }

        public void CloseLoginMenu()
        {
            ClearPendingLoginMenu();
            try { BanModLoginUi.Instance?.Close(); } catch { }
            BanModLoginSubmitBridge.ClearCallback();
        }

        public void MarkLoginReady()
        {
            ClearPendingLoginMenu();
            BanModLoginSubmitBridge.ClearCallback();
            BanModLoginRuntime.MarkReady();
        }

        public void RequestPremiumRefresh()
        {
            BanModCore.RequestPremiumRefresh();
        }

        public void DisableMod(string reason)
        {
            ApplyServerDisable(reason);
        }

        public bool ShowModCompatibilityNotice(bool cheatDetected)
        {
            return ModCompatibilityNotices.Show(cheatDetected);
        }

        public bool ForceDisableModAfterCheatNotice(string reason)
        {
            return ModCompatibilityNotices.DisableAfterNotice(() => ApplyServerDisable(reason));
        }

        public void ForceDisableMod(string reason)
        {
            ApplyServerDisable(reason);
        }

        private static void ApplyServerDisable(string reason)
        {
            lock (DisableSync)
            {
                if (_disableApplied)
                    return;

                _disableApplied = true;
            }

            try
            {
                Debug.LogWarning(
                    "[BANMOD][LOGIN] Server forcedisable accepted: " +
                    (reason ?? ""));
            }
            catch { }

            try
            {
                BanMod.DisableMod();
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.LogError(
                        "[BANMOD][LOGIN] Forcedisable bridge failed: " + ex);
                }
                catch { }
            }
        }

        public void LogInfo(string message)
        {
            try { Debug.Log("[BANMOD][LOGIN] " + (message ?? "")); } catch { }
        }

        public void LogWarning(string message)
        {
            try { Debug.LogWarning("[BANMOD][LOGIN] " + (message ?? "")); } catch { }
        }
    }

    internal static class BanModLoginRuntime
    {
        private const string RequiredTypeName = "BanMod.Login.LoginModule";
        private const string RequiredModuleId = "login";

        private static object _moduleInstance;
        private static MethodInfo _shutdownMethod;
        private static bool _ready;
        private static bool _loaded;

        public static bool IsReady => _ready;
        public static bool IsLoaded => _loaded;
        public static string LoginBinSha256 { get; private set; } = "";
        public static string LoginBinVersion { get; private set; } = "";

        public static bool LoadAndStart(byte[] bytes, string sha256, string manifestVersion, out string error)
        {
            error = "";

            if (bytes == null || bytes.Length == 0)
            {
                error = "login.bin is empty.";
                return false;
            }

            try
            {
                Assembly assembly = Assembly.Load(bytes);
                Type moduleType = assembly.GetType(RequiredTypeName, false, false);
                if (moduleType == null || moduleType.IsAbstract)
                    throw new MissingMethodException("Required type not found: " + RequiredTypeName);

                object module = Activator.CreateInstance(moduleType, true);
                if (module == null)
                    throw new InvalidOperationException("Could not create login module instance.");

                string moduleId = ReadStringProperty(moduleType, module, "ModuleId");
                if (!string.Equals(moduleId, RequiredModuleId, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Invalid login module id: " + moduleId);

                string moduleVersion = ReadStringProperty(moduleType, module, "ModuleVersion");
                if (string.IsNullOrWhiteSpace(moduleVersion))
                    moduleVersion = manifestVersion ?? "";

                MethodInfo run = moduleType.GetMethod(
                    "Run",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[] { typeof(object) },
                    null);

                if (run == null)
                    throw new MissingMethodException(moduleType.FullName, "Run(object)");

                object routineObject = run.Invoke(module, new object[] { new BanModLoginRuntimeHost() });
                IEnumerator routine = routineObject as IEnumerator;
                if (routine == null)
                    throw new InvalidOperationException("Run(object) did not return IEnumerator.");

                if (AmongUsClient.Instance == null)
                    throw new InvalidOperationException("AmongUsClient is not ready.");

                _moduleInstance = module;
                _shutdownMethod = moduleType.GetMethod("Shutdown", BindingFlags.Public | BindingFlags.Instance);
                LoginBinSha256 = sha256 ?? "";
                LoginBinVersion = moduleVersion;
                _ready = false;
                _loaded = true;

                AmongUsClient.Instance.StartCoroutine(routine.WrapToIl2Cpp());
                return true;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                _moduleInstance = null;
                _shutdownMethod = null;
                _ready = false;
                _loaded = false;
                return false;
            }
        }

        public static void MarkReady()
        {
            _ready = true;
        }

        public static void ResetReady()
        {
            _ready = false;
        }

        public static void Shutdown()
        {
            try { _shutdownMethod?.Invoke(_moduleInstance, null); } catch { }
            try { BanModLoginUi.Instance?.Close(); } catch { }
            BanModLoginSubmitBridge.ClearCallback();
            _moduleInstance = null;
            _shutdownMethod = null;
            _loaded = false;
            _ready = false;
            LoginBinSha256 = "";
            LoginBinVersion = "";
        }

        private static string ReadStringProperty(Type type, object instance, string name)
        {
            try
            {
                PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                return property != null ? property.GetValue(instance, null)?.ToString() ?? "" : "";
            }
            catch
            {
                return "";
            }
        }


        internal static void LogInfo(string message)
        {
            _ = message;
        }

        internal static void LogWarning(string message)
        {
            _ = message;
        }
    }


    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
    internal static class BanModLoginUiBootstrapPatch
    {
        public static void Postfix()
        {
            try
            {
                BanModLoginUi.EnsureCreated();
                if (BanModLoginRuntimeHost.HasPendingLoginMenu)
                    BanModLoginRuntimeHost.TryPresentPendingLoginMenu();
            }
            catch (Exception ex)
            {
                try { Debug.LogError("[BANMOD][LOGIN] UI bootstrap failed: " + ex); } catch { }
            }
        }
    }
}
