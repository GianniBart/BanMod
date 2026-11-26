// credits and licenses in the resources folder
using Assets.InnerNet;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Il2CppInterop.Runtime.Attributes;
using BanMod;

namespace BanMod
{

    internal static class AutoFriendInviteTexts
    {
        private static string currentLanguage = "en";

        private static readonly Dictionary<string, Dictionary<string, string>> Texts =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "en", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "auto_invite", "AUTO FRIEND INVITE" },
                        { "add_player", "ADD PLAYER" },
                        { "friends", "FRIENDS" },
                        { "non_friends", "NON-FRIENDS" },
                        { "player", "PLAYER" },
                        { "move_player_title", "MOVE {0}" },
                        { "no_lists", "NO LISTS" },
                        { "create_list_start", "Create a list to get started." },
                        { "player_count_one", "{0} player" },
                        { "player_count_many", "{0} players" },
                        { "create_list", "+  CREATE LIST" },
                        { "stop_auto_invite", "STOP AUTO INVITE" },
                        { "invite_this_list", "INVITE THIS LIST" },
                        { "empty_list", "EMPTY LIST" },
                        { "no_friends", "NO FRIENDS" },
                        { "empty_list_hint", "Use \"Add Player\" to add someone." },
                        { "no_friends_hint", "The game did not return any available friends." },
                        { "back", "BACK" },
                        { "rename", "RENAME" },
                        { "delete_list", "DELETE LIST" },
                        { "choose_section", "Choose which section to add the player from." },
                        { "no_player_available", "NO PLAYERS AVAILABLE" },
                        { "no_available_friends", "There are no friends in the lobby to add. Players already in a list are hidden." },
                        { "no_available_nonfriends", "There are no non-friends in the lobby to add. Players already in a list are hidden." },
                        { "player_unavailable", "PLAYER UNAVAILABLE" },
                        { "player_unavailable_hint", "Go back to the list and select the player again." },
                        { "move_to", "MOVE TO..." },
                        { "remove_from_list", "REMOVE FROM LIST" },
                        { "no_target_lists", "NO LIST AVAILABLE" },
                        { "no_target_lists_hint", "Create another list before moving this player." },
                        { "create_new_list", "CREATE NEW LIST" },
                        { "rename_current_list", "RENAME CURRENT LIST" },
                        { "type_list_name", "Type the list name with your physical keyboard" },
                        { "type_name", "<type name>" },
                        { "keyboard_help", "Enter = confirm   |   Backspace = delete   |   Esc = cancel" },
                        { "confirm", "CONFIRM" },
                        { "cancel", "CANCEL" },
                        { "all_friends", "ALL FRIENDS" },
                        { "ui_error", "Auto Friend Invite UI error.\nCheck the BepInEx log for [AutoFriendInviteUi]." },
                        { "button_auto_invite", "Auto Invite" }
                    }
                },
                {
                    "it", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "auto_invite", "INVITO AUTOMATICO AMICI" },
                        { "add_player", "AGGIUNGI PLAYER" },
                        { "friends", "AMICI" },
                        { "non_friends", "NON AMICI" },
                        { "player", "PLAYER" },
                        { "move_player_title", "SPOSTA {0}" },
                        { "no_lists", "NESSUNA LISTA" },
                        { "create_list_start", "Crea una lista per iniziare." },
                        { "player_count_one", "{0} player" },
                        { "player_count_many", "{0} player" },
                        { "create_list", "+  CREA LISTA" },
                        { "stop_auto_invite", "STOP AUTO INVITE" },
                        { "invite_this_list", "INVITA QUESTA LISTA" },
                        { "empty_list", "LISTA VUOTA" },
                        { "no_friends", "NESSUN AMICO" },
                        { "empty_list_hint", "Usa \"Aggiungi Player\" per inserire qualcuno." },
                        { "no_friends_hint", "Il gioco non ha restituito amici disponibili." },
                        { "back", "INDIETRO" },
                        { "rename", "RINOMINA" },
                        { "delete_list", "ELIMINA LISTA" },
                        { "choose_section", "Scegli da quale sezione aggiungere il player." },
                        { "no_player_available", "NESSUN PLAYER DISPONIBILE" },
                        { "no_available_friends", "Non ci sono amici in lobby da aggiungere. I player già presenti in una lista sono nascosti." },
                        { "no_available_nonfriends", "Non ci sono non-amici in lobby da aggiungere. I player già presenti in una lista sono nascosti." },
                        { "player_unavailable", "PLAYER NON DISPONIBILE" },
                        { "player_unavailable_hint", "Torna alla lista e seleziona di nuovo il player." },
                        { "move_to", "SPOSTA IN..." },
                        { "remove_from_list", "RIMUOVI DALLA LISTA" },
                        { "no_target_lists", "NESSUNA LISTA DISPONIBILE" },
                        { "no_target_lists_hint", "Crea un'altra lista prima di spostare questo player." },
                        { "create_new_list", "CREA NUOVA LISTA" },
                        { "rename_current_list", "RINOMINA LISTA ATTUALE" },
                        { "type_list_name", "Digita il nome della lista con la tastiera" },
                        { "type_name", "<digita nome>" },
                        { "keyboard_help", "Invio = conferma   |   Backspace = cancella   |   Esc = annulla" },
                        { "confirm", "CONFERMA" },
                        { "cancel", "ANNULLA" },
                        { "all_friends", "TUTTI GLI AMICI" },
                        { "ui_error", "Errore UI Auto Friend Invite.\nControlla il log BepInEx per [AutoFriendInviteUi]." },
                        { "button_auto_invite", "Auto Invite" }
                    }
                },
                {
                    "fr", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "auto_invite", "INVITATION AUTOMATIQUE" },
                        { "add_player", "AJOUTER UN JOUEUR" },
                        { "friends", "AMIS" },
                        { "non_friends", "NON-AMIS" },
                        { "player", "JOUEUR" },
                        { "move_player_title", "DÉPLACER {0}" },
                        { "no_lists", "AUCUNE LISTE" },
                        { "create_list_start", "Créez une liste pour commencer." },
                        { "player_count_one", "{0} joueur" },
                        { "player_count_many", "{0} joueurs" },
                        { "create_list", "+  CRÉER UNE LISTE" },
                        { "stop_auto_invite", "ARRÊTER L'INVITATION AUTO" },
                        { "invite_this_list", "INVITER CETTE LISTE" },
                        { "empty_list", "LISTE VIDE" },
                        { "no_friends", "AUCUN AMI" },
                        { "empty_list_hint", "Utilisez \"Ajouter un joueur\" pour ajouter quelqu'un." },
                        { "no_friends_hint", "Le jeu n'a renvoyé aucun ami disponible." },
                        { "back", "RETOUR" },
                        { "rename", "RENOMMER" },
                        { "delete_list", "SUPPRIMER LA LISTE" },
                        { "choose_section", "Choisissez la section depuis laquelle ajouter le joueur." },
                        { "no_player_available", "AUCUN JOUEUR DISPONIBLE" },
                        { "no_available_friends", "Aucun ami du lobby n'est disponible. Les joueurs déjà dans une liste sont masqués." },
                        { "no_available_nonfriends", "Aucun non-ami du lobby n'est disponible. Les joueurs déjà dans une liste sont masqués." },
                        { "player_unavailable", "JOUEUR INDISPONIBLE" },
                        { "player_unavailable_hint", "Revenez à la liste et sélectionnez à nouveau le joueur." },
                        { "move_to", "DÉPLACER VERS..." },
                        { "remove_from_list", "RETIRER DE LA LISTE" },
                        { "no_target_lists", "AUCUNE LISTE DISPONIBLE" },
                        { "no_target_lists_hint", "Créez une autre liste avant de déplacer ce joueur." },
                        { "create_new_list", "CRÉER UNE NOUVELLE LISTE" },
                        { "rename_current_list", "RENOMMER LA LISTE ACTUELLE" },
                        { "type_list_name", "Saisissez le nom de la liste avec votre clavier" },
                        { "type_name", "<saisir le nom>" },
                        { "keyboard_help", "Entrée = confirmer   |   Retour arrière = effacer   |   Échap = annuler" },
                        { "confirm", "CONFIRMER" },
                        { "cancel", "ANNULER" },
                        { "all_friends", "TOUS LES AMIS" },
                        { "ui_error", "Erreur de l'interface Auto Friend Invite.\nConsultez le journal BepInEx pour [AutoFriendInviteUi]." },
                        { "button_auto_invite", "Invitation auto" }
                    }
                },
                {
                    "de", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "auto_invite", "AUTO-EINLADUNG" },
                        { "add_player", "SPIELER HINZUFÜGEN" },
                        { "friends", "FREUNDE" },
                        { "non_friends", "NICHT-FREUNDE" },
                        { "player", "SPIELER" },
                        { "move_player_title", "{0} VERSCHIEBEN" },
                        { "no_lists", "KEINE LISTEN" },
                        { "create_list_start", "Erstelle eine Liste, um zu beginnen." },
                        { "player_count_one", "{0} Spieler" },
                        { "player_count_many", "{0} Spieler" },
                        { "create_list", "+  LISTE ERSTELLEN" },
                        { "stop_auto_invite", "AUTO-EINLADUNG STOPPEN" },
                        { "invite_this_list", "DIESE LISTE EINLADEN" },
                        { "empty_list", "LEERE LISTE" },
                        { "no_friends", "KEINE FREUNDE" },
                        { "empty_list_hint", "Verwende \"Spieler hinzufügen\", um jemanden hinzuzufügen." },
                        { "no_friends_hint", "Das Spiel hat keine verfügbaren Freunde zurückgegeben." },
                        { "back", "ZURÜCK" },
                        { "rename", "UMBENENNEN" },
                        { "delete_list", "LISTE LÖSCHEN" },
                        { "choose_section", "Wähle aus, aus welchem Bereich der Spieler hinzugefügt werden soll." },
                        { "no_player_available", "KEINE SPIELER VERFÜGBAR" },
                        { "no_available_friends", "Keine Freunde in der Lobby können hinzugefügt werden. Spieler in einer Liste werden ausgeblendet." },
                        { "no_available_nonfriends", "Keine Nicht-Freunde in der Lobby können hinzugefügt werden. Spieler in einer Liste werden ausgeblendet." },
                        { "player_unavailable", "SPIELER NICHT VERFÜGBAR" },
                        { "player_unavailable_hint", "Gehe zurück zur Liste und wähle den Spieler erneut aus." },
                        { "move_to", "VERSCHIEBEN NACH..." },
                        { "remove_from_list", "AUS LISTE ENTFERNEN" },
                        { "no_target_lists", "KEINE LISTE VERFÜGBAR" },
                        { "no_target_lists_hint", "Erstelle eine weitere Liste, bevor du diesen Spieler verschiebst." },
                        { "create_new_list", "NEUE LISTE ERSTELLEN" },
                        { "rename_current_list", "AKTUELLE LISTE UMBENENNEN" },
                        { "type_list_name", "Gib den Listennamen mit deiner Tastatur ein" },
                        { "type_name", "<Name eingeben>" },
                        { "keyboard_help", "Enter = bestätigen   |   Backspace = löschen   |   Esc = abbrechen" },
                        { "confirm", "BESTÄTIGEN" },
                        { "cancel", "ABBRECHEN" },
                        { "all_friends", "ALLE FREUNDE" },
                        { "ui_error", "Fehler in der Auto-Friend-Invite-Oberfläche.\nPrüfe das BepInEx-Log für [AutoFriendInviteUi]." },
                        { "button_auto_invite", "Auto-Einladung" }
                    }
                },
                {
                    "nl", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "auto_invite", "AUTO-UITNODIGING" },
                        { "add_player", "SPELER TOEVOEGEN" },
                        { "friends", "VRIENDEN" },
                        { "non_friends", "NIET-VRIENDEN" },
                        { "player", "SPELER" },
                        { "move_player_title", "{0} VERPLAATSEN" },
                        { "no_lists", "GEEN LIJSTEN" },
                        { "create_list_start", "Maak een lijst om te beginnen." },
                        { "player_count_one", "{0} speler" },
                        { "player_count_many", "{0} spelers" },
                        { "create_list", "+  LIJST MAKEN" },
                        { "stop_auto_invite", "AUTO-UITNODIGING STOPPEN" },
                        { "invite_this_list", "DEZE LIJST UITNODIGEN" },
                        { "empty_list", "LEGE LIJST" },
                        { "no_friends", "GEEN VRIENDEN" },
                        { "empty_list_hint", "Gebruik \"Speler toevoegen\" om iemand toe te voegen." },
                        { "no_friends_hint", "Het spel heeft geen beschikbare vrienden gevonden." },
                        { "back", "TERUG" },
                        { "rename", "HERNOEMEN" },
                        { "delete_list", "LIJST VERWIJDEREN" },
                        { "choose_section", "Kies uit welke sectie je de speler wilt toevoegen." },
                        { "no_player_available", "GEEN SPELERS BESCHIKBAAR" },
                        { "no_available_friends", "Er zijn geen vrienden in de lobby om toe te voegen. Spelers die al in een lijst staan zijn verborgen." },
                        { "no_available_nonfriends", "Er zijn geen niet-vrienden in de lobby om toe te voegen. Spelers die al in een lijst staan zijn verborgen." },
                        { "player_unavailable", "SPELER NIET BESCHIKBAAR" },
                        { "player_unavailable_hint", "Ga terug naar de lijst en selecteer de speler opnieuw." },
                        { "move_to", "VERPLAATSEN NAAR..." },
                        { "remove_from_list", "UIT LIJST VERWIJDEREN" },
                        { "no_target_lists", "GEEN LIJST BESCHIKBAAR" },
                        { "no_target_lists_hint", "Maak eerst een andere lijst voordat je deze speler verplaatst." },
                        { "create_new_list", "NIEUWE LIJST MAKEN" },
                        { "rename_current_list", "HUIDIGE LIJST HERNOEMEN" },
                        { "type_list_name", "Typ de lijstnaam met je toetsenbord" },
                        { "type_name", "<naam typen>" },
                        { "keyboard_help", "Enter = bevestigen   |   Backspace = wissen   |   Esc = annuleren" },
                        { "confirm", "BEVESTIGEN" },
                        { "cancel", "ANNULEREN" },
                        { "all_friends", "ALLE VRIENDEN" },
                        { "ui_error", "Fout in Auto Friend Invite UI.\nControleer het BepInEx-log voor [AutoFriendInviteUi]." },
                        { "button_auto_invite", "Auto-uitnodiging" }
                    }
                },
                {
                    "ru", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "auto_invite", "АВТОПРИГЛАШЕНИЕ" },
                        { "add_player", "ДОБАВИТЬ ИГРОКА" },
                        { "friends", "ДРУЗЬЯ" },
                        { "non_friends", "НЕ ДРУЗЬЯ" },
                        { "player", "ИГРОК" },
                        { "move_player_title", "ПЕРЕМЕСТИТЬ {0}" },
                        { "no_lists", "НЕТ СПИСКОВ" },
                        { "create_list_start", "Создайте список, чтобы начать." },
                        { "player_count_one", "{0} игрок" },
                        { "player_count_many", "{0} игроков" },
                        { "create_list", "+  СОЗДАТЬ СПИСОК" },
                        { "stop_auto_invite", "ОСТАНОВИТЬ АВТОПРИГЛАШЕНИЕ" },
                        { "invite_this_list", "ПРИГЛАСИТЬ ЭТОТ СПИСОК" },
                        { "empty_list", "ПУСТОЙ СПИСОК" },
                        { "no_friends", "НЕТ ДРУЗЕЙ" },
                        { "empty_list_hint", "Используйте \"Добавить игрока\", чтобы добавить кого-нибудь." },
                        { "no_friends_hint", "Игра не вернула доступных друзей." },
                        { "back", "НАЗАД" },
                        { "rename", "ПЕРЕИМЕНОВАТЬ" },
                        { "delete_list", "УДАЛИТЬ СПИСОК" },
                        { "choose_section", "Выберите раздел, из которого добавить игрока." },
                        { "no_player_available", "НЕТ ДОСТУПНЫХ ИГРОКОВ" },
                        { "no_available_friends", "В лобби нет друзей для добавления. Игроки, уже находящиеся в списке, скрыты." },
                        { "no_available_nonfriends", "В лобби нет других игроков для добавления. Игроки, уже находящиеся в списке, скрыты." },
                        { "player_unavailable", "ИГРОК НЕДОСТУПЕН" },
                        { "player_unavailable_hint", "Вернитесь к списку и снова выберите игрока." },
                        { "move_to", "ПЕРЕМЕСТИТЬ В..." },
                        { "remove_from_list", "УДАЛИТЬ ИЗ СПИСКА" },
                        { "no_target_lists", "НЕТ ДОСТУПНЫХ СПИСКОВ" },
                        { "no_target_lists_hint", "Создайте другой список перед перемещением этого игрока." },
                        { "create_new_list", "СОЗДАТЬ НОВЫЙ СПИСОК" },
                        { "rename_current_list", "ПЕРЕИМЕНОВАТЬ ТЕКУЩИЙ СПИСОК" },
                        { "type_list_name", "Введите название списка с клавиатуры" },
                        { "type_name", "<введите название>" },
                        { "keyboard_help", "Enter = подтвердить   |   Backspace = удалить   |   Esc = отмена" },
                        { "confirm", "ПОДТВЕРДИТЬ" },
                        { "cancel", "ОТМЕНА" },
                        { "all_friends", "ВСЕ ДРУЗЬЯ" },
                        { "ui_error", "Ошибка интерфейса Auto Friend Invite.\nПроверьте журнал BepInEx для [AutoFriendInviteUi]." },
                        { "button_auto_invite", "Автоприглашение" }
                    }
                },
                {
                    "ja", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "auto_invite", "自動招待" },
                        { "add_player", "プレイヤーを追加" },
                        { "friends", "フレンド" },
                        { "non_friends", "フレンド以外" },
                        { "player", "プレイヤー" },
                        { "move_player_title", "{0} を移動" },
                        { "no_lists", "リストがありません" },
                        { "create_list_start", "まずリストを作成してください。" },
                        { "player_count_one", "{0} 人" },
                        { "player_count_many", "{0} 人" },
                        { "create_list", "+  リストを作成" },
                        { "stop_auto_invite", "自動招待を停止" },
                        { "invite_this_list", "このリストを招待" },
                        { "empty_list", "空のリスト" },
                        { "no_friends", "フレンドがいません" },
                        { "empty_list_hint", "「プレイヤーを追加」から追加してください。" },
                        { "no_friends_hint", "利用可能なフレンドが見つかりませんでした。" },
                        { "back", "戻る" },
                        { "rename", "名前を変更" },
                        { "delete_list", "リストを削除" },
                        { "choose_section", "プレイヤーを追加する区分を選択してください。" },
                        { "no_player_available", "追加できるプレイヤーがいません" },
                        { "no_available_friends", "追加できるフレンドがロビーにいません。すでにリストにいるプレイヤーは非表示です。" },
                        { "no_available_nonfriends", "追加できるフレンド以外のプレイヤーがロビーにいません。すでにリストにいるプレイヤーは非表示です。" },
                        { "player_unavailable", "プレイヤーを利用できません" },
                        { "player_unavailable_hint", "リストに戻ってプレイヤーを選び直してください。" },
                        { "move_to", "移動先..." },
                        { "remove_from_list", "リストから削除" },
                        { "no_target_lists", "移動先のリストがありません" },
                        { "no_target_lists_hint", "このプレイヤーを移動する前に別のリストを作成してください。" },
                        { "create_new_list", "新しいリストを作成" },
                        { "rename_current_list", "現在のリスト名を変更" },
                        { "type_list_name", "キーボードでリスト名を入力してください" },
                        { "type_name", "<名前を入力>" },
                        { "keyboard_help", "Enter = 決定   |   Backspace = 削除   |   Esc = キャンセル" },
                        { "confirm", "決定" },
                        { "cancel", "キャンセル" },
                        { "all_friends", "すべてのフレンド" },
                        { "ui_error", "Auto Friend Invite UI エラー。\nBepInEx ログの [AutoFriendInviteUi] を確認してください。" },
                        { "button_auto_invite", "自動招待" }
                    }
                },
                {
                    "ko", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "auto_invite", "자동 초대" },
                        { "add_player", "플레이어 추가" },
                        { "friends", "친구" },
                        { "non_friends", "친구 아님" },
                        { "player", "플레이어" },
                        { "move_player_title", "{0} 이동" },
                        { "no_lists", "목록 없음" },
                        { "create_list_start", "시작하려면 목록을 만드세요." },
                        { "player_count_one", "{0}명" },
                        { "player_count_many", "{0}명" },
                        { "create_list", "+  목록 만들기" },
                        { "stop_auto_invite", "자동 초대 중지" },
                        { "invite_this_list", "이 목록 초대" },
                        { "empty_list", "빈 목록" },
                        { "no_friends", "친구 없음" },
                        { "empty_list_hint", "\"플레이어 추가\"를 사용해 플레이어를 추가하세요." },
                        { "no_friends_hint", "게임에서 사용 가능한 친구를 찾지 못했습니다." },
                        { "back", "뒤로" },
                        { "rename", "이름 변경" },
                        { "delete_list", "목록 삭제" },
                        { "choose_section", "플레이어를 추가할 구역을 선택하세요." },
                        { "no_player_available", "사용 가능한 플레이어 없음" },
                        { "no_available_friends", "추가할 수 있는 친구가 로비에 없습니다. 이미 목록에 있는 플레이어는 숨겨집니다." },
                        { "no_available_nonfriends", "추가할 수 있는 친구가 아닌 플레이어가 로비에 없습니다. 이미 목록에 있는 플레이어는 숨겨집니다." },
                        { "player_unavailable", "플레이어를 사용할 수 없음" },
                        { "player_unavailable_hint", "목록으로 돌아가 플레이어를 다시 선택하세요." },
                        { "move_to", "이동..." },
                        { "remove_from_list", "목록에서 제거" },
                        { "no_target_lists", "사용 가능한 목록 없음" },
                        { "no_target_lists_hint", "이 플레이어를 이동하기 전에 다른 목록을 만드세요." },
                        { "create_new_list", "새 목록 만들기" },
                        { "rename_current_list", "현재 목록 이름 변경" },
                        { "type_list_name", "키보드로 목록 이름을 입력하세요" },
                        { "type_name", "<이름 입력>" },
                        { "keyboard_help", "Enter = 확인   |   Backspace = 삭제   |   Esc = 취소" },
                        { "confirm", "확인" },
                        { "cancel", "취소" },
                        { "all_friends", "모든 친구" },
                        { "ui_error", "Auto Friend Invite UI 오류.\nBepInEx 로그에서 [AutoFriendInviteUi]를 확인하세요." },
                        { "button_auto_invite", "자동 초대" }
                    }
                },
                {
                    "es", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "auto_invite", "INVITACIÓN AUTOMÁTICA" },
                        { "add_player", "AÑADIR JUGADOR" },
                        { "friends", "AMIGOS" },
                        { "non_friends", "NO AMIGOS" },
                        { "player", "JUGADOR" },
                        { "move_player_title", "MOVER {0}" },
                        { "no_lists", "NO HAY LISTAS" },
                        { "create_list_start", "Crea una lista para empezar." },
                        { "player_count_one", "{0} jugador" },
                        { "player_count_many", "{0} jugadores" },
                        { "create_list", "+  CREAR LISTA" },
                        { "stop_auto_invite", "DETENER INVITACIÓN AUTO" },
                        { "invite_this_list", "INVITAR ESTA LISTA" },
                        { "empty_list", "LISTA VACÍA" },
                        { "no_friends", "NO HAY AMIGOS" },
                        { "empty_list_hint", "Usa \"Añadir jugador\" para añadir a alguien." },
                        { "no_friends_hint", "El juego no devolvió amigos disponibles." },
                        { "back", "ATRÁS" },
                        { "rename", "RENOMBRAR" },
                        { "delete_list", "ELIMINAR LISTA" },
                        { "choose_section", "Elige desde qué sección añadir al jugador." },
                        { "no_player_available", "NO HAY JUGADORES DISPONIBLES" },
                        { "no_available_friends", "No hay amigos en la sala para añadir. Los jugadores que ya están en una lista se ocultan." },
                        { "no_available_nonfriends", "No hay jugadores que no sean amigos para añadir. Los que ya están en una lista se ocultan." },
                        { "player_unavailable", "JUGADOR NO DISPONIBLE" },
                        { "player_unavailable_hint", "Vuelve a la lista y selecciona de nuevo al jugador." },
                        { "move_to", "MOVER A..." },
                        { "remove_from_list", "QUITAR DE LA LISTA" },
                        { "no_target_lists", "NO HAY LISTAS DISPONIBLES" },
                        { "no_target_lists_hint", "Crea otra lista antes de mover a este jugador." },
                        { "create_new_list", "CREAR NUEVA LISTA" },
                        { "rename_current_list", "RENOMBRAR LISTA ACTUAL" },
                        { "type_list_name", "Escribe el nombre de la lista con el teclado" },
                        { "type_name", "<escribe el nombre>" },
                        { "keyboard_help", "Enter = confirmar   |   Backspace = borrar   |   Esc = cancelar" },
                        { "confirm", "CONFIRMAR" },
                        { "cancel", "CANCELAR" },
                        { "all_friends", "TODOS LOS AMIGOS" },
                        { "ui_error", "Error de la interfaz Auto Friend Invite.\nRevisa el registro de BepInEx para [AutoFriendInviteUi]." },
                        { "button_auto_invite", "Invitación auto" }
                    }
                },
                {
                    "pt", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "auto_invite", "CONVITE AUTOMÁTICO" },
                        { "add_player", "ADICIONAR JOGADOR" },
                        { "friends", "AMIGOS" },
                        { "non_friends", "NÃO AMIGOS" },
                        { "player", "JOGADOR" },
                        { "move_player_title", "MOVER {0}" },
                        { "no_lists", "SEM LISTAS" },
                        { "create_list_start", "Crie uma lista para começar." },
                        { "player_count_one", "{0} jogador" },
                        { "player_count_many", "{0} jogadores" },
                        { "create_list", "+  CRIAR LISTA" },
                        { "stop_auto_invite", "PARAR CONVITE AUTOMÁTICO" },
                        { "invite_this_list", "CONVIDAR ESTA LISTA" },
                        { "empty_list", "LISTA VAZIA" },
                        { "no_friends", "SEM AMIGOS" },
                        { "empty_list_hint", "Use \"Adicionar jogador\" para adicionar alguém." },
                        { "no_friends_hint", "O jogo não retornou amigos disponíveis." },
                        { "back", "VOLTAR" },
                        { "rename", "RENOMEAR" },
                        { "delete_list", "EXCLUIR LISTA" },
                        { "choose_section", "Escolha de qual seção adicionar o jogador." },
                        { "no_player_available", "NENHUM JOGADOR DISPONÍVEL" },
                        { "no_available_friends", "Não há amigos no lobby para adicionar. Jogadores que já estão em uma lista ficam ocultos." },
                        { "no_available_nonfriends", "Não há não-amigos no lobby para adicionar. Jogadores que já estão em uma lista ficam ocultos." },
                        { "player_unavailable", "JOGADOR INDISPONÍVEL" },
                        { "player_unavailable_hint", "Volte à lista e selecione o jogador novamente." },
                        { "move_to", "MOVER PARA..." },
                        { "remove_from_list", "REMOVER DA LISTA" },
                        { "no_target_lists", "NENHUMA LISTA DISPONÍVEL" },
                        { "no_target_lists_hint", "Crie outra lista antes de mover este jogador." },
                        { "create_new_list", "CRIAR NOVA LISTA" },
                        { "rename_current_list", "RENOMEAR LISTA ATUAL" },
                        { "type_list_name", "Digite o nome da lista com o teclado" },
                        { "type_name", "<digite o nome>" },
                        { "keyboard_help", "Enter = confirmar   |   Backspace = apagar   |   Esc = cancelar" },
                        { "confirm", "CONFIRMAR" },
                        { "cancel", "CANCELAR" },
                        { "all_friends", "TODOS OS AMIGOS" },
                        { "ui_error", "Erro na interface Auto Friend Invite.\nVerifique o log do BepInEx para [AutoFriendInviteUi]." },
                        { "button_auto_invite", "Convite auto" }
                    }
                },
                {
                    "zh-cn", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "auto_invite", "自动邀请" },
                        { "add_player", "添加玩家" },
                        { "friends", "好友" },
                        { "non_friends", "非好友" },
                        { "player", "玩家" },
                        { "move_player_title", "移动 {0}" },
                        { "no_lists", "没有列表" },
                        { "create_list_start", "创建一个列表以开始。" },
                        { "player_count_one", "{0} 名玩家" },
                        { "player_count_many", "{0} 名玩家" },
                        { "create_list", "+  创建列表" },
                        { "stop_auto_invite", "停止自动邀请" },
                        { "invite_this_list", "邀请此列表" },
                        { "empty_list", "空列表" },
                        { "no_friends", "没有好友" },
                        { "empty_list_hint", "使用“添加玩家”来添加玩家。" },
                        { "no_friends_hint", "游戏没有返回可用好友。" },
                        { "back", "返回" },
                        { "rename", "重命名" },
                        { "delete_list", "删除列表" },
                        { "choose_section", "选择要从哪个分类添加玩家。" },
                        { "no_player_available", "没有可用玩家" },
                        { "no_available_friends", "大厅中没有可添加的好友。已经在列表中的玩家会被隐藏。" },
                        { "no_available_nonfriends", "大厅中没有可添加的非好友玩家。已经在列表中的玩家会被隐藏。" },
                        { "player_unavailable", "玩家不可用" },
                        { "player_unavailable_hint", "返回列表并重新选择该玩家。" },
                        { "move_to", "移动到..." },
                        { "remove_from_list", "从列表中移除" },
                        { "no_target_lists", "没有可用列表" },
                        { "no_target_lists_hint", "移动此玩家前请先创建另一个列表。" },
                        { "create_new_list", "创建新列表" },
                        { "rename_current_list", "重命名当前列表" },
                        { "type_list_name", "使用键盘输入列表名称" },
                        { "type_name", "<输入名称>" },
                        { "keyboard_help", "Enter = 确认   |   Backspace = 删除   |   Esc = 取消" },
                        { "confirm", "确认" },
                        { "cancel", "取消" },
                        { "all_friends", "所有好友" },
                        { "ui_error", "Auto Friend Invite 界面错误。\n请检查 BepInEx 日志中的 [AutoFriendInviteUi]。" },
                        { "button_auto_invite", "自动邀请" }
                    }
                },
                {
                    "zh-tw", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "auto_invite", "自動邀請" },
                        { "add_player", "新增玩家" },
                        { "friends", "好友" },
                        { "non_friends", "非好友" },
                        { "player", "玩家" },
                        { "move_player_title", "移動 {0}" },
                        { "no_lists", "沒有列表" },
                        { "create_list_start", "建立一個列表以開始。" },
                        { "player_count_one", "{0} 名玩家" },
                        { "player_count_many", "{0} 名玩家" },
                        { "create_list", "+  建立列表" },
                        { "stop_auto_invite", "停止自動邀請" },
                        { "invite_this_list", "邀請此列表" },
                        { "empty_list", "空列表" },
                        { "no_friends", "沒有好友" },
                        { "empty_list_hint", "使用「新增玩家」來加入玩家。" },
                        { "no_friends_hint", "遊戲沒有回傳可用好友。" },
                        { "back", "返回" },
                        { "rename", "重新命名" },
                        { "delete_list", "刪除列表" },
                        { "choose_section", "選擇要從哪個分類新增玩家。" },
                        { "no_player_available", "沒有可用玩家" },
                        { "no_available_friends", "大廳中沒有可新增的好友。已在列表中的玩家會被隱藏。" },
                        { "no_available_nonfriends", "大廳中沒有可新增的非好友玩家。已在列表中的玩家會被隱藏。" },
                        { "player_unavailable", "玩家不可用" },
                        { "player_unavailable_hint", "返回列表並重新選擇該玩家。" },
                        { "move_to", "移動到..." },
                        { "remove_from_list", "從列表移除" },
                        { "no_target_lists", "沒有可用列表" },
                        { "no_target_lists_hint", "移動此玩家前請先建立另一個列表。" },
                        { "create_new_list", "建立新列表" },
                        { "rename_current_list", "重新命名目前列表" },
                        { "type_list_name", "使用鍵盤輸入列表名稱" },
                        { "type_name", "<輸入名稱>" },
                        { "keyboard_help", "Enter = 確認   |   Backspace = 刪除   |   Esc = 取消" },
                        { "confirm", "確認" },
                        { "cancel", "取消" },
                        { "all_friends", "所有好友" },
                        { "ui_error", "Auto Friend Invite 介面錯誤。\n請檢查 BepInEx 日誌中的 [AutoFriendInviteUi]。" },
                        { "button_auto_invite", "自動邀請" }
                    }
                }
            };

        public static void RefreshLanguage()
        {
            string detected = DetectGameLanguage();

            if (!string.IsNullOrWhiteSpace(detected))
                currentLanguage = detected;
        }

        public static string Get(string key)
        {
            Dictionary<string, string> lang;

            if (!Texts.TryGetValue(currentLanguage, out lang))
                lang = Texts["en"];

            string value;

            if (lang.TryGetValue(key, out value))
                return value;

            if (Texts["en"].TryGetValue(key, out value))
                return value;

            return key;
        }

        public static string Format(string key, params object[] args)
        {
            try
            {
                return string.Format(Get(key), args);
            }
            catch
            {
                return Get(key);
            }
        }

        private static string DetectGameLanguage()
        {
            try
            {
                TranslationController controller = TranslationController.Instance;

                if (controller != null)
                {
                    string detected = DetectFromObject(controller, 0);

                    if (!string.IsNullOrEmpty(detected))
                        return detected;
                }
            }
            catch
            {
            }

            try
            {
                string systemToken = Application.systemLanguage.ToString();
                string mapped = MapLanguageToken(systemToken);

                if (!string.IsNullOrEmpty(mapped))
                    return mapped;
            }
            catch
            {
            }

            return "en";
        }

        private static string DetectFromObject(object source, int depth)
        {
            if (source == null || depth > 1)
                return "";

            string direct = MapLanguageToken(source.ToString());

            if (!string.IsNullOrEmpty(direct))
                return direct;

            Type type;

            try
            {
                type = source.GetType();
            }
            catch
            {
                return "";
            }

            string[] preferred =
            {
                "currentLanguage", "CurrentLanguage",
                "language", "Language",
                "currentLanguageUnit", "CurrentLanguageUnit",
                "currentLanguageData", "CurrentLanguageData",
                "languageName", "LanguageName",
                "languageId", "LanguageId",
                "languageID", "LanguageID",
                "code", "Code",
                "name", "Name"
            };

            BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            for (int i = 0; i < preferred.Length; i++)
            {
                object value = ReadMember(type, source, preferred[i], flags);

                if (value == null)
                    continue;

                string mapped = MapLanguageToken(value.ToString());

                if (!string.IsNullOrEmpty(mapped))
                    return mapped;

                mapped = DetectFromObject(value, depth + 1);

                if (!string.IsNullOrEmpty(mapped))
                    return mapped;
            }

            try
            {
                PropertyInfo[] properties = type.GetProperties(flags);

                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo p = properties[i];

                    if (p == null ||
                        p.Name == null ||
                        p.Name.IndexOf("language", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    object value = null;

                    try
                    {
                        if (p.GetIndexParameters().Length == 0)
                            value = p.GetValue(source, null);
                    }
                    catch
                    {
                    }

                    if (value == null)
                        continue;

                    string mapped = MapLanguageToken(value.ToString());

                    if (!string.IsNullOrEmpty(mapped))
                        return mapped;
                }
            }
            catch
            {
            }

            try
            {
                FieldInfo[] fields = type.GetFields(flags);

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo f = fields[i];

                    if (f == null ||
                        f.Name == null ||
                        f.Name.IndexOf("language", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    object value = null;

                    try
                    {
                        value = f.GetValue(source);
                    }
                    catch
                    {
                    }

                    if (value == null)
                        continue;

                    string mapped = MapLanguageToken(value.ToString());

                    if (!string.IsNullOrEmpty(mapped))
                        return mapped;
                }
            }
            catch
            {
            }

            return "";
        }

        private static object ReadMember(
            Type type,
            object source,
            string name,
            BindingFlags flags)
        {
            try
            {
                PropertyInfo property = type.GetProperty(name, flags);

                if (property != null && property.GetIndexParameters().Length == 0)
                    return property.GetValue(source, null);
            }
            catch
            {
            }

            try
            {
                FieldInfo field = type.GetField(name, flags);

                if (field != null)
                    return field.GetValue(source);
            }
            catch
            {
            }

            return null;
        }

        private static string MapLanguageToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return "";

            string t = token.Trim().ToLowerInvariant();

            if (t.Contains("schinese") ||
                t.Contains("simplified") ||
                t.Contains("chinesesimplified") ||
                t.Contains("zh-cn") ||
                t.Contains("zh_hans") ||
                t.Contains("简体"))
            {
                return "zh-cn";
            }

            if (t.Contains("tchinese") ||
                t.Contains("traditional") ||
                t.Contains("chinesetraditional") ||
                t.Contains("zh-tw") ||
                t.Contains("zh_hant") ||
                t.Contains("繁體") ||
                t.Contains("繁体"))
            {
                return "zh-tw";
            }

            if (t == "chinese" || t.Contains("中文"))
                return "zh-cn";

            if (t.Contains("italian") || t.Contains("italiano"))
                return "it";

            if (t.Contains("french") || t.Contains("français") || t.Contains("francais"))
                return "fr";

            if (t.Contains("german") || t.Contains("deutsch"))
                return "de";

            if (t.Contains("dutch") || t.Contains("nederlands"))
                return "nl";

            if (t.Contains("russian") || t.Contains("рус"))
                return "ru";

            if (t.Contains("japanese") || t.Contains("日本語"))
                return "ja";

            if (t.Contains("korean") || t.Contains("한국"))
                return "ko";

            if (t.Contains("spanish") ||
                t.Contains("español") ||
                t.Contains("espanol") ||
                t.Contains("latam") ||
                t.Contains("latinamerica") ||
                t.Contains("latin america"))
            {
                return "es";
            }

            if (t.Contains("portuguese") ||
                t.Contains("português") ||
                t.Contains("portugues") ||
                t.Contains("brazil") ||
                t.Contains("brasil"))
            {
                return "pt";
            }

            if (t.Contains("english"))
                return "en";

            return "";
        }
    }

    public class AutoFriendInviteUi : MonoBehaviour
    {
        public static AutoFriendInviteUi Instance;

        public bool showMenu = false;

        private Rect windowRect;
        private Vector2 scrollPosition = Vector2.zero;

        private GUIStyle windowStyle;
        private GUIStyle titleStyle;
        private GUIStyle headerStyle;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;
        private GUIStyle sectionStyle;
        private GUIStyle mutedLabelStyle;
        private GUIStyle centerLabelStyle;

        private Texture2D blackTexture;
        private Texture2D roundedWindowTexture;
        private Texture2D roundedPanelTexture;
        private Texture2D roundedButtonTexture;

        private enum UiPage
        {
            Lists,
            ListDetails,
            AddChoice,
            AddFriends,
            PlayerActions,
            MovePlayer
        }

        private UiPage uiPage = UiPage.Lists;
        private string selectedPlayerPuid = "";
        private string selectedPlayerName = "";

        private int lastScreenWidth;
        private int lastScreenHeight;
        private bool lastLobbyState;
        private string renameListName = "";
        private string renameSourceList = "";

        private int nameDialogMode = 0;
        private string nameDialogBuffer = "";
        private const int MaxListNameLength = 32;

        private const float WindowWidth = 760f;
        private const float WindowHeight = 620f;
        private const float Padding = 12f;
        private const float RowHeight = 38f;
        private const float Gap = 8f;

        private void Awake()
        {
            Instance = this;
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            lastLobbyState = IsLobbySafe();
            CenterWindow();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            DestroyResources();
        }

        private void Update()
        {
            bool lobbyNow = IsLobbySafe();

            if (lobbyNow != lastLobbyState)
            {
                lastLobbyState = lobbyNow;

                if (!lobbyNow)
                    showMenu = false;
            }

            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                CenterWindow();
            }

        }

        private void AppendNameText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];

                if (nameDialogBuffer.Length >= MaxListNameLength)
                    break;

                if (char.IsLetterOrDigit(ch) ||
                    ch == ' ' ||
                    ch == '_' ||
                    ch == '-' ||
                    ch == '.' ||
                    ch == '(' ||
                    ch == ')')
                {
                    nameDialogBuffer += ch;
                }
            }
        }

        private void NameDialogBackspace()
        {
            if (!string.IsNullOrEmpty(nameDialogBuffer))
                nameDialogBuffer = nameDialogBuffer.Substring(0, nameDialogBuffer.Length - 1);
        }

        private void HandleNameDialogEvent()
        {
            try
            {
                Event ev = Event.current;

                if (ev == null || ev.type != EventType.KeyDown)
                    return;

                if (ev.keyCode == KeyCode.Escape)
                {
                    CloseNameDialog();
                    ev.Use();
                    return;
                }

                if (ev.keyCode == KeyCode.Return || ev.keyCode == KeyCode.KeypadEnter)
                {
                    SubmitNameDialog();
                    ev.Use();
                    return;
                }

                if (ev.keyCode == KeyCode.Backspace)
                {
                    NameDialogBackspace();
                    ev.Use();
                    return;
                }

                if (ev.character != '\0' &&
                    ev.character != '\n' &&
                    ev.character != '\r' &&
                    ev.character != '\b')
                {
                    AppendNameText(ev.character.ToString());
                    ev.Use();
                }
            }
            catch
            {
            }
        }

        private void OpenCreateDialog()
        {
            nameDialogMode = 1;
            nameDialogBuffer = "";
            Debug.Log("[AutoFriendInviteUi] Create List dialog opened.");
        }

        private void OpenRenameDialog()
        {
            string current = GetCurrentListSafe();

            if (IsAllFriendsName(current))
                return;

            nameDialogMode = 2;
            nameDialogBuffer = current;
            Debug.Log("[AutoFriendInviteUi] Rename List dialog opened for: " + current);
        }

        private void CloseNameDialog()
        {
            nameDialogMode = 0;
            nameDialogBuffer = "";
        }

        private void SubmitNameDialog()
        {
            string value = (nameDialogBuffer ?? "").Trim();

            if (string.IsNullOrWhiteSpace(value))
                return;

            try
            {
                if (nameDialogMode == 1)
                {
                    if (AutoFriendInviteManager.CreateList(value, true))
                    {
                        AutoFriendInviteManager.SetCurrentList(value);
                        SyncRenameField();
                        CloseNameDialog();
                        OpenPage(UiPage.ListDetails);
                    }
                }
                else if (nameDialogMode == 2)
                {
                    string current = GetCurrentListSafe();

                    if (AutoFriendInviteManager.RenameList(current, value, true))
                    {
                        AutoFriendInviteManager.SetCurrentList(value);
                        SyncRenameField();
                        CloseNameDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[AutoFriendInviteUi] SubmitNameDialog failed: " + ex);
            }
        }

        private static AutoFriendInviteUi EnsureInstance()
        {
            if (Instance == null)
            {
                GameObject obj = new GameObject("AutoFriendInviteUi");
                DontDestroyOnLoad(obj);
                Instance = obj.AddComponent<AutoFriendInviteUi>();
            }

            return Instance;
        }

        public static void OpenStatic()
        {
            EnsureInstance().OpenMenu();
        }

        public static void ShowMenu()
        {
            OpenStatic();
        }

        public static void ToggleStatic()
        {
            EnsureInstance().ToggleMenu();
        }

        public void ToggleMenu()
        {
            if (showMenu)
                CloseMenu();
            else
                OpenMenu();
        }

        public void OpenMenu()
        {
            AutoFriendInviteTexts.RefreshLanguage();

            try
            {
                AutoFriendInviteManager.Load();
            }
            catch (Exception ex)
            {
                Debug.LogError("[AutoFriendInviteUi] Load failed: " + ex);
            }

            SyncRenameField();
            uiPage = UiPage.Lists;
            selectedPlayerPuid = "";
            selectedPlayerName = "";
            scrollPosition = Vector2.zero;
            CenterWindow();
            showMenu = true;
        }

        public void CloseMenu()
        {
            showMenu = false;
        }

        private bool IsLobbySafe()
        {
            try
            {
                return AmongUsClient.Instance != null && GameStates.isLobby;
            }
            catch
            {
                return false;
            }
        }

        private void CenterWindow()
        {
            float width = Mathf.Min(WindowWidth, Mathf.Max(500f, Screen.width - 30f));
            float height = Mathf.Min(WindowHeight, Mathf.Max(420f, Screen.height - 30f));

            windowRect = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height
            );
        }

        private void DestroyResources()
        {
            DestroyTexture(ref blackTexture);
            DestroyTexture(ref roundedWindowTexture);
            DestroyTexture(ref roundedPanelTexture);
            DestroyTexture(ref roundedButtonTexture);

            windowStyle = null;
            titleStyle = null;
            headerStyle = null;
            labelStyle = null;
            buttonStyle = null;
            sectionStyle = null;
            mutedLabelStyle = null;
            centerLabelStyle = null;
        }

        [HideFromIl2Cpp]
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

                blackTexture = CreateRoundedTexture(
                    32,
                    8,
                    new Color(0.025f, 0.03f, 0.04f, 0.98f)
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
                titleStyle.fontSize = 22;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.alignment = TextAnchor.MiddleLeft;
                titleStyle.normal.textColor = Color.white;
            }

            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.fontSize = 16;
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.alignment = TextAnchor.MiddleLeft;
                headerStyle.normal.textColor = new Color(0.93f, 0.95f, 1f, 1f);
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = 13;
                labelStyle.alignment = TextAnchor.MiddleLeft;
                labelStyle.wordWrap = true;
                labelStyle.normal.textColor = new Color(0.90f, 0.92f, 0.96f, 1f);
            }

            if (mutedLabelStyle == null)
            {
                mutedLabelStyle = new GUIStyle(labelStyle);
                mutedLabelStyle.fontSize = 12;
                mutedLabelStyle.normal.textColor = new Color(0.62f, 0.66f, 0.74f, 1f);
            }

            if (centerLabelStyle == null)
            {
                centerLabelStyle = new GUIStyle(labelStyle);
                centerLabelStyle.alignment = TextAnchor.MiddleCenter;
                centerLabelStyle.fontSize = 14;
            }

            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.fontSize = 13;
                buttonStyle.fontStyle = FontStyle.Bold;
                buttonStyle.alignment = TextAnchor.MiddleCenter;
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

        [HideFromIl2Cpp]
        private Texture2D CreateRoundedTexture(int size, int radius, Color color)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
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
                        alphaFactor = Mathf.Clamp01(r + 0.5f - distance);

                    Color32 pixel = fill;
                    pixel.a = (byte)Mathf.RoundToInt(fill.a * alphaFactor);
                    pixels[y * size + x] = pixel;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }


        private void OnGUI()
        {
            if (!showMenu)
                return;

            if (!IsLobbySafe())
            {
                CloseMenu();
                return;
            }

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

                windowRect = GUI.Window(
                    92871,
                    windowRect,
                    (GUI.WindowFunction)DrawWindowSafe,
                    "",
                    windowStyle
                );
            }
            catch (Exception ex)
            {
                Debug.LogError("[AutoFriendInviteUi] OnGUI failed: " + ex);
            }
            finally
            {
                GUI.color = oldColor;
                GUI.backgroundColor = oldBackground;
                GUI.contentColor = oldContent;
                GUI.depth = oldDepth;
            }
        }

        private void DrawWindowSafe(int id)
        {
            try
            {
                DrawWindow(id);
            }
            catch (Exception ex)
            {
                Debug.LogError("[AutoFriendInviteUi] DrawWindow failed: " + ex);

                GUI.Label(
                    new Rect(16f, 55f, windowRect.width - 32f, 70f),
                    T("ui_error"),
                    labelStyle
                );

                if (GUI.Button(
                    new Rect(windowRect.width - 52f, 10f, 36f, 32f),
                    "X",
                    buttonStyle))
                {
                    CloseMenu();
                }
            }
        }

        private void DrawWindow(int id)
        {
            float width = windowRect.width;
            float height = windowRect.height;

            string pageTitle = GetPageTitle();

            GUI.Label(
                new Rect(20f, 10f, width - 92f, 34f),
                pageTitle,
                titleStyle
            );

            Color oldBg = GUI.backgroundColor;

            GUI.backgroundColor = new Color(0.72f, 0.16f, 0.18f, 1f);

            if (GUI.Button(
                new Rect(width - 54f, 10f, 36f, 32f),
                "X",
                buttonStyle))
            {
                CloseMenu();
            }

            GUI.backgroundColor = oldBg;

            Rect body = new Rect(
                18f,
                56f,
                width - 36f,
                height - 74f
            );

            switch (uiPage)
            {
                case UiPage.Lists:
                    DrawListsPage(body);
                    break;

                case UiPage.ListDetails:
                    DrawListDetailsPage(body);
                    break;

                case UiPage.AddChoice:
                    DrawAddChoicePage(body);
                    break;

                case UiPage.AddFriends:
                    DrawAvailablePlayersPage(body, true);
                    break;

                case UiPage.PlayerActions:
                    DrawPlayerActionsPage(body);
                    break;

                case UiPage.MovePlayer:
                    DrawMovePlayerPage(body);
                    break;
            }

            if (nameDialogMode != 0)
                DrawNameDialog(width, height);

            GUI.DragWindow(new Rect(0f, 0f, width - 62f, 48f));
        }

        [HideFromIl2Cpp]
        private string T(string key)
        {
            return AutoFriendInviteTexts.Get(key);
        }

        [HideFromIl2Cpp]
        private string GetPageTitle()
        {
            switch (uiPage)
            {
                case UiPage.Lists:
                    return T("auto_invite");

                case UiPage.ListDetails:
                    {
                        string current = GetCurrentListSafe();
                        return IsAllFriendsName(current)
                            ? T("all_friends")
                            : current;
                    }

                case UiPage.AddChoice:
                    return T("add_player");

                case UiPage.AddFriends:
                    return T("friends");

                case UiPage.PlayerActions:
                    return string.IsNullOrWhiteSpace(selectedPlayerName)
                        ? T("player")
                        : selectedPlayerName;

                case UiPage.MovePlayer:
                    return AutoFriendInviteTexts.Format(
                        "move_player_title",
                        string.IsNullOrWhiteSpace(selectedPlayerName)
                            ? T("player")
                            : selectedPlayerName
                    );

                default:
                    return T("auto_invite");
            }
        }

        [HideFromIl2Cpp]
        private void OpenPage(UiPage page)
        {
            uiPage = page;
            scrollPosition = Vector2.zero;
        }

        [HideFromIl2Cpp]
        private bool DrawTintedButton(Rect rect, string text, Color color)
        {
            Color oldBg = GUI.backgroundColor;
            GUI.backgroundColor = color;

            bool clicked = GUI.Button(rect, text, buttonStyle);

            GUI.backgroundColor = oldBg;
            return clicked;
        }

        [HideFromIl2Cpp]
        private void DrawPanel(Rect rect)
        {
            Color oldBg = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;
            GUI.Box(rect, GUIContent.none, sectionStyle);
            GUI.backgroundColor = oldBg;
        }

        [HideFromIl2Cpp]
        private void DrawEmptyState(Rect rect, string title, string subtitle)
        {
            DrawPanel(rect);

            GUIStyle titleTextStyle = new GUIStyle(headerStyle);
            titleTextStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Label(
                new Rect(rect.x + 16f, rect.y + 18f, rect.width - 32f, 28f),
                title,
                titleTextStyle
            );

            GUIStyle sub = new GUIStyle(mutedLabelStyle);
            sub.alignment = TextAnchor.UpperCenter;

            GUI.Label(
                new Rect(rect.x + 24f, rect.y + 50f, rect.width - 48f, rect.height - 64f),
                subtitle,
                sub
            );
        }

        [HideFromIl2Cpp]
        private void DrawListsPage(Rect body)
        {
            List<string> lists = GetListNamesSafe();

            float footerHeight = 54f;
            Rect listArea = new Rect(
                body.x,
                body.y,
                body.width,
                body.height - footerHeight - 12f
            );

            float rowHeight = 56f;
            float contentHeight = Mathf.Max(
                listArea.height,
                12f + Mathf.Max(1, lists.Count) * (rowHeight + 8f)
            );

            Rect content = new Rect(
                0f,
                0f,
                listArea.width - 18f,
                contentHeight
            );

            scrollPosition = GUI.BeginScrollView(
                listArea,
                scrollPosition,
                content
            );

            if (lists.Count == 0)
            {
                DrawEmptyState(
                    new Rect(4f, 4f, content.width - 8f, 110f),
                    T("no_lists"),
                    T("create_list_start")
                );
            }
            else
            {
                for (int i = 0; i < lists.Count; i++)
                {
                    string listName = lists[i];
                    bool allFriends = IsAllFriendsName(listName);

                    Color color = allFriends
                        ? new Color(0.12f, 0.42f, 0.78f, 1f)
                        : new Color(0.16f, 0.18f, 0.23f, 1f);

                    int listCount = GetListCountSafe(listName);
                    string displayListName = allFriends
                        ? T("all_friends")
                        : listName;

                    string countText = AutoFriendInviteTexts.Format(
                        listCount == 1 ? "player_count_one" : "player_count_many",
                        listCount
                    );

                    string label =
                        displayListName +
                        "   •   " +
                        countText;

                    if (DrawTintedButton(
                        new Rect(4f, 8f + i * (rowHeight + 8f), content.width - 8f, rowHeight),
                        label,
                        color))
                    {
                        if (AutoFriendInviteManager.SetCurrentList(listName))
                        {
                            SyncRenameField();
                            selectedPlayerPuid = "";
                            selectedPlayerName = "";
                            OpenPage(UiPage.ListDetails);
                        }
                    }
                }
            }

            GUI.EndScrollView();

            Rect footer = new Rect(
                body.x,
                body.y + body.height - footerHeight,
                body.width,
                footerHeight
            );

            float half = (footer.width - 10f) * 0.5f;

            if (DrawTintedButton(
                new Rect(footer.x, footer.y, half, 44f),
                T("create_list"),
                new Color(0.12f, 0.58f, 0.34f, 1f)))
            {
                OpenCreateDialog();
            }

            bool stopEnabled = AutoFriendInviteManager.Enabled;
            bool oldEnabled = GUI.enabled;
            GUI.enabled = oldEnabled && stopEnabled;

            if (DrawTintedButton(
                new Rect(footer.x + half + 10f, footer.y, half, 44f),
                T("stop_auto_invite"),
                stopEnabled
                    ? new Color(0.70f, 0.17f, 0.20f, 1f)
                    : new Color(0.18f, 0.19f, 0.23f, 1f)))
            {
                AutoFriendInviteManager.StopAutoInvite(true);
            }

            GUI.enabled = oldEnabled;
        }

        [HideFromIl2Cpp]
        private void DrawListDetailsPage(Rect body)
        {
            string current = GetCurrentListSafe();
            bool editable = !IsAllFriendsName(current);
            List<AutoFriendInviteManager.InviteEntry> entries =
                GetVisibleSelectedEntries();

            float topHeight = 62f;
            Rect top = new Rect(body.x, body.y, body.width, topHeight);

            if (DrawTintedButton(
                new Rect(top.x, top.y, top.width, 44f),
                AutoFriendInviteManager.Enabled
                    ? T("stop_auto_invite")
                    : T("invite_this_list"),
                AutoFriendInviteManager.Enabled
                    ? new Color(0.70f, 0.17f, 0.20f, 1f)
                    : new Color(0.14f, 0.48f, 0.86f, 1f)))
            {
                if (AutoFriendInviteManager.Enabled)
                    AutoFriendInviteManager.StopAutoInvite(true);
                else
                    AutoFriendInviteManager.StartInviteList(current);
            }

            float footerHeight = editable ? 104f : 54f;

            Rect listArea = new Rect(
                body.x,
                body.y + topHeight,
                body.width,
                body.height - topHeight - footerHeight - 8f
            );

            float rowHeight = 50f;
            float contentHeight = Mathf.Max(
                listArea.height,
                12f + Mathf.Max(1, entries.Count) * (rowHeight + 8f)
            );

            Rect content = new Rect(
                0f,
                0f,
                listArea.width - 18f,
                contentHeight
            );

            scrollPosition = GUI.BeginScrollView(
                listArea,
                scrollPosition,
                content
            );

            if (entries.Count == 0)
            {
                DrawEmptyState(
                    new Rect(4f, 6f, content.width - 8f, 120f),
                    editable ? T("empty_list") : T("no_friends"),
                    editable
                        ? T("empty_list_hint")
                        : T("no_friends_hint")
                );
            }
            else
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    AutoFriendInviteManager.InviteEntry entry = entries[i];

                    if (entry == null)
                        continue;

                    string label = GetEntryName(entry);

                    Color rowColor = editable
                        ? new Color(0.16f, 0.18f, 0.23f, 1f)
                        : new Color(0.10f, 0.32f, 0.56f, 1f);

                    bool oldEnabled = GUI.enabled;
                    GUI.enabled = oldEnabled && editable;

                    if (DrawTintedButton(
                        new Rect(4f, 8f + i * (rowHeight + 8f), content.width - 8f, rowHeight),
                        label,
                        rowColor))
                    {
                        selectedPlayerPuid = entry.Puid ?? "";
                        selectedPlayerName = label;
                        OpenPage(UiPage.PlayerActions);
                    }

                    GUI.enabled = oldEnabled;
                }
            }

            GUI.EndScrollView();

            float footerY = body.y + body.height - footerHeight;

            if (editable)
            {
                if (DrawTintedButton(
                    new Rect(body.x, footerY, body.width, 44f),
                    T("add_player"),
                    new Color(0.12f, 0.58f, 0.34f, 1f)))
                {
                    OpenPage(UiPage.AddFriends);
                }

                float third = (body.width - 20f) / 3f;
                float y = footerY + 54f;

                if (DrawTintedButton(
                    new Rect(body.x, y, third, 42f),
                    T("back"),
                    new Color(0.18f, 0.19f, 0.23f, 1f)))
                {
                    OpenPage(UiPage.Lists);
                }

                if (DrawTintedButton(
                    new Rect(body.x + third + 10f, y, third, 42f),
                    T("rename"),
                    new Color(0.63f, 0.42f, 0.11f, 1f)))
                {
                    OpenRenameDialog();
                }

                bool canDelete = !string.Equals(
                    current,
                    AutoFriendInviteManager.DefaultListName,
                    StringComparison.OrdinalIgnoreCase
                );

                bool oldEnabled = GUI.enabled;
                GUI.enabled = oldEnabled && canDelete;

                if (DrawTintedButton(
                    new Rect(body.x + (third + 10f) * 2f, y, third, 42f),
                    T("delete_list"),
                    canDelete
                        ? new Color(0.66f, 0.16f, 0.19f, 1f)
                        : new Color(0.18f, 0.19f, 0.23f, 1f)))
                {
                    if (AutoFriendInviteManager.DeleteList(current, true))
                    {
                        selectedPlayerPuid = "";
                        selectedPlayerName = "";
                        SyncRenameField();
                        OpenPage(UiPage.Lists);
                    }
                }

                GUI.enabled = oldEnabled;
            }
            else
            {
                if (DrawTintedButton(
                    new Rect(body.x, footerY, body.width, 44f),
                    T("back"),
                    new Color(0.18f, 0.19f, 0.23f, 1f)))
                {
                    OpenPage(UiPage.Lists);
                }
            }
        }

        [HideFromIl2Cpp]
        private void DrawAddChoicePage(Rect body)
        {
            float buttonHeight = 72f;
            float startY = body.y + (body.height - buttonHeight) * 0.5f - 28f;

            GUIStyle info = new GUIStyle(mutedLabelStyle);
            info.alignment = TextAnchor.MiddleCenter;

            GUI.Label(
                new Rect(body.x, startY - 52f, body.width, 34f),
                T("choose_section"),
                info
            );

            if (DrawTintedButton(
                new Rect(body.x + 50f, startY, body.width - 100f, buttonHeight),
                T("friends"),
                new Color(0.12f, 0.48f, 0.84f, 1f)))
            {
                OpenPage(UiPage.AddFriends);
            }

            if (DrawTintedButton(
                new Rect(body.x, body.y + body.height - 44f, body.width, 44f),
                T("back"),
                new Color(0.18f, 0.19f, 0.23f, 1f)))
            {
                OpenPage(UiPage.ListDetails);
            }
        }

        [HideFromIl2Cpp]
        private void DrawAvailablePlayersPage(Rect body, bool friendsOnly)
        {
            List<AutoFriendInviteManager.InviteEntry> friends = GetAvailableOfficialFriends();

            float footerHeight = 54f;
            Rect listArea = new Rect(
                body.x,
                body.y,
                body.width,
                body.height - footerHeight - 8f
            );

            float rowHeight = 52f;
            float contentHeight = Mathf.Max(
                listArea.height,
                12f + Mathf.Max(1, friends.Count) * (rowHeight + 8f)
            );

            Rect content = new Rect(
                0f,
                0f,
                listArea.width - 18f,
                contentHeight
            );

            scrollPosition = GUI.BeginScrollView(
                listArea,
                scrollPosition,
                content
            );

            if (friends.Count == 0)
            {
                DrawEmptyState(
                    new Rect(4f, 6f, content.width - 8f, 120f),
                    T("no_friends"),
                    T("no_friends_hint")
                );
            }
            else
            {
                for (int i = 0; i < friends.Count; i++)
                {
                    AutoFriendInviteManager.InviteEntry entry = friends[i];

                    if (entry == null || string.IsNullOrWhiteSpace(entry.Puid))
                        continue;

                    string friendName = GetEntryName(entry);

                    if (DrawTintedButton(
                        new Rect(4f, 8f + i * (rowHeight + 8f), content.width - 8f, rowHeight),
                        friendName,
                        new Color(0.12f, 0.40f, 0.70f, 1f)))
                    {
                        try
                        {
                            AutoFriendInviteManager.AddEntryToList(
                                GetCurrentListSafe(),
                                new AutoFriendInviteManager.InviteEntry
                                {
                                    Puid = entry.Puid,
                                    FriendCode = entry.FriendCode,
                                    DisplayName = entry.DisplayName
                                },
                                true
                            );
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(
                                "[AutoFriendInviteUi] Add friend failed: " + ex
                            );
                        }
                    }
                }
            }

            GUI.EndScrollView();

            if (DrawTintedButton(
                new Rect(
                    body.x,
                    body.y + body.height - 44f,
                    body.width,
                    44f
                ),
                T("back"),
                new Color(0.18f, 0.19f, 0.23f, 1f)))
            {
                OpenPage(UiPage.ListDetails);
            }
        }

        [HideFromIl2Cpp]
        private List<AutoFriendInviteManager.InviteEntry> GetAvailableOfficialFriends()
        {
            List<AutoFriendInviteManager.InviteEntry> result =
                new List<AutoFriendInviteManager.InviteEntry>();

            try
            {
                HashSet<string> assigned = GetAssignedCustomListPuids();
                List<AutoFriendInviteManager.InviteEntry> allFriends =
                    AutoFriendInviteManager.GetAllFriendEntries();

                if (allFriends == null)
                    return result;

                for (int i = 0; i < allFriends.Count; i++)
                {
                    AutoFriendInviteManager.InviteEntry entry = allFriends[i];

                    if (entry == null || string.IsNullOrWhiteSpace(entry.Puid))
                        continue;

                    if (assigned.Contains(entry.Puid))
                        continue;

                    result.Add(entry);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[AutoFriendInviteUi] GetAvailableOfficialFriends failed: " + ex
                );
            }

            return result;
        }

        [HideFromIl2Cpp]
        private void DrawPlayerActionsPage(Rect body)
        {
            if (string.IsNullOrWhiteSpace(selectedPlayerPuid))
            {
                DrawEmptyState(
                    new Rect(body.x, body.y + 30f, body.width, 120f),
                    T("player_unavailable"),
                    T("player_unavailable_hint")
                );

                if (DrawTintedButton(
                    new Rect(body.x, body.y + body.height - 44f, body.width, 44f),
                    T("back"),
                    new Color(0.18f, 0.19f, 0.23f, 1f)))
                {
                    OpenPage(UiPage.ListDetails);
                }

                return;
            }

            float cardWidth = Mathf.Min(520f, body.width - 40f);
            float cardX = body.x + (body.width - cardWidth) * 0.5f;
            float startY = body.y + 74f;

            DrawPanel(
                new Rect(cardX, startY, cardWidth, 210f)
            );

            GUIStyle centeredHeader = new GUIStyle(headerStyle);
            centeredHeader.alignment = TextAnchor.MiddleCenter;
            centeredHeader.fontSize = 20;

            GUI.Label(
                new Rect(cardX + 18f, startY + 22f, cardWidth - 36f, 34f),
                selectedPlayerName,
                centeredHeader
            );

            if (DrawTintedButton(
                new Rect(cardX + 24f, startY + 78f, cardWidth - 48f, 48f),
                T("move_to"),
                new Color(0.14f, 0.48f, 0.86f, 1f)))
            {
                OpenPage(UiPage.MovePlayer);
            }

            if (DrawTintedButton(
                new Rect(cardX + 24f, startY + 140f, cardWidth - 48f, 48f),
                T("remove_from_list"),
                new Color(0.68f, 0.16f, 0.19f, 1f)))
            {
                try
                {
                    AutoFriendInviteManager.RemoveByPuidFromList(
                        GetCurrentListSafe(),
                        selectedPlayerPuid,
                        true
                    );
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        "[AutoFriendInviteUi] Remove player failed: " + ex
                    );
                }

                selectedPlayerPuid = "";
                selectedPlayerName = "";
                OpenPage(UiPage.ListDetails);
            }

            if (DrawTintedButton(
                new Rect(body.x, body.y + body.height - 44f, body.width, 44f),
                T("back"),
                new Color(0.18f, 0.19f, 0.23f, 1f)))
            {
                OpenPage(UiPage.ListDetails);
            }
        }

        [HideFromIl2Cpp]
        private void DrawMovePlayerPage(Rect body)
        {
            List<string> targets = GetMoveTargets();

            float footerHeight = 54f;
            Rect listArea = new Rect(
                body.x,
                body.y,
                body.width,
                body.height - footerHeight - 8f
            );

            float rowHeight = 52f;
            float contentHeight = Mathf.Max(
                listArea.height,
                12f + Mathf.Max(1, targets.Count) * (rowHeight + 8f)
            );

            Rect content = new Rect(
                0f,
                0f,
                listArea.width - 18f,
                contentHeight
            );

            scrollPosition = GUI.BeginScrollView(
                listArea,
                scrollPosition,
                content
            );

            if (targets.Count == 0)
            {
                DrawEmptyState(
                    new Rect(4f, 6f, content.width - 8f, 120f),
                    T("no_target_lists"),
                    T("no_target_lists_hint")
                );
            }
            else
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    string target = targets[i];

                    if (DrawTintedButton(
                        new Rect(4f, 8f + i * (rowHeight + 8f), content.width - 8f, rowHeight),
                        target,
                        new Color(0.14f, 0.40f, 0.68f, 1f)))
                    {
                        if (MoveSelectedPlayerTo(target))
                        {
                            selectedPlayerPuid = "";
                            selectedPlayerName = "";
                            OpenPage(UiPage.ListDetails);
                        }
                    }
                }
            }

            GUI.EndScrollView();

            if (DrawTintedButton(
                new Rect(
                    body.x,
                    body.y + body.height - 44f,
                    body.width,
                    44f
                ),
                T("back"),
                new Color(0.18f, 0.19f, 0.23f, 1f)))
            {
                OpenPage(UiPage.PlayerActions);
            }
        }

        [HideFromIl2Cpp]
        private List<PlayerControl> GetAvailableLobbyPlayers(bool friendsOnly)
        {
            List<PlayerControl> result = new List<PlayerControl>();
            HashSet<string> assigned = GetAssignedCustomListPuids();
            List<PlayerControl> lobbyPlayers = GetLobbyPlayers();

            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                PlayerControl player = lobbyPlayers[i];

                if (player == null)
                    continue;

                string puid = GetPuid(player);

                if (string.IsNullOrWhiteSpace(puid))
                    continue;

                if (assigned.Contains(puid))
                    continue;

                bool isFriend = IsFriend(puid);

                if (friendsOnly != isFriend)
                    continue;

                result.Add(player);
            }

            return result;
        }

        [HideFromIl2Cpp]
        private HashSet<string> GetAssignedCustomListPuids()
        {
            HashSet<string> result =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                List<string> lists =
                    AutoFriendInviteManager.GetListNames(false);

                if (lists == null)
                    return result;

                for (int i = 0; i < lists.Count; i++)
                {
                    string listName = lists[i];

                    if (string.IsNullOrWhiteSpace(listName) ||
                        IsAllFriendsName(listName))
                    {
                        continue;
                    }

                    List<AutoFriendInviteManager.InviteEntry> entries =
                        AutoFriendInviteManager.GetEntries(listName);

                    if (entries == null)
                        continue;

                    for (int e = 0; e < entries.Count; e++)
                    {
                        AutoFriendInviteManager.InviteEntry entry = entries[e];

                        if (entry == null ||
                            string.IsNullOrWhiteSpace(entry.Puid))
                        {
                            continue;
                        }

                        result.Add(entry.Puid);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[AutoFriendInviteUi] GetAssignedCustomListPuids failed: " +
                    ex
                );
            }

            return result;
        }

        [HideFromIl2Cpp]
        private List<string> GetMoveTargets()
        {
            List<string> result = new List<string>();
            string current = GetCurrentListSafe();

            try
            {
                List<string> lists =
                    AutoFriendInviteManager.GetListNames(false);

                if (lists == null)
                    return result;

                for (int i = 0; i < lists.Count; i++)
                {
                    string listName = lists[i];

                    if (string.IsNullOrWhiteSpace(listName))
                        continue;

                    if (IsAllFriendsName(listName))
                        continue;

                    if (string.Equals(
                        listName,
                        current,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    result.Add(listName);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[AutoFriendInviteUi] GetMoveTargets failed: " + ex
                );
            }

            return result;
        }

        [HideFromIl2Cpp]
        private AutoFriendInviteManager.InviteEntry FindEntryInList(
            string listName,
            string puid)
        {
            if (string.IsNullOrWhiteSpace(listName) ||
                string.IsNullOrWhiteSpace(puid))
            {
                return null;
            }

            try
            {
                List<AutoFriendInviteManager.InviteEntry> entries =
                    AutoFriendInviteManager.GetEntries(listName);

                if (entries == null)
                    return null;

                for (int i = 0; i < entries.Count; i++)
                {
                    AutoFriendInviteManager.InviteEntry entry = entries[i];

                    if (entry == null)
                        continue;

                    if (string.Equals(
                        entry.Puid,
                        puid,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return new AutoFriendInviteManager.InviteEntry
                        {
                            Puid = entry.Puid,
                            FriendCode = entry.FriendCode,
                            DisplayName = entry.DisplayName
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[AutoFriendInviteUi] FindEntryInList failed: " + ex
                );
            }

            return null;
        }

        [HideFromIl2Cpp]
        private bool MoveSelectedPlayerTo(string targetList)
        {
            string sourceList = GetCurrentListSafe();

            if (string.IsNullOrWhiteSpace(selectedPlayerPuid) ||
                string.IsNullOrWhiteSpace(sourceList) ||
                string.IsNullOrWhiteSpace(targetList))
            {
                return false;
            }

            AutoFriendInviteManager.InviteEntry entry =
                FindEntryInList(sourceList, selectedPlayerPuid);

            if (entry == null)
            {
                AutoFriendInviteManager.ShowChat(
                    "<color=#ff5555>Player not found in current list.</color>"
                );
                return false;
            }

            try
            {
                AutoFriendInviteManager.RemoveByPuidFromList(
                    sourceList,
                    selectedPlayerPuid,
                    false
                );

                try
                {
                    AutoFriendInviteManager.AddEntryToList(
                        targetList,
                        entry,
                        false
                    );
                }
                catch
                {
                    AutoFriendInviteManager.AddEntryToList(
                        sourceList,
                        entry,
                        false
                    );
                    throw;
                }

                AutoFriendInviteManager.ShowChat(
                    "<color=#00ff00>Moved:</color> " +
                    GetEntryName(entry) +
                    " <color=#cccccc>(" +
                    sourceList +
                    " -> " +
                    targetList +
                    ")</color>"
                );

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[AutoFriendInviteUi] MoveSelectedPlayerTo failed: " +
                    ex
                );

                AutoFriendInviteManager.ShowChat(
                    "<color=#ff5555>Unable to move player.</color>"
                );

                return false;
            }
        }

        [HideFromIl2Cpp]
        private float CalculateContentHeight(
            int listCount,
            int lobbyCount,
            int selectedCount,
            bool allFriends)
        {
            int listRows = Mathf.Max(1, (listCount + 3) / 4);
            int selectedRows = Mathf.Max(1, (selectedCount + 1) / 2);

            float result = 105f + listRows * 42f;

            if (allFriends)
                result += 95f;
            else
                result += 150f + lobbyCount * 46f;

            result += 80f + selectedRows * 46f;
            result += 80f;

            return Mathf.Max(580f, result);
        }

        [HideFromIl2Cpp]
        private float DrawListManager(float width, float y, List<string> lists)
        {
            float startY = y;
            int rows = Mathf.Max(1, (lists.Count + 3) / 4);
            float boxHeight = 48f + rows * 42f;

            GUI.Box(
                new Rect(0f, startY, width, boxHeight),
                GUIContent.none,
                sectionStyle
            );

            float innerX = 10f;
            float innerWidth = width - 20f;
            float cy = startY + 8f;

            GUI.Label(
                new Rect(innerX, cy, innerWidth, 26f),
                "INVITE LISTS - CLICK A LIST TO SELECT IT",
                headerStyle
            );

            cy += 31f;

            float buttonWidth = (innerWidth - Gap * 3f) / 4f;

            for (int i = 0; i < lists.Count; i++)
            {
                int row = i / 4;
                int col = i % 4;

                string listName = lists[i];

                Rect r = new Rect(
                    innerX + col * (buttonWidth + Gap),
                    cy + row * 42f,
                    buttonWidth,
                    34f
                );

                Color oldBg = GUI.backgroundColor;

                bool selected = string.Equals(
                    listName,
                    GetCurrentListSafe(),
                    StringComparison.OrdinalIgnoreCase
                );

                GUI.backgroundColor = selected
                    ? new Color(0.1f, 0.55f, 0.95f, 1f)
                    : new Color(0.28f, 0.28f, 0.28f, 1f);

                string label =
                    (selected ? "> " : "") +
                    listName +
                    " (" +
                    GetListCountSafe(listName) +
                    ")";

                if (GUI.Button(r, label, buttonStyle))
                {
                    if (AutoFriendInviteManager.SetCurrentList(listName))
                        SyncRenameField();
                }

                GUI.backgroundColor = oldBg;
            }

            return startY + boxHeight;
        }

        private void DrawNameDialog(float width, float height)
        {
            HandleNameDialogEvent();

            Color oldColor = GUI.color;
            Color oldBg = GUI.backgroundColor;
            bool oldEnabled = GUI.enabled;

            float dialogWidth = Mathf.Min(620f, width - 80f);
            float dialogHeight = 220f;
            float x = (width - dialogWidth) * 0.5f;
            float y = Mathf.Max(48f, (height - dialogHeight) * 0.5f);

            GUI.color = Color.white;
            GUI.backgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f);

            GUI.Box(
                new Rect(x, y, dialogWidth, dialogHeight),
                GUIContent.none,
                sectionStyle
            );

            string title = nameDialogMode == 1
                ? T("create_new_list")
                : T("rename_current_list");

            GUI.Label(
                new Rect(x + 16f, y + 12f, dialogWidth - 32f, 30f),
                title,
                headerStyle
            );

            GUI.Label(
                new Rect(x + 16f, y + 46f, dialogWidth - 32f, 28f),
                T("type_list_name"),
                labelStyle
            );

            string visibleValue = string.IsNullOrEmpty(nameDialogBuffer)
                ? T("type_name")
                : nameDialogBuffer;

            GUI.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);

            GUI.Box(
                new Rect(x + 16f, y + 78f, dialogWidth - 32f, 44f),
                visibleValue,
                buttonStyle
            );

            GUI.Label(
                new Rect(x + 16f, y + 128f, dialogWidth - 32f, 26f),
                T("keyboard_help"),
                labelStyle
            );

            float actionWidth = (dialogWidth - 46f) * 0.5f;

            GUI.backgroundColor = new Color(0.15f, 0.7f, 0.2f, 1f);

            if (GUI.Button(
                new Rect(x + 16f, y + 166f, actionWidth, 36f),
                T("confirm"),
                buttonStyle))
            {
                SubmitNameDialog();
            }

            GUI.backgroundColor = new Color(0.65f, 0.18f, 0.18f, 1f);

            if (GUI.Button(
                new Rect(
                    x + 30f + actionWidth,
                    y + 166f,
                    actionWidth,
                    36f
                ),
                T("cancel"),
                buttonStyle))
            {
                CloseNameDialog();
            }

            GUI.enabled = oldEnabled;
            GUI.backgroundColor = oldBg;
            GUI.color = oldColor;
        }

        private float DrawAllFriendsInfo(float width, float y)
        {
            float h = 82f;

            GUI.Box(new Rect(0f, y, width, h), GUIContent.none, sectionStyle);
            GUI.Label(new Rect(10f, y + 8f, width - 20f, 26f), "ALL FRIENDS", headerStyle);
            GUI.Label(
                new Rect(10f, y + 35f, width - 20f, 38f),
                "\"All Friends\" contains the official friends returned by the game server. " +
                "Use a custom list to save lobby players who are not friends.",
                labelStyle
            );

            return y + h;
        }

        [HideFromIl2Cpp]
        private float DrawLobbyPlayers(float width, float y, List<PlayerControl> players)
        {
            float h = 130f + Mathf.Max(1, players.Count) * 46f;

            GUI.Box(new Rect(0f, y, width, h), GUIContent.none, sectionStyle);

            GUI.Label(new Rect(10f, y + 8f, width - 20f, 26f), "PLAYERS IN LOBBY", headerStyle);

            GUI.Label(
                new Rect(10f, y + 34f, width - 20f, 36f),
                "All lobby players with a valid PUID are shown here, including players who are NOT official friends.",
                labelStyle
            );

            float half = (width - 28f) * 0.5f;
            Color oldBg = GUI.backgroundColor;

            GUI.backgroundColor = new Color(0.15f, 0.7f, 0.2f, 1f);

            if (GUI.Button(new Rect(10f, y + 74f, half, 34f), "Add All Lobby Players", buttonStyle))
                AddAllLobbyPlayers(players);

            GUI.backgroundColor = new Color(0.78f, 0.15f, 0.15f, 1f);

            if (GUI.Button(
                new Rect(18f + half, y + 74f, half, 34f),
                "Remove All Lobby Players",
                buttonStyle))
            {
                RemoveAllLobbyPlayers(players);
            }

            GUI.backgroundColor = oldBg;

            float cy = y + 116f;

            if (players.Count == 0)
            {
                GUI.Label(new Rect(10f, cy, width - 20f, 34f), "No valid players found in lobby.", labelStyle);
                return y + h;
            }

            for (int i = 0; i < players.Count; i++)
            {
                PlayerControl player = players[i];

                if (player == null)
                    continue;

                string puid = GetPuid(player);
                bool selected = IsSelectedPuidSafe(puid);
                bool friend = IsFriend(puid);

                string label =
                    (selected ? "Remove  " : "Add  ") +
                    GetPlayerName(player) +
                    (friend ? "  [Friend]" : "  [Not Friend]");

                GUI.backgroundColor = selected
                    ? new Color(0.78f, 0.15f, 0.15f, 1f)
                    : new Color(0.15f, 0.65f, 0.2f, 1f);

                if (GUI.Button(
                    new Rect(10f, cy + i * 46f, width - 20f, 38f),
                    label,
                    buttonStyle))
                {
                    TogglePlayer(player);
                }
            }

            GUI.backgroundColor = oldBg;
            return y + h;
        }

        [HideFromIl2Cpp]
        private float DrawSelectedList(
            float width,
            float y,
            List<AutoFriendInviteManager.InviteEntry> entries)
        {
            int rows = Mathf.Max(1, (entries.Count + 1) / 2);
            float h = 52f + rows * 46f;

            GUI.Box(new Rect(0f, y, width, h), GUIContent.none, sectionStyle);

            string current = GetCurrentListSafe();

            GUI.Label(
                new Rect(10f, y + 8f, width - 20f, 26f),
                "CURRENT INVITE LIST: " + current,
                headerStyle
            );

            if (entries.Count == 0)
            {
                GUI.Label(
                    new Rect(10f, y + 37f, width - 20f, 34f),
                    IsAllFriendsName(current)
                        ? "No official friends found."
                        : "The list is empty. Add players from the lobby.",
                    labelStyle
                );

                return y + h;
            }

            bool editable = !IsAllFriendsName(current);
            float buttonWidth = (width - 28f) * 0.5f;
            Color oldBg = GUI.backgroundColor;
            bool oldEnabled = GUI.enabled;

            for (int i = 0; i < entries.Count; i++)
            {
                AutoFriendInviteManager.InviteEntry entry = entries[i];

                if (entry == null)
                    continue;

                int row = i / 2;
                int col = i % 2;

                Rect r = new Rect(
                    10f + col * (buttonWidth + 8f),
                    y + 38f + row * 46f,
                    buttonWidth,
                    38f
                );

                GUI.enabled = oldEnabled && editable;
                GUI.backgroundColor = editable
                    ? new Color(0.78f, 0.15f, 0.15f, 1f)
                    : new Color(0.18f, 0.38f, 0.68f, 1f);

                string label = editable
                    ? "Remove  " + GetEntryName(entry)
                    : GetEntryName(entry);

                if (GUI.Button(r, label, buttonStyle) && editable)
                    AutoFriendInviteManager.RemoveByPuid(entry.Puid, true);
            }

            GUI.enabled = oldEnabled;
            GUI.backgroundColor = oldBg;

            return y + h;
        }

        private void TogglePlayer(PlayerControl player)
        {
            if (player == null || player.Data == null)
                return;

            string puid = GetPuid(player);

            if (string.IsNullOrWhiteSpace(puid))
                return;

            if (IsSelectedPuidSafe(puid))
                AutoFriendInviteManager.RemoveByPuid(puid, true);
            else
                AutoFriendInviteManager.AddByPlayer(player, true);
        }

        [HideFromIl2Cpp]
        private void AddAllLobbyPlayers(List<PlayerControl> players)
        {
            int added = 0;

            for (int i = 0; i < players.Count; i++)
            {
                PlayerControl player = players[i];

                if (player == null)
                    continue;

                string puid = GetPuid(player);

                if (string.IsNullOrWhiteSpace(puid))
                    continue;

                if (IsSelectedPuidSafe(puid))
                    continue;

                try
                {
                    AutoFriendInviteManager.AddByPlayer(player, false);
                    added++;
                }
                catch
                {
                }
            }

            AutoFriendInviteManager.ShowChat(
                "<color=#00ff00>Added " + added + " lobby players to:</color> " +
                GetCurrentListSafe()
            );
        }

        [HideFromIl2Cpp]
        private void RemoveAllLobbyPlayers(List<PlayerControl> players)
        {
            int removed = 0;

            for (int i = 0; i < players.Count; i++)
            {
                string puid = GetPuid(players[i]);

                if (string.IsNullOrWhiteSpace(puid))
                    continue;

                if (!IsSelectedPuidSafe(puid))
                    continue;

                try
                {
                    AutoFriendInviteManager.RemoveByPuid(puid, false);
                    removed++;
                }
                catch
                {
                }
            }

            AutoFriendInviteManager.ShowChat(
                "<color=#ffcc00>Removed " + removed + " lobby players from:</color> " +
                GetCurrentListSafe()
            );
        }

        private bool IsLocalPlayer(PlayerControl player)
        {
            if (player == null)
                return false;

            PlayerControl local = null;

            try
            {
                local = PlayerControl.LocalPlayer;
            }
            catch
            {
            }

            if (local == null)
                return false;

            if (player == local)
                return true;

            try
            {
                if (player.PlayerId == local.PlayerId)
                    return true;
            }
            catch
            {
            }

            string playerPuid = GetPuid(player);
            string localPuid = GetPuid(local);

            return !string.IsNullOrEmpty(playerPuid) &&
                   !string.IsNullOrEmpty(localPuid) &&
                   playerPuid == localPuid;
        }

        private string GetPuid(PlayerControl player)
        {
            try
            {
                if (player == null ||
                    player.Data == null ||
                    string.IsNullOrWhiteSpace(player.Data.Puid))
                {
                    return "";
                }

                return player.Data.Puid;
            }
            catch
            {
                return "";
            }
        }

        private string GetPlayerName(PlayerControl player)
        {
            try
            {
                if (player == null ||
                    player.Data == null ||
                    string.IsNullOrWhiteSpace(player.Data.PlayerName))
                {
                    return "Unknown Player";
                }

                return player.Data.PlayerName;
            }
            catch
            {
                return "Unknown Player";
            }
        }

        private bool IsSelectedPuidSafe(string puid)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(puid) &&
                       AutoFriendInviteManager.IsSelectedPuid(puid);
            }
            catch
            {
                return false;
            }
        }

        private bool IsFriend(string puid)
        {
            if (string.IsNullOrWhiteSpace(puid))
                return false;

            try
            {
                if (!DestroyableSingleton<FriendsListManager>.InstanceExists)
                    return false;

                FriendsListManager manager =
                    DestroyableSingleton<FriendsListManager>.Instance;

                if (manager == null || manager.Friends == null)
                    return false;

                for (int i = 0; i < manager.Friends.Count; i++)
                {
                    ResponseFriends friend = manager.Friends[i];

                    if (friend != null && friend.FriendPuid == puid)
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }

        [HideFromIl2Cpp]
        private List<PlayerControl> GetLobbyPlayers()
        {
            List<PlayerControl> result = new List<PlayerControl>();

            try
            {
                if (PlayerControl.AllPlayerControls == null)
                    return result;

                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player == null || player.gameObject == null)
                        continue;

                    if (player.Data == null)
                        continue;

                    if (IsLocalPlayer(player))
                        continue;

                    string puid = GetPuid(player);

                    if (string.IsNullOrWhiteSpace(puid))
                        continue;

                    result.Add(player);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[AutoFriendInviteUi] GetLobbyPlayers failed: " + ex);
            }

            return result;
        }

        [HideFromIl2Cpp]
        private List<string> GetListNamesSafe()
        {
            try
            {
                List<string> result = AutoFriendInviteManager.GetListNames(true);
                return result ?? new List<string>();
            }
            catch (Exception ex)
            {
                Debug.LogError("[AutoFriendInviteUi] GetListNamesSafe failed: " + ex);
                return new List<string>();
            }
        }

        [HideFromIl2Cpp]
        private List<AutoFriendInviteManager.InviteEntry> GetVisibleSelectedEntries()
        {
            List<AutoFriendInviteManager.InviteEntry> result =
                new List<AutoFriendInviteManager.InviteEntry>();

            try
            {
                List<AutoFriendInviteManager.InviteEntry> entries =
                    AutoFriendInviteManager.GetSelectedEntries();

                if (entries == null)
                    return result;

                string localPuid = "";

                try
                {
                    if (PlayerControl.LocalPlayer != null)
                        localPuid = GetPuid(PlayerControl.LocalPlayer);
                }
                catch
                {
                }

                for (int i = 0; i < entries.Count; i++)
                {
                    AutoFriendInviteManager.InviteEntry entry = entries[i];

                    if (entry == null || string.IsNullOrWhiteSpace(entry.Puid))
                        continue;

                    if (!string.IsNullOrEmpty(localPuid) && entry.Puid == localPuid)
                        continue;

                    result.Add(entry);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[AutoFriendInviteUi] GetVisibleSelectedEntries failed: " + ex);
            }

            return result;
        }

        private string GetCurrentListSafe()
        {
            try
            {
                string result = AutoFriendInviteManager.GetCurrentListName();

                return string.IsNullOrWhiteSpace(result)
                    ? AutoFriendInviteManager.DefaultListName
                    : result;
            }
            catch
            {
                return AutoFriendInviteManager.DefaultListName;
            }
        }

        private int GetListCountSafe(string listName)
        {
            try
            {
                return AutoFriendInviteManager.GetListCount(listName);
            }
            catch
            {
                return 0;
            }
        }

        private bool IsAllFriendsSelected()
        {
            return IsAllFriendsName(GetCurrentListSafe());
        }

        private bool IsAllFriendsName(string listName)
        {
            return string.Equals(
                listName,
                AutoFriendInviteManager.AllFriendsListName,
                StringComparison.OrdinalIgnoreCase
            );
        }

        private void SyncRenameField()
        {
            string current = GetCurrentListSafe();

            if (string.Equals(renameSourceList, current, StringComparison.OrdinalIgnoreCase))
                return;

            renameSourceList = current;

            renameListName = IsAllFriendsName(current)
                ? ""
                : current;
        }

        [HideFromIl2Cpp]
        private string GetEntryName(AutoFriendInviteManager.InviteEntry entry)
        {
            if (entry == null)
                return "Unknown Player";

            if (!string.IsNullOrWhiteSpace(entry.DisplayName))
                return entry.DisplayName;

            if (!string.IsNullOrWhiteSpace(entry.FriendCode))
                return entry.FriendCode;

            if (!string.IsNullOrWhiteSpace(entry.Puid))
                return entry.Puid;

            return "Unknown Player";
        }
    }
}

[HarmonyPatch(typeof(FriendsListUI), nameof(FriendsListUI.Open))]
public static class AddAutoInviteButtonPatch
{
    public static void Postfix(FriendsListUI __instance)
    {
        string buttonName = "AutoFriendInviteBtn";

        if (__instance == null || __instance.transform.Find(buttonName) != null)
            return;

        GameObject customButtonObj = UnityEngine.Object.Instantiate(
            __instance.PlatformFriendsButton,
            __instance.transform
        );

        customButtonObj.name = buttonName;
        customButtonObj.transform.localPosition =
            new Vector3(9.247619f, 4.801587f, -18f);

        PassiveButton button = customButtonObj.GetComponent<PassiveButton>();

        if (button != null)
        {
            button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();

            System.Action action = () =>
            {
                AutoFriendInviteUi.OpenStatic();
            };

            button.OnClick.AddListener((UnityAction)action);
        }

        AutoFriendInviteTexts.RefreshLanguage();

        TextMeshPro[] textComponents =
            customButtonObj.GetComponentsInChildren<TextMeshPro>();

        foreach (TextMeshPro txt in textComponents)
        {
            TextTranslatorTMP translator =
                txt.GetComponent<TextTranslatorTMP>();

            if (translator != null)
                UnityEngine.Object.Destroy(translator);

            txt.text = AutoFriendInviteTexts.Get("button_auto_invite");
        }

        customButtonObj.SetActive(true);
    }
}