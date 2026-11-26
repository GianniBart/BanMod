// credits and licenses in the resources folder

using AmongUs.GameOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static QRCoder.QRCodeGenerator;

namespace BanMod
{
    public enum MainTab
    {
        Host,
        Game,
        Moderation,
        Roles,
        Tasks,
        Sabotages
    }



    public static class OptionCategoryExtensions
    {
        public static MainTab GetDefaultMainTab(this OptionCategory category)
        {
            return category switch
            {
                OptionCategory.Lobby
                or OptionCategory.Chat
                or OptionCategory.Appearance
                or OptionCategory.Protection
                    => MainTab.Host,


                OptionCategory.GameMode1
                    or OptionCategory.GameMode
                    or OptionCategory.SNS
                    or OptionCategory.PNS
                    or OptionCategory.FFA
                    or OptionCategory.FFATEAM
                    or OptionCategory.KillRace
                    or OptionCategory.HotPotato
                    or OptionCategory.Assassin
                    or OptionCategory.Zombie 
                    or OptionCategory.Seeker
                    or OptionCategory.Gameplay
                    or OptionCategory.Meetings
                    => MainTab.Game,


                OptionCategory.AntiBot
                    or OptionCategory.Levels
                    or OptionCategory.Blocklist
                    or OptionCategory.Cheat
                    or OptionCategory.Afk
                    or OptionCategory.Cam
                    or OptionCategory.CamTask
                    or OptionCategory.Follow
                    or OptionCategory.Spamlist
                    or OptionCategory.Wordlist
                    => MainTab.Moderation,


                OptionCategory.Impostor
                    or OptionCategory.Engineer
                    or OptionCategory.Watcher
                    or OptionCategory.Scientist
                    or OptionCategory.Guesser
                    or OptionCategory.Jester
                    or OptionCategory.Exiler
                    or OptionCategory.Profiler
                    or OptionCategory.Judge
                    or OptionCategory.Immortal
                    => MainTab.Roles,


                OptionCategory.Task
                    or OptionCategory.Common
                    or OptionCategory.Short
                    or OptionCategory.Long
                    => MainTab.Tasks,


                OptionCategory.Sabotage
                    or OptionCategory.SabotageOption
                    => MainTab.Sabotages,


                _ => MainTab.Game
            };
        }



        public static string GetHeaderKey(this OptionCategory category)
        {
            return category switch
            {
                // GENERAL
                OptionCategory.Lobby => "TabGroup.Lobby",
                OptionCategory.Gameplay => "TabGroup.Gameplay",
                OptionCategory.Meetings => "TabGroup.Meetings",
                OptionCategory.Chat => "TabGroup.Chat",
                OptionCategory.Appearance => "TabGroup.Appearance",
                OptionCategory.Protection => "TabGroup.Protection",

                // GAME MODES
                OptionCategory.GameMode1 => "TabGroup.GameMode",
                OptionCategory.GameMode => "TabGroup.GameMode",
                OptionCategory.SNS => "TabGroup.SNS",
                OptionCategory.PNS => "TabGroup.PNS",
                OptionCategory.Zombie => "TabGroup.Zombie",
                OptionCategory.HotPotato => "TabGroup.HotPotato",
                OptionCategory.FFA => "TabGroup.FFA",
                OptionCategory.FFATEAM => "TabGroup.FFATEAM",
                OptionCategory.KillRace => "TabGroup.KillRace",
                OptionCategory.Seeker => "Seeker",
                OptionCategory.Assassin => "Assassin",

                // MODERATION
                OptionCategory.Blocklist => "TabGroup.Block",
                OptionCategory.Cheat => "TabGroup.Cheat",
                OptionCategory.Afk => "TabGroup.Afk",
                OptionCategory.Cam => "TabGroup.Cam",
                OptionCategory.CamTask => "TabGroup.CamTask",
                OptionCategory.Follow => "TabGroup.Follow",
                OptionCategory.Spamlist => "TabGroup.Spam",
                OptionCategory.Wordlist => "TabGroup.Word",
                OptionCategory.AntiBot => "TabGroup.AntiBot",

                // ROLES
                OptionCategory.Impostor => "TabGroup.Impostor",
                OptionCategory.Engineer => "TabGroup.Engineer",
                OptionCategory.Watcher => "TabGroup.Watcher",
                OptionCategory.Scientist => "TabGroup.Scientist",
                OptionCategory.Guesser => "TabGroup.guesser",
                OptionCategory.Jester => "TabGroup.Jester",
                OptionCategory.Exiler => "TabGroup.Exiler",
                OptionCategory.Profiler => "TabGroup.Profiler",
                OptionCategory.Judge => "TabGroup.Judge",
                OptionCategory.Immortal => "TabGroup.Immortal",

                // TASKS
                OptionCategory.Task => "TabGroup.Task",
                OptionCategory.Common => "TabGroup.Common",
                OptionCategory.Short => "TabGroup.Short",
                OptionCategory.Long => "TabGroup.Long",

                // SABOTAGES
                OptionCategory.Sabotage => "TabGroup.Sabotage",
                OptionCategory.SabotageOption => "TabGroup.SabotageOption",

                _ => category.ToString()
            };
        }
    }



    public abstract class OptionItem
    {
        #region Static

        private static readonly List<OptionItem> _allOptions = new(1024);

        private static readonly Dictionary<OptionCategory, List<OptionItem>>
            _categorizedOptions = new();

        private static readonly Dictionary<int, OptionItem>
            _fastOptions = new(1024);

        private static int _nextAutoId = 990000;


        public static IReadOnlyList<OptionItem> AllOptions => _allOptions;

        public static IReadOnlyDictionary<int, OptionItem> FastOptions
            => _fastOptions;



        public static int NextAutoId()
        {
            while (_fastOptions.ContainsKey(_nextAutoId))
                _nextAutoId++;

            return _nextAutoId++;
        }



        public static IReadOnlyList<OptionItem> GetOptions(
            OptionCategory category)
        {
            if (_categorizedOptions.TryGetValue(
                    category,
                    out var options))
            {
                return options;
            }

            return Array.Empty<OptionItem>();
        }



        public static IReadOnlyList<OptionItem> GameMode1Options
            => GetOptions(OptionCategory.GameMode1);
        public static IReadOnlyList<OptionItem> GameModeOptions
            => GetOptions(OptionCategory.GameMode);

        public static IReadOnlyList<OptionItem> SNSOptions
            => GetOptions(OptionCategory.SNS);

        public static IReadOnlyList<OptionItem> PNSOptions
            => GetOptions(OptionCategory.PNS);

        public static IReadOnlyList<OptionItem> HotPotatoOptions
            => GetOptions(OptionCategory.HotPotato);

        public static IReadOnlyList<OptionItem> ZombieOptions
            => GetOptions(OptionCategory.Zombie);

        public static IReadOnlyList<OptionItem> FFAOptions
            => GetOptions(OptionCategory.FFA);
        public static IReadOnlyList<OptionItem> FFATEAMOptions
            => GetOptions(OptionCategory.FFATEAM);

        public static IReadOnlyList<OptionItem> KillRaceOptions
            => GetOptions(OptionCategory.KillRace);

        public static IReadOnlyList<OptionItem> AssassinOptions
            => GetOptions(OptionCategory.Assassin);

        public static IReadOnlyList<OptionItem> SeekerOptions
            => GetOptions(OptionCategory.Seeker);


        public static IReadOnlyList<OptionItem> LobbyOptions
            => GetOptions(OptionCategory.Lobby);

        public static IReadOnlyList<OptionItem> GameplayOptions
            => GetOptions(OptionCategory.Gameplay);

        public static IReadOnlyList<OptionItem> MeetingsOptions
            => GetOptions(OptionCategory.Meetings);

        public static IReadOnlyList<OptionItem> ChatOptions
            => GetOptions(OptionCategory.Chat);

        public static IReadOnlyList<OptionItem> AppearanceOptions
            => GetOptions(OptionCategory.Appearance);

        public static IReadOnlyList<OptionItem> ProtectionOptions
            => GetOptions(OptionCategory.Protection);




        public static IReadOnlyList<OptionItem> BlocklistOptions
            => GetOptions(OptionCategory.Blocklist);

        public static IReadOnlyList<OptionItem> CheatOptions
            => GetOptions(OptionCategory.Cheat);

        public static IReadOnlyList<OptionItem> AfkOptions
            => GetOptions(OptionCategory.Afk);

        public static IReadOnlyList<OptionItem> CamOptions
            => GetOptions(OptionCategory.Cam);

        public static IReadOnlyList<OptionItem> CamTaskOptions
            => GetOptions(OptionCategory.CamTask);

        public static IReadOnlyList<OptionItem> FollowOptions
            => GetOptions(OptionCategory.Follow);

        public static IReadOnlyList<OptionItem> SpamlistOptions
            => GetOptions(OptionCategory.Spamlist);

        public static IReadOnlyList<OptionItem> WordlistOptions
            => GetOptions(OptionCategory.Wordlist);



        public static IReadOnlyList<OptionItem> ImpostorOptions
            => GetOptions(OptionCategory.Impostor);

        public static IReadOnlyList<OptionItem> EngineerOptions
            => GetOptions(OptionCategory.Engineer);

        public static IReadOnlyList<OptionItem> WatcherOptions
            => GetOptions(OptionCategory.Watcher);

        public static IReadOnlyList<OptionItem> ScientistOptions
            => GetOptions(OptionCategory.Scientist);

        public static IReadOnlyList<OptionItem> GuesserOptions
            => GetOptions(OptionCategory.Guesser);

        public static IReadOnlyList<OptionItem> JesterOptions
            => GetOptions(OptionCategory.Jester);

        public static IReadOnlyList<OptionItem> ExilerOptions
            => GetOptions(OptionCategory.Exiler);

        public static IReadOnlyList<OptionItem> ProfilerOptions
            => GetOptions(OptionCategory.Profiler);

        public static IReadOnlyList<OptionItem> JudgeOptions
            => GetOptions(OptionCategory.Judge);

        public static IReadOnlyList<OptionItem> ImmortalOptions
            => GetOptions(OptionCategory.Immortal);



        public static IReadOnlyList<OptionItem> TaskOptions
            => GetOptions(OptionCategory.Task);

        public static IReadOnlyList<OptionItem> CommonOptions
            => GetOptions(OptionCategory.Common);

        public static IReadOnlyList<OptionItem> ShortOptions
            => GetOptions(OptionCategory.Short);

        public static IReadOnlyList<OptionItem> LongOptions
            => GetOptions(OptionCategory.Long);



        public static IReadOnlyList<OptionItem> SabotageOptions
            => GetOptions(OptionCategory.Sabotage);

        public static IReadOnlyList<OptionItem> SabotageSettingOptions
            => GetOptions(OptionCategory.SabotageOption);


        public static int CurrentPreset { get; set; }


#if DEBUG
        public static bool IdDuplicated { get; private set; }
#endif

        #endregion



        public int Id { get; }

        public string Name { get; }

        public int DefaultValue { get; }

        public OptionCategory Category { get; }

        public MainTab MainTab { get; }


        public bool IsSingleValue { get; }

        public bool IsExternallyEnabled { get; private set; } = true;


        public Color NameColor { get; protected set; }

        public OptionFormat ValueFormat { get; protected set; }

        public bool IsHeader { get; protected set; }

        public bool IsHidden { get; protected set; }


        private Dictionary<string, string> _replacementDictionary;

        public Dictionary<string, string> ReplacementDictionary
        {
            get => _replacementDictionary;

            set
            {
                if (value == null)
                    _replacementDictionary?.Clear();
                else
                    _replacementDictionary = value;
            }
        }


        public int[] AllValues { get; private set; }
            = new int[NumPresets];


        public int CurrentValue
        {
            get => GetValue();
            set => SetValue(value);
        }


        public int SingleValue { get; private set; }


        public OptionItem Parent { get; private set; }

        public List<OptionItem> Children;

        public StringOption OptionBehaviour;


        public event EventHandler<UpdateValueEventArgs>
            UpdateValueEvent;



        public void SetEnabled(bool enabled)
        {
            IsExternallyEnabled = enabled;
            RefreshVisibilityRecursive();
        }


        public bool IsVisibleByParent()
        {
            return
                !IsHidden &&
                IsExternallyEnabled &&
                (Parent == null || Parent.GetBool());
        }


        public void RefreshVisibilityRecursive()
        {
            if (OptionBehaviour != null)
            {
                OptionBehaviour.gameObject.SetActive(
                    GameSettingMenuPatch.ShouldShowOption(this)
                );
            }

            RefreshChildrenVisibility();
        }


        public void RefreshChildrenVisibility()
        {
            if (Children == null)
                return;

            foreach (var child in Children)
            {
                if (child == null)
                    continue;

                if (child.OptionBehaviour != null)
                {
                    child.OptionBehaviour.gameObject.SetActive(
                        GameSettingMenuPatch.ShouldShowOption(child)
                    );
                }

                child.RefreshChildrenVisibility();
            }
        }


        public OptionItem(
            int id,
            string name,
            int defaultValue,
            OptionCategory category,
            bool isSingleValue)
        {
            Id = id;
            Name = name;
            DefaultValue = defaultValue;
            Category = category;

            MainTab = category.GetDefaultMainTab();

            IsSingleValue = isSingleValue;

            NameColor = Color.white;
            ValueFormat = OptionFormat.None;

            Children = new List<OptionItem>();


            if (Id == PresetId)
            {
                SingleValue = DefaultValue;
                CurrentPreset = SingleValue;
            }
            else if (IsSingleValue)
            {
                SingleValue = DefaultValue;
            }
            else
            {
                for (int i = 0; i < NumPresets; i++)
                    AllValues[i] = DefaultValue;
            }


            if (_fastOptions.TryAdd(Id, this))
            {
                _allOptions.Add(this);

                if (!_categorizedOptions.TryGetValue(
                        Category,
                        out var categoryOptions))
                {
                    categoryOptions = new List<OptionItem>();

                    _categorizedOptions.Add(
                        Category,
                        categoryOptions
                    );
                }

                categoryOptions.Add(this);
            }
            else
            {
#if DEBUG
                IdDuplicated = true;
#endif

                BMLogger.Error(
                    $"ID duplicato rilevato: {Id}",
                    "OptionItem"
                );
            }
        }



        public OptionItem Do(Action<OptionItem> action)
        {
            action(this);
            return this;
        }


        public OptionItem SetColor(Color value)
            => Do(i => i.NameColor = value);


        public OptionItem SetValueFormat(OptionFormat value)
            => Do(i => i.ValueFormat = value);


        public OptionItem SetHeader(bool value)
            => Do(i => i.IsHeader = value);


        public OptionItem SetHidden(bool value)
        {
            IsHidden = value;
            RefreshVisibilityRecursive();

            return this;
        }


        public OptionItem SetParent(OptionItem parent)
        {
            if (Parent == parent)
                return this;

            if (Parent != null)
                Parent.Children.Remove(this);

            Parent = parent;

            if (parent != null &&
                !parent.Children.Contains(this))
            {
                parent.Children.Add(this);
            }

            RefreshVisibilityRecursive();

            return this;
        }


        public OptionItem RegisterUpdateValueEvent(
            EventHandler<UpdateValueEventArgs> handler)
        {
            UpdateValueEvent += handler;
            return this;
        }


        public OptionItem AddReplacement(
            (string key, string value) kvp)
        {
            ReplacementDictionary ??= new();

            ReplacementDictionary[kvp.key] = kvp.value;

            return this;
        }


        public OptionItem RemoveReplacement(string key)
        {
            ReplacementDictionary?.Remove(key);
            return this;
        }



        public virtual string GetName(
            bool disableColor = false)
        {
            string name = Translator.GetString(
                Name,
                ReplacementDictionary
            );

            return disableColor
                ? name
                : Utils.ColorString(NameColor, name);
        }


        public virtual bool GetBool()
        {
            return CurrentValue != 0 &&
                   (Parent == null || Parent.GetBool());
        }


        public virtual int GetInt()
            => CurrentValue;


        public virtual float GetFloat()
            => CurrentValue;


        public virtual string GetString()
            => ApplyFormat(CurrentValue.ToString());


        public virtual int GetValue()
        {
            return IsSingleValue
                ? SingleValue
                : AllValues[CurrentPreset];
        }


        public string ApplyFormat(string value)
        {
            if (ValueFormat == OptionFormat.None)
                return value;

            return string.Format(
                Translator.GetString(
                    "Format." + ValueFormat
                ),
                value
            );
        }


        public virtual void Refresh()
        {
            if (OptionBehaviour is not StringOption opt ||
                opt == null)
            {
                RefreshVisibilityRecursive();
                return;
            }

            if (opt.TitleText != null)
                opt.TitleText.text = GetName();

            if (opt.ValueText != null)
                opt.ValueText.text = GetString();

            opt.oldValue = opt.Value = CurrentValue;

            RefreshVisibilityRecursive();
        }


        public virtual void SetValue(
            int afterValue,
            bool doSave,
            bool doSync = true)
        {
            int beforeValue = CurrentValue;

            if (IsSingleValue)
            {
                SingleValue = afterValue;
            }
            else
            {
                AllValues[CurrentPreset] = afterValue;
            }

            CallUpdateValueEvent(
                beforeValue,
                afterValue
            );

            Refresh();
            RefreshChildrenVisibility();

            if (doSync)
                SyncAllOptions();

            if (doSave)
                OptionSaver.Save();
        }


        public virtual void SetValue(
            int afterValue,
            bool doSync = true)
        {
            SetValue(
                afterValue,
                true,
                doSync
            );
        }


        public void SetAllValues(int[] values)
        {
            AllValues = values;
        }


        public static OptionItem operator ++(
            OptionItem item)
        {
            item.SetValue(item.CurrentValue + 1);
            return item;
        }


        public static OptionItem operator --(
            OptionItem item)
        {
            item.SetValue(item.CurrentValue - 1);
            return item;
        }



        public static void SwitchPreset(int newPreset)
        {
            CurrentPreset = Math.Clamp(
                newPreset,
                0,
                NumPresets - 1
            );

            foreach (var option in AllOptions)
                option.Refresh();

            SyncAllOptions();
        }



        public static void SyncAllOptions()
        {
            if (Options.GameMode == null)
                return;

            if (BanMod.AllPlayerControls == null ||
                BanMod.AllPlayerControls.Count() <= 0)
            {
                return;
            }

            GameModeType gameMode =
                Options.GameMode.Selected;


            if (BanMod.AllPlayerControls.Count() <= 0)
                return;

            if (AmongUsClient.Instance == null ||
                !AmongUsClient.Instance.AmHost)
            {
                return;
            }

            if (PlayerControl.LocalPlayer == null)
                return;

            bool isHideAndSeek =
                GameOptionsManager.Instance != null &&
                GameOptionsManager.Instance.CurrentGameOptions != null &&
                GameOptionsManager.Instance.CurrentGameOptions.GameMode ==
                    GameModes.HideNSeek;

            if (isHideAndSeek ||
                gameMode == GameModeType.SnS ||
                gameMode == GameModeType.PnS ||
                gameMode == GameModeType.TaskRun ||
                gameMode == GameModeType.ZombieMode ||
                gameMode == GameModeType.HotPotatoModded ||
                gameMode == GameModeType.RoomRush ||
                gameMode == GameModeType.DeathRun ||
                gameMode == GameModeType.TargetRush ||
                gameMode == GameModeType.FFATeam ||
                gameMode == GameModeType.KillRace ||
                gameMode == GameModeType.Assassin ||
                gameMode == GameModeType.FFA
            )
            {
                BanMod.DisableAllRoles();
            }
        }



        private void CallUpdateValueEvent(
            int beforeValue,
            int currentValue)
        {
            if (UpdateValueEvent == null)
                return;

            try
            {
                UpdateValueEvent(
                    this,
                    new UpdateValueEventArgs(
                        beforeValue,
                        currentValue
                    )
                );
            }
            catch (Exception ex)
            {
                BMLogger.Error(
                    $"[{Name}] Eccezione durante UpdateValueEvent",
                    "OptionItem.UpdateValueEvent"
                );

                BMLogger.Exception(
                    ex,
                    "OptionItem.UpdateValueEvent"
                );
            }
        }


        public class UpdateValueEventArgs : EventArgs
        {
            public int CurrentValue { get; set; }

            public int BeforeValue { get; set; }


            public UpdateValueEventArgs(
                int beforeValue,
                int currentValue)
            {
                BeforeValue = beforeValue;
                CurrentValue = currentValue;
            }
        }



        public const int NumPresets = 5;

        public const int PresetId = 0;
    }



    public enum OptionCategory
    {
        // GENERAL
        Lobby,
        Gameplay,
        Meetings,
        Chat,
        Appearance,
        Protection,

        // GAME MODES
        GameMode1,
        GameMode,
        SNS,
        PNS,
        FFA,
        FFATEAM,
        KillRace,
        HotPotato,
        Zombie,
        Seeker,
        Assassin,

        // MODERATION
        Levels,
        AntiBot,
        Blocklist,
        Cheat,
        Afk,
        Cam,
        CamTask,
        Follow,
        Spamlist,
        Wordlist,

        // ROLES
        Impostor,
        Engineer,
        Watcher,
        Scientist,
        Guesser,
        Jester,
        Exiler,
        Profiler,
        Judge,
        Immortal,

        // TASKS
        Task,
        Common,
        Short,
        Long,

        // SABOTAGES
        SabotageOption,
        Sabotage
    }


    public enum OptionFormat
    {
        None,
        Players,
        Seconds,
        Percent,
        Times,
        Multiplier,
        Votes,
        Pieces
    }
}