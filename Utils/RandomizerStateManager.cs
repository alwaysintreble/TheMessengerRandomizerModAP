using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using MessengerRando.Archipelago;
using MessengerRando.GameOverrideManagers;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MessengerRando.Utils
{
    public class RandomizerStateManager
    {
        public static RandomizerStateManager Instance { private set; get; }
        public int CurrentFileSlot { set; get; }

        public RandoPowerSealManager PowerSealManager;
        public RandoBossManager BossManager;
        public static List<NetworkItem> SeenHints = new List<NetworkItem>();

        public bool SkipMusicBox;
        public bool SkipPhantom;
        public bool InGame;

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
            SeenHints = new List<NetworkItem>();

            if (Instance.ScoutedLocations == null || Instance.ScoutedLocations.Count < 1)
            {
                var index = ItemsAndLocationsHandler.BaseOffset;
                var scoutIDs = new List<long>();
                foreach (var unused in DialogChanger.ItemDialogID)
                {
                    scoutIDs.Add(index);
                    index++;
                }
                ArchipelagoClient.Session.Locations.ScoutLocationsAsync(
                    SetupScoutedLocations,
                    scoutIDs.ToArray()
                );
            }

            ArchipelagoData.DeathLink = Convert.ToBoolean(slotData.TryGetValue("deathlink", out var deathLink)
                ? deathLink : slotData["death_link"]);

            Instance.PowerSealManager = new RandoPowerSealManager(Convert.ToInt32(slotData["required_seals"]));
            Instance.SkipMusicBox = !Convert.ToBoolean(slotData["music_box"]);
            RandoShopManager.ShopPrices = ((JObject)slotData["shop"]).ToObject<Dictionary<EShopUpgradeID, int>>();
            RandoShopManager.FigurePrices = ((JObject)slotData["figures"]).ToObject<Dictionary<EFigurine, int>>();
        }

        private static void SetupScoutedLocations(LocationInfoPacket scoutedLocationInfo)
        {
            Instance.ScoutedLocations = new Dictionary<long, NetworkItem>();
            foreach (var networkItem in scoutedLocationInfo.Locations)
            {
                // janky workaround due to scouted locations all having a player of 1 for some reason?
                var item = networkItem;
                item.Player = ArchipelagoClient.Session.ConnectionInfo.Slot;
                Instance.ScoutedLocations.Add(item.Location, item);
            }

            ArchipelagoClient.ServerData.ScoutedLocations = Instance.ScoutedLocations;
            Console.WriteLine("Scouting done");
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
                return ArchipelagoClient.Session.Locations.AllLocations.Contains(locationID);
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

        public static void InitializeNewSecondQuest(SaveGameSelectionScreen saveScreen, int slot)
        {
            var saveManager = Manager<SaveManager>.Instance;
            saveManager.SelectSaveGameSlot(slot);
            saveManager.NewGame();
            saveManager.GetCurrentSaveGameSlot().SlotName = "Test";
            var progManager = Manager<ProgressionManager>.Instance;
            progManager.secondQuest = true;
            progManager.checkpointSaveInfo = new CheckpointSaveInfo
            {
                mana = 3,
                loadedLevelDimension = EBits.BITS_16,
                playerLocationDimension = EBits.BITS_16,
                loadedLevelName = ELevel.Level_13_TowerOfTimeHQ.ToString(),
                playerLocationSceneName = ELevel.Level_13_TowerOfTimeHQ.ToString(),
                loadedLevelPlayerPosition = new Vector3(1160.79f, -42.5f, 0f),
                loadedLevelCheckpointIndex = -1,
                playerFacingDirection = -1f
            };
            var discoveredLevels = new List<ELevel>
            {
                ELevel.Level_01_NinjaVillage,
                ELevel.Level_02_AutumnHills,
                ELevel.Level_03_ForlornTemple,
                ELevel.Level_04_Catacombs,
                ELevel.Level_06_A_BambooCreek,
                ELevel.Level_05_A_HowlingGrotto,
                ELevel.Level_07_QuillshroomMarsh,
                ELevel.Level_08_SearingCrags,
                ELevel.Level_09_A_GlacialPeak,
                ELevel.Level_10_A_TowerOfTime,
                ELevel.Level_11_A_CloudRuins,
                ELevel.Level_12_UnderWorld,
                ELevel.Level_13_TowerOfTimeHQ,
            };
            progManager.levelsDiscovered.AddRange(discoveredLevels);
            progManager.allTimeDiscoveredLevels.AddRange(discoveredLevels);
            progManager.SetFlag(Flags.TOTHQ_SmallMageFirstInterractionDone, false);
            progManager.SetFlag(Flags.CloudStepTutorialDone, false);
            var skipCutscenes = new List<string>
            {
                // "ClimbDownToCatacombsCutscene",
                // "ClimbDownToSearingCragsCutscene",
                // "ClimbUpFromCatacombsCutscene",
                // "ClimbUpToForlornCutscene",
                // "CloudStepIntroCutscene",
                // "CloudStepComeBackCutscene",
                // "CloudRuinsTowerEntranceCutscene",
                // "GlacialPeakTowerOfTimeCutscene",
                // "GlacialPeakTowerOutCutscene",
                // "GlouciousEntranceFromSearingRopeCutscene",
                "NinjaVillageElderCutScene",
                "NinjaVillageIntroEndCutScene",
                // "SearagToGlacialPeakEntranceCutscene",
                "SecondQuestStartShopCutscene",
            };
            progManager.cutscenesPlayed.AddRange(skipCutscenes);
            Manager<LevelManager>.Instance.lastLevelLoaded = ELevel.Level_13_TowerOfTimeHQ.ToString();
            saveManager.Save();
            try
            {
                saveScreen.OnLoadGame(slot);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static int ReceivedItemsCount()
        {
            return ArchipelagoClient.ServerData.ReceivedItems.Sum(item => item.Value);
        }
    }
}
