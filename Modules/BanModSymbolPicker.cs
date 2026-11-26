//using BanMod;
//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Reflection;
//using System.Text;
//using System.Text.RegularExpressions;
//using TMPro;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//namespace BanMod
//{
//    internal static class BanModSymbolPicker
//    {
//        private sealed class SymbolCategory
//        {
//            public readonly string Name;
//            public readonly string Symbols;

//            public SymbolCategory(string name, string symbols)
//            {
//                Name = name;
//                Symbols = symbols;
//            }
//        }

//        private sealed class FavoriteHitTarget
//        {
//            public RectTransform Rect;
//            public string Symbol;
//        }

//        private static readonly SymbolCategory[] Categories =
//        {
//            new SymbolCategory("Stars", "★|☆|⁂|⁑|✽"),
//            new SymbolCategory("Arrows", "☝|☞|☟|☜|↑|↓|→|←|↔|↕|⬆|↗|➡|↘|⬇|↙|⬅|↖|⤴|⤵|￩|￪|￫|￬|⇦|⇧|⇨|⇩|⇵|⇄|⇅|⇆|↹|↸|⌅|⌆|⏎|▶|➔"),
//            new SymbolCategory("Shapes", "•|○|◦|⦿|▲|▼|♠|♥|♣|♦|♤|♡|♧|♢|■|□|▢|▣|▤|▥|▦|▧|▨|▩|▪|▫|◌|●|◐|◑|◒|◓|◯|◆|◇|◈|❖|▱|▶|◀|◉|◍|◎|〇|〶|〄"),
//            new SymbolCategory("Symbols", "✓|╳|∞|†|✚|♫|♪|♲|♳|♴|♵|♶|♷|♸|♹|♺|♻|♼|♽|☎|☏|✂|♀|♂|⚠"),
//            new SymbolCategory("Emoji", "😂|☹️|😆|☺️|😎|😉|😅|😊|😋|😀|😁|😃|😄|😍|☁️|☂️|☀️"),
//            new SymbolCategory("Random", "‰|§|¶|©|™|¥|$|¢|€|ƒ|£|Æ"),
//            new SymbolCategory("Copyright", "©|®|™|℡|№|℀|℅|☎|☏|‰|§|¶|☝|☜"),
//            new SymbolCategory("Currency", "¢|$|€|£|¥|₩|₫|￥|¤|ƒ"),
//            new SymbolCategory("Brackets", "〈|〉|《|》|「|」|『|』|〖|〗|〔|〕|︵|︶|︷|︸|︹|︺|︻|︼|︽|︾|︿|﹀|﹁|﹂|﹃|﹄|﹙|﹚|﹛|﹜|﹝|﹞|﹤|﹥|（|）|＜|＞|｛|｝|〘|〙|〚|〛|«|»|‹|›|〈|〉|〱"),
//            new SymbolCategory("Cards", "♤|♠|♧|♣|♡|♥|♢|♦"),
//            new SymbolCategory("Musical", "♩|♪|♫|♬|♭|♮|♯|°|ø|≠"),
//            new SymbolCategory("Degree", "°|℃|℉|☀|☁|☂|☃|☉|♁|♨|㎎|㎏|㎜|㎝|㎞|㎡|㏄|㏎|㏑|㏒|㏕"),
//            new SymbolCategory("Astrological", "☯|✚|†|‡|♁|❖|卍|卐|〷"),
//            new SymbolCategory("Heart", "♥|♡"),
//            new SymbolCategory("Check", "✓|∨|√|〤|〥"),
//            new SymbolCategory("Gender / Faces", "♀|♂|☹|☺|〠|ヅ|ツ|㋡|웃|유|ü|Ü|シ|ッ|㋛|☃|〲|〴"),
//            new SymbolCategory("Punctuation", "·|‑|‒|–|—|―|‘|’|‚|“|”|„|•|‥|…|‧|′|″|‵|ʻ|ˇ|ˉ|ˊ|ˋ|˙|～|¿|﹐|﹒|﹔|﹕|！|＃|＄|％|＆|＊|，|．|：|；|？|＠|、|。|〃|〝|〞|︰"),
//            new SymbolCategory("Math / Numbers", "π|∞|Σ|√|∫|∬|∭|∀|∂|∃|∅|∆|∇|∈|∉|∊|∋|∏|∑|−|∓|∕|∝|∟|∠|∣|∥|∦|∧|∨|∩|∪|∴|∵|∶|∷|∽|≃|≅|≈|≌|≒|≠|≡|≢|≤|≥|≦|≧|≪|≫|≮|≯|≲|≳|≶|≷|⊂|⊃|⊄|⊅|⊆|⊇|⊊|⊋|⊕|⊖|⊗|⊘|⊙|⊠|⊥|⊿|⋚|⋛|⋯|﹢|﹣|＋|－|／|＝|÷|±|Ⅰ|Ⅱ|Ⅲ|Ⅳ|Ⅴ|Ⅵ|Ⅶ|Ⅷ|Ⅸ|Ⅹ|Ⅺ|Ⅻ|ⅰ|ⅱ|ⅲ|ⅳ|ⅴ|ⅵ|ⅶ|ⅷ|ⅸ|ⅹ|ⅺ|ⅻ|➀|➁|➂|➃|➄|➅|➆|➇|➈|➉|➊|➋|➌|➍|➎|➏|➐|➑|➒|➓|⓵|⓶|⓷|⓸|⓹|⓺|⓻|⓼|⓽|⓾|⓿|❶|❷|❸|❹|❺|❻|❼|❽|❾|❿|¹|²|³|⁴|⓪|①|②|③|④|⑤|⑥|⑦|⑧|⑨|⑩|⑪|⑫|⑬|⑭|⑮|⑯|⑰|⑱|⑲|⑳|⑴|⑵|⑶|⑷|⑸|⑹|⑺|⑻|⑼|⑽|⑾|⑿|⒀|⒁|⒂|⒃|⒄|⒅|⒆|⒇|⒈|⒉|⒊|⒋|⒌|⒍|⒎|⒏|⒐|⒑|⒒|⒓|⒔|⒕|⒖|⒗|⒘|⒙|⒚|⒛|㈠|㈡|㈢|㈣|㈤|㈥|㈦|㈧|㈨|㈩|㊀|㊁|㊂|㊃|㊄|㊅|㊆|㊇|㊈|㊉|０|１|２|３|４|５|６|７|８|９|◉|○|◌|◍|◎|●|◐|◑|◒|◓|⊗|⊙|◯|〇|〶|◦|∅|⊕|⊖|⊘|⦿|⚽|⚾|〄|θ|ð|ĩ|Ň|Ⓐ|Ⓑ|Ⓒ|Ⓓ|Ⓔ|Ⓕ|Ⓖ|Ⓗ|Ⓘ|Ⓙ|Ⓚ|Ⓛ|Ⓜ|Ⓝ|Ⓞ|Ⓟ|Ⓠ|Ⓡ|Ⓢ|Ⓣ|Ⓤ|Ⓥ|Ⓦ|Ⓧ|Ⓨ|Ⓩ|ⓐ|ⓑ|ⓒ|ⓓ|ⓔ|ⓕ|ⓖ|ⓗ|ⓘ|ⓙ|ⓚ|ⓛ|ⓜ|ⓝ|ⓞ|ⓟ|ⓠ|ⓡ|ⓢ|ⓣ|ⓤ|ⓥ|ⓦ|ⓧ|ⓨ|ⓩ|⓫|⓬|⓭|⓮|⓯|⓰|⓱|⓲|⓳|⓴|㊏|㊐|㊑|㊒|㊓|㊔|㊕|㊖|㊗|㊘|㊙|㊚|㊛|㊜|㊝|㊞|㊟|㊠|㊡|㊢|㊣|㉈|㉉|㉊|㉋|㉌|㉍|㉎|㉏|㉐|㉑|㉒|㉓|㉔|㉕|㉖|㉗|㉘|㉙|㉚|㉛|㉜|㉝|㉞|㉟|㊱|㊲|㊳|㊴|㊵|㊶|㊷|㊸|㊹|㊺|㊻|㊼|㊽|㊾|㊿"),
//            new SymbolCategory("Latin", "ĩ|Ň|Ⓐ|Ⓑ|Ⓒ|Ⓓ|Ⓔ|Ⓕ|Ⓖ|Ⓗ|Ⓘ|Ⓙ|Ⓚ|Ⓛ|Ⓜ|Ⓝ|Ⓞ|Ⓟ|Ⓠ|Ⓡ|Ⓢ|Ⓣ|Ⓤ|Ⓥ|Ⓦ|Ⓧ|Ⓨ|Ⓩ|ⓐ|ⓑ|ⓒ|ⓓ|ⓔ|ⓕ|ⓖ|ⓗ|ⓘ|ⓙ|ⓚ|ⓛ|ⓜ|ⓝ|ⓞ|ⓟ|ⓠ|ⓡ|ⓢ|ⓣ|ⓤ|ⓥ|ⓦ|ⓧ|ⓨ|ⓩ"),
//            new SymbolCategory("Symbol Emoji", "™|〰|🆗|🆕|🆙|🆒|🆓|🆖|🅿|Ⓜ|🆑|🆘|🆚|⚠|🅰|🅱|🆎|🅾|♻|🆔")
//        };

//        private static readonly HashSet<char> GamePickerSymbolChars = BuildGamePickerSymbolChars();
//        private static int _gameTextBoxInstanceId;
//        private static bool _gameNativeSymbolWhitelistInstalled;
//        private static int _lastSubmitFrame = -1;

//        internal static bool PrepareChatSubmit(ChatController chat)
//        {
//            RefreshGameChatAdapter();
//            if (chat == null || chat != _gameChatController) return true;
//            if (_lastSubmitFrame == Time.frameCount) return false;
//            _lastSubmitFrame = Time.frameCount;
//            CloseWindow();
//            return true;
//        }

//        private static string _editorPlain = "";
//        private static readonly List<string> _editorColors = new List<string>();

//        private static void TrackEditorEdit(string next)
//        {
//            string old = _editorPlain;
//            int prefix = 0;
//            while (prefix < old.Length && prefix < next.Length && old[prefix] == next[prefix]) prefix++;
//            int suffix = 0;
//            while (suffix < old.Length - prefix && suffix < next.Length - prefix &&
//                old[old.Length - 1 - suffix] == next[next.Length - 1 - suffix]) suffix++;
//            while (_editorColors.Count < old.Length) _editorColors.Add(null);
//            _editorColors.RemoveRange(prefix, old.Length - prefix - suffix);
//            _editorColors.InsertRange(prefix, new string[next.Length - prefix - suffix]);
//            _editorPlain = next;
//        }

//        internal static void ClampNativeEditorCaret(TextBoxTMP box)
//        {
//            if (!IsRegisteredGameTextBox(box)) return;
//            int caret = ReadInt(box, 0, "caretPos");
//            int safe = Mathf.Clamp(caret, 0, (box.text ?? "").Length);
//            if (safe != caret) SetMember(box, "caretPos", safe);
//        }

//        internal static void SetGameEditorText(TextBoxTMP box, string input, string composition)
//        {
//            input = input ?? "";
//            composition = composition ?? "";
//            string old = box.text ?? "";
//            int caret = Mathf.Clamp(ReadInt(box, old.Length, "caretPos"), 0, old.Length);

//            int inputCaret = Mathf.Clamp(caret + input.Length - old.Length, 0, input.Length);
//            int nextCaret = 0;
//            bool enter = false;
//            StringBuilder buffer = new StringBuilder(input.Length);
//            for (int i = 0; i < input.Length; i++)
//            {
//                char c = input[i];
//                if (c == '\r' || c == '\n') enter = true;
//                else if (c == '\b')
//                {
//                    if (buffer.Length > 0)
//                    {
//                        int remove = 1;
//                        if (buffer.Length >= 2 && char.IsLowSurrogate(buffer[buffer.Length - 1]) &&
//                            char.IsHighSurrogate(buffer[buffer.Length - 2])) remove = 2;
//                        buffer.Length -= remove;
//                    }
//                }
//                else
//                {
//                    if (box.ForceUppercase) c = char.ToUpperInvariant(c);


//                    if (c == ' ' || GamePickerSymbolChars.Contains(c) || box.IsCharAllowed(c))
//                        buffer.Append(c);
//                }
//                if (i < inputCaret) nextCaret = buffer.Length;
//            }
//            int limit = box.characterLimit;
//            if (limit > 0 && buffer.Length > limit)
//            {
//                int end = limit;
//                if (end > 0 && end < buffer.Length && char.IsHighSurrogate(buffer[end - 1]) &&
//                    char.IsLowSurrogate(buffer[end])) end--;
//                buffer.Length = end;
//            }
//            string next = buffer.ToString();
//            nextCaret = Mathf.Clamp(nextCaret, 0, next.Length);
//            string oldComposition = GetMember(box, "compoText") as string ?? "";
//            bool changed = next != old || composition != oldComposition;
//            box.text = next;
//            SetMember(box, "compoText", composition);
//            SetMember(box, "caretPos", nextCaret);
//            TrackEditorEdit(next);
//            if (box.outputText != null)
//            {
//                box.outputText.isRightToLeftText = false;
//                box.outputText.richText = false;
//                box.outputText.text = (box.Hidden ? new string('*', next.Length) : next) + composition;
//                box.outputText.ForceMeshUpdate(true, true);
//            }
//            TouchScreenKeyboard keyboard = GetMember(box, "keyboard") as TouchScreenKeyboard;
//            if (keyboard != null && changed) keyboard.text = next;

//            if (changed && box.OnChange != null) box.OnChange.Invoke();
//            if (changed && limit > 0 && next.Length == limit && box.SendOnFullChars)
//            {
//                if (box.OnEnter != null) box.OnEnter.Invoke();
//                box.LoseFocus();
//            }
//            if (enter && box.OnEnter != null) box.OnEnter.Invoke();
//            ClampNativeEditorCaret(box);
//            TryInvokeNoArgs(box, "SetPipePosition");
//        }

//        internal static void ObserveGameEditor(TextBoxTMP box)
//        {
//            if (!IsRegisteredGameTextBox(box)) return;


//            TrackEditorEdit(box.text ?? "");
//            if (box.outputText != null)
//                box.outputText.isRightToLeftText = false;
//        }

//        private static string ApplyEditorColors(string plain)
//        {
//            if (!string.Equals(plain, _editorPlain, StringComparison.Ordinal) ||
//                _editorColors.Count != plain.Length) return plain;
//            StringBuilder result = new StringBuilder();
//            int i = 0;
//            while (i < plain.Length)
//            {
//                string hex = _editorColors[i];
//                int end = i + 1;
//                while (end < plain.Length && _editorColors[end] == hex) end++;
//                if (hex != null) result.Append("<color=#").Append(hex).Append(">");
//                result.Append(plain, i, end - i);
//                if (hex != null) result.Append("</color>");
//                i = end;
//            }
//            return result.ToString();
//        }

//        private static string SymbolPreviewHex(string symbol)
//        {

//            if (symbol == "♥" || symbol == "♡" || symbol == "😍") return "FF709D";
//            if (symbol == "☁️" || symbol == "☂️") return "78CFFF";
//            uint hash = 2166136261;
//            foreach (char c in symbol) hash = unchecked((hash ^ c) * 16777619);
//            string[] colors = { "FFD76A", "79DBFF", "A896FF", "FF93BC", "6EE7B7", "FFAD73" };
//            return colors[(int)(hash % (uint)colors.Length)];
//        }

//        private const int FavoritesCategoryIndex = 0;
//        private const string FavoritesCategoryName = "★ Favorites";
//        private static readonly string FavoritesFolder = "./DATA/Symbols";
//        private static readonly string FavoritesFilePath = Path.Combine(FavoritesFolder, "Favorites.txt");
//        private static readonly List<string> FavoriteSymbols = new List<string>();
//        private static readonly HashSet<string> FavoriteSymbolSet = new HashSet<string>();
//        private static readonly List<FavoriteHitTarget> FavoriteHitTargets = new List<FavoriteHitTarget>();
//        private static bool _favoritesLoaded;

//        private static readonly string[] PaletteHex =
//        {
//            "FFFFFF", "D8D8D8", "9B9B9B", "4A4A4A",
//            "FF4D4D", "FF8A3D", "FFD93D", "B8E85C",
//            "56E07B", "42D7D7", "4FC3F7", "5D7CFA",
//            "8A7DFF", "B66DFF", "E66DFF", "FF6FAE",
//            "E74C3C", "E67E22", "F1C40F", "2ECC71",
//            "1ABC9C", "3498DB", "9B59B6", "E84393"
//        };

//        private static readonly Regex AllowedColorSpanRegex = new Regex(
//            @"<color=#(?<hex>[0-9A-Fa-f]{6})(?:[0-9A-Fa-f]{2})?>(?<body>.*?)</color>",
//            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

//        private static readonly Regex AnyRichTextTagRegex = new Regex(
//            @"<.*?>",
//            RegexOptions.Compiled | RegexOptions.Singleline);

//        private static readonly Regex NoParseCloseRegex = new Regex(
//            @"</\s*noparse\s*>",
//            RegexOptions.Compiled | RegexOptions.IgnoreCase);

//        private static GameObject _canvasRoot;
//        private static GameObject _blocker;
//        private static GameObject _window;
//        private static RectTransform _categoryContent;
//        private static RectTransform _symbolContent;
//        private static RectTransform _symbolScrollRoot;
//        private static TextMeshProUGUI _contextText;
//        private static TextMeshProUGUI _colorText;
//        private static TextMeshProUGUI _copyInfoText;
//        private static TextMeshProUGUI _favoritesInfoText;
//        private static readonly List<GameObject> _colorControls = new List<GameObject>();
//        private static GameObject _gameLauncher;
//        private static Button _gameLauncherButton;
//        private static SpriteRenderer _gameLauncherNativeSprite;

//        private const float OmegaDefaultOffsetX = -225f;
//        private const float OmegaDefaultOffsetY = 37f;
//        private const float OmegaSizePixels = 124f;
//        private const int OmegaSortingOffset = 0;
//        private const float OmegaZOffset = 0f;
//        private const string OmegaOffsetXPrefKey = "BanMod.SymbolPicker.OmegaOffsetX";
//        private const string OmegaOffsetYPrefKey = "BanMod.SymbolPicker.OmegaOffsetY";
//        private static float _omegaOffsetX = OmegaDefaultOffsetX;
//        private static float _omegaOffsetY = OmegaDefaultOffsetY;
//        private static bool _omegaPositionLoaded;
//        private static bool _omegaDragging;
//        private static Vector2 _omegaDragMouseStart;
//        private static float _omegaDragStartOffsetX;
//        private static float _omegaDragStartOffsetY;

//        private static Action<string> _insertAction;
//        private static TMP_InputField _focusInputAfterCopy;
//        private static bool _focusGameChatAfterCopy;
//        private static Func<bool> _colorAllowed;
//        private static string _contextName = "SYMBOLS";
//        private static string _selectedHex = "FFFFFF";
//        private static int _selectedCategoryIndex;
//        private static TMP_InputField _symbolSearchInput;
//        private static string _symbolSearchText = "";

//        private static float _nextGameScanTime;
//        private const float GameChatScanInterval = 0.35f;
//        private static ChatController _gameChatController;
//        private static GameChatTextAdapter _gameAdapter;

//        private static string _pendingOutgoingColoredGameChat;
//        private static uint _pendingOutgoingColoredGameChatNetId;

//        private static Func<bool> _gameModdedResolver = delegate { return false; };
//        private static bool _hasExplicitGameModdedResolver;

//        internal static bool IsOpen
//        {
//            get
//            {
//                try
//                {
//                    return _blocker != null &&
//                           _window != null &&
//                           _blocker.activeInHierarchy &&
//                           _window.activeInHierarchy;
//                }
//                catch { return false; }
//            }
//        }

//        internal static void SetGameModdedResolver(Func<bool> resolver)
//        {
//            _gameModdedResolver = resolver ?? delegate { return false; };
//            _hasExplicitGameModdedResolver = resolver != null;
//        }

//        internal static Button AttachToTmpInput(
//            Transform parent,
//            string name,
//            TMP_InputField input,
//            Vector2 position,
//            Vector2 size,
//            bool alwaysColor)
//        {
//            if (parent == null || input == null)
//                return null;

//            EnableRichText(input);

//            Button button = CreateUiButton(parent, name, "Ω", position, size, new Color(0.25f, 0.29f, 0.37f, 0.98f));
//            if (button != null)
//            {
//                button.onClick.AddListener((UnityAction)delegate
//                {
//                    OpenForTmpInput(input, alwaysColor, alwaysColor ? "COMMUNITY / PRIVATE" : "TEXT");
//                });
//            }
//            return button;
//        }

//        internal static void OpenForTmpInput(TMP_InputField input, bool alwaysColor, string contextName)
//        {
//            if (input == null)
//                return;

//            EnsurePickerUi();
//            _focusInputAfterCopy = input;
//            _focusGameChatAfterCopy = false;

//            _contextName = string.IsNullOrWhiteSpace(contextName) ? "SYMBOLS" : contextName;
//            _colorAllowed = alwaysColor ? (Func<bool>)(delegate { return true; }) : (delegate { return false; });
//            _insertAction = delegate (string symbol)
//            {
//                GUIUtility.systemCopyBuffer = FormatSymbolForCurrentContext(symbol);
//            };

//            RefreshHeader();
//            OpenWindow();

//        }

//        internal static void RuntimeTick()
//        {
//            try
//            {
//                if (IsOpen)
//                {
//                    HandleFavoriteRightClickHitTest();
//                    if (Input.GetKeyDown(KeyCode.Escape)) CloseWindow();
//                }

//                if (PlayerControl.LocalPlayer == null)
//                {
//                    if (_gameLauncher != null)
//                        _gameLauncher.SetActive(false);
//                    _gameChatController = null;
//                    _gameAdapter = null;
//                    return;
//                }

//                if (Time.unscaledTime >= _nextGameScanTime)
//                {
//                    _nextGameScanTime = Time.unscaledTime + GameChatScanInterval;
//                    RefreshGameChatAdapter();
//                }

//                bool showLauncher = _gameAdapter != null && _gameAdapter.IsVisible;
//                if (showLauncher)
//                {
//                    EnsureOmegaPositionLoaded();
//                    EnsureGameLauncherUi();

//                    if (_gameLauncher != null)
//                        _gameLauncher.SetActive(!IsOpen);

//                    HandleOmegaDrag();


//                    _gameAdapter.UpdateNativeLauncherSprite(!IsOpen);

//                    try
//                    {
//                        RectTransform launcherRect = _gameLauncher != null ? _gameLauncher.GetComponent<RectTransform>() : null;
//                        RectTransform canvasRect = _canvasRoot != null ? _canvasRoot.GetComponent<RectTransform>() : null;

//                        _gameAdapter.UpdateLauncherPosition(launcherRect, canvasRect);
//                    }
//                    catch
//                    {
//                    }
//                }
//                else
//                {
//                    if (_gameLauncherNativeSprite != null) _gameLauncherNativeSprite.enabled = false;
//                }

//                if (_gameLauncher != null && (!showLauncher || IsOpen))
//                    _gameLauncher.SetActive(false);
//                if (!showLauncher || IsOpen)
//                    _omegaDragging = false;
//            }
//            catch
//            {
//            }
//        }

//        private static void EnsureOmegaPositionLoaded()
//        {
//            if (_omegaPositionLoaded)
//                return;

//            _omegaOffsetX = PlayerPrefs.HasKey(OmegaOffsetXPrefKey)
//                ? PlayerPrefs.GetFloat(OmegaOffsetXPrefKey, OmegaDefaultOffsetX)
//                : OmegaDefaultOffsetX;
//            _omegaOffsetY = PlayerPrefs.HasKey(OmegaOffsetYPrefKey)
//                ? PlayerPrefs.GetFloat(OmegaOffsetYPrefKey, OmegaDefaultOffsetY)
//                : OmegaDefaultOffsetY;
//            _omegaPositionLoaded = true;
//        }

//        private static void SaveOmegaPosition()
//        {
//            try
//            {
//                PlayerPrefs.SetFloat(OmegaOffsetXPrefKey, _omegaOffsetX);
//                PlayerPrefs.SetFloat(OmegaOffsetYPrefKey, _omegaOffsetY);
//                PlayerPrefs.Save();
//            }
//            catch { }
//        }

//        private static void HandleOmegaDrag()
//        {
//            if (_gameLauncher == null || !_gameLauncher.activeInHierarchy)
//            {
//                _omegaDragging = false;
//                return;
//            }

//            EnsureOmegaPositionLoaded();

//            RectTransform launcherRect = _gameLauncher.GetComponent<RectTransform>();
//            if (launcherRect == null)
//                return;

//            bool altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
//            Vector2 mouse = Input.mousePosition;
//            bool overLauncher = RectTransformUtility.RectangleContainsScreenPoint(launcherRect, mouse, null);

//            if (!_omegaDragging && altHeld && overLauncher && Input.GetMouseButtonDown(0))
//            {
//                _omegaDragging = true;
//                _omegaDragMouseStart = mouse;
//                _omegaDragStartOffsetX = _omegaOffsetX;
//                _omegaDragStartOffsetY = _omegaOffsetY;
//                return;
//            }

//            if (_omegaDragging)
//            {
//                if (!Input.GetMouseButton(0) || !altHeld)
//                {
//                    _omegaDragging = false;
//                    SaveOmegaPosition();
//                    return;
//                }

//                Vector2 delta = mouse - _omegaDragMouseStart;
//                _omegaOffsetX = _omegaDragStartOffsetX + delta.x;
//                _omegaOffsetY = _omegaDragStartOffsetY + delta.y;
//            }
//        }

//        internal static string ToSafeRichText(string raw)
//        {
//            raw = raw ?? "";
//            if (raw.Length == 0)
//                return "";

//            StringBuilder sb = new StringBuilder(raw.Length + 32);
//            int index = 0;
//            MatchCollection matches = AllowedColorSpanRegex.Matches(raw);
//            for (int i = 0; i < matches.Count; i++)
//            {
//                Match m = matches[i];
//                if (m.Index > index)
//                    AppendNoParse(sb, raw.Substring(index, m.Index - index));

//                string hex = m.Groups["hex"].Value.ToUpperInvariant();
//                string body = m.Groups["body"].Value;
//                sb.Append("<color=#").Append(hex).Append(">");
//                AppendNoParse(sb, body);
//                sb.Append("</color>");
//                index = m.Index + m.Length;
//            }

//            if (index < raw.Length)
//                AppendNoParse(sb, raw.Substring(index));

//            return sb.ToString();
//        }

//        internal static string StripPickerColorTags(string raw)
//        {
//            raw = raw ?? "";
//            if (raw.Length == 0)
//                return "";
//            return AllowedColorSpanRegex.Replace(raw, delegate (Match m) { return m.Groups["body"].Value; });
//        }

//        internal static string TranslatePreservingPickerColors(string raw, Func<string, string> translate)
//        {
//            raw = raw ?? "";
//            if (raw.Length == 0 || translate == null)
//                return raw;

//            MatchCollection matches = AllowedColorSpanRegex.Matches(raw);
//            if (matches.Count == 0)
//            {
//                string translated = translate(raw);
//                return string.IsNullOrWhiteSpace(translated) ? raw : translated;
//            }

//            StringBuilder sb = new StringBuilder(raw.Length + 32);
//            int index = 0;
//            for (int i = 0; i < matches.Count; i++)
//            {
//                Match m = matches[i];
//                if (m.Index > index)
//                {
//                    string plain = raw.Substring(index, m.Index - index);
//                    string translated = translate(plain);
//                    sb.Append(string.IsNullOrWhiteSpace(translated) ? plain : translated);
//                }

//                sb.Append(m.Value);
//                index = m.Index + m.Length;
//            }

//            if (index < raw.Length)
//            {
//                string plain = raw.Substring(index);
//                string translated = translate(plain);
//                sb.Append(string.IsNullOrWhiteSpace(translated) ? plain : translated);
//            }

//            return sb.ToString();
//        }

//        private static void AppendNoParse(StringBuilder sb, string literal)
//        {
//            if (string.IsNullOrEmpty(literal))
//                return;

//            literal = NoParseCloseRegex.Replace(literal, "‹/noparse›");
//            sb.Append("<noparse>").Append(literal).Append("</noparse>");
//        }

//        private static void RefreshGameChatAdapter()
//        {
//            try
//            {
//                if (_gameChatController == null)
//                {
//                    try
//                    {
//                        if (HudManager.Instance != null)
//                            _gameChatController = HudManager.Instance.GetComponentInChildren<ChatController>(true);
//                    }
//                    catch { }

//                    if (_gameChatController == null)
//                        _gameChatController = UnityEngine.Object.FindObjectOfType<ChatController>();

//                    _gameAdapter = null;
//                }

//                if (_gameChatController == null)
//                {
//                    _gameAdapter = null;
//                    return;
//                }

//                if (_gameAdapter == null || !_gameAdapter.IsAlive)
//                    _gameAdapter = GameChatTextAdapter.TryCreate(_gameChatController);
//            }
//            catch
//            {
//                _gameChatController = null;
//                _gameAdapter = null;
//            }
//        }

//        private static void OpenForGameChat()
//        {
//            RefreshGameChatAdapter();
//            if (_gameAdapter == null)
//                return;


//            try
//            {
//                if (_gameChatController != null &&
//                    _gameChatController.freeChatField != null &&
//                    _gameChatController.freeChatField.textArea != null)
//                    RegisterGameTextBox(_gameChatController.freeChatField.textArea);
//            }
//            catch
//            {
//            }

//            EnsurePickerUi();
//            _focusInputAfterCopy = null;
//            _focusGameChatAfterCopy = true;
//            _contextName = SafeGameColorAllowed() ? "CHAT · MODDED COLORS" : "CHAT · SYMBOLS";
//            _colorAllowed = SafeGameColorAllowed;
//            _insertAction = delegate (string symbol)
//            {
//                GUIUtility.systemCopyBuffer = FormatSymbolForCurrentContext(symbol);
//            };
//            RefreshHeader();
//            OpenWindow();

//        }

//        private static bool SafeGameColorAllowed()
//        {
//            try
//            {
//                return _hasExplicitGameModdedResolver
//                    ? _gameModdedResolver()
//                    : BanModServerSelection.Mode == BanModServerMode.Modded25;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        private static bool ContainsGamePickerSymbol(string raw)
//        {
//            if (string.IsNullOrEmpty(raw))
//                return false;

//            try
//            {
//                if (AllowedColorSpanRegex.IsMatch(raw))
//                    return true;

//                for (int i = 0; i < raw.Length; i++)
//                {
//                    if (GamePickerSymbolChars.Contains(raw[i]))
//                        return true;
//                }
//            }
//            catch { }

//            return false;
//        }

//        private static string BuildDirectGameChatPayload(string raw)
//        {
//            raw = raw ?? "";

//            if (SafeGameColorAllowed())
//            {
//                return KeepOnlyPickerColorTags(ApplyEditorColors(raw));
//            }

//            string plain = StripPickerColorTags(raw);
//            return AnyRichTextTagRegex.Replace(plain, "");
//        }

//        internal static bool TrySendPickerGameChatDirect(PlayerControl player, string rawText)
//        {
//            if (player == null || string.IsNullOrEmpty(rawText))
//                return false;

//            if (!SafeGameColorAllowed() || !AllowedColorSpanRegex.IsMatch(ApplyEditorColors(rawText)))
//                return false;

//            try
//            {
//                string payload = BuildDirectGameChatPayload(rawText);
//                if (string.IsNullOrEmpty(payload))
//                {
//                    return false;
//                }


//                RefreshGameChatAdapter();

//                ChatController chat = _gameChatController;
//                if (chat == null)
//                {
//                    try
//                    {
//                        if (HudManager.Instance != null)
//                            chat = HudManager.Instance.GetComponentInChildren<ChatController>(true);
//                    }
//                    catch { }
//                }

//                var message =
//                    new AmongUs.InnerNet.GameDataMessages.RpcSendChatMessage(
//                        player.NetId,
//                        payload);

//                if (AmongUsClient.Instance == null)
//                {
//                    return false;
//                }

//                var gameDataMessage =
//                    message.Cast<AmongUs.InnerNet.GameDataMessages.IGameDataMessage>();

//                AmongUsClient.Instance.LateBroadcastReliableMessage(gameDataMessage);


//                if (chat != null)
//                {
//                    try
//                    {
//                        chat.AddChat(player, payload);
//                    }
//                    catch (Exception)
//                    {
//                    }
//                }
//                else
//                {
//                }

//                return true;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        internal static void BeginOutgoingGameChat(PlayerControl player, string rawText)
//        {
//            _pendingOutgoingColoredGameChat = null;
//            _pendingOutgoingColoredGameChatNetId = 0;

//            bool colorAllowed = false;
//            try { colorAllowed = SafeGameColorAllowed(); } catch { }


//            try
//            {
//                if (!colorAllowed)
//                {
//                    return;
//                }

//                if (player == null)
//                {
//                    return;
//                }

//                if (string.IsNullOrEmpty(rawText))
//                {
//                    return;
//                }

//                bool hasColorSpan = AllowedColorSpanRegex.IsMatch(rawText);

//                if (!hasColorSpan)
//                {
//                    return;
//                }

//                string safe = KeepOnlyPickerColorTags(rawText);

//                if (string.IsNullOrEmpty(safe))
//                {
//                    return;
//                }

//                _pendingOutgoingColoredGameChat = safe;
//                _pendingOutgoingColoredGameChatNetId = player.NetId;

//            }
//            catch (Exception)
//            {
//                _pendingOutgoingColoredGameChat = null;
//                _pendingOutgoingColoredGameChatNetId = 0;
//            }
//        }

//        internal static void EndOutgoingGameChat()
//        {

//            _pendingOutgoingColoredGameChat = null;
//            _pendingOutgoingColoredGameChatNetId = 0;
//        }

//        internal static void RestorePendingColorForLocalBubble(PlayerControl sourcePlayer, ref string text)
//        {

//            try
//            {
//                if (string.IsNullOrEmpty(_pendingOutgoingColoredGameChat))
//                {
//                    return;
//                }

//                if (!SafeGameColorAllowed())
//                {
//                    return;
//                }

//                if (sourcePlayer == null || PlayerControl.LocalPlayer == null || sourcePlayer != PlayerControl.LocalPlayer)
//                {
//                    return;
//                }

//                text = _pendingOutgoingColoredGameChat;
//            }
//            catch (Exception)
//            {
//            }
//        }

//        internal static void RestorePendingColorForRpcMessage(uint netId, ref string text)
//        {

//            try
//            {
//                if (string.IsNullOrEmpty(_pendingOutgoingColoredGameChat))
//                {
//                    return;
//                }

//                if (!SafeGameColorAllowed())
//                {
//                    return;
//                }

//                if (_pendingOutgoingColoredGameChatNetId != 0 &&
//                    netId != _pendingOutgoingColoredGameChatNetId)
//                {
//                    return;
//                }

//                text = _pendingOutgoingColoredGameChat;
//            }
//            catch (Exception)
//            {
//            }
//        }

//        private static string KeepOnlyPickerColorTags(string raw)
//        {
//            raw = raw ?? "";
//            if (raw.Length == 0)
//                return "";

//            MatchCollection matches = AllowedColorSpanRegex.Matches(raw);
//            if (matches.Count == 0)
//                return AnyRichTextTagRegex.Replace(raw, "");

//            StringBuilder sb = new StringBuilder(raw.Length);
//            int index = 0;
//            for (int i = 0; i < matches.Count; i++)
//            {
//                Match m = matches[i];
//                if (m.Index > index)
//                {
//                    string plain = raw.Substring(index, m.Index - index);
//                    sb.Append(AnyRichTextTagRegex.Replace(plain, ""));
//                }

//                string hex = m.Groups["hex"].Value.ToUpperInvariant();
//                string body = AnyRichTextTagRegex.Replace(m.Groups["body"].Value ?? "", "");
//                sb.Append("<color=#").Append(hex).Append(">").Append(body).Append("</color>");
//                index = m.Index + m.Length;
//            }

//            if (index < raw.Length)
//                sb.Append(AnyRichTextTagRegex.Replace(raw.Substring(index), ""));

//            return sb.ToString();
//        }

//        private static Func<bool> DiscoverBanModModdedResolver()
//        {
//            try
//            {
//                Assembly assembly = typeof(BanModSymbolPicker).Assembly;
//                Type[] types = assembly.GetTypes();
//                string[] exactBoolNames =
//                {
//                    "IsFullyModdedLobby", "AllPlayersModded", "AllClientsModded",
//                    "IsAllModded", "CanUseColoredGameChat"
//                };

//                for (int t = 0; t < types.Length; t++)
//                {
//                    Type type = types[t];
//                    if (type == null || type.Namespace == null || !type.Namespace.StartsWith("BanMod", StringComparison.Ordinal))
//                        continue;

//                    for (int i = 0; i < exactBoolNames.Length; i++)
//                    {
//                        string name = exactBoolNames[i];
//                        PropertyInfo prop = type.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
//                        if (prop != null && prop.PropertyType == typeof(bool) && prop.CanRead)
//                        {
//                            return delegate
//                            {
//                                try { return (bool)prop.GetValue(null, null); } catch { return false; }
//                            };
//                        }

//                        FieldInfo field = type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
//                        if (field != null && field.FieldType == typeof(bool))
//                        {
//                            return delegate
//                            {
//                                try { return (bool)field.GetValue(null); } catch { return false; }
//                            };
//                        }
//                    }
//                }

//                for (int t = 0; t < types.Length; t++)
//                {
//                    Type type = types[t];
//                    if (type == null || type.Namespace == null || !type.Namespace.StartsWith("BanMod", StringComparison.Ordinal))
//                        continue;

//                    FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
//                    for (int i = 0; i < fields.Length; i++)
//                    {
//                        FieldInfo field = fields[i];
//                        string n = field.Name ?? "";
//                        bool looksLikeVersionMap =
//                            n.IndexOf("PlayerVersion", StringComparison.OrdinalIgnoreCase) >= 0 ||
//                            n.IndexOf("ClientVersion", StringComparison.OrdinalIgnoreCase) >= 0 ||
//                            n.IndexOf("ModdedPlayers", StringComparison.OrdinalIgnoreCase) >= 0;
//                        if (!looksLikeVersionMap)
//                            continue;

//                        return delegate
//                        {
//                            try
//                            {
//                                object value = field.GetValue(null);
//                                System.Collections.ICollection collection = value as System.Collections.ICollection;
//                                int playerCount = GameData.Instance == null ? 0 : GameData.Instance.PlayerCount;
//                                return collection != null && playerCount > 0 && collection.Count >= playerCount;
//                            }
//                            catch { return false; }
//                        };
//                    }
//                }
//            }
//            catch { }

//            return delegate { return false; };
//        }

//        private static HashSet<char> BuildGamePickerSymbolChars()
//        {
//            HashSet<char> chars = new HashSet<char>();
//            try
//            {
//                for (int i = 0; i < Categories.Length; i++)
//                {
//                    string symbols = Categories[i] == null ? "" : (Categories[i].Symbols ?? "");
//                    for (int j = 0; j < symbols.Length; j++)
//                    {
//                        char c = symbols[j];
//                        if (c != '|')
//                            chars.Add(c);
//                    }
//                }
//            }
//            catch { }
//            return chars;
//        }

//        private static void EnsureGameNativeSymbolWhitelist()
//        {
//            if (_gameNativeSymbolWhitelistInstalled)
//                return;

//            try
//            {
//                var native = TextBoxTMP.SymbolChars;
//                if (native == null)
//                    return;

//                foreach (char c in GamePickerSymbolChars)
//                {
//                    if (!native.Contains(c))
//                        native.Add(c);
//                }

//                const string RichTextSyntaxChars = "<>/=#";
//                for (int i = 0; i < RichTextSyntaxChars.Length; i++)
//                {
//                    char c = RichTextSyntaxChars[i];
//                    if (!native.Contains(c))
//                        native.Add(c);
//                }

//                _gameNativeSymbolWhitelistInstalled = true;
//            }
//            catch
//            {
//                _gameNativeSymbolWhitelistInstalled = false;
//            }
//        }

//        internal static void RegisterGameTextBox(TextBoxTMP textBox)
//        {
//            try
//            {
//                EnsureGameNativeSymbolWhitelist();
//                int nextId = textBox == null ? 0 : textBox.GetInstanceID();
//                if (nextId != _gameTextBoxInstanceId)
//                {
//                    _editorPlain = "";
//                    _editorColors.Clear();
//                }
//                _gameTextBoxInstanceId = nextId;
//                if (textBox != null)
//                {
//                    textBox.AllowSymbols = true;
//                    if (textBox.outputText != null) textBox.outputText.isRightToLeftText = false;
//                }
//            }
//            catch { _gameTextBoxInstanceId = 0; }
//        }

//        internal static void PasteUnicodeFromClipboard(TextBoxTMP box, string clipboard)
//        {
//            if (box == null || string.IsNullOrEmpty(clipboard)) return;
//            RegisterGameTextBox(box);
//            string original = box.text ?? "";


//            string plain = StripPickerColorTags(clipboard);
//            plain = AnyRichTextTagRegex.Replace(plain, "");
//            if (plain.Length == 0) return;
//            int limit = box.characterLimit;
//            if (limit > 0 && original.Length + plain.Length > limit) return;
//            var colors = new List<string>();
//            int cursor = 0;
//            foreach (Match m in AllowedColorSpanRegex.Matches(clipboard))
//            {
//                int prefixLen = Mathf.Min(m.Index - cursor, plain.Length - colors.Count);
//                for (int i = 0; i < prefixLen; i++) colors.Add(null);
//                string hex = m.Groups["hex"].Value.ToUpperInvariant();
//                string body = AnyRichTextTagRegex.Replace(m.Groups["body"].Value, "");
//                for (int i = 0; i < body.Length; i++) colors.Add(SafeGameColorAllowed() ? hex : null);
//                cursor = m.Index + m.Length;
//            }
//            while (colors.Count < plain.Length) colors.Add(null);

//            SetGameEditorText(box, original + plain, "");
//            if (string.Equals(box.text, original + plain, StringComparison.Ordinal))
//            {
//                while (_editorColors.Count < original.Length + plain.Length) _editorColors.Add(null);
//                for (int i = 0; i < plain.Length; i++)
//                    _editorColors[original.Length + i] = colors[i];
//                _editorPlain = box.text;
//                if (SafeGameColorAllowed() && box.outputText != null)
//                {
//                    box.outputText.richText = true;
//                    box.outputText.text = ApplyEditorColors(_editorPlain);
//                    box.outputText.ForceMeshUpdate(true, true);
//                }
//            }
//        }

//        internal static bool IsRegisteredGameTextBox(TextBoxTMP textBox)
//        {
//            if (textBox == null || _gameTextBoxInstanceId == 0)
//                return false;
//            try { return textBox.GetInstanceID() == _gameTextBoxInstanceId; }
//            catch { return false; }
//        }

//        internal static bool ShouldForceAllowGameCharacter(TextBoxTMP textBox, object[] args)
//        {
//            if (!IsRegisteredGameTextBox(textBox) || args == null)
//                return false;

//            try
//            {
//                for (int i = 0; i < args.Length; i++)
//                {
//                    object arg = args[i];
//                    if (arg is char c)
//                        return GamePickerSymbolChars.Contains(c);

//                    if (arg is string str && str.Length == 1)
//                        return GamePickerSymbolChars.Contains(str[0]);

//                    if (arg is int code && code >= char.MinValue && code <= char.MaxValue)
//                        return GamePickerSymbolChars.Contains((char)code);
//                }
//            }
//            catch { }

//            return false;
//        }

//        private static string FormatSymbolForCurrentContext(string symbol)
//        {
//            if (string.IsNullOrEmpty(symbol))
//                return "";

//            bool useColor = false;
//            try { useColor = _colorAllowed != null && _colorAllowed(); } catch { }
//            if (!useColor)
//                return symbol;

//            string hex = string.IsNullOrWhiteSpace(_selectedHex) ? "FFFFFF" : _selectedHex.Trim().ToUpperInvariant();
//            return "<color=#" + hex + ">" + symbol + "</color>";
//        }

//        private static void InsertIntoTmpInput(TMP_InputField input, string token)
//        {
//            if (input == null || string.IsNullOrEmpty(token))
//                return;

//            try
//            {
//                string current = input.text ?? "";

//                int fallbackCaret = 0;
//                try { fallbackCaret = input.caretPosition; } catch { }

//                int anchorVisual = ReadTmpInputPosition(
//                    input,
//                    fallbackCaret,
//                    "selectionAnchorPosition");

//                int focusVisual = ReadTmpInputPosition(
//                    input,
//                    anchorVisual,
//                    "selectionFocusPosition");

//                int visibleLength = GetPickerVisibleLength(current);
//                anchorVisual = Mathf.Clamp(anchorVisual, 0, visibleLength);
//                focusVisual = Mathf.Clamp(focusVisual, 0, visibleLength);

//                int startVisual = Math.Min(anchorVisual, focusVisual);
//                int endVisual = Math.Max(anchorVisual, focusVisual);
//                int startRaw = PickerVisibleIndexToRawIndex(current, startVisual);
//                int endRaw = PickerVisibleIndexToRawIndex(current, endVisual);

//                startRaw = Mathf.Clamp(startRaw, 0, current.Length);
//                endRaw = Mathf.Clamp(endRaw, startRaw, current.Length);

//                string next = current.Remove(startRaw, endRaw - startRaw).Insert(startRaw, token);

//                int nextVisibleLength = GetPickerVisibleLength(next);
//                if (input.characterLimit > 0 && nextVisibleLength > input.characterLimit)
//                    return;

//                input.text = next;

//                int insertedVisibleLength = GetPickerVisibleLength(token);
//                int nextVisualCaret = startVisual + insertedVisibleLength;
//                int nextRawCaret = startRaw + token.Length;

//                WriteTmpInputPosition(input, nextRawCaret, "stringPosition");
//                WriteTmpInputPosition(input, nextRawCaret, "stringSelectPosition");
//                WriteTmpInputPosition(input, nextVisualCaret, "selectionAnchorPosition");
//                WriteTmpInputPosition(input, nextVisualCaret, "selectionFocusPosition");
//                try { input.caretPosition = nextVisualCaret; } catch { }

//                try { input.ActivateInputField(); } catch { }
//                try { input.ForceLabelUpdate(); } catch { }
//            }
//            catch
//            {
//            }
//        }

//        private static int GetPickerVisibleLength(string raw)
//        {
//            raw = raw ?? "";
//            if (raw.Length == 0)
//                return 0;

//            try { return StripPickerColorTags(raw).Length; }
//            catch { return raw.Length; }
//        }
//        private static int PickerVisibleIndexToRawIndex(string raw, int visibleIndex)
//        {
//            raw = raw ?? "";
//            if (raw.Length == 0 || visibleIndex <= 0)
//                return 0;

//            int maxVisible = GetPickerVisibleLength(raw);
//            visibleIndex = Mathf.Clamp(visibleIndex, 0, maxVisible);

//            int rawIndex = 0;
//            int visible = 0;
//            MatchCollection matches = AllowedColorSpanRegex.Matches(raw);

//            for (int i = 0; i < matches.Count; i++)
//            {
//                Match m = matches[i];
//                if (m.Index < rawIndex)
//                    continue;

//                int plainLength = m.Index - rawIndex;
//                if (visibleIndex <= visible + plainLength)
//                    return rawIndex + (visibleIndex - visible);

//                visible += plainLength;

//                string body = m.Groups["body"].Value ?? "";
//                int bodyLength = body.Length;
//                if (visibleIndex < visible + bodyLength)
//                {
//                    int bodyOffset = visibleIndex - visible;
//                    return m.Groups["body"].Index + bodyOffset;
//                }

//                visible += bodyLength;
//                rawIndex = m.Index + m.Length;

//                if (visibleIndex == visible)
//                    return rawIndex;
//            }

//            int remaining = raw.Length - rawIndex;
//            if (visibleIndex <= visible + remaining)
//                return rawIndex + (visibleIndex - visible);

//            return raw.Length;
//        }

//        private static int ReadTmpInputPosition(TMP_InputField input, int fallback, params string[] propertyNames)
//        {
//            if (input == null || propertyNames == null)
//                return fallback;

//            Type type = input.GetType();
//            for (int i = 0; i < propertyNames.Length; i++)
//            {
//                string name = propertyNames[i];
//                if (string.IsNullOrWhiteSpace(name))
//                    continue;

//                try
//                {
//                    PropertyInfo property = type.GetProperty(
//                        name,
//                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//                    if (property == null || !property.CanRead)
//                        continue;

//                    object value = property.GetValue(input, null);
//                    if (value is int)
//                        return (int)value;
//                }
//                catch { }
//            }

//            return fallback;
//        }

//        private static void WriteTmpInputPosition(TMP_InputField input, int value, params string[] propertyNames)
//        {
//            if (input == null || propertyNames == null)
//                return;

//            Type type = input.GetType();
//            for (int i = 0; i < propertyNames.Length; i++)
//            {
//                string name = propertyNames[i];
//                if (string.IsNullOrWhiteSpace(name))
//                    continue;

//                try
//                {
//                    PropertyInfo property = type.GetProperty(
//                        name,
//                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//                    if (property == null || !property.CanWrite || property.PropertyType != typeof(int))
//                        continue;

//                    property.SetValue(input, value, null);
//                    return;
//                }
//                catch { }
//            }
//        }

//        private static void EnableRichText(TMP_InputField input)
//        {
//            if (input == null)
//                return;
//            try { input.richText = true; } catch { }
//            try { if (input.textComponent != null) input.textComponent.richText = true; } catch { }
//        }

//        private static void EnsureCanvasRoot()
//        {
//            if (_canvasRoot != null)
//                return;

//            _canvasRoot = new GameObject("BanMod_SymbolPickerCanvas");
//            UnityEngine.Object.DontDestroyOnLoad(_canvasRoot);

//            Canvas canvas = _canvasRoot.AddComponent<Canvas>();
//            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//            canvas.overrideSorting = true;

//            canvas.sortingOrder = 32000;

//            CanvasScaler scaler = _canvasRoot.AddComponent<CanvasScaler>();
//            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
//            scaler.referenceResolution = new Vector2(1920f, 1080f);
//            scaler.matchWidthOrHeight = 0.5f;
//            _canvasRoot.AddComponent<GraphicRaycaster>();
//        }

//        private static void EnsureGameLauncherUi()
//        {
//            EnsureCanvasRoot();
//            if (_gameLauncher == null)
//                BuildGameLauncher();
//        }

//        private static void EnsurePickerUi()
//        {
//            EnsureCanvasRoot();

//            if (_gameLauncher == null)
//                BuildGameLauncher();

//            if (_blocker == null || _window == null)
//                BuildPickerWindow();
//        }


//        private static void BuildGameLauncher()
//        {
//            _gameLauncher = new GameObject("GameChatSymbolLauncher");
//            _gameLauncher.transform.SetParent(_canvasRoot.transform, false);

//            RectTransform rt = _gameLauncher.AddComponent<RectTransform>();

//            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
//            rt.pivot = new Vector2(1f, 0f);
//            rt.anchoredPosition = new Vector2(-520f, 155f);

//            rt.sizeDelta = new Vector2(100f, 100f);


//            Image image = _gameLauncher.AddComponent<Image>();
//            image.sprite = Utils.LoadSprite("BanMod.Resources.image.icon.png", 100f);

//            image.color = new Color(1f, 1f, 1f, 0f);
//            image.raycastTarget = true;
//            image.preserveAspect = true;

//            _gameLauncherButton = _gameLauncher.AddComponent<Button>();
//            _gameLauncherButton.targetGraphic = image;
//            _gameLauncherButton.transition = Selectable.Transition.None;

//            _gameLauncherButton.onClick.AddListener((UnityAction)delegate
//            {
//                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
//                    return;
//                OpenForGameChat();
//            });

//            _gameLauncher.SetActive(false);
//        }

//        private static void BuildPickerWindow()
//        {
//            _blocker = new GameObject("PickerBlocker");
//            _blocker.transform.SetParent(_canvasRoot.transform, false);
//            RectTransform blockerRt = _blocker.AddComponent<RectTransform>();
//            Stretch(blockerRt);
//            Image blockerImage = _blocker.AddComponent<Image>();
//            blockerImage.color = new Color(0f, 0f, 0f, 0.64f);
//            blockerImage.raycastTarget = true;
//            Button blockerButton = _blocker.AddComponent<Button>();
//            blockerButton.transition = Selectable.Transition.None;
//            blockerButton.targetGraphic = blockerImage;
//            blockerButton.onClick.AddListener((UnityAction)CloseWindow);

//            _window = new GameObject("PickerWindow");
//            _window.transform.SetParent(_blocker.transform, false);
//            RectTransform wrt = _window.AddComponent<RectTransform>();
//            wrt.anchorMin = wrt.anchorMax = new Vector2(0.5f, 0.5f);
//            wrt.pivot = new Vector2(0.5f, 0.5f);
//            wrt.sizeDelta = new Vector2(1080f, 720f);
//            wrt.anchoredPosition = Vector2.zero;
//            Image wbg = _window.AddComponent<Image>();
//            wbg.sprite = RoundedSprite();
//            wbg.type = Image.Type.Sliced;
//            wbg.color = new Color(0.105f, 0.12f, 0.16f, 0.995f);
//            wbg.raycastTarget = true;
//            Shadow shadow = _window.AddComponent<Shadow>();
//            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
//            shadow.effectDistance = new Vector2(0f, -8f);
//            Outline edge = _window.AddComponent<Outline>();
//            edge.effectColor = new Color(0.055f, 0.065f, 0.085f, 0.95f);
//            edge.effectDistance = new Vector2(1f, -1f);

//            CreateUiButton(_window.transform, "Close", "×", new Vector2(507f, 326f), new Vector2(52f, 48f), new Color(0.10f, 0.12f, 0.16f, 1f))
//                .onClick.AddListener((UnityAction)CloseWindow);

//            _colorControls.Clear();
//            TextMeshProUGUI colorLabel = CreateText(_window.transform, "COLOR", 16f, TextAlignmentOptions.Left, new Color(0.80f, 0.85f, 0.94f, 1f), new Vector2(-455f, 261f), new Vector2(100f, 30f), false);
//            _colorControls.Add(colorLabel.gameObject);
//            _colorText = CreateText(_window.transform, "#FFFFFF", 15f, TextAlignmentOptions.Left, Color.white, new Vector2(-340f, 261f), new Vector2(105f, 30f), false);
//            _colorControls.Add(_colorText.gameObject);
//            BuildPalette();
//            BuildSymbolSearchInput();

//            _copyInfoText = CreateText(_window.transform,
//                "Click a symbol to copy it. Press Ctrl+V in chat to paste.",
//                18f, TextAlignmentOptions.Left,
//                new Color(0.86f, 0.90f, 0.97f, 1f),
//                new Vector2(-200f, 145f), new Vector2(505f, 30f), false);
//            _favoritesInfoText = CreateText(_window.transform,
//                "Right-click a symbol to add or remove it from Favorites.",
//                18f, TextAlignmentOptions.Left,
//                new Color(0.86f, 0.90f, 0.97f, 1f),
//                new Vector2(270f, 145), new Vector2(500f, 30f), false);
//            CreateText(_window.transform,
//                "Hold Alt and drag the button to move it",
//                16f, TextAlignmentOptions.Center,
//                new Color(0.86f, 0.90f, 0.97f, 1f),
//                new Vector2(0f, -340f), new Vector2(920f, 26f), false);

//            EnsureFavoritesLoaded();
//            BuildCategoryScroll();
//            BuildSymbolScroll();
//            SelectCategory(1);

//            _window.SetActive(false);
//            _blocker.SetActive(false);
//        }

//        private static void BuildPalette()
//        {
//            float startX = -250f;
//            float y = 261f;
//            float step = 31f;
//            for (int i = 0; i < PaletteHex.Length; i++)
//            {
//                int captured = i;
//                Color color;
//                if (!ColorUtility.TryParseHtmlString("#" + PaletteHex[i], out color))
//                    color = Color.white;

//                Button b = CreateUiButton(_window.transform, "Color_" + PaletteHex[i], "", new Vector2(startX + i * step, y), new Vector2(26f, 26f), color);
//                _colorControls.Add(b.gameObject);
//                b.onClick.AddListener((UnityAction)delegate
//                {
//                    _selectedHex = PaletteHex[captured];
//                    RefreshHeader();
//                });
//            }
//        }

//        private static void BuildSymbolSearchInput()
//        {
//            GameObject root = new GameObject("SymbolSearch");
//            root.transform.SetParent(_window.transform, false);
//            RectTransform rt = root.AddComponent<RectTransform>();
//            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
//            rt.pivot = new Vector2(0.5f, 0.5f);
//            rt.anchoredPosition = new Vector2(-7f, 315f);
//            rt.sizeDelta = new Vector2(940f, 43f);
//            Image bg = root.AddComponent<Image>();
//            bg.sprite = RoundedSprite();
//            bg.type = Image.Type.Sliced;
//            bg.color = new Color(0.20f, 0.23f, 0.30f, 1f);
//            bg.raycastTarget = true;
//            _symbolSearchInput = root.AddComponent<TMP_InputField>();
//            GameObject textGo = new GameObject("SearchText");
//            textGo.transform.SetParent(root.transform, false);
//            RectTransform tr = textGo.AddComponent<RectTransform>();
//            Stretch(tr, new Vector4(9f, 5f, 9f, 5f));
//            TextMeshProUGUI label = textGo.AddComponent<TextMeshProUGUI>();
//            label.fontSize = 17f;
//            label.color = Color.white;
//            label.alignment = TextAlignmentOptions.MidlineLeft;
//            label.enableWordWrapping = false;
//            _symbolSearchInput.textViewport = rt;
//            _symbolSearchInput.textComponent = label;
//            _symbolSearchInput.lineType = TMP_InputField.LineType.SingleLine;
//            _symbolSearchInput.onValueChanged.AddListener((UnityAction<string>)delegate (string value)
//            {
//                _symbolSearchText = value ?? "";
//                SelectCategory(_selectedCategoryIndex);
//            });
//            GameObject placeholderGo = new GameObject("SearchPlaceholder");
//            placeholderGo.transform.SetParent(root.transform, false);
//            RectTransform placeholderRect = placeholderGo.AddComponent<RectTransform>();
//            Stretch(placeholderRect, new Vector4(9f, 3f, 9f, 3f));
//            TextMeshProUGUI placeholder = placeholderGo.AddComponent<TextMeshProUGUI>();
//            placeholder.text = "Search by name or category...";
//            placeholder.fontSize = 16f;
//            placeholder.color = new Color(0.70f, 0.75f, 0.84f, 0.9f);
//            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
//            placeholder.enableWordWrapping = true;
//            placeholder.raycastTarget = false;
//            _symbolSearchInput.placeholder = placeholder;
//        }

//        private static void BuildCategoryScroll()
//        {

//            GameObject holder = new GameObject("SymbolTabs");
//            holder.transform.SetParent(_window.transform, false);
//            _categoryContent = holder.AddComponent<RectTransform>();
//            _categoryContent.anchorMin = _categoryContent.anchorMax = new Vector2(0.5f, 0.5f);
//            _categoryContent.anchoredPosition = new Vector2(0f, 94f);
//            _categoryContent.sizeDelta = new Vector2(950f, 45f);
//            for (int i = 0; i < 2; i++)
//            {
//                int captured = i;
//                string caption = i == 0 ? "Favorites ★" : "All Symbols";
//                Button tab = CreateUiButton(_categoryContent, "Tab_" + i, caption,
//                    new Vector2(i == 0 ? 120f : -120f, 0f), new Vector2(220f, 40f),
//                    new Color(0.20f, 0.23f, 0.30f, 1f));
//                tab.onClick.AddListener((UnityAction)delegate { SelectCategory(captured); });
//            }
//        }

//        private static void BuildSymbolScroll()
//        {
//            GameObject root = new GameObject("SymbolScroll");
//            root.transform.SetParent(_window.transform, false);
//            RectTransform rr = root.AddComponent<RectTransform>();
//            _symbolScrollRoot = rr;
//            rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
//            rr.pivot = new Vector2(0.5f, 0.5f);
//            rr.anchoredPosition = new Vector2(0f, -132f);
//            rr.sizeDelta = new Vector2(950f, 400f);
//            Image bg = root.AddComponent<Image>();
//            bg.sprite = RoundedSprite();
//            bg.type = Image.Type.Sliced;
//            bg.color = new Color(0.14f, 0.16f, 0.21f, 1f);
//            bg.raycastTarget = true;
//            ScrollRect scroll = root.AddComponent<ScrollRect>();
//            scroll.horizontal = false;
//            scroll.vertical = true;
//            scroll.movementType = ScrollRect.MovementType.Clamped;
//            scroll.scrollSensitivity = 42f;

//            GameObject viewport = new GameObject("Viewport");
//            viewport.transform.SetParent(root.transform, false);
//            RectTransform vrt = viewport.AddComponent<RectTransform>();
//            Stretch(vrt, new Vector4(10f, 10f, 10f, 10f));
//            Image vpImage = viewport.AddComponent<Image>();
//            vpImage.color = new Color(0f, 0f, 0f, 0.001f);
//            vpImage.raycastTarget = true;
//            viewport.AddComponent<RectMask2D>();

//            GameObject content = new GameObject("Content");
//            content.transform.SetParent(viewport.transform, false);
//            _symbolContent = content.AddComponent<RectTransform>();
//            _symbolContent.anchorMin = new Vector2(0f, 1f);
//            _symbolContent.anchorMax = new Vector2(1f, 1f);
//            _symbolContent.pivot = new Vector2(0.5f, 1f);
//            _symbolContent.anchoredPosition = Vector2.zero;
//            _symbolContent.sizeDelta = new Vector2(0f, 540f);

//            scroll.viewport = vrt;
//            scroll.content = _symbolContent;
//        }

//        private static void SelectCategory(int index)
//        {
//            if (_symbolContent == null)
//                return;

//            EnsureFavoritesLoaded();

//            int categoryCount = GetCategoryCount();
//            if (categoryCount <= 0)
//                return;

//            _selectedCategoryIndex = Mathf.Clamp(index, 0, categoryCount - 1);
//            if (_categoryContent != null)
//            {
//                for (int c = 0; c < _categoryContent.childCount; c++)
//                {
//                    Image image = _categoryContent.GetChild(c).GetComponent<Image>();
//                    if (image != null) image.color = c == _selectedCategoryIndex
//                        ? new Color(0.31f, 0.44f, 0.64f, 1f)
//                        : new Color(0.38f, 0.42f, 0.51f, 1f);
//                }
//            }
//            FavoriteHitTargets.Clear();
//            ClearChildren(_symbolContent);

//            List<string> symbols = GetCategorySymbols(_selectedCategoryIndex);
//            symbols = FilterSymbolsBySearch(symbols);
//            const int columns = 11;
//            const float cellW = 86f;
//            const float cellH = 66f;
//            const float buttonW = 74f;
//            const float buttonH = 54f;
//            float startX = -430f;
//            float startY = -38f;

//            if (_selectedCategoryIndex == FavoritesCategoryIndex && symbols.Count == 0)
//            {
//                TextMeshProUGUI empty = CreateText(
//                    _symbolContent,
//                    "No favorites yet.\nRight-click a symbol to add it.",
//                    19f,
//                    TextAlignmentOptions.Center,
//                    new Color(0.72f, 0.76f, 0.86f, 1f),
//                    new Vector2(0f, -115f),
//                    new Vector2(720f, 80f),
//                    false);
//                empty.enableWordWrapping = true;
//            }

//            for (int i = 0; i < symbols.Count; i++)
//            {
//                string captured = symbols[i];
//                string preview = "<color=#" + SymbolPreviewHex(captured) + ">" + captured + "</color>";
//                int row = i / columns;
//                int col = i % columns;
//                Vector2 pos = new Vector2(startX + col * cellW, startY - row * cellH);
//                Button b = CreateUiButton(_symbolContent, "Symbol_" + i, preview, pos, new Vector2(buttonW, buttonH), new Color(0.20f, 0.23f, 0.30f, 1f));
//                RectTransform br = b.GetComponent<RectTransform>();
//                if (br != null)
//                {
//                    br.anchorMin = br.anchorMax = new Vector2(0.5f, 1f);
//                    br.pivot = new Vector2(0.5f, 0.5f);
//                    br.anchoredPosition = pos;
//                }

//                b.onClick.AddListener((UnityAction)delegate
//                {
//                    try
//                    {

//                        if (_insertAction == null) return;
//                        _insertAction(captured);
//                        TMP_InputField focusInput = _focusInputAfterCopy;
//                        bool focusGameChat = _focusGameChatAfterCopy;
//                        CloseWindow();

//                        if (focusGameChat)
//                        {
//                            try { if (_gameAdapter != null) _gameAdapter.Focus(); } catch { }
//                        }
//                        else if (focusInput != null)
//                        {
//                            try { focusInput.ActivateInputField(); } catch { }
//                        }
//                    }
//                    catch
//                    {
//                    }
//                });

//                FavoriteHitTargets.Add(new FavoriteHitTarget
//                {
//                    Rect = br,
//                    Symbol = captured
//                });

//                TextMeshProUGUI t = b.GetComponentInChildren<TextMeshProUGUI>(true);
//                if (t != null)
//                {
//                    t.fontSize = 24f;
//                    t.enableAutoSizing = true;
//                    t.fontSizeMin = 13f;
//                    t.fontSizeMax = 26f;
//                }
//            }

//            int rows = (symbols.Count + columns - 1) / columns;
//            _symbolContent.sizeDelta = new Vector2(0f, Mathf.Max(475f, 24f + rows * cellH));
//            RefreshHeader();
//        }

//        private static int GetCategoryCount()
//        {
//            return 2;
//        }

//        private static string GetCategoryName(int index)
//        {
//            if (index == FavoritesCategoryIndex)
//                return FavoritesCategoryName;

//            if (index == 1) return "All Symbols";
//            int realIndex = index - 2;
//            if (realIndex < 0 || realIndex >= Categories.Length)
//                return "Symbols";

//            return Categories[realIndex].Name;
//        }

//        private static List<string> GetCategorySymbols(int index)
//        {
//            if (index == FavoritesCategoryIndex)
//                return new List<string>(FavoriteSymbols);

//            if (index == 1)
//            {
//                List<string> all = new List<string>();
//                HashSet<string> seen = new HashSet<string>();
//                for (int c = 0; c < Categories.Length; c++)
//                    foreach (string symbol in SplitUnique(Categories[c].Symbols))
//                        if (seen.Add(symbol)) all.Add(symbol);
//                return all;
//            }
//            int realIndex = index - 2;
//            if (realIndex < 0 || realIndex >= Categories.Length)
//                return new List<string>();

//            return SplitUnique(Categories[realIndex].Symbols);
//        }

//        private static List<string> FilterSymbolsBySearch(List<string> original)
//        {
//            string query = (_symbolSearchText ?? "").Trim();
//            if (query.Length == 0) return original;
//            string[] terms = query.ToLowerInvariant().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
//            HashSet<string> matching = new HashSet<string>();
//            for (int c = 0; c < Categories.Length; c++)
//            {
//                string category = Categories[c].Name.ToLowerInvariant();
//                string aliases = CategorySearchAliases(category);
//                bool match = true;
//                for (int t = 0; t < terms.Length; t++)
//                    if (!category.Contains(terms[t]) && !aliases.Contains(terms[t])) { match = false; break; }
//                if (!match) continue;
//                foreach (string symbol in SplitUnique(Categories[c].Symbols)) matching.Add(symbol);
//            }
//            List<string> filtered = new List<string>();
//            foreach (string symbol in original)
//                if (symbol.Contains(query) || matching.Contains(symbol)) filtered.Add(symbol);
//            return filtered;
//        }

//        private static string CategorySearchAliases(string name)
//        {
//            if (name.Contains("star")) return "star stars";
//            if (name.Contains("arrow")) return "arrow arrows direction";
//            if (name.Contains("heart")) return "heart hearts love";
//            if (name.Contains("currency")) return "currency coin euro dollar money";
//            if (name.Contains("math")) return "math mathematics numbers number";
//            if (name.Contains("music")) return "music musical note notes";
//            if (name.Contains("shape")) return "shape shapes circle triangle square";
//            if (name.Contains("emoji")) return "faces smileys emoji";
//            if (name.Contains("check")) return "check tick confirm";
//            if (name.Contains("card")) return "cards poker";
//            if (name.Contains("bracket")) return "brackets parentheses";
//            if (name.Contains("gender")) return "gender face faces";
//            if (name.Contains("degree")) return "degrees temperature";
//            if (name.Contains("punctuation")) return "punctuation";
//            if (name.Contains("copyright")) return "copyright trademark rights";
//            return name;
//        }

//        private static void EnsureFavoritesLoaded()
//        {
//            if (_favoritesLoaded)
//                return;

//            _favoritesLoaded = true;
//            FavoriteSymbols.Clear();
//            FavoriteSymbolSet.Clear();

//            try
//            {
//                Directory.CreateDirectory(FavoritesFolder);
//                if (!File.Exists(FavoritesFilePath))
//                    return;

//                string[] lines = File.ReadAllLines(FavoritesFilePath, Encoding.UTF8);
//                for (int i = 0; i < lines.Length; i++)
//                {
//                    string symbol = (lines[i] ?? "").Trim();
//                    if (string.IsNullOrEmpty(symbol) || !FavoriteSymbolSet.Add(symbol))
//                        continue;

//                    FavoriteSymbols.Add(symbol);
//                }
//            }
//            catch
//            {
//            }
//        }

//        private static void SaveFavorites()
//        {
//            try
//            {
//                Directory.CreateDirectory(FavoritesFolder);
//                File.WriteAllText(
//                    FavoritesFilePath,
//                    string.Join("\n", FavoriteSymbols.ToArray()),
//                    new UTF8Encoding(false));
//            }
//            catch
//            {
//            }
//        }

//        private static void ToggleFavorite(string symbol)
//        {
//            if (string.IsNullOrEmpty(symbol))
//                return;

//            EnsureFavoritesLoaded();

//            if (FavoriteSymbolSet.Remove(symbol))
//            {
//                FavoriteSymbols.Remove(symbol);
//            }
//            else
//            {
//                FavoriteSymbolSet.Add(symbol);
//                FavoriteSymbols.Add(symbol);
//            }

//            SaveFavorites();

//            if (_selectedCategoryIndex == FavoritesCategoryIndex)
//                SelectCategory(FavoritesCategoryIndex);
//            else
//                RefreshHeader();
//        }

//        private static void HandleFavoriteRightClickHitTest()
//        {
//            if (!Input.GetMouseButtonDown(1) || FavoriteHitTargets.Count == 0)
//                return;

//            Vector2 mouse = Input.mousePosition;
//            for (int i = FavoriteHitTargets.Count - 1; i >= 0; i--)
//            {
//                FavoriteHitTarget hit = FavoriteHitTargets[i];
//                if (hit == null || hit.Rect == null || string.IsNullOrEmpty(hit.Symbol))
//                    continue;

//                try
//                {
//                    if (!hit.Rect.gameObject.activeInHierarchy)
//                        continue;

//                    if (!RectTransformUtility.RectangleContainsScreenPoint(hit.Rect, mouse, null))
//                        continue;

//                    ToggleFavorite(hit.Symbol);
//                    return;
//                }
//                catch { }
//            }
//        }

//        private static List<string> SplitUnique(string packed)
//        {
//            List<string> result = new List<string>();
//            HashSet<string> seen = new HashSet<string>();
//            string[] parts = (packed ?? "").Split('|');
//            for (int i = 0; i < parts.Length; i++)
//            {
//                string s = parts[i];
//                if (string.IsNullOrEmpty(s) || !seen.Add(s))
//                    continue;
//                result.Add(s);
//            }
//            return result;
//        }

//        private static void RefreshHeader()
//        {
//            try
//            {
//                bool colored = _colorAllowed != null && _colorAllowed();
//                bool showPalette = colored && SafeGameColorAllowed();
//                float infoY = showPalette ? 207f : 255f;
//                if (_copyInfoText != null)
//                {
//                    RectTransform rect = _copyInfoText.GetComponent<RectTransform>();
//                    if (rect != null) rect.anchoredPosition = new Vector2(-200f, infoY);
//                }
//                if (_favoritesInfoText != null)
//                {
//                    RectTransform rect = _favoritesInfoText.GetComponent<RectTransform>();
//                    if (rect != null) rect.anchoredPosition = new Vector2(270f, infoY);
//                }
//                float tabsY = infoY - 45f;
//                if (_categoryContent != null)
//                    _categoryContent.anchoredPosition = new Vector2(0f, tabsY);
//                if (_symbolScrollRoot != null)
//                {
//                    float topY = tabsY - 26f;
//                    const float bottomY = -332f;
//                    _symbolScrollRoot.anchoredPosition = new Vector2(0f, (topY + bottomY) * 0.5f);
//                    _symbolScrollRoot.sizeDelta = new Vector2(950f, topY - bottomY);
//                }
//                for (int i = 0; i < _colorControls.Count; i++)
//                {
//                    GameObject item = _colorControls[i];
//                    if (item != null && item.activeSelf != showPalette)
//                        item.SetActive(showPalette);
//                }
//                if (_colorText != null && showPalette)
//                {
//                    _colorText.text = "#" + _selectedHex;
//                    Color parsed;
//                    _colorText.color = ColorUtility.TryParseHtmlString("#" + _selectedHex, out parsed) ? parsed : Color.white;
//                }
//            }
//            catch { }
//        }

//        private static void OpenWindow()
//        {
//            if (_blocker != null) _blocker.SetActive(true);
//            if (_window != null) _window.SetActive(true);
//            if (_gameLauncher != null) _gameLauncher.SetActive(false);
//        }

//        private static void CloseWindow()
//        {
//            if (_window != null) _window.SetActive(false);
//            if (_blocker != null) _blocker.SetActive(false);
//            _insertAction = null;
//            _focusInputAfterCopy = null;
//            _focusGameChatAfterCopy = false;
//            _colorAllowed = null;
//            _symbolSearchText = "";
//            if (_symbolSearchInput != null) _symbolSearchInput.SetTextWithoutNotify("");
//            if (_symbolContent != null) SelectCategory(1);
//        }

//        private static Sprite _roundedSprite;
//        private static Sprite RoundedSprite()
//        {
//            if (_roundedSprite != null) return _roundedSprite;
//            const int side = 64;
//            const int radius = 14;
//            Texture2D texture = new Texture2D(side, side, TextureFormat.RGBA32, false);
//            texture.filterMode = FilterMode.Bilinear;
//            for (int y = 0; y < side; y++)
//                for (int x = 0; x < side; x++)
//                {
//                    float cx = Mathf.Clamp(x, radius, side - radius - 1);
//                    float cy = Mathf.Clamp(y, radius, side - radius - 1);
//                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
//                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(radius - dist + 0.5f)));
//                }
//            texture.Apply();
//            _roundedSprite = Sprite.Create(texture, new Rect(0f, 0f, side, side),
//                new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect,
//                new Vector4(16f, 16f, 16f, 16f));
//            return _roundedSprite;
//        }

//        private static Button CreateUiButton(Transform parent, string name, string text, Vector2 position, Vector2 size, Color color)
//        {
//            GameObject go = new GameObject(name);
//            go.transform.SetParent(parent, false);
//            RectTransform rt = go.AddComponent<RectTransform>();
//            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
//            rt.pivot = new Vector2(0.5f, 0.5f);
//            rt.anchoredPosition = position;
//            rt.sizeDelta = size;
//            Image img = go.AddComponent<Image>();
//            img.sprite = RoundedSprite();
//            img.type = Image.Type.Sliced;
//            img.color = color;
//            img.raycastTarget = true;
//            Button button = go.AddComponent<Button>();
//            button.targetGraphic = img;
//            button.transition = Selectable.Transition.ColorTint;
//            ColorBlock cb = button.colors;
//            cb.normalColor = Color.white;
//            cb.highlightedColor = new Color(1.14f, 1.14f, 1.14f, 1f);
//            cb.pressedColor = new Color(0.52f, 0.75f, 1f, 1f);
//            cb.fadeDuration = 0f;
//            cb.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.65f);
//            button.colors = cb;
//            if (!string.IsNullOrEmpty(text))
//                CreateText(go.transform, text, 20f, TextAlignmentOptions.Center, Color.white, Vector2.zero, Vector2.zero, true);
//            return button;
//        }

//        private static TextMeshProUGUI CreateText(Transform parent, string text, float fontSize, TextAlignmentOptions align, Color color, Vector2 position, Vector2 size, bool stretch)
//        {
//            GameObject go = new GameObject("Text");
//            go.transform.SetParent(parent, false);
//            RectTransform rt = go.AddComponent<RectTransform>();
//            if (stretch)
//            {
//                Stretch(rt, new Vector4(3f, 3f, 3f, 3f));
//            }
//            else
//            {
//                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
//                rt.pivot = new Vector2(0.5f, 0.5f);
//                rt.anchoredPosition = position;
//                rt.sizeDelta = size;
//            }

//            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
//            tmp.text = text ?? "";
//            tmp.fontSize = fontSize;
//            tmp.alignment = align;
//            tmp.color = color;
//            tmp.enableWordWrapping = false;
//            tmp.raycastTarget = false;
//            tmp.richText = true;
//            try { if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset; } catch { }
//            return tmp;
//        }

//        private static void Stretch(RectTransform rt)
//        {
//            Stretch(rt, Vector4.zero);
//        }

//        private static void Stretch(RectTransform rt, Vector4 pad)
//        {
//            if (rt == null) return;
//            rt.anchorMin = Vector2.zero;
//            rt.anchorMax = Vector2.one;
//            rt.pivot = new Vector2(0.5f, 0.5f);
//            rt.offsetMin = new Vector2(pad.x, pad.y);
//            rt.offsetMax = new Vector2(-pad.z, -pad.w);
//        }

//        private static void ClearChildren(RectTransform root)
//        {
//            if (root == null) return;
//            for (int i = root.childCount - 1; i >= 0; i--)
//            {
//                Transform child = root.GetChild(i);
//                if (child != null)
//                    UnityEngine.Object.Destroy(child.gameObject);
//            }
//        }

//        private sealed class GameChatTextAdapter
//        {
//            private readonly ChatController _chat;
//            private readonly FreeChatInputField _freeChat;
//            private readonly TextBoxTMP _textBox;
//            private readonly ChatInputFieldButton _submitButton;

//            private GameChatTextAdapter(
//                ChatController chat,
//                FreeChatInputField freeChat,
//                TextBoxTMP textBox,
//                ChatInputFieldButton submitButton)
//            {
//                _chat = chat;
//                _freeChat = freeChat;
//                _textBox = textBox;
//                _submitButton = submitButton;
//            }

//            internal bool IsAlive
//            {
//                get
//                {
//                    try { return _chat != null && _freeChat != null && _textBox != null; }
//                    catch { return false; }
//                }
//            }

//            internal bool IsVisible
//            {
//                get
//                {
//                    try
//                    {
//                        if (!IsAlive || !_chat.gameObject.activeInHierarchy || !_freeChat.gameObject.activeInHierarchy)
//                            return false;

//                        try
//                        {
//                            if (_chat.IsOpenOrOpening)
//                                return true;
//                        }
//                        catch { }

//                        if (IsSubmitButtonActuallyVisible())
//                            return true;

//                        SpriteRenderer bg = null;
//                        try { bg = _freeChat.Background; } catch { }
//                        if (bg != null && bg.gameObject.activeInHierarchy && IsBoundsInsideScreen(bg.bounds, 140f))
//                            return true;

//                        try
//                        {
//                            if (_textBox.outputText != null &&
//                                _textBox.outputText.gameObject.activeInHierarchy &&
//                                _textBox.outputText.enabled)
//                            {
//                                Renderer renderer = _textBox.outputText.GetComponent<Renderer>();
//                                if (renderer != null && renderer.enabled && IsBoundsInsideScreen(renderer.bounds, 140f))
//                                    return true;
//                            }
//                        }
//                        catch { }

//                        return false;
//                    }
//                    catch { return false; }
//                }
//            }

//            private bool IsSubmitButtonActuallyVisible()
//            {
//                if (_submitButton == null || !_submitButton.gameObject.activeInHierarchy)
//                    return false;

//                try
//                {
//                    Renderer[] renderers = _submitButton.GetComponentsInChildren<Renderer>(true);
//                    for (int i = 0; i < renderers.Length; i++)
//                    {
//                        Renderer renderer = renderers[i];
//                        if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
//                            continue;
//                        if (IsBoundsInsideScreen(renderer.bounds, 140f))
//                            return true;
//                    }
//                }
//                catch { }

//                return false;
//            }

//            private static bool IsBoundsInsideScreen(Bounds bounds, float margin)
//            {
//                try
//                {
//                    Camera camera = GetGameUiCamera();
//                    if (camera == null)
//                        return true;

//                    Vector3 screen = RectTransformUtility.WorldToScreenPoint(camera, bounds.center);
//                    return screen.x >= -margin && screen.x <= Screen.width + margin &&
//                           screen.y >= -margin && screen.y <= Screen.height + margin;
//                }
//                catch { return false; }
//            }

//            internal static GameChatTextAdapter TryCreate(ChatController chat)
//            {
//                if (chat == null)
//                    return null;

//                try
//                {
//                    FreeChatInputField freeChat = null;
//                    TextBoxTMP textBox = null;
//                    ChatInputFieldButton submitButton = null;

//                    try { freeChat = chat.GetComponentInChildren<FreeChatInputField>(true); } catch { }

//                    if (freeChat == null)
//                    {
//                        Component[] components = chat.GetComponentsInChildren<Component>(true);
//                        for (int i = 0; i < components.Length; i++)
//                        {
//                            Component c = components[i];
//                            if (c == null)
//                                continue;
//                            if (freeChat == null)
//                                freeChat = c as FreeChatInputField;
//                        }
//                    }

//                    if (freeChat == null)
//                        return null;

//                    try { textBox = freeChat.GetComponentInChildren<TextBoxTMP>(true); } catch { }
//                    try { submitButton = freeChat.GetComponentInChildren<ChatInputFieldButton>(true); } catch { }

//                    Component[] freeComponents = freeChat.GetComponentsInChildren<Component>(true);
//                    for (int i = 0; i < freeComponents.Length; i++)
//                    {
//                        Component c = freeComponents[i];
//                        if (c == null)
//                            continue;

//                        if (textBox == null)
//                            textBox = c as TextBoxTMP;

//                        if (submitButton == null)
//                            submitButton = c as ChatInputFieldButton;
//                    }

//                    if (submitButton == null)
//                    {
//                        try
//                        {
//                            ChatInputFieldButton[] allSubmitButtons = chat.GetComponentsInChildren<ChatInputFieldButton>(true);
//                            if (allSubmitButtons != null && allSubmitButtons.Length > 0)
//                                submitButton = allSubmitButtons[0];
//                        }
//                        catch { }
//                    }

//                    if (textBox == null)
//                        return null;


//                    return new GameChatTextAdapter(chat, freeChat, textBox, submitButton);
//                }
//                catch (Exception)
//                {
//                    return null;
//                }
//            }

//            internal void Insert(string token)
//            {

//                if (string.IsNullOrEmpty(token) || !IsAlive)
//                {
//                    return;
//                }

//                try
//                {
//                    bool clearOnFocus = _textBox.ClearOnFocus;
//                    try
//                    {
//                        _textBox.ClearOnFocus = false;
//                        _textBox.GiveFocus();
//                    }
//                    finally { _textBox.ClearOnFocus = clearOnFocus; }
//                    string current = StripPickerColorTags(_textBox.text ?? "");
//                    TrackEditorEdit(current);
//                    int caret = Mathf.Clamp(ReadInt(_textBox, current.Length, "caretPos"), 0, current.Length);
//                    if (caret > 0 && caret < current.Length && char.IsHighSurrogate(current[caret - 1]) &&
//                        char.IsLowSurrogate(current[caret])) caret++;
//                    string plainToken = StripPickerColorTags(token);
//                    string next = current.Insert(caret, plainToken);
//                    int limit = _textBox.characterLimit;
//                    if (limit > 0 && next.Length > limit) return;
//                    EnsureGameNativeSymbolWhitelist();
//                    _textBox.AllowSymbols = true;
//                    SetMember(_textBox, "caretPos", caret);
//                    _textBox.SetText(next, "");
//                    ObserveGameEditor(_textBox);

//                    if (string.Equals(_textBox.text, next, StringComparison.Ordinal))
//                    {
//                        for (int n = caret; n < caret + plainToken.Length; n++)
//                            _editorColors[n] = SafeGameColorAllowed() ? _selectedHex : null;
//                    }
//                    else
//                    {
//                    }


//                    try { _freeChat.UpdateCharCount(); } catch { }


//                }
//                catch
//                {
//                }
//            }

//            internal void UpdateNativeLauncherSprite(bool visible)
//            {
//                if (_submitButton == null) return;
//                SpriteRenderer send = _submitButton.GetComponentInChildren<SpriteRenderer>(true);
//                if (send == null) return;
//                if (_gameLauncherNativeSprite == null)
//                {
//                    Sprite omega = null;

//                    if (BanMod.DarkTheme.Value)
//                    {
//                        omega = Utils.LoadSprite("BanMod.Resources.image.icondark.png", 100f);
//                    }
//                    else
//                    {
//                        omega = Utils.LoadSprite("BanMod.Resources.image.icon.png", 100f);
//                    }
//                    if (omega == null)
//                    {
//                        return;
//                    }
//                    GameObject go = new GameObject("BanMod_Omega_NativeSendSprite");
//                    go.layer = send.gameObject.layer;
//                    go.transform.SetParent(send.transform.parent, false);
//                    _gameLauncherNativeSprite = go.AddComponent<SpriteRenderer>();
//                    _gameLauncherNativeSprite.sprite = omega;
//                    _gameLauncherNativeSprite.color = new Color(1f, 1f, 1f, 0.25f);
//                }

//                SpriteRenderer icon = _gameLauncherNativeSprite;
//                if (icon == null) return;
//                icon.gameObject.layer = send.gameObject.layer;
//                icon.sortingLayerID = send.sortingLayerID;
//                icon.sortingOrder = send.sortingOrder + OmegaSortingOffset;
//                icon.color = new Color(1f, 1f, 1f, 0.25f);

//                icon.enabled = visible;
//                if (!visible) return;

//                Camera cam = GetGameUiCamera();
//                float unitsPerPixel = cam != null && cam.orthographic
//                    ? 2f * cam.orthographicSize / Mathf.Max(1, Screen.height)
//                    : 0.01f;
//                Vector3 parentLossy = icon.transform.parent != null
//                    ? icon.transform.parent.lossyScale : Vector3.one;
//                float pxHeight = Mathf.Max(0.001f, icon.sprite.bounds.size.y);
//                float localSize = OmegaSizePixels * unitsPerPixel /
//                    Mathf.Max(0.0001f, pxHeight * Mathf.Abs(parentLossy.y));
//                icon.transform.localScale = new Vector3(localSize, localSize, 1f);


//                Vector3 world = send.bounds.center;
//                world.x += _omegaOffsetX * unitsPerPixel;
//                world.y += _omegaOffsetY * unitsPerPixel;
//                world.z = send.transform.position.z + OmegaZOffset;
//                if (icon.transform.parent != null)
//                    icon.transform.localPosition = icon.transform.parent.InverseTransformPoint(world);
//                else
//                    icon.transform.position = world;
//                icon.transform.localRotation = send.transform.localRotation;

//            }

//            internal void UpdateLauncherPosition(RectTransform launcherRect, RectTransform canvasRect)
//            {
//                if (launcherRect == null || canvasRect == null || _submitButton == null)
//                    return;


//                try
//                {
//                    Camera uiCamera = GetGameUiCamera();
//                    Bounds sendBounds;
//                    Vector3 screenCenter;
//                    float leftEdge;

//                    if (TryGetSubmitBounds(out sendBounds))
//                    {
//                        Vector3 center = RectTransformUtility.WorldToScreenPoint(uiCamera, sendBounds.center);
//                        Vector3 edge = RectTransformUtility.WorldToScreenPoint(
//                            uiCamera, new Vector3(sendBounds.min.x, sendBounds.center.y, sendBounds.center.z));
//                        screenCenter = center;
//                        leftEdge = edge.x;
//                    }
//                    else
//                    {
//                        screenCenter = RectTransformUtility.WorldToScreenPoint(uiCamera, _submitButton.transform.position);
//                        leftEdge = screenCenter.x - 25f;
//                    }

//                    CanvasScaler scaler = canvasRect.GetComponent<CanvasScaler>();
//                    float scale = 1f;
//                    Canvas rootCanvas = canvasRect.GetComponent<Canvas>();
//                    if (rootCanvas != null && rootCanvas.scaleFactor > 0.001f)
//                        scale = rootCanvas.scaleFactor;

//                    launcherRect.sizeDelta = new Vector2(OmegaSizePixels / Mathf.Max(scale, 0.001f),
//                                                         OmegaSizePixels / Mathf.Max(scale, 0.001f));

//                    float halfWidthPixels = launcherRect.rect.width * scale * 0.5f;
//                    Vector2 target = new Vector2(leftEdge + 32f - halfWidthPixels, screenCenter.y);
//                    if (_gameLauncherNativeSprite != null && _gameLauncherNativeSprite.enabled)
//                    {
//                        Vector3 nativeScreen = RectTransformUtility.WorldToScreenPoint(
//                            uiCamera, _gameLauncherNativeSprite.bounds.center);
//                        target = new Vector2(nativeScreen.x, nativeScreen.y);
//                    }
//                    target.x = Mathf.Clamp(target.x, halfWidthPixels, Screen.width - halfWidthPixels);
//                    target.y = Mathf.Clamp(target.y, halfWidthPixels, Screen.height - halfWidthPixels);

//                    Vector2 localPoint;
//                    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, target, null, out localPoint))
//                        return;

//                    launcherRect.anchorMin = launcherRect.anchorMax = new Vector2(0.5f, 0.5f);
//                    launcherRect.pivot = new Vector2(0.5f, 0.5f);
//                    launcherRect.anchoredPosition = localPoint;
//                }
//                catch
//                {
//                }
//            }

//            private static Camera GetGameUiCamera()
//            {
//                Camera camera = null;
//                try
//                {
//                    if (HudManager.Instance != null)
//                        camera = HudManager.Instance.UICamera;
//                }
//                catch { }
//                if (camera == null)
//                    camera = Camera.main;
//                return camera;
//            }

//            private bool TryGetSubmitBounds(out Bounds bounds)
//            {
//                bounds = new Bounds();
//                if (_submitButton == null)
//                    return false;

//                bool found = false;
//                try
//                {
//                    Component[] components = _submitButton.GetComponentsInChildren<Component>(true);
//                    for (int i = 0; i < components.Length; i++)
//                    {
//                        SpriteRenderer renderer = components[i] as SpriteRenderer;
//                        if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
//                            continue;

//                        if (!found)
//                        {
//                            bounds = renderer.bounds;
//                            found = true;
//                        }
//                        else
//                        {
//                            bounds.Encapsulate(renderer.bounds);
//                        }
//                    }
//                }
//                catch { }

//                return found;
//            }

//            internal void Focus()
//            {
//                try
//                {
//                    if (_freeChat != null)
//                        _freeChat.Focus();
//                    else if (_textBox != null)
//                        _textBox.GiveFocus();
//                }
//                catch { }
//            }
//        }

//        private static readonly Dictionary<Type, Dictionary<string, MemberInfo>> MemberCache =
//            new Dictionary<Type, Dictionary<string, MemberInfo>>();

//        private static MemberInfo FindMemberQuietly(Type type, string name)
//        {
//            if (!MemberCache.TryGetValue(type, out var members))
//            {
//                members = new Dictionary<string, MemberInfo>();
//                MemberCache.Add(type, members);
//            }
//            if (members.TryGetValue(name, out var cached)) return cached;
//            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
//                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;


//            MemberInfo found = null;
//            for (Type current = type; current != null && found == null; current = current.BaseType)
//                found = current.GetField(name, flags);
//            for (Type current = type; current != null && found == null; current = current.BaseType)
//                found = current.GetProperty(name, flags);
//            members[name] = found;
//            return found;
//        }

//        private static object GetMember(object obj, string name)
//        {
//            if (obj == null || string.IsNullOrEmpty(name)) return null;
//            Type type = obj.GetType();
//            try
//            {
//                FieldInfo field = FindMemberQuietly(type, name) as FieldInfo;
//                if (field != null) return field.GetValue(obj);
//            }
//            catch { }
//            try
//            {
//                PropertyInfo prop = FindMemberQuietly(type, name) as PropertyInfo;
//                if (prop != null && prop.CanRead) return prop.GetValue(obj, null);
//            }
//            catch { }
//            return null;
//        }

//        private static bool SetMember(object obj, string name, object value)
//        {
//            if (obj == null || string.IsNullOrEmpty(name)) return false;
//            Type type = obj.GetType();
//            try
//            {
//                FieldInfo field = FindMemberQuietly(type, name) as FieldInfo;
//                if (field != null)
//                {
//                    field.SetValue(obj, value);
//                    return true;
//                }
//            }
//            catch { }
//            try
//            {
//                PropertyInfo prop = FindMemberQuietly(type, name) as PropertyInfo;
//                if (prop != null && prop.CanWrite)
//                {
//                    prop.SetValue(obj, value, null);
//                    return true;
//                }
//            }
//            catch { }
//            return false;
//        }

//        private static int ReadInt(object obj, int fallback, params string[] names)
//        {
//            for (int i = 0; i < names.Length; i++)
//            {
//                object value = GetMember(obj, names[i]);
//                if (value is int n) return n;
//            }
//            return fallback;
//        }

//        private static void SetIntMember(object obj, string name, int value)
//        {
//            try { SetMember(obj, name, value); } catch { }
//        }

//        private static void InvokeIntMethod(object obj, string name, int value)
//        {
//            if (obj == null) return;
//            try
//            {
//                MethodInfo method = AccessTools.Method(obj.GetType(), name, new Type[] { typeof(int) });
//                if (method != null) method.Invoke(obj, new object[] { value });
//            }
//            catch { }
//        }

//        private static void TryInvokeNoArgs(object obj, string name)
//        {
//            if (obj == null) return;
//            try
//            {
//                MethodInfo method = AccessTools.Method(obj.GetType(), name, Type.EmptyTypes);
//                if (method != null) method.Invoke(obj, null);
//            }
//            catch { }
//        }
//    }


//    [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText), new Type[] { typeof(string), typeof(string) })]
//    internal static class BanModSymbolUnicodeSetTextPatch
//    {
//        private static bool Prefix(TextBoxTMP __instance, string __0, string __1)
//        {
//            if (!BanModSymbolPicker.IsRegisteredGameTextBox(__instance)) return true;
//            BanModSymbolPicker.SetGameEditorText(__instance, __0, __1);
//            return false;
//        }
//    }


//    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
//    internal static class BanModSymbolColoredMessagePatch
//    {
//        private static bool Prefix(PlayerControl __instance, string __0, ref bool __result)
//        {
//            if (!BanModServerSelection.IsModded25 || __instance != PlayerControl.LocalPlayer)
//                return true;
//            if (!BanModSymbolPicker.TrySendPickerGameChatDirect(__instance, __0))
//                return true;
//            __result = true;
//            return false;
//        }
//    }

//    [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetText))]
//    internal static class BanModSymbolColoredBubblePatch
//    {
//        private static void Postfix(ChatBubble __instance, string chatText)
//        {
//            if (!BanModServerSelection.IsModded25 || __instance == null ||
//                __instance.TextArea == null || string.IsNullOrEmpty(chatText) ||
//                chatText.IndexOf("<color=#", StringComparison.OrdinalIgnoreCase) < 0)
//                return;
//            __instance.TextArea.richText = true;
//            __instance.TextArea.text = BanModSymbolPicker.ToSafeRichText(chatText);
//        }
//    }

//    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
//    internal static class BanModSymbolPickerRuntimePatch
//    {
//        private static void Postfix()
//        {
//            BanModSymbolPicker.RuntimeTick();
//        }
//    }
//}
using BanMod;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BanMod
{
    internal static class BanModSymbolPicker
    {
        private sealed class SymbolCategory
        {
            public readonly string Name;
            public readonly string Symbols;

            public SymbolCategory(string name, string symbols)
            {
                Name = name;
                Symbols = symbols;
            }
        }

        private sealed class FavoriteHitTarget
        {
            public RectTransform Rect;
            public string Symbol;
        }

        private static readonly SymbolCategory[] Categories =
        {
            new SymbolCategory("Stars", "★|☆|⁂|⁑|✽"),
            new SymbolCategory("Arrows", "☝|☞|☟|☜|↑|↓|→|←|↔|↕|⬆|↗|➡|↘|⬇|↙|⬅|↖|⤴|⤵|￩|￪|￫|￬|⇦|⇧|⇨|⇩|⇵|⇄|⇅|⇆|↹|↸|⌅|⌆|⏎|▶|➔"),
            new SymbolCategory("Shapes", "•|○|◦|⦿|▲|▼|♠|♥|♣|♦|♤|♡|♧|♢|■|□|▢|▣|▤|▥|▦|▧|▨|▩|▪|▫|◌|●|◐|◑|◒|◓|◯|◆|◇|◈|❖|▱|▶|◀|◉|◍|◎|〇|〶|〄"),
            new SymbolCategory("Symbols", "✓|╳|∞|†|✚|♫|♪|♲|♳|♴|♵|♶|♷|♸|♹|♺|♻|♼|♽|☎|☏|✂|♀|♂|⚠"),
            new SymbolCategory("Emoji", "😂|☹️|😆|☺️|😎|😉|😅|😊|😋|😀|😁|😃|😄|😍|☁️|☂️|☀️"),
            new SymbolCategory("Random", "‰|§|¶|©|™|¥|$|¢|€|ƒ|£|Æ"),
            new SymbolCategory("Copyright", "©|®|™|℡|№|℀|℅|☎|☏|‰|§|¶|☝|☜"),
            new SymbolCategory("Currency", "¢|$|€|£|¥|₩|₫|￥|¤|ƒ"),
            new SymbolCategory("Brackets", "〈|〉|《|》|「|」|『|』|〖|〗|〔|〕|︵|︶|︷|︸|︹|︺|︻|︼|︽|︾|︿|﹀|﹁|﹂|﹃|﹄|﹙|﹚|﹛|﹜|﹝|﹞|﹤|﹥|（|）|＜|＞|｛|｝|〘|〙|〚|〛|«|»|‹|›|〈|〉|〱"),
            new SymbolCategory("Cards", "♤|♠|♧|♣|♡|♥|♢|♦"),
            new SymbolCategory("Musical", "♩|♪|♫|♬|♭|♮|♯|°|ø|≠"),
            new SymbolCategory("Degree", "°|℃|℉|☀|☁|☂|☃|☉|♁|♨|㎎|㎏|㎜|㎝|㎞|㎡|㏄|㏎|㏑|㏒|㏕"),
            new SymbolCategory("Astrological", "☯|✚|†|‡|♁|❖|卍|卐|〷"),
            new SymbolCategory("Heart", "♥|♡"),
            new SymbolCategory("Check", "✓|∨|√|〤|〥"),
            new SymbolCategory("Gender / Faces", "♀|♂|☹|☺|〠|ヅ|ツ|㋡|웃|유|ü|Ü|シ|ッ|㋛|☃|〲|〴"),
            new SymbolCategory("Punctuation", "·|‑|‒|–|—|―|‘|’|‚|“|”|„|•|‥|…|‧|′|″|‵|ʻ|ˇ|ˉ|ˊ|ˋ|˙|～|¿|﹐|﹒|﹔|﹕|！|＃|＄|％|＆|＊|，|．|：|；|？|＠|、|。|〃|〝|〞|︰"),
            new SymbolCategory("Math / Numbers", "π|∞|Σ|√|∫|∬|∭|∀|∂|∃|∅|∆|∇|∈|∉|∊|∋|∏|∑|−|∓|∕|∝|∟|∠|∣|∥|∦|∧|∨|∩|∪|∴|∵|∶|∷|∽|≃|≅|≈|≌|≒|≠|≡|≢|≤|≥|≦|≧|≪|≫|≮|≯|≲|≳|≶|≷|⊂|⊃|⊄|⊅|⊆|⊇|⊊|⊋|⊕|⊖|⊗|⊘|⊙|⊠|⊥|⊿|⋚|⋛|⋯|﹢|﹣|＋|－|／|＝|÷|±|Ⅰ|Ⅱ|Ⅲ|Ⅳ|Ⅴ|Ⅵ|Ⅶ|Ⅷ|Ⅸ|Ⅹ|Ⅺ|Ⅻ|ⅰ|ⅱ|ⅲ|ⅳ|ⅴ|ⅵ|ⅶ|ⅷ|ⅸ|ⅹ|ⅺ|ⅻ|➀|➁|➂|➃|➄|➅|➆|➇|➈|➉|➊|➋|➌|➍|➎|➏|➐|➑|➒|➓|⓵|⓶|⓷|⓸|⓹|⓺|⓻|⓼|⓽|⓾|⓿|❶|❷|❸|❹|❺|❻|❼|❽|❾|❿|¹|²|³|⁴|⓪|①|②|③|④|⑤|⑥|⑦|⑧|⑨|⑩|⑪|⑫|⑬|⑭|⑮|⑯|⑰|⑱|⑲|⑳|⑴|⑵|⑶|⑷|⑸|⑹|⑺|⑻|⑼|⑽|⑾|⑿|⒀|⒁|⒂|⒃|⒄|⒅|⒆|⒇|⒈|⒉|⒊|⒋|⒌|⒍|⒎|⒏|⒐|⒑|⒒|⒓|⒔|⒕|⒖|⒗|⒘|⒙|⒚|⒛|㈠|㈡|㈢|㈣|㈤|㈥|㈦|㈧|㈨|㈩|㊀|㊁|㊂|㊃|㊄|㊅|㊆|㊇|㊈|㊉|０|１|２|３|４|５|６|７|８|９|◉|○|◌|◍|◎|●|◐|◑|◒|◓|⊗|⊙|◯|〇|〶|◦|∅|⊕|⊖|⊘|⦿|⚽|⚾|〄|θ|ð|ĩ|Ň|Ⓐ|Ⓑ|Ⓒ|Ⓓ|Ⓔ|Ⓕ|Ⓖ|Ⓗ|Ⓘ|Ⓙ|Ⓚ|Ⓛ|Ⓜ|Ⓝ|Ⓞ|Ⓟ|Ⓠ|Ⓡ|Ⓢ|Ⓣ|Ⓤ|Ⓥ|Ⓦ|Ⓧ|Ⓨ|Ⓩ|ⓐ|ⓑ|ⓒ|ⓓ|ⓔ|ⓕ|ⓖ|ⓗ|ⓘ|ⓙ|ⓚ|ⓛ|ⓜ|ⓝ|ⓞ|ⓟ|ⓠ|ⓡ|ⓢ|ⓣ|ⓤ|ⓥ|ⓦ|ⓧ|ⓨ|ⓩ|⓫|⓬|⓭|⓮|⓯|⓰|⓱|⓲|⓳|⓴|㊏|㊐|㊑|㊒|㊓|㊔|㊕|㊖|㊗|㊘|㊙|㊚|㊛|㊜|㊝|㊞|㊟|㊠|㊡|㊢|㊣|㉈|㉉|㉊|㉋|㉌|㉍|㉎|㉏|㉐|㉑|㉒|㉓|㉔|㉕|㉖|㉗|㉘|㉙|㉚|㉛|㉜|㉝|㉞|㉟|㊱|㊲|㊳|㊴|㊵|㊶|㊷|㊸|㊹|㊺|㊻|㊼|㊽|㊾|㊿"),
            new SymbolCategory("Latin", "ĩ|Ň|Ⓐ|Ⓑ|Ⓒ|Ⓓ|Ⓔ|Ⓕ|Ⓖ|Ⓗ|Ⓘ|Ⓙ|Ⓚ|Ⓛ|Ⓜ|Ⓝ|Ⓞ|Ⓟ|Ⓠ|Ⓡ|Ⓢ|Ⓣ|Ⓤ|Ⓥ|Ⓦ|Ⓧ|Ⓨ|Ⓩ|ⓐ|ⓑ|ⓒ|ⓓ|ⓔ|ⓕ|ⓖ|ⓗ|ⓘ|ⓙ|ⓚ|ⓛ|ⓜ|ⓝ|ⓞ|ⓟ|ⓠ|ⓡ|ⓢ|ⓣ|ⓤ|ⓥ|ⓦ|ⓧ|ⓨ|ⓩ"),
            new SymbolCategory("Symbol Emoji", "™|〰|🆗|🆕|🆙|🆒|🆓|🆖|🅿|Ⓜ|🆑|🆘|🆚|⚠|🅰|🅱|🆎|🅾|♻|🆔")
        };

        private static readonly HashSet<char> GamePickerSymbolChars = BuildGamePickerSymbolChars();
        private static int _gameTextBoxInstanceId;
        private static bool _gameNativeSymbolWhitelistInstalled;
        private static int _lastSubmitFrame = -1;

        internal static bool PrepareChatSubmit(ChatController chat)
        {
            RefreshGameChatAdapter();
            if (chat == null || chat != _gameChatController) return true;
            if (_lastSubmitFrame == Time.frameCount) return false;
            _lastSubmitFrame = Time.frameCount;
            CloseWindow();
            return true;
        }

        private static string _editorPlain = "";
        private static readonly List<string> _editorColors = new List<string>();

        private static void TrackEditorEdit(string next)
        {
            string old = _editorPlain;
            int prefix = 0;
            while (prefix < old.Length && prefix < next.Length && old[prefix] == next[prefix]) prefix++;
            int suffix = 0;
            while (suffix < old.Length - prefix && suffix < next.Length - prefix &&
                old[old.Length - 1 - suffix] == next[next.Length - 1 - suffix]) suffix++;
            while (_editorColors.Count < old.Length) _editorColors.Add(null);
            _editorColors.RemoveRange(prefix, old.Length - prefix - suffix);
            _editorColors.InsertRange(prefix, new string[next.Length - prefix - suffix]);
            _editorPlain = next;
        }

        internal static void ClampNativeEditorCaret(TextBoxTMP box)
        {
            if (!IsRegisteredGameTextBox(box)) return;
            int caret = ReadInt(box, 0, "caretPos");
            int safe = Mathf.Clamp(caret, 0, (box.text ?? "").Length);
            if (safe != caret) SetMember(box, "caretPos", safe);
        }

        internal static void SetGameEditorText(TextBoxTMP box, string input, string composition)
        {
            input = input ?? "";
            composition = composition ?? "";
            string old = box.text ?? "";
            int caret = Mathf.Clamp(ReadInt(box, old.Length, "caretPos"), 0, old.Length);

            int inputCaret = Mathf.Clamp(caret + input.Length - old.Length, 0, input.Length);
            int nextCaret = 0;
            bool enter = false;
            StringBuilder buffer = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '\r' || c == '\n') enter = true;
                else if (c == '\b')
                {
                    if (buffer.Length > 0)
                    {
                        int remove = 1;
                        if (buffer.Length >= 2 && char.IsLowSurrogate(buffer[buffer.Length - 1]) &&
                            char.IsHighSurrogate(buffer[buffer.Length - 2])) remove = 2;
                        buffer.Length -= remove;
                    }
                }
                else
                {
                    if (box.ForceUppercase) c = char.ToUpperInvariant(c);


                    if (c == ' ' || GamePickerSymbolChars.Contains(c) || box.IsCharAllowed(c))
                        buffer.Append(c);
                }
                if (i < inputCaret) nextCaret = buffer.Length;
            }
            int limit = box.characterLimit;
            if (limit > 0 && buffer.Length > limit)
            {
                int end = limit;
                if (end > 0 && end < buffer.Length && char.IsHighSurrogate(buffer[end - 1]) &&
                    char.IsLowSurrogate(buffer[end])) end--;
                buffer.Length = end;
            }
            string next = buffer.ToString();
            nextCaret = Mathf.Clamp(nextCaret, 0, next.Length);
            string oldComposition = GetMember(box, "compoText") as string ?? "";
            bool changed = next != old || composition != oldComposition;
            box.text = next;
            SetMember(box, "compoText", composition);
            SetMember(box, "caretPos", nextCaret);
            TrackEditorEdit(next);
            if (box.outputText != null)
            {
                box.outputText.isRightToLeftText = false;
                box.outputText.richText = false;
                box.outputText.text = (box.Hidden ? new string('*', next.Length) : next) + composition;
                box.outputText.ForceMeshUpdate(true, true);
            }
            TouchScreenKeyboard keyboard = GetMember(box, "keyboard") as TouchScreenKeyboard;
            if (keyboard != null && changed) keyboard.text = next;

            if (changed && box.OnChange != null) box.OnChange.Invoke();
            if (changed && limit > 0 && next.Length == limit && box.SendOnFullChars)
            {
                if (box.OnEnter != null) box.OnEnter.Invoke();
                box.LoseFocus();
            }
            if (enter && box.OnEnter != null) box.OnEnter.Invoke();
            ClampNativeEditorCaret(box);
            TryInvokeNoArgs(box, "SetPipePosition");
        }

        internal static void ObserveGameEditor(TextBoxTMP box)
        {
            if (!IsRegisteredGameTextBox(box)) return;


            TrackEditorEdit(box.text ?? "");
            if (box.outputText != null)
                box.outputText.isRightToLeftText = false;
        }

        private static string ApplyEditorColors(string plain)
        {
            if (!string.Equals(plain, _editorPlain, StringComparison.Ordinal) ||
                _editorColors.Count != plain.Length) return plain;
            StringBuilder result = new StringBuilder();
            int i = 0;
            while (i < plain.Length)
            {
                string hex = _editorColors[i];
                int end = i + 1;
                while (end < plain.Length && _editorColors[end] == hex) end++;
                if (hex != null) result.Append("<color=#").Append(hex).Append(">");
                result.Append(plain, i, end - i);
                if (hex != null) result.Append("</color>");
                i = end;
            }
            return result.ToString();
        }

        private static string SymbolPreviewHex(string symbol)
        {

            if (symbol == "♥" || symbol == "♡" || symbol == "😍") return "FF709D";
            if (symbol == "☁️" || symbol == "☂️") return "78CFFF";
            uint hash = 2166136261;
            foreach (char c in symbol) hash = unchecked((hash ^ c) * 16777619);
            string[] colors = { "FFD76A", "79DBFF", "A896FF", "FF93BC", "6EE7B7", "FFAD73" };
            return colors[(int)(hash % (uint)colors.Length)];
        }

        private const int FavoritesCategoryIndex = 0;
        private const string FavoritesCategoryName = "★ Favorites";
        private static readonly string FavoritesFolder = "./DATA/Symbols";
        private static readonly string FavoritesFilePath = Path.Combine(FavoritesFolder, "Favorites.txt");
        private static readonly List<string> FavoriteSymbols = new List<string>();
        private static readonly HashSet<string> FavoriteSymbolSet = new HashSet<string>();
        private static readonly List<FavoriteHitTarget> FavoriteHitTargets = new List<FavoriteHitTarget>();
        private static bool _favoritesLoaded;

        private static readonly string[] PaletteHex =
        {
            "FFFFFF", "D8D8D8", "9B9B9B", "4A4A4A",
            "FF4D4D", "FF8A3D", "FFD93D", "B8E85C",
            "56E07B", "42D7D7", "4FC3F7", "5D7CFA",
            "8A7DFF", "B66DFF", "E66DFF", "FF6FAE",
            "E74C3C", "E67E22", "F1C40F", "2ECC71",
            "1ABC9C", "3498DB", "9B59B6", "E84393"
        };

        private static readonly Regex AllowedColorSpanRegex = new Regex(
            @"<color=#(?<hex>[0-9A-Fa-f]{6})(?:[0-9A-Fa-f]{2})?>(?<body>.*?)</color>",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        private static readonly Regex AnyRichTextTagRegex = new Regex(
            @"<.*?>",
            RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex NoParseCloseRegex = new Regex(
            @"</\s*noparse\s*>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static GameObject _canvasRoot;
        private static GameObject _blocker;
        private static GameObject _window;
        private static RectTransform _categoryContent;
        private static RectTransform _symbolContent;
        private static RectTransform _symbolScrollRoot;
        private static TextMeshProUGUI _colorText;
        private static TextMeshProUGUI _copyInfoText;
        private static TextMeshProUGUI _favoritesInfoText;
        private static readonly List<GameObject> _colorControls = new List<GameObject>();
        private static GameObject _gameLauncher;
        private static Button _gameLauncherButton;
        private static SpriteRenderer _gameLauncherNativeSprite;

        private const float OmegaDefaultOffsetX = -225f;
        private const float OmegaDefaultOffsetY = 37f;
        private const float OmegaSizePixels = 124f;
        private const int OmegaSortingOffset = 0;
        private const float OmegaZOffset = 0f;
        private const string OmegaOffsetXPrefKey = "BanMod.SymbolPicker.OmegaOffsetX";
        private const string OmegaOffsetYPrefKey = "BanMod.SymbolPicker.OmegaOffsetY";
        private static float _omegaOffsetX = OmegaDefaultOffsetX;
        private static float _omegaOffsetY = OmegaDefaultOffsetY;
        private static bool _omegaPositionLoaded;
        private static bool _omegaDragging;
        private static Vector2 _omegaDragMouseStart;
        private static float _omegaDragStartOffsetX;
        private static float _omegaDragStartOffsetY;

        private static Action<string> _insertAction;
        private static TMP_InputField _focusInputAfterCopy;
        private static bool _focusGameChatAfterCopy;
        private static Func<bool> _colorAllowed;
        private static string _contextName = "SYMBOLS";
        private static string _selectedHex = "FFFFFF";
        private static int _selectedCategoryIndex;
        private static TMP_InputField _symbolSearchInput;
        private static string _symbolSearchText = "";
        private static string _coloredTextDraft = "";
        private static string _savedSymbolSearchText = "";
        private static bool _coloredTextMode;
        private static int _textSize = 100;
        private static Toggle _coloredTextToggle;
        private static readonly List<GameObject> _textStyleControls = new List<GameObject>();
        private static Button _copyTextButton;

        private static bool CanStyleText()
        {
            try { return SafeGameColorAllowed() && _colorAllowed != null && _colorAllowed(); }
            catch { return false; }
        }

        private static Toggle CreateTextCheckbox(string name, string caption, float x, float width, UnityAction<bool> changed)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(_window.transform, false);
            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, 218f);
            rt.sizeDelta = new Vector2(width, 30f);
            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0.20f, 0.23f, 0.30f, 1f);
            TextMeshProUGUI label = CreateText(root.transform, caption, 15f,
                TextAlignmentOptions.Center, Color.white, new Vector2(9f, 0f), new Vector2(width - 24f, 28f), false);
            label.raycastTarget = false;
            GameObject box = new GameObject("CheckboxBorder");
            box.transform.SetParent(root.transform, false);
            RectTransform boxRect = box.AddComponent<RectTransform>();
            boxRect.anchorMin = boxRect.anchorMax = new Vector2(0f, 0.5f);
            boxRect.anchoredPosition = new Vector2(15f, 0f);
            boxRect.sizeDelta = new Vector2(20f, 20f);
            Image boxImage = box.AddComponent<Image>();
            boxImage.color = new Color(0.10f, 0.12f, 0.16f, 1f);
            boxImage.raycastTarget = false;
            Outline border = box.AddComponent<Outline>();
            border.effectColor = new Color(0.70f, 0.75f, 0.84f, 1f);
            border.effectDistance = new Vector2(1f, -1f);
            TextMeshProUGUI mark = CreateText(box.transform, "X", 18f,
                TextAlignmentOptions.Center, Color.white, Vector2.zero, new Vector2(20f, 20f), false);
            mark.raycastTarget = false;
            Toggle toggle = root.AddComponent<Toggle>();
            toggle.targetGraphic = bg;
            toggle.graphic = mark;
            toggle.SetIsOnWithoutNotify(false);
            toggle.onValueChanged.AddListener(changed);
            return toggle;
        }

        private static void BuildTextStyleControls()
        {
            _textStyleControls.Clear();
            _coloredTextToggle = CreateTextCheckbox("ColoredText", "Colored text", -410f, 160f,
                (UnityAction<bool>)delegate (bool value)
                {
                    bool next = value && CanStyleText();
                    if (_coloredTextMode) _coloredTextDraft = _symbolSearchInput.text ?? "";
                    else _savedSymbolSearchText = _symbolSearchInput.text ?? "";
                    _coloredTextMode = next;
                    _symbolSearchText = next ? "" : _savedSymbolSearchText;
                    _symbolSearchInput.SetTextWithoutNotify(next ? _coloredTextDraft : _symbolSearchText);
                    SelectCategory(_selectedCategoryIndex);
                });
            int[] sizes = { 25, 50, 75, 100, 125 };
            for (int i = 0; i < sizes.Length; i++)
            {
                int size = sizes[i];
                Button button = CreateUiButton(_window.transform, "TextSize" + size,
                    size == _textSize ? "[" + size + "%]" : size + "%",
                    new Vector2(-255f + i * 126f, 218f), new Vector2(110f, 30f),
                    new Color(0.20f, 0.23f, 0.30f, 1f));
                _textStyleControls.Add(button.gameObject);
                button.onClick.AddListener((UnityAction)delegate
                {
                    _textSize = size;
                    foreach (GameObject control in _textStyleControls)
                    {
                        if (!control.name.StartsWith("TextSize", StringComparison.Ordinal)) continue;
                        int n = int.Parse(control.name.Substring(8));
                        control.GetComponentInChildren<TextMeshProUGUI>(true).text =
                            n == _textSize ? "[" + n + "%]" : n + "%";
                    }
                });
            }
            _copyTextButton = CreateUiButton(_window.transform, "CopyText", "Copy text",
                new Vector2(432f, 218f), new Vector2(135f, 30f), new Color(0.20f, 0.45f, 0.32f, 1f));
            _textStyleControls.Add(_copyTextButton.gameObject);
            _copyTextButton.onClick.AddListener((UnityAction)delegate
            {
                if (!_coloredTextMode || !CanStyleText() || string.IsNullOrEmpty(_coloredTextDraft)) return;
                string text = NoParseCloseRegex.Replace(_coloredTextDraft, "");
                text = "<noparse>" + text + "</noparse>";
                text = "<size=" + _textSize + "%>" + text + "</size>";
                GUIUtility.systemCopyBuffer = FormatSymbolForCurrentContext(text);
                TMP_InputField focusInput = _focusInputAfterCopy;
                bool focusGame = _focusGameChatAfterCopy;
                CloseWindow();
                if (focusGame && _gameAdapter != null) _gameAdapter.Focus();
                else if (focusInput != null) focusInput.ActivateInputField();
            });
        }


        private static float _nextGameScanTime;
        private const float GameChatScanInterval = 0.35f;
        private static ChatController _gameChatController;
        private static GameChatTextAdapter _gameAdapter;

        private static string _pendingOutgoingColoredGameChat;
        private static uint _pendingOutgoingColoredGameChatNetId;

        private static Func<bool> _gameModdedResolver = delegate { return false; };
        private static bool _hasExplicitGameModdedResolver;

        internal static bool IsOpen
        {
            get
            {
                try
                {
                    return _blocker != null &&
                           _window != null &&
                           _blocker.activeInHierarchy &&
                           _window.activeInHierarchy;
                }
                catch { return false; }
            }
        }

        internal static void SetGameModdedResolver(Func<bool> resolver)
        {
            _gameModdedResolver = resolver ?? delegate { return false; };
            _hasExplicitGameModdedResolver = resolver != null;
        }

        internal static Button AttachToTmpInput(
            Transform parent,
            string name,
            TMP_InputField input,
            Vector2 position,
            Vector2 size,
            bool alwaysColor)
        {
            if (parent == null || input == null)
                return null;

            EnableRichText(input);

            Button button = CreateUiButton(parent, name, "Ω", position, size, new Color(0.25f, 0.29f, 0.37f, 0.98f));
            if (button != null)
            {
                button.onClick.AddListener((UnityAction)delegate
                {
                    OpenForTmpInput(input, alwaysColor, alwaysColor ? "COMMUNITY / PRIVATE" : "TEXT");
                });
            }
            return button;
        }

        internal static void OpenForTmpInput(TMP_InputField input, bool alwaysColor, string contextName)
        {
            if (input == null)
                return;

            EnsurePickerUi();
            _focusInputAfterCopy = input;
            _focusGameChatAfterCopy = false;

            _contextName = string.IsNullOrWhiteSpace(contextName) ? "SYMBOLS" : contextName;
            _colorAllowed = alwaysColor ? (Func<bool>)(delegate { return true; }) : (delegate { return false; });
            _insertAction = delegate (string symbol)
            {
                GUIUtility.systemCopyBuffer = FormatSymbolForCurrentContext(symbol);
            };

            RefreshHeader();
            OpenWindow();

        }

        internal static void RuntimeTick()
        {
            try
            {
                if (IsOpen)
                {
                    HandleFavoriteRightClickHitTest();
                    if (Input.GetKeyDown(KeyCode.Escape)) CloseWindow();
                }

                if (PlayerControl.LocalPlayer == null)
                {
                    if (_gameLauncher != null)
                        _gameLauncher.SetActive(false);
                    _gameChatController = null;
                    _gameAdapter = null;
                    return;
                }

                if (Time.unscaledTime >= _nextGameScanTime)
                {
                    _nextGameScanTime = Time.unscaledTime + GameChatScanInterval;
                    RefreshGameChatAdapter();
                }

                bool showLauncher = _gameAdapter != null && _gameAdapter.IsVisible;
                if (showLauncher)
                {
                    EnsureOmegaPositionLoaded();
                    EnsureGameLauncherUi();

                    if (_gameLauncher != null)
                        _gameLauncher.SetActive(!IsOpen);

                    HandleOmegaDrag();


                    _gameAdapter.UpdateNativeLauncherSprite(!IsOpen);

                    try
                    {
                        RectTransform launcherRect = _gameLauncher != null ? _gameLauncher.GetComponent<RectTransform>() : null;
                        RectTransform canvasRect = _canvasRoot != null ? _canvasRoot.GetComponent<RectTransform>() : null;

                        _gameAdapter.UpdateLauncherPosition(launcherRect, canvasRect);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    if (_gameLauncherNativeSprite != null) _gameLauncherNativeSprite.enabled = false;
                }

                if (_gameLauncher != null && (!showLauncher || IsOpen))
                    _gameLauncher.SetActive(false);
                if (!showLauncher || IsOpen)
                    _omegaDragging = false;
            }
            catch
            {
            }
        }

        private static void EnsureOmegaPositionLoaded()
        {
            if (_omegaPositionLoaded)
                return;

            _omegaOffsetX = PlayerPrefs.HasKey(OmegaOffsetXPrefKey)
                ? PlayerPrefs.GetFloat(OmegaOffsetXPrefKey, OmegaDefaultOffsetX)
                : OmegaDefaultOffsetX;
            _omegaOffsetY = PlayerPrefs.HasKey(OmegaOffsetYPrefKey)
                ? PlayerPrefs.GetFloat(OmegaOffsetYPrefKey, OmegaDefaultOffsetY)
                : OmegaDefaultOffsetY;
            _omegaPositionLoaded = true;
        }

        private static void SaveOmegaPosition()
        {
            try
            {
                PlayerPrefs.SetFloat(OmegaOffsetXPrefKey, _omegaOffsetX);
                PlayerPrefs.SetFloat(OmegaOffsetYPrefKey, _omegaOffsetY);
                PlayerPrefs.Save();
            }
            catch { }
        }

        private static void HandleOmegaDrag()
        {
            if (_gameLauncher == null || !_gameLauncher.activeInHierarchy)
            {
                _omegaDragging = false;
                return;
            }

            EnsureOmegaPositionLoaded();

            RectTransform launcherRect = _gameLauncher.GetComponent<RectTransform>();
            if (launcherRect == null)
                return;

            bool altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            Vector2 mouse = Input.mousePosition;
            bool overLauncher = RectTransformUtility.RectangleContainsScreenPoint(launcherRect, mouse, null);

            if (!_omegaDragging && altHeld && overLauncher && Input.GetMouseButtonDown(0))
            {
                _omegaDragging = true;
                _omegaDragMouseStart = mouse;
                _omegaDragStartOffsetX = _omegaOffsetX;
                _omegaDragStartOffsetY = _omegaOffsetY;
                return;
            }

            if (_omegaDragging)
            {
                if (!Input.GetMouseButton(0) || !altHeld)
                {
                    _omegaDragging = false;
                    SaveOmegaPosition();
                    return;
                }

                Vector2 delta = mouse - _omegaDragMouseStart;
                _omegaOffsetX = _omegaDragStartOffsetX + delta.x;
                _omegaOffsetY = _omegaDragStartOffsetY + delta.y;
            }
        }

        internal static string ToSafeRichText(string raw)
        {
            raw = raw ?? "";
            if (raw.Length == 0)
                return "";

            StringBuilder sb = new StringBuilder(raw.Length + 32);
            int index = 0;
            MatchCollection matches = AllowedColorSpanRegex.Matches(raw);
            for (int i = 0; i < matches.Count; i++)
            {
                Match m = matches[i];
                if (m.Index > index)
                    AppendNoParse(sb, raw.Substring(index, m.Index - index));

                string hex = m.Groups["hex"].Value.ToUpperInvariant();
                string body = m.Groups["body"].Value;
                sb.Append("<color=#").Append(hex).Append(">");
                AppendNoParse(sb, body);
                sb.Append("</color>");
                index = m.Index + m.Length;
            }

            if (index < raw.Length)
                AppendNoParse(sb, raw.Substring(index));

            return sb.ToString();
        }

        internal static string StripPickerColorTags(string raw)
        {
            raw = raw ?? "";
            if (raw.Length == 0)
                return "";
            return AllowedColorSpanRegex.Replace(raw, delegate (Match m) { return m.Groups["body"].Value; });
        }

        internal static string TranslatePreservingPickerColors(string raw, Func<string, string> translate)
        {
            raw = raw ?? "";
            if (raw.Length == 0 || translate == null)
                return raw;

            MatchCollection matches = AllowedColorSpanRegex.Matches(raw);
            if (matches.Count == 0)
            {
                string translated = translate(raw);
                return string.IsNullOrWhiteSpace(translated) ? raw : translated;
            }

            StringBuilder sb = new StringBuilder(raw.Length + 32);
            int index = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                Match m = matches[i];
                if (m.Index > index)
                {
                    string plain = raw.Substring(index, m.Index - index);
                    string translated = translate(plain);
                    sb.Append(string.IsNullOrWhiteSpace(translated) ? plain : translated);
                }

                sb.Append(m.Value);
                index = m.Index + m.Length;
            }

            if (index < raw.Length)
            {
                string plain = raw.Substring(index);
                string translated = translate(plain);
                sb.Append(string.IsNullOrWhiteSpace(translated) ? plain : translated);
            }

            return sb.ToString();
        }

        private static void AppendNoParse(StringBuilder sb, string literal)
        {
            if (string.IsNullOrEmpty(literal))
                return;

            literal = NoParseCloseRegex.Replace(literal, "‹/noparse›");
            sb.Append("<noparse>").Append(literal).Append("</noparse>");
        }

        private static void RefreshGameChatAdapter()
        {
            try
            {
                if (_gameChatController == null)
                {
                    try
                    {
                        if (HudManager.Instance != null)
                            _gameChatController = HudManager.Instance.GetComponentInChildren<ChatController>(true);
                    }
                    catch { }

                    if (_gameChatController == null)
                        _gameChatController = UnityEngine.Object.FindObjectOfType<ChatController>();

                    _gameAdapter = null;
                }

                if (_gameChatController == null)
                {
                    _gameAdapter = null;
                    return;
                }

                if (_gameAdapter == null || !_gameAdapter.IsAlive)
                    _gameAdapter = GameChatTextAdapter.TryCreate(_gameChatController);
            }
            catch
            {
                _gameChatController = null;
                _gameAdapter = null;
            }
        }

        private static void OpenForGameChat()
        {
            RefreshGameChatAdapter();
            if (_gameAdapter == null)
                return;


            try
            {
                if (_gameChatController != null &&
                    _gameChatController.freeChatField != null &&
                    _gameChatController.freeChatField.textArea != null)
                    RegisterGameTextBox(_gameChatController.freeChatField.textArea);
            }
            catch
            {
            }

            EnsurePickerUi();
            _focusInputAfterCopy = null;
            _focusGameChatAfterCopy = true;
            _contextName = SafeGameColorAllowed() ? "CHAT · MODDED COLORS" : "CHAT · SYMBOLS";
            _colorAllowed = SafeGameColorAllowed;
            _insertAction = delegate (string symbol)
            {
                GUIUtility.systemCopyBuffer = FormatSymbolForCurrentContext(symbol);
            };
            RefreshHeader();
            OpenWindow();

        }

        private static bool SafeGameColorAllowed()
        {
            try
            {
                return _hasExplicitGameModdedResolver
                    ? _gameModdedResolver()
                    : BanModServerSelection.Mode == BanModServerMode.Modded25;
            }
            catch
            {
                return false;
            }
        }

        private static bool ContainsGamePickerSymbol(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return false;

            try
            {
                if (AllowedColorSpanRegex.IsMatch(raw))
                    return true;

                for (int i = 0; i < raw.Length; i++)
                {
                    if (GamePickerSymbolChars.Contains(raw[i]))
                        return true;
                }
            }
            catch { }

            return false;
        }

        private static string BuildDirectGameChatPayload(string raw)
        {
            raw = raw ?? "";

            if (SafeGameColorAllowed())
            {
                return ApplyEditorColors(raw);
            }

            string plain = StripPickerColorTags(raw);
            return AnyRichTextTagRegex.Replace(plain, "");
        }

        internal static bool TrySendPickerGameChatDirect(PlayerControl player, string rawText)
        {
            // Clipboard-only picker: let the game's normal chat path handle sending.
            return false;
        }

        internal static void BeginOutgoingGameChat(PlayerControl player, string rawText)
        {
            _pendingOutgoingColoredGameChat = null;
            _pendingOutgoingColoredGameChatNetId = 0;

            bool colorAllowed = false;
            try { colorAllowed = SafeGameColorAllowed(); } catch { }


            try
            {
                if (!colorAllowed)
                {
                    return;
                }

                if (player == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(rawText))
                {
                    return;
                }

                bool hasColorSpan = AllowedColorSpanRegex.IsMatch(rawText);

                if (!hasColorSpan)
                {
                    return;
                }

                string safe = KeepOnlyPickerColorTags(rawText);

                if (string.IsNullOrEmpty(safe))
                {
                    return;
                }

                _pendingOutgoingColoredGameChat = safe;
                _pendingOutgoingColoredGameChatNetId = player.NetId;

            }
            catch (Exception)
            {
                _pendingOutgoingColoredGameChat = null;
                _pendingOutgoingColoredGameChatNetId = 0;
            }
        }

        internal static void EndOutgoingGameChat()
        {

            _pendingOutgoingColoredGameChat = null;
            _pendingOutgoingColoredGameChatNetId = 0;
        }

        internal static void RestorePendingColorForLocalBubble(PlayerControl sourcePlayer, ref string text)
        {

            try
            {
                if (string.IsNullOrEmpty(_pendingOutgoingColoredGameChat))
                {
                    return;
                }

                if (!SafeGameColorAllowed())
                {
                    return;
                }

                if (sourcePlayer == null || PlayerControl.LocalPlayer == null || sourcePlayer != PlayerControl.LocalPlayer)
                {
                    return;
                }

                text = _pendingOutgoingColoredGameChat;
            }
            catch (Exception)
            {
            }
        }

        internal static void RestorePendingColorForRpcMessage(uint netId, ref string text)
        {

            try
            {
                if (string.IsNullOrEmpty(_pendingOutgoingColoredGameChat))
                {
                    return;
                }

                if (!SafeGameColorAllowed())
                {
                    return;
                }

                if (_pendingOutgoingColoredGameChatNetId != 0 &&
                    netId != _pendingOutgoingColoredGameChatNetId)
                {
                    return;
                }

                text = _pendingOutgoingColoredGameChat;
            }
            catch (Exception)
            {
            }
        }

        private static string KeepOnlyPickerColorTags(string raw)
        {
            raw = raw ?? "";
            if (raw.Length == 0)
                return "";

            MatchCollection matches = AllowedColorSpanRegex.Matches(raw);
            if (matches.Count == 0)
                return AnyRichTextTagRegex.Replace(raw, "");

            StringBuilder sb = new StringBuilder(raw.Length);
            int index = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                Match m = matches[i];
                if (m.Index > index)
                {
                    string plain = raw.Substring(index, m.Index - index);
                    sb.Append(AnyRichTextTagRegex.Replace(plain, ""));
                }

                string hex = m.Groups["hex"].Value.ToUpperInvariant();
                string body = AnyRichTextTagRegex.Replace(m.Groups["body"].Value ?? "", "");
                sb.Append("<color=#").Append(hex).Append(">").Append(body).Append("</color>");
                index = m.Index + m.Length;
            }

            if (index < raw.Length)
                sb.Append(AnyRichTextTagRegex.Replace(raw.Substring(index), ""));

            return sb.ToString();
        }

        private static Func<bool> DiscoverBanModModdedResolver()
        {
            try
            {
                Assembly assembly = typeof(BanModSymbolPicker).Assembly;
                Type[] types = assembly.GetTypes();
                string[] exactBoolNames =
                {
                    "IsFullyModdedLobby", "AllPlayersModded", "AllClientsModded",
                    "IsAllModded", "CanUseColoredGameChat"
                };

                for (int t = 0; t < types.Length; t++)
                {
                    Type type = types[t];
                    if (type == null || type.Namespace == null || !type.Namespace.StartsWith("BanMod", StringComparison.Ordinal))
                        continue;

                    for (int i = 0; i < exactBoolNames.Length; i++)
                    {
                        string name = exactBoolNames[i];
                        PropertyInfo prop = type.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (prop != null && prop.PropertyType == typeof(bool) && prop.CanRead)
                        {
                            return delegate
                            {
                                try { return (bool)prop.GetValue(null, null); } catch { return false; }
                            };
                        }

                        FieldInfo field = type.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (field != null && field.FieldType == typeof(bool))
                        {
                            return delegate
                            {
                                try { return (bool)field.GetValue(null); } catch { return false; }
                            };
                        }
                    }
                }

                for (int t = 0; t < types.Length; t++)
                {
                    Type type = types[t];
                    if (type == null || type.Namespace == null || !type.Namespace.StartsWith("BanMod", StringComparison.Ordinal))
                        continue;

                    FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        FieldInfo field = fields[i];
                        string n = field.Name ?? "";
                        bool looksLikeVersionMap =
                            n.IndexOf("PlayerVersion", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            n.IndexOf("ClientVersion", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            n.IndexOf("ModdedPlayers", StringComparison.OrdinalIgnoreCase) >= 0;
                        if (!looksLikeVersionMap)
                            continue;

                        return delegate
                        {
                            try
                            {
                                object value = field.GetValue(null);
                                System.Collections.ICollection collection = value as System.Collections.ICollection;
                                int playerCount = GameData.Instance == null ? 0 : GameData.Instance.PlayerCount;
                                return collection != null && playerCount > 0 && collection.Count >= playerCount;
                            }
                            catch { return false; }
                        };
                    }
                }
            }
            catch { }

            return delegate { return false; };
        }

        private static HashSet<char> BuildGamePickerSymbolChars()
        {
            HashSet<char> chars = new HashSet<char>();
            try
            {
                for (int i = 0; i < Categories.Length; i++)
                {
                    string symbols = Categories[i] == null ? "" : (Categories[i].Symbols ?? "");
                    for (int j = 0; j < symbols.Length; j++)
                    {
                        char c = symbols[j];
                        if (c != '|')
                            chars.Add(c);
                    }
                }
            }
            catch { }
            return chars;
        }

        private static void EnsureGameNativeSymbolWhitelist()
        {
            if (_gameNativeSymbolWhitelistInstalled)
                return;

            try
            {
                var native = TextBoxTMP.SymbolChars;
                if (native == null)
                    return;

                foreach (char c in GamePickerSymbolChars)
                {
                    if (!native.Contains(c))
                        native.Add(c);
                }

                const string RichTextSyntaxChars = "<>/=#";
                for (int i = 0; i < RichTextSyntaxChars.Length; i++)
                {
                    char c = RichTextSyntaxChars[i];
                    if (!native.Contains(c))
                        native.Add(c);
                }

                _gameNativeSymbolWhitelistInstalled = true;
            }
            catch
            {
                _gameNativeSymbolWhitelistInstalled = false;
            }
        }

        internal static void RegisterGameTextBox(TextBoxTMP textBox)
        {
            try
            {
                EnsureGameNativeSymbolWhitelist();
                int nextId = textBox == null ? 0 : textBox.GetInstanceID();
                if (nextId != _gameTextBoxInstanceId)
                {
                    _editorPlain = "";
                    _editorColors.Clear();
                }
                _gameTextBoxInstanceId = nextId;
                if (textBox != null)
                {
                    textBox.AllowSymbols = true;
                    if (textBox.outputText != null) textBox.outputText.isRightToLeftText = false;
                }
            }
            catch { _gameTextBoxInstanceId = 0; }
        }

        internal static void PasteUnicodeFromClipboard(TextBoxTMP box, string clipboard)
        {
            if (box == null || string.IsNullOrEmpty(clipboard)) return;

            RegisterGameTextBox(box);
            string original = box.text ?? "";
            string next = original + clipboard;

            int limit = box.characterLimit;
            if (limit > 0 && next.Length > limit) return;

            // Preserve template markup as editable source text.
            SetGameEditorText(box, next, "");
        }

        internal static bool IsRegisteredGameTextBox(TextBoxTMP textBox)
        {
            if (textBox == null || _gameTextBoxInstanceId == 0)
                return false;
            try { return textBox.GetInstanceID() == _gameTextBoxInstanceId; }
            catch { return false; }
        }

        internal static bool ShouldForceAllowGameCharacter(TextBoxTMP textBox, object[] args)
        {
            if (!IsRegisteredGameTextBox(textBox) || args == null)
                return false;

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    object arg = args[i];
                    if (arg is char c)
                        return GamePickerSymbolChars.Contains(c);

                    if (arg is string str && str.Length == 1)
                        return GamePickerSymbolChars.Contains(str[0]);

                    if (arg is int code && code >= char.MinValue && code <= char.MaxValue)
                        return GamePickerSymbolChars.Contains((char)code);
                }
            }
            catch { }

            return false;
        }

        private static string FormatSymbolForCurrentContext(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                return "";

            bool useColor = false;
            try { useColor = _colorAllowed != null && _colorAllowed(); } catch { }
            if (!useColor)
                return symbol;

            string hex = string.IsNullOrWhiteSpace(_selectedHex) ? "FFFFFF" : _selectedHex.Trim().ToUpperInvariant();
            return "<color=#" + hex + ">" + symbol + "</color>";
        }

        private static void InsertIntoTmpInput(TMP_InputField input, string token)
        {
            if (input == null || string.IsNullOrEmpty(token))
                return;

            try
            {
                string current = input.text ?? "";

                int fallbackCaret = 0;
                try { fallbackCaret = input.caretPosition; } catch { }

                int anchorVisual = ReadTmpInputPosition(
                    input,
                    fallbackCaret,
                    "selectionAnchorPosition");

                int focusVisual = ReadTmpInputPosition(
                    input,
                    anchorVisual,
                    "selectionFocusPosition");

                int visibleLength = GetPickerVisibleLength(current);
                anchorVisual = Mathf.Clamp(anchorVisual, 0, visibleLength);
                focusVisual = Mathf.Clamp(focusVisual, 0, visibleLength);

                int startVisual = Math.Min(anchorVisual, focusVisual);
                int endVisual = Math.Max(anchorVisual, focusVisual);
                int startRaw = PickerVisibleIndexToRawIndex(current, startVisual);
                int endRaw = PickerVisibleIndexToRawIndex(current, endVisual);

                startRaw = Mathf.Clamp(startRaw, 0, current.Length);
                endRaw = Mathf.Clamp(endRaw, startRaw, current.Length);

                string next = current.Remove(startRaw, endRaw - startRaw).Insert(startRaw, token);

                int nextVisibleLength = GetPickerVisibleLength(next);
                if (input.characterLimit > 0 && nextVisibleLength > input.characterLimit)
                    return;

                input.text = next;

                int insertedVisibleLength = GetPickerVisibleLength(token);
                int nextVisualCaret = startVisual + insertedVisibleLength;
                int nextRawCaret = startRaw + token.Length;

                WriteTmpInputPosition(input, nextRawCaret, "stringPosition");
                WriteTmpInputPosition(input, nextRawCaret, "stringSelectPosition");
                WriteTmpInputPosition(input, nextVisualCaret, "selectionAnchorPosition");
                WriteTmpInputPosition(input, nextVisualCaret, "selectionFocusPosition");
                try { input.caretPosition = nextVisualCaret; } catch { }

                try { input.ActivateInputField(); } catch { }
                try { input.ForceLabelUpdate(); } catch { }
            }
            catch
            {
            }
        }

        private static int GetPickerVisibleLength(string raw)
        {
            raw = raw ?? "";
            if (raw.Length == 0)
                return 0;

            try { return StripPickerColorTags(raw).Length; }
            catch { return raw.Length; }
        }
        private static int PickerVisibleIndexToRawIndex(string raw, int visibleIndex)
        {
            raw = raw ?? "";
            if (raw.Length == 0 || visibleIndex <= 0)
                return 0;

            int maxVisible = GetPickerVisibleLength(raw);
            visibleIndex = Mathf.Clamp(visibleIndex, 0, maxVisible);

            int rawIndex = 0;
            int visible = 0;
            MatchCollection matches = AllowedColorSpanRegex.Matches(raw);

            for (int i = 0; i < matches.Count; i++)
            {
                Match m = matches[i];
                if (m.Index < rawIndex)
                    continue;

                int plainLength = m.Index - rawIndex;
                if (visibleIndex <= visible + plainLength)
                    return rawIndex + (visibleIndex - visible);

                visible += plainLength;

                string body = m.Groups["body"].Value ?? "";
                int bodyLength = body.Length;
                if (visibleIndex < visible + bodyLength)
                {
                    int bodyOffset = visibleIndex - visible;
                    return m.Groups["body"].Index + bodyOffset;
                }

                visible += bodyLength;
                rawIndex = m.Index + m.Length;

                if (visibleIndex == visible)
                    return rawIndex;
            }

            int remaining = raw.Length - rawIndex;
            if (visibleIndex <= visible + remaining)
                return rawIndex + (visibleIndex - visible);

            return raw.Length;
        }

        private static int ReadTmpInputPosition(TMP_InputField input, int fallback, params string[] propertyNames)
        {
            if (input == null || propertyNames == null)
                return fallback;

            Type type = input.GetType();
            for (int i = 0; i < propertyNames.Length; i++)
            {
                string name = propertyNames[i];
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                try
                {
                    PropertyInfo property = type.GetProperty(
                        name,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (property == null || !property.CanRead)
                        continue;

                    object value = property.GetValue(input, null);
                    if (value is int)
                        return (int)value;
                }
                catch { }
            }

            return fallback;
        }

        private static void WriteTmpInputPosition(TMP_InputField input, int value, params string[] propertyNames)
        {
            if (input == null || propertyNames == null)
                return;

            Type type = input.GetType();
            for (int i = 0; i < propertyNames.Length; i++)
            {
                string name = propertyNames[i];
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                try
                {
                    PropertyInfo property = type.GetProperty(
                        name,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (property == null || !property.CanWrite || property.PropertyType != typeof(int))
                        continue;

                    property.SetValue(input, value, null);
                    return;
                }
                catch { }
            }
        }

        private static void EnableRichText(TMP_InputField input)
        {
            if (input == null)
                return;
            try { input.richText = true; } catch { }
            try { if (input.textComponent != null) input.textComponent.richText = true; } catch { }
        }

        private static void EnsureCanvasRoot()
        {
            if (_canvasRoot != null)
                return;

            _canvasRoot = new GameObject("BanMod_SymbolPickerCanvas");
            UnityEngine.Object.DontDestroyOnLoad(_canvasRoot);

            Canvas canvas = _canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;

            canvas.sortingOrder = 32000;

            CanvasScaler scaler = _canvasRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            _canvasRoot.AddComponent<GraphicRaycaster>();
        }

        private static void EnsureGameLauncherUi()
        {
            EnsureCanvasRoot();
            if (_gameLauncher == null)
                BuildGameLauncher();
        }

        private static void EnsurePickerUi()
        {
            EnsureCanvasRoot();

            if (_gameLauncher == null)
                BuildGameLauncher();

            if (_blocker == null || _window == null)
                BuildPickerWindow();
        }


        private static void BuildGameLauncher()
        {
            _gameLauncher = new GameObject("GameChatSymbolLauncher");
            _gameLauncher.transform.SetParent(_canvasRoot.transform, false);

            RectTransform rt = _gameLauncher.AddComponent<RectTransform>();

            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-520f, 155f);

            rt.sizeDelta = new Vector2(100f, 100f);


            Image image = _gameLauncher.AddComponent<Image>();
            image.sprite = Utils.LoadSprite("BanMod.Resources.image.icon.png", 100f);

            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;
            image.preserveAspect = true;

            _gameLauncherButton = _gameLauncher.AddComponent<Button>();
            _gameLauncherButton.targetGraphic = image;
            _gameLauncherButton.transition = Selectable.Transition.None;

            _gameLauncherButton.onClick.AddListener((UnityAction)delegate
            {
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                    return;
                OpenForGameChat();
            });

            _gameLauncher.SetActive(false);
        }

        private static void BuildPickerWindow()
        {
            _blocker = new GameObject("PickerBlocker");
            _blocker.transform.SetParent(_canvasRoot.transform, false);
            RectTransform blockerRt = _blocker.AddComponent<RectTransform>();
            Stretch(blockerRt);
            Image blockerImage = _blocker.AddComponent<Image>();
            blockerImage.color = new Color(0f, 0f, 0f, 0.64f);
            blockerImage.raycastTarget = true;
            Button blockerButton = _blocker.AddComponent<Button>();
            blockerButton.transition = Selectable.Transition.None;
            blockerButton.targetGraphic = blockerImage;
            blockerButton.onClick.AddListener((UnityAction)CloseWindow);

            _window = new GameObject("PickerWindow");
            _window.transform.SetParent(_blocker.transform, false);
            RectTransform wrt = _window.AddComponent<RectTransform>();
            wrt.anchorMin = wrt.anchorMax = new Vector2(0.5f, 0.5f);
            wrt.pivot = new Vector2(0.5f, 0.5f);
            wrt.sizeDelta = new Vector2(1080f, 720f);
            wrt.anchoredPosition = Vector2.zero;
            Image wbg = _window.AddComponent<Image>();
            wbg.sprite = RoundedSprite();
            wbg.type = Image.Type.Sliced;
            wbg.color = new Color(0.105f, 0.12f, 0.16f, 0.995f);
            wbg.raycastTarget = true;
            Shadow shadow = _window.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(0f, -8f);
            Outline edge = _window.AddComponent<Outline>();
            edge.effectColor = new Color(0.055f, 0.065f, 0.085f, 0.95f);
            edge.effectDistance = new Vector2(1f, -1f);

            CreateUiButton(_window.transform, "Close", "×", new Vector2(507f, 326f), new Vector2(52f, 48f), new Color(0.10f, 0.12f, 0.16f, 1f))
                .onClick.AddListener((UnityAction)CloseWindow);

            _colorControls.Clear();
            TextMeshProUGUI colorLabel = CreateText(_window.transform, "COLOR", 16f, TextAlignmentOptions.Left, new Color(0.80f, 0.85f, 0.94f, 1f), new Vector2(-455f, 261f), new Vector2(100f, 30f), false);
            _colorControls.Add(colorLabel.gameObject);
            _colorText = CreateText(_window.transform, "#FFFFFF", 15f, TextAlignmentOptions.Left, Color.white, new Vector2(-340f, 261f), new Vector2(105f, 30f), false);
            _colorControls.Add(_colorText.gameObject);
            BuildPalette();
            BuildSymbolSearchInput();

            _copyInfoText = CreateText(_window.transform,
                "Click a symbol to copy it. Press Ctrl+V in chat to paste.",
                18f, TextAlignmentOptions.Left,
                new Color(0.86f, 0.90f, 0.97f, 1f),
                new Vector2(-200f, 145f), new Vector2(505f, 30f), false);
            _favoritesInfoText = CreateText(_window.transform,
                "Right-click a symbol to add or remove it from Favorites.",
                18f, TextAlignmentOptions.Left,
                new Color(0.86f, 0.90f, 0.97f, 1f),
                new Vector2(270f, 145), new Vector2(500f, 30f), false);
            CreateText(_window.transform,
                "Hold Alt and drag the button to move it",
                16f, TextAlignmentOptions.Center,
                new Color(0.86f, 0.90f, 0.97f, 1f),
                new Vector2(0f, -340f), new Vector2(920f, 26f), false);

            EnsureFavoritesLoaded();
            BuildCategoryScroll();
            BuildSymbolScroll();
            SelectCategory(1);

            _window.SetActive(false);
            _blocker.SetActive(false);
        }

        private static void BuildPalette()
        {
            float startX = -250f;
            float y = 261f;
            float step = 31f;
            for (int i = 0; i < PaletteHex.Length; i++)
            {
                int captured = i;
                Color color;
                if (!ColorUtility.TryParseHtmlString("#" + PaletteHex[i], out color))
                    color = Color.white;

                Button b = CreateUiButton(_window.transform, "Color_" + PaletteHex[i], "", new Vector2(startX + i * step, y), new Vector2(26f, 26f), color);
                _colorControls.Add(b.gameObject);
                b.onClick.AddListener((UnityAction)delegate
                {
                    _selectedHex = PaletteHex[captured];
                    RefreshHeader();
                });
            }
        }

        private static void BuildSymbolSearchInput()
        {
            GameObject root = new GameObject("SymbolSearch");
            root.transform.SetParent(_window.transform, false);
            RectTransform rt = root.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-7f, 315f);
            rt.sizeDelta = new Vector2(940f, 43f);
            Image bg = root.AddComponent<Image>();
            bg.sprite = RoundedSprite();
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.20f, 0.23f, 0.30f, 1f);
            bg.raycastTarget = true;
            _symbolSearchInput = root.AddComponent<TMP_InputField>();
            GameObject textGo = new GameObject("SearchText");
            textGo.transform.SetParent(root.transform, false);
            RectTransform tr = textGo.AddComponent<RectTransform>();
            Stretch(tr, new Vector4(9f, 5f, 9f, 5f));
            TextMeshProUGUI label = textGo.AddComponent<TextMeshProUGUI>();
            label.fontSize = 17f;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.enableWordWrapping = false;
            _symbolSearchInput.textViewport = rt;
            _symbolSearchInput.textComponent = label;
            _symbolSearchInput.lineType = TMP_InputField.LineType.SingleLine;
            _symbolSearchInput.onValueChanged.AddListener((UnityAction<string>)delegate (string value)
            {
                if (_coloredTextMode && CanStyleText())
                {
                    _coloredTextDraft = value ?? "";
                    return;
                }
                _symbolSearchText = value ?? "";
                SelectCategory(_selectedCategoryIndex);
            });
            GameObject placeholderGo = new GameObject("SearchPlaceholder");
            placeholderGo.transform.SetParent(root.transform, false);
            RectTransform placeholderRect = placeholderGo.AddComponent<RectTransform>();
            Stretch(placeholderRect, new Vector4(9f, 3f, 9f, 3f));
            TextMeshProUGUI placeholder = placeholderGo.AddComponent<TextMeshProUGUI>();
            placeholder.text = "Search by name or category...";
            placeholder.fontSize = 16f;
            placeholder.color = new Color(0.70f, 0.75f, 0.84f, 0.9f);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.enableWordWrapping = true;
            placeholder.raycastTarget = false;
            _symbolSearchInput.placeholder = placeholder;
        }

        private static void BuildCategoryScroll()
        {

            GameObject holder = new GameObject("SymbolTabs");
            holder.transform.SetParent(_window.transform, false);
            _categoryContent = holder.AddComponent<RectTransform>();
            _categoryContent.anchorMin = _categoryContent.anchorMax = new Vector2(0.5f, 0.5f);
            _categoryContent.anchoredPosition = new Vector2(0f, 94f);
            _categoryContent.sizeDelta = new Vector2(950f, 45f);
            for (int i = 0; i < 2; i++)
            {
                int captured = i;
                string caption = i == 0 ? "Favorites ★" : "All Symbols";
                Button tab = CreateUiButton(_categoryContent, "Tab_" + i, caption,
                    new Vector2(i == 0 ? 120f : -120f, 0f), new Vector2(220f, 40f),
                    new Color(0.20f, 0.23f, 0.30f, 1f));
                tab.onClick.AddListener((UnityAction)delegate { SelectCategory(captured); });
            }
        }

        private static void BuildSymbolScroll()
        {
            GameObject root = new GameObject("SymbolScroll");
            root.transform.SetParent(_window.transform, false);
            RectTransform rr = root.AddComponent<RectTransform>();
            _symbolScrollRoot = rr;
            rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
            rr.pivot = new Vector2(0.5f, 0.5f);
            rr.anchoredPosition = new Vector2(0f, -132f);
            rr.sizeDelta = new Vector2(950f, 400f);
            Image bg = root.AddComponent<Image>();
            bg.sprite = RoundedSprite();
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.14f, 0.16f, 0.21f, 1f);
            bg.raycastTarget = true;
            ScrollRect scroll = root.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 42f;

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(root.transform, false);
            RectTransform vrt = viewport.AddComponent<RectTransform>();
            Stretch(vrt, new Vector4(10f, 10f, 10f, 10f));
            Image vpImage = viewport.AddComponent<Image>();
            vpImage.color = new Color(0f, 0f, 0f, 0.001f);
            vpImage.raycastTarget = true;
            viewport.AddComponent<RectMask2D>();

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            _symbolContent = content.AddComponent<RectTransform>();
            _symbolContent.anchorMin = new Vector2(0f, 1f);
            _symbolContent.anchorMax = new Vector2(1f, 1f);
            _symbolContent.pivot = new Vector2(0.5f, 1f);
            _symbolContent.anchoredPosition = Vector2.zero;
            _symbolContent.sizeDelta = new Vector2(0f, 540f);

            scroll.viewport = vrt;
            scroll.content = _symbolContent;
        }

        private static void SelectCategory(int index)
        {
            if (_symbolContent == null)
                return;

            EnsureFavoritesLoaded();

            int categoryCount = GetCategoryCount();
            if (categoryCount <= 0)
                return;

            _selectedCategoryIndex = Mathf.Clamp(index, 0, categoryCount - 1);
            if (_categoryContent != null)
            {
                for (int c = 0; c < _categoryContent.childCount; c++)
                {
                    Image image = _categoryContent.GetChild(c).GetComponent<Image>();
                    if (image != null) image.color = c == _selectedCategoryIndex
                        ? new Color(0.31f, 0.44f, 0.64f, 1f)
                        : new Color(0.38f, 0.42f, 0.51f, 1f);
                }
            }
            FavoriteHitTargets.Clear();
            ClearChildren(_symbolContent);

            List<string> symbols = GetCategorySymbols(_selectedCategoryIndex);
            symbols = FilterSymbolsBySearch(symbols);
            const int columns = 11;
            const float cellW = 86f;
            const float cellH = 66f;
            const float buttonW = 74f;
            const float buttonH = 54f;
            float startX = -430f;
            float startY = -38f;

            if (_selectedCategoryIndex == FavoritesCategoryIndex && symbols.Count == 0)
            {
                TextMeshProUGUI empty = CreateText(
                    _symbolContent,
                    "No favorites yet.\nRight-click a symbol to add it.",
                    19f,
                    TextAlignmentOptions.Center,
                    new Color(0.72f, 0.76f, 0.86f, 1f),
                    new Vector2(0f, -115f),
                    new Vector2(720f, 80f),
                    false);
                empty.enableWordWrapping = true;
            }

            for (int i = 0; i < symbols.Count; i++)
            {
                string captured = symbols[i];
                string preview = "<color=#" + SymbolPreviewHex(captured) + ">" + captured + "</color>";
                int row = i / columns;
                int col = i % columns;
                Vector2 pos = new Vector2(startX + col * cellW, startY - row * cellH);
                Button b = CreateUiButton(_symbolContent, "Symbol_" + i, preview, pos, new Vector2(buttonW, buttonH), new Color(0.20f, 0.23f, 0.30f, 1f));
                RectTransform br = b.GetComponent<RectTransform>();
                if (br != null)
                {
                    br.anchorMin = br.anchorMax = new Vector2(0.5f, 1f);
                    br.pivot = new Vector2(0.5f, 0.5f);
                    br.anchoredPosition = pos;
                }

                b.onClick.AddListener((UnityAction)delegate
                {
                    try
                    {

                        if (_insertAction == null) return;
                        _insertAction(captured);
                        TMP_InputField focusInput = _focusInputAfterCopy;
                        bool focusGameChat = _focusGameChatAfterCopy;
                        CloseWindow();

                        if (focusGameChat)
                        {
                            try { if (_gameAdapter != null) _gameAdapter.Focus(); } catch { }
                        }
                        else if (focusInput != null)
                        {
                            try { focusInput.ActivateInputField(); } catch { }
                        }
                    }
                    catch
                    {
                    }
                });

                FavoriteHitTargets.Add(new FavoriteHitTarget
                {
                    Rect = br,
                    Symbol = captured
                });

                TextMeshProUGUI t = b.GetComponentInChildren<TextMeshProUGUI>(true);
                if (t != null)
                {
                    t.fontSize = 24f;
                    t.enableAutoSizing = true;
                    t.fontSizeMin = 13f;
                    t.fontSizeMax = 26f;
                }
            }

            int rows = (symbols.Count + columns - 1) / columns;
            _symbolContent.sizeDelta = new Vector2(0f, Mathf.Max(475f, 24f + rows * cellH));
            RefreshHeader();
        }

        private static int GetCategoryCount()
        {
            return 2;
        }

        private static string GetCategoryName(int index)
        {
            if (index == FavoritesCategoryIndex)
                return FavoritesCategoryName;

            if (index == 1) return "All Symbols";
            int realIndex = index - 2;
            if (realIndex < 0 || realIndex >= Categories.Length)
                return "Symbols";

            return Categories[realIndex].Name;
        }

        private static List<string> GetCategorySymbols(int index)
        {
            if (index == FavoritesCategoryIndex)
                return new List<string>(FavoriteSymbols);

            if (index == 1)
            {
                List<string> all = new List<string>();
                HashSet<string> seen = new HashSet<string>();
                for (int c = 0; c < Categories.Length; c++)
                    foreach (string symbol in SplitUnique(Categories[c].Symbols))
                        if (seen.Add(symbol)) all.Add(symbol);
                return all;
            }
            int realIndex = index - 2;
            if (realIndex < 0 || realIndex >= Categories.Length)
                return new List<string>();

            return SplitUnique(Categories[realIndex].Symbols);
        }

        private static List<string> FilterSymbolsBySearch(List<string> original)
        {
            string query = (_symbolSearchText ?? "").Trim();
            if (query.Length == 0) return original;
            string[] terms = query.ToLowerInvariant().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            HashSet<string> matching = new HashSet<string>();
            for (int c = 0; c < Categories.Length; c++)
            {
                string category = Categories[c].Name.ToLowerInvariant();
                string aliases = CategorySearchAliases(category);
                bool match = true;
                for (int t = 0; t < terms.Length; t++)
                    if (!category.Contains(terms[t]) && !aliases.Contains(terms[t])) { match = false; break; }
                if (!match) continue;
                foreach (string symbol in SplitUnique(Categories[c].Symbols)) matching.Add(symbol);
            }
            List<string> filtered = new List<string>();
            foreach (string symbol in original)
                if (symbol.Contains(query) || matching.Contains(symbol)) filtered.Add(symbol);
            return filtered;
        }

        private static string CategorySearchAliases(string name)
        {
            if (name.Contains("star")) return "star stars";
            if (name.Contains("arrow")) return "arrow arrows direction";
            if (name.Contains("heart")) return "heart hearts love";
            if (name.Contains("currency")) return "currency coin euro dollar money";
            if (name.Contains("math")) return "math mathematics numbers number";
            if (name.Contains("music")) return "music musical note notes";
            if (name.Contains("shape")) return "shape shapes circle triangle square";
            if (name.Contains("emoji")) return "faces smileys emoji";
            if (name.Contains("check")) return "check tick confirm";
            if (name.Contains("card")) return "cards poker";
            if (name.Contains("bracket")) return "brackets parentheses";
            if (name.Contains("gender")) return "gender face faces";
            if (name.Contains("degree")) return "degrees temperature";
            if (name.Contains("punctuation")) return "punctuation";
            if (name.Contains("copyright")) return "copyright trademark rights";
            return name;
        }

        private static void EnsureFavoritesLoaded()
        {
            if (_favoritesLoaded)
                return;

            _favoritesLoaded = true;
            FavoriteSymbols.Clear();
            FavoriteSymbolSet.Clear();

            try
            {
                Directory.CreateDirectory(FavoritesFolder);
                if (!File.Exists(FavoritesFilePath))
                    return;

                string[] lines = File.ReadAllLines(FavoritesFilePath, Encoding.UTF8);
                for (int i = 0; i < lines.Length; i++)
                {
                    string symbol = (lines[i] ?? "").Trim();
                    if (string.IsNullOrEmpty(symbol) || !FavoriteSymbolSet.Add(symbol))
                        continue;

                    FavoriteSymbols.Add(symbol);
                }
            }
            catch
            {
            }
        }

        private static void SaveFavorites()
        {
            try
            {
                Directory.CreateDirectory(FavoritesFolder);
                File.WriteAllText(
                    FavoritesFilePath,
                    string.Join("\n", FavoriteSymbols.ToArray()),
                    new UTF8Encoding(false));
            }
            catch
            {
            }
        }

        private static void ToggleFavorite(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                return;

            EnsureFavoritesLoaded();

            if (FavoriteSymbolSet.Remove(symbol))
            {
                FavoriteSymbols.Remove(symbol);
            }
            else
            {
                FavoriteSymbolSet.Add(symbol);
                FavoriteSymbols.Add(symbol);
            }

            SaveFavorites();

            if (_selectedCategoryIndex == FavoritesCategoryIndex)
                SelectCategory(FavoritesCategoryIndex);
            else
                RefreshHeader();
        }

        private static void HandleFavoriteRightClickHitTest()
        {
            if (!Input.GetMouseButtonDown(1) || FavoriteHitTargets.Count == 0)
                return;

            Vector2 mouse = Input.mousePosition;
            for (int i = FavoriteHitTargets.Count - 1; i >= 0; i--)
            {
                FavoriteHitTarget hit = FavoriteHitTargets[i];
                if (hit == null || hit.Rect == null || string.IsNullOrEmpty(hit.Symbol))
                    continue;

                try
                {
                    if (!hit.Rect.gameObject.activeInHierarchy)
                        continue;

                    if (!RectTransformUtility.RectangleContainsScreenPoint(hit.Rect, mouse, null))
                        continue;

                    ToggleFavorite(hit.Symbol);
                    return;
                }
                catch { }
            }
        }

        private static List<string> SplitUnique(string packed)
        {
            List<string> result = new List<string>();
            HashSet<string> seen = new HashSet<string>();
            string[] parts = (packed ?? "").Split('|');
            for (int i = 0; i < parts.Length; i++)
            {
                string s = parts[i];
                if (string.IsNullOrEmpty(s) || !seen.Add(s))
                    continue;
                result.Add(s);
            }
            return result;
        }

        private static void RefreshHeader()
        {
            try
            {
                bool colored = _colorAllowed != null && _colorAllowed();
                bool showPalette = colored && SafeGameColorAllowed();
                bool canStyle = CanStyleText();
                if (canStyle && _coloredTextToggle == null) BuildTextStyleControls();
                if (!canStyle && _coloredTextMode)
                {
                    _coloredTextDraft = _symbolSearchInput.text ?? "";
                    _coloredTextMode = false;
                    _symbolSearchText = _savedSymbolSearchText;
                    _symbolSearchInput.SetTextWithoutNotify(_symbolSearchText);
                }
                if (_coloredTextToggle != null)
                {
                    _coloredTextToggle.gameObject.SetActive(canStyle);
                    _coloredTextToggle.SetIsOnWithoutNotify(_coloredTextMode);
                }
                foreach (GameObject control in _textStyleControls)
                    if (control != null) control.SetActive(canStyle && _coloredTextMode);
                if (_symbolSearchInput != null && _symbolSearchInput.placeholder != null)
                {
                    TextMeshProUGUI hint = _symbolSearchInput.placeholder as TextMeshProUGUI;
                    if (hint != null) hint.text = _coloredTextMode ? "Type your text here to copy..." : "Search by name or category...";
                }
                if (_copyInfoText != null) _copyInfoText.text = _coloredTextMode
                    ? "Choose a color and size, then click Copy text."
                    : "Click a symbol to copy it. Press Ctrl+V in chat to paste.";
                float infoY = canStyle ? 173f : (showPalette ? 207f : 255f);
                if (_copyInfoText != null)
                {
                    RectTransform rect = _copyInfoText.GetComponent<RectTransform>();
                    if (rect != null) rect.anchoredPosition = new Vector2(-200f, infoY);
                }
                if (_favoritesInfoText != null)
                {
                    RectTransform rect = _favoritesInfoText.GetComponent<RectTransform>();
                    if (rect != null) rect.anchoredPosition = new Vector2(270f, infoY);
                }
                float tabsY = infoY - 45f;
                if (_categoryContent != null)
                    _categoryContent.anchoredPosition = new Vector2(0f, tabsY);
                if (_symbolScrollRoot != null)
                {
                    float topY = tabsY - 26f;
                    const float bottomY = -332f;
                    _symbolScrollRoot.anchoredPosition = new Vector2(0f, (topY + bottomY) * 0.5f);
                    _symbolScrollRoot.sizeDelta = new Vector2(950f, topY - bottomY);
                }
                for (int i = 0; i < _colorControls.Count; i++)
                {
                    GameObject item = _colorControls[i];
                    if (item != null && item.activeSelf != showPalette)
                        item.SetActive(showPalette);
                }
                if (_colorText != null && showPalette)
                {
                    _colorText.text = "#" + _selectedHex;
                    Color parsed;
                    _colorText.color = ColorUtility.TryParseHtmlString("#" + _selectedHex, out parsed) ? parsed : Color.white;
                }
            }
            catch { }
        }

        private static void OpenWindow()
        {
            if (_blocker != null) _blocker.SetActive(true);
            if (_window != null) _window.SetActive(true);
            if (_gameLauncher != null) _gameLauncher.SetActive(false);
        }

        private static void CloseWindow()
        {
            if (_window != null) _window.SetActive(false);
            if (_blocker != null) _blocker.SetActive(false);
            _insertAction = null;
            _focusInputAfterCopy = null;
            _focusGameChatAfterCopy = false;
            _colorAllowed = null;
            _symbolSearchText = "";
            _coloredTextDraft = "";
            _savedSymbolSearchText = "";
            _coloredTextMode = false;
            if (_symbolSearchInput != null) _symbolSearchInput.SetTextWithoutNotify("");
            if (_symbolContent != null) SelectCategory(1);
        }

        private static Sprite _roundedSprite;
        private static Sprite RoundedSprite()
        {
            if (_roundedSprite != null) return _roundedSprite;
            const int side = 64;
            const int radius = 14;
            Texture2D texture = new Texture2D(side, side, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < side; y++)
                for (int x = 0; x < side; x++)
                {
                    float cx = Mathf.Clamp(x, radius, side - radius - 1);
                    float cy = Mathf.Clamp(y, radius, side - radius - 1);
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(radius - dist + 0.5f)));
                }
            texture.Apply();
            _roundedSprite = Sprite.Create(texture, new Rect(0f, 0f, side, side),
                new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect,
                new Vector4(16f, 16f, 16f, 16f));
            return _roundedSprite;
        }

        private static Button CreateUiButton(Transform parent, string name, string text, Vector2 position, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            Image img = go.AddComponent<Image>();
            img.sprite = RoundedSprite();
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = true;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = img;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = button.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.14f, 1.14f, 1.14f, 1f);
            cb.pressedColor = new Color(0.52f, 0.75f, 1f, 1f);
            cb.fadeDuration = 0f;
            cb.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.65f);
            button.colors = cb;
            if (!string.IsNullOrEmpty(text))
                CreateText(go.transform, text, 20f, TextAlignmentOptions.Center, Color.white, Vector2.zero, Vector2.zero, true);
            return button;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string text, float fontSize, TextAlignmentOptions align, Color color, Vector2 position, Vector2 size, bool stretch)
        {
            GameObject go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            if (stretch)
            {
                Stretch(rt, new Vector4(3f, 3f, 3f, 3f));
            }
            else
            {
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = position;
                rt.sizeDelta = size;
            }

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text ?? "";
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
            tmp.richText = true;
            try { if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset; } catch { }
            return tmp;
        }

        private static void Stretch(RectTransform rt)
        {
            Stretch(rt, Vector4.zero);
        }

        private static void Stretch(RectTransform rt, Vector4 pad)
        {
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(pad.x, pad.y);
            rt.offsetMax = new Vector2(-pad.z, -pad.w);
        }

        private static void ClearChildren(RectTransform root)
        {
            if (root == null) return;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (child != null)
                    UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        private sealed class GameChatTextAdapter
        {
            private readonly ChatController _chat;
            private readonly FreeChatInputField _freeChat;
            private readonly TextBoxTMP _textBox;
            private readonly ChatInputFieldButton _submitButton;

            private GameChatTextAdapter(
                ChatController chat,
                FreeChatInputField freeChat,
                TextBoxTMP textBox,
                ChatInputFieldButton submitButton)
            {
                _chat = chat;
                _freeChat = freeChat;
                _textBox = textBox;
                _submitButton = submitButton;
            }

            internal bool IsAlive
            {
                get
                {
                    try { return _chat != null && _freeChat != null && _textBox != null; }
                    catch { return false; }
                }
            }

            internal bool IsVisible
            {
                get
                {
                    try
                    {
                        if (!IsAlive || !_chat.gameObject.activeInHierarchy || !_freeChat.gameObject.activeInHierarchy)
                            return false;

                        try
                        {
                            if (_chat.IsOpenOrOpening)
                                return true;
                        }
                        catch { }

                        if (IsSubmitButtonActuallyVisible())
                            return true;

                        SpriteRenderer bg = null;
                        try { bg = _freeChat.Background; } catch { }
                        if (bg != null && bg.gameObject.activeInHierarchy && IsBoundsInsideScreen(bg.bounds, 140f))
                            return true;

                        try
                        {
                            if (_textBox.outputText != null &&
                                _textBox.outputText.gameObject.activeInHierarchy &&
                                _textBox.outputText.enabled)
                            {
                                Renderer renderer = _textBox.outputText.GetComponent<Renderer>();
                                if (renderer != null && renderer.enabled && IsBoundsInsideScreen(renderer.bounds, 140f))
                                    return true;
                            }
                        }
                        catch { }

                        return false;
                    }
                    catch { return false; }
                }
            }

            private bool IsSubmitButtonActuallyVisible()
            {
                if (_submitButton == null || !_submitButton.gameObject.activeInHierarchy)
                    return false;

                try
                {
                    Renderer[] renderers = _submitButton.GetComponentsInChildren<Renderer>(true);
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        Renderer renderer = renderers[i];
                        if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                            continue;
                        if (IsBoundsInsideScreen(renderer.bounds, 140f))
                            return true;
                    }
                }
                catch { }

                return false;
            }

            private static bool IsBoundsInsideScreen(Bounds bounds, float margin)
            {
                try
                {
                    Camera camera = GetGameUiCamera();
                    if (camera == null)
                        return true;

                    Vector3 screen = RectTransformUtility.WorldToScreenPoint(camera, bounds.center);
                    return screen.x >= -margin && screen.x <= Screen.width + margin &&
                           screen.y >= -margin && screen.y <= Screen.height + margin;
                }
                catch { return false; }
            }

            internal static GameChatTextAdapter TryCreate(ChatController chat)
            {
                if (chat == null)
                    return null;

                try
                {
                    FreeChatInputField freeChat = null;
                    TextBoxTMP textBox = null;
                    ChatInputFieldButton submitButton = null;

                    try { freeChat = chat.GetComponentInChildren<FreeChatInputField>(true); } catch { }

                    if (freeChat == null)
                    {
                        Component[] components = chat.GetComponentsInChildren<Component>(true);
                        for (int i = 0; i < components.Length; i++)
                        {
                            Component c = components[i];
                            if (c == null)
                                continue;
                            if (freeChat == null)
                                freeChat = c as FreeChatInputField;
                        }
                    }

                    if (freeChat == null)
                        return null;

                    try { textBox = freeChat.GetComponentInChildren<TextBoxTMP>(true); } catch { }
                    try { submitButton = freeChat.GetComponentInChildren<ChatInputFieldButton>(true); } catch { }

                    Component[] freeComponents = freeChat.GetComponentsInChildren<Component>(true);
                    for (int i = 0; i < freeComponents.Length; i++)
                    {
                        Component c = freeComponents[i];
                        if (c == null)
                            continue;

                        if (textBox == null)
                            textBox = c as TextBoxTMP;

                        if (submitButton == null)
                            submitButton = c as ChatInputFieldButton;
                    }

                    if (submitButton == null)
                    {
                        try
                        {
                            ChatInputFieldButton[] allSubmitButtons = chat.GetComponentsInChildren<ChatInputFieldButton>(true);
                            if (allSubmitButtons != null && allSubmitButtons.Length > 0)
                                submitButton = allSubmitButtons[0];
                        }
                        catch { }
                    }

                    if (textBox == null)
                        return null;


                    return new GameChatTextAdapter(chat, freeChat, textBox, submitButton);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            internal void Insert(string token)
            {

                if (string.IsNullOrEmpty(token) || !IsAlive)
                {
                    return;
                }

                try
                {
                    bool clearOnFocus = _textBox.ClearOnFocus;
                    try
                    {
                        _textBox.ClearOnFocus = false;
                        _textBox.GiveFocus();
                    }
                    finally { _textBox.ClearOnFocus = clearOnFocus; }
                    string current = _textBox.text ?? "";
                    TrackEditorEdit(current);
                    int caret = Mathf.Clamp(ReadInt(_textBox, current.Length, "caretPos"), 0, current.Length);
                    if (caret > 0 && caret < current.Length && char.IsHighSurrogate(current[caret - 1]) &&
                        char.IsLowSurrogate(current[caret])) caret++;
                    string plainToken = StripPickerColorTags(token);
                    string next = current.Insert(caret, plainToken);
                    int limit = _textBox.characterLimit;
                    if (limit > 0 && next.Length > limit) return;
                    EnsureGameNativeSymbolWhitelist();
                    _textBox.AllowSymbols = true;
                    SetMember(_textBox, "caretPos", caret);
                    _textBox.SetText(next, "");
                    ObserveGameEditor(_textBox);

                    if (string.Equals(_textBox.text, next, StringComparison.Ordinal))
                    {
                        for (int n = caret; n < caret + plainToken.Length; n++)
                            _editorColors[n] = SafeGameColorAllowed() ? _selectedHex : null;
                    }
                    else
                    {
                    }


                    try { _freeChat.UpdateCharCount(); } catch { }


                }
                catch
                {
                }
            }

            internal void UpdateNativeLauncherSprite(bool visible)
            {
                if (_submitButton == null) return;
                SpriteRenderer send = _submitButton.GetComponentInChildren<SpriteRenderer>(true);
                if (send == null) return;
                if (_gameLauncherNativeSprite == null)
                {
                    Sprite omega = null;

                    if (BanMod.DarkTheme.Value)
                    {
                        omega = Utils.LoadSprite("BanMod.Resources.image.icondark.png", 100f);
                    }
                    else
                    {
                        omega = Utils.LoadSprite("BanMod.Resources.image.icon.png", 100f);
                    }
                    if (omega == null)
                    {
                        return;
                    }
                    GameObject go = new GameObject("BanMod_Omega_NativeSendSprite");
                    go.layer = send.gameObject.layer;
                    go.transform.SetParent(send.transform.parent, false);
                    _gameLauncherNativeSprite = go.AddComponent<SpriteRenderer>();
                    _gameLauncherNativeSprite.sprite = omega;
                    _gameLauncherNativeSprite.color = new Color(1f, 1f, 1f, 0.25f);
                }

                SpriteRenderer icon = _gameLauncherNativeSprite;
                if (icon == null) return;
                icon.gameObject.layer = send.gameObject.layer;
                icon.sortingLayerID = send.sortingLayerID;
                icon.sortingOrder = send.sortingOrder + OmegaSortingOffset;
                icon.color = new Color(1f, 1f, 1f, 0.25f);

                icon.enabled = visible;
                if (!visible) return;

                Camera cam = GetGameUiCamera();
                float unitsPerPixel = cam != null && cam.orthographic
                    ? 2f * cam.orthographicSize / Mathf.Max(1, Screen.height)
                    : 0.01f;
                Vector3 parentLossy = icon.transform.parent != null
                    ? icon.transform.parent.lossyScale : Vector3.one;
                float pxHeight = Mathf.Max(0.001f, icon.sprite.bounds.size.y);
                float localSize = OmegaSizePixels * unitsPerPixel /
                    Mathf.Max(0.0001f, pxHeight * Mathf.Abs(parentLossy.y));
                icon.transform.localScale = new Vector3(localSize, localSize, 1f);


                Vector3 world = send.bounds.center;
                world.x += _omegaOffsetX * unitsPerPixel;
                world.y += _omegaOffsetY * unitsPerPixel;
                world.z = send.transform.position.z + OmegaZOffset;
                if (icon.transform.parent != null)
                    icon.transform.localPosition = icon.transform.parent.InverseTransformPoint(world);
                else
                    icon.transform.position = world;
                icon.transform.localRotation = send.transform.localRotation;

            }

            internal void UpdateLauncherPosition(RectTransform launcherRect, RectTransform canvasRect)
            {
                if (launcherRect == null || canvasRect == null || _submitButton == null)
                    return;


                try
                {
                    Camera uiCamera = GetGameUiCamera();
                    Bounds sendBounds;
                    Vector3 screenCenter;
                    float leftEdge;

                    if (TryGetSubmitBounds(out sendBounds))
                    {
                        Vector3 center = RectTransformUtility.WorldToScreenPoint(uiCamera, sendBounds.center);
                        Vector3 edge = RectTransformUtility.WorldToScreenPoint(
                            uiCamera, new Vector3(sendBounds.min.x, sendBounds.center.y, sendBounds.center.z));
                        screenCenter = center;
                        leftEdge = edge.x;
                    }
                    else
                    {
                        screenCenter = RectTransformUtility.WorldToScreenPoint(uiCamera, _submitButton.transform.position);
                        leftEdge = screenCenter.x - 25f;
                    }

                    CanvasScaler scaler = canvasRect.GetComponent<CanvasScaler>();
                    float scale = 1f;
                    Canvas rootCanvas = canvasRect.GetComponent<Canvas>();
                    if (rootCanvas != null && rootCanvas.scaleFactor > 0.001f)
                        scale = rootCanvas.scaleFactor;

                    launcherRect.sizeDelta = new Vector2(OmegaSizePixels / Mathf.Max(scale, 0.001f),
                                                         OmegaSizePixels / Mathf.Max(scale, 0.001f));

                    float halfWidthPixels = launcherRect.rect.width * scale * 0.5f;
                    Vector2 target = new Vector2(leftEdge + 32f - halfWidthPixels, screenCenter.y);
                    if (_gameLauncherNativeSprite != null && _gameLauncherNativeSprite.enabled)
                    {
                        Vector3 nativeScreen = RectTransformUtility.WorldToScreenPoint(
                            uiCamera, _gameLauncherNativeSprite.bounds.center);
                        target = new Vector2(nativeScreen.x, nativeScreen.y);
                    }
                    target.x = Mathf.Clamp(target.x, halfWidthPixels, Screen.width - halfWidthPixels);
                    target.y = Mathf.Clamp(target.y, halfWidthPixels, Screen.height - halfWidthPixels);

                    Vector2 localPoint;
                    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, target, null, out localPoint))
                        return;

                    launcherRect.anchorMin = launcherRect.anchorMax = new Vector2(0.5f, 0.5f);
                    launcherRect.pivot = new Vector2(0.5f, 0.5f);
                    launcherRect.anchoredPosition = localPoint;
                }
                catch
                {
                }
            }

            private static Camera GetGameUiCamera()
            {
                Camera camera = null;
                try
                {
                    if (HudManager.Instance != null)
                        camera = HudManager.Instance.UICamera;
                }
                catch { }
                if (camera == null)
                    camera = Camera.main;
                return camera;
            }

            private bool TryGetSubmitBounds(out Bounds bounds)
            {
                bounds = new Bounds();
                if (_submitButton == null)
                    return false;

                bool found = false;
                try
                {
                    Component[] components = _submitButton.GetComponentsInChildren<Component>(true);
                    for (int i = 0; i < components.Length; i++)
                    {
                        SpriteRenderer renderer = components[i] as SpriteRenderer;
                        if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                            continue;

                        if (!found)
                        {
                            bounds = renderer.bounds;
                            found = true;
                        }
                        else
                        {
                            bounds.Encapsulate(renderer.bounds);
                        }
                    }
                }
                catch { }

                return found;
            }

            internal void Focus()
            {
                try
                {
                    if (_freeChat != null)
                        _freeChat.Focus();
                    else if (_textBox != null)
                        _textBox.GiveFocus();
                }
                catch { }
            }
        }

        private static readonly Dictionary<Type, Dictionary<string, MemberInfo>> MemberCache =
            new Dictionary<Type, Dictionary<string, MemberInfo>>();

        private static MemberInfo FindMemberQuietly(Type type, string name)
        {
            if (!MemberCache.TryGetValue(type, out var members))
            {
                members = new Dictionary<string, MemberInfo>();
                MemberCache.Add(type, members);
            }
            if (members.TryGetValue(name, out var cached)) return cached;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;


            MemberInfo found = null;
            for (Type current = type; current != null && found == null; current = current.BaseType)
                found = current.GetField(name, flags);
            for (Type current = type; current != null && found == null; current = current.BaseType)
                found = current.GetProperty(name, flags);
            members[name] = found;
            return found;
        }

        private static object GetMember(object obj, string name)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return null;
            Type type = obj.GetType();
            try
            {
                FieldInfo field = FindMemberQuietly(type, name) as FieldInfo;
                if (field != null) return field.GetValue(obj);
            }
            catch { }
            try
            {
                PropertyInfo prop = FindMemberQuietly(type, name) as PropertyInfo;
                if (prop != null && prop.CanRead) return prop.GetValue(obj, null);
            }
            catch { }
            return null;
        }

        private static bool SetMember(object obj, string name, object value)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return false;
            Type type = obj.GetType();
            try
            {
                FieldInfo field = FindMemberQuietly(type, name) as FieldInfo;
                if (field != null)
                {
                    field.SetValue(obj, value);
                    return true;
                }
            }
            catch { }
            try
            {
                PropertyInfo prop = FindMemberQuietly(type, name) as PropertyInfo;
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(obj, value, null);
                    return true;
                }
            }
            catch { }
            return false;
        }

        private static int ReadInt(object obj, int fallback, params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                object value = GetMember(obj, names[i]);
                if (value is int n) return n;
            }
            return fallback;
        }

        private static void SetIntMember(object obj, string name, int value)
        {
            try { SetMember(obj, name, value); } catch { }
        }

        private static void InvokeIntMethod(object obj, string name, int value)
        {
            if (obj == null) return;
            try
            {
                MethodInfo method = AccessTools.Method(obj.GetType(), name, new Type[] { typeof(int) });
                if (method != null) method.Invoke(obj, new object[] { value });
            }
            catch { }
        }

        private static void TryInvokeNoArgs(object obj, string name)
        {
            if (obj == null) return;
            try
            {
                MethodInfo method = AccessTools.Method(obj.GetType(), name, Type.EmptyTypes);
                if (method != null) method.Invoke(obj, null);
            }
            catch { }
        }
    }


    [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText), new Type[] { typeof(string), typeof(string) })]
    internal static class BanModSymbolUnicodeSetTextPatch
    {
        private static bool Prefix(TextBoxTMP __instance, string __0, string __1)
        {
            if (!BanModSymbolPicker.IsRegisteredGameTextBox(__instance)) return true;
            BanModSymbolPicker.SetGameEditorText(__instance, __0, __1);
            return false;
        }
    }


    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
    internal static class BanModSymbolPickerRuntimePatch
    {
        private static void Postfix()
        {
            BanModSymbolPicker.RuntimeTick();
        }
    }
}
