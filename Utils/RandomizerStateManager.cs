using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using MessengerRando.Archipelago;
using MessengerRando.GameOverrideManagers;
using MessengerRando.Utils.Constants;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocketSharp;
using Random = System.Random;

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace MessengerRando.Utils
{
    public class RandomizerStateManager
    {
        public static RandomizerStateManager Instance { private set; get; }
        public int CurrentFileSlot { set; get; }

        public RandoPowerSealManager PowerSealManager;
        // ReSharper disable once UnassignedField.Global
        // gets assigned externally
        public RandoBossManager BossManager;
        public static List<long> SeenHints = new ();

        public bool SkipMusicBox;
        // ReSharper disable once UnassignedField.Global
        // useful for debugging
        public bool SkipPhantom;

        public Dictionary<long, ScoutedItemInfo> ScoutedLocations;
        public readonly Dictionary<int, ArchipelagoData> APSave;

        public static Random SeedRandom;
        public static bool OnMainMenu = true;
        public bool CanNinjaWarp;
        public bool CanSearingWarp;

        public RandomizerStateManager()
        {
            #if DEBUG
            // SkipPhantom = true;
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
                if (ArchipelagoClient.Authenticated) InitializeSeed();   
            } catch (Exception e) {Console.WriteLine(e);}
        }

        public static void InitializeSeed()
        {
            var slotData = ArchipelagoClient.ServerData.SlotData;
            SeenHints = new List<long>();

            if (ArchipelagoClient.ServerData.SeedName.IsNullOrEmpty() ||
                ArchipelagoClient.ServerData.SeedName.Equals("Unknown"))
            {
                SeedRandom = new Random();
            }
            else
            {
                SeedRandom = new Random(ArchipelagoClient.ServerData.SeedName.GetHashCode());
            }

            if ((Instance.ScoutedLocations == null || Instance.ScoutedLocations.Count < 1) && ArchipelagoClient.Authenticated)
            {
                ArchipelagoClient.Session.Locations.ScoutLocationsAsync(
                    SetupScoutedLocations,
                    ArchipelagoClient.Session.Locations.AllLocations.ToArray()
                );
            }

            ArchipelagoData.DeathLink = Convert.ToBoolean(slotData.TryGetValue("deathlink", out var deathLink)
                ? deathLink : slotData["death_link"]);

            Instance.PowerSealManager = new RandoPowerSealManager(Convert.ToInt32(slotData["required_seals"]));
            Instance.SkipMusicBox = !Convert.ToBoolean(slotData["music_box"]);
            RandoShopManager.ShopPrices = ((JObject)slotData["shop"]).ToObject<Dictionary<EShopUpgradeID, int>>();
            RandoShopManager.FigurePrices = ((JObject)slotData["figures"]).ToObject<Dictionary<EFigurine, int>>();
            if (slotData.TryGetValue("starting_portals", out var portals))
            {
                var startingPortals = ((JArray)portals).ToObject<List<string>>();
                RandoPortalManager.StartingPortals = new List<string>();
                foreach (var portal in startingPortals)
                {
                    Console.WriteLine(portal);
                    RandoPortalManager.StartingPortals.Add(portal);
                }

                var portalExits = ((JArray)slotData["portal_exits"]).ToObject<List<int>>();
                RandoPortalManager.PortalMapping = new List<RandoPortalManager.Portal>();
                foreach (var portalExit in portalExits)
                {
                    RandoPortalManager.PortalMapping.Add(new RandoPortalManager.Portal(portalExit));
                }

                if (slotData.TryGetValue("transitions", out var transitions))
                {
                    RandoLevelManager.RandoLevelMapping =
                        new Dictionary<string, LevelConstants.RandoLevel>();
                    var transitionPairs = ((JArray)transitions).ToObject<List<List<int>>>();
                    if (transitionPairs.Count == 0) RandoLevelManager.RandoLevelMapping = null;
                    else
                    {
                        foreach (var pairing in transitionPairs)
                        {
                            var orig = LevelConstants.TransitionNames[pairing[0]];
                            var replacement =
                                LevelConstants.EntranceNameToRandoLevel[LevelConstants.TransitionNames[pairing[1]]];
                            RandoLevelManager.RandoLevelMapping[orig] = replacement;
                            Console.WriteLine($"{orig}: {LevelConstants.TransitionNames[pairing[1]]}");
                        }
                    }
                }
            }
            else
            {
                RandoPortalManager.StartingPortals = new List<string>
                {
                    "AutumnHillsPortal",
                    "RiviereTurquoisePortal",
                    "HowlingGrottoPortal",
                    "SunkenShrinePortal",
                    "SearingCragsPortal",
                    "GlacialPeakPortal",
                };
            }
        }

        private static void SetupScoutedLocations(Dictionary<long, ScoutedItemInfo> scoutedLocationInfo)
        {
            foreach (var itemInfo in scoutedLocationInfo.Values)
            {
            }
            Instance.ScoutedLocations = scoutedLocationInfo;
            Console.WriteLine("scouting done");
        }

        public static bool IsSafeTeleportState()
        {
            //Unsafe teleport states are shops/hq/boss fights
            try
            {
                return !(Manager<TotHQ>.Instance.root.gameObject.activeInHierarchy ||
                         Manager<Shop>.Instance.gameObject.activeInHierarchy ||
                         Manager<GameManager>.Instance.IsCutscenePlaying() ||
                         Manager<PlayerManager>.Instance.Player.IsInvincible() ||
                         Manager<PlayerManager>.Instance.Player.InputBlocked() ||
                         Manager<PlayerManager>.Instance.Player.IsKnockedBack);
            }
            catch (Exception)
            {
                return false;
            }
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
                return (ScoutedLocations != null && ScoutedLocations.ContainsKey(locationID)) ||
                       ArchipelagoClient.ServerData.LocationData.ContainsKey(locationID);
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
            // create a fresh save slot
            var saveManager = Manager<SaveManager>.Instance;
            saveManager.SelectSaveGameSlot(slot);
            saveManager.NewGame();
            saveManager.LoadSaveSlot(saveManager.GetCurrentSaveGameSlot());
            saveManager.GetCurrentSaveGameSlot().SlotName = ArchipelagoClient.ServerData.SlotName;
            // add everything to the various managers that we need for our save slot, following the order in SaveGameSlot.UpdateSaveGameData()
            var progManager = Manager<ProgressionManager>.Instance;
            progManager.challengeRoomsCompleted = [];
            progManager.lastSaveTime = Time.time;
            progManager.checkpointSaveInfo = new CheckpointSaveInfo
            {
                mana = 0,
                loadedLevelDimension = EBits.BITS_16,
                playerLocationDimension = EBits.BITS_16,
                loadedLevelName = ELevel.Level_02_AutumnHills.ToString(),
                playerLocationSceneName = ELevel.Level_13_TowerOfTimeHQ.ToString(),
                loadedLevelPlayerPosition = new Vector3(485.03f, -101.5f, 0f),
                loadedLevelCheckpointIndex = -1,
                playerFacingDirection = 1
            };
            var flagsToSet = new[]
            {
                "CloudStepTutorialDone", "RuxxtinEncounter_1", "RuxxtinEncounter_2", "ManfredChase_1_Done",
                "ManfredChase_2_Done", "ManfredChase_3_Done", "TOTHQ_SmallMageFirstInterractionDone"
            };
            foreach (var flag in flagsToSet)
            {
                progManager.SetFlag(flag, false);
            }

            var invManager = Manager<InventoryManager>.Instance;
            invManager.ItemQuantityByItemId = new();
            invManager.shopUpgradeUnlocked = [];
            invManager.figurinesCollected = [];
            
            invManager.ItemQuantityByItemId.Add(EItems.SCROLL_UPGRADE, 1);
            invManager.ItemQuantityByItemId.Add(EItems.TIME_SHARD, 0);
            invManager.ItemQuantityByItemId.Add(EItems.MAP, 1);
            invManager.ItemQuantityByItemId.Add(EItems.CLIMBING_CLAWS, 1);
            invManager.AllTimeItemQuantityByItemId = invManager.ItemQuantityByItemId;
            
            progManager.secondQuest = true;
            // var discoveredLevels = new List<ELevel>
            // {
            //     ELevel.Level_01_NinjaVillage,
            //     ELevel.Level_02_AutumnHills,
            //     ELevel.Level_03_ForlornTemple,
            //     ELevel.Level_04_Catacombs,
            //     ELevel.Level_06_A_BambooCreek,
            //     ELevel.Level_05_A_HowlingGrotto,
            //     ELevel.Level_07_QuillshroomMarsh,
            //     ELevel.Level_08_SearingCrags,
            //     ELevel.Level_09_A_GlacialPeak,
            //     ELevel.Level_10_A_TowerOfTime,
            //     ELevel.Level_11_A_CloudRuins,
            //     ELevel.Level_12_UnderWorld,
            //     ELevel.Level_13_TowerOfTimeHQ,
            //     ELevel.Level_04_C_RiviereTurquoise,
            //     ELevel.Level_05_B_SunkenShrine,
            // };
            // progManager.levelsDiscovered.AddRange(discoveredLevels);
            // progManager.allTimeDiscoveredLevels.AddRange(discoveredLevels);
            progManager.isLevelDiscoveredAwarded = true;
            
            var skipCutscenes = new List<string>
            {
                "MessengerCutScene",
                "CloudStepIntroCutScene",
                "NinjaVillageElderCutScene",
                "DemonGeneralCutScene",
                "NinjaVillageIntroEndCutScene",
                "PowerSealCollectedCutscene",
                // "LeafGolemIntroCutScene",
                // "LeafGolemOutroCutScene",
                // "NecrophobicWorkerCutscene",
                // "NecromancerIntroCutscene",
                // "NecromancerOutroCutscene",
                // "HowlingGrottoBossIntroCutscene",
                // "HowlingGrottoBossOutroCutscene",
                // "HowlingGrottoToQuillshroomFirstQuestCutScene",
                // "QuillshroomMarshBossIntroCutscene",
                // "QuillshroomMarshBossOutroCutscene",
                // "SearingCragsBossIntroCutscene",
                // "SearingCragsBossOutroCutscene",
                // "SearagToGlacialPeakEntranceCutscene",
                // "GlacialPeakTowerOfTimeCutscene",
                // "GlacialPeakTowerOutCutscene",
                "QuibbleIntroCutscene",
                // "TowerOfTimeBossIntroCutscene",
                // "TowerOfTimeBossOutroCutscene",
                // "Teleport16BitsRoomCutscene",
                // "To16BitsCutscene",
                // "TowerOfTimeTeleportToCloudRuinsCutscene",
                // "CloudRuinsTowerEntranceCutscene",
                // "ManfredBossIntroCutscene",
                // "ManfredBossOutroCutscene",
                // "DemonGeneralBossIntroCutscene",
                // "DemonGeneralBossOutroCutscene",
                // "UnderworldManfredEscapeCutscene",
                "FutureMessengerCutscene",
                "SecondQuestStartShopCutscene",
                "ArmoireOpeningCutscene",
                "DialogCutscene",
                "GoToTotMageDialog",
                "ProphetIntroCutscene",
                "ProphetHeyCutscene",
                // "PortalOpeningCutscene",
                "ExitPortalAwardMapCutscene",
            };
            progManager.cutscenesPlayed = skipCutscenes;

            progManager.actionSequenceDone =
                ["AwardGrimplouSequence(Clone)", "AwardGlidouSequence(Clone)", "AwardGraplouSequence(Clone)"];
            // copy everything from the managers to the save slot
            saveManager.GetCurrentSaveGameSlot().UpdateSaveGameData();
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

        public static void StartOfflineSeed()
        {
            if (ItemsAndLocationsHandler.ItemsLookup == null)
                ItemsAndLocationsHandler.Initialize();
            var filePath = Directory.GetCurrentDirectory() + "\\Archipelago\\output";
            DateTime latestTime = new DateTime();
            string gameFile = "";
            foreach (var file in Directory.GetFiles(filePath))
            {
                if (File.GetCreationTime(file) > latestTime && file.EndsWith("aptm"))
                {
                    latestTime = File.GetCreationTime(file);
                    gameFile = file;
                }
            }

            if (gameFile.IsNullOrEmpty())
            {
                Console.WriteLine("unable to find file");
                return;
            }

            try
            {
                var fileData = File.ReadAllText(gameFile);
                var gameData = JObject.Parse(fileData);
                ArchipelagoClient.ServerData = new ArchipelagoData();
                ArchipelagoClient.ServerData.StartNewSeed();
                var fileNameParts = gameFile.Split('_');
                foreach (var part in fileNameParts)
                {
                    if (!double.TryParse(part, out _)) continue;
                    ArchipelagoClient.ServerData.SeedName = part;
                    break;
                }

                ArchipelagoClient.ServerData.Uri = "offline";
                ArchipelagoClient.ServerData.SlotName = gameData["name"].ToObject<string>();
                Console.WriteLine("casting slot data");
                ArchipelagoClient.ServerData.SlotData = gameData["slot_data"].ToObject<Dictionary<string, object>>();
                Console.WriteLine("casting loc data");
                ArchipelagoClient.ServerData.LocationData =
                    gameData["loc_data"].ToObject<Dictionary<long, Dictionary<string, List<long>>>>();
                ArchipelagoClient.HasConnected = ArchipelagoClient.Offline = true;
                InitializeSeed();
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
