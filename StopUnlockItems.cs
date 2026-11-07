using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace DressmakerArchipelago
{

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
            Plugin.Logger.LogInfo("Skipped Modded Unlocks");

            return false;
        }
    }
    
    [HarmonyPatch(typeof(PlayerProgress), "ApplyUnlocksForPrestigeLevel")]
    public class StopUnlockFabrics
    {
        [HarmonyPrefix]
        public static bool OverridePrestigeUnlocks(PlayerProgress __instance, int level)
        {
            if (UnlockItems.isLoaded && level > 0)
            {
                LocationChecks.PrestigeLocationCheck(level);

            }

            int accessoryLevel = 50;
            Plugin.Logger.LogInfo("Prestige Unlock " + accessoryLevel+ " accessories");
            AccessoryDefinition[] allAccessoryDefinitions = AccessoryDefinition.AllAccessoryDefinitions;
            foreach (AccessoryDefinition accessoryDefinition in allAccessoryDefinitions)
            {
                if (accessoryDefinition.prestigeUnlockLevel >= 0 && accessoryDefinition.prestigeUnlockLevel <= accessoryLevel && !__instance.unlockedAccessories.Contains(accessoryDefinition))
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

    [HarmonyPatch(typeof(PlayerProgress), "ApplyPatternsForRelationshipLevel")]
    public class StopUnlockRelationships
    {
        [HarmonyPrefix]
        public static bool OverrideRelationshipUnlocks(CharacterDefinition character, int level)
        {
            if (UnlockItems.isLoaded)
            {
                LocationChecks.RelationshipLocationCheck(character.GetName(), level);
            }
            Plugin.Logger.LogInfo("Skipped Relationship unlocks!");

            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerProgress), "ApplyStartingUnlocks")]
    public class StopStarterPatternUnlocks
    {
        [HarmonyPrefix]
        public static bool OverrideApplyStartingUnlocks()
        {
            Plugin.Logger.LogInfo("Skipped Apply Starting!");

            return false;
        }
    }
}
