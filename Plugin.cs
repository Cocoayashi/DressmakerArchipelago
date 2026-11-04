using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace DressmakerArchipelago;
[BepInDependency("yuu.dressmaker.mods")]

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static ArchipelagoSession session;
    internal static UnlockItems unlocker = new UnlockItems();


    //NOTE: ApplyStartingUnlocks() Hijack that for beginning
    // CheckAvhievements() SingletonBehaviour<GameManager>.Instance.activeDress.DressState;
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 100, 150, 30), "Connect to server"))
        {
            session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);
            session.Items.ItemReceived += unlocker.ItemReceived;

            LoginResult login = session.TryConnectAndLogin("Dressmaker", "Coco", ItemsHandlingFlags.AllItems);
            if (login.Successful)
            {
                Logger.LogInfo("Connection Success!");

            }
            else
            {
                Logger.LogInfo("Failed, loser");
            }
        }

        //if (GUI.Button(new Rect(10, 10, 150, 30), "JSON dump"))
        //{
        //    GetDump.GetDressmakerDump();
        //}

        /*       For use when doing checks, not useful now ActiveDress dress = new ActiveDress();
               dress.collarName = SingletonBehaviour<GameManager>.Instance.activeDress.state.collarDefinition.GetName();
               dress.skirtName = SingletonBehaviour<GameManager>.Instance.activeDress.state.skirtDefinition.GetName();
               dress.bodiceName = SingletonBehaviour<GameManager>.Instance.activeDress.state.bodiceDefinition.GetName();
               dress.sleeveName = SingletonBehaviour<GameManager>.Instance.activeDress.state.sleeveDefinition.GetName();
               dump.activeDress = dress;
       */

    }


    internal static Dictionary<string, float> getTags(GarmentComponent garment)
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



