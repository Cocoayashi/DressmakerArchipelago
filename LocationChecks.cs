using Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.TextCore.Text;

namespace DressmakerArchipelago
{
    internal class LocationChecks
    {
        internal static bool locationNeedsChecked = false;
        internal static Dictionary<string, long> allLocations = new Dictionary<string, long>();

        internal async static void DressLocationCheck()
        {
            if (!locationNeedsChecked) return;
            locationNeedsChecked = false;

            await CheckPatterns();
            await CheckFabrics();

        }

        internal static async Task PrestigeLocationCheck(int prestige)
        {

            string locationName = "Reach reputation level: " + prestige;
            RememberCheck(locationName);


            if (LocationChecks.allLocations.TryGetValue(locationName, out long locationID))
            {
                await Plugin.session.Locations.CompleteLocationChecksAsync(locationID);
            }

        }

        internal async static Task RelationshipLocationCheck(string characterName, int characterLevel)
        {
            string locationName = $"Reach friendship: {characterLevel} with {characterName}";
            RememberCheck(locationName);

            if (LocationChecks.allLocations.TryGetValue(locationName, out long locationID))
            {
                await Plugin.session.Locations.CompleteLocationChecksAsync(locationID);
            }
        }

        private async static Task CheckFabrics()
        {

            DressState dress = OverrideCheckSteamAchievements.finishedDress.state;

            List<PanelState> panelStates = dress.panelStates;

            foreach (PanelState panelState in panelStates)
            {
                if (panelState.fabric != null)
                {
                    string fabricName = panelState.fabric.name.Trim();
                    RememberCheck(fabricName);

                    if (allLocations.TryGetValue(fabricName, out long locationID))
                    {
                        await Plugin.session.Locations.CompleteLocationChecksAsync(locationID);
                    }
                }
            }
        }

        private static async Task CheckPatterns()
        {
            DressState dress = OverrideCheckSteamAchievements.finishedDress.state;
            string collarName = "";
            string sleeveName = "";

            if (dress.collarDefinition != null)
            {
                collarName = dress.collarDefinition.name.Trim();
            }
            if(dress.sleeveDefinition != null)
            {
                sleeveName = dress.sleeveDefinition.name.Trim();
            }
            string bodiceName = dress.bodiceDefinition.name.Trim();
            string skirtName = dress.skirtDefinition.name.Trim();
            long[] patternLocations = [0, 0, 0, 0];
            if (!collarName.Equals("No Collar") && !collarName.Equals(""))
            {
                patternLocations[0] = allLocations[collarName];
                RememberCheck(collarName);

            }
            if (!sleeveName.Equals("No Sleeves") && !sleeveName.Equals(""))
            {
                patternLocations[1] = allLocations[sleeveName];
                RememberCheck(sleeveName);

            }
            patternLocations[2] = allLocations[bodiceName];
            patternLocations[3] = allLocations[skirtName];
            RememberCheck(skirtName);
            RememberCheck(bodiceName);


            await Plugin.session.Locations.CompleteLocationChecksAsync(patternLocations);
        }
        internal static void RememberCheck(string locationName)
        {
            bool isNew = Plugin.playerJson.allLocationsReached.Add(locationName);
            if (!isNew) return;
            Plugin.playerJson.SaveFile();
            Plugin.Logger.LogInfo("First time reached: " + locationName);

        }
    }
}
