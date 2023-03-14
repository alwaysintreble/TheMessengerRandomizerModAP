using System;
using System.Collections.Generic;
using System.Linq;
using MessengerRando.Archipelago;
using MessengerRando.Utils.Constants;
using WebSocketSharp;
using Object = UnityEngine.Object;

namespace MessengerRando.GameOverrideManagers
{
    public static class RandoBossManager
    {
        private static readonly List<string> DefeatedBosses = new List<string>();
        public static Dictionary<string, string> OrigToNewBoss;

        public static bool FightingBoss;
        public static string TempBossOverride;

        public static void AdjustPlayerInBossRoom(string bossName)
        {
            var newLocation = BossConstants.BossLocations[bossName];
            if (Manager<LevelManager>.Instance.GetCurrentLevelEnum().Equals(newLocation.BossRegion))
            {
                Manager<PlayerManager>.Instance.Player.transform.position = newLocation.PlayerPosition;
                Manager<DimensionManager>.Instance.SetDimension(newLocation.PlayerDimension);
            }
            else
            {
                RandoLevelManager.TeleportInArea(newLocation.BossRegion, newLocation.PlayerPosition,
                    newLocation.PlayerDimension);
            }
        }
        
        public static bool HasBossDefeated(string bossName)
        {
            Console.WriteLine($"Checking if {bossName} is defeated.");
            return true;
            bool result;
            // if we currently have a boss override we want to change rooms so return true if the current boss == override
            if (!TempBossOverride.IsNullOrEmpty()) result = bossName == TempBossOverride;
            // boss rando but it isn't currently overriden. override it and get the new boss name if the new boss
            // hasn't been defeated
            else if (OrigToNewBoss != null)
            {
                if (OrigToNewBoss.TryGetValue(bossName, out var newBossName))
                {
                    Console.WriteLine($"Boss is shuffled. should be false");
                    TempBossOverride = bossName;
                    result = false;
                }
                else result = true;
            }
            // don't fight emerald golem when we're on entrance shuffle. might change this so that you have to fight
            // him when entering from the left side specifically, just need to verify order of operations
            else result = (RandoLevelManager.RandoLevelMapping != null && bossName == "EmeraldGolem") || !BossConstants.VanillaBossNames.Contains(bossName) ||
                          DefeatedBosses.Contains(bossName);
            Console.WriteLine($"returning {result}");
            return result;
        }

        public static void SetBossAsDefeated(string bossName)
        {
            if (!TempBossOverride.IsNullOrEmpty())
            {
                bossName = TempBossOverride;
                OrigToNewBoss.Remove(bossName); // doing this so we can probably force fighting the same boss multiple times?
                TempBossOverride = String.Empty;
            }
            if (ArchipelagoClient.HasConnected) ArchipelagoClient.ServerData.DefeatedBosses.Add(bossName);
            DefeatedBosses.Add(bossName);
            if (OrigToNewBoss != null)
            {
                var newPosition = BossConstants.BossLocations[bossName];

                FightingBoss = false;
                RandoLevelManager.TeleportInArea(newPosition.BossRegion, newPosition.PlayerPosition,
                    newPosition.PlayerDimension);
            }
        }

        public static bool ShouldPlayBossCutscene(string currentScene)
        {
            return OrigToNewBoss != null && !BossConstants.BossCutscenes.Values.Contains(currentScene);
        }

        public static bool ShouldFightBoss(string bossName)
        {
            if (HasBossDefeated(bossName) && TempBossOverride.IsNullOrEmpty()) return false;
            var teleporting = OrigToNewBoss != null;
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
                bossName = OrigToNewBoss[bossName];
            }
            AdjustPlayerInBossRoom(bossName);
            FightingBoss = true;
            return teleporting;
        }
    }
}