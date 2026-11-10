using HarmonyLib;
using System.IO;


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
    public static class OverrideCheckSteamAchievements
    {
        public static Dress finishedDress;

        [HarmonyPostfix]
        public static void CheckDress(Dress __instance)
        {

            finishedDress = __instance;
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
    [HarmonyPatch(typeof(PlayerProgress), "RecordCompletedQuest")]
    public class CompleteQuest
    {
        [HarmonyPostfix]
        public static void CompleteQuestCheck(PlayerProgress __instance, QuestState qs)
        {
            LocationChecks.completedQuest = qs;
            LocationChecks.locationNeedsChecked = true;
        }
    }
}
