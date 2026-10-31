using BepInEx;
using BepInEx.Logging;
using Framework;
using HarmonyLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DressmakerArchipelago;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    //NOTE: ApplyStartingUnlocks() Hijack that for beginning
    // CheckAvhievements() SingletonBehaviour<GameManager>.Instance.activeDress.DressState;
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 30), "Generate APWorld (Do after launching a save)"))
        {

            int count = QuestDefinition.AllQuestDefinitions.Count();
            Logger.LogInfo(count + " Quests");
            count = SingletonBehaviour<GameManager>.Instance.ActiveCampaign.quests.Count();
            Logger.LogInfo(count + " Story Quests");
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
                var _addTagDict = getTags(fabric);
                fabricInfo.name = fabric.name.Trim();
                fabricInfo.prettyName = fabric.PrettyName.Trim();
                fabricInfo.itemTags = _addTagDict;
                allFabrics.Add(fabricInfo);
            }
            dump.fabricInfos = allFabrics;


            List<PatternInfo> list = new List<PatternInfo>();

            /*           foreach (CharacterDefinition character in CharacterDefinition.AllCharacterDefinitions) 
                       {
                           for (int i = 0; i < 6; i++)
                           {
                               foreach (GarmentComponent garment in character.GetPatternsUnlockedAtLevel(i))
                               {
                                   var _addTagDict = getTags(garment);
                                   list.Add(new PatternInfo{
                                       name = garment.name,
                                       prettyName = garment.PrettyName,
                                       PatternType = garment.type.ToString(),    
                                       itemTags = _addTagDict,
                                       });
                               }
                           }
                       }*/
            foreach (GarmentComponent garment in PlayerProgress.Current.unlockedPatterns)
            {
                if (garment.name.Contains("Missing"))
                {
                    continue;
                }

                Dictionary<string, float> _addTagDict = getTags(garment);
                list.Add(new PatternInfo
                {

                    name = garment.name.Trim(),
                    prettyName = garment.PrettyName.Trim(),
                    PatternType = garment.type.ToString().Trim(),
                    itemTags = _addTagDict,
                });
            }
            dump.patternInfos = list;

            /*    For use when doing checks, not useful now        ActiveDress dress = new ActiveDress();
                        dress.collarName = SingletonBehaviour<GameManager>.Instance.activeDress.state.collarDefinition.GetName();
                        dress.skirtName = SingletonBehaviour<GameManager>.Instance.activeDress.state.skirtDefinition.GetName();
                        dress.bodiceName = SingletonBehaviour<GameManager>.Instance.activeDress.state.bodiceDefinition.GetName();
                        dress.sleeveName = SingletonBehaviour<GameManager>.Instance.activeDress.state.sleeveDefinition.GetName();
                        dump.activeDress = dress;*/

            string json = JsonConvert.SerializeObject(dump, Formatting.Indented);
            File.WriteAllText(Path.Combine(Paths.PluginPath, "dressmaker_dump.json"), json);
            Logger.LogInfo("printed json successfully, check plugins folder");
        }
    }

    private static Dictionary<string, float> getTags(GarmentComponent garment)
    {
        var _addTagDict = new Dictionary<string, float>();
        foreach (TagWeight additiveTag in garment.additiveTags)
        {
            if (additiveTag.weight != 0f)
            {
                _addTagDict.TryAdd(additiveTag.tag.GetName(), additiveTag.weight);
            }
        }

        return _addTagDict;
    }
    private static Dictionary<string, float> getTags(Fabric fabric)
    {
        var _addTagDict = new Dictionary<string, float>();
        foreach (TagWeight additiveTag in fabric.tagWeights)
        {
            if (additiveTag.weight != 0f)
            {
                _addTagDict.TryAdd(additiveTag.tag.GetName(), additiveTag.weight);
            }
        }

        return _addTagDict;
    }
}



