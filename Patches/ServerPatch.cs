
//using HarmonyLib;
//using InnerNet;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using TMPro;
//using UnityEngine;
//using static BanMod.Utils;

//namespace BanMod
//{
//    public enum BanModServerMode
//    {
//        None,
//        Modded25,
//        Vanilla
//    }

//    public static class BanModServerSelection
//    {
//        private static BanModServerMode _mode = BanModServerMode.Vanilla;

//        public static BanModServerMode Mode
//        {
//            get => _mode;
//            set
//            {
//                if (_mode == value)
//                    return;

//                _mode = value;

//                try
//                {
//                    Options.UpdateGameModesForServer();
//                }
//                catch (Exception ex)
//                {
//                    Debug.LogWarning(
//                        "[BanMod] UpdateGameModesForServer error: " + ex
//                    );
//                }
//            }
//        }

//        public static bool VanillaAcknowledged = false;
//        public static bool VanillaCreateBypassOnce = false;

//        public static bool IsModded25 =>
//            Mode == BanModServerMode.Modded25;

//        public static bool IsVanilla =>
//            Mode == BanModServerMode.Vanilla;

//        public static bool HasSelectedMode =>
//            Mode != BanModServerMode.None;

//        public static void Reset()
//        {
//            Mode = BanModServerMode.Vanilla;
//            VanillaAcknowledged = false;
//            VanillaCreateBypassOnce = false;
//        }

//        public static bool ConsumeVanillaCreateBypass()
//        {
//            if (!VanillaCreateBypassOnce)
//                return false;

//            VanillaCreateBypassOnce = false;
//            return true;
//        }
//        private const string VanillaAcceptedKey = "BanMod_VanillaAcceptedAt";

//        public static bool HasAcceptedVanillaThisWeek()
//        {
//            string saved = PlayerPrefs.GetString(VanillaAcceptedKey, "");

//            if (string.IsNullOrEmpty(saved))
//                return false;

//            if (!long.TryParse(saved, out long ticks))
//                return false;

//            DateTime acceptedAt = new DateTime(ticks, DateTimeKind.Utc);

//            return DateTime.UtcNow - acceptedAt < TimeSpan.FromDays(7);
//        }
//        public static DateTime? GetVanillaAcceptanceExpiry()
//        {
//            string saved = PlayerPrefs.GetString(
//                VanillaAcceptedKey,
//                ""
//            );

//            if (string.IsNullOrEmpty(saved))
//                return null;

//            if (!long.TryParse(saved, out long ticks))
//                return null;

//            DateTime acceptedAt =
//                new DateTime(
//                    ticks,
//                    DateTimeKind.Utc
//                );

//            return acceptedAt
//                .AddDays(7)
//                .ToLocalTime();
//        }
//        public static void SaveVanillaAcceptance()
//        {
//            PlayerPrefs.SetString(
//                VanillaAcceptedKey,
//                DateTime.UtcNow.Ticks.ToString()
//            );

//            PlayerPrefs.Save();

//            VanillaAcknowledged = true;
//        }
//    }

//    public class BanModServerTexts
//    {
//        public string SelectTitle;
//        public string SelectDescription;
//        public string ModdedDescription;
//        public string VanillaDescription;
//        public string ModdedButton;
//        public string VanillaButton;
//        public string PrivateFooter;

//        public string VanillaTitle;
//        public string VanillaIntro;
//        public string VanillaWarning;
//        public string ConfirmButton;
//        public string BackButton;

//    }

//    public static class BanModServerLocalization
//    {
//        public static BanModServerTexts Get()
//        {
//            string language = GetLanguageId();

//            if (Has(language, "italian", "italiano"))
//                return Italian();

//            if (Has(language, "french", "franÃ§ais", "francais"))
//                return French();

//            if (Has(language, "german", "deutsch"))
//                return German();

//            if (Has(
//                language,
//                "latam",
//                "spanishlatam",
//                "spanish_la",
//                "spanishla",
//                "espanollatam",
//                "espaÃ±ollatam"
//            ))
//                return SpanishLatam();

//            if (Has(
//                language,
//                "spanish",
//                "spanisheU",
//                "spanish_eu",
//                "espanol",
//                "espaÃ±ol"
//            ))
//                return Spanish();

//            if (Has(
//                language,
//                "brazilian",
//                "brazilianportuguese",
//                "portuguesebrazil",
//                "portuguesebr",
//                "portuguÃªsbr",
//                "portuguesbr"
//            ))
//                return BrazilianPortuguese();

//            if (Has(
//                language,
//                "portuguese",
//                "portugueseeu",
//                "portuguese_eu",
//                "portuguÃªs",
//                "portugues"
//            ))
//                return Portuguese();

//            if (Has(language, "dutch", "nederlands"))
//                return Dutch();

//            if (Has(language, "russian", "Ñ€ÑƒÑÑÐºÐ¸Ð¹"))
//                return Russian();

//            if (Has(language, "japanese", "æ—¥æœ¬èªž"))
//                return Japanese();

//            if (Has(language, "korean", "í•œêµ­ì–´"))
//                return Korean();

//            if (Has(
//                language,
//                "schinese",
//                "simplifiedchinese",
//                "chinesesimplified",
//                "chinese_cn",
//                "ç®€ä½“ä¸­æ–‡"
//            ))
//                return SimplifiedChinese();

//            if (Has(
//                language,
//                "tchinese",
//                "traditionalchinese",
//                "chinesetraditional",
//                "chinese_tw",
//                "ç¹é«”ä¸­æ–‡",
//                "ç¹ä½“ä¸­æ–‡"
//            ))
//                return TraditionalChinese();

//            if (Has(
//                language,
//                "filipino",
//                "bisaya",
//                "cebuano"
//            ))
//                return Filipino();

//            return English();
//        }

//        private static string GetLanguageId()
//        {
//            try
//            {
//                if (TranslationController.Instance != null &&
//                    TranslationController.Instance.currentLanguage != null)
//                {
//                    string id =
//                        TranslationController.Instance
//                            .currentLanguage
//                            .languageID
//                            .ToString();

//                    if (!string.IsNullOrEmpty(id))
//                        return id.ToLowerInvariant();
//                }
//            }
//            catch
//            {
//            }

//            return "english";
//        }

//        private static bool Has(
//            string value,
//            params string[] names
//        )
//        {
//            if (string.IsNullOrEmpty(value))
//                return false;

//            for (int i = 0; i < names.Length; i++)
//            {
//                if (value.Equals(
//                    names[i].ToLowerInvariant(),
//                    StringComparison.OrdinalIgnoreCase
//                ))
//                    return true;
//            }

//            return false;
//        }

//        private static BanModServerTexts English()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - SELECT LOBBY MODE",
//                SelectDescription =
//                    "Choose the lobby mode before creating this lobby.",
//                ModdedDescription =
//                    "Use this server to add roles and modify the gameplay.",
//                VanillaDescription =
//                    "Use this server if you only use anti-cheat and visual modifications.",
//                ModdedButton =
//                    "MODDED +25\nRECOMMENDED",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "Use BanMod responsibly and select the mode that matches the type of lobby you are creating.",
//                VanillaTitle =
//                    "IMPORTANT - VANILLA MODE",
//                VanillaIntro =
//                    "You selected Vanilla mode.",
//                VanillaWarning =
//                    "Please do not use BanMod to annoy or disturb other players, and do not enable options that change gameplay, provide unfair advantages, or alter the experience of other players.\n\nImproper, unauthorized, or rule-breaking use may result in warnings, restrictions, suspensions, bans, or other sanctions from Innersloth or other services involved.\n\nBanMod and its developers are not responsible for any consequences resulting from improper, unlawful, or unauthorized use of the mod.\n\nUse only features compatible with Vanilla mode and respect the rules of Among Us and any other services being used.",
//                ConfirmButton =
//                    "I AGREE",
//                BackButton =
//                    "I DECLINE"
//            };
//        }

//        private static BanModServerTexts Italian()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - SELEZIONA MODALITÀ LOBBY",
//                SelectDescription =
//                    "Scegli la modalità della lobby prima di crearla.",
//                ModdedDescription =
//                    "Usa questo server per aggiungere ruoli e modificare il gameplay.",
//                VanillaDescription =
//                    "Usa questo server se usi solo anti-cheat e modifiche visuali.",
//                ModdedButton =
//                    "MODDED +25\nCONSIGLIATO",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "Usa BanMod responsabilmente e seleziona la modalità che corrisponde al tipo di lobby che stai creando.",
//                VanillaTitle =
//                    "IMPORTANTE - MODALITÀ VANILLA",
//                VanillaIntro =
//                    "Hai scelto la modalità Vanilla.",
//                VanillaWarning =
//                    "Per favore, non utilizzare BanMod per infastidire o disturbare altri giocatori e non attivare opzioni che modificano il gameplay, forniscono vantaggi sleali o alterano l’esperienza degli altri giocatori.\n\nUn utilizzo improprio, non consentito o contrario alle regole può provocare avvertimenti, restrizioni, sospensioni, ban o altre sanzioni da parte di Innersloth o di altri servizi coinvolti.\n\nBanMod e i suoi sviluppatori non sono responsabili per eventuali conseguenze derivanti da un utilizzo improprio, illecito o non consentito della mod.\n\nAssicurati di utilizzare esclusivamente funzionalità compatibili con la modalità Vanilla e di rispettare le regole di Among Us e degli altri servizi utilizzati.",
//                ConfirmButton =
//                    "ACCETTO",
//                BackButton =
//                    "NEGO"
//            };
//        }

//        private static BanModServerTexts French()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - SÉLECTION DU MODE DU SALON",
//                SelectDescription =
//                    "Choisissez le mode du salon avant de le créer.",
//                ModdedDescription =
//                    "Utilisez ce serveur pour ajouter des rôles et modifier le gameplay.",
//                VanillaDescription =
//                    "Utilisez ce serveur si vous utilisez uniquement un anti-triche et des modifications visuelles.",
//                ModdedButton =
//                    "MODDED +25\nRECOMMANDÉ",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "Utilisez BanMod de manière responsable et choisissez le mode correspondant au type de salon que vous créez.",
//                VanillaTitle =
//                    "IMPORTANT - MODE VANILLA",
//                VanillaIntro =
//                    "Vous avez choisi le mode Vanilla.",
//                VanillaWarning =
//                    "Veuillez ne pas utiliser BanMod pour déranger ou perturber les autres joueurs, et n’activez pas d’options qui modifient le gameplay, donnent des avantages injustes ou changent l’expérience des autres joueurs.\n\nUne utilisation abusive, non autorisée ou contraire aux règles peut entraîner des avertissements, restrictions, suspensions, bannissements ou autres sanctions de la part d’Innersloth ou d’autres services concernés.\n\nBanMod et ses développeurs ne sont pas responsables des conséquences résultant d’une utilisation abusive, illégale ou non autorisée du mod.\n\nUtilisez uniquement des fonctionnalités compatibles avec le mode Vanilla et respectez les règles d’Among Us et des autres services utilisés.",
//                ConfirmButton =
//                    "J’ACCEPTE",
//                BackButton =
//                    "JE REFUSE"
//            };
//        }

//        private static BanModServerTexts German()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - LOBBY-MODUS AUSWÄHLEN",
//                SelectDescription =
//                    "Wähle den Lobby-Modus, bevor du diese Lobby erstellst.",
//                ModdedDescription =
//                    "Verwende diesen Server, um Rollen hinzuzufügen und das Gameplay zu verändern.",
//                VanillaDescription =
//                    "Verwende diesen Server, wenn du nur Anti-Cheat und visuelle Änderungen nutzt.",
//                ModdedButton =
//                    "MODDED +25\nEMPFOHLEN",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "Verwende BanMod verantwortungsvoll und wähle den Modus, der zu deiner Lobby passt.",
//                VanillaTitle =
//                    "WICHTIG - VANILLA-MODUS",
//                VanillaIntro =
//                    "Du hast den Vanilla-Modus ausgewählt.",
//                VanillaWarning =
//                    "Bitte verwende BanMod nicht, um andere Spieler zu stören oder zu belästigen, und aktiviere keine Optionen, die das Gameplay verändern, unfaire Vorteile geben oder die Erfahrung anderer Spieler verändern.\n\nEine unsachgemäße, nicht autorisierte oder regelwidrige Nutzung kann zu Verwarnungen, Einschränkungen, Sperren, Bans oder anderen Maßnahmen durch Innersloth oder andere beteiligte Dienste führen.\n\nBanMod und seine Entwickler sind nicht für Folgen verantwortlich, die aus einer unsachgemäßen, rechtswidrigen oder nicht autorisierten Nutzung der Mod entstehen.\n\nVerwende nur mit dem Vanilla-Modus kompatible Funktionen und halte die Regeln von Among Us sowie der verwendeten Dienste ein.",
//                ConfirmButton =
//                    "ICH STIMME ZU",
//                BackButton =
//                    "ICH LEHNE AB"
//            };
//        }

//        private static BanModServerTexts Spanish()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - SELECCIONAR MODO DE SALA",
//                SelectDescription =
//                    "Elige el modo de la sala antes de crearla.",
//                ModdedDescription =
//                    "Usa este servidor para añadir roles y modificar el gameplay.",
//                VanillaDescription =
//                    "Usa este servidor si solo utilizas anti-cheat y modificaciones visuales.",
//                ModdedButton =
//                    "MODDED +25\nRECOMENDADO",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "Usa BanMod de forma responsable y selecciona el modo que corresponda al tipo de sala que vas a crear.",
//                VanillaTitle =
//                    "IMPORTANTE - MODO VANILLA",
//                VanillaIntro =
//                    "Has elegido el modo Vanilla.",
//                VanillaWarning =
//                    "Por favor, no uses BanMod para molestar o perturbar a otros jugadores y no actives opciones que cambien el gameplay, proporcionen ventajas injustas o alteren la experiencia de otros jugadores.\n\nEl uso indebido, no autorizado o contrario a las reglas puede provocar advertencias, restricciones, suspensiones, baneos u otras sanciones por parte de Innersloth u otros servicios involucrados.\n\nBanMod y sus desarrolladores no son responsables de las consecuencias derivadas de un uso indebido, ilegal o no autorizado del mod.\n\nUtiliza únicamente funciones compatibles con el modo Vanilla y respeta las reglas de Among Us y de los demás servicios utilizados.",
//                ConfirmButton =
//                    "ACEPTO",
//                BackButton =
//                    "NO ACEPTO"
//            };
//        }

//        private static BanModServerTexts SpanishLatam()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - SELECCIONAR MODO DE SALA",
//                SelectDescription =
//                    "Elige el modo de la sala antes de crearla.",
//                ModdedDescription =
//                    "Usa este servidor para añadir roles y modificar el gameplay.",
//                VanillaDescription =
//                    "Usa este servidor si solo usas anti-cheat y modificaciones visuales.",
//                ModdedButton =
//                    "MODDED +25\nRECOMENDADO",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "Usa BanMod de forma responsable y selecciona el modo que corresponda al tipo de sala que vas a crear.",
//                VanillaTitle =
//                    "IMPORTANTE - MODO VANILLA",
//                VanillaIntro =
//                    "Has elegido el modo Vanilla.",
//                VanillaWarning =
//                    "Por favor, no uses BanMod para molestar o perturbar a otros jugadores y no actives opciones que cambien el gameplay, den ventajas injustas o alteren la experiencia de otros jugadores.\n\nEl uso indebido, no autorizado o contrario a las reglas puede provocar advertencias, restricciones, suspensiones, baneos u otras sanciones por parte de Innersloth u otros servicios involucrados.\n\nBanMod y sus desarrolladores no son responsables de las consecuencias derivadas de un uso indebido, ilegal o no autorizado del mod.\n\nUtiliza únicamente funciones compatibles con el modo Vanilla y respeta las reglas de Among Us y de los demás servicios utilizados.",
//                ConfirmButton =
//                    "ACEPTO",
//                BackButton =
//                    "NO ACEPTO"
//            };
//        }

//        private static BanModServerTexts BrazilianPortuguese()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - SELECIONAR MODO DA SALA",
//                SelectDescription =
//                    "Escolha o modo da sala antes de criá-la.",
//                ModdedDescription =
//                    "Use este servidor para adicionar funções e modificar o gameplay.",
//                VanillaDescription =
//                    "Use este servidor se você utiliza apenas anti-cheat e modificações visuais.",
//                ModdedButton =
//                    "MODDED +25\nRECOMENDADO",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "Use o BanMod com responsabilidade e selecione o modo correspondente ao tipo de sala que você está criando.",
//                VanillaTitle =
//                    "IMPORTANTE - MODO VANILLA",
//                VanillaIntro =
//                    "Você escolheu o modo Vanilla.",
//                VanillaWarning =
//                    "Por favor, não use o BanMod para incomodar ou perturbar outros jogadores e não ative opções que alterem o gameplay, forneçam vantagens injustas ou mudem a experiência de outros jogadores.\n\nO uso indevido, não autorizado ou contrário às regras pode resultar em avisos, restrições, suspensões, banimentos ou outras sanções da Innersloth ou de outros serviços envolvidos.\n\nO BanMod e seus desenvolvedores não são responsáveis por quaisquer consequências decorrentes do uso indevido, ilegal ou não autorizado do mod.\n\nUse apenas recursos compatíveis com o modo Vanilla e respeite as regras de Among Us e dos outros serviços utilizados.",
//                ConfirmButton =
//                    "ACEITO",
//                BackButton =
//                    "NÃO ACEITO"
//            };
//        }

//        private static BanModServerTexts Portuguese()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - SELECIONAR MODO DA SALA",
//                SelectDescription =
//                    "Escolhe o modo da sala antes de a criares.",
//                ModdedDescription =
//                    "Usa este servidor para adicionar funções e modificar o gameplay.",
//                VanillaDescription =
//                    "Usa este servidor se utilizares apenas anti-cheat e modificações visuais.",
//                ModdedButton =
//                    "MODDED +25\nRECOMENDADO",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "Usa o BanMod de forma responsável e seleciona o modo correspondente ao tipo de sala que estás a criar.",
//                VanillaTitle =
//                    "IMPORTANTE - MODO VANILLA",
//                VanillaIntro =
//                    "Escolheste o modo Vanilla.",
//                VanillaWarning =
//                    "Por favor, não uses o BanMod para incomodar ou perturbar outros jogadores e não atives opções que alterem o gameplay, deem vantagens injustas ou mudem a experiência dos outros jogadores.\n\nUma utilização indevida, não autorizada ou contrária às regras pode resultar em avisos, restrições, suspensões, banimentos ou outras sanções da Innersloth ou de outros serviços envolvidos.\n\nO BanMod e os seus desenvolvedores não são responsáveis por quaisquer consequências resultantes de uma utilização indevida, ilegal ou não autorizada da mod.\n\nUtiliza apenas funcionalidades compatíveis com o modo Vanilla e respeita as regras de Among Us e dos outros serviços utilizados.",
//                ConfirmButton =
//                    "ACEITO",
//                BackButton =
//                    "NÃO ACEITO"
//            };
//        }

//        private static BanModServerTexts Dutch()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - LOBBYMODUS SELECTEREN",
//                SelectDescription =
//                    "Kies de lobbymodus voordat je deze lobby maakt.",
//                ModdedDescription =
//                    "Gebruik deze server om rollen toe te voegen en de gameplay aan te passen.",
//                VanillaDescription =
//                    "Gebruik deze server als je alleen anti-cheat en visuele aanpassingen gebruikt.",
//                ModdedButton =
//                    "MODDED +25\nAANBEVOLEN",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "Gebruik BanMod verantwoord en kies de modus die past bij het type lobby dat je maakt.",
//                VanillaTitle =
//                    "BELANGRIJK - VANILLA-MODUS",
//                VanillaIntro =
//                    "Je hebt de Vanilla-modus gekozen.",
//                VanillaWarning =
//                    "Gebruik BanMod niet om andere spelers te ergeren of te hinderen en schakel geen opties in die de gameplay veranderen, oneerlijke voordelen geven of de ervaring van andere spelers aanpassen.\n\nOnjuist, ongeoorloofd of regelstrijdig gebruik kan leiden tot waarschuwingen, beperkingen, schorsingen, bans of andere sancties van Innersloth of andere betrokken diensten.\n\nBanMod en de ontwikkelaars zijn niet verantwoordelijk voor gevolgen die voortkomen uit onjuist, onwettig of ongeoorloofd gebruik van de mod.\n\nGebruik alleen functies die compatibel zijn met de Vanilla-modus en respecteer de regels van Among Us en de gebruikte diensten.",
//                ConfirmButton =
//                    "IK GA AKKOORD",
//                BackButton =
//                    "IK WEIGER"
//            };
//        }

//        private static BanModServerTexts Russian()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - ВЫБОР РЕЖИМА ЛОББИ",
//                SelectDescription =
//                    "Выберите режим лобби перед его созданием.",
//                ModdedDescription =
//                    "Используйте этот сервер, чтобы добавлять роли и изменять игровой процесс.",
//                VanillaDescription =
//                    "Используйте этот сервер, если вы применяете только античит и визуальные изменения.",
//                ModdedButton =
//                    "MODDED +25\nРЕКОМЕНДУЕТСЯ",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "Используйте BanMod ответственно и выбирайте режим, соответствующий создаваемому лобби.",
//                VanillaTitle =
//                    "ВАЖНО - РЕЖИМ VANILLA",
//                VanillaIntro =
//                    "Вы выбрали режим Vanilla.",
//                VanillaWarning =
//                    "Пожалуйста, не используйте BanMod для того, чтобы мешать или раздражать других игроков, и не включайте функции, которые изменяют игровой процесс, дают нечестные преимущества или влияют на опыт других игроков.\n\nНеправильное, несанкционированное или нарушающее правила использование может привести к предупреждениям, ограничениям, приостановке, блокировке или другим санкциям со стороны Innersloth или других сервисов.\n\nBanMod и его разработчики не несут ответственности за последствия неправильного, незаконного или несанкционированного использования мода.\n\nИспользуйте только функции, совместимые с режимом Vanilla, и соблюдайте правила Among Us и других используемых сервисов.",
//                ConfirmButton =
//                    "ПРИНИМАЮ",
//                BackButton =
//                    "ОТКАЗЫВАЮСЬ"
//            };
//        }

//        private static BanModServerTexts Japanese()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - ロビーモードを選択",
//                SelectDescription =
//                    "ロビーを作成する前にモードを選択してください。",
//                ModdedDescription =
//                    "役職を追加したりゲームプレイを変更したりする場合は、このサーバーを使用してください。",
//                VanillaDescription =
//                    "アンチチートと見た目の変更だけを使用する場合は、このサーバーを使用してください。",
//                ModdedButton =
//                    "MODDED +25\n推奨",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "BanModを責任を持って使用し、作成するロビーに合ったモードを選択してください。",
//                VanillaTitle =
//                    "重要 - VANILLAモード",
//                VanillaIntro =
//                    "VANILLAモードを選択しました。",
//                VanillaWarning =
//                    "他のプレイヤーを困らせたり妨害したりするためにBanModを使用しないでください。また、ゲームプレイを変更したり、不公平な優位性を与えたり、他のプレイヤーの体験を変えたりするオプションを有効にしないでください。\n\n不適切、許可されていない、またはルールに反する使用は、Innerslothや関係する他のサービスによる警告、制限、停止、BAN、その他の措置につながる可能性があります。\n\nBanModおよび開発者は、Modの不適切、違法、または許可されていない使用によって生じた結果について責任を負いません。\n\nVANILLAモードと互換性のある機能のみを使用し、Among Usおよび利用する他のサービスのルールを守ってください。",
//                ConfirmButton =
//                    "同意する",
//                BackButton =
//                    "拒否する"
//            };
//        }

//        private static BanModServerTexts Korean()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - 로비 모드 선택",
//                SelectDescription =
//                    "로비를 만들기 전에 로비 모드를 선택하세요.",
//                ModdedDescription =
//                    "역할을 추가하고 게임플레이를 변경하려면 이 서버를 사용하세요.",
//                VanillaDescription =
//                    "안티치트와 시각적 변경만 사용하는 경우 이 서버를 사용하세요.",
//                ModdedButton =
//                    "MODDED +25\n권장",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "BanMod를 책임감 있게 사용하고 생성하려는 로비에 맞는 모드를 선택하세요.",
//                VanillaTitle =
//                    "중요 - VANILLA 모드",
//                VanillaIntro =
//                    "VANILLA 모드를 선택했습니다.",
//                VanillaWarning =
//                    "다른 플레이어를 괴롭히거나 방해하기 위해 BanMod를 사용하지 말고, 게임플레이를 변경하거나 부당한 이점을 제공하거나 다른 플레이어의 경험을 바꾸는 옵션을 활성화하지 마세요.\n\n부적절하거나 승인되지 않았거나 규칙을 위반하는 사용은 Innersloth 또는 관련 서비스에서 경고, 제한, 정지, 밴 또는 기타 제재를 받을 수 있습니다.\n\nBanMod와 개발자는 모드의 부적절하거나 불법적이거나 승인되지 않은 사용으로 인해 발생하는 결과에 대해 책임을 지지 않습니다.\n\nVANILLA 모드와 호환되는 기능만 사용하고 Among Us 및 사용하는 다른 서비스의 규칙을 준수하세요.",
//                ConfirmButton =
//                    "동의",
//                BackButton =
//                    "거부"
//            };
//        }

//        private static BanModServerTexts SimplifiedChinese()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - 选择大厅模式",
//                SelectDescription =
//                    "创建大厅前请选择大厅模式。",
//                ModdedDescription =
//                    "使用此服务器来添加角色并修改游戏玩法。",
//                VanillaDescription =
//                    "如果你只使用反作弊和视觉修改，请使用此服务器。",
//                ModdedButton =
//                    "MODDED +25\n推荐",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "请负责任地使用 BanMod，并选择与当前大厅类型相符的模式。",
//                VanillaTitle =
//                    "重要 - VANILLA 模式",
//                VanillaIntro =
//                    "你选择了 Vanilla 模式。",
//                VanillaWarning =
//                    "请不要使用 BanMod 骚扰或干扰其他玩家，也不要启用会修改游戏玩法、提供不公平优势或改变其他玩家游戏体验的选项。\n\n不当、未经授权或违反规则的使用可能导致 Innersloth 或其他相关服务发出警告、限制、暂停、封禁或其他处罚。\n\n对于因不当、违法或未经授权使用该 Mod 而产生的任何后果，BanMod 及其开发者不承担责任。\n\n请仅使用与 Vanilla 模式兼容的功能，并遵守 Among Us 及其他所使用服务的规则。",
//                ConfirmButton =
//                    "接受",
//                BackButton =
//                    "拒绝"
//            };
//        }

//        private static BanModServerTexts TraditionalChinese()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - 選擇大廳模式",
//                SelectDescription =
//                    "建立大廳前請選擇大廳模式。",
//                ModdedDescription =
//                    "使用此伺服器來新增角色並修改遊戲玩法。",
//                VanillaDescription =
//                    "如果你只使用反作弊和視覺修改，請使用此伺服器。",
//                ModdedButton =
//                    "MODDED +25\n推薦",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "請負責任地使用 BanMod，並選擇符合目前大廳類型的模式。",
//                VanillaTitle =
//                    "重要 - VANILLA 模式",
//                VanillaIntro =
//                    "你選擇了 Vanilla 模式。",
//                VanillaWarning =
//                    "請不要使用 BanMod 騷擾或干擾其他玩家，也不要啟用會修改遊戲玩法、提供不公平優勢或改變其他玩家遊戲體驗的選項。\n\n不當、未經授權或違反規則的使用可能導致 Innersloth 或其他相關服務發出警告、限制、暫停、封禁或其他處分。\n\n對於因不當、違法或未經授權使用此 Mod 而產生的任何後果，BanMod 及其開發者不承擔責任。\n\n請僅使用與 Vanilla 模式相容的功能，並遵守 Among Us 及其他使用中服務的規則。",
//                ConfirmButton =
//                    "接受",
//                BackButton =
//                    "拒絕"
//            };
//        }

//        private static BanModServerTexts Filipino()
//        {
//            return new BanModServerTexts
//            {
//                SelectTitle =
//                    "BANMOD - PILIIN ANG LOBBY MODE",
//                SelectDescription =
//                    "Piliin ang lobby mode bago gawin ang lobby.",
//                ModdedDescription =
//                    "Gamitin ang server na ito para magdagdag ng mga role at baguhin ang gameplay.",
//                VanillaDescription =
//                    "Gamitin ang server na ito kung anti-cheat at visual modifications lang ang ginagamit mo.",
//                ModdedButton =
//                    "MODDED +25\nINIREREKOMENDA",
//                VanillaButton =
//                    "VANILLA",
//                PrivateFooter =
//                    "Gamitin ang BanMod nang responsable at piliin ang mode na tumutugma sa lobby na iyong ginagawa.",
//                VanillaTitle =
//                    "MAHALAGA - VANILLA MODE",
//                VanillaIntro =
//                    "Pinili mo ang Vanilla mode.",
//                VanillaWarning =
//                    "Huwag gamitin ang BanMod para manggulo o mang-abala ng ibang manlalaro, at huwag i-enable ang mga option na nagbabago ng gameplay, nagbibigay ng hindi patas na advantage, o binabago ang experience ng ibang manlalaro.\n\nAng maling paggamit, hindi awtorisadong paggamit, o paggamit na labag sa rules ay maaaring magresulta sa warnings, restrictions, suspensions, bans, o iba pang sanctions mula sa Innersloth o iba pang serbisyong kasangkot.\n\nHindi mananagot ang BanMod at ang mga developer nito sa anumang kahihinatnan mula sa maling paggamit, ilegal, o hindi awtorisadong paggamit ng mod.\n\nGamitin lamang ang mga feature na compatible sa Vanilla mode at sundin ang rules ng Among Us at ng iba pang serbisyong ginagamit.",
//                ConfirmButton =
//                    "SUMASANG-AYON AKO",
//                BackButton =
//                    "HINDI AKO SUMASANG-AYON"
//            };
//        }

//    }

//    public class ServerSelectionMenu : MonoBehaviour
//    {
//        public static ServerSelectionMenu Instance;

//        private bool showMenu = false;
//        private bool showVanillaWarning = false;

//        private bool wasCreateScreenOpen = false;
//        private bool selectionShownForCurrentScreen = false;

//        private float createScreenOpenTime = -1f;

//        private Rect windowRect;
//        private Vector2 warningScroll = Vector2.zero;

//        private CreateGameOptions pendingCreateGame = null;

//        private GUIStyle windowStyle;
//        private GUIStyle titleStyle;
//        private GUIStyle headerStyle;
//        private GUIStyle textStyle;
//        private GUIStyle mutedStyle;
//        private GUIStyle buttonStyle;
//        private GUIStyle sectionStyle;
//        private GUIStyle warningStyle;

//        private Texture2D roundedWindowTexture;
//        private Texture2D roundedPanelTexture;
//        private Texture2D roundedButtonTexture;

//        private int lastScreenWidth;
//        private int lastScreenHeight;

//        private const float NormalWindowWidth = 760f;
//        private const float NormalWindowHeight = 620f;
//        private const float WarningWindowWidth = 860f;
//        private const float WarningWindowHeight = 720f;

//        public ServerSelectionMenu(IntPtr ptr) : base(ptr)
//        {
//        }

//        void Awake()
//        {
//            Instance = this;
//            lastScreenWidth = Screen.width;
//            lastScreenHeight = Screen.height;
//        }

//        void OnDestroy()
//        {
//            if (Instance == this)
//                Instance = null;

//            DestroyResources();
//        }

//        void Update()
//        {
//            if (BanMod.IsBanModDisabled)
//            {
//                CloseMenu();
//                return;
//            }

//            if (Screen.width != lastScreenWidth ||
//                Screen.height != lastScreenHeight)
//            {
//                lastScreenWidth = Screen.width;
//                lastScreenHeight = Screen.height;

//                if (showMenu)
//                    CenterWindow();
//            }

//            bool createScreenOpen = false;

//            try
//            {
//                GameObject createScreen =
//                    GameObject.Find("CreateGameScreen");

//                createScreenOpen =
//                    createScreen != null &&
//                    createScreen.activeInHierarchy;
//            }
//            catch
//            {
//                createScreenOpen = false;
//            }

//            if (createScreenOpen && !wasCreateScreenOpen)
//            {
//                wasCreateScreenOpen = true;
//                selectionShownForCurrentScreen = false;

//                createScreenOpenTime = Time.time;

//                BanModServerSelection.VanillaAcknowledged = false;
//                BanModServerSelection.VanillaCreateBypassOnce = false;

//            }

//            if (!createScreenOpen && wasCreateScreenOpen)
//            {
//                wasCreateScreenOpen = false;
//                selectionShownForCurrentScreen = false;

//                createScreenOpenTime = -1f;

//                CloseMenu();

//                BanModCreateGameServerSettingPatch
//                    .CloseModeDropdown();


//                return;
//            }

//            if (!createScreenOpen)
//                return;

//            BanModCreateGameServerSettingPatch.Refresh();

//            if (selectionShownForCurrentScreen)
//                return;

//            if (createScreenOpenTime < 0f)
//                return;

//            if (Time.time - createScreenOpenTime < 0.35f)
//                return;

//            selectionShownForCurrentScreen = true;

//            try
//            {
//                GameObject createScreen =
//                    GameObject.Find("CreateGameScreen");

//                if (createScreen == null)
//                {
//                    return;
//                }

//                CreateGameOptions options =
//                    createScreen.GetComponent<CreateGameOptions>();

//                if (options == null)
//                {
//                    options =
//                        createScreen.GetComponentInChildren<CreateGameOptions>(
//                            true
//                        );
//                }

//                if (options == null)
//                {
//                    return;
//                }


//                BanModCreateGameServerSettingPatch.EnsureFor(
//                    options
//                );
//            }
//            catch
//            {
//            }
//        }

//        public void OpenMenu()
//        {
//            showVanillaWarning = false;
//            warningScroll = Vector2.zero;
//            pendingCreateGame = null;
//            showMenu = true;

//            CenterWindow();

//        }

//        public void OpenVanillaCreateWarning(
//            CreateGameOptions createGameOptions)
//        {
//            if (createGameOptions == null)
//                return;

//            pendingCreateGame =
//                createGameOptions;

//            showVanillaWarning = true;
//            warningScroll = Vector2.zero;
//            showMenu = true;

//            CenterWindow();

//        }

//        public void CloseMenu()
//        {
//            showMenu = false;
//            showVanillaWarning = false;
//            warningScroll = Vector2.zero;
//            pendingCreateGame = null;
//        }

//        public bool IsOpen()
//        {
//            return showMenu;
//        }

//        private void CenterWindow()
//        {
//            float targetWidth = showVanillaWarning
//                ? WarningWindowWidth
//                : NormalWindowWidth;

//            float targetHeight = showVanillaWarning
//                ? WarningWindowHeight
//                : NormalWindowHeight;

//            float width = Mathf.Min(
//                targetWidth,
//                Mathf.Max(500f, Screen.width - 30f)
//            );

//            float height = Mathf.Min(
//                targetHeight,
//                Mathf.Max(420f, Screen.height - 30f)
//            );

//            windowRect = new Rect(
//                (Screen.width - width) * 0.5f,
//                (Screen.height - height) * 0.5f,
//                width,
//                height
//            );
//        }

//        private void DestroyResources()
//        {
//            DestroyTexture(ref roundedWindowTexture);
//            DestroyTexture(ref roundedPanelTexture);
//            DestroyTexture(ref roundedButtonTexture);

//            windowStyle = null;
//            titleStyle = null;
//            headerStyle = null;
//            textStyle = null;
//            mutedStyle = null;
//            buttonStyle = null;
//            sectionStyle = null;
//            warningStyle = null;
//        }

//        private void DestroyTexture(ref Texture2D texture)
//        {
//            if (texture == null)
//                return;

//            try
//            {
//                UnityEngine.Object.Destroy(texture);
//            }
//            catch
//            {
//            }

//            texture = null;
//        }

//        private void EnsureStyles()
//        {
//            if (roundedWindowTexture == null)
//            {
//                roundedWindowTexture = CreateRoundedTexture(
//                    64,
//                    18,
//                    new Color(0.055f, 0.06f, 0.075f, 0.985f)
//                );

//                roundedPanelTexture = CreateRoundedTexture(
//                    48,
//                    14,
//                    new Color(0.095f, 0.105f, 0.13f, 0.98f)
//                );

//                roundedButtonTexture = CreateRoundedTexture(
//                    40,
//                    12,
//                    Color.white
//                );
//            }

//            if (windowStyle == null)
//            {
//                windowStyle = new GUIStyle(GUI.skin.window);
//                windowStyle.normal.background = roundedWindowTexture;
//                windowStyle.onNormal.background = roundedWindowTexture;
//                windowStyle.active.background = roundedWindowTexture;
//                windowStyle.focused.background = roundedWindowTexture;
//                windowStyle.normal.textColor = Color.white;
//                windowStyle.border = new RectOffset
//                {
//                    left = 18,
//                    right = 18,
//                    top = 18,
//                    bottom = 18
//                };
//                windowStyle.padding = new RectOffset
//                {
//                    left = 14,
//                    right = 14,
//                    top = 14,
//                    bottom = 14
//                };
//            }

//            if (titleStyle == null)
//            {
//                titleStyle = new GUIStyle(GUI.skin.label);
//                titleStyle.fontSize = 27;
//                titleStyle.fontStyle = FontStyle.Bold;
//                titleStyle.alignment = TextAnchor.MiddleLeft;
//                titleStyle.wordWrap = true;
//                titleStyle.normal.textColor = new Color(0.35f, 0.82f, 1f, 1f);
//            }

//            if (headerStyle == null)
//            {
//                headerStyle = new GUIStyle(GUI.skin.label);
//                headerStyle.fontSize = 19;
//                headerStyle.fontStyle = FontStyle.Bold;
//                headerStyle.alignment = TextAnchor.MiddleLeft;
//                headerStyle.wordWrap = true;
//                headerStyle.normal.textColor =
//                    new Color(0.93f, 0.95f, 1f, 1f);
//            }

//            if (textStyle == null)
//            {
//                textStyle = new GUIStyle(GUI.skin.label);
//                textStyle.fontSize = 16;
//                textStyle.alignment = TextAnchor.UpperLeft;
//                textStyle.wordWrap = true;
//                textStyle.normal.textColor =
//                    new Color(0.88f, 0.94f, 1f, 1f);
//            }

//            if (mutedStyle == null)
//            {
//                mutedStyle = new GUIStyle(textStyle);
//                mutedStyle.fontSize = 14;
//                mutedStyle.normal.textColor =
//                    new Color(0.68f, 0.76f, 0.86f, 1f);
//            }

//            if (warningStyle == null)
//            {
//                warningStyle = new GUIStyle(textStyle);
//                warningStyle.fontSize = 16;
//                warningStyle.fontStyle = FontStyle.Bold;
//                warningStyle.normal.textColor =
//                    new Color(1f, 0.74f, 0.22f, 1f);
//            }

//            if (buttonStyle == null)
//            {
//                buttonStyle = new GUIStyle(GUI.skin.button);
//                buttonStyle.fontSize = 17;
//                buttonStyle.fontStyle = FontStyle.Bold;
//                buttonStyle.alignment = TextAnchor.MiddleCenter;
//                buttonStyle.wordWrap = true;

//                buttonStyle.normal.background = roundedButtonTexture;
//                buttonStyle.hover.background = roundedButtonTexture;
//                buttonStyle.active.background = roundedButtonTexture;
//                buttonStyle.focused.background = roundedButtonTexture;
//                buttonStyle.onNormal.background = roundedButtonTexture;
//                buttonStyle.onHover.background = roundedButtonTexture;
//                buttonStyle.onActive.background = roundedButtonTexture;
//                buttonStyle.onFocused.background = roundedButtonTexture;

//                buttonStyle.normal.textColor = Color.white;
//                buttonStyle.hover.textColor = Color.white;
//                buttonStyle.active.textColor = Color.white;
//                buttonStyle.focused.textColor = Color.white;

//                buttonStyle.border = new RectOffset
//                {
//                    left = 12,
//                    right = 12,
//                    top = 12,
//                    bottom = 12
//                };

//                buttonStyle.padding = new RectOffset
//                {
//                    left = 12,
//                    right = 12,
//                    top = 6,
//                    bottom = 6
//                };
//            }

//            if (sectionStyle == null)
//            {
//                sectionStyle = new GUIStyle(GUI.skin.box);
//                sectionStyle.normal.background = roundedPanelTexture;
//                sectionStyle.normal.textColor = Color.white;
//                sectionStyle.border = new RectOffset
//                {
//                    left = 14,
//                    right = 14,
//                    top = 14,
//                    bottom = 14
//                };
//                sectionStyle.padding = new RectOffset
//                {
//                    left = 12,
//                    right = 12,
//                    top = 12,
//                    bottom = 12
//                };
//            }
//        }

//        private Texture2D CreateRoundedTexture(
//            int size,
//            int radius,
//            Color color)
//        {
//            Texture2D texture =
//                new Texture2D(
//                    size,
//                    size,
//                    TextureFormat.RGBA32,
//                    false
//                );

//            texture.hideFlags = HideFlags.DontUnloadUnusedAsset;
//            texture.wrapMode = TextureWrapMode.Clamp;
//            texture.filterMode = FilterMode.Bilinear;

//            Color32[] pixels = new Color32[size * size];
//            Color32 fill = color;

//            float r = Mathf.Max(1f, radius);
//            float leftCenter = r - 0.5f;
//            float rightCenter = size - r - 0.5f;
//            float bottomCenter = r - 0.5f;
//            float topCenter = size - r - 0.5f;

//            for (int y = 0; y < size; y++)
//            {
//                for (int x = 0; x < size; x++)
//                {
//                    float cx = x;
//                    float cy = y;

//                    if (x < radius)
//                        cx = leftCenter;
//                    else if (x >= size - radius)
//                        cx = rightCenter;

//                    if (y < radius)
//                        cy = bottomCenter;
//                    else if (y >= size - radius)
//                        cy = topCenter;

//                    float dx = x - cx;
//                    float dy = y - cy;
//                    float distance = Mathf.Sqrt(dx * dx + dy * dy);

//                    float alphaFactor = 1f;

//                    bool inCorner =
//                        (x < radius || x >= size - radius) &&
//                        (y < radius || y >= size - radius);

//                    if (inCorner)
//                        alphaFactor =
//                            Mathf.Clamp01(r + 0.5f - distance);

//                    Color32 pixel = fill;
//                    pixel.a =
//                        (byte)Mathf.RoundToInt(
//                            fill.a * alphaFactor
//                        );

//                    pixels[y * size + x] = pixel;
//                }
//            }

//            texture.SetPixels32(pixels);
//            texture.Apply(false, false);
//            return texture;
//        }

//        void OnGUI()
//        {
//            if (!showMenu)
//                return;

//            if (BanMod.IsBanModDisabled)
//                return;

//            EnsureStyles();

//            Color oldColor = GUI.color;
//            Color oldBackground = GUI.backgroundColor;
//            Color oldContent = GUI.contentColor;
//            int oldDepth = GUI.depth;

//            try
//            {
//                GUI.depth = -1000;
//                GUI.color = Color.white;
//                GUI.backgroundColor = Color.white;
//                GUI.contentColor = Color.white;

//                if (showVanillaWarning)
//                {
//                    windowRect = GUI.Window(
//                        42101,
//                        windowRect,
//                        (GUI.WindowFunction)DrawVanillaWarning,
//                        "",
//                        windowStyle
//                    );
//                }
//                else
//                {
//                    windowRect = GUI.Window(
//                        42100,
//                        windowRect,
//                        (GUI.WindowFunction)DrawServerSelection,
//                        "",
//                        windowStyle
//                    );
//                }
//            }
//            catch
//            {
//            }
//            finally
//            {
//                GUI.color = oldColor;
//                GUI.backgroundColor = oldBackground;
//                GUI.contentColor = oldContent;
//                GUI.depth = oldDepth;
//            }
//        }

//        private void DrawServerSelection(int id)
//        {
//            BanModServerTexts t =
//                BanModServerLocalization.Get();

//            float width = windowRect.width;
//            float height = windowRect.height;

//            GUI.Label(
//                new Rect(20f, 10f, width - 40f, 42f),
//                t.SelectTitle,
//                titleStyle
//            );

//            Rect body = new Rect(
//                18f,
//                64f,
//                width - 36f,
//                height - 82f
//            );

//            GUI.Label(
//                new Rect(body.x + 4f, body.y, body.width - 8f, 48f),
//                t.SelectDescription,
//                mutedStyle
//            );

//            float footerHeight = 64f;
//            float cardsTop = body.y + 56f;
//            float cardsBottom = body.y + body.height - footerHeight;
//            float gap = 12f;
//            float cardHeight = (cardsBottom - cardsTop - gap) * 0.5f;

//            Rect moddedCard = new Rect(
//                body.x,
//                cardsTop,
//                body.width,
//                cardHeight
//            );

//            Rect vanillaCard = new Rect(
//                body.x,
//                cardsTop + cardHeight + gap,
//                body.width,
//                cardHeight
//            );

//            DrawPanel(moddedCard);
//            DrawPanel(vanillaCard);

//            GUIStyle moddedHeaderStyle = new GUIStyle(headerStyle);
//            moddedHeaderStyle.normal.textColor =
//                new Color(0.38f, 1f, 0.62f, 1f);

//            GUI.Label(
//                new Rect(
//                    moddedCard.x + 18f,
//                    moddedCard.y + 16f,
//                    moddedCard.width - 36f,
//                    56f
//                ),
//                t.ModdedDescription,
//                moddedHeaderStyle
//            );

//            if (DrawTintedButton(
//                new Rect(
//                    moddedCard.x + 18f,
//                    moddedCard.y + moddedCard.height - 72f,
//                    moddedCard.width - 36f,
//                    54f
//                ),
//                t.ModdedButton,
//                new Color(0.12f, 0.58f, 0.34f, 1f)))
//            {
//                BanModServerSelection.Mode =
//                    BanModServerMode.Modded25;

//                BanModServerSelection.VanillaAcknowledged =
//                    false;

//                BanModCreateGameServerSettingPatch.Refresh();


//                CloseMenu();
//            }

//            GUIStyle vanillaHeaderStyle = new GUIStyle(headerStyle);
//            vanillaHeaderStyle.normal.textColor =
//                new Color(1f, 0.72f, 0.30f, 1f);

//            GUI.Label(
//                new Rect(
//                    vanillaCard.x + 18f,
//                    vanillaCard.y + 16f,
//                    vanillaCard.width - 36f,
//                    56f
//                ),
//                t.VanillaDescription,
//                vanillaHeaderStyle
//            );

//            if (DrawTintedButton(
//                new Rect(
//                    vanillaCard.x + 18f,
//                    vanillaCard.y + vanillaCard.height - 72f,
//                    vanillaCard.width - 36f,
//                    54f
//                ),
//                t.VanillaButton,
//                new Color(0.76f, 0.42f, 0.12f, 1f)))
//            {
//                BanModServerSelection.Mode =
//                    BanModServerMode.Vanilla;

//                BanModServerSelection.VanillaAcknowledged =
//                    false;

//                BanModCreateGameServerSettingPatch.Refresh();


//                CloseMenu();
//            }

//            GUIStyle footerStyle = new GUIStyle(warningStyle);
//            footerStyle.alignment = TextAnchor.MiddleCenter;
//            footerStyle.fontSize = 14;
//            footerStyle.normal.textColor =
//                new Color(1f, 0.82f, 0.38f, 1f);

//            GUI.Label(
//                new Rect(
//                    body.x + 10f,
//                    body.y + body.height - 54f,
//                    body.width - 20f,
//                    46f
//                ),
//                t.PrivateFooter,
//                footerStyle
//            );

//            GUI.DragWindow(
//                new Rect(0f, 0f, width, 50f)
//            );
//        }

//        private void DrawVanillaWarning(int id)
//        {
//            BanModServerTexts t =
//                BanModServerLocalization.Get();

//            float width = windowRect.width;
//            float height = windowRect.height;

//            GUI.Label(
//                new Rect(20f, 10f, width - 40f, 42f),
//                t.VanillaTitle,
//                titleStyle
//            );

//            Rect body = new Rect(
//                18f,
//                64f,
//                width - 36f,
//                height - 82f
//            );

//            float footerHeight = 68f;

//            Rect warningPanel = new Rect(
//                body.x,
//                body.y,
//                body.width,
//                body.height - footerHeight - 12f
//            );

//            DrawPanel(warningPanel);

//            GUI.Label(
//                new Rect(
//                    warningPanel.x + 18f,
//                    warningPanel.y + 14f,
//                    warningPanel.width - 36f,
//                    36f
//                ),
//                t.VanillaIntro,
//                warningStyle
//            );

//            Rect scrollArea = new Rect(
//                warningPanel.x + 14f,
//                warningPanel.y + 56f,
//                warningPanel.width - 28f,
//                warningPanel.height - 70f
//            );

//            float textWidth = Mathf.Max(100f, scrollArea.width - 24f);
//            float contentHeight = Mathf.Max(
//                scrollArea.height,
//                textStyle.CalcHeight(
//                    new GUIContent(t.VanillaWarning),
//                    textWidth
//                ) + 18f
//            );

//            Rect contentRect = new Rect(
//                0f,
//                0f,
//                textWidth,
//                contentHeight
//            );

//            warningScroll = GUI.BeginScrollView(
//                scrollArea,
//                warningScroll,
//                contentRect
//            );

//            GUI.Label(
//                new Rect(
//                    4f,
//                    4f,
//                    textWidth - 8f,
//                    contentHeight - 8f
//                ),
//                t.VanillaWarning,
//                textStyle
//            );

//            GUI.EndScrollView();

//            Rect footer = new Rect(
//                body.x,
//                body.y + body.height - footerHeight,
//                body.width,
//                footerHeight
//            );

//            float half = (footer.width - 10f) * 0.5f;

//            if (DrawTintedButton(
//                new Rect(
//                    footer.x,
//                    footer.y + 8f,
//                    half,
//                    48f
//                ),
//                t.ConfirmButton,
//                new Color(0.12f, 0.58f, 0.34f, 1f)))
//            {
//                CreateGameOptions createGame =
//                    pendingCreateGame;

//                BanModServerSelection.Mode =
//                    BanModServerMode.Vanilla;

//                BanModServerSelection.SaveVanillaAcceptance();

//                BanModServerSelection.VanillaCreateBypassOnce =
//                    true;

//                BanModCreateGameServerSettingPatch.Refresh();


//                CloseMenu();

//                ResumeCreateGame(
//                    createGame
//                );
//            }

//            if (DrawTintedButton(
//                new Rect(
//                    footer.x + half + 10f,
//                    footer.y + 8f,
//                    half,
//                    48f
//                ),
//                t.BackButton,
//                new Color(0.18f, 0.19f, 0.23f, 1f)))
//            {
//                CreateGameOptions createGame =
//                    pendingCreateGame;

//                BanModServerSelection.Mode =
//                    BanModServerMode.Modded25;

//                BanModServerSelection.VanillaAcknowledged =
//                    false;

//                BanModServerSelection.VanillaCreateBypassOnce =
//                    false;

//                BanModCreateGameServerSettingPatch.Refresh();


//                CloseMenu();

//                ResumeCreateGame(
//                    createGame
//                );
//            }

//            GUI.DragWindow(
//                new Rect(0f, 0f, width, 50f)
//            );
//        }

//        private void ResumeCreateGame(
//            CreateGameOptions createGame)
//        {
//            if (createGame == null)
//            {
//                return;
//            }

//            try
//            {
//                createGame.Confirm();
//            }
//            catch
//            {
//            }
//        }

//        private bool DrawTintedButton(
//            Rect rect,
//            string text,
//            Color color)
//        {
//            Color oldBackground = GUI.backgroundColor;
//            GUI.backgroundColor = color;

//            bool clicked = GUI.Button(
//                rect,
//                text,
//                buttonStyle
//            );

//            GUI.backgroundColor = oldBackground;
//            return clicked;
//        }

//        private void DrawPanel(Rect rect)
//        {
//            Color oldBackground = GUI.backgroundColor;
//            GUI.backgroundColor = Color.white;

//            GUI.Box(
//                rect,
//                GUIContent.none,
//                sectionStyle
//            );

//            GUI.backgroundColor = oldBackground;
//        }
//    }

//    [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Show))]
//    public static class BanModCreateGameServerSettingPatch
//    {
//        private static CreateGameOptions currentCreateScreen;

//        private static GameObject modeRowRoot;

//        private static GameObject serverLabelRoot;

//        private static PassiveButton vanillaButton;
//        private static PassiveButton moddedButton;

//        private static TMP_Text serverHoverDescriptionText;
//        private static string serverHoverOriginalText = "";
//        private static bool serverHoverTextCaptured = false;

//        private const float YOffset = 0.72f;
//        private const float ExtendedScrollMax = 4.0f;

//        [HarmonyPostfix]
//        public static void Postfix(
//            CreateGameOptions __instance)
//        {

//            EnsureFor(
//                __instance
//            );
//        }

//        public static void EnsureFor(
//            CreateGameOptions createScreen)
//        {
//            if (createScreen == null)
//            {
//                return;
//            }

//            if (BanMod.IsBanModDisabled)
//                return;

//            if (BanModServerSelection.Mode ==
//                BanModServerMode.None)
//            {
//                BanModServerSelection.Mode =
//                    BanModServerMode.Modded25;
//            }

//            try
//            {
//                bool screenChanged =
//                    currentCreateScreen != null &&
//                    currentCreateScreen != createScreen;

//                currentCreateScreen =
//                    createScreen;

//                if (screenChanged)
//                {
//                    DestroyServerUi();
//                }

//                ExtendScroll(
//                    createScreen
//                );

//                if (!IsAlive(modeRowRoot) ||
//                    vanillaButton == null ||
//                    moddedButton == null)
//                {
//                    DestroyServerUi();


//                    BuildServerRow(
//                        createScreen
//                    );
//                }
//                else
//                {
//                    SetRowActive(
//                        true
//                    );
//                }

//                Refresh();
//            }
//            catch
//            {
//            }
//        }

//        private static void BuildServerRow(
//            CreateGameOptions createScreen)
//        {
//            GameObject regionButton =
//                TryGetRuntimeMember<GameObject>(
//                    createScreen,
//                    "serverButton"
//                );

//            if (regionButton == null)
//            {
//                regionButton =
//                    FindRegionButtonFromHierarchy(
//                        createScreen
//                    );
//            }

//            if (regionButton == null)
//            {

//                LogUsefulButtons(
//                    createScreen
//                );

//                return;
//            }


//            List<PassiveButton> originalModeButtons =
//                TryGetRuntimePassiveButtonCollection(
//                    createScreen,
//                    "modeButtons"
//                );

//            if (originalModeButtons.Count < 2)
//            {
//                originalModeButtons =
//                    FindModeButtonsFromHierarchy(
//                        createScreen
//                    );
//            }

//            if (originalModeButtons.Count < 2)
//            {

//                LogUsefulButtons(
//                    createScreen
//                );

//                return;
//            }

//            PassiveButton originalLeft =
//                originalModeButtons[0];

//            PassiveButton originalRight =
//                originalModeButtons[1];

//            if (originalLeft == null ||
//                originalRight == null)
//            {

//                return;
//            }

//            serverHoverDescriptionText =
//                FindServerHoverDescriptionText(
//                    createScreen,
//                    originalLeft,
//                    originalRight
//                );

//            if (serverHoverDescriptionText != null)
//            {
//                serverHoverOriginalText =
//                    serverHoverDescriptionText.text ?? "";

//                serverHoverTextCaptured =
//                    true;

//            }
//            else
//            {
//                serverHoverOriginalText = "";
//                serverHoverTextCaptured = false;

//            }

//            Transform buttonParent =
//                originalLeft.transform.parent;

//            if (buttonParent == null)
//            {

//                return;
//            }

//            Transform regionParent =
//                regionButton.transform.parent;

//            if (regionParent == null)
//                return;

//            Vector3 regionTargetLocal =
//                regionButton.transform.localPosition;

//            regionTargetLocal.y -=
//                YOffset;

//            Vector3 targetWorld =
//                regionParent.TransformPoint(
//                    regionTargetLocal
//                );

//            float targetYInModeParent =
//                buttonParent.InverseTransformPoint(
//                    targetWorld
//                ).y;

//            modeRowRoot =
//                new GameObject(
//                    "BanModServerChoiceRow"
//                );

//            modeRowRoot.transform.SetParent(
//                buttonParent,
//                false
//            );

//            modeRowRoot.transform.localPosition =
//                Vector3.zero;

//            modeRowRoot.transform.localRotation =
//                Quaternion.identity;

//            modeRowRoot.transform.localScale =
//                Vector3.one;

//            GameObject vanillaObject =
//                CloneModeButton(
//                    originalLeft,
//                    modeRowRoot.transform,
//                    targetYInModeParent,
//                    "BanModServerVanilla",
//                    "Vanilla",
//                    true,
//                    SelectVanilla
//                );

//            GameObject moddedObject =
//                CloneModeButton(
//                    originalRight,
//                    modeRowRoot.transform,
//                    targetYInModeParent,
//                    "BanModServerModded",
//                    "Modded",
//                    false,
//                    SelectModded
//                );

//            if (!IsAlive(vanillaObject) ||
//                !IsAlive(moddedObject))
//            {

//                DestroyServerUi();
//                return;
//            }

//            vanillaButton =
//                vanillaObject.GetComponent<PassiveButton>();

//            if (vanillaButton == null)
//            {
//                vanillaButton =
//                    vanillaObject
//                        .GetComponentInChildren<PassiveButton>(
//                            true
//                        );
//            }

//            moddedButton =
//                moddedObject.GetComponent<PassiveButton>();

//            if (moddedButton == null)
//            {
//                moddedButton =
//                    moddedObject
//                        .GetComponentInChildren<PassiveButton>(
//                            true
//                        );
//            }

//            if (vanillaButton == null ||
//                moddedButton == null)
//            {

//                DestroyServerUi();
//                return;
//            }

//            serverLabelRoot =
//                CloneServerLabel(
//                    createScreen,
//                    regionButton
//                );

//            modeRowRoot.SetActive(
//                true
//            );

//            Refresh();

//        }

//        private static GameObject CloneModeButton(
//            PassiveButton original,
//            Transform newParent,
//            float targetY,
//            string objectName,
//            string displayText,
//            bool isVanilla,
//            Action onClick)
//        {
//            if (original == null ||
//                newParent == null)
//                return null;

//            GameObject clone =
//                UnityEngine.Object.Instantiate(
//                    original.gameObject,
//                    newParent
//                );

//            clone.name =
//                objectName;

//            clone.SetActive(
//                true
//            );

//            Vector3 local =
//                original.transform.localPosition;

//            local.y =
//                targetY;

//            clone.transform.localPosition =
//                local;

//            clone.transform.localRotation =
//                original.transform.localRotation;

//            clone.transform.localScale =
//                original.transform.localScale;

//            DisableLocalizationComponents(
//                clone
//            );

//            TMP_Text[] texts =
//                clone.GetComponentsInChildren<TMP_Text>(
//                    true
//                );

//            if (texts != null)
//            {
//                for (int i = 0;
//                     i < texts.Length;
//                     i++)
//                {
//                    TMP_Text tmp =
//                        texts[i];

//                    if (tmp == null)
//                        continue;

//                    tmp.text =
//                        displayText;
//                }
//            }

//            PassiveButton[] buttons =
//                clone.GetComponentsInChildren<PassiveButton>(
//                    true
//                );

//            if (buttons == null ||
//                buttons.Length == 0)
//            {
//                UnityEngine.Object.Destroy(
//                    clone
//                );

//                return null;
//            }

//            for (int i = 0;
//                 i < buttons.Length;
//                 i++)
//            {
//                PassiveButton button =
//                    buttons[i];

//                if (button == null)
//                    continue;

//                try
//                {
//                    button.OnClick =
//                        new UnityEngine.UI.Button.ButtonClickedEvent();

//                    button.OnClick.AddListener(
//                        onClick
//                    );

//                    BindServerHoverEvents(
//                        button,
//                        isVanilla
//                    );
//                }
//                catch
//                {
//                }
//            }

//            return clone;
//        }

//        private static void BindServerHoverEvents(
//            PassiveButton button,
//            bool isVanilla)
//        {
//            if (button == null)
//                return;

//            try
//            {


//                button.OnMouseOver =
//                    new UnityEngine.Events.UnityEvent();

//                button.OnMouseOut =
//                    new UnityEngine.Events.UnityEvent();

//                button.OnMouseOver.AddListener(
//                    (UnityEngine.Events.UnityAction)(() =>
//                    {
//                        ShowServerHoverDescription(
//                            isVanilla
//                        );
//                    })
//                );

//                button.OnMouseOut.AddListener(
//                    (UnityEngine.Events.UnityAction)(() =>
//                    {
//                        RestoreServerHoverDescription();
//                    })
//                );

//            }
//            catch
//            {
//            }
//        }

//        private static void ShowServerHoverDescription(
//            bool isVanilla)
//        {
//            if (serverHoverDescriptionText == null)
//                return;

//            try
//            {


//                serverHoverOriginalText =
//                    serverHoverDescriptionText.text ?? "";

//                serverHoverTextCaptured =
//                    true;

//                BanModServerTexts texts =
//                    BanModServerLocalization.Get();

//                serverHoverDescriptionText.text =
//                    isVanilla
//                        ? texts.VanillaDescription
//                        : texts.ModdedDescription;
//            }
//            catch
//            {
//            }
//        }

//        private static void RestoreServerHoverDescription()
//        {
//            if (serverHoverDescriptionText == null ||
//                !serverHoverTextCaptured)
//                return;

//            try
//            {
//                serverHoverDescriptionText.text =
//                    serverHoverOriginalText;
//            }
//            catch
//            {
//            }
//        }

//        private static TMP_Text FindServerHoverDescriptionText(
//            CreateGameOptions createScreen,
//            PassiveButton originalLeft,
//            PassiveButton originalRight)
//        {
//            if (createScreen == null)
//                return null;


//            TMP_Text detected =
//                DetectDescriptionTextFromOriginalHover(
//                    createScreen,
//                    originalLeft
//                );

//            if (detected == null)
//            {
//                detected =
//                    DetectDescriptionTextFromOriginalHover(
//                        createScreen,
//                        originalRight
//                    );
//            }

//            if (detected != null)
//            {

//                return detected;
//            }


//            return FindServerHoverDescriptionTextFallback(
//                createScreen,
//                originalLeft,
//                originalRight
//            );
//        }

//        private static TMP_Text DetectDescriptionTextFromOriginalHover(
//            CreateGameOptions createScreen,
//            PassiveButton originalButton)
//        {
//            if (createScreen == null ||
//                originalButton == null)
//                return null;

//            TMP_Text[] allTexts =
//                createScreen.GetComponentsInChildren<TMP_Text>(
//                    true
//                );

//            if (allTexts == null ||
//                allTexts.Length == 0)
//                return null;

//            string[] before =
//                new string[allTexts.Length];

//            for (int i = 0; i < allTexts.Length; i++)
//            {
//                TMP_Text text =
//                    allTexts[i];

//                before[i] =
//                    text != null
//                        ? (text.text ?? "")
//                        : "";
//            }

//            try
//            {
//                if (originalButton.OnMouseOver == null)
//                    return null;

//                originalButton.OnMouseOver.Invoke();

//                TMP_Text best = null;
//                float bestScore = float.MaxValue;

//                Vector3 buttonLocal =
//                    createScreen.transform.InverseTransformPoint(
//                        originalButton.transform.position
//                    );

//                for (int i = 0; i < allTexts.Length; i++)
//                {
//                    TMP_Text candidate =
//                        allTexts[i];

//                    if (candidate == null)
//                        continue;

//                    if (candidate.transform.IsChildOf(
//                            originalButton.transform
//                        ))
//                    {
//                        continue;
//                    }

//                    string oldValue =
//                        before[i] ?? "";

//                    string newValue =
//                        candidate.text ?? "";

//                    if (oldValue == newValue)
//                        continue;


//                    if (newValue.Length < 8 &&
//                        oldValue.Length < 8)
//                        continue;

//                    string hierarchy =
//                        BuildHierarchyName(
//                            candidate.transform,
//                            createScreen.transform
//                        );

//                    Vector3 local =
//                        createScreen.transform.InverseTransformPoint(
//                            candidate.transform.position
//                        );

//                    float score = 0f;


//                    if (local.y > buttonLocal.y)
//                        score -= 3000f;
//                    else
//                        score += 3000f;

//                    score +=
//                        Mathf.Abs(local.x - buttonLocal.x) * 30f;

//                    score +=
//                        Mathf.Abs(local.y - buttonLocal.y) * 80f;

//                    if (ContainsIgnoreCase(hierarchy, "desc") ||
//                        ContainsIgnoreCase(hierarchy, "description"))
//                    {
//                        score -= 1200f;
//                    }

//                    if (ContainsIgnoreCase(hierarchy, "chat"))
//                        score += 8000f;

//                    if (ContainsIgnoreCase(hierarchy, "title") ||
//                        ContainsIgnoreCase(hierarchy, "label"))
//                    {
//                        score += 1000f;
//                    }

//                    if (oldValue.Length >= 15)
//                        score -= 300f;

//                    if (newValue.Length >= 15)
//                        score -= 300f;


//                    if (score < bestScore)
//                    {
//                        bestScore = score;
//                        best = candidate;
//                    }
//                }

//                return best;
//            }
//            catch
//            {

//                return null;
//            }
//            finally
//            {
//                try
//                {
//                    if (originalButton.OnMouseOut != null)
//                        originalButton.OnMouseOut.Invoke();
//                }
//                catch
//                {
//                }
//            }
//        }

//        private static TMP_Text FindServerHoverDescriptionTextFallback(
//            CreateGameOptions createScreen,
//            PassiveButton originalLeft,
//            PassiveButton originalRight)
//        {
//            if (createScreen == null)
//                return null;

//            string[] runtimeNames =
//            {
//                "modeDescriptionText",
//                "gameModeDescriptionText",
//                "gameTypeDescriptionText",
//                "descriptionText",
//                "modeDescription",
//                "gameModeDescription",
//                "gameTypeDescription"
//            };

//            for (int i = 0;
//                 i < runtimeNames.Length;
//                 i++)
//            {
//                TMP_Text runtimeText =
//                    TryGetRuntimeMember<TMP_Text>(
//                        createScreen,
//                        runtimeNames[i]
//                    );

//                if (runtimeText != null)
//                    return runtimeText;
//            }

//            TMP_Text[] allTexts =
//                createScreen.GetComponentsInChildren<TMP_Text>(
//                    true
//                );

//            if (allTexts == null ||
//                allTexts.Length == 0)
//                return null;

//            Vector3 leftLocal =
//                createScreen.transform.InverseTransformPoint(
//                    originalLeft.transform.position
//                );

//            Vector3 rightLocal =
//                createScreen.transform.InverseTransformPoint(
//                    originalRight.transform.position
//                );

//            float buttonCenterX =
//                (leftLocal.x + rightLocal.x) * 0.5f;

//            float buttonY =
//                (leftLocal.y + rightLocal.y) * 0.5f;

//            TMP_Text best = null;
//            float bestScore = float.MaxValue;

//            for (int i = 0;
//                 i < allTexts.Length;
//                 i++)
//            {
//                TMP_Text candidate =
//                    allTexts[i];

//                if (candidate == null)
//                    continue;

//                if ((originalLeft != null &&
//                     candidate.transform.IsChildOf(
//                         originalLeft.transform
//                     )) ||
//                    (originalRight != null &&
//                     candidate.transform.IsChildOf(
//                         originalRight.transform
//                     )))
//                {
//                    continue;
//                }

//                string value =
//                    candidate.text ?? "";

//                if (string.IsNullOrWhiteSpace(value))
//                    continue;

//                if (value.Length < 8)
//                    continue;

//                string hierarchy =
//                    BuildHierarchyName(
//                        candidate.transform,
//                        createScreen.transform
//                    );


//                if (ContainsIgnoreCase(hierarchy, "chat"))
//                    continue;

//                Vector3 local =
//                    createScreen.transform.InverseTransformPoint(
//                        candidate.transform.position
//                    );

//                float xDistance =
//                    Mathf.Abs(
//                        local.x - buttonCenterX
//                    );

//                float yDistance =
//                    Mathf.Abs(
//                        local.y - buttonY
//                    );

//                float score =
//                    yDistance * 1000f +
//                    xDistance * 100f;

//                if (ContainsIgnoreCase(hierarchy, "description") ||
//                    ContainsIgnoreCase(hierarchy, "desc"))
//                {
//                    score -= 5000f;
//                }

//                if (ContainsIgnoreCase(hierarchy, "mode") ||
//                    ContainsIgnoreCase(hierarchy, "game"))
//                {
//                    score -= 500f;
//                }

//                if (ContainsIgnoreCase(hierarchy, "title") ||
//                    ContainsIgnoreCase(hierarchy, "label"))
//                {
//                    score += 1200f;
//                }

//                if (local.y > buttonY)
//                    score -= 300f;
//                else
//                    score += 800f;

//                if (value.Length >= 20)
//                    score -= 250f;

//                if (value.Length > 180)
//                    score += 1200f;

//                if (score < bestScore)
//                {
//                    bestScore = score;
//                    best = candidate;
//                }
//            }

//            return best;
//        }

//        private static void DisableLocalizationComponents(
//            GameObject root)
//        {
//            if (!IsAlive(root))
//                return;

//            Component[] components =
//                root.GetComponentsInChildren<Component>(
//                    true
//                );

//            if (components == null)
//                return;

//            for (int i = 0;
//                 i < components.Length;
//                 i++)
//            {
//                Component component =
//                    components[i];

//                if (component == null)
//                    continue;

//                string typeName =
//                    component.GetType().Name ?? "";

//                if (typeName != "TextTranslatorTMP" &&
//                    typeName != "PlatformTextTranslationTMP")
//                {
//                    continue;
//                }

//                try
//                {
//                    Behaviour behaviour =
//                        component as Behaviour;

//                    if (behaviour != null)
//                        behaviour.enabled = false;
//                }
//                catch
//                {
//                }

//                try
//                {
//                    UnityEngine.Object.Destroy(
//                        component
//                    );
//                }
//                catch
//                {
//                }

//            }
//        }

//        private static void ForceChoiceButtonText(
//            PassiveButton button,
//            string textValue)
//        {
//            if (button == null)
//                return;

//            GameObject root =
//                button.gameObject;

//            if (!IsAlive(root))
//                return;

//            TMP_Text[] texts =
//                root.GetComponentsInChildren<TMP_Text>(
//                    true
//                );

//            if (texts == null)
//                return;

//            for (int i = 0;
//                 i < texts.Length;
//                 i++)
//            {
//                TMP_Text text =
//                    texts[i];

//                if (text != null)
//                    text.text =
//                        textValue;
//            }
//        }

//        private static void SelectVanilla()
//        {
//            BanModServerSelection.Mode =
//                BanModServerMode.Vanilla;

//            BanModServerSelection.VanillaAcknowledged =
//                false;

//            BanModServerSelection.VanillaCreateBypassOnce =
//                false;

//            Refresh();
//        }

//        private static void SelectModded()
//        {
//            BanModServerSelection.Mode =
//                BanModServerMode.Modded25;

//            BanModServerSelection.VanillaAcknowledged =
//                false;

//            BanModServerSelection.VanillaCreateBypassOnce =
//                false;

//            Options.UpdateGameModesForServer();

//            Refresh();
//        }

//        public static void Refresh()
//        {
//            if (BanModServerSelection.Mode ==
//                BanModServerMode.None)
//            {
//                BanModServerSelection.Mode =
//                    BanModServerMode.Modded25;
//            }

//            if (vanillaButton != null)
//            {
//                try
//                {
//                    vanillaButton.SelectButton(
//                        BanModServerSelection.IsVanilla
//                    );
//                }
//                catch
//                {
//                }
//            }

//            if (moddedButton != null)
//            {
//                try
//                {
//                    moddedButton.SelectButton(
//                        !BanModServerSelection.IsVanilla
//                    );
//                }
//                catch
//                {
//                }
//            }

//            ForceChoiceButtonText(
//                vanillaButton,
//                "Vanilla"
//            );

//            ForceChoiceButtonText(
//                moddedButton,
//                "Modded"
//            );

//            ForceServerLabel();
//        }

//        public static void CloseModeDropdown()
//        {
//            SetRowActive(
//                true
//            );
//        }

//        private static void SetRowActive(
//            bool active)
//        {
//            if (IsAlive(modeRowRoot))
//                modeRowRoot.SetActive(active);

//            if (IsAlive(serverLabelRoot))
//                serverLabelRoot.SetActive(active);
//        }

//        private static void DestroyServerUi()
//        {
//            if (IsAlive(modeRowRoot))
//            {
//                try
//                {
//                    UnityEngine.Object.Destroy(
//                        modeRowRoot
//                    );
//                }
//                catch
//                {
//                }
//            }

//            if (IsAlive(serverLabelRoot))
//            {
//                try
//                {
//                    UnityEngine.Object.Destroy(
//                        serverLabelRoot
//                    );
//                }
//                catch
//                {
//                }
//            }

//            modeRowRoot =
//                null;

//            serverLabelRoot =
//                null;

//            vanillaButton =
//                null;

//            moddedButton =
//                null;

//            serverHoverDescriptionText =
//                null;

//            serverHoverOriginalText =
//                "";

//            serverHoverTextCaptured =
//                false;
//        }

//        private static GameObject CloneServerLabel(
//            CreateGameOptions createScreen,
//            GameObject regionButton)
//        {
//            if (createScreen == null ||
//                regionButton == null)
//                return null;

//            TMP_Text[] allTexts =
//                createScreen.GetComponentsInChildren<TMP_Text>(
//                    true
//                );

//            if (allTexts == null ||
//                allTexts.Length == 0)
//                return null;

//            Vector3 regionLocal =
//                createScreen.transform.InverseTransformPoint(
//                    regionButton.transform.position
//                );

//            TMP_Text best =
//                null;

//            float bestScore =
//                float.MaxValue;

//            for (int i = 0;
//                 i < allTexts.Length;
//                 i++)
//            {
//                TMP_Text candidate =
//                    allTexts[i];

//                if (candidate == null)
//                    continue;

//                if (candidate.transform ==
//                    regionButton.transform ||
//                    candidate.transform.IsChildOf(
//                        regionButton.transform))
//                {
//                    continue;
//                }

//                string value =
//                    candidate.text ?? "";

//                if (string.IsNullOrEmpty(value) ||
//                    value.Length > 40)
//                    continue;

//                Vector3 local =
//                    createScreen.transform
//                        .InverseTransformPoint(
//                            candidate.transform.position
//                        );

//                float yDistance =
//                    Mathf.Abs(
//                        local.y - regionLocal.y
//                    );

//                float xDistance =
//                    Mathf.Abs(
//                        local.x - regionLocal.x
//                    );

//                float score =
//                    yDistance * 1000f +
//                    xDistance;


//                if (local.x < regionLocal.x)
//                    score -= 50f;
//                else
//                    score += 400f;

//                string hierarchy =
//                    BuildHierarchyName(
//                        candidate.transform,
//                        createScreen.transform
//                    );

//                if (ContainsIgnoreCase(value, "region") ||
//                    ContainsIgnoreCase(value, "regione") ||
//                    ContainsIgnoreCase(value, "région") ||
//                    ContainsIgnoreCase(value, "región") ||
//                    ContainsIgnoreCase(hierarchy, "region"))
//                {
//                    score -= 5000f;
//                }

//                if (score < bestScore)
//                {
//                    bestScore =
//                        score;

//                    best =
//                        candidate;
//                }
//            }

//            if (best == null)
//            {

//                return null;
//            }

//            Transform labelRoot =
//                FindLabelBlockRoot(
//                    best.transform,
//                    createScreen.transform,
//                    regionButton.transform
//                );

//            if (labelRoot == null ||
//                labelRoot.parent == null)
//                return null;

//            GameObject clone =
//                UnityEngine.Object.Instantiate(
//                    labelRoot.gameObject,
//                    labelRoot.parent
//                );

//            clone.name =
//                "BanModServerLabel";

//            Vector3 pos =
//                labelRoot.localPosition;

//            pos.y -=
//                YOffset;

//            clone.transform.localPosition =
//                pos;

//            clone.transform.localRotation =
//                labelRoot.localRotation;

//            clone.transform.localScale =
//                labelRoot.localScale;

//            clone.SetActive(
//                true
//            );

//            TMP_Text[] clonedTexts =
//                clone.GetComponentsInChildren<TMP_Text>(
//                    true
//                );

//            if (clonedTexts != null)
//            {
//                for (int i = 0;
//                     i < clonedTexts.Length;
//                     i++)
//                {
//                    TMP_Text label =
//                        clonedTexts[i];

//                    if (label != null)
//                        label.text =
//                            "SERVER";
//                }
//            }


//            return clone;
//        }

//        private static Transform FindLabelBlockRoot(
//            Transform labelText,
//            Transform screenRoot,
//            Transform regionButton)
//        {
//            if (labelText == null)
//                return null;

//            Transform best =
//                labelText;

//            Transform current =
//                labelText.parent;

//            int guard =
//                0;

//            while (current != null &&
//                   current != screenRoot &&
//                   guard < 3)
//            {
//                if (regionButton != null &&
//                    (current == regionButton ||
//                     regionButton.IsChildOf(current)))
//                {
//                    break;
//                }

//                try
//                {
//                    PassiveButton[] buttons =
//                        current.GetComponentsInChildren<PassiveButton>(
//                            true
//                        );

//                    TMP_Text[] texts =
//                        current.GetComponentsInChildren<TMP_Text>(
//                            true
//                        );

//                    int buttonCount =
//                        buttons == null
//                            ? 0
//                            : buttons.Length;

//                    int textCount =
//                        texts == null
//                            ? 0
//                            : texts.Length;

//                    if (buttonCount == 0 &&
//                        textCount <= 3)
//                    {
//                        best =
//                            current;
//                    }
//                    else
//                    {
//                        break;
//                    }
//                }
//                catch
//                {
//                    break;
//                }

//                current =
//                    current.parent;

//                guard++;
//            }

//            return best;
//        }

//        private static void ForceServerLabel()
//        {
//            if (!IsAlive(serverLabelRoot))
//                return;

//            TMP_Text[] texts =
//                serverLabelRoot.GetComponentsInChildren<TMP_Text>(
//                    true
//                );

//            if (texts == null)
//                return;

//            for (int i = 0;
//                 i < texts.Length;
//                 i++)
//            {
//                if (texts[i] != null)
//                    texts[i].text =
//                        "SERVER";
//            }
//        }

//        private static void ExtendScroll(
//            CreateGameOptions createScreen)
//        {
//            Scroller runtimeScroller =
//                TryGetRuntimeMember<Scroller>(
//                    createScreen,
//                    "scrollBar"
//                );

//            if (runtimeScroller != null)
//            {
//                try
//                {
//                    runtimeScroller.SetYBoundsMax(
//                        ExtendedScrollMax
//                    );
//                }
//                catch
//                {
//                }

//                return;
//            }

//            Scroller[] scrollers =
//                createScreen.GetComponentsInChildren<Scroller>(
//                    true
//                );

//            if (scrollers == null)
//                return;

//            for (int i = 0;
//                 i < scrollers.Length;
//                 i++)
//            {
//                Scroller scroller =
//                    scrollers[i];

//                if (scroller == null)
//                    continue;

//                try
//                {
//                    scroller.SetYBoundsMax(
//                        ExtendedScrollMax
//                    );
//                }
//                catch
//                {
//                }
//            }
//        }

//        private static T TryGetRuntimeMember<T>(
//            object instance,
//            string name)
//            where T : class
//        {
//            object value =
//                TryGetRuntimeMemberObject(
//                    instance,
//                    name
//                );

//            return value as T;
//        }

//        private static object TryGetRuntimeMemberObject(
//            object instance,
//            string name)
//        {
//            if (instance == null ||
//                string.IsNullOrEmpty(name))
//                return null;

//            Type type =
//                instance.GetType();

//            const System.Reflection.BindingFlags flags =
//                System.Reflection.BindingFlags.Instance |
//                System.Reflection.BindingFlags.Public |
//                System.Reflection.BindingFlags.NonPublic;

//            try
//            {
//                System.Reflection.FieldInfo field =
//                    type.GetField(
//                        name,
//                        flags
//                    );

//                if (field != null)
//                {
//                    return field.GetValue(
//                        instance
//                    );
//                }
//            }
//            catch
//            {
//            }

//            try
//            {
//                System.Reflection.PropertyInfo property =
//                    type.GetProperty(
//                        name,
//                        flags
//                    );

//                if (property != null &&
//                    property.GetIndexParameters().Length == 0)
//                {
//                    return property.GetValue(
//                        instance,
//                        null
//                    );
//                }
//            }
//            catch
//            {
//            }

//            return null;
//        }

//        private static List<PassiveButton>
//            TryGetRuntimePassiveButtonCollection(
//                object instance,
//                string name)
//        {
//            List<PassiveButton> result =
//                new List<PassiveButton>();

//            object value =
//                TryGetRuntimeMemberObject(
//                    instance,
//                    name
//                );

//            if (value == null)
//                return result;

//            PassiveButton single =
//                value as PassiveButton;

//            if (single != null)
//            {
//                result.Add(
//                    single
//                );

//                return result;
//            }

//            System.Collections.IEnumerable enumerable =
//                value as System.Collections.IEnumerable;

//            if (enumerable != null)
//            {
//                try
//                {
//                    foreach (object item in enumerable)
//                    {
//                        PassiveButton button =
//                            item as PassiveButton;

//                        if (button != null &&
//                            !result.Contains(button))
//                        {
//                            result.Add(
//                                button
//                            );
//                        }
//                    }
//                }
//                catch
//                {
//                }

//                if (result.Count > 0)
//                    return result;
//            }

//            try
//            {
//                Type collectionType =
//                    value.GetType();

//                System.Reflection.PropertyInfo lengthProperty =
//                    collectionType.GetProperty(
//                        "Length"
//                    );

//                System.Reflection.PropertyInfo itemProperty =
//                    collectionType.GetProperty(
//                        "Item"
//                    );

//                if (lengthProperty != null &&
//                    itemProperty != null)
//                {
//                    int length =
//                        Convert.ToInt32(
//                            lengthProperty.GetValue(
//                                value,
//                                null
//                            )
//                        );

//                    for (int i = 0;
//                         i < length;
//                         i++)
//                    {
//                        object item =
//                            itemProperty.GetValue(
//                                value,
//                                new object[] { i }
//                            );

//                        PassiveButton button =
//                            item as PassiveButton;

//                        if (button != null &&
//                            !result.Contains(button))
//                        {
//                            result.Add(
//                                button
//                            );
//                        }
//                    }
//                }
//            }
//            catch
//            {
//            }

//            return result;
//        }

//        private static GameObject FindRegionButtonFromHierarchy(
//            CreateGameOptions createScreen)
//        {
//            PassiveButton[] buttons =
//                createScreen.GetComponentsInChildren<PassiveButton>(
//                    true
//                );

//            if (buttons == null)
//                return null;

//            PassiveButton best =
//                null;

//            float bestScore =
//                float.MaxValue;

//            for (int i = 0;
//                 i < buttons.Length;
//                 i++)
//            {
//                PassiveButton button =
//                    buttons[i];

//                if (button == null)
//                    continue;

//                string hierarchy =
//                    BuildHierarchyName(
//                        button.transform,
//                        createScreen.transform
//                    );

//                float score =
//                    1000f;

//                if (ContainsIgnoreCase(
//                        hierarchy,
//                        "serverbox"))
//                {
//                    score -=
//                        900f;
//                }

//                if (ContainsIgnoreCase(
//                        hierarchy,
//                        "server"))
//                {
//                    score -=
//                        500f;
//                }

//                if (ContainsIgnoreCase(
//                        hierarchy,
//                        "region"))
//                {
//                    score -=
//                        400f;
//                }

//                if (score < bestScore)
//                {
//                    bestScore =
//                        score;

//                    best =
//                        button;
//                }
//            }

//            return best != null
//                ? best.gameObject
//                : null;
//        }

//        private static List<PassiveButton>
//            FindModeButtonsFromHierarchy(
//                CreateGameOptions createScreen)
//        {
//            List<PassiveButton> result =
//                new List<PassiveButton>();

//            PassiveButton[] buttons =
//                createScreen.GetComponentsInChildren<PassiveButton>(
//                    true
//                );

//            if (buttons == null)
//                return result;

//            PassiveButton classic =
//                null;

//            PassiveButton hide =
//                null;

//            for (int i = 0;
//                 i < buttons.Length;
//                 i++)
//            {
//                PassiveButton button =
//                    buttons[i];

//                if (button == null)
//                    continue;

//                string hierarchy =
//                    BuildHierarchyName(
//                        button.transform,
//                        createScreen.transform
//                    );

//                string name =
//                    button.gameObject.name ?? "";

//                if (classic == null &&
//                    (ContainsIgnoreCase(name, "classic") ||
//                     ContainsIgnoreCase(hierarchy, "classic")))
//                {
//                    classic =
//                        button;
//                }

//                if (hide == null &&
//                    (ContainsIgnoreCase(name, "hide") ||
//                     ContainsIgnoreCase(name, "seek") ||
//                     ContainsIgnoreCase(hierarchy, "hide") ||
//                     ContainsIgnoreCase(hierarchy, "seek")))
//                {
//                    hide =
//                        button;
//                }
//            }

//            if (classic != null)
//                result.Add(classic);

//            if (hide != null &&
//                hide != classic)
//            {
//                result.Add(hide);
//            }

//            return result;
//        }

//        private static string BuildHierarchyName(
//            Transform current,
//            Transform stopAt)
//        {
//            if (current == null)
//                return "";

//            string path =
//                current.name ?? "";

//            Transform parent =
//                current.parent;

//            int guard =
//                0;

//            while (parent != null &&
//                   parent != stopAt &&
//                   guard < 16)
//            {
//                path =
//                    (parent.name ?? "") +
//                    "/" +
//                    path;

//                parent =
//                    parent.parent;

//                guard++;
//            }

//            return path;
//        }

//        private static bool ContainsIgnoreCase(
//            string value,
//            string search)
//        {
//            if (string.IsNullOrEmpty(value) ||
//                string.IsNullOrEmpty(search))
//                return false;

//            return value.IndexOf(
//                search,
//                StringComparison.OrdinalIgnoreCase
//            ) >= 0;
//        }

//        private static bool IsAlive(
//            UnityEngine.Object obj)
//        {
//            try
//            {
//                return obj != null;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        private static void LogUsefulButtons(
//            CreateGameOptions createScreen)
//        {
//            if (createScreen == null)
//                return;

//            try
//            {
//                PassiveButton[] buttons =
//                    createScreen.GetComponentsInChildren<PassiveButton>(
//                        true
//                    );

//                if (buttons == null)
//                    return;

//                for (int i = 0;
//                     i < buttons.Length;
//                     i++)
//                {
//                    PassiveButton button =
//                        buttons[i];

//                    if (button == null)
//                        continue;

//                }
//            }
//            catch
//            {
//            }
//        }

//    }

//    [HarmonyPatch(
//        typeof(CreateGameOptions),
//        nameof(CreateGameOptions.Confirm)
//    )]
//    public static class BanModCreateGameConfirmPatch
//    {
//        [HarmonyPrefix]
//        public static bool Prefix(
//            CreateGameOptions __instance)
//        {
//            if (BanMod.IsBanModDisabled)
//                return true;

//            if (!BanModServerSelection.IsVanilla)
//                return true;

//            if (BanModServerSelection.ConsumeVanillaCreateBypass())
//            {

//                return true;
//            }

//            if (BanModServerSelection.HasAcceptedVanillaThisWeek())
//            {
//                BanModServerSelection.VanillaAcknowledged =
//                    true;

//                DateTime? expiresAt =
//                    BanModServerSelection
//                        .GetVanillaAcceptanceExpiry();

//                if (expiresAt.HasValue)
//                {
//                    string nextDate =
//                        expiresAt.Value.ToString(
//                            "MMMM d, yyyy",
//                            CultureInfo.InvariantCulture
//                        );

//                    MainMenuInfo.Show(
//                        $"Already accepted - Next request: {nextDate}"
//                    );
//                }
//                else
//                {
//                    MainMenuInfo.Show(
//                        "Already accepted."
//                    );
//                }


//                return true;
//            }

//            if (ServerSelectionMenu.Instance == null)
//            {

//                return false;
//            }


//            ServerSelectionMenu.Instance
//                .OpenVanillaCreateWarning(
//                    __instance
//                );

//            return false;
//        }

//    }


//    [HarmonyPatch(
//        typeof(Constants),
//        nameof(Constants.GetBroadcastVersion)
//    )]
//    public static class BanModBroadcastVersionPatch
//    {
//        public static void Postfix(ref int __result)
//        {
//            if (!BanModServerSelection.IsModded25)
//                return;

//            int original =
//                __result;

//            __result += 25;

//        }
//    }
//    [HarmonyPatch(
//    typeof(Constants),
//    nameof(Constants.IsVersionModded)
//)]
//    public static class BanModIsVersionModdedPatch
//    {
//        public static bool Prefix(ref bool __result)
//        {
//            if (!BanModServerSelection.IsModded25)
//                return true;

//            __result = true;

//            return false;
//        }
//    }
//}

using HarmonyLib;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using static BanMod.Utils;

namespace BanMod
{
    public enum BanModServerMode
    {
        None,
        Modded25,
        Vanilla
    }

    public static class BanModServerSelection
    {
        private static BanModServerMode _mode = BanModServerMode.Vanilla;

        public static BanModServerMode Mode
        {
            get => _mode;
            set
            {
                if (_mode == value)
                    return;

                _mode = value;

                try
                {
                    Options.UpdateGameModesForServer();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        "[BanMod] UpdateGameModesForServer error: " + ex
                    );
                }
            }
        }

        public static bool VanillaAcknowledged = false;
        public static bool VanillaCreateBypassOnce = false;

        public static bool IsModded25 =>
            Mode == BanModServerMode.Modded25;

        public static bool IsVanilla =>
            Mode == BanModServerMode.Vanilla;

        public static bool HasSelectedMode =>
            Mode != BanModServerMode.None;

        public static void Reset()
        {
            Mode = BanModServerMode.Vanilla;
            VanillaAcknowledged = false;
            VanillaCreateBypassOnce = false;
        }

        public static bool ConsumeVanillaCreateBypass()
        {
            if (!VanillaCreateBypassOnce)
                return false;

            VanillaCreateBypassOnce = false;
            return true;
        }
        private const string VanillaAcceptedKey = "BanMod_VanillaAcceptedAt";

        public static bool HasAcceptedVanillaThisWeek()
        {
            string saved = PlayerPrefs.GetString(VanillaAcceptedKey, "");

            if (string.IsNullOrEmpty(saved))
                return false;

            if (!long.TryParse(saved, out long ticks))
                return false;

            DateTime acceptedAt = new DateTime(ticks, DateTimeKind.Utc);

            return DateTime.UtcNow - acceptedAt < TimeSpan.FromDays(7);
        }
        public static DateTime? GetVanillaAcceptanceExpiry()
        {
            string saved = PlayerPrefs.GetString(
                VanillaAcceptedKey,
                ""
            );

            if (string.IsNullOrEmpty(saved))
                return null;

            if (!long.TryParse(saved, out long ticks))
                return null;

            DateTime acceptedAt =
                new DateTime(
                    ticks,
                    DateTimeKind.Utc
                );

            return acceptedAt
                .AddDays(7)
                .ToLocalTime();
        }
        public static void SaveVanillaAcceptance()
        {
            PlayerPrefs.SetString(
                VanillaAcceptedKey,
                DateTime.UtcNow.Ticks.ToString()
            );

            PlayerPrefs.Save();

            VanillaAcknowledged = true;
        }
    }

    public class BanModServerTexts
    {
        public string SelectTitle;
        public string SelectDescription;
        public string ModdedDescription;
        public string VanillaDescription;
        public string ModdedButton;
        public string VanillaButton;
        public string PrivateFooter;

        public string VanillaTitle;
        public string VanillaIntro;
        public string VanillaWarning;
        public string ConfirmButton;
        public string BackButton;

    }

    public static class BanModServerLocalization
    {
        public static BanModServerTexts Get()
        {
            string language = GetLanguageId();

            if (Has(language, "italian", "italiano"))
                return Italian();

            if (Has(language, "french", "franÃ§ais", "francais"))
                return French();

            if (Has(language, "german", "deutsch"))
                return German();

            if (Has(
                language,
                "latam",
                "spanishlatam",
                "spanish_la",
                "spanishla",
                "espanollatam",
                "espaÃ±ollatam"
            ))
                return SpanishLatam();

            if (Has(
                language,
                "spanish",
                "spanisheU",
                "spanish_eu",
                "espanol",
                "espaÃ±ol"
            ))
                return Spanish();

            if (Has(
                language,
                "brazilian",
                "brazilianportuguese",
                "portuguesebrazil",
                "portuguesebr",
                "portuguÃªsbr",
                "portuguesbr"
            ))
                return BrazilianPortuguese();

            if (Has(
                language,
                "portuguese",
                "portugueseeu",
                "portuguese_eu",
                "portuguÃªs",
                "portugues"
            ))
                return Portuguese();

            if (Has(language, "dutch", "nederlands"))
                return Dutch();

            if (Has(language, "russian", "Ñ€ÑƒÑÑÐºÐ¸Ð¹"))
                return Russian();

            if (Has(language, "japanese", "æ—¥æœ¬èªž"))
                return Japanese();

            if (Has(language, "korean", "í•œêµ­ì–´"))
                return Korean();

            if (Has(
                language,
                "schinese",
                "simplifiedchinese",
                "chinesesimplified",
                "chinese_cn",
                "ç®€ä½“ä¸­æ–‡"
            ))
                return SimplifiedChinese();

            if (Has(
                language,
                "tchinese",
                "traditionalchinese",
                "chinesetraditional",
                "chinese_tw",
                "ç¹é«”ä¸­æ–‡",
                "ç¹ä½“ä¸­æ–‡"
            ))
                return TraditionalChinese();

            if (Has(
                language,
                "filipino",
                "bisaya",
                "cebuano"
            ))
                return Filipino();

            return English();
        }

        private static string GetLanguageId()
        {
            try
            {
                if (TranslationController.Instance != null &&
                    TranslationController.Instance.currentLanguage != null)
                {
                    string id =
                        TranslationController.Instance
                            .currentLanguage
                            .languageID
                            .ToString();

                    if (!string.IsNullOrEmpty(id))
                        return id.ToLowerInvariant();
                }
            }
            catch
            {
            }

            return "english";
        }

        private static bool Has(
            string value,
            params string[] names
        )
        {
            if (string.IsNullOrEmpty(value))
                return false;

            for (int i = 0; i < names.Length; i++)
            {
                if (value.Equals(
                    names[i].ToLowerInvariant(),
                    StringComparison.OrdinalIgnoreCase
                ))
                    return true;
            }

            return false;
        }

        private static BanModServerTexts English()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - SELECT LOBBY MODE",
                SelectDescription =
                    "Choose the lobby mode before creating this lobby.",
                ModdedDescription =
                    "Use this server to add roles and modify the gameplay.",
                VanillaDescription =
                    "Use this server if you only use anti-cheat and visual modifications.",
                ModdedButton =
                    "MODDED +25\nRECOMMENDED",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "Use BanMod responsibly and select the mode that matches the type of lobby you are creating.",
                VanillaTitle =
                    "IMPORTANT - VANILLA MODE",
                VanillaIntro =
                    "You selected Vanilla mode.",
                VanillaWarning =
                    "Please do not use BanMod to annoy or disturb other players, and do not enable options that change gameplay, provide unfair advantages, or alter the experience of other players.\n\nImproper, unauthorized, or rule-breaking use may result in warnings, restrictions, suspensions, bans, or other sanctions from Innersloth or other services involved.\n\nBanMod and its developers are not responsible for any consequences resulting from improper, unlawful, or unauthorized use of the mod.\n\nUse only features compatible with Vanilla mode and respect the rules of Among Us and any other services being used.",
                ConfirmButton =
                    "I AGREE",
                BackButton =
                    "I DECLINE"
            };
        }

        private static BanModServerTexts Italian()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - SELEZIONA MODALITÀ LOBBY",
                SelectDescription =
                    "Scegli la modalità della lobby prima di crearla.",
                ModdedDescription =
                    "Usa questo server per aggiungere ruoli e modificare il gameplay.",
                VanillaDescription =
                    "Usa questo server se usi solo anti-cheat e modifiche visuali.",
                ModdedButton =
                    "MODDED +25\nCONSIGLIATO",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "Usa BanMod responsabilmente e seleziona la modalità che corrisponde al tipo di lobby che stai creando.",
                VanillaTitle =
                    "IMPORTANTE - MODALITÀ VANILLA",
                VanillaIntro =
                    "Hai scelto la modalità Vanilla.",
                VanillaWarning =
                    "Per favore, non utilizzare BanMod per infastidire o disturbare altri giocatori e non attivare opzioni che modificano il gameplay, forniscono vantaggi sleali o alterano l’esperienza degli altri giocatori.\n\nUn utilizzo improprio, non consentito o contrario alle regole può provocare avvertimenti, restrizioni, sospensioni, ban o altre sanzioni da parte di Innersloth o di altri servizi coinvolti.\n\nBanMod e i suoi sviluppatori non sono responsabili per eventuali conseguenze derivanti da un utilizzo improprio, illecito o non consentito della mod.\n\nAssicurati di utilizzare esclusivamente funzionalità compatibili con la modalità Vanilla e di rispettare le regole di Among Us e degli altri servizi utilizzati.",
                ConfirmButton =
                    "ACCETTO",
                BackButton =
                    "NEGO"
            };
        }

        private static BanModServerTexts French()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - SÉLECTION DU MODE DU SALON",
                SelectDescription =
                    "Choisissez le mode du salon avant de le créer.",
                ModdedDescription =
                    "Utilisez ce serveur pour ajouter des rôles et modifier le gameplay.",
                VanillaDescription =
                    "Utilisez ce serveur si vous utilisez uniquement un anti-triche et des modifications visuelles.",
                ModdedButton =
                    "MODDED +25\nRECOMMANDÉ",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "Utilisez BanMod de manière responsable et choisissez le mode correspondant au type de salon que vous créez.",
                VanillaTitle =
                    "IMPORTANT - MODE VANILLA",
                VanillaIntro =
                    "Vous avez choisi le mode Vanilla.",
                VanillaWarning =
                    "Veuillez ne pas utiliser BanMod pour déranger ou perturber les autres joueurs, et n’activez pas d’options qui modifient le gameplay, donnent des avantages injustes ou changent l’expérience des autres joueurs.\n\nUne utilisation abusive, non autorisée ou contraire aux règles peut entraîner des avertissements, restrictions, suspensions, bannissements ou autres sanctions de la part d’Innersloth ou d’autres services concernés.\n\nBanMod et ses développeurs ne sont pas responsables des conséquences résultant d’une utilisation abusive, illégale ou non autorisée du mod.\n\nUtilisez uniquement des fonctionnalités compatibles avec le mode Vanilla et respectez les règles d’Among Us et des autres services utilisés.",
                ConfirmButton =
                    "J’ACCEPTE",
                BackButton =
                    "JE REFUSE"
            };
        }

        private static BanModServerTexts German()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - LOBBY-MODUS AUSWÄHLEN",
                SelectDescription =
                    "Wähle den Lobby-Modus, bevor du diese Lobby erstellst.",
                ModdedDescription =
                    "Verwende diesen Server, um Rollen hinzuzufügen und das Gameplay zu verändern.",
                VanillaDescription =
                    "Verwende diesen Server, wenn du nur Anti-Cheat und visuelle Änderungen nutzt.",
                ModdedButton =
                    "MODDED +25\nEMPFOHLEN",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "Verwende BanMod verantwortungsvoll und wähle den Modus, der zu deiner Lobby passt.",
                VanillaTitle =
                    "WICHTIG - VANILLA-MODUS",
                VanillaIntro =
                    "Du hast den Vanilla-Modus ausgewählt.",
                VanillaWarning =
                    "Bitte verwende BanMod nicht, um andere Spieler zu stören oder zu belästigen, und aktiviere keine Optionen, die das Gameplay verändern, unfaire Vorteile geben oder die Erfahrung anderer Spieler verändern.\n\nEine unsachgemäße, nicht autorisierte oder regelwidrige Nutzung kann zu Verwarnungen, Einschränkungen, Sperren, Bans oder anderen Maßnahmen durch Innersloth oder andere beteiligte Dienste führen.\n\nBanMod und seine Entwickler sind nicht für Folgen verantwortlich, die aus einer unsachgemäßen, rechtswidrigen oder nicht autorisierten Nutzung der Mod entstehen.\n\nVerwende nur mit dem Vanilla-Modus kompatible Funktionen und halte die Regeln von Among Us sowie der verwendeten Dienste ein.",
                ConfirmButton =
                    "ICH STIMME ZU",
                BackButton =
                    "ICH LEHNE AB"
            };
        }

        private static BanModServerTexts Spanish()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - SELECCIONAR MODO DE SALA",
                SelectDescription =
                    "Elige el modo de la sala antes de crearla.",
                ModdedDescription =
                    "Usa este servidor para añadir roles y modificar el gameplay.",
                VanillaDescription =
                    "Usa este servidor si solo utilizas anti-cheat y modificaciones visuales.",
                ModdedButton =
                    "MODDED +25\nRECOMENDADO",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "Usa BanMod de forma responsable y selecciona el modo que corresponda al tipo de sala que vas a crear.",
                VanillaTitle =
                    "IMPORTANTE - MODO VANILLA",
                VanillaIntro =
                    "Has elegido el modo Vanilla.",
                VanillaWarning =
                    "Por favor, no uses BanMod para molestar o perturbar a otros jugadores y no actives opciones que cambien el gameplay, proporcionen ventajas injustas o alteren la experiencia de otros jugadores.\n\nEl uso indebido, no autorizado o contrario a las reglas puede provocar advertencias, restricciones, suspensiones, baneos u otras sanciones por parte de Innersloth u otros servicios involucrados.\n\nBanMod y sus desarrolladores no son responsables de las consecuencias derivadas de un uso indebido, ilegal o no autorizado del mod.\n\nUtiliza únicamente funciones compatibles con el modo Vanilla y respeta las reglas de Among Us y de los demás servicios utilizados.",
                ConfirmButton =
                    "ACEPTO",
                BackButton =
                    "NO ACEPTO"
            };
        }

        private static BanModServerTexts SpanishLatam()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - SELECCIONAR MODO DE SALA",
                SelectDescription =
                    "Elige el modo de la sala antes de crearla.",
                ModdedDescription =
                    "Usa este servidor para añadir roles y modificar el gameplay.",
                VanillaDescription =
                    "Usa este servidor si solo usas anti-cheat y modificaciones visuales.",
                ModdedButton =
                    "MODDED +25\nRECOMENDADO",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "Usa BanMod de forma responsable y selecciona el modo que corresponda al tipo de sala que vas a crear.",
                VanillaTitle =
                    "IMPORTANTE - MODO VANILLA",
                VanillaIntro =
                    "Has elegido el modo Vanilla.",
                VanillaWarning =
                    "Por favor, no uses BanMod para molestar o perturbar a otros jugadores y no actives opciones que cambien el gameplay, den ventajas injustas o alteren la experiencia de otros jugadores.\n\nEl uso indebido, no autorizado o contrario a las reglas puede provocar advertencias, restricciones, suspensiones, baneos u otras sanciones por parte de Innersloth u otros servicios involucrados.\n\nBanMod y sus desarrolladores no son responsables de las consecuencias derivadas de un uso indebido, ilegal o no autorizado del mod.\n\nUtiliza únicamente funciones compatibles con el modo Vanilla y respeta las reglas de Among Us y de los demás servicios utilizados.",
                ConfirmButton =
                    "ACEPTO",
                BackButton =
                    "NO ACEPTO"
            };
        }

        private static BanModServerTexts BrazilianPortuguese()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - SELECIONAR MODO DA SALA",
                SelectDescription =
                    "Escolha o modo da sala antes de criá-la.",
                ModdedDescription =
                    "Use este servidor para adicionar funções e modificar o gameplay.",
                VanillaDescription =
                    "Use este servidor se você utiliza apenas anti-cheat e modificações visuais.",
                ModdedButton =
                    "MODDED +25\nRECOMENDADO",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "Use o BanMod com responsabilidade e selecione o modo correspondente ao tipo de sala que você está criando.",
                VanillaTitle =
                    "IMPORTANTE - MODO VANILLA",
                VanillaIntro =
                    "Você escolheu o modo Vanilla.",
                VanillaWarning =
                    "Por favor, não use o BanMod para incomodar ou perturbar outros jogadores e não ative opções que alterem o gameplay, forneçam vantagens injustas ou mudem a experiência de outros jogadores.\n\nO uso indevido, não autorizado ou contrário às regras pode resultar em avisos, restrições, suspensões, banimentos ou outras sanções da Innersloth ou de outros serviços envolvidos.\n\nO BanMod e seus desenvolvedores não são responsáveis por quaisquer consequências decorrentes do uso indevido, ilegal ou não autorizado do mod.\n\nUse apenas recursos compatíveis com o modo Vanilla e respeite as regras de Among Us e dos outros serviços utilizados.",
                ConfirmButton =
                    "ACEITO",
                BackButton =
                    "NÃO ACEITO"
            };
        }

        private static BanModServerTexts Portuguese()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - SELECIONAR MODO DA SALA",
                SelectDescription =
                    "Escolhe o modo da sala antes de a criares.",
                ModdedDescription =
                    "Usa este servidor para adicionar funções e modificar o gameplay.",
                VanillaDescription =
                    "Usa este servidor se utilizares apenas anti-cheat e modificações visuais.",
                ModdedButton =
                    "MODDED +25\nRECOMENDADO",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "Usa o BanMod de forma responsável e seleciona o modo correspondente ao tipo de sala que estás a criar.",
                VanillaTitle =
                    "IMPORTANTE - MODO VANILLA",
                VanillaIntro =
                    "Escolheste o modo Vanilla.",
                VanillaWarning =
                    "Por favor, não uses o BanMod para incomodar ou perturbar outros jogadores e não atives opções que alterem o gameplay, deem vantagens injustas ou mudem a experiência dos outros jogadores.\n\nUma utilização indevida, não autorizada ou contrária às regras pode resultar em avisos, restrições, suspensões, banimentos ou outras sanções da Innersloth ou de outros serviços envolvidos.\n\nO BanMod e os seus desenvolvedores não são responsáveis por quaisquer consequências resultantes de uma utilização indevida, ilegal ou não autorizada da mod.\n\nUtiliza apenas funcionalidades compatíveis com o modo Vanilla e respeita as regras de Among Us e dos outros serviços utilizados.",
                ConfirmButton =
                    "ACEITO",
                BackButton =
                    "NÃO ACEITO"
            };
        }

        private static BanModServerTexts Dutch()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - LOBBYMODUS SELECTEREN",
                SelectDescription =
                    "Kies de lobbymodus voordat je deze lobby maakt.",
                ModdedDescription =
                    "Gebruik deze server om rollen toe te voegen en de gameplay aan te passen.",
                VanillaDescription =
                    "Gebruik deze server als je alleen anti-cheat en visuele aanpassingen gebruikt.",
                ModdedButton =
                    "MODDED +25\nAANBEVOLEN",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "Gebruik BanMod verantwoord en kies de modus die past bij het type lobby dat je maakt.",
                VanillaTitle =
                    "BELANGRIJK - VANILLA-MODUS",
                VanillaIntro =
                    "Je hebt de Vanilla-modus gekozen.",
                VanillaWarning =
                    "Gebruik BanMod niet om andere spelers te ergeren of te hinderen en schakel geen opties in die de gameplay veranderen, oneerlijke voordelen geven of de ervaring van andere spelers aanpassen.\n\nOnjuist, ongeoorloofd of regelstrijdig gebruik kan leiden tot waarschuwingen, beperkingen, schorsingen, bans of andere sancties van Innersloth of andere betrokken diensten.\n\nBanMod en de ontwikkelaars zijn niet verantwoordelijk voor gevolgen die voortkomen uit onjuist, onwettig of ongeoorloofd gebruik van de mod.\n\nGebruik alleen functies die compatibel zijn met de Vanilla-modus en respecteer de regels van Among Us en de gebruikte diensten.",
                ConfirmButton =
                    "IK GA AKKOORD",
                BackButton =
                    "IK WEIGER"
            };
        }

        private static BanModServerTexts Russian()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - ВЫБОР РЕЖИМА ЛОББИ",
                SelectDescription =
                    "Выберите режим лобби перед его созданием.",
                ModdedDescription =
                    "Используйте этот сервер, чтобы добавлять роли и изменять игровой процесс.",
                VanillaDescription =
                    "Используйте этот сервер, если вы применяете только античит и визуальные изменения.",
                ModdedButton =
                    "MODDED +25\nРЕКОМЕНДУЕТСЯ",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "Используйте BanMod ответственно и выбирайте режим, соответствующий создаваемому лобби.",
                VanillaTitle =
                    "ВАЖНО - РЕЖИМ VANILLA",
                VanillaIntro =
                    "Вы выбрали режим Vanilla.",
                VanillaWarning =
                    "Пожалуйста, не используйте BanMod для того, чтобы мешать или раздражать других игроков, и не включайте функции, которые изменяют игровой процесс, дают нечестные преимущества или влияют на опыт других игроков.\n\nНеправильное, несанкционированное или нарушающее правила использование может привести к предупреждениям, ограничениям, приостановке, блокировке или другим санкциям со стороны Innersloth или других сервисов.\n\nBanMod и его разработчики не несут ответственности за последствия неправильного, незаконного или несанкционированного использования мода.\n\nИспользуйте только функции, совместимые с режимом Vanilla, и соблюдайте правила Among Us и других используемых сервисов.",
                ConfirmButton =
                    "ПРИНИМАЮ",
                BackButton =
                    "ОТКАЗЫВАЮСЬ"
            };
        }

        private static BanModServerTexts Japanese()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - ロビーモードを選択",
                SelectDescription =
                    "ロビーを作成する前にモードを選択してください。",
                ModdedDescription =
                    "役職を追加したりゲームプレイを変更したりする場合は、このサーバーを使用してください。",
                VanillaDescription =
                    "アンチチートと見た目の変更だけを使用する場合は、このサーバーを使用してください。",
                ModdedButton =
                    "MODDED +25\n推奨",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "BanModを責任を持って使用し、作成するロビーに合ったモードを選択してください。",
                VanillaTitle =
                    "重要 - VANILLAモード",
                VanillaIntro =
                    "VANILLAモードを選択しました。",
                VanillaWarning =
                    "他のプレイヤーを困らせたり妨害したりするためにBanModを使用しないでください。また、ゲームプレイを変更したり、不公平な優位性を与えたり、他のプレイヤーの体験を変えたりするオプションを有効にしないでください。\n\n不適切、許可されていない、またはルールに反する使用は、Innerslothや関係する他のサービスによる警告、制限、停止、BAN、その他の措置につながる可能性があります。\n\nBanModおよび開発者は、Modの不適切、違法、または許可されていない使用によって生じた結果について責任を負いません。\n\nVANILLAモードと互換性のある機能のみを使用し、Among Usおよび利用する他のサービスのルールを守ってください。",
                ConfirmButton =
                    "同意する",
                BackButton =
                    "拒否する"
            };
        }

        private static BanModServerTexts Korean()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - 로비 모드 선택",
                SelectDescription =
                    "로비를 만들기 전에 로비 모드를 선택하세요.",
                ModdedDescription =
                    "역할을 추가하고 게임플레이를 변경하려면 이 서버를 사용하세요.",
                VanillaDescription =
                    "안티치트와 시각적 변경만 사용하는 경우 이 서버를 사용하세요.",
                ModdedButton =
                    "MODDED +25\n권장",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "BanMod를 책임감 있게 사용하고 생성하려는 로비에 맞는 모드를 선택하세요.",
                VanillaTitle =
                    "중요 - VANILLA 모드",
                VanillaIntro =
                    "VANILLA 모드를 선택했습니다.",
                VanillaWarning =
                    "다른 플레이어를 괴롭히거나 방해하기 위해 BanMod를 사용하지 말고, 게임플레이를 변경하거나 부당한 이점을 제공하거나 다른 플레이어의 경험을 바꾸는 옵션을 활성화하지 마세요.\n\n부적절하거나 승인되지 않았거나 규칙을 위반하는 사용은 Innersloth 또는 관련 서비스에서 경고, 제한, 정지, 밴 또는 기타 제재를 받을 수 있습니다.\n\nBanMod와 개발자는 모드의 부적절하거나 불법적이거나 승인되지 않은 사용으로 인해 발생하는 결과에 대해 책임을 지지 않습니다.\n\nVANILLA 모드와 호환되는 기능만 사용하고 Among Us 및 사용하는 다른 서비스의 규칙을 준수하세요.",
                ConfirmButton =
                    "동의",
                BackButton =
                    "거부"
            };
        }

        private static BanModServerTexts SimplifiedChinese()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - 选择大厅模式",
                SelectDescription =
                    "创建大厅前请选择大厅模式。",
                ModdedDescription =
                    "使用此服务器来添加角色并修改游戏玩法。",
                VanillaDescription =
                    "如果你只使用反作弊和视觉修改，请使用此服务器。",
                ModdedButton =
                    "MODDED +25\n推荐",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "请负责任地使用 BanMod，并选择与当前大厅类型相符的模式。",
                VanillaTitle =
                    "重要 - VANILLA 模式",
                VanillaIntro =
                    "你选择了 Vanilla 模式。",
                VanillaWarning =
                    "请不要使用 BanMod 骚扰或干扰其他玩家，也不要启用会修改游戏玩法、提供不公平优势或改变其他玩家游戏体验的选项。\n\n不当、未经授权或违反规则的使用可能导致 Innersloth 或其他相关服务发出警告、限制、暂停、封禁或其他处罚。\n\n对于因不当、违法或未经授权使用该 Mod 而产生的任何后果，BanMod 及其开发者不承担责任。\n\n请仅使用与 Vanilla 模式兼容的功能，并遵守 Among Us 及其他所使用服务的规则。",
                ConfirmButton =
                    "接受",
                BackButton =
                    "拒绝"
            };
        }

        private static BanModServerTexts TraditionalChinese()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - 選擇大廳模式",
                SelectDescription =
                    "建立大廳前請選擇大廳模式。",
                ModdedDescription =
                    "使用此伺服器來新增角色並修改遊戲玩法。",
                VanillaDescription =
                    "如果你只使用反作弊和視覺修改，請使用此伺服器。",
                ModdedButton =
                    "MODDED +25\n推薦",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "請負責任地使用 BanMod，並選擇符合目前大廳類型的模式。",
                VanillaTitle =
                    "重要 - VANILLA 模式",
                VanillaIntro =
                    "你選擇了 Vanilla 模式。",
                VanillaWarning =
                    "請不要使用 BanMod 騷擾或干擾其他玩家，也不要啟用會修改遊戲玩法、提供不公平優勢或改變其他玩家遊戲體驗的選項。\n\n不當、未經授權或違反規則的使用可能導致 Innersloth 或其他相關服務發出警告、限制、暫停、封禁或其他處分。\n\n對於因不當、違法或未經授權使用此 Mod 而產生的任何後果，BanMod 及其開發者不承擔責任。\n\n請僅使用與 Vanilla 模式相容的功能，並遵守 Among Us 及其他使用中服務的規則。",
                ConfirmButton =
                    "接受",
                BackButton =
                    "拒絕"
            };
        }

        private static BanModServerTexts Filipino()
        {
            return new BanModServerTexts
            {
                SelectTitle =
                    "BANMOD - PILIIN ANG LOBBY MODE",
                SelectDescription =
                    "Piliin ang lobby mode bago gawin ang lobby.",
                ModdedDescription =
                    "Gamitin ang server na ito para magdagdag ng mga role at baguhin ang gameplay.",
                VanillaDescription =
                    "Gamitin ang server na ito kung anti-cheat at visual modifications lang ang ginagamit mo.",
                ModdedButton =
                    "MODDED +25\nINIREREKOMENDA",
                VanillaButton =
                    "VANILLA",
                PrivateFooter =
                    "Gamitin ang BanMod nang responsable at piliin ang mode na tumutugma sa lobby na iyong ginagawa.",
                VanillaTitle =
                    "MAHALAGA - VANILLA MODE",
                VanillaIntro =
                    "Pinili mo ang Vanilla mode.",
                VanillaWarning =
                    "Huwag gamitin ang BanMod para manggulo o mang-abala ng ibang manlalaro, at huwag i-enable ang mga option na nagbabago ng gameplay, nagbibigay ng hindi patas na advantage, o binabago ang experience ng ibang manlalaro.\n\nAng maling paggamit, hindi awtorisadong paggamit, o paggamit na labag sa rules ay maaaring magresulta sa warnings, restrictions, suspensions, bans, o iba pang sanctions mula sa Innersloth o iba pang serbisyong kasangkot.\n\nHindi mananagot ang BanMod at ang mga developer nito sa anumang kahihinatnan mula sa maling paggamit, ilegal, o hindi awtorisadong paggamit ng mod.\n\nGamitin lamang ang mga feature na compatible sa Vanilla mode at sundin ang rules ng Among Us at ng iba pang serbisyong ginagamit.",
                ConfirmButton =
                    "SUMASANG-AYON AKO",
                BackButton =
                    "HINDI AKO SUMASANG-AYON"
            };
        }

    }

    public class ServerSelectionMenu : MonoBehaviour
    {
        public static ServerSelectionMenu Instance;

        private bool showMenu = false;
        private bool showVanillaWarning = false;

        private bool wasCreateScreenOpen = false;
        private bool selectionShownForCurrentScreen = false;

        private float createScreenOpenTime = -1f;

        private Rect windowRect;
        private Vector2 warningScroll = Vector2.zero;

        private CreateGameOptions pendingCreateGame = null;

        private GUIStyle windowStyle;
        private GUIStyle titleStyle;
        private GUIStyle headerStyle;
        private GUIStyle textStyle;
        private GUIStyle mutedStyle;
        private GUIStyle buttonStyle;
        private GUIStyle sectionStyle;
        private GUIStyle warningStyle;

        private Texture2D roundedWindowTexture;
        private Texture2D roundedPanelTexture;
        private Texture2D roundedButtonTexture;

        private int lastScreenWidth;
        private int lastScreenHeight;

        private const float NormalWindowWidth = 760f;
        private const float NormalWindowHeight = 620f;
        private const float WarningWindowWidth = 860f;
        private const float WarningWindowHeight = 720f;

        public ServerSelectionMenu(IntPtr ptr) : base(ptr)
        {
        }

        void Awake()
        {
            Instance = this;
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            DestroyResources();
        }

        void Update()
        {
            if (BanMod.IsBanModDisabled)
            {
                CloseMenu();
                return;
            }

            if (Screen.width != lastScreenWidth ||
                Screen.height != lastScreenHeight)
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;

                if (showMenu)
                    CenterWindow();
            }

            bool createScreenOpen = false;

            try
            {
                GameObject createScreen =
                    GameObject.Find("CreateGameScreen");

                createScreenOpen =
                    createScreen != null &&
                    createScreen.activeInHierarchy;
            }
            catch
            {
                createScreenOpen = false;
            }

            if (createScreenOpen && !wasCreateScreenOpen)
            {
                wasCreateScreenOpen = true;
                selectionShownForCurrentScreen = false;

                createScreenOpenTime = Time.time;

                BanModServerSelection.VanillaAcknowledged = false;
                BanModServerSelection.VanillaCreateBypassOnce = false;

            }

            if (!createScreenOpen && wasCreateScreenOpen)
            {
                wasCreateScreenOpen = false;
                selectionShownForCurrentScreen = false;

                createScreenOpenTime = -1f;

                CloseMenu();

                BanModCreateGameServerSettingPatch
                    .CloseModeDropdown();


                return;
            }

            if (!createScreenOpen)
                return;

            BanModCreateGameServerSettingPatch.Refresh();

            if (selectionShownForCurrentScreen)
                return;

            if (createScreenOpenTime < 0f)
                return;

            if (Time.time - createScreenOpenTime < 0.35f)
                return;

            selectionShownForCurrentScreen = true;

            try
            {
                GameObject createScreen =
                    GameObject.Find("CreateGameScreen");

                if (createScreen == null)
                {
                    return;
                }

                CreateGameOptions options =
                    createScreen.GetComponent<CreateGameOptions>();

                if (options == null)
                {
                    options =
                        createScreen.GetComponentInChildren<CreateGameOptions>(
                            true
                        );
                }

                if (options == null)
                {
                    return;
                }


                BanModCreateGameServerSettingPatch.EnsureFor(
                    options
                );
            }
            catch
            {
            }
        }

        public void OpenMenu()
        {
            showVanillaWarning = false;
            warningScroll = Vector2.zero;
            pendingCreateGame = null;
            showMenu = true;

            CenterWindow();

        }

        public void OpenVanillaCreateWarning(
            CreateGameOptions createGameOptions)
        {
            if (createGameOptions == null)
                return;

            pendingCreateGame =
                createGameOptions;

            showVanillaWarning = true;
            warningScroll = Vector2.zero;
            showMenu = true;

            CenterWindow();

        }

        public void CloseMenu()
        {
            showMenu = false;
            showVanillaWarning = false;
            warningScroll = Vector2.zero;
            pendingCreateGame = null;
        }

        public bool IsOpen()
        {
            return showMenu;
        }

        private void CenterWindow()
        {
            float targetWidth = showVanillaWarning
                ? WarningWindowWidth
                : NormalWindowWidth;

            float targetHeight = showVanillaWarning
                ? WarningWindowHeight
                : NormalWindowHeight;

            float width = Mathf.Min(
                targetWidth,
                Mathf.Max(500f, Screen.width - 30f)
            );

            float height = Mathf.Min(
                targetHeight,
                Mathf.Max(420f, Screen.height - 30f)
            );

            windowRect = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height
            );
        }

        private void DestroyResources()
        {
            DestroyTexture(ref roundedWindowTexture);
            DestroyTexture(ref roundedPanelTexture);
            DestroyTexture(ref roundedButtonTexture);

            windowStyle = null;
            titleStyle = null;
            headerStyle = null;
            textStyle = null;
            mutedStyle = null;
            buttonStyle = null;
            sectionStyle = null;
            warningStyle = null;
        }

        private void DestroyTexture(ref Texture2D texture)
        {
            if (texture == null)
                return;

            try
            {
                UnityEngine.Object.Destroy(texture);
            }
            catch
            {
            }

            texture = null;
        }

        private void EnsureStyles()
        {
            if (roundedWindowTexture == null)
            {
                roundedWindowTexture = CreateRoundedTexture(
                    64,
                    18,
                    new Color(0.055f, 0.06f, 0.075f, 0.985f)
                );

                roundedPanelTexture = CreateRoundedTexture(
                    48,
                    14,
                    new Color(0.095f, 0.105f, 0.13f, 0.98f)
                );

                roundedButtonTexture = CreateRoundedTexture(
                    40,
                    12,
                    Color.white
                );
            }

            if (windowStyle == null)
            {
                windowStyle = new GUIStyle(GUI.skin.window);
                windowStyle.normal.background = roundedWindowTexture;
                windowStyle.onNormal.background = roundedWindowTexture;
                windowStyle.active.background = roundedWindowTexture;
                windowStyle.focused.background = roundedWindowTexture;
                windowStyle.normal.textColor = Color.white;
                windowStyle.border = new RectOffset
                {
                    left = 18,
                    right = 18,
                    top = 18,
                    bottom = 18
                };
                windowStyle.padding = new RectOffset
                {
                    left = 14,
                    right = 14,
                    top = 14,
                    bottom = 14
                };
            }

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.fontSize = 27;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.alignment = TextAnchor.MiddleLeft;
                titleStyle.wordWrap = true;
                titleStyle.normal.textColor = new Color(0.35f, 0.82f, 1f, 1f);
            }

            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.fontSize = 19;
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.alignment = TextAnchor.MiddleLeft;
                headerStyle.wordWrap = true;
                headerStyle.normal.textColor =
                    new Color(0.93f, 0.95f, 1f, 1f);
            }

            if (textStyle == null)
            {
                textStyle = new GUIStyle(GUI.skin.label);
                textStyle.fontSize = 16;
                textStyle.alignment = TextAnchor.UpperLeft;
                textStyle.wordWrap = true;
                textStyle.normal.textColor =
                    new Color(0.88f, 0.94f, 1f, 1f);
            }

            if (mutedStyle == null)
            {
                mutedStyle = new GUIStyle(textStyle);
                mutedStyle.fontSize = 14;
                mutedStyle.normal.textColor =
                    new Color(0.68f, 0.76f, 0.86f, 1f);
            }

            if (warningStyle == null)
            {
                warningStyle = new GUIStyle(textStyle);
                warningStyle.fontSize = 16;
                warningStyle.fontStyle = FontStyle.Bold;
                warningStyle.normal.textColor =
                    new Color(1f, 0.74f, 0.22f, 1f);
            }

            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.fontSize = 17;
                buttonStyle.fontStyle = FontStyle.Bold;
                buttonStyle.alignment = TextAnchor.MiddleCenter;
                buttonStyle.wordWrap = true;

                buttonStyle.normal.background = roundedButtonTexture;
                buttonStyle.hover.background = roundedButtonTexture;
                buttonStyle.active.background = roundedButtonTexture;
                buttonStyle.focused.background = roundedButtonTexture;
                buttonStyle.onNormal.background = roundedButtonTexture;
                buttonStyle.onHover.background = roundedButtonTexture;
                buttonStyle.onActive.background = roundedButtonTexture;
                buttonStyle.onFocused.background = roundedButtonTexture;

                buttonStyle.normal.textColor = Color.white;
                buttonStyle.hover.textColor = Color.white;
                buttonStyle.active.textColor = Color.white;
                buttonStyle.focused.textColor = Color.white;

                buttonStyle.border = new RectOffset
                {
                    left = 12,
                    right = 12,
                    top = 12,
                    bottom = 12
                };

                buttonStyle.padding = new RectOffset
                {
                    left = 12,
                    right = 12,
                    top = 6,
                    bottom = 6
                };
            }

            if (sectionStyle == null)
            {
                sectionStyle = new GUIStyle(GUI.skin.box);
                sectionStyle.normal.background = roundedPanelTexture;
                sectionStyle.normal.textColor = Color.white;
                sectionStyle.border = new RectOffset
                {
                    left = 14,
                    right = 14,
                    top = 14,
                    bottom = 14
                };
                sectionStyle.padding = new RectOffset
                {
                    left = 12,
                    right = 12,
                    top = 12,
                    bottom = 12
                };
            }
        }

        private Texture2D CreateRoundedTexture(
            int size,
            int radius,
            Color color)
        {
            Texture2D texture =
                new Texture2D(
                    size,
                    size,
                    TextureFormat.RGBA32,
                    false
                );

            texture.hideFlags = HideFlags.DontUnloadUnusedAsset;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[size * size];
            Color32 fill = color;

            float r = Mathf.Max(1f, radius);
            float leftCenter = r - 0.5f;
            float rightCenter = size - r - 0.5f;
            float bottomCenter = r - 0.5f;
            float topCenter = size - r - 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cx = x;
                    float cy = y;

                    if (x < radius)
                        cx = leftCenter;
                    else if (x >= size - radius)
                        cx = rightCenter;

                    if (y < radius)
                        cy = bottomCenter;
                    else if (y >= size - radius)
                        cy = topCenter;

                    float dx = x - cx;
                    float dy = y - cy;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);

                    float alphaFactor = 1f;

                    bool inCorner =
                        (x < radius || x >= size - radius) &&
                        (y < radius || y >= size - radius);

                    if (inCorner)
                        alphaFactor =
                            Mathf.Clamp01(r + 0.5f - distance);

                    Color32 pixel = fill;
                    pixel.a =
                        (byte)Mathf.RoundToInt(
                            fill.a * alphaFactor
                        );

                    pixels[y * size + x] = pixel;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        void OnGUI()
        {
            if (!showMenu)
                return;

            if (BanMod.IsBanModDisabled)
                return;

            EnsureStyles();

            Color oldColor = GUI.color;
            Color oldBackground = GUI.backgroundColor;
            Color oldContent = GUI.contentColor;
            int oldDepth = GUI.depth;

            try
            {
                GUI.depth = -1000;
                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;
                GUI.contentColor = Color.white;

                if (showVanillaWarning)
                {
                    windowRect = GUI.Window(
                        42101,
                        windowRect,
                        (GUI.WindowFunction)DrawVanillaWarning,
                        "",
                        windowStyle
                    );
                }
                else
                {
                    windowRect = GUI.Window(
                        42100,
                        windowRect,
                        (GUI.WindowFunction)DrawServerSelection,
                        "",
                        windowStyle
                    );
                }
            }
            catch
            {
            }
            finally
            {
                GUI.color = oldColor;
                GUI.backgroundColor = oldBackground;
                GUI.contentColor = oldContent;
                GUI.depth = oldDepth;
            }
        }

        private void DrawServerSelection(int id)
        {
            BanModServerTexts t =
                BanModServerLocalization.Get();

            float width = windowRect.width;
            float height = windowRect.height;

            GUI.Label(
                new Rect(20f, 10f, width - 40f, 42f),
                t.SelectTitle,
                titleStyle
            );

            Rect body = new Rect(
                18f,
                64f,
                width - 36f,
                height - 82f
            );

            GUI.Label(
                new Rect(body.x + 4f, body.y, body.width - 8f, 48f),
                t.SelectDescription,
                mutedStyle
            );

            float footerHeight = 64f;
            float cardsTop = body.y + 56f;
            float cardsBottom = body.y + body.height - footerHeight;
            float gap = 12f;
            float cardHeight = (cardsBottom - cardsTop - gap) * 0.5f;

            Rect moddedCard = new Rect(
                body.x,
                cardsTop,
                body.width,
                cardHeight
            );

            Rect vanillaCard = new Rect(
                body.x,
                cardsTop + cardHeight + gap,
                body.width,
                cardHeight
            );

            DrawPanel(moddedCard);
            DrawPanel(vanillaCard);

            GUIStyle moddedHeaderStyle = new GUIStyle(headerStyle);
            moddedHeaderStyle.normal.textColor =
                new Color(0.38f, 1f, 0.62f, 1f);

            GUI.Label(
                new Rect(
                    moddedCard.x + 18f,
                    moddedCard.y + 16f,
                    moddedCard.width - 36f,
                    56f
                ),
                t.ModdedDescription,
                moddedHeaderStyle
            );

            if (DrawTintedButton(
                new Rect(
                    moddedCard.x + 18f,
                    moddedCard.y + moddedCard.height - 72f,
                    moddedCard.width - 36f,
                    54f
                ),
                t.ModdedButton,
                new Color(0.12f, 0.58f, 0.34f, 1f)))
            {
                BanModServerSelection.Mode =
                    BanModServerMode.Modded25;

                BanModServerSelection.VanillaAcknowledged =
                    false;

                BanModCreateGameServerSettingPatch.Refresh();


                CloseMenu();
            }

            GUIStyle vanillaHeaderStyle = new GUIStyle(headerStyle);
            vanillaHeaderStyle.normal.textColor =
                new Color(1f, 0.72f, 0.30f, 1f);

            GUI.Label(
                new Rect(
                    vanillaCard.x + 18f,
                    vanillaCard.y + 16f,
                    vanillaCard.width - 36f,
                    56f
                ),
                t.VanillaDescription,
                vanillaHeaderStyle
            );

            if (DrawTintedButton(
                new Rect(
                    vanillaCard.x + 18f,
                    vanillaCard.y + vanillaCard.height - 72f,
                    vanillaCard.width - 36f,
                    54f
                ),
                t.VanillaButton,
                new Color(0.76f, 0.42f, 0.12f, 1f)))
            {
                BanModServerSelection.Mode =
                    BanModServerMode.Vanilla;

                BanModServerSelection.VanillaAcknowledged =
                    false;

                BanModCreateGameServerSettingPatch.Refresh();


                CloseMenu();
            }

            GUIStyle footerStyle = new GUIStyle(warningStyle);
            footerStyle.alignment = TextAnchor.MiddleCenter;
            footerStyle.fontSize = 14;
            footerStyle.normal.textColor =
                new Color(1f, 0.82f, 0.38f, 1f);

            GUI.Label(
                new Rect(
                    body.x + 10f,
                    body.y + body.height - 54f,
                    body.width - 20f,
                    46f
                ),
                t.PrivateFooter,
                footerStyle
            );

            GUI.DragWindow(
                new Rect(0f, 0f, width, 50f)
            );
        }

        private void DrawVanillaWarning(int id)
        {
            BanModServerTexts t =
                BanModServerLocalization.Get();

            float width = windowRect.width;
            float height = windowRect.height;

            GUI.Label(
                new Rect(20f, 10f, width - 40f, 42f),
                t.VanillaTitle,
                titleStyle
            );

            Rect body = new Rect(
                18f,
                64f,
                width - 36f,
                height - 82f
            );

            float footerHeight = 68f;

            Rect warningPanel = new Rect(
                body.x,
                body.y,
                body.width,
                body.height - footerHeight - 12f
            );

            DrawPanel(warningPanel);

            GUI.Label(
                new Rect(
                    warningPanel.x + 18f,
                    warningPanel.y + 14f,
                    warningPanel.width - 36f,
                    36f
                ),
                t.VanillaIntro,
                warningStyle
            );

            Rect scrollArea = new Rect(
                warningPanel.x + 14f,
                warningPanel.y + 56f,
                warningPanel.width - 28f,
                warningPanel.height - 70f
            );

            float textWidth = Mathf.Max(100f, scrollArea.width - 24f);
            float contentHeight = Mathf.Max(
                scrollArea.height,
                textStyle.CalcHeight(
                    new GUIContent(t.VanillaWarning),
                    textWidth
                ) + 18f
            );

            Rect contentRect = new Rect(
                0f,
                0f,
                textWidth,
                contentHeight
            );

            warningScroll = GUI.BeginScrollView(
                scrollArea,
                warningScroll,
                contentRect
            );

            GUI.Label(
                new Rect(
                    4f,
                    4f,
                    textWidth - 8f,
                    contentHeight - 8f
                ),
                t.VanillaWarning,
                textStyle
            );

            GUI.EndScrollView();

            Rect footer = new Rect(
                body.x,
                body.y + body.height - footerHeight,
                body.width,
                footerHeight
            );

            float half = (footer.width - 10f) * 0.5f;

            if (DrawTintedButton(
                new Rect(
                    footer.x,
                    footer.y + 8f,
                    half,
                    48f
                ),
                t.ConfirmButton,
                new Color(0.12f, 0.58f, 0.34f, 1f)))
            {
                CreateGameOptions createGame =
                    pendingCreateGame;

                BanModServerSelection.Mode =
                    BanModServerMode.Vanilla;

                BanModServerSelection.SaveVanillaAcceptance();

                BanModServerSelection.VanillaCreateBypassOnce =
                    true;

                BanModCreateGameServerSettingPatch.Refresh();


                CloseMenu();

                ResumeCreateGame(
                    createGame
                );
            }

            if (DrawTintedButton(
                new Rect(
                    footer.x + half + 10f,
                    footer.y + 8f,
                    half,
                    48f
                ),
                t.BackButton,
                new Color(0.18f, 0.19f, 0.23f, 1f)))
            {
                CreateGameOptions createGame =
                    pendingCreateGame;

                BanModServerSelection.Mode =
                    BanModServerMode.Modded25;

                BanModServerSelection.VanillaAcknowledged =
                    false;

                BanModServerSelection.VanillaCreateBypassOnce =
                    false;

                BanModCreateGameServerSettingPatch.Refresh();


                CloseMenu();

                ResumeCreateGame(
                    createGame
                );
            }

            GUI.DragWindow(
                new Rect(0f, 0f, width, 50f)
            );
        }

        private void ResumeCreateGame(
            CreateGameOptions createGame)
        {
            if (createGame == null)
            {
                return;
            }

            try
            {
                createGame.Confirm();
            }
            catch
            {
            }
        }

        private bool DrawTintedButton(
            Rect rect,
            string text,
            Color color)
        {
            Color oldBackground = GUI.backgroundColor;
            GUI.backgroundColor = color;

            bool clicked = GUI.Button(
                rect,
                text,
                buttonStyle
            );

            GUI.backgroundColor = oldBackground;
            return clicked;
        }

        private void DrawPanel(Rect rect)
        {
            Color oldBackground = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;

            GUI.Box(
                rect,
                GUIContent.none,
                sectionStyle
            );

            GUI.backgroundColor = oldBackground;
        }
    }

    [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Show))]
    public static class BanModCreateGameServerSettingPatch
    {
        private static CreateGameOptions currentCreateScreen;

        private static GameObject modeRowRoot;

        private static GameObject serverLabelRoot;

        private static PassiveButton vanillaButton;
        private static PassiveButton moddedButton;

        private static TMP_Text serverHoverDescriptionText;
        private static string serverHoverOriginalText = "";
        private static bool serverHoverTextCaptured = false;

        private const float YOffset = 0.72f;
        private const float ExtendedScrollMax = 4.0f;
        private const float AprilFoolsServerExtraOffset = 0.45f;
        private static Vector3 serverRowBasePosition;
        private static Vector3 serverLabelBasePosition;

        private static bool AreAprilFoolsControlsVisible(CreateGameOptions screen)
        {
            if (screen == null) return false;

            foreach (PassiveButton button in
                TryGetRuntimePassiveButtonCollection(screen, "aprilFoolButtons"))
            {
                if (button != null && button.gameObject.activeInHierarchy)
                    return true;
            }

            object toggle = TryGetRuntimeMemberObject(screen, "AprilFoolsToggle");
            GameObject toggleObject = toggle as GameObject;
            Component toggleComponent = toggle as Component;
            if (toggleObject == null && toggleComponent != null)
                toggleObject = toggleComponent.gameObject;
            return toggleObject != null && toggleObject.activeInHierarchy;
        }

        private static void RefreshServerRowPosition()
        {
            if (currentCreateScreen == null || !IsAlive(modeRowRoot)) return;

            // Always derive positions from the original layout; never accumulate offsets.
            Vector3 offset = AreAprilFoolsControlsVisible(currentCreateScreen)
                ? currentCreateScreen.transform.TransformVector(
                    new Vector3(0f, -AprilFoolsServerExtraOffset, 0f))
                : Vector3.zero;

            Transform row = modeRowRoot.transform;
            row.localPosition = serverRowBasePosition +
                (row.parent != null ? row.parent.InverseTransformVector(offset) : offset);

            if (IsAlive(serverLabelRoot))
            {
                Transform label = serverLabelRoot.transform;
                label.localPosition = serverLabelBasePosition +
                    (label.parent != null ? label.parent.InverseTransformVector(offset) : offset);
            }
        }


        [HarmonyPostfix]
        public static void Postfix(
            CreateGameOptions __instance)
        {

            EnsureFor(
                __instance
            );
        }

        public static void EnsureFor(
            CreateGameOptions createScreen)
        {
            if (createScreen == null)
            {
                return;
            }

            if (BanMod.IsBanModDisabled)
                return;

            if (BanModServerSelection.Mode ==
                BanModServerMode.None)
            {
                BanModServerSelection.Mode =
                    BanModServerMode.Modded25;
            }

            try
            {
                bool screenChanged =
                    currentCreateScreen != null &&
                    currentCreateScreen != createScreen;

                currentCreateScreen =
                    createScreen;

                if (screenChanged)
                {
                    DestroyServerUi();
                }

                ExtendScroll(
                    createScreen
                );

                if (!IsAlive(modeRowRoot) ||
                    vanillaButton == null ||
                    moddedButton == null)
                {
                    DestroyServerUi();


                    BuildServerRow(
                        createScreen
                    );
                }
                else
                {
                    SetRowActive(
                        true
                    );
                }

                Refresh();
            }
            catch
            {
            }
        }

        private static void BuildServerRow(
            CreateGameOptions createScreen)
        {
            GameObject regionButton =
                TryGetRuntimeMember<GameObject>(
                    createScreen,
                    "serverButton"
                );

            if (regionButton == null)
            {
                regionButton =
                    FindRegionButtonFromHierarchy(
                        createScreen
                    );
            }

            if (regionButton == null)
            {

                LogUsefulButtons(
                    createScreen
                );

                return;
            }


            List<PassiveButton> originalModeButtons =
                TryGetRuntimePassiveButtonCollection(
                    createScreen,
                    "modeButtons"
                );

            if (originalModeButtons.Count < 2)
            {
                originalModeButtons =
                    FindModeButtonsFromHierarchy(
                        createScreen
                    );
            }

            if (originalModeButtons.Count < 2)
            {

                LogUsefulButtons(
                    createScreen
                );

                return;
            }

            PassiveButton originalLeft =
                originalModeButtons[0];

            PassiveButton originalRight =
                originalModeButtons[1];

            if (originalLeft == null ||
                originalRight == null)
            {

                return;
            }

            serverHoverDescriptionText =
                FindServerHoverDescriptionText(
                    createScreen,
                    originalLeft,
                    originalRight
                );

            if (serverHoverDescriptionText != null)
            {
                serverHoverOriginalText =
                    serverHoverDescriptionText.text ?? "";

                serverHoverTextCaptured =
                    true;

            }
            else
            {
                serverHoverOriginalText = "";
                serverHoverTextCaptured = false;

            }

            Transform buttonParent =
                originalLeft.transform.parent;

            if (buttonParent == null)
            {

                return;
            }

            Transform regionParent =
                regionButton.transform.parent;

            if (regionParent == null)
                return;

            Vector3 regionTargetLocal =
                regionButton.transform.localPosition;

            regionTargetLocal.y -=
                YOffset;

            Vector3 targetWorld =
                regionParent.TransformPoint(
                    regionTargetLocal
                );

            float targetYInModeParent =
                buttonParent.InverseTransformPoint(
                    targetWorld
                ).y;

            modeRowRoot =
                new GameObject(
                    "BanModServerChoiceRow"
                );

            modeRowRoot.transform.SetParent(
                buttonParent,
                false
            );

            modeRowRoot.transform.localPosition =
                Vector3.zero;

            modeRowRoot.transform.localRotation =
                Quaternion.identity;

            modeRowRoot.transform.localScale =
                Vector3.one;

            GameObject vanillaObject =
                CloneModeButton(
                    originalLeft,
                    modeRowRoot.transform,
                    targetYInModeParent,
                    "BanModServerVanilla",
                    "Vanilla",
                    true,
                    SelectVanilla
                );

            GameObject moddedObject =
                CloneModeButton(
                    originalRight,
                    modeRowRoot.transform,
                    targetYInModeParent,
                    "BanModServerModded",
                    "Modded",
                    false,
                    SelectModded
                );

            if (!IsAlive(vanillaObject) ||
                !IsAlive(moddedObject))
            {

                DestroyServerUi();
                return;
            }

            vanillaButton =
                vanillaObject.GetComponent<PassiveButton>();

            if (vanillaButton == null)
            {
                vanillaButton =
                    vanillaObject
                        .GetComponentInChildren<PassiveButton>(
                            true
                        );
            }

            moddedButton =
                moddedObject.GetComponent<PassiveButton>();

            if (moddedButton == null)
            {
                moddedButton =
                    moddedObject
                        .GetComponentInChildren<PassiveButton>(
                            true
                        );
            }

            if (vanillaButton == null ||
                moddedButton == null)
            {

                DestroyServerUi();
                return;
            }

            serverLabelRoot =
                CloneServerLabel(
                    createScreen,
                    regionButton
                );

            serverRowBasePosition = modeRowRoot.transform.localPosition;
            if (IsAlive(serverLabelRoot))
                serverLabelBasePosition = serverLabelRoot.transform.localPosition;

            modeRowRoot.SetActive(
                true
            );

            Refresh();

        }

        private static GameObject CloneModeButton(
            PassiveButton original,
            Transform newParent,
            float targetY,
            string objectName,
            string displayText,
            bool isVanilla,
            Action onClick)
        {
            if (original == null ||
                newParent == null)
                return null;

            GameObject clone =
                UnityEngine.Object.Instantiate(
                    original.gameObject,
                    newParent
                );

            clone.name =
                objectName;

            clone.SetActive(
                true
            );

            Vector3 local =
                original.transform.localPosition;

            local.y =
                targetY;

            clone.transform.localPosition =
                local;

            clone.transform.localRotation =
                original.transform.localRotation;

            clone.transform.localScale =
                original.transform.localScale;

            DisableLocalizationComponents(
                clone
            );

            TMP_Text[] texts =
                clone.GetComponentsInChildren<TMP_Text>(
                    true
                );

            if (texts != null)
            {
                for (int i = 0;
                     i < texts.Length;
                     i++)
                {
                    TMP_Text tmp =
                        texts[i];

                    if (tmp == null)
                        continue;

                    tmp.text =
                        displayText;
                }
            }

            PassiveButton[] buttons =
                clone.GetComponentsInChildren<PassiveButton>(
                    true
                );

            if (buttons == null ||
                buttons.Length == 0)
            {
                UnityEngine.Object.Destroy(
                    clone
                );

                return null;
            }

            for (int i = 0;
                 i < buttons.Length;
                 i++)
            {
                PassiveButton button =
                    buttons[i];

                if (button == null)
                    continue;

                try
                {
                    button.OnClick =
                        new UnityEngine.UI.Button.ButtonClickedEvent();

                    button.OnClick.AddListener(
                        onClick
                    );

                    BindServerHoverEvents(
                        button,
                        isVanilla
                    );
                }
                catch
                {
                }
            }

            return clone;
        }

        private static void BindServerHoverEvents(
            PassiveButton button,
            bool isVanilla)
        {
            if (button == null)
                return;

            try
            {


                button.OnMouseOver =
                    new UnityEngine.Events.UnityEvent();

                button.OnMouseOut =
                    new UnityEngine.Events.UnityEvent();

                button.OnMouseOver.AddListener(
                    (UnityEngine.Events.UnityAction)(() =>
                    {
                        ShowServerHoverDescription(
                            isVanilla
                        );
                    })
                );

                button.OnMouseOut.AddListener(
                    (UnityEngine.Events.UnityAction)(() =>
                    {
                        RestoreServerHoverDescription();
                    })
                );

            }
            catch
            {
            }
        }

        private static void ShowServerHoverDescription(
            bool isVanilla)
        {
            if (serverHoverDescriptionText == null)
                return;

            try
            {


                serverHoverOriginalText =
                    serverHoverDescriptionText.text ?? "";

                serverHoverTextCaptured =
                    true;

                BanModServerTexts texts =
                    BanModServerLocalization.Get();

                serverHoverDescriptionText.text =
                    isVanilla
                        ? texts.VanillaDescription
                        : texts.ModdedDescription;
            }
            catch
            {
            }
        }

        private static void RestoreServerHoverDescription()
        {
            if (serverHoverDescriptionText == null ||
                !serverHoverTextCaptured)
                return;

            try
            {
                serverHoverDescriptionText.text =
                    serverHoverOriginalText;
            }
            catch
            {
            }
        }

        private static TMP_Text FindServerHoverDescriptionText(
            CreateGameOptions createScreen,
            PassiveButton originalLeft,
            PassiveButton originalRight)
        {
            if (createScreen == null)
                return null;


            TMP_Text detected =
                DetectDescriptionTextFromOriginalHover(
                    createScreen,
                    originalLeft
                );

            if (detected == null)
            {
                detected =
                    DetectDescriptionTextFromOriginalHover(
                        createScreen,
                        originalRight
                    );
            }

            if (detected != null)
            {

                return detected;
            }


            return FindServerHoverDescriptionTextFallback(
                createScreen,
                originalLeft,
                originalRight
            );
        }

        private static TMP_Text DetectDescriptionTextFromOriginalHover(
            CreateGameOptions createScreen,
            PassiveButton originalButton)
        {
            if (createScreen == null ||
                originalButton == null)
                return null;

            TMP_Text[] allTexts =
                createScreen.GetComponentsInChildren<TMP_Text>(
                    true
                );

            if (allTexts == null ||
                allTexts.Length == 0)
                return null;

            string[] before =
                new string[allTexts.Length];

            for (int i = 0; i < allTexts.Length; i++)
            {
                TMP_Text text =
                    allTexts[i];

                before[i] =
                    text != null
                        ? (text.text ?? "")
                        : "";
            }

            try
            {
                if (originalButton.OnMouseOver == null)
                    return null;

                originalButton.OnMouseOver.Invoke();

                TMP_Text best = null;
                float bestScore = float.MaxValue;

                Vector3 buttonLocal =
                    createScreen.transform.InverseTransformPoint(
                        originalButton.transform.position
                    );

                for (int i = 0; i < allTexts.Length; i++)
                {
                    TMP_Text candidate =
                        allTexts[i];

                    if (candidate == null)
                        continue;

                    if (candidate.transform.IsChildOf(
                            originalButton.transform
                        ))
                    {
                        continue;
                    }

                    string oldValue =
                        before[i] ?? "";

                    string newValue =
                        candidate.text ?? "";

                    if (oldValue == newValue)
                        continue;


                    if (newValue.Length < 8 &&
                        oldValue.Length < 8)
                        continue;

                    string hierarchy =
                        BuildHierarchyName(
                            candidate.transform,
                            createScreen.transform
                        );

                    Vector3 local =
                        createScreen.transform.InverseTransformPoint(
                            candidate.transform.position
                        );

                    float score = 0f;


                    if (local.y > buttonLocal.y)
                        score -= 3000f;
                    else
                        score += 3000f;

                    score +=
                        Mathf.Abs(local.x - buttonLocal.x) * 30f;

                    score +=
                        Mathf.Abs(local.y - buttonLocal.y) * 80f;

                    if (ContainsIgnoreCase(hierarchy, "desc") ||
                        ContainsIgnoreCase(hierarchy, "description"))
                    {
                        score -= 1200f;
                    }

                    if (ContainsIgnoreCase(hierarchy, "chat"))
                        score += 8000f;

                    if (ContainsIgnoreCase(hierarchy, "title") ||
                        ContainsIgnoreCase(hierarchy, "label"))
                    {
                        score += 1000f;
                    }

                    if (oldValue.Length >= 15)
                        score -= 300f;

                    if (newValue.Length >= 15)
                        score -= 300f;


                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = candidate;
                    }
                }

                return best;
            }
            catch
            {

                return null;
            }
            finally
            {
                try
                {
                    if (originalButton.OnMouseOut != null)
                        originalButton.OnMouseOut.Invoke();
                }
                catch
                {
                }
            }
        }

        private static TMP_Text FindServerHoverDescriptionTextFallback(
            CreateGameOptions createScreen,
            PassiveButton originalLeft,
            PassiveButton originalRight)
        {
            if (createScreen == null)
                return null;

            string[] runtimeNames =
            {
                "modeDescriptionText",
                "gameModeDescriptionText",
                "gameTypeDescriptionText",
                "descriptionText",
                "modeDescription",
                "gameModeDescription",
                "gameTypeDescription"
            };

            for (int i = 0;
                 i < runtimeNames.Length;
                 i++)
            {
                TMP_Text runtimeText =
                    TryGetRuntimeMember<TMP_Text>(
                        createScreen,
                        runtimeNames[i]
                    );

                if (runtimeText != null)
                    return runtimeText;
            }

            TMP_Text[] allTexts =
                createScreen.GetComponentsInChildren<TMP_Text>(
                    true
                );

            if (allTexts == null ||
                allTexts.Length == 0)
                return null;

            Vector3 leftLocal =
                createScreen.transform.InverseTransformPoint(
                    originalLeft.transform.position
                );

            Vector3 rightLocal =
                createScreen.transform.InverseTransformPoint(
                    originalRight.transform.position
                );

            float buttonCenterX =
                (leftLocal.x + rightLocal.x) * 0.5f;

            float buttonY =
                (leftLocal.y + rightLocal.y) * 0.5f;

            TMP_Text best = null;
            float bestScore = float.MaxValue;

            for (int i = 0;
                 i < allTexts.Length;
                 i++)
            {
                TMP_Text candidate =
                    allTexts[i];

                if (candidate == null)
                    continue;

                if ((originalLeft != null &&
                     candidate.transform.IsChildOf(
                         originalLeft.transform
                     )) ||
                    (originalRight != null &&
                     candidate.transform.IsChildOf(
                         originalRight.transform
                     )))
                {
                    continue;
                }

                string value =
                    candidate.text ?? "";

                if (string.IsNullOrWhiteSpace(value))
                    continue;

                if (value.Length < 8)
                    continue;

                string hierarchy =
                    BuildHierarchyName(
                        candidate.transform,
                        createScreen.transform
                    );


                if (ContainsIgnoreCase(hierarchy, "chat"))
                    continue;

                Vector3 local =
                    createScreen.transform.InverseTransformPoint(
                        candidate.transform.position
                    );

                float xDistance =
                    Mathf.Abs(
                        local.x - buttonCenterX
                    );

                float yDistance =
                    Mathf.Abs(
                        local.y - buttonY
                    );

                float score =
                    yDistance * 1000f +
                    xDistance * 100f;

                if (ContainsIgnoreCase(hierarchy, "description") ||
                    ContainsIgnoreCase(hierarchy, "desc"))
                {
                    score -= 5000f;
                }

                if (ContainsIgnoreCase(hierarchy, "mode") ||
                    ContainsIgnoreCase(hierarchy, "game"))
                {
                    score -= 500f;
                }

                if (ContainsIgnoreCase(hierarchy, "title") ||
                    ContainsIgnoreCase(hierarchy, "label"))
                {
                    score += 1200f;
                }

                if (local.y > buttonY)
                    score -= 300f;
                else
                    score += 800f;

                if (value.Length >= 20)
                    score -= 250f;

                if (value.Length > 180)
                    score += 1200f;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        private static void DisableLocalizationComponents(
            GameObject root)
        {
            if (!IsAlive(root))
                return;

            Component[] components =
                root.GetComponentsInChildren<Component>(
                    true
                );

            if (components == null)
                return;

            for (int i = 0;
                 i < components.Length;
                 i++)
            {
                Component component =
                    components[i];

                if (component == null)
                    continue;

                string typeName =
                    component.GetType().Name ?? "";

                if (typeName != "TextTranslatorTMP" &&
                    typeName != "PlatformTextTranslationTMP")
                {
                    continue;
                }

                try
                {
                    Behaviour behaviour =
                        component as Behaviour;

                    if (behaviour != null)
                        behaviour.enabled = false;
                }
                catch
                {
                }

                try
                {
                    UnityEngine.Object.Destroy(
                        component
                    );
                }
                catch
                {
                }

            }
        }

        private static void ForceChoiceButtonText(
            PassiveButton button,
            string textValue)
        {
            if (button == null)
                return;

            GameObject root =
                button.gameObject;

            if (!IsAlive(root))
                return;

            TMP_Text[] texts =
                root.GetComponentsInChildren<TMP_Text>(
                    true
                );

            if (texts == null)
                return;

            for (int i = 0;
                 i < texts.Length;
                 i++)
            {
                TMP_Text text =
                    texts[i];

                if (text != null)
                    text.text =
                        textValue;
            }
        }

        private static void SelectVanilla()
        {
            BanModServerSelection.Mode =
                BanModServerMode.Vanilla;

            BanModServerSelection.VanillaAcknowledged =
                false;

            BanModServerSelection.VanillaCreateBypassOnce =
                false;

            Refresh();
        }

        private static void SelectModded()
        {
            BanModServerSelection.Mode =
                BanModServerMode.Modded25;

            BanModServerSelection.VanillaAcknowledged =
                false;

            BanModServerSelection.VanillaCreateBypassOnce =
                false;

            Options.UpdateGameModesForServer();

            Refresh();
        }

        public static void Refresh()
        {
            RefreshServerRowPosition();
            if (BanModServerSelection.Mode ==
                BanModServerMode.None)
            {
                BanModServerSelection.Mode =
                    BanModServerMode.Modded25;
            }

            if (vanillaButton != null)
            {
                try
                {
                    vanillaButton.SelectButton(
                        BanModServerSelection.IsVanilla
                    );
                }
                catch
                {
                }
            }

            if (moddedButton != null)
            {
                try
                {
                    moddedButton.SelectButton(
                        !BanModServerSelection.IsVanilla
                    );
                }
                catch
                {
                }
            }

            ForceChoiceButtonText(
                vanillaButton,
                "Vanilla"
            );

            ForceChoiceButtonText(
                moddedButton,
                "Modded"
            );

            ForceServerLabel();
        }

        public static void CloseModeDropdown()
        {
            SetRowActive(
                true
            );
        }

        private static void SetRowActive(
            bool active)
        {
            if (IsAlive(modeRowRoot))
                modeRowRoot.SetActive(active);

            if (IsAlive(serverLabelRoot))
                serverLabelRoot.SetActive(active);
        }

        private static void DestroyServerUi()
        {
            if (IsAlive(modeRowRoot))
            {
                try
                {
                    UnityEngine.Object.Destroy(
                        modeRowRoot
                    );
                }
                catch
                {
                }
            }

            if (IsAlive(serverLabelRoot))
            {
                try
                {
                    UnityEngine.Object.Destroy(
                        serverLabelRoot
                    );
                }
                catch
                {
                }
            }

            modeRowRoot =
                null;

            serverLabelRoot =
                null;

            vanillaButton =
                null;

            moddedButton =
                null;

            serverHoverDescriptionText =
                null;

            serverHoverOriginalText =
                "";

            serverHoverTextCaptured =
                false;
        }

        private static GameObject CloneServerLabel(
            CreateGameOptions createScreen,
            GameObject regionButton)
        {
            if (createScreen == null ||
                regionButton == null)
                return null;

            TMP_Text[] allTexts =
                createScreen.GetComponentsInChildren<TMP_Text>(
                    true
                );

            if (allTexts == null ||
                allTexts.Length == 0)
                return null;

            Vector3 regionLocal =
                createScreen.transform.InverseTransformPoint(
                    regionButton.transform.position
                );

            TMP_Text best =
                null;

            float bestScore =
                float.MaxValue;

            for (int i = 0;
                 i < allTexts.Length;
                 i++)
            {
                TMP_Text candidate =
                    allTexts[i];

                if (candidate == null)
                    continue;

                if (candidate.transform ==
                    regionButton.transform ||
                    candidate.transform.IsChildOf(
                        regionButton.transform))
                {
                    continue;
                }

                string value =
                    candidate.text ?? "";

                if (string.IsNullOrEmpty(value) ||
                    value.Length > 40)
                    continue;

                Vector3 local =
                    createScreen.transform
                        .InverseTransformPoint(
                            candidate.transform.position
                        );

                float yDistance =
                    Mathf.Abs(
                        local.y - regionLocal.y
                    );

                float xDistance =
                    Mathf.Abs(
                        local.x - regionLocal.x
                    );

                float score =
                    yDistance * 1000f +
                    xDistance;


                if (local.x < regionLocal.x)
                    score -= 50f;
                else
                    score += 400f;

                string hierarchy =
                    BuildHierarchyName(
                        candidate.transform,
                        createScreen.transform
                    );

                if (ContainsIgnoreCase(value, "region") ||
                    ContainsIgnoreCase(value, "regione") ||
                    ContainsIgnoreCase(value, "région") ||
                    ContainsIgnoreCase(value, "región") ||
                    ContainsIgnoreCase(hierarchy, "region"))
                {
                    score -= 5000f;
                }

                if (score < bestScore)
                {
                    bestScore =
                        score;

                    best =
                        candidate;
                }
            }

            if (best == null)
            {

                return null;
            }

            Transform labelRoot =
                FindLabelBlockRoot(
                    best.transform,
                    createScreen.transform,
                    regionButton.transform
                );

            if (labelRoot == null ||
                labelRoot.parent == null)
                return null;

            GameObject clone =
                UnityEngine.Object.Instantiate(
                    labelRoot.gameObject,
                    labelRoot.parent
                );

            clone.name =
                "BanModServerLabel";

            Vector3 pos =
                labelRoot.localPosition;

            pos.y -=
                YOffset;

            clone.transform.localPosition =
                pos;

            clone.transform.localRotation =
                labelRoot.localRotation;

            clone.transform.localScale =
                labelRoot.localScale;

            clone.SetActive(
                true
            );

            TMP_Text[] clonedTexts =
                clone.GetComponentsInChildren<TMP_Text>(
                    true
                );

            if (clonedTexts != null)
            {
                for (int i = 0;
                     i < clonedTexts.Length;
                     i++)
                {
                    TMP_Text label =
                        clonedTexts[i];

                    if (label != null)
                        label.text =
                            "SERVER";
                }
            }


            return clone;
        }

        private static Transform FindLabelBlockRoot(
            Transform labelText,
            Transform screenRoot,
            Transform regionButton)
        {
            if (labelText == null)
                return null;

            Transform best =
                labelText;

            Transform current =
                labelText.parent;

            int guard =
                0;

            while (current != null &&
                   current != screenRoot &&
                   guard < 3)
            {
                if (regionButton != null &&
                    (current == regionButton ||
                     regionButton.IsChildOf(current)))
                {
                    break;
                }

                try
                {
                    PassiveButton[] buttons =
                        current.GetComponentsInChildren<PassiveButton>(
                            true
                        );

                    TMP_Text[] texts =
                        current.GetComponentsInChildren<TMP_Text>(
                            true
                        );

                    int buttonCount =
                        buttons == null
                            ? 0
                            : buttons.Length;

                    int textCount =
                        texts == null
                            ? 0
                            : texts.Length;

                    if (buttonCount == 0 &&
                        textCount <= 3)
                    {
                        best =
                            current;
                    }
                    else
                    {
                        break;
                    }
                }
                catch
                {
                    break;
                }

                current =
                    current.parent;

                guard++;
            }

            return best;
        }

        private static void ForceServerLabel()
        {
            if (!IsAlive(serverLabelRoot))
                return;

            TMP_Text[] texts =
                serverLabelRoot.GetComponentsInChildren<TMP_Text>(
                    true
                );

            if (texts == null)
                return;

            for (int i = 0;
                 i < texts.Length;
                 i++)
            {
                if (texts[i] != null)
                    texts[i].text =
                        "SERVER";
            }
        }

        private static void ExtendScroll(
            CreateGameOptions createScreen)
        {
            float scrollMax = ExtendedScrollMax +
                (AreAprilFoolsControlsVisible(createScreen) ? AprilFoolsServerExtraOffset : 0f);

            Scroller runtimeScroller =
                TryGetRuntimeMember<Scroller>(
                    createScreen,
                    "scrollBar"
                );

            if (runtimeScroller != null)
            {
                try
                {
                    runtimeScroller.SetYBoundsMax(
                        scrollMax
                    );
                }
                catch
                {
                }

                return;
            }

            Scroller[] scrollers =
                createScreen.GetComponentsInChildren<Scroller>(
                    true
                );

            if (scrollers == null)
                return;

            for (int i = 0;
                 i < scrollers.Length;
                 i++)
            {
                Scroller scroller =
                    scrollers[i];

                if (scroller == null)
                    continue;

                try
                {
                    scroller.SetYBoundsMax(
                        scrollMax
                    );
                }
                catch
                {
                }
            }
        }

        private static T TryGetRuntimeMember<T>(
            object instance,
            string name)
            where T : class
        {
            object value =
                TryGetRuntimeMemberObject(
                    instance,
                    name
                );

            return value as T;
        }

        private static object TryGetRuntimeMemberObject(
            object instance,
            string name)
        {
            if (instance == null ||
                string.IsNullOrEmpty(name))
                return null;

            Type type =
                instance.GetType();

            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic;

            try
            {
                System.Reflection.FieldInfo field =
                    type.GetField(
                        name,
                        flags
                    );

                if (field != null)
                {
                    return field.GetValue(
                        instance
                    );
                }
            }
            catch
            {
            }

            try
            {
                System.Reflection.PropertyInfo property =
                    type.GetProperty(
                        name,
                        flags
                    );

                if (property != null &&
                    property.GetIndexParameters().Length == 0)
                {
                    return property.GetValue(
                        instance,
                        null
                    );
                }
            }
            catch
            {
            }

            return null;
        }

        private static List<PassiveButton>
            TryGetRuntimePassiveButtonCollection(
                object instance,
                string name)
        {
            List<PassiveButton> result =
                new List<PassiveButton>();

            object value =
                TryGetRuntimeMemberObject(
                    instance,
                    name
                );

            if (value == null)
                return result;

            PassiveButton single =
                value as PassiveButton;

            if (single != null)
            {
                result.Add(
                    single
                );

                return result;
            }

            System.Collections.IEnumerable enumerable =
                value as System.Collections.IEnumerable;

            if (enumerable != null)
            {
                try
                {
                    foreach (object item in enumerable)
                    {
                        PassiveButton button =
                            item as PassiveButton;

                        if (button != null &&
                            !result.Contains(button))
                        {
                            result.Add(
                                button
                            );
                        }
                    }
                }
                catch
                {
                }

                if (result.Count > 0)
                    return result;
            }

            try
            {
                Type collectionType =
                    value.GetType();

                System.Reflection.PropertyInfo lengthProperty =
                    collectionType.GetProperty(
                        "Length"
                    );

                System.Reflection.PropertyInfo itemProperty =
                    collectionType.GetProperty(
                        "Item"
                    );

                if (lengthProperty != null &&
                    itemProperty != null)
                {
                    int length =
                        Convert.ToInt32(
                            lengthProperty.GetValue(
                                value,
                                null
                            )
                        );

                    for (int i = 0;
                         i < length;
                         i++)
                    {
                        object item =
                            itemProperty.GetValue(
                                value,
                                new object[] { i }
                            );

                        PassiveButton button =
                            item as PassiveButton;

                        if (button != null &&
                            !result.Contains(button))
                        {
                            result.Add(
                                button
                            );
                        }
                    }
                }
            }
            catch
            {
            }

            return result;
        }

        private static GameObject FindRegionButtonFromHierarchy(
            CreateGameOptions createScreen)
        {
            PassiveButton[] buttons =
                createScreen.GetComponentsInChildren<PassiveButton>(
                    true
                );

            if (buttons == null)
                return null;

            PassiveButton best =
                null;

            float bestScore =
                float.MaxValue;

            for (int i = 0;
                 i < buttons.Length;
                 i++)
            {
                PassiveButton button =
                    buttons[i];

                if (button == null)
                    continue;

                string hierarchy =
                    BuildHierarchyName(
                        button.transform,
                        createScreen.transform
                    );

                float score =
                    1000f;

                if (ContainsIgnoreCase(
                        hierarchy,
                        "serverbox"))
                {
                    score -=
                        900f;
                }

                if (ContainsIgnoreCase(
                        hierarchy,
                        "server"))
                {
                    score -=
                        500f;
                }

                if (ContainsIgnoreCase(
                        hierarchy,
                        "region"))
                {
                    score -=
                        400f;
                }

                if (score < bestScore)
                {
                    bestScore =
                        score;

                    best =
                        button;
                }
            }

            return best != null
                ? best.gameObject
                : null;
        }

        private static List<PassiveButton>
            FindModeButtonsFromHierarchy(
                CreateGameOptions createScreen)
        {
            List<PassiveButton> result =
                new List<PassiveButton>();

            PassiveButton[] buttons =
                createScreen.GetComponentsInChildren<PassiveButton>(
                    true
                );

            if (buttons == null)
                return result;

            PassiveButton classic =
                null;

            PassiveButton hide =
                null;

            for (int i = 0;
                 i < buttons.Length;
                 i++)
            {
                PassiveButton button =
                    buttons[i];

                if (button == null)
                    continue;

                string hierarchy =
                    BuildHierarchyName(
                        button.transform,
                        createScreen.transform
                    );

                string name =
                    button.gameObject.name ?? "";

                if (classic == null &&
                    (ContainsIgnoreCase(name, "classic") ||
                     ContainsIgnoreCase(hierarchy, "classic")))
                {
                    classic =
                        button;
                }

                if (hide == null &&
                    (ContainsIgnoreCase(name, "hide") ||
                     ContainsIgnoreCase(name, "seek") ||
                     ContainsIgnoreCase(hierarchy, "hide") ||
                     ContainsIgnoreCase(hierarchy, "seek")))
                {
                    hide =
                        button;
                }
            }

            if (classic != null)
                result.Add(classic);

            if (hide != null &&
                hide != classic)
            {
                result.Add(hide);
            }

            return result;
        }

        private static string BuildHierarchyName(
            Transform current,
            Transform stopAt)
        {
            if (current == null)
                return "";

            string path =
                current.name ?? "";

            Transform parent =
                current.parent;

            int guard =
                0;

            while (parent != null &&
                   parent != stopAt &&
                   guard < 16)
            {
                path =
                    (parent.name ?? "") +
                    "/" +
                    path;

                parent =
                    parent.parent;

                guard++;
            }

            return path;
        }

        private static bool ContainsIgnoreCase(
            string value,
            string search)
        {
            if (string.IsNullOrEmpty(value) ||
                string.IsNullOrEmpty(search))
                return false;

            return value.IndexOf(
                search,
                StringComparison.OrdinalIgnoreCase
            ) >= 0;
        }

        private static bool IsAlive(
            UnityEngine.Object obj)
        {
            try
            {
                return obj != null;
            }
            catch
            {
                return false;
            }
        }

        private static void LogUsefulButtons(
            CreateGameOptions createScreen)
        {
            if (createScreen == null)
                return;

            try
            {
                PassiveButton[] buttons =
                    createScreen.GetComponentsInChildren<PassiveButton>(
                        true
                    );

                if (buttons == null)
                    return;

                for (int i = 0;
                     i < buttons.Length;
                     i++)
                {
                    PassiveButton button =
                        buttons[i];

                    if (button == null)
                        continue;

                }
            }
            catch
            {
            }
        }

    }

    [HarmonyPatch(
        typeof(CreateGameOptions),
        nameof(CreateGameOptions.Confirm)
    )]
    public static class BanModCreateGameConfirmPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            CreateGameOptions __instance)
        {
            if (BanMod.IsBanModDisabled)
                return true;

            if (!BanModServerSelection.IsVanilla)
                return true;

            if (BanModServerSelection.ConsumeVanillaCreateBypass())
            {

                return true;
            }

            if (BanModServerSelection.HasAcceptedVanillaThisWeek())
            {
                BanModServerSelection.VanillaAcknowledged =
                    true;

                DateTime? expiresAt =
                    BanModServerSelection
                        .GetVanillaAcceptanceExpiry();

                if (expiresAt.HasValue)
                {
                    string nextDate =
                        expiresAt.Value.ToString(
                            "MMMM d, yyyy",
                            CultureInfo.InvariantCulture
                        );

                    MainMenuInfo.Show(
                        $"Already accepted - Next request: {nextDate}"
                    );
                }
                else
                {
                    MainMenuInfo.Show(
                        "Already accepted."
                    );
                }


                return true;
            }

            if (ServerSelectionMenu.Instance == null)
            {

                return false;
            }


            ServerSelectionMenu.Instance
                .OpenVanillaCreateWarning(
                    __instance
                );

            return false;
        }

    }


    [HarmonyPatch(
        typeof(Constants),
        nameof(Constants.GetBroadcastVersion)
    )]
    public static class BanModBroadcastVersionPatch
    {
        public static void Postfix(ref int __result)
        {
            if (!BanModServerSelection.IsModded25)
                return;

            int original =
                __result;

            __result += 25;

        }
    }
    [HarmonyPatch(
    typeof(Constants),
    nameof(Constants.IsVersionModded)
)]
    public static class BanModIsVersionModdedPatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (!BanModServerSelection.IsModded25)
                return true;

            __result = true;

            return false;
        }
    }
}
