using Framework;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;


namespace DressmakerArchipelago
{
    [HarmonyPatch(typeof(GameManager), "SetScene")]
    public class HarmonyClass
    {
        [HarmonyPostfix]
        public static void LogArea(GameManager.Scene newScene)
        {
            if (!UnlockItems.isLoaded && newScene == GameManager.Scene.FrontDesk)
            {
                UnlockItems.isLoaded = true;
                Plugin.saveSlot = PlayerProgress.CurrentSlot;
                Plugin.playerJson = PlayerJson.Loadfile();
            }
        }
    }
    [HarmonyPatch(typeof(Dress), "CheckAchievements")]
    public class OverrideCheckSteamAchievements
    {
        [HarmonyPostfix]
        public static void CheckDress()
        {
            LocationChecks.locationNeedsChecked = true;
        }
    }
    [HarmonyPatch(typeof(PlayerProgress), "DeleteSlot")]
    public class DeleteSlot
    {
        [HarmonyPostfix]
        public static void DeleteSaveFile(int slot)
        {
            Plugin.saveSlot = slot;
            File.Delete(PlayerJson.filepath);
        }
    }
}
