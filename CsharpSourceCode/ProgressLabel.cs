using Framework;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DressmakerArchipelago
{

    internal static class ProgressLabel
    {
        internal static GameManager.Scene? currentScene;

        private static readonly Vector2 padding = new Vector2(24f, 10f);
        private const float distanceFromTop = 20f;

        private static RectTransform box;     
        private static TextMeshProUGUI label;  
        private static float nextCreateAttempt;

        internal static void Update()
        {
            bool shouldShow = currentScene == GameManager.Scene.FrontDesk
                              && !string.IsNullOrEmpty(LocationChecks.progressString)
                              && (LocationChecks.goalChoice == 0 || LocationChecks.goalChoice == 1);


            if (box == null)
            {
                if (!shouldShow || Time.realtimeSinceStartup < nextCreateAttempt) return;
                nextCreateAttempt = Time.realtimeSinceStartup + 1f;
                if (!TryCreate()) return;
            }

            if (box.gameObject.activeSelf != shouldShow)
            {
                box.gameObject.SetActive(shouldShow);
            }
            if (shouldShow && label.text != LocationChecks.progressString)
            {
                label.text = LocationChecks.progressString;
                Vector2 textSize = label.GetPreferredValues(label.text);
                box.sizeDelta = textSize + padding * 2f;
            }
        }

        private static bool TryCreate()
        {
            FrontDesk frontDesk = Object.FindFirstObjectByType<FrontDesk>();
            if (frontDesk == null) return false;
            Canvas canvas = frontDesk.GetComponentInParent<Canvas>();
            if (canvas == null) return false;

            GoldDisplay goldDisplay = Traverse.Create(SingletonBehaviour<NotificationHandler>.Instance).Field("goldDisplay").GetValue<GoldDisplay>();
            TextMeshProUGUI goldText = goldDisplay == null ? null : Traverse.Create(goldDisplay).Field("goldText").GetValue<TextMeshProUGUI>();

            GameObject boxObject = new GameObject("ArchipelagoGoalProgress", typeof(RectTransform));
            boxObject.transform.SetParent(canvas.rootCanvas.transform, false);
            boxObject.transform.SetAsLastSibling(); 
            box = (RectTransform)boxObject.transform;
            box.anchorMin = new Vector2(0.5f, 1f);
            box.anchorMax = new Vector2(0.5f, 1f);
            box.pivot = new Vector2(0.5f, 1f);
            box.anchoredPosition = new Vector2(0f, -distanceFromTop);
            CopyBoxStyle(boxObject, goldDisplay, goldText);

            GameObject textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(box, false);
            label = textObject.AddComponent<TextMeshProUGUI>();
            label.raycastTarget = false; 
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            RectTransform textRect = label.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = padding;
            textRect.offsetMax = -padding;
            CopyTextStyle(label, goldDisplay, goldText, frontDesk);

            Plugin.Logger.LogInfo("Created goal progress label");
            return true;
        }
        private static void CopyBoxStyle(GameObject target, GoldDisplay goldDisplay, TextMeshProUGUI goldText)
        {
            Image source = null;
            if (goldText != null && goldDisplay != null)
            {
                for (Transform t = goldText.transform.parent; t != null; t = t.parent)
                {
                    Image image = t.GetComponent<Image>();
                    if (image != null && image.sprite != null)
                    {
                        source = image;
                        break;
                    }
                    if (t == goldDisplay.transform) break; 
                }
            }
            if (source == null)
            {
                Plugin.Logger.LogWarning("Couldn't find the gold counter's box; label will have no background");
                return;
            }
            Plugin.Logger.LogInfo($"Copying box from '{source.name}' (sprite '{source.sprite.name}')");

            Image boxImage = target.AddComponent<Image>();
            boxImage.sprite = source.sprite;
            boxImage.type = source.type; 
            boxImage.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
            boxImage.color = source.color;
            boxImage.material = source.material;
            boxImage.raycastTarget = false;
        }

        private static void CopyTextStyle(TextMeshProUGUI target, GoldDisplay goldDisplay, TextMeshProUGUI goldText, FrontDesk frontDesk)
        {
            TextMeshProUGUI source = goldText != null ? goldText : Traverse.Create(frontDesk).Field("nameText").GetValue<TextMeshProUGUI>();
            if (source == null)
            {
                Plugin.Logger.LogWarning("Couldn't find game text to copy style from; using defaults");
                return;
            }

            target.font = source.font;
            target.fontSharedMaterial = source.fontSharedMaterial; 
            target.fontSize = source.fontSize;
            target.fontStyle = source.fontStyle;
            target.color = source.color;
            if (goldText != null)
            {
                target.color = Traverse.Create(goldDisplay).Field("defaultColour").GetValue<Color>();
            }
        }
    }
}
