using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Framework;
using HarmonyLib;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace DressmakerArchipelago
{
    public class UnlockItems
    {
        private static ConcurrentQueue<ItemInfo> itemsAddQueue = new ConcurrentQueue<ItemInfo>();
        internal static bool isLoaded = false;

        public void ItemReceived(ReceivedItemsHelper helper)
        {
            while (helper.Any())
            {
                ItemInfo item = helper.DequeueItem();
                if (item == null) return;
                itemsAddQueue.Enqueue(item); 
            }

        }


        internal static void UnlockQueue()
        {
            while (itemsAddQueue.TryDequeue(out ItemInfo item))
            {
                string itemName = item.ItemName;
                string source = DescribeSource(item);
                try
                {

                    if (itemName.StartsWith("Fabric: "))
                    {
                        TryAddFabric(itemName, source);
                    }
                    else if (itemName.StartsWith("Pattern: "))
                    {
                        TryAddPattern(itemName, source);
                    }
                    else if (itemName.Equals("Gold"))
                    {
                        TryAddGold(source);
                    }
                    else
                    {
                        Plugin.Logger.LogError(itemName + ": item not found");
                    }
                }
                catch (System.Exception ex)
                {
                    Plugin.Logger.LogError(ex.Message);
                }
                finally
                {
                    MethodInfo method = AccessTools.Method(typeof(Sketchbook), "RecreateTexture");
                    method.Invoke(SingletonBehaviour<Sketchbook>.Instance, new object[] { true });
                    SingletonBehaviour<Sketchbook>.Instance.UpdateDisplay();
                    
                }
            }
        }

        private static string DescribeSource(ItemInfo item)
        {
            if (item.Player == null) return "";
            if (item.Player.Slot == 0) return " (starting item)";      // slot 0 is the server: start inventory, !getitem
            if (item.Player.Slot == Plugin.session.ConnectionInfo.Slot) return " (self)";
            return " (from " + item.Player.Name + ")";
        }

        private static void TryAddGold(string source)
        {

            int unlockedGoldCount = Plugin.session.Items.AllItemsReceived.Where(x => x.ItemName == "Gold").Count();
            if (unlockedGoldCount > Plugin.playerJson.goldCount)
            {
                Plugin.playerJson.goldCount++;
                Plugin.playerJson.SaveFile();
                SingletonBehaviour<GameManager>.Instance.AddGold(50);
                SingletonBehaviour<NotificationHandler>.Instance.ShowNotification("50 gold given" + source);
                Plugin.Logger.LogInfo("50 Gold given");
                return;
            }
            Plugin.Logger.LogInfo("Gold skipped");
        }

        private static bool TryAddPattern(string itemName, string source)
        {
            string internalName = itemName.Substring(itemName.LastIndexOf('/') + 1);
            GarmentComponent patternToUnlock = GarmentComponent.AllGarmentComponents.FirstOrDefault(x => x.name.Equals(internalName));
            if (patternToUnlock == null)
            {
                Plugin.Logger.LogError("Pattern insert error: " + internalName);
                return false;
            }
            if (PlayerProgress.Current.unlockedPatterns.Contains(patternToUnlock))
            {
                Plugin.Logger.LogWarning("Pattern inserted already: " + internalName);

                return false;
            }
            PlayerProgress.Current.unlockedPatterns.Add(patternToUnlock);
            SingletonBehaviour<NotificationHandler>.Instance.ShowNotification(internalName + " unlocked" + source);

            Plugin.Logger.LogInfo(internalName + " unlocked");

            return true;
        }

        private static bool TryAddFabric(string itemName, string source)
        {
            string internalName = itemName.Substring(itemName.LastIndexOf('/') + 1);
            Fabric fabricToUnlock = Fabric.AllFabrics.FirstOrDefault(x => x.name.Equals(internalName));
            if (fabricToUnlock == null)
            {
                Plugin.Logger.LogError("Fabric insert error: " + internalName);
                return false;
            }
            if (PlayerProgress.Current.unlockedFabrics.Contains(fabricToUnlock))
            {
                Plugin.Logger.LogWarning("Fabric inserted already: " + internalName);

                return false;
            }

            PlayerProgress.Current.unlockedFabrics.Add(fabricToUnlock);
            SingletonBehaviour<NotificationHandler>.Instance.ShowNotification(internalName + " unlocked" + source);
            Plugin.Logger.LogInfo(internalName + " unlocked");
            return true;
        }

    }
}
