using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using BepInEx;
using BepInEx.Logging;
using Framework;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace DressmakerArchipelago;
[BepInDependency("yuu.dressmaker.mods")]

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static ArchipelagoSession session;
    internal static UnlockItems unlocker = new UnlockItems();
    internal static int saveSlot;
    internal static string seed;
    internal static bool isCorrectServer = true;
    internal static PlayerJson playerJson;
    private bool isConnected = false;


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
    private void Update()
    {
        if (!UnlockItems.isLoaded) return;

        if (SceneManager.GetActiveScene().name.Equals("TitleScreen"))
        {
            UnlockItems.isLoaded = false;
            Plugin.saveSlot = -1;
            isCorrectServer = true;
            return;
        }
        if (!isCorrectServer) return;

        if (!isConnected) Login();

        UnlockItems.UnlockQueue();
        LocationChecks.DressLocationCheck();


    }

    private async void Login()
    {
        session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);

        session.Items.ItemReceived += unlocker.ItemReceived;
        LoginResult login = session.TryConnectAndLogin("Dressmaker", "Coco", ItemsHandlingFlags.AllItems);
        if (login.Successful)
        {
            if (playerJson.seed != session.RoomState.Seed && !playerJson.seed.IsNullOrEmpty())
            {
                SingletonBehaviour<NotificationHandler>.Instance.ShowNotification("Wrong archipelago server. Archipelago progress will not be saved.");
                Logger.LogError("playerJson.seed does not match the server's seed");
                isCorrectServer = false;
                return;
            }
            playerJson.seed = session.RoomState.Seed;
            playerJson.SaveFile();
            Logger.LogInfo("Connection Success!");
            SingletonBehaviour<NotificationHandler>.Instance.ShowNotification("Connected to archipelago");


            isConnected = true;
            foreach (long id in Plugin.session.Locations.AllLocations)
            {
                string itemName = "";

                string fullName = Plugin.session.Locations.GetLocationNameFromId(id, "Dressmaker");
                if (fullName.Contains("/"))
                {
                    itemName = fullName.Substring(fullName.LastIndexOf('/') + 1);
                }
                else
                {
                    itemName = fullName;
                }
                LocationChecks.allLocations[itemName] = id;
            }
            List<long> locations = new List<long>();
            foreach (string locationCheck in playerJson.allLocationsReached)
            {
                if (LocationChecks.allLocations.TryGetValue(locationCheck, out long locationID))
                {
                    locations.Add(locationID);
                }
                Plugin.Logger.LogInfo("Added check for " + locationCheck);
            }
            if (locations.Count > 0)
            {
                await Plugin.session.Locations.CompleteLocationChecksAsync(locations.ToArray());
            }
            Plugin.Logger.LogInfo(LocationChecks.allLocations.Count + " Locations");
        }
        else
        {
            Logger.LogError("Failed, loser");
            isConnected = false;
        }
    }

    private void OnGUI()
    {
        /*        if (GUI.Button(new Rect(10, 100, 150, 30), "Connect to server"))
                {

                }*/

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
    internal static Dictionary<string, float> getTags(Fabric fabric)
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



