using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using BepInEx;
using BepInEx.Logging;
using Framework;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
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
    internal static bool openingConnection;
    private bool isConnected = false;
    internal float timeSinceLastConnectionAttempt = -999f;



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
        if (SceneManager.GetActiveScene().name.Equals("TitleScreen"))
        {
            if (isConnected)
            {
                session.Socket.DisconnectAsync();
                isConnected = false;
            }
            UnlockItems.isLoaded = false;
            Plugin.saveSlot = -1;
            isCorrectServer = true;
            return;
        }

        // This might be useless since title screen already does the return, but it doesn't hurt to
        // keep it here just in case. isLoaded is set to true when the FrontDesk is the current scene.
        if (!UnlockItems.isLoaded) return;

        // Resets on title screen if this happens
        if (!isCorrectServer) return;

        if (!isConnected) Login();

        // as long as all those other ifs don't pass, these will be called every frame.

        UnlockItems.UnlockQueue();
        LocationChecks.DressLocationCheck();

    }

    private async void Login()
    {
        var timeDif = Time.realtimeSinceStartup - timeSinceLastConnectionAttempt;
        if (timeDif < 2f || openingConnection == true)
        {
            return;
        }
        timeSinceLastConnectionAttempt = Time.realtimeSinceStartup;
        openingConnection = true;

        session = ArchipelagoSessionFactory.CreateSession("localhost", 38281);
        UnlockItems.EmptyList();
        LoginResult login;
        try
        {
            await session.ConnectAsync();
            login = await session.LoginAsync("Dressmaker", "Coco", ItemsHandlingFlags.AllItems);

        }
        catch (System.Exception ex)
        {
            Plugin.Logger.LogError(ex.Message);
            SingletonBehaviour<NotificationHandler>.Instance.ShowNotification("Could not connect to archipelago");

            Logger.LogError("Connection failed, loser");
            isConnected = false;
            openingConnection = false;

            return;
        }
        finally
        {
            openingConnection = false;
        }
        // Bad log in
        if (!login.Successful)
        {
            SingletonBehaviour<NotificationHandler>.Instance.ShowNotification("Could not connect to archipelago");

            Logger.LogError("Connection failed, loser");
            isConnected = false;
            openingConnection = false;

            return;
        }
        //Good login
        session.Items.ItemReceived += unlocker.ItemReceived;
        session.Socket.SocketClosed += closedConnection;
        // Bad seed, currently means it doesnt try reconnecting until you go to title screen and open another save
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

    private void closedConnection(string reason)
    {
        SingletonBehaviour<NotificationHandler>.Instance.ShowNotification("Connection lost.");
        isConnected = false;
    }

    /* private void OnGUI()
{
            if (GUI.Button(new Rect(10, 100, 150, 30), "Connect to server"))
            {

            }

}
    */

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



