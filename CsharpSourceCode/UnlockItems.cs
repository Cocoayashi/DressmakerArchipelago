using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Framework;
using System.Collections.Concurrent;
using System.Linq;

namespace DressmakerArchipelago
{
    public class UnlockItems
    {
        private static ConcurrentQueue<string> itemsAddQueue = new ConcurrentQueue<string>();
        internal static bool isLoaded = false;

        public void ItemReceived(ReceivedItemsHelper helper)
        {
            while (helper.Any())
            {
                ItemInfo item = helper.DequeueItem();
                if (item == null) return;
                string itemName = item.ItemName;
                itemsAddQueue.Enqueue(itemName); 
            }

        }


        internal static void UnlockQueue()
        {
            while (itemsAddQueue.TryDequeue(out string itemName))
            {

                try
                {

                    if (itemName.StartsWith("Fabric: "))
                    {
                        TryAddFabric(itemName);
                    }
                    else if (itemName.StartsWith("Pattern: "))
                    {
                        TryAddPattern(itemName);
                    }
                    else if (itemName.Equals("Gold"))
                    {
                        TryAddGold();
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
            }
        }

        private static void TryAddGold()
        {

            int unlockedGoldCount = Plugin.session.Items.AllItemsReceived.Where(x => x.ItemName == "Gold").Count();
            if (unlockedGoldCount > Plugin.playerJson.goldCount)
            {
                Plugin.playerJson.goldCount++;
                Plugin.playerJson.SaveFile();
                SingletonBehaviour<GameManager>.Instance.AddGold(100);
                SingletonBehaviour<NotificationHandler>.Instance.ShowNotification("100 gold given");
                Plugin.Logger.LogInfo("100 Gold given");
                return;
            }
            Plugin.Logger.LogInfo("Gold skipped");
        }

        private static bool TryAddPattern(string itemName)
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
            SingletonBehaviour<NotificationHandler>.Instance.ShowNotification(internalName + " unlocked");

            Plugin.Logger.LogInfo(internalName + " unlocked");

            return true;
        }

        private static bool TryAddFabric(string itemName)
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
            SingletonBehaviour<NotificationHandler>.Instance.ShowNotification(internalName + " unlocked");
            Plugin.Logger.LogInfo(internalName + " unlocked");
            return true;
        }

    }
}
