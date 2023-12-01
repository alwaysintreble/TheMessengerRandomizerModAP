using System;
using System.Collections;
using System.Linq;
using System.Threading;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using MessengerRando.Utils;
using Mod.Courier.UI;
using UnityEngine;
using static Mod.Courier.UI.TextEntryButtonInfo;

namespace MessengerRando.Archipelago
{
    public static class ArchipelagoClient
    {
        private const string ApVersion = "0.4.2";
        public static ArchipelagoData ServerData = new ArchipelagoData();

        private delegate void OnConnectAttempt(LoginResult result);
        public static bool Authenticated;
        public static bool HasConnected;
        private static bool attemptingConnection;

        public static bool DisplayAPMessages = true;
        public static bool FilterAPMessages = true;
        public static bool DisplayStatus = true;
        public static bool HintPopUps = true;

        public static ArchipelagoSession Session;
        public static DeathLinkInterface DeathLinkHandler;

        private static readonly Queue ItemQueue = new Queue();
        private static readonly Queue DialogQueue = new Queue();
        private static readonly Queue MessageQueue = new Queue();

        public static void ConnectAsync()
        {
            if (attemptingConnection || Authenticated) return;
            attemptingConnection = true;
            Debug.Log($"Connecting to {ServerData.Uri}:{ServerData.Port} as {ServerData.SlotName}");
            ThreadPool.QueueUserWorkItem(_ => Connect(OnConnected));
        }

        public static void ConnectAsync(SubMenuButtonInfo connectButton)
        {
            if (attemptingConnection || Authenticated) return;
            attemptingConnection = true;
            if (ServerData == null)
                ServerData = new ArchipelagoData();
            Debug.Log($"Connecting to {ServerData.Uri}:{ServerData.Port} as {ServerData.SlotName}");
            Connect(result => OnConnected(result, connectButton));
        }

        private static void OnConnected(LoginResult connectStats)
        {
            if (ServerData.CheckedLocations != null)
            {
                Session.Locations.CompleteLocationChecksAsync(
                    _ => ServerData.CheckedLocations = Session.Locations.AllLocationsChecked.ToList(),
                    ServerData.CheckedLocations.ToArray());
            }
            attemptingConnection = false;
        }

        private static void OnConnected(LoginResult connectResult, SubMenuButtonInfo connectButton)
        {
            TextEntryPopup successPopup = InitTextEntryPopup(connectButton.addedTo, string.Empty,
                entry => true, 0, null, CharsetFlags.Space);

            string outputText;
            if (connectResult.Successful)
                outputText = $"Successfully connected to {ServerData.Uri}:{ServerData.Port} as {ServerData.SlotName}!";
            else
            {
                outputText = $"Failed to connect to {ServerData.Uri}:{ServerData.Port} as {ServerData.SlotName}\n";
                foreach (var error in ((LoginFailure)connectResult).Errors)
                    outputText += error;
            }
            
            successPopup.Init(outputText);
            successPopup.gameObject.SetActive(true);
            // Object.Destroy(successPopup.transform.Find("BigFrame").Find("SymbolsGrid").gameObject);
            Console.WriteLine(outputText);
            attemptingConnection = false;
        }

        private static ArchipelagoSession CreateSession()
        {
            var session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri, ServerData.Port);
            session.MessageLog.OnMessageReceived += OnMessageReceived;
            session.Items.ItemReceived += OnItemReceived;
            session.Socket.ErrorReceived += SessionErrorReceived;
            session.Socket.SocketClosed += SessionSocketClosed;
            return session;
        }

        private static void Connect(OnConnectAttempt attempt)
        {
            if (Authenticated) return;
            if (ItemsAndLocationsHandler.ItemsLookup == null) ItemsAndLocationsHandler.Initialize();
            var needSlotData = ServerData.SlotData == null;

            LoginResult result;

            try
            {
                Session = CreateSession();
                result = Session.TryConnectAndLogin(
                    "The Messenger",
                    ServerData.SlotName,
                    ItemsHandlingFlags.AllItems,
                    new Version(ApVersion),
                    password: ServerData.Password,
                    requestSlotData: needSlotData
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
                result = new LoginFailure(e.GetBaseException().Message);
            }
            

            if (result.Successful)
            {
                var success = (LoginSuccessful)result;
                if (needSlotData)
                    ServerData.SlotData = success.SlotData;
                ServerData.SeedName = Session.RoomState.Seed;
                Authenticated = true;

                RandomizerStateManager.InitializeMultiSeed();
                
                DeathLinkHandler = new DeathLinkInterface();
                
                ThreadPool.QueueUserWorkItem(o =>
                    Session.Locations.CompleteLocationChecksAsync(null,
                        ServerData.CheckedLocations.ToArray()));

                HasConnected = true;
            }
            else
            {
                LoginFailure failure = (LoginFailure)result;
                string errorMessage = $"Failed to connect to {ServerData.Uri} as {ServerData.SlotName}:";
                errorMessage +=
                    failure.Errors.Aggregate(errorMessage, (current, error) => current + $"\n    {error}");

                Console.WriteLine($"Failed to connect: {errorMessage}");

                Authenticated = false;
                Disconnect();
            }

            attempt(result);
        }

        private static string ColorizePlayerName(int player)
        {
            string color;
            color = player.Equals(Session.ConnectionInfo.Slot) ? UserConfig.PlayerColor : UserConfig.OtherPlayerColor;
            return $"<color=#{color}>{Session.Players.GetPlayerAlias(player)}</color>";
        }

        private static string ColorizeLocation(long locID)
        {
            string color = UserConfig.LocationColor;
            return $"<color=#{color}>{Session.Locations.GetLocationNameFromId(locID)}</color>";
        }

        private static string ConvertHintMessage(HintItemSendLogMessage hintMessage)
        {
            var colorizedMessage = hintMessage.ToString();
            colorizedMessage = colorizedMessage.Replace(hintMessage.Sender.Name,
                ColorizePlayerName(hintMessage.Sender.Slot));
            colorizedMessage = colorizedMessage.Replace(hintMessage.Receiver.Name,
                ColorizePlayerName(hintMessage.Receiver.Slot));
            colorizedMessage = colorizedMessage.Replace(
                Session.Locations.GetLocationNameFromId(hintMessage.Item.Location),
                ColorizeLocation(hintMessage.Item.Location));
            colorizedMessage = colorizedMessage.Replace(hintMessage.Item.ToString(),
                hintMessage.Item.ColorizeItem());
            Console.WriteLine(colorizedMessage);
            return colorizedMessage;
        }

        private static void OnMessageReceived(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            if (FilterAPMessages)
            {
                switch (message)
                {
                    case HintItemSendLogMessage hintMessage:
                        if (hintMessage.IsFound) break;
                        if (hintMessage.IsRelatedToActivePlayer && HintPopUps)
                        {
                            MessageQueue.Enqueue(hintMessage.ToString());
                            DialogQueue.Enqueue(ConvertHintMessage(hintMessage));
                        }
                        break;
                    case ItemSendLogMessage itemSendMessage:
                        if (itemSendMessage.IsRelatedToActivePlayer)
                        {
                            if (!itemSendMessage.IsReceiverTheActivePlayer && !ItemsAndLocationsHandler.HasDialog(itemSendMessage.Item.Location))
                            {
                                Console.WriteLine($"adding {itemSendMessage.Item.ToReadableString()} to dialog queue.");
                                DialogQueue.Enqueue(itemSendMessage.Item.ToReadableString());
                            }
                            MessageQueue.Enqueue(itemSendMessage.ToString());
                        }
                        break;
                }
            }
            else
            {
                if (HintPopUps && message is HintItemSendLogMessage hintMessage)
                    DialogQueue.Enqueue(ConvertHintMessage(hintMessage));
                MessageQueue.Enqueue(message.ToString());
            }
        }

        public static void SyncLocations()
        {
            if (RandomizerStateManager.Instance.CurrentFileSlot == 0) return;
            var checkedLocations = Session.Locations.AllLocationsChecked;
            if (ServerData.CheckedLocations.Count == checkedLocations.Count) return;
            foreach (var location in checkedLocations)
            {
                try
                {
                    if (ServerData.CheckedLocations.Contains(location)) continue;
                    var locName = Session.Locations.GetLocationNameFromId(location);
                    if (locName.Contains("Seal"))
                    {
                        var roomKey =
                            ItemsAndLocationsHandler.ArchipelagoLocations.Find(
                                loc => loc.PrettyLocationName.Equals(locName)).LocationName;
                        Manager<ProgressionManager>.Instance.SetChallengeRoomAsCompleted(roomKey);
                    }
                    else if (ItemsAndLocationsHandler.ShopLocation(location, out var shopLoc))
                    {
                        var shopID = (EShopUpgradeID)Enum.Parse(typeof(EShopUpgradeID), shopLoc.PrettyLocationName);
                        Manager<InventoryManager>.Instance.SetShopUpgradeAsUnlocked(shopID);
                    }

                    ServerData.CheckedLocations.Add(location);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine($"{Session.Locations.GetLocationNameFromId(location)}: {location}");
                }
            }
        }

        private static void OnItemReceived(ReceivedItemsHelper helper)
        {
            Console.WriteLine("ItemReceived called");
            if (helper.Index < ServerData.Index)
            {
                Console.WriteLine("Removing previously unlocked items from queue");
                helper.DequeueItem();
                return;
            }

            var itemToUnlock = helper.DequeueItem();
            if (RandomizerStateManager.Instance.InGame &&
                RandomizerStateManager.IsSafeTeleportState() &&
                !Manager<PauseManager>.Instance.IsPaused)
            {
                try
                {
                    ItemsAndLocationsHandler.Unlock(itemToUnlock.Item);
                }
                catch (Exception e)
                {
                    ItemQueue.Enqueue(itemToUnlock.Item);
                }
            }
            else
                ItemQueue.Enqueue(itemToUnlock.Item);
            ServerData.Index++;
            if (itemToUnlock.Player.Equals(Session.ConnectionInfo.Slot) &&
                ItemsAndLocationsHandler.HasDialog(itemToUnlock.Location))
                return;
            Console.WriteLine($"adding {itemToUnlock.ToReadableString()} to dialog queue");
            DialogQueue.Enqueue(itemToUnlock.ToReadableString());
        }

        private static void SessionErrorReceived(Exception e, string message)
        {
            Console.WriteLine(message);
            Console.WriteLine(e.GetBaseException().Message);
        }

        private static void SessionSocketClosed(string reason) 
        {
            Console.WriteLine($"Connection to Archipelago lost: {reason}");
            Disconnect();
        }

        public static void Disconnect()
        {
            Console.WriteLine("Disconnecting from server...");
            Session?.Socket.Disconnect();
            Session = null;
            Authenticated = false;
            attemptingConnection = false;
        }

        public static void UpdateArchipelagoState()
        {
            while (ItemQueue.Count > 0)
            {
                ItemsAndLocationsHandler.Unlock((long)ItemQueue.Dequeue());
            }

            if (DialogQueue.Count > 0)
            {
                var message = (string)DialogQueue.Dequeue();
                Console.WriteLine(message);
                DialogChanger.CreateDialogBox(message);
            }
            if (!Authenticated)
            {
                Console.WriteLine("Attempting to reconnect to Archipelago Server...");
                ThreadPool.QueueUserWorkItem(_ => ConnectAsync());
                return;
            }
            if (ServerData.Index > RandomizerStateManager.ReceivedItemsCount())
            {
                ItemsAndLocationsHandler.ReSync();
            }
        }

        public static void UpdateClientStatus(ArchipelagoClientState newState)
        {
            if (newState == ArchipelagoClientState.ClientGoal)
                Session.DataStorage[Scope.Slot, "HasFinished"] = true;
            Console.WriteLine($"Updating client status to {newState}");
            var statusUpdatePacket = new StatusUpdatePacket { Status = newState };
            Session.Socket.SendPacket(statusUpdatePacket);
        }

        private static bool ClientFinished()
        {
            if (!Authenticated) return false;
            return Session.DataStorage[Scope.Slot, "HasFinished"].To<bool?>() == true;
        }

        public static bool CanRelease()
        {
            if (!Authenticated) return false;
            Permissions releasePermission = Session.RoomState.ReleasePermissions;
            switch (releasePermission)
            {
                case Permissions.Goal:
                    return ClientFinished();
                case Permissions.Enabled:
                    return true;
            }
            return false;
        }

        public static bool CanCollect()
        {
            if (!Authenticated) return false;
            var collectPermission = Session.RoomState.CollectPermissions;
            switch (collectPermission)
            {
                case Permissions.Goal:
                    return ClientFinished();
                case Permissions.Enabled:
                    return true;
            }
            return false;
        }

        public static int GetHintCost()
        {
            var hintPercent = Session.RoomState.HintCostPercentage;
            if (hintPercent > 0)
            {
                var totalLocations = Session.Locations.AllLocations.Count;
                hintPercent = (int)Math.Round(totalLocations * (hintPercent * 0.01));
            }
            return hintPercent;
        }


        public static bool CanHint()
        {
            bool canHint = false;
            if (Authenticated)
            {
                canHint = GetHintCost() <= Session.RoomState.HintPoints;
            }
            return canHint;
        }

        public static string UpdateStatusText()
        {
            string text = string.Empty;
            if (DisplayStatus)
            {
                if (Authenticated)
                {
                    text = $"Connected to Archipelago v{Session.RoomState.Version}";
                    var hintCost = GetHintCost();
                    if (hintCost > 0)
                    {
                        text += $"\nHint points available: {Session.RoomState.HintPoints}\nHint point cost: {hintCost}";
                    }
                }
                else if (HasConnected)
                {
                    text = "Disconnected from Archipelago server.";
                }
            }            
            return text;
        }

        public static string UpdateMessagesText()
        {
            var text = string.Empty;
            if (MessageQueue.Count > 0)
            {
                if (DisplayAPMessages)
                    text = (string)MessageQueue.Dequeue();
            }
            return text;
        }
    }
}