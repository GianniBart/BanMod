//credits and licenses in the resources folder
using BepInEx.Unity.IL2CPP.Utils;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BanMod.Translator;
using static BanMod.Utils;

namespace BanMod
{
    public static class CloseMeetingManager
    {
        public static GameObject closeMeetingConfirmUI = null;

        private const string CloseMeetingButtonName = "CloseMeetingButton";
        private const string SmallRightButtonName = "SmallRightButton";

        private const string CloseMeetingIconPath = "BanMod.Resources.image.CloseMeetingIcon.png";
        private const string SmallRightButtonIconPath = "BanMod.Resources.image.SmallRightButtonIcon.png";

        public static IEnumerator WaitForCloseMeetingButton(MeetingHud __instance)
        {
            while (__instance == null)
                yield return null;

            while (__instance.playerStates == null || __instance.playerStates.Count == 0)
                yield return null;

            while (__instance.playerStates.All(pva => pva == null || pva.Buttons == null))
                yield return null;

            CreateCloseMeetingButton(__instance);
        }

        public static void CreateCloseMeetingButton(MeetingHud __instance)
        {
            if (__instance == null)
                return;

            if (AmongUsClient.Instance == null)
                return;

            bool canShowAsHost = AmongUsClient.Instance.AmHost;

            if (!canShowAsHost)
                return;

            CreateMainCloseMeetingButton(__instance);
            CreateSmallRightButton(__instance);
        }

        private static void CreateMainCloseMeetingButton(MeetingHud __instance)
        {
            if (__instance.transform.Find(CloseMeetingButtonName) != null)
                return;

            GameObject template = __instance.SkipVoteButton.gameObject;
            GameObject closeButtonGO = UnityEngine.Object.Instantiate(template, __instance.transform);
            closeButtonGO.name = CloseMeetingButtonName;

            closeButtonGO.transform.localPosition = new Vector3(0f, -2.5f, 0f);
            closeButtonGO.transform.localScale = Vector3.one;

            closeButtonGO.SetActive(true);

            TextMeshPro textMesh = closeButtonGO.GetComponentInChildren<TextMeshPro>();
            if (textMesh != null)
                textMesh.gameObject.SetActive(false);

            SpriteRenderer spriteRenderer = closeButtonGO.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = closeButtonGO.AddComponent<SpriteRenderer>();

            var customSprite = Utils.LoadSprite(CloseMeetingIconPath, 100f);

            if (customSprite != null)
            {
                spriteRenderer.sprite = customSprite;
                spriteRenderer.color = Color.white;
            }
            else
            {
                spriteRenderer.color = new Color(0.8f, 0.4f, 0.1f, 1f);
            }

            PassiveButton button = closeButtonGO.GetComponent<PassiveButton>();
            if (button != null)
            {
                button.OnClick.RemoveAllListeners();

                System.Action value = () =>
                {
                    if (CloseMeetingManager.closeMeetingConfirmUI != null || GuessManager.guesserUI != null)
                        return;

                    ShowCloseMeetingConfirmation(__instance);
                };

                button.OnClick.AddListener((UnityEngine.Events.UnityAction)value);
            }
        }

        private static void CreateSmallRightButton(MeetingHud __instance)
        {
            if (__instance.transform.Find(SmallRightButtonName) != null)
                return;

            GameObject template = __instance.SkipVoteButton.gameObject;
            GameObject smallButtonGO = UnityEngine.Object.Instantiate(template, __instance.transform);
            smallButtonGO.name = SmallRightButtonName;

            smallButtonGO.transform.localPosition = new Vector3(4.85f, -2.65f, 0f);

            smallButtonGO.transform.localScale = new Vector3(0.55f, 0.55f, 1f);

            smallButtonGO.SetActive(true);

            TextMeshPro textMesh = smallButtonGO.GetComponentInChildren<TextMeshPro>();
            if (textMesh != null)
                textMesh.gameObject.SetActive(false);

            SpriteRenderer spriteRenderer = smallButtonGO.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = smallButtonGO.AddComponent<SpriteRenderer>();

            var customSprite = Utils.LoadSprite(SmallRightButtonIconPath, 100f);

            if (customSprite != null)
            {
                spriteRenderer.sprite = customSprite;
                spriteRenderer.color = Color.white;
            }
            else
            {
                spriteRenderer.color = new Color(0.2f, 0.6f, 1f, 1f);
            }

            PassiveButton button = smallButtonGO.GetComponent<PassiveButton>();
            if (button != null)
            {
                button.OnClick.RemoveAllListeners();

                System.Action value = () =>
                {
                    if (CloseMeetingManager.closeMeetingConfirmUI != null || GuessManager.guesserUI != null)
                        return;

                    Utils.SondaggioChiusuraMeeting(__instance, true);
                };

                button.OnClick.AddListener((UnityEngine.Events.UnityAction)value);
            }
        }

        // Kept as the public entry point for callers: confirmation is now a lobby poll.
        public static void ShowCloseMeetingConfirmation(MeetingHud __instance)
        {
            if (__instance == null || AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
                return;
            Utils.SondaggioChiusuraMeeting(__instance);
        }

        public static void ResetForNewGame()
        {
            DestroyConfirmUI();
            DestroyMeetingButton(CloseMeetingButtonName);
            DestroyMeetingButton(SmallRightButtonName);
        }

        public static void CleanupAfterMeeting()
        {
            DestroyConfirmUI();
            DestroyMeetingButton(CloseMeetingButtonName);
            DestroyMeetingButton(SmallRightButtonName);
        }

        private static void DestroyConfirmUI()
        {
            if (closeMeetingConfirmUI != null)
            {
                UnityEngine.Object.Destroy(closeMeetingConfirmUI);
                closeMeetingConfirmUI = null;
            }
        }

        private static void DestroyMeetingButton(string buttonName)
        {
            if (MeetingHud.Instance == null)
                return;

            var existingButton = MeetingHud.Instance.transform.Find(buttonName);

            if (existingButton != null)
                UnityEngine.Object.Destroy(existingButton.gameObject);
        }
    }
}