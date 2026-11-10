using BepInEx;
using Framework;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace DressmakerArchipelago
{
    [HarmonyPatch(typeof(GarmentComponentUIButton), "Initialize")]
    public static class AddGarmentCheckmark
    {
        [HarmonyPostfix]
        public static void AddGarmentCheck(GarmentComponentUIButton __instance, GarmentComponent gc, Image ___icon)
        {
            if (Plugin.playerJson == null) return;

            if (!Plugin.playerJson.allLocationsReached.Contains(gc.name))
            {
                return;
            }
            Transform garmentTransform = ___icon.transform;

            Checkmark.AddMark(garmentTransform);
        }

    }
    [HarmonyPatch(typeof(SidebarInventory), "RefreshFabrics")]
    public static class AddInventoryFabricCheckmark
    {
        [HarmonyPostfix]
        public static void AddFabricCheck(SidebarInventory __instance, List<InventoryItem> ____fabricItems)
        {
            if (Plugin.playerJson == null) return;
            foreach(InventoryItem fabricItem in ____fabricItems)
            {
                if (!Plugin.playerJson.allLocationsReached.Contains(fabricItem.FabricPiece.fabric.name))
                {
                    continue;
                }
                Checkmark.AddMark(fabricItem.transform);
            }


        }

    }
    [HarmonyPatch(typeof(Shop), "PopulateFabrics")]
    public static class AddShopFabricCheckmark
    {
        [HarmonyPostfix]
        public static void AddFabricCheck(SidebarInventory __instance, List<ShopBolt> ____allBolts)
        {
            if (Plugin.playerJson == null) return;
            foreach (ShopBolt fabricItem in ____allBolts)
            {
                if (!Plugin.playerJson.allLocationsReached.Contains(fabricItem.Fabric.name))
                {
                    continue;
                }
                Checkmark.AddMarkTo3DObject(fabricItem.HighlightPosition);
            }


        }

    }
    public static class Checkmark
    {
        public static void AddMark(Transform img, bool fillFull = false)
        {
            if (!Plugin.showCheckmarks.Value)
            {
                return;
            }
            if(img.childCount > 5)
            {
                Plugin.Logger.LogInfo("Skipped adding mark");
                return;
            }
            GameObject checkBadgeObject = new GameObject("CheckMarkBadge");
            Image checkBadge = checkBadgeObject.AddComponent<Image>();
            checkBadge.sprite = getCheckMarkBadge();
            if (checkBadge.sprite == null)
            {
                return;
            }
            checkBadge.transform.SetParent(img, false);
            checkBadge.raycastTarget = false;
            if (fillFull)
            {
                checkBadge.rectTransform.anchorMin = new Vector2(0f, 0f);
                checkBadge.rectTransform.anchorMax = new Vector2(1f, 1f);
            }
            else
            {
                checkBadge.rectTransform.anchorMin = new Vector2(0.80f, 0f);
                checkBadge.rectTransform.anchorMax = new Vector2(1f, 0.20f);
            }
            checkBadge.rectTransform.sizeDelta = Vector2.zero;
            checkBadge.preserveAspect = true;
            checkBadge.rectTransform.anchoredPosition = Vector2.zero;
            Plugin.Logger.LogInfo("Added mark");

        }
        public static void AddMarkTo3DObject(Transform bolt)
        {
            if (bolt.childCount > 5)
            {
                Plugin.Logger.LogInfo("Skipped adding mark");
                return;
            }
            GameObject checkBadgeObject = new GameObject("CheckMarkBadge");
            Canvas checkBadgeCanvas = checkBadgeObject.AddComponent<Canvas>();
            checkBadgeCanvas.transform.SetParent(bolt, false);
            checkBadgeCanvas.renderMode = RenderMode.WorldSpace;

            RectTransform rect = (RectTransform)checkBadgeObject.transform;

            rect.sizeDelta = new Vector2(100f, 100f);           
            rect.localScale = Vector3.one * 0.002f;              
            rect.localPosition = new Vector3(0f, .1f, 0f);
            rect.localRotation = Quaternion.Euler(0f, 180f, 0f);
            AddMark(rect, true);

            Plugin.Logger.LogInfo("Added mark");

        }

        public static Sprite checkMarkBadge;
        public static Sprite getCheckMarkBadge()
        {
            if (checkMarkBadge != null) return checkMarkBadge;
            else if (!File.Exists(Path.Combine(Paths.PluginPath, $"check-mark.png")))
            {
                Plugin.Logger.LogWarning("Checkmark file doesnt exist");
                return null;
            }
            byte[] file = File.ReadAllBytes(Path.Combine(Paths.PluginPath, $"check-mark.png"));
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(file);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(1f, 0f));
            checkMarkBadge = sprite;

            return sprite;

        }

    }
}
