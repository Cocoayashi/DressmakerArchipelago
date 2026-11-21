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
        internal static string progressString = string.Empty;
        internal static QuestState completedQuest = null;
        internal static int goalPercentage = 75;
        
        internal static int goalChoice = -1;
        internal static int prestigeLevel;

        internal async static void DressLocationCheck()
        {
            if (!locationNeedsChecked) return;
            locationNeedsChecked = false;

            await CheckQuest();

            await CheckPatterns();
            await CheckFabrics();
            await CheckCompletion();
            await PrestigeLocationCheck();

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
            int.TryParse(goalChoiceObject.ToString(), out goalChoice);
            switch (goalChoice)
            {

                case PATTERN_GOAL:
                    CheckCollectionGoal("Use pattern: ", GarmentComponent.AllGarmentComponents.Select(x => x.name), "patterns");
                    break;
                case FABRIC_GOAL:
                    CheckCollectionGoal("Use fabric: ", Fabric.AllFabrics.Select(x => x.name), "fabrics");
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
        private static void CheckCollectionGoal(string locationPrefix, IEnumerable<string> gameNames, string goalType)
        {
            HashSet<string> names = new HashSet<string>(gameNames);
            int current = Plugin.playerJson.allLocationsReached.Count(names.Contains);
            int total = Plugin.session.Locations.AllLocations.Count(id =>
                Plugin.session.Locations.GetLocationNameFromId(id, "Dressmaker")?.StartsWith(locationPrefix) == true);
            if (total == 0) return;
            int percent = current * 100 / total;

            progressString =
                    $"{current} / {total} of {goalType} {percent}%";

            int needed = total * goalPercentage / 100;
            if (current >= needed)
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

        internal static async Task PrestigeLocationCheck()
        {
            List<long> locations = new List<long>();
            string locationName = "";
            for (int i = 1; i <= prestigeLevel; i++)
            {
                locationName = "Reach reputation level: " + i;
                RememberCheck(locationName);
                if (LocationChecks.allLocations.TryGetValue(locationName, out long locationID))
                {
                    locations.Add(locationID);
                }
            }
            var array = locations.ToArray();
            try
            {
                await Plugin.session.Locations.CompleteLocationChecksAsync(array);
            }
            catch (System.Exception)
            {

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
            List<PanelState> panelStates = new List<PanelState>();
            try
            {

                DressState dress = OverrideCheckSteamAchievements.finishedDress.state;

                panelStates = dress.panelStates;
            }
            catch (System.Exception ex)
            {
                return;
            }

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
            long[] patternLocations = [0, 0, 0, 0];

            try
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
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError(ex.Message);
            }


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
