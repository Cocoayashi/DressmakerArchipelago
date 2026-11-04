using BepInEx;
using BepInEx.Logging;
using Framework;
using HarmonyLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;

namespace DressmakerArchipelago
{
    internal class GetDump
    {
        internal static void GetDressmakerDump()
        {
            int count = QuestDefinition.AllQuestDefinitions.Count();
            Plugin.Logger.LogInfo(count + " Quests");
            count = SingletonBehaviour<GameManager>.Instance.ActiveCampaign.quests.Count();
            Plugin.Logger.LogInfo(count + " Story Quests");
            count = SingletonBehaviour<GameManager>.Instance.ActiveCampaign.sideQuests.Count();
            List<string> characters = new List<string>();
            DressmakerDump dump = new DressmakerDump();
            List<StoryQuestInfo> mainStory = new List<StoryQuestInfo>();
            foreach (QuestDefinition storyQuest in SingletonBehaviour<GameManager>.Instance.ActiveCampaign.quests)
            {
                StoryQuestInfo storyQuestInfo = new StoryQuestInfo();
                storyQuestInfo.questGiverCharacterName = storyQuest.questGiverCharacter.GetName();
                if (!characters.Contains(storyQuestInfo.questGiverCharacterName))
                {
                    characters.Add(storyQuestInfo.questGiverCharacterName);
                }
                storyQuestInfo.name = storyQuest.name.Trim();
                storyQuestInfo.prettyName = storyQuest.PrettyName.Trim();
                mainStory.Add(storyQuestInfo);
            }
            dump.characterNames = characters;
            FinalQuestInfo finalQuestInfo = new FinalQuestInfo();
            finalQuestInfo.name = QuestDefinition.FinalQuest.name.Trim();
            finalQuestInfo.prettyName = QuestDefinition.FinalQuest.PrettyName.Trim();
            finalQuestInfo.questGiverCharacterName = QuestDefinition.FinalQuest.questGiverCharacter.GetName().Trim();

            dump.finalQuestInfo = finalQuestInfo;
            dump.storyQuestInfos = mainStory;

            List<SideQuestInfo> pigeonStories = new List<SideQuestInfo>();
            foreach (QuestDefinition pigeonQuest in SingletonBehaviour<GameManager>.Instance.ActiveCampaign.sideQuests)
            {
                SideQuestInfo sideQuestInfo = new SideQuestInfo();
                sideQuestInfo.questGiverCharacterName = pigeonQuest.questGiverCharacter.GetName();
                sideQuestInfo.name = pigeonQuest.name.Trim();
                sideQuestInfo.prettyName = pigeonQuest.PrettyName.Trim();
                pigeonStories.Add(sideQuestInfo);
            }
            dump.sideQuestInfos = pigeonStories;

            List<FabricInfo> allFabrics = new List<FabricInfo>();
            foreach (Fabric fabric in PlayerProgress.Current.unlockedFabrics)
            {

                FabricInfo fabricInfo = new FabricInfo();
                if (fabric.name.Contains("Missing"))
                {
                    continue;
                }
                var _addTagDict = Plugin.getTags(fabric);
                fabricInfo.name = fabric.name.Trim();
                fabricInfo.prettyName = fabric.PrettyName.Trim();
                fabricInfo.itemTags = _addTagDict;
                allFabrics.Add(fabricInfo);
            }
            dump.fabricInfos = allFabrics;


            List<PatternInfo> list = new List<PatternInfo>();

            foreach (GarmentComponent garment in PlayerProgress.Current.unlockedPatterns)
            {
                if (garment.name.Contains("Missing"))
                {
                    continue;
                }

                Dictionary<string, float> _addTagDict = Plugin.getTags(garment);
                list.Add(new PatternInfo
                {

                    name = garment.name.Trim(),
                    prettyName = garment.PrettyName.Trim(),
                    PatternType = garment.type.ToString().Trim(),
                    itemTags = _addTagDict,
                });
            }
            dump.patternInfos = list;
            string json = JsonConvert.SerializeObject(dump, Formatting.Indented);
            File.WriteAllText(Path.Combine(Paths.PluginPath, "dressmaker_dump.json"), json);
            Plugin.Logger.LogInfo("printed json successfully, check plugins folder");
        }

    }
}
