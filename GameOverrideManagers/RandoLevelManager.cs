using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MessengerRando.GameOverrideManagers
{
    public class RandoLevelManager
    {
        public struct RandoLevel: IEquatable<RandoLevel>
        {
            public ELevel LevelName { get; }
            public Vector3 PlayerPos { get; }
            public RandoLevel(ELevel levelName, Vector3 playerPos)
            {
                LevelName = levelName;
                PlayerPos = playerPos;
            }

            public bool Equals(RandoLevel other)
            {
                return LevelName == other.LevelName && PlayerPos.Equals(other.PlayerPos);
            }

            public override bool Equals(object obj)
            {
                return obj is RandoLevel other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (int)LevelName;
                    hashCode = (hashCode * 397) ^ PlayerPos.GetHashCode();
                    return hashCode;
                }
            }
        }

        public struct Transition: IEquatable<Transition>
        {
            public ELevel Leaving { get; }
            public ELevel Entering { get; }
            public Transition(ELevel leaving, ELevel entering)
            {
                Leaving = leaving;
                Entering = entering;
            }

            public bool Equals(Transition other)
            {
                return Leaving == other.Leaving && Entering == other.Entering;
            }

            public override bool Equals(object obj)
            {
                return obj is Transition other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int)Leaving * 397) ^ (int)Entering;
                }
            }
        }
        
        public static readonly Dictionary<string, RandoLevel> EntranceNameToRandoLevel =
            new Dictionary<string, RandoLevel>
            {
                { "Ninja Village - Right", new RandoLevel(ELevel.Level_01_NinjaVillage, new Vector3(-153.32f, -57f)) },
                { "Autumn Hills - Portal", new RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(-20076.59f, -20004f)) },
                { "Autumn Hills - Left", new RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(-304.7717f, -72.99999f)) },
                { "Autumn Hills - Right", new RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(968.5283f, -27f)) },
                { "Autumn Hills - Bottom", new RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(679.6884f, -139f)) }, // Must be 16
                { "Forlorn Temple - Left", new RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(-17.13942f, -11f)) },
                { "Forlorn Temple - Bottom", new RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(36.87273f, -20.98116f)) }, // Must be 16
                { "Forlorn Temple - Right", new RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(454.4301f, -11f)) },
                { "Catacombs - Top Left", new RandoLevel(ELevel.Level_04_Catacombs, new Vector3()) },
                { "Catacombs - Right", new RandoLevel(ELevel.Level_04_Catacombs, new Vector3(-177.0621f, -11f)) },
                { "Catacombs - Bottom", new RandoLevel(ELevel.Level_04_Catacombs, new Vector3(511.0319f, -123f)) }, // Must be 1
                { "Catacombs - Bottom Left", new RandoLevel(ELevel.Level_04_Catacombs, new Vector3(144.5119f, -59f)) }, // Must be 16
                { "Dark Cave - Right", new RandoLevel(ELevel.Level_04_B_DarkCave, new Vector3(-4.059998f, -4f)) }, // This needs to be forced to 16 bit to not soft lock the player
                // { "Dark Cave - Left", new RandoLevel(ELevel.Level_04_B_DarkCave, new Vector3()) }, // This may or may not be possible. This is a drop down into riviere in vanilla and you can't return from riviere
                { "Riviere Turquoise - Portal", new RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(837f, 13f)) }, //TOT D
                { "Riviere Turquoise - Right", new RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(997.7f, 5.999998f)) }, // This needs to be forced to 8 bit
                { "Howling Grotto - Portal", new RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(241f, -196f)) }, //TOT G
                { "Howling Grotto - Left", new RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(-46.82001f, -11f)) },
                { "Howling Grotto - Right", new RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(584.21f, -123f)) }, // Emerald Golem is here. Need to either figure out teleporting out of boss rooms without fighting them or temporarily mark it as dead. Scene loaded gets called before the check, but level loaded is after 
                { "Howling Grotto - Top", new RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(419.95f, -58.66491f)) }, // This needs to be forced to 16 bit
                { "Howling Grotto - Bottom", new RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(194.9271f, -151.665f)) }, // needs to be 16
                { "Sunken Shrine - Portal", new RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(29f, -55f)) }, // 16
                { "Sunken Shrine - Left", new RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(-46.41f, -58.66499f)) }, // This needs to be forced to 16 bit
                { "Bamboo Creek - Top Left", new RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(-50.09212f, 45f)) },
                { "Bamboo Creek - Bottom Left", new RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(-177.0621f, -11f)) },
                { "Bamboo Creek - Right", new RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(488.4079f, -91f)) },
                { "Quillshroom Marsh - Top Left", new RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(-16.80286f, -11f)) },
                { "Quillshroom Marsh - Bottom Left", new RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(598.1053f, -75.66499f)) }, // This needs to be forced to 16 bit
                { "Quillshroom Marsh - Top Right", new RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(1160.79f, -43f)) },
                { "Quillshroom Marsh - Bottom Right", new RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(841.3f, -121f)) }, // 16
                { "Searing Crags - Portal", new RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(384.5f, 135f)) },
                { "Searing Crags - Left", new RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(-17.23438f, -27f)) },
                { "Searing Crags - Bottom", new RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(303.3f, 39f)) },
                { "Searing Crags - Right", new RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(521.8f, 71f)) }, // Force 8 bit
                { "Searing Crags - Top", new RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(351.5f, 324.2888f)) },
                { "Glacial Peak - Portal", new RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(220.6358f, -67f)) },
                { "Glacial Peak - Bottom", new RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(194.9839f, -517.7611f)) },
                // { "Glacial Peak - Left", new RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(), ELevelEntranceID.NONE) }, // This is one way to ES so no actual entrance here
                { "Glacial Peak - Top", new RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(176.04f, -13.71676f)) },
                { "Elemental Skylands - Left", new RandoLevel(ELevel.Level_09_B_ElementalSkylands, new Vector3(-424.9693f, 366.5677f)) }, // 16 - This will probably be really difficult since it uses a different controller
                { "Tower of Time - Left", new RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(-18f, -11f)) }, // 8
                { "Cloud Ruins - Left", new RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(-486f, -57f)) },
                { "Music Box - Left", new RandoLevel(ELevel.Level_11_B_MusicBox, new Vector3(-428, -55)) },
                { "Underworld - Top Left", new RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-337.5526f, -25f)) }, // Force 8 bit
                { "Underworld - Bottom Left", new RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-431.7626f, -57f)) }, // force 16 bit - one time and one way only - after defeating manfred - might be able to force it by calling ManfredBossOutroCutscene?
                { "Corrupted Future - Portal", new RandoLevel(ELevel.Level_14_CorruptedFuture, new Vector3(-19971.98f, -20011f)) }, // 16
                { "Ruxxtin Surfin'", new RandoLevel(ELevel.Level_15_Surf, new Vector3(-472.191f, 472f)) }, // 8
                { "Beach - Left", new RandoLevel(ELevel.Level_16_Beach, new Vector3(-431.7f, -9f)) },
                { "Fire Mountain - Bottom", new RandoLevel(ELevel.Level_17_Volcano, new Vector3(-443.5f, -34f)) },
                { "Voodoo Heart - Left", new RandoLevel(ELevel.Level_18_Volcano_Chase, new Vector3(-165.5f, 104.5551f)) }, // 8
            };

        public static readonly Dictionary<Transition, string> TransitionToEntranceName =
            new Dictionary<Transition, string>
            {
                {new Transition(ELevel.Level_02_AutumnHills, ELevel.Level_01_NinjaVillage), "Ninja Village - Right"},
                {new Transition(ELevel.Level_01_NinjaVillage, ELevel.Level_02_AutumnHills), "Autumn Hills - Left"},
                {new Transition(ELevel.Level_03_ForlornTemple, ELevel.Level_02_AutumnHills), "Autumn Hills - Right"},
                {new Transition(ELevel.Level_04_Catacombs, ELevel.Level_02_AutumnHills), "Autumn Hills - Bottom"},
                {new Transition(ELevel.Level_02_AutumnHills, ELevel.Level_03_ForlornTemple), "Forlorn Temple - Left"},
                {new Transition(ELevel.Level_04_Catacombs, ELevel.Level_03_ForlornTemple), "Forlorn Temple - Bottom"},
                {new Transition(ELevel.Level_06_A_BambooCreek, ELevel.Level_03_ForlornTemple), "Forlorn Temple - Right"},
                {new Transition(ELevel.Level_03_ForlornTemple, ELevel.Level_04_Catacombs), "Catacombs - Top Left"},
                {new Transition(ELevel.Level_02_AutumnHills, ELevel.Level_04_Catacombs), "Catacombs - Bottom Left"},
                {new Transition(ELevel.Level_04_B_DarkCave, ELevel.Level_04_Catacombs), "Catacombs - Bottom"},
                {new Transition(ELevel.Level_06_A_BambooCreek, ELevel.Level_04_Catacombs), "Catacombs - Right"},
                {new Transition(ELevel.Level_04_Catacombs, ELevel.Level_04_B_DarkCave), "Dark Cave - Right"},
                {new Transition(ELevel.Level_04_B_DarkCave, ELevel.Level_04_C_RiviereTurquoise), "Riviere Turquoise - Right"},
                {new Transition(ELevel.Level_04_Catacombs, ELevel.Level_06_A_BambooCreek), "Bamboo Creek - Bottom Left"},
                {new Transition(ELevel.Level_03_ForlornTemple, ELevel.Level_06_A_BambooCreek), "Bamboo Creek - Top Left"},
                {new Transition(ELevel.Level_05_A_HowlingGrotto, ELevel.Level_06_A_BambooCreek), "Bamboo Creek - Right"},
                {new Transition(ELevel.Level_06_A_BambooCreek, ELevel.Level_05_A_HowlingGrotto), "Howling Grotto - Top"},
                {new Transition(ELevel.Level_05_B_SunkenShrine, ELevel.Level_05_A_HowlingGrotto), "Howling Grotto - Left"},
                {new Transition(ELevel.Level_07_QuillshroomMarsh, ELevel.Level_05_A_HowlingGrotto), "Howling Grotto - Right"}, // This could be either Top Right or Bottom Right. Probably need to check the position and see if it's within ~10 of one of the positions.
                {new Transition(ELevel.Level_05_A_HowlingGrotto, ELevel.Level_07_QuillshroomMarsh), "Quillshroom Marsh - Left"}, // Same issue as above
                {new Transition(ELevel.Level_08_SearingCrags, ELevel.Level_07_QuillshroomMarsh), "Quillshroom Marsh - Right"}, // same issue again
                {new Transition(ELevel.Level_07_QuillshroomMarsh, ELevel.Level_08_SearingCrags), "Searing Crags - Left"}, // again
                {new Transition(ELevel.Level_09_A_GlacialPeak, ELevel.Level_08_SearingCrags), "Searing Crags - Top"},
                {new Transition(ELevel.Level_12_UnderWorld, ELevel.Level_08_SearingCrags), "Searing Crags - Right"},
                {new Transition(ELevel.Level_08_SearingCrags, ELevel.Level_09_A_GlacialPeak), "Glacial Peak - Bottom"},
                {new Transition(ELevel.Level_11_A_CloudRuins, ELevel.Level_09_A_GlacialPeak), "Glacial Peak - Top"},
                {new Transition(ELevel.Level_08_SearingCrags, ELevel.Level_12_UnderWorld), "Underworld - Top Left"},
                {new Transition(ELevel.Level_11_A_CloudRuins, ELevel.Level_12_UnderWorld), "Underworld - Bottom Left"}, // one way after beating manny
                {new Transition(ELevel.Level_05_A_HowlingGrotto, ELevel.Level_05_B_SunkenShrine), "Sunken Shrine - Left"}, // not sure if I can make these transitions require seashell
                {new Transition(ELevel.Level_09_A_GlacialPeak, ELevel.Level_09_B_ElementalSkylands), "Elemental Skylands - Left"},
                {new Transition(ELevel.Level_09_A_GlacialPeak, ELevel.Level_11_A_CloudRuins), "Cloud Ruins - Left"},
                {new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_11_B_MusicBox), "Music Box - Left"},
                {new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_02_AutumnHills), "Autumn Hills - Portal"},
                {new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_05_A_HowlingGrotto), "Howling Grotto - Portal"},
                {new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_08_SearingCrags), "Searing Crags - Portal"},
                {new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_09_A_GlacialPeak), "Glacial Peak - Portal"},
                {new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_04_C_RiviereTurquoise), "Riviere Turquoise - Portal"},
                {new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_05_B_SunkenShrine), "Sunken Shrine - Portal"},
                {new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_10_A_TowerOfTime), "Tower of Time - Left"},
                {new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_14_CorruptedFuture), "Corrupted Future - Portal"},
                {new Transition(ELevel.Level_01_NinjaVillage, ELevel.Level_15_Surf), "Ruxxtin Surfin'"},
                {new Transition(ELevel.Level_15_Surf, ELevel.Level_16_Beach), "Beach - Left"},
                {new Transition(ELevel.Level_16_Beach, ELevel.Level_17_Volcano), "Fire Mountain - Bottom"},
                {new Transition(ELevel.Level_17_Volcano, ELevel.Level_18_Volcano_Chase), "Voodoo Heart - Left"},
            };
        
        public static readonly List<string> SpecialCutscenes = new List<string>
        {
            "SunkenShrinePortalOpeningCutscene",
        };
        
        public static readonly List<string> PlayedSpecialCutscenes = new List<string>();
        public static readonly Dictionary<RandoLevel, RandoLevel> RandoLevelMapping = new Dictionary<RandoLevel, RandoLevel>
        {
            { EntranceNameToRandoLevel["Howling Grotto - Top"], EntranceNameToRandoLevel["Underworld - Top Left"] }
        };
        
        /// <summary>
        /// I'm doing the magic by determining where the player teleported from as that's definitely the easiest way to
        /// do this. These areas connect to each other twice so I need to use the player pos to determine which one
        /// we're actually using
        /// </summary>
        private static readonly List<string> SpecialEntranceNames = new List<string>
        {
            "Howling Grotto - Right", "Quillshroom Marsh - Left",
            "Quillshroom Marsh - Right", "Searing Crags - Left"
        };

        /// <summary>
        /// entrance that have to be in 16 bit or we get soft locked
        /// </summary>
        private static readonly List<string> Force16 = new List<string>
        {
            "Autumn Hills - Bottom",
            "Forlorn Temple - Bottom",
            "Catacombs - Bottom Left",
            "Catacombs - Bottom",
            "Dark Cave - Right",
            "Howling Grotto - Top",
            "Howling Grotto - Bottom",
            "Sunken Shrine - Left",
            "Quillshroom Marsh - Bottom Left",
            "Quillshroom Marsh - Bottom Right",
            "Searing Crags - Bottom Left",
            "Underworld - Bottom Left",
            "Elemental Skylands - Left",
            
            "Autumn Hills - Portal",
            "Riviere Turquoise - Portal",
            "Howling Grotto - Portal",
            "Sunken Shrine - Portal",
            "Searing Crags - Portal",
            "Glacial Peak - Portal",
            "Corrupted Future - Portal"
        };

        /// <summary>
        /// entrances that have to be in 8 bit or we get soft locked
        /// </summary>
        private static readonly List<string> Force8 = new List<string>
        {
            "Riviere Turquoise - Right",
            "Searing Crags - Right",
            "Tower of Time - Left",
            "Underworld - Top Left",
            "Ruxxtin Surfin'",
            "Voodoo Heart - Left",
        };

        private static bool teleportOverride;
        private static ELevel lastLevel;
        private static ELevel currentLevel;

        public static void LoadLevel(On.LevelManager.orig_LoadLevel orig, LevelManager self, LevelLoadingInfo levelInfo)
        {
            #if DEBUG
            Console.WriteLine($"Current Level: {Manager<LevelManager>.Instance.GetCurrentLevelEnum()}");
            Console.WriteLine($"Loading Level: {levelInfo.levelName}");
            Console.WriteLine($"Entrance ID: {levelInfo.levelEntranceId}, Dimension: {levelInfo.dimension}, Scene Mode: {levelInfo.loadSceneMode}");
            Console.WriteLine($"Position Player: {levelInfo.positionPlayer}, Show Transition: {levelInfo.showTransition}, Transition Type: {levelInfo.transitionType}");
            Console.WriteLine($"Pooled Level Instance: {levelInfo.pooledLevelInstance}, Show Intro: {levelInfo.showLevelIntro}, Close Transition On Level Loaded: {levelInfo.closeTransitionOnLevelLoaded}");
            Console.WriteLine($"Set Scene as Active Scene: {levelInfo.setSceneAsActiveScene}");
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

        private static RandoLevel FindEntrance(out string entrance)
        {
            try
            {
                currentLevel = Manager<LevelManager>.Instance.GetCurrentLevelEnum();
                Console.WriteLine($"Moving between levels: {lastLevel}, {currentLevel}");
                var playerPos = Manager<PlayerManager>.Instance.Player.transform.position;
                if (!TransitionToEntranceName.TryGetValue(new Transition(lastLevel, currentLevel), out entrance)) return new RandoLevel(ELevel.NONE, new Vector3());
                Console.WriteLine($"Transitioning from {entrance}");
                RandoLevel oldLevel = default;
                if (SpecialEntranceNames.Contains(entrance))
                {

                    Vector3 comparePos;
                    switch (entrance)
                    {
                        case "Howling Grotto - Right":
                            comparePos = EntranceNameToRandoLevel["Howling Grotto - Right"].PlayerPos;
                            if (WithinRange(playerPos.x, comparePos.x))
                            {
                                entrance = "Howling Grotto - Right";
                                oldLevel = EntranceNameToRandoLevel[entrance];
                            }
                            else
                            {
                                entrance = "Howling Grotto - Bottom";
                                oldLevel = EntranceNameToRandoLevel[entrance];
                            }

                            break;
                        case "Quillshroom Marsh - Left":
                            comparePos = EntranceNameToRandoLevel["Quillshroom Marsh - Top Left"].PlayerPos;
                            if (WithinRange(playerPos.x, comparePos.x))
                            {
                                entrance = "Quillshroom Marsh - Top Left";
                                oldLevel = EntranceNameToRandoLevel[entrance];
                            }
                            else
                            {
                                entrance = "Quillshroom Marsh - Bottom Left";
                                oldLevel = EntranceNameToRandoLevel[entrance];
                            }

                            break;
                        case "Quillshroom Marsh - Right":
                            comparePos = EntranceNameToRandoLevel["Quillshroom Marsh - Top Right"].PlayerPos;
                            if (WithinRange(playerPos.x, comparePos.x))
                            {
                                entrance = "Quillshroom Marsh - Top Right";
                                oldLevel = EntranceNameToRandoLevel[entrance];
                            }
                            else
                            {
                                entrance = "Quillshroom Marsh - Bottom Right";
                                oldLevel = EntranceNameToRandoLevel[entrance];
                            }

                            break;
                        case "Searing Crags - Left":
                            comparePos = EntranceNameToRandoLevel["Searing Crags - Left"].PlayerPos;
                            if (WithinRange(playerPos.x, comparePos.x))
                            {
                                entrance = "Searing Crags - Left";
                                oldLevel = EntranceNameToRandoLevel[entrance];
                            }
                            else
                            {
                                entrance = "Searing Crags - Bottom";
                                oldLevel = EntranceNameToRandoLevel[entrance];
                            }

                            break;
                    }
                }
                else EntranceNameToRandoLevel.TryGetValue(entrance, out oldLevel);
                
                return oldLevel;
            } catch (Exception e){ Console.WriteLine(e);}

            entrance = string.Empty;
            return new RandoLevel(ELevel.NONE, new Vector3());
        }

        public static void EndLevelLoading(On.LevelManager.orig_EndLevelLoading orig, LevelManager self)
        {
            #if DEBUG
            var playerPos = Manager<PlayerManager>.Instance.Player.transform.position;
            Console.WriteLine($"Finished loading into {Manager<LevelManager>.Instance.GetCurrentLevelEnum()}\n" +
                              $"From {Manager<LevelManager>.Instance.GetLevelEnumFromLevelName(Manager<LevelManager>.Instance.lastLevelLoaded)}\n" +
                              $"player position: {playerPos.x}f, {playerPos.y}f");
            #endif
            orig(self);
            if (teleportOverride || RandoLevelMapping == null)
            {
                teleportOverride = false;
                return;
            }
            Console.WriteLine("Attempting to teleport player");

            var oldLevel = FindEntrance(out var entrance);
            Console.WriteLine($"teleporting from {oldLevel.LevelName}, {entrance}");
            if (!RandoLevelMapping.TryGetValue(oldLevel, out var newLevel)) return;
            var actualEntrance = EntranceNameToRandoLevel.First(ret => ret.Value.Equals(newLevel)).Key;
            Console.WriteLine($"Teleporting to {newLevel.LevelName}");
            EBits newDimension;
            if (Force16.Contains(actualEntrance)) newDimension = EBits.BITS_16;
            else if (Force8.Contains(actualEntrance)) newDimension = EBits.BITS_8;
            else newDimension = Manager<DimensionManager>.Instance.currentDimension;
            if (newLevel.LevelName.Equals(ELevel.Level_11_B_MusicBox) && RandomizerStateManager.Instance.SkipMusicBox)
                SkipMusicBox();
            else TeleportInArea(newLevel.LevelName, newLevel.PlayerPos, newDimension);
        }

        public static void PortalIntoArea(On.TotHQLevelInitializer.orig_InitLevel orig, TotHQLevelInitializer self,
            Scene levelScene, ELevelEntranceID levelEntranceID, EBits dimension, bool positionPlayer,
            LevelInitializerParams levelInitParams)
        {
            Console.WriteLine("Initializing level from ToTHQ");
            Console.WriteLine($"Scene: {levelScene.name}, EntranceID: {levelEntranceID}, dimen: {dimension}");
            orig(self, levelScene, levelEntranceID, dimension, positionPlayer, levelInitParams);
        }
        
        public static void SkipMusicBox()
        {
            if (teleportOverride)
            {
                teleportOverride = false;
                return;
            }
            Manager<AudioManager>.Instance.StopMusic();
            var playerPosition = RandomizerStateManager.Instance.SkipMusicBox
                ? new Vector2(125, 40)
                : new Vector2(-428, -55);

            var playerDimension = RandomizerStateManager.Instance.SkipMusicBox ? EBits.BITS_8 : EBits.BITS_16;

            TeleportInArea(ELevel.Level_11_B_MusicBox, playerPosition, playerDimension);
        }

        public static void TeleportInArea(ELevel area, Vector2 position, EBits dimension)
        {
            if (teleportOverride)
            {
                teleportOverride = false;
                return;
            }
            Console.WriteLine($"Attempting to teleport to {area}, ({position.x}, {position.y}), {dimension}");
            Manager<AudioManager>.Instance.StopMusic();
            Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition = position;
            LevelLoadingInfo levelLoadingInfo = new LevelLoadingInfo(area + "_Build",
                true, true, LoadSceneMode.Single,
                ELevelEntranceID.NONE, dimension);
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
            var position = Manager<PlayerManager>.Instance.Player.transform.position;
            Console.WriteLine("Player position: " +
                              $"{position.x} " +
                              $"{position.y} " +
                              $"{position.z}");


            //This func checks if the new roomKey exists within levelRooms before changing and checks if currentRoom exists
            //if we're in a room, it leaves the current room then enters the new room with the teleported bool
            //no idea what the teleported bool does currently
            orig(self, newRoomLeftEdge, newRoomRightEdge, newRoomBottomEdge, newRoomTopEdge, teleportedInRoom);
            RandoBossManager.ShouldFightBoss(GetRoomKey());
        }
    }
}