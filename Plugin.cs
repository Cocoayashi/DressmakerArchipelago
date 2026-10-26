using BepInEx;
using BepInEx.Logging;
using Framework;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using HarmonyLib;

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

            DressmakerDump dump = new DressmakerDump();
            List<StoryQuestInfo> mainStory = new List<StoryQuestInfo>();
            foreach (QuestDefinition storyQuest in SingletonBehaviour<GameManager>.Instance.ActiveCampaign.quests)
            {
                StoryQuestInfo storyQuestInfo = new StoryQuestInfo();
                storyQuestInfo.questGiverCharacterName = storyQuest.questGiverCharacter.GetName();
                storyQuestInfo.name = storyQuest.name;
                storyQuestInfo.prettyName = storyQuest.PrettyName;
                mainStory.Add(storyQuestInfo);
            }

            FinalQuestInfo finalQuestInfo = new FinalQuestInfo();
            finalQuestInfo.name = QuestDefinition.FinalQuest.name;
            finalQuestInfo.prettyName = QuestDefinition.FinalQuest.PrettyName;
            finalQuestInfo.questGiverCharacterName = QuestDefinition.FinalQuest.questGiverCharacter.GetName();

            dump.finalQuestInfo = finalQuestInfo;
            dump.storyQuestInfos = mainStory;

            List<SideQuestInfo> pigeonStories = new List<SideQuestInfo>();
            foreach (QuestDefinition pigeonQuest in SingletonBehaviour<GameManager>.Instance.ActiveCampaign.sideQuests)
            {
                SideQuestInfo sideQuestInfo = new SideQuestInfo();
                sideQuestInfo.questGiverCharacterName = pigeonQuest.questGiverCharacter.GetName();
                sideQuestInfo.name = pigeonQuest.name;
                sideQuestInfo.prettyName = pigeonQuest.PrettyName;
                pigeonStories.Add(sideQuestInfo);
            }
            dump.sideQuestInfos = pigeonStories;

            List<FabricInfo> allFabrics = new List<FabricInfo>();
            foreach (Fabric fabric in Fabric.AllFabrics)
            {

                FabricInfo fabricInfo = new FabricInfo();
                fabricInfo.name = fabric.name;
                fabricInfo.prettyName = fabric.PrettyName;
                allFabrics.Add(fabricInfo);
            }
            dump.fabricInfos = allFabrics;

            List<PatternInfo> patterns = new List<PatternInfo>();
            foreach (GarmentComponent pattern in GarmentComponent.AllGarmentComponents)
            {
                PatternInfo patternInfo = new PatternInfo();
                patternInfo.name = pattern.name;
                patternInfo.prettyName = pattern.PrettyName;
                patternInfo.PatternType = pattern.type.ToString();
                patterns.Add(patternInfo);
            }
            dump.patternInfos = patterns;

/*            ActiveDress dress = new ActiveDress();
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
        }

    

