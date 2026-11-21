using Archipelago.MultiClient.Net.MessageLog.Messages;
using Framework;
using System.Collections.Concurrent;

namespace DressmakerArchipelago
{
    internal static class MessageNotifications
    {
        private static readonly ConcurrentQueue<string> pending = new ConcurrentQueue<string>();

        internal static void OnMessageReceived(LogMessage message)
        {
            if (message is HintItemSendLogMessage) return;          
            if (message is not ItemSendLogMessage send) return;
            if (!send.IsSenderTheActivePlayer || send.IsReceiverTheActivePlayer) return;           

            pending.Enqueue($"Sent {send.Item.ItemDisplayName} to {send.Receiver.Name}");
        }

        internal static void ShowPending()
        {
            while (pending.TryDequeue(out string text))
                SingletonBehaviour<NotificationHandler>.Instance.ShowNotification(text);
        }
    }
}
