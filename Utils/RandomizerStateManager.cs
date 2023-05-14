using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using MessengerRando.Archipelago;
using MessengerRando.GameOverrideManagers;
using Newtonsoft.Json;

namespace MessengerRando.Utils
{
    public class RandomizerStateManager
    {
        public static RandomizerStateManager Instance { private set; get; }
        public int CurrentFileSlot { set; get; }

        public RandoPowerSealManager PowerSealManager;
        public RandoBossManager BossManager;

        public string Goal;
        public bool SkipMusicBox;
        public bool SkipPhantom;
        public bool MegaShards;

        public Dictionary<long, NetworkItem> ScoutedLocations;
        public Dictionary<int, ArchipelagoData> APSave;

        public RandomizerStateManager()
        {
            #if DEBUG
            SkipPhantom = true;
            #endif
            try
            {
                //Create initial values for the state machine
                ItemsAndLocationsHandler.RandoStateManager = Instance = this;
                APSave = new Dictionary<int, ArchipelagoData>
                {
                    { 1, new ArchipelagoData() },
                    { 2, new ArchipelagoData() },
                    { 3, new ArchipelagoData() },
                };
                if (ArchipelagoClient.Authenticated) InitializeMultiSeed();   
            } catch (Exception e) {Console.WriteLine(e);}
        }

        public static void InitializeMultiSeed()
        {
            var slotData = ArchipelagoClient.ServerData.SlotData;

            ArchipelagoClient.Session.Locations.ScoutLocationsAsync(
                SetupScoutedLocations,
                ItemsAndLocationsHandler.LocationsLookup.Values
                    .Where(loc => ArchipelagoClient.Session.Locations.AllMissingLocations.Contains(loc))
                    .ToArray()
                );

            if (slotData.TryGetValue("deathlink", out var deathLink))
                ArchipelagoData.DeathLink = Convert.ToInt32(deathLink) == 1;
            else Console.WriteLine("Failed to get deathlink option");

            if (slotData.TryGetValue("goal", out var gameGoal))
            {
                var goal = (string)gameGoal;
                Instance.Goal = goal;
                if (RandoPowerSealManager.Goals.Contains(goal))
                {
                    if (slotData.TryGetValue("required_seals", out var requiredSeals))
                    {
                        Instance.PowerSealManager =
                            new RandoPowerSealManager(Convert.ToInt32(requiredSeals));
                    }
                }

                if (slotData.TryGetValue("music_box", out var doMusicBox))
                    Instance.SkipMusicBox = Convert.ToInt32(doMusicBox) == 0;
                else Console.WriteLine("Failed to get music_box option");
                
            }
            else Console.WriteLine("Failed to get goal option");

            if (slotData.TryGetValue("bosses", out var bosses))
            {
                var bossMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(bosses.ToString());
                Console.WriteLine("Bosses:");
                foreach (var bossPair in bossMap)
                {
                    Console.WriteLine($"{bossPair.Key}: {bossPair.Value}");
                }
                try
                {
                    Instance.BossManager =
                        new RandoBossManager(bossMap);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else Console.WriteLine("Failed to get bosses option");

            if (slotData.TryGetValue("settings", out var genSettings))
            {
                var gameSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(genSettings.ToString());
                if (gameSettings.TryGetValue("Mega Shards", out var shuffleShards))
                    if (Convert.ToInt32(shuffleShards) == 1)
                        Instance.MegaShards = true;
            }
            else if (slotData.TryGetValue("mega_shards", out var shuffleShards))
                if (JsonConvert.DeserializeObject<int>(shuffleShards.ToString()) == 1)
                    Instance.MegaShards = true;

            if (slotData.TryGetValue("shop", out var shopSettings))
            {
                var shopPrices = JsonConvert.DeserializeObject<Dictionary<string, int>>(shopSettings.ToString());
                RandoShopManager.ShopPrices = new Dictionary<EShopUpgradeID, int>();
                foreach (var shopItem in shopPrices)
                {
                    RandoShopManager.ShopPrices.Add(
                        (EShopUpgradeID)Enum.Parse(typeof(EShopUpgradeID), shopItem.Key),
                        shopItem.Value);
                }
            }

            if (slotData.TryGetValue("figures", out var figureSettings))
            {
                var figurePrices = JsonConvert.DeserializeObject<Dictionary<string, int>>(figureSettings.ToString());
                RandoShopManager.FigurePrices = new Dictionary<EFigurine, int>();
                foreach (var figure in figurePrices)
                {
                    RandoShopManager.FigurePrices.Add((EFigurine)Enum.Parse(typeof(EFigurine), figure.Key), figure.Value);
                }
            }
        }

        private static void SetupScoutedLocations(LocationInfoPacket scoutedLocationInfo)
        {
            Console.WriteLine("Setting up scouts");
            Instance.ScoutedLocations = new Dictionary<long, NetworkItem>();
            foreach (var networkItem in scoutedLocationInfo.Locations)
            {
                Instance.ScoutedLocations.Add(networkItem.Location, networkItem);
            }
            Manager<DialogManager>.Instance.LoadDialogs(Manager<LocalizationManager>.Instance.CurrentLanguage);
        }

        public static bool IsSafeTeleportState()
        {
            //Unsafe teleport states are shops/hq/boss fights
            return !(Manager<TotHQ>.Instance.root.gameObject.activeInHierarchy ||
                     Manager<Shop>.Instance.gameObject.activeInHierarchy ||
                     Manager<GameManager>.Instance.IsCutscenePlaying() ||
                     Manager<PlayerManager>.Instance.Player.IsInvincible() ||
                     Manager<PlayerManager>.Instance.Player.InputBlocked() || 
                     Manager<PlayerManager>.Instance.Player.IsKnockedBack);
        }

        /// <summary>
        /// Check through the mappings for any location that is represented by vanilla location item(since that is the key used to uniquely identify locations).
        /// </summary>
        /// <param name="vanillaLocationItem">EItem being used to look up location.</param>
        /// <param name="locationID">Out parameter used to return the location found.</param>
        /// <returns>true if location was found, otherwise false(location item will be null in this case)</returns>
        public bool IsLocationRandomized(EItems vanillaLocationItem, out long locationID)
        {
            locationID = 0;

            if (!ArchipelagoClient.HasConnected) return false;
            try
            {
                locationID =
                    ItemsAndLocationsHandler.LocationFromEItem(vanillaLocationItem);
                if (locationID == 0) return false;
                Console.WriteLine($"Checking if {vanillaLocationItem}, id: {locationID} is randomized.");
                if (ScoutedLocations.ContainsKey(locationID) ||
                     ArchipelagoClient.ServerData.CheckedLocations.Contains(locationID))
                    return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }

        public static bool HasCompletedCheck(long locationID)
        {
            return ArchipelagoClient.ServerData.CheckedLocations != null &&
                   ArchipelagoClient.ServerData.CheckedLocations.Contains(locationID);
        }

        public static int ReceivedItemsCount()
        {
            return ArchipelagoClient.ServerData.ReceivedItems.Sum(item => item.Value);
        }
    }
}
