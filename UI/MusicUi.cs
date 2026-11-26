//credits and licenses in the resources folder
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using NLayer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BanMod;

public class CustomMusicPlayer : MonoBehaviour
{
    public CustomMusicPlayer(IntPtr ptr) : base(ptr) { } 

    internal static string _s(string b) => Encoding.UTF8.GetString(Convert.FromBase64String(b));
    public static CustomMusicPlayer Instance;

    private AudioSource _l1;
    private readonly List<string> _l2 = new();
    private int _l3 = 0;

    public bool _l4 = false; 
    private string _l5 = "";
    private bool _l6 = true; 
    private bool _l7 = false; 
    private float _l8 = 0.5f; 
    private bool _l9 = false; 

    private Rect _r1 = new Rect(100, 100, 350, 210);
    private Vector2 _v1 = Vector2.zero;
    private bool _b1 = false;

    private GUIStyle _windowStyle;
    private GUIStyle _titleStyle;

    private readonly ConcurrentQueue<AudioDataChunk> _q1 = new();
    private float _trackDuration = 0f;

    private void Awake()
    {
        Instance = this;
        _l1 = gameObject.AddComponent<AudioSource>();
        _l1.loop = false;
        _l1.volume = _l8;
        _l5 = _s("Tm9UcmFjaw=="); 
        RefreshPlaylist();
    }

    private void OnEnable()
    {
        MenuRouter.OnPanelChanged += HandlePanelChanged;
    }

    private void OnDisable()
    {
        MenuRouter.OnPanelChanged -= HandlePanelChanged;
    }

    private void HandlePanelChanged(MenuRouter.Panel p)
    {
        _l4 = (p == MenuRouter.Panel.MusicPlayer);
        if (_l4) RefreshPlaylist();
    }

    private void Update()
    {
        if (BanMod.IsBanModDisabled) return;
        if (Input.GetKeyDown(KeyBindOptions.K20) && !BanMod.chatOpen)
        {
            if (MenuRouter.Current == MenuRouter.Panel.MusicPlayer)
                MenuRouter.Open(MenuRouter.Panel.None);
            else
                MenuRouter.Open(MenuRouter.Panel.MusicPlayer);
        }

        while (_q1.TryDequeue(out var chunk))
        {
            try
            {
                if (_l1 == null) continue;
                var clip = _l1.clip;
                if (clip == null) continue;

                int channels = Mathf.Max(1, clip.channels);
                int clipFrames = clip.samples; 
                int offsetFrames = chunk.Offset;

                if (offsetFrames < 0) continue;
                if (offsetFrames >= clipFrames) continue;

                int remainingFrames = clipFrames - offsetFrames;
                int remainingFloats = remainingFrames * channels;
                if (remainingFloats <= 0) continue;

                float[] data = chunk.Data;
                if (data == null || data.Length == 0) continue;

                if (data.Length > remainingFloats)
                {
                    float[] trimmed = new float[remainingFloats];
                    Array.Copy(data, 0, trimmed, 0, remainingFloats);
                    data = trimmed;
                }

                clip.SetData(data, offsetFrames);
            }
            catch
            {
            }
        }

        if (_l6 && _l1 != null && _l1.clip != null && !_l1.isPlaying && !_l9)
        {
            if (_l1.timeSamples >= (_l1.clip.samples - 1024) || (_l1.time == 0 && _trackDuration > 0))
            {
                NextTrack();
            }
        }
    }

    public void RefreshPlaylist()
    {
        string p = Path.Combine(Directory.GetCurrentDirectory(), _s("REFUQQ=="), _s("TVVTSUM=")); 
        if (!Directory.Exists(p)) Directory.CreateDirectory(p);

        _l2.Clear();
        foreach (string f in Directory.GetFiles(p, "*.*"))
        {
            string ex = Path.GetExtension(f).ToLowerInvariant();
            if (ex == ".wav" || ex == ".mp3") _l2.Add(f);
        }

        if (_l3 < 0) _l3 = 0;
        if (_l2.Count == 0) _l3 = 0;
        else if (_l3 >= _l2.Count) _l3 = 0;
    }

    public void PlayAtIndex(int i)
    {
        if (_l2.Count == 0 || i < 0 || i >= _l2.Count) return;

        _l9 = false;
        _l3 = i;

        string p = _l2[_l3];
        _l5 = Path.GetFileName(p);

        if (_l1.isPlaying) _l1.Stop();

        if (_l1.clip != null)
        {
            var old = _l1.clip;
            _l1.clip = null;
            try { Object.Destroy(old); } catch { }
        }

        try
        {
            if (p.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                _l1.clip = LoadMp3Streaming(p);
            else
                _l1.clip = LoadWavAsAudioClip(p);

            if (SoundManager.Instance != null)
                _l1.outputAudioMixerGroup = SoundManager.Instance.MusicChannel;

            if (_l1.clip != null)
            {
                _trackDuration = _l1.clip.length;
                _l1.volume = _l8;
                _l1.Play();
            }
        }
        catch
        {
        }
    }

    public void NextTrack()
    {
        if (_l2.Count == 0) return;
        if (_l7 && _l2.Count > 1)
        {
            _l3 = UnityEngine.Random.Range(0, _l2.Count);
            PlayAtIndex(_l3);
        }
        else
        {
            PlayAtIndex((_l3 + 1) % _l2.Count);
        }
    }

    public void PrevTrack()
    {
        if (_l2.Count == 0) return;
        PlayAtIndex((_l3 - 1 + _l2.Count) % _l2.Count);
    }

    private AudioClip LoadWavAsAudioClip(string f)
    {
        byte[] b = File.ReadAllBytes(f);
        int channels = b[22];
        int sampleRate = BitConverter.ToInt32(b, 24);

        int p = 12;
        while (!(b[p] == 100 && b[p + 1] == 97 && b[p + 2] == 116 && b[p + 3] == 97)) 
        {
            p += 4;
            int chunkLen = BitConverter.ToInt32(b, p);
            p += 4 + chunkLen;
        }
        p += 4;
        int dataSize = BitConverter.ToInt32(b, p);
        p += 4;

        int sampleCount = dataSize / 2;
        float[] d = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
            d[i] = BitConverter.ToInt16(b, p + i * 2) / 32768.0f;

        AudioClip cl = AudioClip.Create(Path.GetFileName(f), sampleCount / channels, channels, sampleRate, false);
        cl.SetData(d, 0);
        return cl;
    }

    private AudioClip LoadMp3Streaming(string f)
    {
        var m = new MpegFile(f);

        int totalFrames = (int)(m.SampleRate * m.Duration.TotalSeconds); 
        if (totalFrames < 1) totalFrames = 1;

        AudioClip cl = AudioClip.Create(Path.GetFileName(f), totalFrames, m.Channels, m.SampleRate, false);

        int initialFloats = m.SampleRate * m.Channels * 5;
        if (initialFloats < 1024) initialFloats = 1024;

        float[] init = new float[initialFloats];
        int read0 = m.ReadSamples(init, 0, initialFloats);
        if (read0 > 0)
        {
            int maxFloats = Mathf.Min(read0, totalFrames * m.Channels);
            if (maxFloats != init.Length)
            {
                float[] trimmed = new float[maxFloats];
                Array.Copy(init, 0, trimmed, 0, maxFloats);
                init = trimmed;
            }
            cl.SetData(init, 0);
        }

        _l9 = true;
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                int currentFloatsRead = read0; 
                float[] buffer = new float[65536];

                while (_l9)
                {
                    int remaining = (int)Math.Min(buffer.Length, m.Length - currentFloatsRead);
                    if (remaining <= 0) break;

                    int read = m.ReadSamples(buffer, 0, remaining);
                    if (read <= 0) break;

                    float[] chunk = new float[read];
                    Array.Copy(buffer, 0, chunk, 0, read);

                    int offsetFrames = currentFloatsRead / m.Channels;

                    if (offsetFrames < totalFrames)
                        _q1.Enqueue(new AudioDataChunk { Data = chunk, Offset = offsetFrames });

                    currentFloatsRead += read;
                }
            }
            catch
            {
            }
            finally
            {
                try { m.Dispose(); } catch { }
                _l9 = false;
            }
        });

        return cl;
    }

    private void OnGUI()
    {
        if (!_l4) return;

        EnsureStyles();

        _r1.height = Mathf.MoveTowards(_r1.height, _b1 ? 510f : 270f, 15f);
        _r1 = GUI.Window(999, _r1, (GUI.WindowFunction)_draw, "", _windowStyle);
    }

    private void EnsureStyles()
    {
        if (_windowStyle == null)
        {
            _windowStyle = new GUIStyle(BanModUiStyles.BlackWindow);
        }

        if (_titleStyle == null)
        {
            _titleStyle = new GUIStyle(BanModUiStyles.TitleLabel)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }
    }

    private Texture2D MakeTex(int w, int h, Color c)
    {
        Color[] pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = c;
        Texture2D t = new Texture2D(w, h);
        t.SetPixels(pix);
        t.Apply();
        return t;
    }

    private void _draw(int id)
    {
        GUILayout.BeginVertical(BanModUiStyles.DarkBox);
        GUILayout.Label("♫  Music Player  ♫", _titleStyle, GUILayout.Height(28));
        GUILayout.EndVertical();

        GUILayout.BeginVertical(BanModUiStyles.DarkBox);
        GUILayout.Label("<color=cyan><b>" + _l5 + "</b></color>");

        float currentPos = (_l1.clip != null) ? _l1.time : 0f;
        float totalPos = (_l1.clip != null) ? _l1.clip.length : 1f;

        GUI.enabled = false;
        GUILayout.HorizontalSlider(currentPos, 0f, totalPos);
        GUI.enabled = true;

        GUILayout.BeginHorizontal();
        GUILayout.Label(FormatTime(currentPos), GUI.skin.label);
        GUILayout.FlexibleSpace();
        GUILayout.Label(FormatTime(totalPos), GUI.skin.label);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        float nV = GUILayout.HorizontalSlider(_l8, 0f, 1f);
        if (Math.Abs(nV - _l8) > 0.0001f) { _l8 = nV; if (_l1 != null) _l1.volume = _l8; }
        GUILayout.Label((_l8 * 100).ToString("0") + "%", GUILayout.Width(40));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<<", BanModUiStyles.ButtonDark)) PrevTrack();
        string pL = _l1 != null && _l1.isPlaying ? _s("UGF1c2U=") : _s("UGxheQ==");
        if (GUILayout.Button(pL, BanModUiStyles.AccentButton))
        {
            if (_l1 == null) { }
            else if (_l1.isPlaying) _l1.Pause();
            else _l1.Play();
        }
        if (GUILayout.Button(">>", BanModUiStyles.ButtonDark)) NextTrack();
        GUILayout.EndHorizontal();

        _l7 = GUILayout.Toggle(_l7, " Shuffle", _l7 ? BanModUiStyles.ToggleOnBlueOutline : BanModUiStyles.ToggleOffDark);
        _l6 = GUILayout.Toggle(_l6, " AutoPlay", _l6 ? BanModUiStyles.ToggleOnBlueOutline : BanModUiStyles.ToggleOffDark);

        if (GUILayout.Button(_b1 ? _s("Q2xvc2VQbGF5bGlzdA==") : _s("T3BlblBsYXlsaXN0"), BanModUiStyles.ButtonDark))
            _b1 = !_b1;

        if (_b1)
        {
            _v1 = GUILayout.BeginScrollView(_v1, GUILayout.Height(230));
            for (int i = 0; i < _l2.Count; i++)
            {
                string n = Path.GetFileName(_l2[i]);
                if (i == _l3) n = "<color=yellow><b>▶ " + n + "</b></color>";
                if (GUILayout.Button(n, BanModUiStyles.ButtonDark)) PlayAtIndex(i);
            }
            GUILayout.EndScrollView();
        }

        GUI.DragWindow();
    }

    private string FormatTime(float seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }

    private struct AudioDataChunk
    {
        public float[] Data;
        public int Offset; 
    }

    public string CurrentTrackName => _l5 ?? "";
    public int CurrentIndex => _l3;
    public int TrackCount => _l2?.Count ?? 0;

    public float CurrentTime => (_l1 != null && _l1.clip != null) ? _l1.time : 0f;
    public float TotalTime => (_l1 != null && _l1.clip != null) ? _l1.clip.length : 0f;

    public bool IsPlaying => (_l1 != null && _l1.isPlaying);

    public float Volume
    {
        get => _l8;
        set
        {
            _l8 = Mathf.Clamp01(value);
            if (_l1 != null) _l1.volume = _l8;
        }
    }

    public bool Shuffle
    {
        get => _l7;
        set => _l7 = value;
    }

    public bool AutoPlay
    {
        get => _l6;
        set => _l6 = value;
    }

    public void TogglePlayPause()
    {
        if (_l1 == null) return;
        if (_l1.isPlaying) _l1.Pause();
        else _l1.Play();
    }

    public string GetTrackName(int i)
    {
        if (_l2 == null || i < 0 || i >= _l2.Count) return "";
        return Path.GetFileName(_l2[i]);
    }

}

[HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
public static class VersionShower_Start
{
    private static void Postfix(VersionShower __instance)
    {
        if (BanMod.IsBanModDisabled) return;
        BanMod.credentialsText = $"<b><size=120%><color={BanMod.ModColor}>BanMod</color></b> <color=#a54aff>By <color=#f34c50>Bart</color> <color=#e0ffff><size=75%>(Version:{BanMod.modVersion})</color>";
        var credentials = UnityEngine.Object.Instantiate(__instance.text);
        credentials.text = BanMod.credentialsText;
        credentials.alignment = TextAlignmentOptions.Left;
        credentials.transform.position = new Vector3(-2.80f, -2.77f, -2f);
        credentials.fontSize = credentials.fontSizeMax = credentials.fontSizeMin = 2f;
        __instance.text.gameObject.SetActive(false);
    }
}



[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.Start))]
public static class Patch_C
{
    public static void Postfix(SoundManager __instance)
    {
        if (BanMod.IsBanModDisabled) return;
        if (CustomMusicPlayer.Instance == null)
        {
            ClassInjector.RegisterTypeInIl2Cpp<CustomMusicPlayer>();
            __instance.gameObject.AddComponent<CustomMusicPlayer>();
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
public static class BlockMovementWhenUIActive_Patch
{
    public static void Postfix(ref bool __result)
    {
        if (BanMod.IsBanModDisabled) return;
        if (PremiumChatUI.Instance != null && PremiumChatUI.Instance._v )
        {
            __result = false;
            return;
        }
    }
}