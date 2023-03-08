using System;
using System.Collections.Generic;
using System.Linq;
using MessengerRando.Archipelago;
using MessengerRando.Utils.Constants;
using Object = UnityEngine.Object;

namespace MessengerRando.GameOverrideManagers
{
    public static class RandoBossManager
    {
        private static string currentBoss;
        private static readonly List<string> DefeatedBosses = new List<string>();
        public static Dictionary<string, string> OrigToNewBoss;
        private static bool bossOverride;
        public static bool FightingBoss;

        private static void AdjustPlayerInBossRoom(string bossName)
        {
            var newLocation = BossConstants.BossLocations[bossName];
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
                bossName = OrigToNewBoss.First(name => name.Value.Equals(bossName)).Key;
            Console.WriteLine($"Checking if {bossName} is defeated.");
            return !BossConstants.VanillaBossNames.Contains(bossName) || DefeatedBosses.Contains(bossName);
        }

        public static void SetBossAsDefeated(string bossName)
        {
            FightingBoss = false;
            if (bossOverride)
            {
                bossName = currentBoss;
                bossOverride = false;
            }
            if (ArchipelagoClient.HasConnected) ArchipelagoClient.ServerData.DefeatedBosses.Add(bossName);
            DefeatedBosses.Add(bossName);
            if (OrigToNewBoss != null)
            {
                var newPosition = BossConstants.BossLocations[bossName];
                
                RandoLevelManager.TeleportInArea(newPosition.BossRegion, newPosition.PlayerPosition,
                    newPosition.PlayerDimension);
            }
        }

        public static bool ShouldFightBoss(string bossName)
        {
            Console.WriteLine($"Entered {bossName}'s room. Has Defeated: {HasBossDefeated(bossName)}");
            if (HasBossDefeated(bossName)) return false;
            var teleporting = OrigToNewBoss != null;
            Console.WriteLine($"Should teleport: {teleporting}");
            if (teleporting)
            {
                currentBoss = bossName;
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