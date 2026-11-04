using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using System.Linq;
using System.Reflection;
namespace DressmakerArchipelago
{
    [HarmonyPatch(typeof(PlayerProgress), "ApplyStartingUnlocks")]
    public class StarterPatternUnlocks
    {
        [HarmonyPrefix]
        public static bool OverrideApplyStartingUnlocks()
        {
            Plugin.Logger.LogInfo("Skipped Apply Starting!");

            return false;
        }
    }
    public class UnlockItems
    {


        public void ItemReceived(ReceivedItemsHelper helper)
        {
            ItemInfo item = helper.DequeueItem();
            if (item == null) return;
            try
            {

                if (item.ItemName.StartsWith("Fabric: "))
                {
                    string internalName = item.ItemName.Substring(item.ItemName.LastIndexOf('/') + 1);
                    Fabric fabricToUnlock = Fabric.AllFabrics.FirstOrDefault(x => x.name.Equals(internalName));
                    if (fabricToUnlock == null)
                    {
                        Plugin.Logger.LogWarning("Fabric insert error: " + internalName);
                        return;
                    }
                    if (PlayerProgress.Current.unlockedFabrics.Contains(fabricToUnlock))
                    {
                        Plugin.Logger.LogWarning("Fabric inserted already: " + internalName);

                        return;
                    }

                    PlayerProgress.Current.unlockedFabrics.Add(fabricToUnlock);
                    Plugin.Logger.LogInfo(internalName + " unlocked");
                }
                if (item.ItemName.StartsWith("Pattern: "))
                {
                    string internalName = item.ItemName.Substring(item.ItemName.LastIndexOf('/') + 1);
                    GarmentComponent patternToUnlock = GarmentComponent.AllGarmentComponents.FirstOrDefault(x => x.name.Equals(internalName));
                    if (patternToUnlock == null)
                    {
                        Plugin.Logger.LogWarning("Pattern insert error: " + internalName);
                        return;
                    }
                    if (PlayerProgress.Current.unlockedPatterns.Contains(patternToUnlock))
                    {
                        Plugin.Logger.LogWarning("Pattern inserted already: " + internalName);

                        return;
                    }
                    PlayerProgress.Current.unlockedPatterns.Add(patternToUnlock);
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogInfo(ex.Message);
            }
            /*            if (item.ItemName.Equals("Gold"))
                        {
                            SingletonBehaviour<GameManager>.Instance.AddGold(50);
                        }*/
        }
    }
    [HarmonyPatch(typeof(PlayerProgress), "ApplyUnlocksForPrestigeLevel")]

    public class UnlockFabrics
    {
        [HarmonyPrefix]
        public static bool OverridePrestigeUnlocks(PlayerProgress __instance, int level)
        {
            level = 50;
            Plugin.Logger.LogInfo("Skipped Prestige Unlock " + level);
            AccessoryDefinition[] allAccessoryDefinitions = AccessoryDefinition.AllAccessoryDefinitions;
            foreach (AccessoryDefinition accessoryDefinition in allAccessoryDefinitions)
            {
                if (accessoryDefinition.prestigeUnlockLevel >= 0 && accessoryDefinition.prestigeUnlockLevel <= level && !__instance.unlockedAccessories.Contains(accessoryDefinition))
                {
                    __instance.unlockedAccessories.Add(accessoryDefinition);
                    if (accessoryDefinition.prestigeUnlockLevel > 0)
                    {
                        __instance.newAccessories.Add(accessoryDefinition);
                    }
                }
            }
            return false;
        }


    }
    [HarmonyPatch]

    public class StopUnlockMods
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("DressmakerMods.FabricAvailability"), "RestoreEarnedUnlocks");
        }
        [HarmonyPrefix]
        public static bool StopModdedUnlocks()
        {
            AccessTools.TypeByName("");
            return false;
        }
    }

        [HarmonyPatch(typeof(PlayerProgress), "ApplyPatternsForRelationshipLevel")]

    public class UnlockRelationships
    {
        [HarmonyPrefix]
        public static bool OverrideRelationshipUnlocks(PlayerProgress __instance, int level)
        {
            Plugin.Logger.LogInfo("Skipped Relationship unlocks!");

            return false;
        }
    }

}
