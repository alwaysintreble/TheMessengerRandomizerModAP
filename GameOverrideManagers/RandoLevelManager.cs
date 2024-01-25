using System;
using MessengerRando.Utils;
using System.Collections.Generic;
using MessengerRando.Utils.Constants;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MessengerRando.GameOverrideManagers
{
    public static class RandoLevelManager
    {
        private static bool teleporting;
        public static ELevel LastLevel;
        public static ELevel CurrentLevel;

        public static Dictionary<LevelConstants.RandoLevel, LevelConstants.RandoLevel> RandoLevelMapping;

        public static void LoadLevel(On.LevelManager.orig_LoadLevel orig, LevelManager self, LevelLoadingInfo levelInfo)
        {
            #if DEBUG
            Console.WriteLine($"Current Level: {Manager<LevelManager>.Instance.GetCurrentLevelEnum()}");
            Console.WriteLine($"Loading Level: {levelInfo.levelName}");
            Console.WriteLine($"Entrance ID: {levelInfo.levelEntranceId}, Dimension: {levelInfo.dimension}");
            #endif
            orig(self, levelInfo);
            if (teleporting) return;
            LastLevel = Manager<LevelManager>.Instance.GetCurrentLevelEnum();
            CurrentLevel = Manager<LevelManager>.Instance.GetLevelEnumFromLevelName(levelInfo.levelName);
        }

        static bool WithinRange(float pos1, float pos2)
        {
            var comparison = pos2 - pos1;
            if (comparison < 0) comparison *= -1;
            return comparison <= 10;
        }

        public static LevelConstants.RandoLevel FindEntrance()
        {
            try
            {
                Console.WriteLine("looking for entrance we just entered");
                var playerPos = Manager<PlayerManager>.Instance.Player.transform.position;
                Console.WriteLine(LastLevel);
                Console.WriteLine(CurrentLevel);
                if (!LevelConstants.TransitionToEntranceName.TryGetValue(
                        new LevelConstants.Transition(LastLevel, CurrentLevel), out var entrance))
                    return new LevelConstants.RandoLevel(ELevel.NONE, new Vector3());
                Console.WriteLine(entrance);
                if (entrance.Contains("Portal"))
                    return RandoPortalManager.PortalMapping == null
                        ? new LevelConstants.RandoLevel(ELevel.NONE, new Vector3())
                        : RandoPortalManager.GetPortalExit(entrance);
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
                                oldLevel = LevelConstants.EntranceNameToRandoLevel[entrance];
                            }
                            else
                            {
                                entrance = "Howling Grotto - Bottom";
                                oldLevel = LevelConstants.EntranceNameToRandoLevel[entrance];
                            }

                            break;
                        case "Quillshroom Marsh - Left":
                            comparePos = LevelConstants.EntranceNameToRandoLevel["Quillshroom Marsh - Top Left"].PlayerPos;
                            if (WithinRange(playerPos.x, comparePos.x))
                            {
                                entrance = "Quillshroom Marsh - Top Left";
                                oldLevel = LevelConstants.EntranceNameToRandoLevel[entrance];
                            }
                            else
                            {
                                entrance = "Quillshroom Marsh - Bottom Left";
                                oldLevel = LevelConstants.EntranceNameToRandoLevel[entrance];
                            }

                            break;
                        case "Quillshroom Marsh - Right":
                            comparePos = LevelConstants.EntranceNameToRandoLevel["Quillshroom Marsh - Top Right"].PlayerPos;
                            if (WithinRange(playerPos.x, comparePos.x))
                            {
                                entrance = "Quillshroom Marsh - Top Right";
                                oldLevel = LevelConstants.EntranceNameToRandoLevel[entrance];
                            }
                            else
                            {
                                entrance = "Quillshroom Marsh - Bottom Right";
                                oldLevel = LevelConstants.EntranceNameToRandoLevel[entrance];
                            }

                            break;
                        case "Searing Crags - Left":
                            comparePos = LevelConstants.EntranceNameToRandoLevel["Searing Crags - Left"].PlayerPos;
                            if (WithinRange(playerPos.x, comparePos.x))
                            {
                                entrance = "Searing Crags - Left";
                                oldLevel = LevelConstants.EntranceNameToRandoLevel[entrance];
                            }
                            else
                            {
                                entrance = "Searing Crags - Bottom";
                                oldLevel = LevelConstants.EntranceNameToRandoLevel[entrance];
                            }

                            break;
                    }
                }
                else LevelConstants.EntranceNameToRandoLevel.TryGetValue(entrance, out oldLevel);
                
                return oldLevel;
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
            if (teleporting)
            {
                teleporting = false;
                return;
            }
            
            if (Manager<LevelManager>.Instance.GetCurrentLevelEnum().Equals(ELevel.Level_11_B_MusicBox) &&
                RandomizerStateManager.Instance.SkipMusicBox)
            {
                SkipMusicBox();
                return;
            }
#if DEBUG
            Console.WriteLine("loaded into level...");
            Console.WriteLine(self.lastLevelLoaded);
            Console.WriteLine(self.GetCurrentLevelEnum());
            if (self.lastLevelLoaded.Equals(ELevel.Level_13_TowerOfTimeHQ + "_Build"))
            {
                // we just teleported into HQ
                
            }
            var oldLevel = FindEntrance();
            if (RandoLevelMapping != null && RandoLevelMapping.TryGetValue(oldLevel, out var newLevel))
                TeleportInArea(
                    newLevel.LevelName,
                    newLevel.PlayerPos,
                    newLevel.Dimension);
            else if (RandoPortalManager.PortalMapping != null && !oldLevel.Equals(ELevel.NONE))
            {
                TeleportInArea(
                    oldLevel.LevelName,
                    oldLevel.PlayerPos,
                    oldLevel.Dimension
                    );
            }
            // put the region we just loaded into in AP data storage for tracking
            if (!ArchipelagoClient.Authenticated || teleporting) return;
            if (self.lastLevelLoaded.Equals(ELevel.Level_13_TowerOfTimeHQ + "_Build"))
                ArchipelagoClient.Session.DataStorage[Scope.Slot, "CurrentRegion"] =
                    ELevel.Level_13_TowerOfTimeHQ.ToString();
            else
                ArchipelagoClient.Session.DataStorage[Scope.Slot, "CurrentRegion"] =
                    self.GetCurrentLevelEnum().ToString();
#endif
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