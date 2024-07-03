using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;
using MessengerRando.GameOverrideManagers;
using MessengerRando.Utils;
using Mod.Courier.UI;
using UnityEngine;
using static Mod.Courier.UI.TextEntryButtonInfo;

namespace MessengerRando.Archipelago
{
    public static class ArchipelagoClient
    {
        private const string ApVersion = "0.4.4";
        public static ArchipelagoData ServerData = new ArchipelagoData();

        private delegate void OnConnectAttempt(string result);
        public static bool Authenticated;
        public static bool HasConnected;
        private static bool attemptingConnection;
        public static bool Offline;

        public static bool DisplayAPMessages = true;
        public static bool FilterAPMessages = true;
        public static bool DisplayStatus = true;
        public static bool HintPopUps = true;

        public static ArchipelagoSession Session;
        public static DeathLinkInterface DeathLinkHandler;

        public static Queue ItemQueue = new Queue();
        public static Queue DialogQueue = new Queue();
        private static Queue messageQueue = new Queue();
        public static int OfflineReceivedItems;

        public static List<string> EventsICareAbout = new List<string>
        {
            "DecurseQueenCutscene",
            "QueenDefrostLanternsCutscene",
            "ElderAwardSeedCutscene",
            "PlantTeaSeedCutscene",
        };

        private static bool hasSynced;
        
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

        private static void OnConnected(string connectStats)
        {
            if (ServerData.CheckedLocations != null)
            {
                Session.Locations.CompleteLocationChecksAsync(
                    _ => ServerData.CheckedLocations = Session.Locations.AllLocationsChecked.ToList(),
                    ServerData.CheckedLocations.ToArray());
            }
            attemptingConnection = false;
        }

        private static void OnConnected(string outputText, SubMenuButtonInfo connectButton)
        {
            TextEntryPopup successPopup = InitTextEntryPopup(connectButton.addedTo, string.Empty,
                entry => true, 0, null, CharsetFlags.Space);
            
            successPopup.Init(outputText);
            successPopup.gameObject.SetActive(true);
            // Object.Destroy(successPopup.transform.Find("BigFrame").Find("SymbolsGrid").gameObject);
            Console.WriteLine(outputText);
            attemptingConnection = false;
        }

        private static ArchipelagoSession CreateSession()
        {
            var session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri, ServerData.Port);
            SetupSession(session);
            return session;
        }

        private static ArchipelagoSession CreateSession(Uri uri)
        {
            var session = ArchipelagoSessionFactory.CreateSession(uri);
            SetupSession(session);
            return session;
        }

        private static void SetupSession(ArchipelagoSession session)
        {
            session.MessageLog.OnMessageReceived += OnMessageReceived;
            session.Items.ItemReceived += OnItemReceived;
            session.Socket.ErrorReceived += SessionErrorReceived;
            session.Socket.SocketClosed += SessionSocketClosed;
        }

        public static string Connect(Uri uri)
        {
            if (Authenticated) return "already connected";
            
            try
            {
                Session = CreateSession(uri);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
                return e.ToString();
            }

            return TryConnect();
        }

        public static string Connect()
        {
            if (Authenticated) return "already connected";

            try
            {
                Session = CreateSession();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
                return e.ToString();
            }

            return TryConnect();
        }

        private static string TryConnect()
        {
            if (ItemsAndLocationsHandler.ItemsLookup == null) ItemsAndLocationsHandler.Initialize();
            var needSlotData = ServerData.SlotData == null;
            LoginResult result;

            try
            {
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
                Console.Write($"Error: {e}");
                result = new LoginFailure(e.GetBaseException().Message);
            }

            return HandleConnectResult(result, needSlotData);
        }

        private static string HandleConnectResult(LoginResult result, bool needSlotData)
        {
            string outputText;
            if (result.Successful)
            {
                var success = (LoginSuccessful)result;
                if (needSlotData)
                    ServerData.SlotData = success.SlotData;
                ServerData.SeedName = Session.RoomState.Seed;
                Authenticated = true;

                try
                {
                    RandomizerStateManager.InitializeSeed();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    outputText =
                        "Something went wrong.\n" +
                        "Please submit a bug report with the log.txt, " +
                        "which can be found by the game executable.";
                    Authenticated = false;
                    Disconnect();
                    return outputText;
                }
                
                DeathLinkHandler = new DeathLinkInterface();
                
                ThreadPool.QueueUserWorkItem(o =>
                    Session.Locations.CompleteLocationChecksAsync(null,
                        ServerData.CheckedLocations.ToArray()));

                HasConnected = true;
                outputText = $"Successfully connected to {ServerData.Uri}:{ServerData.Port} as {ServerData.SlotName}!";
            }
            else
            {
                LoginFailure failure = (LoginFailure)result;
                outputText = $"Failed to connect to {ServerData.Uri}:{ServerData.Port} as {ServerData.SlotName}\n";
                outputText +=
                    failure.Errors.Aggregate(outputText, (current, error) => current + $"\n    {error}");

                Console.WriteLine(outputText);

                Authenticated = false;
                Disconnect();
            }

            return outputText;
        }

        private static void Connect(OnConnectAttempt attempt)
        {
            attempt(Connect());
        }

        private static string ColorizePlayerName(int player)
        {
            var color = player.Equals(Session.ConnectionInfo.Slot)
                ? UserConfig.PlayerColor
                : UserConfig.OtherPlayerColor;
            return $"<color=#{color}>{Session.Players.GetPlayerAlias(player)}</color>";
        }

        private static string ColorizeLocation(long locID)
        {
            var color = UserConfig.LocationColor;
            return $"<color=#{color}>{Session.Locations.GetLocationNameFromId(locID)}</color>";
        }

        private static string ConvertHintMessage(HintItemSendLogMessage hintMessage)
        {
            var colorizedMessage = hintMessage.ToString();
            colorizedMessage = colorizedMessage.Replace(hintMessage.Sender.Name,
                ColorizePlayerName(hintMessage.Sender.Slot));
            colorizedMessage = colorizedMessage.Replace(hintMessage.Receiver.Name,
                ColorizePlayerName(hintMessage.Receiver.Slot));
            colorizedMessage = colorizedMessage.Replace(hintMessage.Item.LocationDisplayName,
                hintMessage.Item.ColorizeLocation());
            colorizedMessage = colorizedMessage.Replace(hintMessage.Item.ItemDisplayName,
                hintMessage.Item.Colorize());
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
                        if (hintMessage.IsFound || !hintMessage.IsRelatedToActivePlayer || !HintPopUps ||
                            RandomizerStateManager.SeenHints.Contains(hintMessage.Item.ItemId)) break;
                        RandomizerStateManager.SeenHints.Add(hintMessage.Item.ItemId);
                        messageQueue.Enqueue(hintMessage.ToString());
                        DialogQueue.Enqueue(ConvertHintMessage(hintMessage));
                        break;
                    case ItemSendLogMessage itemSendMessage:
                        if (itemSendMessage.IsRelatedToActivePlayer)
                        {
                            messageQueue.Enqueue(itemSendMessage.ToString());
                        }
                        break;
                }
            }
            else
            {
                if (HintPopUps && message is HintItemSendLogMessage hintMessage)
                    DialogQueue.Enqueue(ConvertHintMessage(hintMessage));
                messageQueue.Enqueue(message.ToString());
            }
        }

        public static void SyncLocations()
        {
            if (RandomizerStateManager.Instance.CurrentFileSlot == 0) return;
            var checkedLocations = Session.Locations.AllLocationsChecked;
            SyncEvents();
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
                        Debug.Log($"collecting {shopLoc.LocationName} ({shopLoc.PrettyLocationName})");
                        try
                        {
                            var shopID = (EShopUpgradeID)Enum.Parse(typeof(EShopUpgradeID), shopLoc.LocationName);
                            Manager<InventoryManager>.Instance.SetShopUpgradeAsUnlocked(shopID);
                        }
                        catch (Exception e1)
                        {
                            Debug.Log(e1);
                            try
                            {
                                var shopID = (EShopUpgradeID)Enum.Parse(typeof(EShopUpgradeID),
                                    shopLoc.PrettyLocationName);
                                Manager<InventoryManager>.Instance.SetShopUpgradeAsUnlocked(shopID);
                            }
                            catch (Exception e2)
                            {
                                Debug.Log(e2);
                            }
                        }
                    }
                    else
                    {
                        RandoBossManager.TryCollectLocation(location);
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

        private static void SyncEvents()
        {
            if (hasSynced) return;
            Console.WriteLine("Checking datastorage events");
            var eventStorage = Session.DataStorage[Scope.Slot, "Events"];
            eventStorage.Initialize(new List<string>());
            foreach (var cutscene in eventStorage.To<List<string>>())
            {
                Console.WriteLine(cutscene);
                Manager<ProgressionManager>.Instance.cutscenesPlayed.Add(cutscene);
            }

            var teleportStorage = Session.DataStorage[Scope.Slot, "UnlockedTeleports"];
            teleportStorage.Initialize(new List<bool>{false, false});
            var storedBools = teleportStorage.To<List<bool>>();
            RandomizerStateManager.Instance.CanNinjaWarp = storedBools[0];
            RandomizerStateManager.Instance.CanSearingWarp = storedBools[1];

            hasSynced = true;
        }
        private static void OnItemReceived(ReceivedItemsHelper helper)
        {
            var itemToUnlock = helper.DequeueItem();
            Console.WriteLine($"helper index: {helper.Index}");
            Console.WriteLine($"Saved index: {ServerData.Index}");
            if (helper.Index <= ServerData.Index) return;

            if (RandomizerStateManager.OnMainMenu)
            {
                OfflineReceivedItems++;
            }
            else
            {
                ServerData.Index++;
            }
            ItemQueue.Enqueue(itemToUnlock.ItemId);
            if (itemToUnlock.Player != Session.ConnectionInfo.Slot)
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
            DialogQueue = new Queue();
            ItemQueue = new Queue();
            messageQueue = new Queue();
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

            TrapManager.UpdateTrapStatus();
            if (!Authenticated)
            {
                if (Offline) return;
                Console.WriteLine("Attempting to reconnect to Archipelago Server...");
                ThreadPool.QueueUserWorkItem(_ => ConnectAsync());
                return;
            }
            
            if (ServerData.Index < Session.Items.AllItemsReceived.Count)
                return;
            Debug.Log("re-syncing...");
            ItemsAndLocationsHandler.ReSync();
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
            switch (Session.RoomState.ReleasePermissions)
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
            switch (Session.RoomState.CollectPermissions)
            {
                case Permissions.Goal:
                    return ClientFinished();
                case Permissions.Enabled:
                    return true;
            }
            return false;
        }

        private static int GetHintCost()
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
            return Authenticated && GetHintCost() <= Session.RoomState.HintPoints;
        }

        public static string UpdateStatusText()
        {
            var text = string.Empty;
            if (!DisplayStatus) return text;
            if (Authenticated)
            {
                text = $"Connected to Archipelago v{Session.RoomState.Version}";
                var hintCost = GetHintCost();
                if (hintCost > 0)
                {
                    text += $"\nHint points available: {Session.RoomState.HintPoints}\nHint point cost: {hintCost}";
                }
            }
            else if (Offline)
            {
                text = "";
            }
            else if (HasConnected)
            {
                text = "Disconnected from Archipelago server.";
            }
            return text;
        }

        public static string UpdateMessagesText()
        {
            var text = string.Empty;
            if (messageQueue.Count <= 0) return text;
            if (DisplayAPMessages)
                text = (string)messageQueue.Dequeue();
            return text;
        }
    }
}