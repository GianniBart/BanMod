//credits and licenses in the resources folder
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static BanMod.Utils;
using Object = UnityEngine.Object;

namespace BanMod;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public class LobbyStartPatch
{
    public static bool hasSentSummary = false;
    public static bool hasSentSummary1 = false;

    public static void Postfix(LobbyBehaviour __instance)
    {
        Options.SyncLobbyGameModeFromVanilla();

        GameModeType gameMode = Options.GameMode.Selected;

        if (BanMod.AktiveLobby.Value)
        {
            __instance.StartCoroutine(
                LobbyRendererReplacer.ReplaceLobbySprites(__instance).WrapToIl2Cpp()
            );
        }

        __instance.StartCoroutine(
            LobbyStairsColliderFix.ApplyDelayed(__instance).WrapToIl2Cpp()
        );

        if (AmongUsClient.Instance.AmHost && !LobbyStartPatch.hasSentSummary1)
        {
            if (gameMode == GameModeType.FFA) return;
            if (gameMode == GameModeType.FFATeam) return;
            if (gameMode == GameModeType.KillRace) return;
            if (gameMode == GameModeType.HotPotatoModded) return;
            if (gameMode == GameModeType.Assassin) return;
            if (Options.EnableFFA.GetBool()) return;

            __instance.StartCoroutine(SendSummaryDelayed().WrapToIl2Cpp());
        }

        _ = CheaterManager.SyncFromServerAsync();
    }

    private static IEnumerator SendSummaryDelayed()
    {
        yield return new WaitForSeconds(5f);

        if (LobbyStartPatch.hasSentSummary ||
            LobbyStartPatch.hasSentSummary1)
        {
            yield break;
        }
        if (BanMod.EndGameForced)
        {
            Utils.SendMessage(Translator.GetString("Lobby_Error"));
            yield break;
        }
        if (!Options.SendSummary.GetBool())
            yield break;

        List<string> reports =
            MatchSummary1.GetLastSavedReports();

        bool sentAtLeastOne = false;

        foreach (string report in reports)
        {
            if (string.IsNullOrWhiteSpace(report))
                continue;

            Utils.SendMessage(report, 255);
            sentAtLeastOne = true;

            yield return new WaitForSeconds(0.75f);
        }

        if (sentAtLeastOne)
        {
            LobbyStartPatch.hasSentSummary1 = true;
        }
    }
}

public static class LobbyStairsColliderFix
{
    private static GameObject CustomColliderRoot;
    private static int AppliedLobbyInstanceId = int.MinValue;

    public static IEnumerator ApplyDelayed(LobbyBehaviour lobby)
    {
        yield return null;
        yield return null;
        yield return new WaitForSeconds(0.25f);

        Apply(lobby);
    }

    public static void Apply(LobbyBehaviour lobby)
    {
        if (lobby == null)
            return;
        if (!BanMod.AktiveLobby.Value)
            return;

        int lobbyInstanceId = lobby.GetInstanceID();
        if (AppliedLobbyInstanceId == lobbyInstanceId && CustomColliderRoot != null)
            return;

        if (AppliedLobbyInstanceId != lobbyInstanceId)
            CustomColliderRoot = null;

        DisableOriginalMainCollider(lobby);
        CreateCustomColliders(lobby);
        AppliedLobbyInstanceId = lobbyInstanceId;
    }

    private static void DisableOriginalMainCollider(LobbyBehaviour lobby)
    {
        Collider2D[] colliders = lobby.GetComponentsInChildren<Collider2D>(true);

        if (colliders == null || colliders.Length == 0)
            return;

        foreach (Collider2D collider in colliders)
        {
            if (collider == null)
                continue;

            string path = GetHierarchyPath(collider.transform, lobby.transform);

            if (path == "" && collider.gameObject.name.Contains("Lobby"))
            {
                collider.enabled = false;
            }
        }
    }

    private static void CreateCustomColliders(LobbyBehaviour lobby)
    {
        if (CustomColliderRoot != null)
            return;

        CustomColliderRoot = new GameObject("BanMod_Stairs_CustomColliders");
        CustomColliderRoot.transform.SetParent(lobby.transform, false);
        CustomColliderRoot.transform.localPosition = Vector3.zero;
        CustomColliderRoot.transform.localRotation = Quaternion.identity;
        CustomColliderRoot.transform.localScale = Vector3.one;


        AddBoxCollider(CustomColliderRoot, "Extra_Wall_00", new Vector2(0.017f, 3.057f), new Vector2(2.106f, 0.08f), 0f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_01", new Vector2(1.963f, 2.763f), new Vector2(1.896f, 0.08f), -17.486f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_02", new Vector2(-1.92f, 2.707f), new Vector2(1.95f, 0.08f), 20.456f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_03", new Vector2(-2.794f, 0.83f), new Vector2(3.05f, 0.08f), -89.823f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_04", new Vector2(2.873f, 0.94f), new Vector2(3.135f, 0.08f), -89.49f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_05", new Vector2(2.768f, -0.682f), new Vector2(0.377f, 0.08f), 51.047f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_06", new Vector2(-2.71f, -0.605f), new Vector2(0.394f, 0.08f), -54.104f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_07", new Vector2(-1.798f, -0.75f), new Vector2(1.472f, 0.08f), 0f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_08", new Vector2(1.868f, -0.75f), new Vector2(1.561f, 0.08f), 0f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_09", new Vector2(1.169f, -1.42f), new Vector2(1.235f, 0.08f), -86.538f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_10", new Vector2(-1.083f, -1.333f), new Vector2(1.053f, 0.08f), -94.843f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_11", new Vector2(2.067f, -1.32f), new Vector2(0.595f, 0.08f), -89.825f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_12", new Vector2(-1.436f, -1.836f), new Vector2(0.372f, 0.08f), 0f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_13", new Vector2(2.643f, -2.252f), new Vector2(2.958f, 0.08f), -89.087f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_14", new Vector2(1.778f, -2.105f), new Vector2(1.159f, 0.08f), 0f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_16", new Vector2(0.045f, -3.77f), new Vector2(5.243f, 0.08f), 0f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_17", new Vector2(-2.658f, -2.585f), new Vector2(0.1f, 0.08f), 0f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_20_A", new Vector2(-2.572f, -1.674f), new Vector2(1.927f, 0.08f), -89.773f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_20_B", new Vector2(2.272f, -2.237f), new Vector2(0.85f, 0.08f), -28.986f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_21_A", new Vector2(-2.528f, -3.67f), new Vector2(0.486f, 0.08f), -22.751f);
        AddBoxCollider(CustomColliderRoot, "Extra_Wall_21_B", new Vector2(-2.109f, -1.852f), new Vector2(0.85f, 0.08f), 0f);
    }

    private static void AddBoxCollider(
        GameObject parent,
        string name,
        Vector2 localPosition,
        Vector2 size,
        float rotationZ = 0f)
    {
        if (parent == null)
            return;

        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        obj.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
        obj.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        obj.transform.localScale = Vector3.one;

        BoxCollider2D box = obj.AddComponent<BoxCollider2D>();
        box.isTrigger = false;
        box.enabled = true;
        box.offset = Vector2.zero;
        box.size = size;
    }

    private static string GetHierarchyPath(Transform transform, Transform root)
    {
        List<string> names = new();

        Transform current = transform;

        while (current != null && current != root)
        {
            names.Add(current.name);
            current = current.parent;
        }

        names.Reverse();

        return string.Join("_", names);
    }
}

public static class LobbyRendererReplacer
{
    private const string ResourcePrefix = "BanMod.Resources.image.";
    private static readonly Dictionary<string, Sprite> LobbySpriteCache = new();

    private static bool UseCustomBackground => true;
    private static bool ShowBackground => true;
    private static bool CollideBackground => true;

    private static bool UseCustomRear => true;
    private static bool ShowRear => false;
    private static bool CollideRear => false;

    private static bool UseCustomLeftBox => true;
    private static bool ShowLeftBox = false;
    private static bool CollideLeftBox = false;

    private static bool UseCustomRightBox => true;
    private static bool ShowRightBox = false;
    private static bool CollideRightBox = false;

    private static bool UseCustomSmallBox => true;
    private static bool ShowSmallBox => false;
    private static bool CollideSmallBox => false;

    private static bool UseCustomSmallBoxPanel => true;
    private static bool ShowSmallBoxPanel => true;
    private static bool CollideSmallBoxPanel => true;

    private static bool UseCustomWardrobePanel => true;
    private static bool ShowWardrobePanel => true;
    private static bool CollideWardrobePanel => true;

    private sealed class SpriteReplacement
    {
        public string RendererPath;
        public string ResourceFileName;

        public Func<bool> UseCustomSpriteGetter;
        public Func<bool> ShowRendererGetter;
        public Func<bool> EnableCollisionGetter;

        public Vector3? LocalPosition;
        public Vector3? LocalScale;
        public float? LocalRotationZ;

        public bool UseCustomSprite => UseCustomSpriteGetter?.Invoke() ?? false;
        public bool ShowRenderer => ShowRendererGetter?.Invoke() ?? true;
        public bool EnableCollision => EnableCollisionGetter?.Invoke() ?? true;

        public SpriteReplacement(
            string rendererPath,
            string resourceFileName,
            Func<bool> useCustomSpriteGetter,
            Func<bool> showRendererGetter,
            Func<bool> enableCollisionGetter,
            Vector3? localPosition = null,
            Vector3? localScale = null,
            float? localRotationZ = null
        )
        {
            RendererPath = rendererPath;
            ResourceFileName = resourceFileName;

            UseCustomSpriteGetter = useCustomSpriteGetter;
            ShowRendererGetter = showRendererGetter;
            EnableCollisionGetter = enableCollisionGetter;

            LocalPosition = localPosition;
            LocalScale = localScale;
            LocalRotationZ = localRotationZ;
        }
    }

    private static readonly SpriteReplacement[] SpriteReplacements =
    {
        new SpriteReplacement(
            "Background",
            "000_Background__Dropship.png",
            () => UseCustomBackground,
            () => ShowBackground,
            () => CollideBackground
        ),

        new SpriteReplacement(
            "Background_dropship_rear",
            "001_Background_dropship_rear__dropship_rear.png",
            () => UseCustomRear,
            () => ShowRear,
            () => CollideRear
        ),

        new SpriteReplacement(
            "Leftbox",
            "004_Leftbox__box.png",
            () => UseCustomLeftBox,
            () => ShowLeftBox,
            () => CollideLeftBox
        ),

        new SpriteReplacement(
            "RightBox",
            "005_RightBox__box.png",
            () => UseCustomRightBox,
            () => ShowRightBox,
            () => CollideRightBox
        ),

        new SpriteReplacement(
            "SmallBox",
            "006_SmallBox__box.png",
            () => UseCustomSmallBox,
            () => ShowSmallBox,
            () => CollideSmallBox
        ),

        new SpriteReplacement(
            "SmallBox_Panel",
            "007_SmallBox_Panel__dropship_panel.png",
            () => UseCustomSmallBoxPanel,
            () => ShowSmallBoxPanel,
            () => CollideSmallBoxPanel,
            localPosition: new Vector3(-1.416f, -6.475f, 0f),
            localScale: new Vector3(1.333f, 1.333f, 1f),
            localRotationZ: null
        ),

        new SpriteReplacement(
            "panel_Wardrobe",
            "008_panel_Wardrobe__panel_Wardrobe.png",
            () => UseCustomWardrobePanel,
            () => ShowWardrobePanel,
            () => CollideWardrobePanel,
            localPosition: new Vector3(1.829f, -1.981f, -9.998f),
            localScale: new Vector3(1f, 1f, 1f),
            localRotationZ: null
        )
    };

    public static IEnumerator ReplaceLobbySprites(LobbyBehaviour lobby)
    {
        yield return null;
        yield return null;

        if (lobby == null)
            yield break;

        SpriteRenderer[] renderers = lobby.GetComponentsInChildren<SpriteRenderer>(true);

        if (renderers == null || renderers.Length == 0)
            yield break;

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null || renderer.sprite == null)
                continue;

            string rendererPath = GetHierarchyPath(renderer.transform, lobby.transform);

            SpriteReplacement replacement = SpriteReplacements.FirstOrDefault(x => x.RendererPath == rendererPath);

            if (replacement == null)
                continue;

            ApplyTransform(renderer.transform, replacement);

            renderer.enabled = replacement.ShowRenderer;

            SetCollidersEnabled(renderer.gameObject, replacement.EnableCollision);

            if (rendererPath == "SmallBox_Panel" || rendererPath == "panel_Wardrobe")
            {
                ForceColliderEnabled(renderer.gameObject);
            }

            if (!replacement.ShowRenderer)
                continue;

            if (!replacement.UseCustomSprite)
                continue;

            Sprite originalSprite = renderer.sprite;
            string resourcePath = ResourcePrefix + replacement.ResourceFileName;

            Sprite newSprite = LoadSpriteKeepingOriginalGeometry(resourcePath, originalSprite);

            if (newSprite == null)
                continue;

            renderer.sprite = newSprite;
        }

        yield return null;
    }

    private static void ApplyTransform(Transform targetTransform, SpriteReplacement replacement)
    {
        if (targetTransform == null || replacement == null)
            return;

        if (replacement.LocalPosition.HasValue)
            targetTransform.localPosition = replacement.LocalPosition.Value;

        if (replacement.LocalScale.HasValue)
            targetTransform.localScale = replacement.LocalScale.Value;

        if (replacement.LocalRotationZ.HasValue)
        {
            Vector3 euler = targetTransform.localEulerAngles;
            euler.z = replacement.LocalRotationZ.Value;
            targetTransform.localEulerAngles = euler;
        }
    }

    private static void SetCollidersEnabled(GameObject obj, bool enabled)
    {
        if (obj == null)
            return;

        Collider2D[] colliders = obj.GetComponents<Collider2D>();

        foreach (Collider2D collider in colliders)
        {
            if (collider != null)
                collider.enabled = enabled;
        }

        Collider2D[] childColliders = obj.GetComponentsInChildren<Collider2D>(true);

        foreach (Collider2D collider in childColliders)
        {
            if (collider != null)
                collider.enabled = enabled;
        }
    }

    private static void ForceColliderEnabled(GameObject obj)
    {
        if (obj == null)
            return;

        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>(true);

        foreach (Collider2D collider in colliders)
        {
            if (collider == null)
                continue;

            collider.enabled = true;
            collider.isTrigger = true;
        }
    }

    private static Sprite LoadSpriteKeepingOriginalGeometry(string resourcePath, Sprite originalSprite)
    {
        try
        {
            string cacheKey = resourcePath + "|" +
                              originalSprite.pixelsPerUnit + "|" +
                              originalSprite.pivot.x + "|" +
                              originalSprite.pivot.y + "|" +
                              originalSprite.rect.width + "|" +
                              originalSprite.rect.height;

            if (LobbySpriteCache.TryGetValue(cacheKey, out Sprite cachedSprite) && cachedSprite != null)
                return cachedSprite;

            Assembly assembly = Assembly.GetExecutingAssembly();

            using Stream stream = assembly.GetManifestResourceStream(resourcePath);

            if (stream == null)
                return null;

            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;

            if (!ImageConversion.LoadImage(texture, data))
                return null;

            texture.filterMode = originalSprite.texture.filterMode;
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 normalizedPivot = new Vector2(
                originalSprite.pivot.x / originalSprite.rect.width,
                originalSprite.pivot.y / originalSprite.rect.height
            );

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                normalizedPivot,
                originalSprite.pixelsPerUnit,
                0,
                SpriteMeshType.FullRect
            );

            sprite.name = originalSprite.name + "_BANMOD";
            LobbySpriteCache[cacheKey] = sprite;

            return sprite;
        }
        catch
        {
            return null;
        }
    }

    private static string GetHierarchyPath(Transform transform, Transform root)
    {
        List<string> names = new();

        Transform current = transform;

        while (current != null && current != root)
        {
            names.Add(current.name);
            current = current.parent;
        }

        names.Reverse();

        return string.Join("_", names);
    }
}

[HarmonyPatch(typeof(LobbyBehaviour))]
public class LobbyBehaviourPatch
{
    private static int MusicStoppedLobbyInstanceId = int.MinValue;

    [HarmonyPatch(nameof(LobbyBehaviour.Update)), HarmonyPostfix]
    public static void Update_Postfix(LobbyBehaviour __instance)
    {
        if (!BanMod.DisableLobbyMusic.Value || __instance == null)
            return;

        int lobbyInstanceId = __instance.GetInstanceID();
        if (MusicStoppedLobbyInstanceId == lobbyInstanceId)
            return;

        System.Func<ISoundPlayer, bool> lobbybgm = x => x.Name.Equals("MapTheme");
        ISoundPlayer mapThemeSound = SoundManager.Instance.soundPlayers.Find(lobbybgm);

        if (mapThemeSound == null)
            return;

        SoundManager.Instance.StopNamedSound("MapTheme");
        MusicStoppedLobbyInstanceId = lobbyInstanceId;
    }
}


[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Update))]
public static class LobbyBehaviour_Update_Patch
{
    private static float timeEnteredLobby = -1f;
    private static HashSet<byte> rpcSentTo = new();

    public static bool _popupShown = false;

    public static Dictionary<string, float> playerJoinTimes = new();
    public static Dictionary<string, float> playersToMessage = new();
    public static HashSet<string> messagedPlayers = new();

    public static void Postfix()
    {
        if (AmongUsClient.Instance == null)
            return;

        if (timeEnteredLobby < 0f)
            timeEnteredLobby = Time.time;

        if (!_popupShown
            && !AmongUsClient.Instance.AmHost
            && GameStates.isLobby
            && Time.time - timeEnteredLobby >= 3f)
        {
            BanModPopup.CreateDisableModPopup(
                Translator.GetString("Warning"),
                Translator.GetString("disableModConfirm")
            );

            _popupShown = true;
        }
        if (AmongUsClient.Instance.AmHost &&
            GameStates.isLobby &&
            BanModServerSelection.IsModded25 &&
            BanMod.AllPlayerControls != null)
        {
            foreach (var player in BanMod.AllPlayerControls)
            {
                if (player == null || player.Data == null)
                    continue;

                ChatCommands.RestoreModdedNameIfNeeded(player.PlayerId);

                if (player == PlayerControl.LocalPlayer)
                {
                    ChatCommands.StoreOriginalName(player.PlayerId);

                    if (Options.TagName.GetBool())
                    {
                        string newName = ChatCommands.GetDisplayName(player);

                        if (player.Data.PlayerName != newName)
                        {
                            player.RpcSetName(newName);
                        }
                    }
                }
            }
        }
        if (!AmongUsClient.Instance.AmHost)
            return;

        if (GameData.Instance == null
            || GameData.Instance.AllPlayers == null
            || !Options.sendwelcome.GetBool())
        {
            return;
        }

        float currentTime = Time.time;


        if (BanMod.AllPlayerControls != null)
        {
            foreach (var player in BanMod.AllPlayerControls)
            {
                if (player == null || player.Data == null)
                    continue;

                string friendCode = player.Data.FriendCode;

                if (string.IsNullOrEmpty(friendCode))
                    continue;

                if (!playerJoinTimes.ContainsKey(friendCode))
                {
                    playerJoinTimes[friendCode] = currentTime;
                }

                if (!playersToMessage.ContainsKey(friendCode)
                    && !messagedPlayers.Contains(friendCode)
                    && !player.Data.IsDead)
                {
                    playersToMessage[friendCode] = currentTime + 1f;
                }
            }
        }

        var toSend = new List<string>();

        foreach (var kvp in playersToMessage)
        {
            if (currentTime >= kvp.Value)
            {
                toSend.Add(kvp.Key);
            }
        }

        foreach (string friendCode in toSend)
        {
            var player = BanMod.AllPlayerControls?
                .FirstOrDefault(p =>
                    p != null
                    && p.Data != null
                    && p.Data.FriendCode == friendCode
                );

            if (player == null || player.Data == null)
            {
                playersToMessage.Remove(friendCode);
                continue;
            }

            string name = player.Data.PlayerName;
            string suffix = TemplateLoader.GetCurrentTemplateSuffix();

            string templateName =
                "WelcomeTemplate" + suffix;

            string templateNameModded =
                "WelcomeTemplate" + suffix + "Modded";
            bool messageSent = false;

            if (BanModServerSelection.IsVanilla)
            {
                string title =
                    TemplateLoader.FormatWelcomeTemplate(
                        templateName,
                        name
                    );

                Utils.SendMessage(
                    title,
                    player.PlayerId
                );

                MessageBlocker.UpdateLastMessageTime();

                messageSent = true;
            }
            else if (BanModServerSelection.IsModded25)
            {
                string title =
                    TemplateLoader.FormatWelcomeModdedTemplate(
                        templateNameModded,
                        name
                    );

                Utils.SendMessage(
                    title,
                    player.PlayerId
                );

                MessageBlocker.UpdateLastMessageTime();

                messageSent = true;
            }

            if (messageSent)
            {
                playersToMessage.Remove(friendCode);
                messagedPlayers.Add(friendCode);
            }
        }
    }

    public static void ResetState()
    {
        rpcSentTo.Clear();

        timeEnteredLobby = -1f;

        playerJoinTimes.Clear();
        playersToMessage.Clear();
        messagedPlayers.Clear();

        _popupShown = false;
    }

}
[HarmonyPatch(typeof(IntroCutscene), "OnDestroy")]
public static class BanModGameNamePatch
{
    public static void Postfix()
    {
        if (AmongUsClient.Instance == null ||
            !AmongUsClient.Instance.AmHost)
            return;

        if (!BanModServerSelection.IsModded25)
            return;

        var player = PlayerControl.LocalPlayer;

        if (player == null || player.Data == null)
            return;

        string newName =
            ChatCommands.GetDisplayName(player);

        if (player.Data.PlayerName != newName)
        {
            player.RpcSetName(newName);
        }
    }
}