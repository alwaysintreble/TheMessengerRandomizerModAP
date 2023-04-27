using System;
using MessengerRando.Utils;
using System.Collections.Generic;
using System.Linq;
using MessengerRando.Utils.Constants;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MessengerRando.GameOverrideManagers
{
    public static class RandoLevelManager
    {
        private static bool teleporting;
        private static bool teleportOverride;
        private static ELevel lastLevel;
        private static ELevel currentLevel;
        public static readonly List<string> PlayedSpecialCutscenes = new List<string>();

        public static Dictionary<LevelConstants.RandoLevel, LevelConstants.RandoLevel> RandoLevelMapping;

        public static void LoadLevel(On.LevelManager.orig_LoadLevel orig, LevelManager self, LevelLoadingInfo levelInfo)
        {
            #if DEBUG
            Console.WriteLine($"Current Level: {Manager<LevelManager>.Instance.GetCurrentLevelEnum()}");
            Console.WriteLine($"Loading Level: {levelInfo.levelName}");
            Console.WriteLine($"Entrance ID: {levelInfo.levelEntranceId}, Dimension: {levelInfo.dimension}");
            #endif
            orig(self, levelInfo);
            if (teleportOverride || RandoLevelMapping == null) teleportOverride = false;
            else
            {
                lastLevel = Manager<LevelManager>.Instance.GetCurrentLevelEnum();
                currentLevel = Manager<LevelManager>.Instance.GetLevelEnumFromLevelName(levelInfo.levelName);
            }
        }
        
        static bool WithinRange(float pos1, float pos2)
        {
            var comparison = pos2 - pos1;
            if (comparison < 0) comparison *= -1;
            return comparison <= 10;
        }

        private static LevelConstants.RandoLevel FindEntrance(out string entrance)
        {
            try
            {
                currentLevel = Manager<LevelManager>.Instance.GetCurrentLevelEnum();
                var playerPos = Manager<PlayerManager>.Instance.Player.transform.position;
                if (!LevelConstants.TransitionToEntranceName.TryGetValue(
                        new LevelConstants.Transition(lastLevel, currentLevel), out entrance))
                    return new LevelConstants.RandoLevel(ELevel.NONE, new Vector3());
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

            entrance = string.Empty;
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
            if (teleportOverride || RandoLevelMapping == null)
            {
                teleportOverride = false;
                return;
            }

            var oldLevel = FindEntrance(out var entrance);
            if (!RandoLevelMapping.TryGetValue(oldLevel, out var newLevel)) return;
            var actualEntrance = LevelConstants.EntranceNameToRandoLevel
                .First(ret => ret.Value.Equals(newLevel)).Key;
            EBits newDimension;
            if (LevelConstants.Force16.Contains(actualEntrance)) newDimension = EBits.BITS_16;
            else if (LevelConstants.Force8.Contains(actualEntrance)) newDimension = EBits.BITS_8;
            else newDimension = Manager<DimensionManager>.Instance.currentDimension;
            
            if (newLevel.LevelName.Equals(ELevel.Level_11_B_MusicBox) && RandomizerStateManager.Instance.SkipMusicBox)
                SkipMusicBox();
            else TeleportInArea(newLevel.LevelName, newLevel.PlayerPos, newDimension);
        }

        public static void PortalIntoArea(On.TotHQLevelInitializer.orig_InitLevel orig, TotHQLevelInitializer self,
            Scene levelScene, ELevelEntranceID levelEntranceID, EBits dimension, bool positionPlayer,
            LevelInitializerParams levelInitParams)
        {
            orig(self, levelScene, levelEntranceID, dimension, positionPlayer, levelInitParams);
        }
        
        public static void SkipMusicBox()
        {
            if (teleporting)
            if (teleportOverride)
            {
                teleporting = false;
                teleportOverride = false;
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
            if (teleportOverride)
            {
                teleporting = false;
                teleportOverride = false;
                return;
            }
            Console.WriteLine($"Attempting to teleport to {area}, ({position.x}, {position.y}), {dimension}");
            Manager<AudioManager>.Instance.StopMusic();
            Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition = position;
            if (dimension.Equals(EBits.NONE)) dimension = Manager<DimensionManager>.Instance.currentDimension;
            LevelLoadingInfo levelLoadingInfo = new LevelLoadingInfo(area + "_Build",
                true, true, LoadSceneMode.Single,
                ELevelEntranceID.NONE, dimension);
            teleporting = true;
            teleportOverride = true;
            Manager<LevelManager>.Instance.LoadLevel(levelLoadingInfo);
        }


        public static void Level_ChangeRoom(On.Level.orig_ChangeRoom orig, Level self,
            ScreenEdge newRoomLeftEdge, ScreenEdge newRoomRightEdge,
            ScreenEdge newRoomBottomEdge, ScreenEdge newRoomTopEdge,
            bool teleportedInRoom)
        {
            string GetRoomKey()
            {
                return newRoomLeftEdge.edgeIdX + newRoomRightEdge.edgeIdX
                                               + newRoomBottomEdge.edgeIdY + newRoomTopEdge.edgeIdY;
            }
            #if DEBUG
            Console.WriteLine("new room params:" +
                              $"{newRoomLeftEdge.edgeIdX} " +
                              $"{newRoomRightEdge.edgeIdX} " +
                              $"{newRoomBottomEdge.edgeIdY} " +
                              $"{newRoomTopEdge.edgeIdY} ");
            Console.WriteLine($"new roomKey: {GetRoomKey()}");
            Console.WriteLine(self.CurrentRoom != null
                ? $"currentRoom roomKey: {self.CurrentRoom.roomKey}"
                : "currentRoom does not exist.");
            Console.WriteLine($"teleported: {teleportedInRoom}");
            #endif
            var position = Manager<PlayerManager>.Instance.Player.transform.position;
            #if DEBUG
            Console.WriteLine("Player position: " +
                              $"{position.x} " +
                              $"{position.y} " +
                              $"{position.z}");
            #endif


            //This func checks if the new roomKey exists within levelRooms before changing and checks if currentRoom exists
            //if we're in a room, it leaves the current room then enters the new room with the teleported bool
            //no idea what the teleported bool does currently
            orig(self, newRoomLeftEdge, newRoomRightEdge, newRoomBottomEdge, newRoomTopEdge, teleportedInRoom);
            RandoBossManager.ShouldFightBoss(GetRoomKey());
        }
    }
}