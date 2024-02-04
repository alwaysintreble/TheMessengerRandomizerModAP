using System;
using MessengerRando.Utils;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using MessengerRando.Archipelago;
using MessengerRando.Utils.Constants;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MessengerRando.GameOverrideManagers
{
    public static class RandoLevelManager
    {
        private static bool teleporting;
        private static ELevel lastLevel;
        private static ELevel currentLevel;

        // ReSharper disable once UnassignedField.Global
        public static Dictionary<string, LevelConstants.RandoLevel> RandoLevelMapping;

        public static void LoadLevel(On.LevelManager.orig_LoadLevel orig, LevelManager self, LevelLoadingInfo levelInfo)
        {
            #if DEBUG
            Console.WriteLine($"Current Level: {Manager<LevelManager>.Instance.GetCurrentLevelEnum()}");
            Console.WriteLine($"Loading Level: {levelInfo.levelName}");
            Console.WriteLine($"Entrance ID: {levelInfo.levelEntranceId}, Dimension: {levelInfo.dimension}");
            #endif
            try
            {
                if (!teleporting)
                {
                    lastLevel = Manager<LevelManager>.Instance.GetCurrentLevelEnum();
                    var levelName = levelInfo.levelName.Contains("_Build")
                        ? levelInfo.levelName.Replace("_Build", "")
                        : levelInfo.levelName;
                    currentLevel = Manager<LevelManager>.Instance.GetLevelEnumFromLevelName(levelName);
                }
                Console.WriteLine(lastLevel);
                Console.WriteLine(currentLevel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            orig(self, levelInfo);
        }

        static bool WithinRange(float pos1, float pos2)
        {
            var comparison = pos2 - pos1;
            if (comparison < 0) comparison *= -1;
            return comparison <= 10;
        }

        private static LevelConstants.RandoLevel FindEntrance()
        {
            try
            {
                Console.WriteLine("looking for entrance we just entered");
                var playerPos = Manager<PlayerManager>.Instance.Player.transform.position;
                Console.WriteLine(lastLevel);
                Console.WriteLine(currentLevel);
                
                if (RandoLevelMapping == null) return new LevelConstants.RandoLevel(ELevel.NONE, new Vector3());
                
                if (!LevelConstants.TransitionToEntranceName.TryGetValue(
                        new LevelConstants.Transition(lastLevel, currentLevel), out var entrance))
                    return new LevelConstants.RandoLevel(ELevel.NONE, new Vector3());
                Console.WriteLine(entrance);
                
                LevelConstants.RandoLevel oldLevel = default;
                if (LevelConstants.SpecialEntranceNames.Contains(entrance))
                {
                    Vector3 comparePos;
                    switch (entrance)
                    {
                        case "Howling Grotto - Right":
                            comparePos = LevelConstants.EntranceNameToRandoLevel["Howling Grotto - Right"].PlayerPos;
                            if (WithinRange(playerPos.x, comparePos.x))
                            {
                                entrance = "Howling Grotto - Right";
                            }
                            else
                            {
                                entrance = "Howling Grotto - Bottom";
                            }

                            break;
                        case "Quillshroom Marsh - Left":
                            comparePos = LevelConstants.EntranceNameToRandoLevel["Quillshroom Marsh - Top Left"].PlayerPos;
                            if (WithinRange(playerPos.x, comparePos.x))
                            {
                                entrance = "Quillshroom Marsh - Top Left";
                            }
                            else
                            {
                                entrance = "Quillshroom Marsh - Bottom Left";
                            }

                            break;
                        case "Quillshroom Marsh - Right":
                            comparePos = LevelConstants.EntranceNameToRandoLevel["Quillshroom Marsh - Top Right"].PlayerPos;
                            if (WithinRange(playerPos.x, comparePos.x))
                            {
                                entrance = "Quillshroom Marsh - Top Right";
                            }
                            else
                            {
                                entrance = "Quillshroom Marsh - Bottom Right";
                            }

                            break;
                        case "Searing Crags - Left":
                            comparePos = LevelConstants.EntranceNameToRandoLevel["Searing Crags - Left"].PlayerPos;
                            if (WithinRange(playerPos.x, comparePos.x))
                            {
                                entrance = "Searing Crags - Left";
                            }
                            else
                            {
                                entrance = "Searing Crags - Bottom";
                            }

                            break;
                    }
                }

                return RandoLevelMapping[entrance];
            } catch (Exception e){ Console.WriteLine(e);}
            return new LevelConstants.RandoLevel(ELevel.NONE, new Vector3());
        }

        public static void EndLevelLoading(On.LevelManager.orig_EndLevelLoading orig, LevelManager self)
        {
            orig(self);
            // i haven't figured out any way to teleport the player between boss rooms yet still
            // this check is specifically for emerald golem since that boss exists on a transition screen
            // if (RandoBossManager.OrigToNewBoss != null &&
            //     RandoRoomManager.IsBossRoom(Manager<Level>.Instance.CurrentRoom.roomKey, out var bossName))
            // {
            //     if (RandoBossManager.OrigToNewBoss.TryGetValue(bossName, out bossName))
            //     {
            //         try
            //         {
            //             RandoBossManager.AdjustPlayerInBossRoom(bossName);
            //         }
            //         catch (Exception e)
            //         {
            //             Console.WriteLine(e);
            //             throw;
            //         }
            //         return;
            //     }
            // }
            if (self.GetCurrentLevelEnum().Equals(ELevel.Level_05_A_HowlingGrotto))
            {
                var progManager = Manager<ProgressionManager>.Instance;
                progManager.levelsDiscovered.Remove(ELevel.Level_05_B_SunkenShrine);
                progManager.allTimeDiscoveredLevels.Remove(ELevel.Level_05_B_SunkenShrine);
            }
            if (teleporting)
            {
                teleporting = false;
                // put the region we just loaded into in AP data storage for tracking
                if (ArchipelagoClient.Authenticated)
                {
                    if (self.lastLevelLoaded.Equals(ELevel.Level_13_TowerOfTimeHQ + "_Build"))
                        ArchipelagoClient.Session.DataStorage[Scope.Slot, "CurrentRegion"] =
                            ELevel.Level_13_TowerOfTimeHQ.ToString();
                    else
                        ArchipelagoClient.Session.DataStorage[Scope.Slot, "CurrentRegion"] =
                            self.GetCurrentLevelEnum().ToString();
                    return;
                }
            }
            
            if (Manager<LevelManager>.Instance.GetCurrentLevelEnum().Equals(ELevel.Level_11_B_MusicBox) &&
                RandomizerStateManager.Instance.SkipMusicBox)
            {
                SkipMusicBox();
                return;
            }
            
            Console.WriteLine("loaded into level...");
            Console.WriteLine(self.lastLevelLoaded);
            Console.WriteLine(self.GetCurrentLevelEnum());
            if (self.lastLevelLoaded.Equals(ELevel.Level_13_TowerOfTimeHQ + "_Build"))
            {
                // we just teleported into HQ
                
            }
            var newLevel = FindEntrance();
            if (RandoLevelMapping != null && !newLevel.LevelName.Equals(ELevel.NONE))
                TeleportInArea(
                    newLevel.LevelName,
                    newLevel.PlayerPos,
                    newLevel.Dimension);

            if (!ArchipelagoClient.Authenticated) return;
            // put the region we just loaded into in AP data storage for tracking
            if (self.lastLevelLoaded.Equals(ELevel.Level_13_TowerOfTimeHQ + "_Build"))
                ArchipelagoClient.Session.DataStorage[Scope.Slot, "CurrentRegion"] =
                    ELevel.Level_13_TowerOfTimeHQ.ToString();
            else
                ArchipelagoClient.Session.DataStorage[Scope.Slot, "CurrentRegion"] =
                    self.GetCurrentLevelEnum().ToString();
        }
        
        public static void SkipMusicBox()
        {
            #if DEBUG
            Console.WriteLine($"attempting to skip music box. already teleporting : {teleporting}");
            #endif
            if (teleporting)
            {
                teleporting = false;
                return;
            }
            Manager<AudioManager>.Instance.StopMusic();
            var playerPosition = RandomizerStateManager.Instance.SkipMusicBox
                ? new Vector2(125, 40)
                : new Vector2(-428, -55);

            TeleportInArea(ELevel.Level_11_B_MusicBox, playerPosition, EBits.BITS_16);
        }

        public static void TeleportInArea(ELevel area, Vector2 position, EBits dimension = EBits.NONE)
        {
            if (teleporting)
            {
                teleporting = false;
                return;
            }
            #if DEBUG
            Console.WriteLine($"Attempting to teleport to {area}, ({position.x}, {position.y}), {dimension}");
            #endif
            Manager<AudioManager>.Instance.StopMusic();
            Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition = position;
            if (dimension.Equals(EBits.NONE)) dimension = Manager<DimensionManager>.Instance.currentDimension;
            LevelLoadingInfo levelLoadingInfo = new LevelLoadingInfo(area + "_Build",
                true, true, LoadSceneMode.Single,
                ELevelEntranceID.NONE, dimension);
            teleporting = true;
            Manager<LevelManager>.Instance.LoadLevel(levelLoadingInfo);
        }
    }
}