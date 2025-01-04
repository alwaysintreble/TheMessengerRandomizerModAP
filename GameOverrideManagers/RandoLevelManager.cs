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
        public static bool KillManfred;
        private static ELevel lastLevel;
        private static ELevel currentLevel;

        // ReSharper disable once UnassignedField.Global
        public static Dictionary<string, LevelConstants.RandoLevel> RandoLevelMapping;

        public static void LoadLevel(On.LevelManager.orig_LoadLevel orig, LevelManager self, LevelLoadingInfo levelInfo)
        {
            Console.WriteLine($"Current Level: {Manager<LevelManager>.Instance.GetCurrentLevelEnum()}");
            Console.WriteLine($"Loading Level: {levelInfo.levelName}");
            Console.WriteLine($"Entrance ID: {levelInfo.levelEntranceId}, Dimension: {levelInfo.dimension}");
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

        public static bool WithinRange(float pos1, float pos2)
        {
            Console.WriteLine($"comparing positions: {pos1}, {pos2}");
            var comparison = pos2 - pos1;
            if (comparison < 0) comparison *= -1;
            return comparison <= 50;
        }

        public static LevelConstants.RandoLevel FindEntrance()
        {
            try
            {
                Console.WriteLine("looking for entrance we just entered");
                var playerPos = Manager<PlayerManager>.Instance.Player.transform.position;
                Console.WriteLine(lastLevel);
                Console.WriteLine(currentLevel);
                
                if (RandoLevelMapping == null) return new LevelConstants.RandoLevel(ELevel.NONE, new Vector3());

                string entrance;
                if (currentLevel.Equals(ELevel.Level_14_CorruptedFuture))
                {
                    entrance = "Corrupted Future";
                }
                else if (RandoPortalManager.EnteredTower)
                {
                    entrance = "Tower of Time - Left";
                    RandoPortalManager.EnteredTower = false;
                }
                else if (!LevelConstants.TransitionToEntranceName.TryGetValue(
                        new LevelConstants.Transition(lastLevel, currentLevel), out entrance))
                    return new LevelConstants.RandoLevel(ELevel.NONE, new Vector3());
                
                if (LevelConstants.SpecialEntranceNames.Contains(entrance))
                {
                    Vector3 comparePos;
                    switch (entrance)
                    {
                        case "Howling Grotto - Right":
                            comparePos = LevelConstants.EntranceNameToRandoLevel["Howling Grotto - Right"].PlayerPos;
                            entrance = WithinRange(playerPos.x, comparePos.x)
                                ? "Howling Grotto - Right"
                                : "Howling Grotto - Top";

                            break;
                        case "Quillshroom Marsh - Left":
                            comparePos = LevelConstants.EntranceNameToRandoLevel["Quillshroom Marsh - Top Left"].PlayerPos;
                            entrance = WithinRange(playerPos.x, comparePos.x)
                                ? "Quillshroom Marsh - Top Left"
                                : "Quillshroom Marsh - Bottom Left";

                            break;
                        case "Quillshroom Marsh - Right":
                            comparePos = LevelConstants.EntranceNameToRandoLevel["Quillshroom Marsh - Top Right"].PlayerPos;
                            entrance = WithinRange(playerPos.x, comparePos.x)
                                ? "Quillshroom Marsh - Top Right"
                                : "Quillshroom Marsh - Bottom Right";

                            break;
                        case "Searing Crags - Left":
                            comparePos = LevelConstants.EntranceNameToRandoLevel["Searing Crags - Left"].PlayerPos;
                            entrance = WithinRange(playerPos.x, comparePos.x)
                                ? "Searing Crags - Left"
                                : "Searing Crags - Bottom";

                            break;
                    }
                }
                Console.WriteLine(entrance);
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

            // if (self.GetCurrentLevelEnum().Equals(ELevel.Level_05_A_HowlingGrotto))
            // {
            //     var progManager = Manager<ProgressionManager>.Instance;
                // progManager.levelsDiscovered.Remove(ELevel.Level_05_B_SunkenShrine);
                // progManager.allTimeDiscoveredLevels.Remove(ELevel.Level_05_B_SunkenShrine);
            // }
            if (teleporting)
            {
                teleporting = false;
                AddCurrentRegionToStorage(self);
                return;
            }
            if ((RandoPortalManager.PortalMapping is null || RandoPortalManager.PortalMapping.Count == 0)
                     && (RandoLevelMapping is null || RandoLevelMapping.Count == 0))
            {
                AddCurrentRegionToStorage(self);
            }
            
            if (currentLevel.Equals(ELevel.Level_11_B_MusicBox) &&
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
                return;
            }

            if (RandoPortalManager.LeftHQPortal)
            {
                RandoPortalManager.Teleport();
                return;
            }
            

            if (RandoLevelMapping is null || RandoLevelMapping.Count == 0)
            {
                AddCurrentRegionToStorage(self);
                return;
            }
            
            if (currentLevel.Equals(ELevel.Level_14_CorruptedFuture) ||
                currentLevel.Equals(ELevel.Level_10_A_TowerOfTime))
            {
                //have to be handled by the portal manager
                return;
            }
            // level transition shuffling
            var newLevel = FindEntrance();
            switch (newLevel.LevelName)
            {
                // case ELevel.Level_05_A_HowlingGrotto:
                //     LostWoodsManager.SolveLostWoods();
                //     break;
                case ELevel.Level_05_B_SunkenShrine:
                    LostWoodsManager.ShouldBeSolved = true;
                    LostWoodsManager.SolveLostWoods();
                    break;
            }
            if (RandoLevelMapping != null && !newLevel.LevelName.Equals(ELevel.NONE))
                TeleportInArea(
                    newLevel.LevelName,
                    newLevel.PlayerPos,
                    newLevel.Dimension);
        }

        private static void AddCurrentRegionToStorage(LevelManager self)
        {
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
            Manager<AudioManager>.Instance.StopMusic();
            var playerPosition = RandomizerStateManager.Instance.SkipMusicBox
                ? new Vector2(125, 40)
                : new Vector2(-428, -55);

            TeleportInArea(ELevel.Level_11_B_MusicBox, playerPosition, EBits.BITS_16);
        }

        public static void TeleportInArea(ELevel area, Vector2 position, EBits dimension = EBits.NONE)
        {
            #if DEBUG
            Console.WriteLine($"Attempting to teleport to {area}, ({position.x}, {position.y}), {dimension}");
            #endif
            CleanupBeforeTeleport();
            Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition = position;
            if (dimension.Equals(EBits.NONE)) dimension = Manager<DimensionManager>.Instance.currentDimension;
            LevelLoadingInfo levelLoadingInfo = new LevelLoadingInfo(area + "_Build",
                true, true, LoadSceneMode.Single,
                ELevelEntranceID.NONE, dimension);
            teleporting = true;
            Manager<LevelManager>.Instance.LoadLevel(levelLoadingInfo);
        }

        public static void TeleportInArea(LevelConstants.RandoLevel teleportLocation)
        {
            TeleportInArea(teleportLocation.LevelName, teleportLocation.PlayerPos, teleportLocation.Dimension);
        }

        public static void ElementalSkylandsInit(On.ElementalSkylandsLevelInitializer.orig_OnBeforeInitDone orig,
            ElementalSkylandsLevelInitializer self)
        {
            if (RandoPortalManager.PortalMapping != null && RandoLevelMapping != null)
                self.startOnManfred = !KillManfred && teleporting;
            else if (RandoPortalManager.PortalMapping != null)
                self.startOnManfred = !KillManfred;
            else if (RandoLevelMapping != null)
                self.startOnManfred = teleporting;
            orig(self);
            KillManfred = false;
        }

        public static void CleanupBeforeTeleport()
        {
            Manager<AudioManager>.Instance.StopMusic();
            Manager<PauseManager>.Instance.Resume();
            Manager<UIManager>.Instance.CloseAllScreensOfType<OptionScreen>(false);
            Manager<UIManager>.Instance.CloseAllScreensOfType<CinematicBordersScreen>(false);
            Manager<UIManager>.Instance.CloseAllScreensOfType<TransitionScreen>(false);
            Manager<UIManager>.Instance.CloseAllScreensOfType<SavingScreen>(false);
            Manager<UIManager>.Instance.CloseAllScreensOfType<LoadingAnimation>(false);
        }
    }
}