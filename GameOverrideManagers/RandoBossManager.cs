﻿using System;
using System.Collections.Generic;
using System.Linq;
using MessengerRando.Archipelago;
using MessengerRando.RO;
using MessengerRando.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
// ReSharper disable StringLiteralTypo

namespace MessengerRando.GameOverrideManagers
{
    public abstract class RandoBossManager
    {
        public static List<string> DefeatedBosses = [];

        private readonly Dictionary<string, string> origToNewBoss;
        private static bool bossOverride;

        private struct BossLocation
        {
            public readonly ELevel BossRegion;
            public readonly Vector2 PlayerPosition;
            public readonly EBits PlayerDimension;

            public BossLocation(ELevel area, Vector2 pos, EBits dim)
            {
                BossRegion = area;
                PlayerPosition = pos;
                PlayerDimension = dim;
            }
        }

        private static readonly List<string> VanillaBossNames = new List<string>
        {
            "LeafGolem",
            "Necromancer",
            "EmeraldGolem",
            "QueenOfQuills",
            // "Colos_Susses",
            "Manfred",
            // "TowerGolem",
            "DemonGeneral",
            "DemonArtificier",
            "ButterflyMatriarch",
            "ClockworkConcierge",
            "Phantom"
        };
        
        private static readonly Dictionary<long, string> IDToBossMap = new Dictionary<long, string>
        {
            { 11391075, "LeafGolem" },
            { 11391076, "Necromancer" },
            { 11391077, "EmeraldGolem" },
            { 11391078, "QueenOfQuills" },
        };

        public static readonly Dictionary<string, string> BossCutscenes = new Dictionary<string, string>
        {
            { "LeafGolem", "LeafGolemIntroCutscene" },
            { "Necromancer", "NecromancerIntroCutscene" },
            { "EmeraldGolem", "EmeraldGolemIntroCutscene" },
            { "QueenOfQuills", "QueenOfQuillsIntroCutscene" },
            // {"Colos_Susses", },
            { "Manfred", "ManfredIntroCutscene" },
            // {"TowerGolem", },
            { "DemonGeneral", "DemonGeneralIntroCutscene" },
            { "DemonArtificier", "DemonArtificierIntroCutscene" },
            { "ButterflyMatriarch", "ButterflyMatriarchIntroCutscene" },
            { "Phantom", "PhantomIntroCutscene" }
        };

        private static readonly Dictionary<string, string> RoomToVanillaBoss = new Dictionary<string, string>
        {
            { "908940-28-12", "LeafGolem" },
            { "748780-76-60", "Necromancer" },
            { "556588-140-60", "EmeraldGolem" },
            { "11001132-44-28", "QueenOfQuills" },
            { "332364308324", "Colos_Susses" },
            { "11641228-28-12", "Manfred" },
            // { "108140228244", "TowerGolem" },
            { "140172-44-28", "DemonGeneral" },
            { "396428-12436", "DemonArtificier" },
            { "-308-276420", "ButterflyMatriarch" }
        };

        private static readonly Dictionary<string, BossLocation> BossLocations = new Dictionary<string, BossLocation> 
        {
            { "LeafGolem", new BossLocation(ELevel.Level_02_AutumnHills, new Vector2(908, -27), EBits.BITS_8) },
            { "Necromancer", new BossLocation(ELevel.Level_04_Catacombs, new Vector2(752, -75), EBits.BITS_8) },
            { "EmeraldGolem", new BossLocation(ELevel.Level_05_A_HowlingGrotto, new Vector2(560, -123), EBits.BITS_8) },
            { "QueenOfQuills", new BossLocation( ELevel.Level_07_QuillshroomMarsh, new Vector2(1100, -43), EBits.BITS_8) },
            // { "Colos_Susses", new BossLocation(ELevel.Level_08_SearingCrags, new Vector2(364, 311), EBits.BITS_8) },
            // { "Manfred", new BossLocation(ELevel.Level_11_A_CloudRuins, new Vector2(1165, -26), EBits.BITS_16)},
            // { "Tower Golem", new BossLocation(ELevel.Level_10_A_TowerOfTime, new Vector2(108, 237), EBits.BITS_16) },
            // { "DemonGeneral", new BossLocation(ELevel.Level_12_UnderWorld, new Vector2(140, -43), EBits.BITS_16) },
            // { "DemonArtificier", new BossLocation(ELevel.Level_03_ForlornTemple, new Vector2(396, -11), EBits.BITS_16)},
            // { "ButterflyMatriarch", new BossLocation(ELevel.Level_04_C_RiviereTurquoise, new Vector2(-276, 6), EBits.BITS_16) }
        };

        private static readonly List<string> BossRoomKeys = new List<string>
        {
            "908940-28-12",
            "748780-76-60",
            "556588-140-60",
            "11001132-44-28",
            "332364308324",
            "11641228-28-12",
            "108140228244",
            "140172-44-28",
            "396428-12436",
            "-308-276420",
        };

        private static string GetVanillaBoss(string roomKey)
        {
            return RoomToVanillaBoss[roomKey];
        }

        private static void AdjustPlayerInBossRoom(string bossName)
        {
            if (!BossLocations.TryGetValue(bossName, out var newLocation)) return;
            if (Manager<LevelManager>.Instance.GetCurrentLevelEnum().Equals(newLocation.BossRegion))
            {
                Manager<PlayerManager>.Instance.Player.transform.position = newLocation.PlayerPosition;
                Manager<DimensionManager>.Instance.SetDimension(newLocation.PlayerDimension);
            }
            else
            {
                bossOverride = true;
                RandoLevelManager.TeleportInArea(newLocation.BossRegion, newLocation.PlayerPosition,
                    newLocation.PlayerDimension);
            }
        }
        
        public static bool HasBossDefeated(string bossName)
        {
            if (bossOverride)
                bossName = RandomizerStateManager.Instance.BossManager.origToNewBoss
                    .First(name => name.Value.Equals(bossName)).Key;
            #if DEBUG
            Console.WriteLine($"Checking if {bossName} is defeated.");
            #endif
            try
            {
                return !VanillaBossNames.Contains(bossName) || DefeatedBosses.Contains(bossName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return true;
            }
        }

        public static void SetBossAsDefeated(string bossName)
        {
            if (DefeatedBosses.Contains(bossName))
                return;
            if (bossOverride)
            {
                bossName = RandomizerStateManager.Instance.BossManager.origToNewBoss[bossName];
                bossOverride = false;
            }
            if (ArchipelagoClient.HasConnected)
            {
                ArchipelagoClient.ServerData.DefeatedBosses.Add(bossName);
                if (BossLocations.ContainsKey(bossName))
                {
                    var bossLoc = new LocationRO(bossName);
                    ItemsAndLocationsHandler.SendLocationCheck(bossLoc);
                    if (bossName.Equals("EmeraldGolem"))
                    {
                        Manager<ProgressionManager>.Instance.cutscenesPlayed.Add("EmeraldGolemOutroCutscene");
                    }
                }
            }
            DefeatedBosses.Add(bossName);
            if (RandomizerStateManager.Instance.BossManager != null)
            {
                var newPosition = BossLocations[bossName];
                
                RandoLevelManager.TeleportInArea(newPosition.BossRegion, newPosition.PlayerPosition,
                    newPosition.PlayerDimension);
            }
        }

        public static bool ShouldFightBoss(string bossName)
        {
            if (bossOverride) return false;
            var currentLevel = Manager<LevelManager>.Instance.GetCurrentLevelEnum();
            Console.WriteLine($"Entered {bossName}'s room. Has Defeated: {HasBossDefeated(bossName)}");
            if (HasBossDefeated(bossName) || !currentLevel.Equals(BossLocations[bossName].BossRegion)) return false;
            
            var teleporting = RandomizerStateManager.Instance.BossManager != null;
            Console.WriteLine($"Should teleport: {teleporting}");
            if (teleporting)
            {
                try
                {
                    foreach (var cutscene in Object.FindObjectsOfType<Cutscene>())
                    {
                        try
                        {
                            if (cutscene.IsInvoking())
                            {
                                Console.WriteLine($"Ending cutscene: {cutscene.name}");
                                cutscene.EndCutScene();
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e}\n{cutscene.name}");
                        }
                    }

                    foreach (var cutscene in Object.FindObjectsOfType<DialogCutscene>())
                    {
                        try
                        {
                            if (cutscene.IsInvoking())
                            {
                                Console.WriteLine($"Ending cutscene: {cutscene.name}");
                                cutscene.EndCutScene();
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e}\n{cutscene.name}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                bossName = RandomizerStateManager.Instance.BossManager.GetActualBoss(bossName);
            }
            AdjustPlayerInBossRoom(bossName);
            return teleporting;
        }

        public static void TryCollectLocation(long locationID)
        {
            if (IDToBossMap.TryGetValue(locationID, out var boss))
            {
                SetBossAsDefeated(boss);
            }
        }

        protected RandoBossManager(Dictionary<string, string> bossMapping)
        {
            Manager<ProgressionManager>.Instance.bossesDefeated =
                Manager<ProgressionManager>.Instance.allTimeBossesDefeated = new List<string>();
            origToNewBoss = bossMapping;
        }

        private string GetActualBoss(string vanillaBoss)
        {
            Console.WriteLine($"requested {vanillaBoss}, going to {origToNewBoss[vanillaBoss]}");
            return origToNewBoss[vanillaBoss];
        }
    }
}