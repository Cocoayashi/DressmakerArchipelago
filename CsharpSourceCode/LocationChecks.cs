using Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DressmakerArchipelago
{
    internal class LocationChecks
    {
        public const int PATTERN_GOAL = 0;
        public const int FABRIC_GOAL = 1;
        public const int MAIN_STORY_GOAL = 2;
        public const int ALL_COMMISSIONS_GOAL = 3;



        internal static bool locationNeedsChecked = false;
        internal static Dictionary<string, long> allLocations = new Dictionary<string, long>();
        internal static QuestState completedQuest = null;
        internal static int goalPercentage = 75;
        internal async static void DressLocationCheck()
        {
            if (!locationNeedsChecked) return;
            locationNeedsChecked = false;

            await CheckPatterns();
            await CheckFabrics();
            await CheckQuest();
            await CheckCompletion();
        }

        public static async Task CheckCompletion()
        {
            if (!Plugin.isConnected)
            {
                return;
            }
            Dictionary<string, object> dataStorage = await Plugin.session.DataStorage.GetSlotDataAsync();
            if (!dataStorage.TryGetValue("goal_choice", out object goalChoiceObject))
            {
                return;
            }
            if (!dataStorage.TryGetValue("goal_percentage", out object goalPercentageObject))
            {
                return;
            }
            
            int.TryParse(goalPercentageObject.ToString(), out goalPercentage);
            int.TryParse(goalChoiceObject.ToString(), out int goalChoice);
            switch (goalChoice)
            {

                case PATTERN_GOAL:
                    CheckPatternCompletion();
                    break;
                case FABRIC_GOAL:
                    CheckFabricCompletion();
                    break;
                case MAIN_STORY_GOAL:
                    CheckCompletedFinalQuest();
                    break;
                case ALL_COMMISSIONS_GOAL:
                    if (Plugin.playerJson.allLocationsReached.ToList().Where(x => x.Contains("Complete")).Count() >= 77)
                    {
                        SendVictoryCheck();
                    }
                    break;

            }
        }

        private static void CheckCompletedFinalQuest()
        {
            if (Plugin.playerJson.allLocationsReached.Contains("FinalQuest"))
            {
                SendVictoryCheck();
            }
        }

        private static void CheckFabricCompletion()
        {
            int fabricsChecked = 0;
            int fabricsTotal = 0;
            foreach (string location in Plugin.playerJson.allLocationsReached)
            {
                if (Fabric.AllFabrics.ToList().Select(x => x.name).Contains(location))
                {
                    fabricsChecked++;
                }
            }
            foreach (long locationID in Plugin.session.Locations.AllLocations)
            {
                string locationName = Plugin.session.Locations.GetLocationNameFromId(locationID);
                if (locationName.Contains("fabric"))
                {
                    fabricsTotal++;
                }
            }
            int patternsNeeded = fabricsTotal * goalPercentage / 100;
            if (fabricsChecked >= patternsNeeded)
            {
                SendVictoryCheck();
            }
        }

        private static void CheckPatternCompletion()
        {
            int patternsChecked = 0;
            int patternsTotal = 0;
            foreach (string location in Plugin.playerJson.allLocationsReached)
            {
                if (GarmentComponent.AllGarmentComponents.ToList().Select(x => x.name).Contains(location))
                {
                    patternsChecked++;
                }
            }
            foreach (long locationID in Plugin.session.Locations.AllLocations)
            {
                string locationName = Plugin.session.Locations.GetLocationNameFromId(locationID);
                if (locationName.Contains("pattern"))
                {
                    patternsTotal++;
                }
            }
            int patternsNeeded = patternsTotal * goalPercentage / 100;
            if (patternsChecked >= patternsNeeded)
            {
                SendVictoryCheck();
            }
        }

        private static void SendVictoryCheck()
        {
            try
            {
                Plugin.session.SetGoalAchieved();
            }
            catch (System.Exception)
            {

            }
        }

        private static async Task CheckQuest()
        {
            if (completedQuest == null)
            {
                return;
            }
            string locationName = completedQuest.questDefinition.name;
            RememberCheck(locationName);
            allLocations.TryGetValue(locationName, out long locationID);
            if (locationName.Equals("FinalQuest"))
            {
                allLocations.TryGetValue("Complete Final Commission", out locationID);
            }
            try
            {
                await Plugin.session.Locations.CompleteLocationChecksAsync(locationID);
            }
            catch (System.Exception)
            {

            }
            finally
            {
                completedQuest = null;
            }
        }

        internal static async Task PrestigeLocationCheck(int prestige)
        {

            string locationName = "Reach reputation level: " + prestige;
            RememberCheck(locationName);


            if (LocationChecks.allLocations.TryGetValue(locationName, out long locationID))
            {
                try
                {
                    await Plugin.session.Locations.CompleteLocationChecksAsync(locationID);
                }
                catch (System.Exception)
                {

                }
            }

        }

        internal async static Task RelationshipLocationCheck(string characterName, int characterLevel)
        {
            string locationName = $"Reach friendship: {characterLevel} with {characterName}";
            RememberCheck(locationName);

            if (LocationChecks.allLocations.TryGetValue(locationName, out long locationID))
            {
                try
                {
                    await Plugin.session.Locations.CompleteLocationChecksAsync(locationID);
                }
                catch (System.Exception)
                {
                }
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
                        try
                        {
                            await Plugin.session.Locations.CompleteLocationChecksAsync(locationID);
                        }
                        catch (System.Exception)
                        {
                        }
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
            if (dress.sleeveDefinition != null)
            {
                sleeveName = dress.sleeveDefinition.name.Trim();
            }
            string bodiceName = dress.bodiceDefinition.name.Trim();
            string skirtName = dress.skirtDefinition.name.Trim();
            long[] patternLocations = [0, 0, 0, 0];
            if (!collarName.IsNullOrEmpty())
            {
                allLocations.TryGetValue(collarName, out patternLocations[0]);
                RememberCheck(collarName);
            }
            if (!sleeveName.IsNullOrEmpty())
            {
                allLocations.TryGetValue(sleeveName, out patternLocations[1]);
                RememberCheck(sleeveName);
            }
            allLocations.TryGetValue(bodiceName, out patternLocations[2]);
            allLocations.TryGetValue(skirtName, out patternLocations[3]);
            RememberCheck(skirtName);
            RememberCheck(bodiceName);


            try
            {
                await Plugin.session.Locations.CompleteLocationChecksAsync(patternLocations);

            }
            catch (System.Exception)
            {
            }
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
