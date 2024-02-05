using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MessengerRando.Utils.Constants
{
    public static class LevelConstants
    {
        public readonly struct RandoLevel : IEquatable<RandoLevel>
        {
            public ELevel LevelName { get; }
            public Vector3 PlayerPos { get; }
            public EBits Dimension { get; }

            public RandoLevel(ELevel levelName, Vector3 playerPos)
            {
                LevelName = levelName;
                PlayerPos = playerPos;
                Dimension = EBits.NONE;
            }

            public RandoLevel(ELevel levelName, Vector3 playerPos, EBits dimension)
            {
                LevelName = levelName;
                PlayerPos = playerPos;
                Dimension = dimension;
            }

            public bool Equals(RandoLevel other)
            {
                return LevelName == other.LevelName && PlayerPos.Equals(other.PlayerPos);
            }

            public bool Equals(ELevel level)
            {
                return LevelName == level;
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

        public struct Transition : IEquatable<Transition>
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
                {
                    "Ninja Village - Right",
                    new RandoLevel(ELevel.Level_01_NinjaVillage, new Vector3(-153.32f, -57f))
                },
                {
                    "Autumn Hills - Left",
                    new RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(-304.7717f, -72.99999f))
                },
                {
                    "Autumn Hills - Right",
                    new RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(968.5283f, -27f))
                },
                {
                    "Autumn Hills - Bottom", new RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(679.6884f, -139f), EBits.BITS_16)
                }, // Must be 16
                {
                    "Forlorn Temple - Left",
                    new RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(-17.13942f, -11f))
                },
                {
                    "Forlorn Temple - Bottom",
                    new RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(36.87273f, -20.98116f), EBits.BITS_16)
                }, // Must be 16
                {
                    "Forlorn Temple - Right",
                    new RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(454.4301f, -11f))
                },
                {
                    "Catacombs - Top Left",
                    new RandoLevel(ELevel.Level_04_Catacombs, new Vector3())
                },
                {
                    "Catacombs - Right",
                    new RandoLevel(ELevel.Level_04_Catacombs, new Vector3(-177.0621f, -11f))
                },
                {
                    "Catacombs - Bottom",
                    new RandoLevel(ELevel.Level_04_Catacombs, new Vector3(511.0319f, -123f), EBits.BITS_16)
                }, // Must be 16
                {
                    "Catacombs - Bottom Left",
                    new RandoLevel(ELevel.Level_04_Catacombs, new Vector3(144.5119f, -59f), EBits.BITS_16)
                }, // Must be 16
                {
                    "Dark Cave - Right",
                    new RandoLevel(ELevel.Level_04_B_DarkCave, new Vector3(-4.059998f, -4f), EBits.BITS_16)
                }, // This needs to be forced to 16 bit to not soft lock the player
                // { "Dark Cave - Left", new RandoLevel(ELevel.Level_04_B_DarkCave, new Vector3()) }, // This may or may not be possible. This is a drop down into riviere in vanilla and you can't return from riviere
                {
                    "Riviere Turquoise - Portal",
                    new RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(837f, 13f))
                }, //TOT D
                {
                    "Riviere Turquoise - Right",
                    new RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(997.7f, 5.999998f), EBits.BITS_8)
                }, // This needs to be forced to 8 bit
                {
                    "Howling Grotto - Left",
                    new RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(-46.82001f, -11f))
                },
                {
                    "Howling Grotto - Right",
                    new RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(584.21f, -123f))
                }, // Emerald Golem is here. Need to either figure out teleporting out of boss rooms without fighting them or temporarily mark it as dead. Scene loaded gets called before the check, but level loaded is after 
                {
                    "Howling Grotto - Top",
                    new RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(419.95f, -58.66491f), EBits.BITS_16)
                }, // This needs to be forced to 16 bit
                {
                    "Howling Grotto - Bottom",
                    new RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(194.9271f, -151.665f), EBits.BITS_16)
                }, // needs to be 16
                {
                    "Sunken Shrine - Left",
                    new RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(-46.41f, -58.66499f), EBits.BITS_16)
                }, // This needs to be forced to 16 bit
                {
                    "Bamboo Creek - Top Left",
                    new RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(-50.09212f, 45f))
                },
                {
                    "Bamboo Creek - Bottom Left",
                    new RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(-177.0621f, -11f))
                },
                {
                    "Bamboo Creek - Right",
                    new RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(488.4079f, -91f))
                },
                {
                    "Quillshroom Marsh - Top Left",
                    new RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(-16.80286f, -11f))
                },
                {
                    "Quillshroom Marsh - Bottom Left",
                    new RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(598.1053f, -75.66499f), EBits.BITS_16)
                }, // This needs to be forced to 16 bit
                {
                    "Quillshroom Marsh - Top Right",
                    new RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(1160.79f, -43f))
                },
                {
                    "Quillshroom Marsh - Bottom Right",
                    new RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(841.3f, -121f), EBits.BITS_16)
                }, // 16
                {
                    "Searing Crags - Left",
                    new RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(-17.23438f, -27f))
                },
                {
                    "Searing Crags - Bottom",
                    new RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(303.3f, 39f), EBits.BITS_16)
                },
                {
                    "Searing Crags - Right",
                    new RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(521.8f, 71f), EBits.BITS_8)
                }, // Force 8 bit
                {
                    "Searing Crags - Top",
                    new RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(351.5f, 324.2888f))
                },
                {
                    "Glacial Peak - Bottom",
                    new RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(194.9839f, -517.7611f))
                },
                // { "Glacial Peak - Left", new RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(), ELevelEntranceID.NONE) }, // This is one way to ES so no actual entrance here
                {
                    "Glacial Peak - Top",
                    new RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(176.04f, -13.71676f))
                },
                {
                    "Elemental Skylands - Air Shmup",
                    new RandoLevel(ELevel.Level_09_B_ElementalSkylands, new Vector3(-37.4f, 359.5f), EBits.BITS_16)
                }, // 16 - This will probably be really difficult since it uses a different controller
                // {
                //     "Elemental Skylands - Air Shmup",
                //     new RandoLevel(ELevel.Level_09_B_ElementalSkylands, new Vector3(-35, 359), EBits.BITS_16)
                // },
                {
                    "Tower of Time - Left",
                    new RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(-18f, -11f), EBits.BITS_8)
                }, // 8
                {
                    "Cloud Ruins - Left",
                    new RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(-486f, -57f), EBits.BITS_16)
                },
                {
                    "Music Box - Left",
                    new RandoLevel(ELevel.Level_11_B_MusicBox, new Vector3(-428, -55))
                },
                {
                    "Underworld - Left",
                    new RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-337.5526f, -25f), EBits.BITS_8)
                }, // Force 8 bit
                // {
                //     "Underworld - Bottom Left",
                //     new RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-431.7626f, -57f), EBits.BITS_16)
                // }, // force 16 bit - one time and one way only - after defeating manfred - might be able to force it by calling ManfredBossOutroCutscene?
                {
                    "Corrupted Future",
                    new RandoLevel(ELevel.Level_14_CorruptedFuture, new Vector3(-19971.98f, -20011f), EBits.BITS_16)
                }, // 16
                {
                    "Ruxxtin Surfin'",
                    new RandoLevel(ELevel.Level_15_Surf, new Vector3(-472.191f, 472f), EBits.BITS_8)
                }, // 8
                {
                    "Beach - Left",
                    new RandoLevel(ELevel.Level_16_Beach, new Vector3(-431.7f, -9f))
                },
                {
                    "Fire Mountain - Bottom",
                    new RandoLevel(ELevel.Level_17_Volcano, new Vector3(-443.5f, -34f))
                },
                {
                    "Voodoo Heart - Left",
                    new RandoLevel(ELevel.Level_18_Volcano_Chase, new Vector3(-165.5f, 104.5551f), EBits.BITS_8)
                }, // 8
            };

        public static readonly Dictionary<Transition, string> TransitionToEntranceName =
            new Dictionary<Transition, string>
            {
                {
                    new Transition(ELevel.Level_02_AutumnHills, ELevel.Level_01_NinjaVillage),
                    "Ninja Village - Right"
                },
                {
                    new Transition(ELevel.Level_01_NinjaVillage, ELevel.Level_02_AutumnHills),
                    "Autumn Hills - Left"
                },
                {
                    new Transition(ELevel.Level_03_ForlornTemple, ELevel.Level_02_AutumnHills),
                    "Autumn Hills - Right"
                },
                {
                    new Transition(ELevel.Level_04_Catacombs, ELevel.Level_02_AutumnHills),
                    "Autumn Hills - Bottom"
                },
                {
                    new Transition(ELevel.Level_02_AutumnHills, ELevel.Level_03_ForlornTemple),
                    "Forlorn Temple - Left"
                },
                {
                    new Transition(ELevel.Level_04_Catacombs, ELevel.Level_03_ForlornTemple),
                    "Forlorn Temple - Bottom"
                },
                {
                    new Transition(ELevel.Level_06_A_BambooCreek, ELevel.Level_03_ForlornTemple),
                    "Forlorn Temple - Right"
                },
                {
                    new Transition(ELevel.Level_03_ForlornTemple, ELevel.Level_04_Catacombs),
                    "Catacombs - Top Left"
                },
                {
                    new Transition(ELevel.Level_02_AutumnHills, ELevel.Level_04_Catacombs),
                    "Catacombs - Bottom Left"
                },
                {
                    new Transition(ELevel.Level_04_B_DarkCave, ELevel.Level_04_Catacombs),
                    "Catacombs - Bottom"
                },
                {
                    new Transition(ELevel.Level_06_A_BambooCreek, ELevel.Level_04_Catacombs),
                    "Catacombs - Right"
                },
                {
                    new Transition(ELevel.Level_04_Catacombs, ELevel.Level_04_B_DarkCave),
                    "Dark Cave - Right"
                },
                {
                    new Transition(ELevel.Level_04_B_DarkCave, ELevel.Level_04_C_RiviereTurquoise),
                    "Riviere Turquoise - Right"
                },
                {
                    new Transition(ELevel.Level_04_Catacombs, ELevel.Level_06_A_BambooCreek),
                    "Bamboo Creek - Bottom Left"
                },
                {
                    new Transition(ELevel.Level_03_ForlornTemple, ELevel.Level_06_A_BambooCreek),
                    "Bamboo Creek - Top Left"
                },
                {
                    new Transition(ELevel.Level_05_A_HowlingGrotto, ELevel.Level_06_A_BambooCreek),
                    "Bamboo Creek - Right"
                },
                {
                    new Transition(ELevel.Level_06_A_BambooCreek, ELevel.Level_05_A_HowlingGrotto),
                    "Howling Grotto - Left"
                },
                {
                    new Transition(ELevel.Level_05_B_SunkenShrine, ELevel.Level_05_A_HowlingGrotto),
                    "Howling Grotto - Bottom"
                },
                {
                    new Transition(ELevel.Level_07_QuillshroomMarsh, ELevel.Level_05_A_HowlingGrotto),
                    "Howling Grotto - Right"
                }, // This could be either Top Right or Bottom Right. Probably need to check the position and see if it's within ~10 of one of the positions.
                {
                    new Transition(ELevel.Level_05_A_HowlingGrotto, ELevel.Level_07_QuillshroomMarsh),
                    "Quillshroom Marsh - Left"
                }, // Same issue as above
                {
                    new Transition(ELevel.Level_08_SearingCrags, ELevel.Level_07_QuillshroomMarsh),
                    "Quillshroom Marsh - Right"
                }, // same issue again
                {
                    new Transition(ELevel.Level_07_QuillshroomMarsh, ELevel.Level_08_SearingCrags),
                    "Searing Crags - Left"
                }, // again
                {
                    new Transition(ELevel.Level_09_A_GlacialPeak, ELevel.Level_08_SearingCrags),
                    "Searing Crags - Top"
                },
                {
                    new Transition(ELevel.Level_12_UnderWorld, ELevel.Level_08_SearingCrags),
                    "Searing Crags - Right"
                },
                {
                    new Transition(ELevel.Level_08_SearingCrags, ELevel.Level_09_A_GlacialPeak),
                    "Glacial Peak - Bottom"
                },
                {
                    new Transition(ELevel.Level_11_A_CloudRuins, ELevel.Level_09_A_GlacialPeak),
                    "Glacial Peak - Top"
                },
                {
                    new Transition(ELevel.Level_09_A_GlacialPeak, ELevel.Level_11_A_CloudRuins),
                    "Cloud Ruins - Left"
                },
                {
                    new Transition(ELevel.Level_08_SearingCrags, ELevel.Level_12_UnderWorld),
                    "Underworld - Left"
                },
                {
                    new Transition(ELevel.Level_11_A_CloudRuins, ELevel.Level_12_UnderWorld),
                    "Underworld - Bottom Left"
                }, // one way after beating manny
                {
                    new Transition(ELevel.Level_05_A_HowlingGrotto, ELevel.Level_05_B_SunkenShrine),
                    "Sunken Shrine - Left"
                }, // not sure if I can make these transitions require seashell
                {
                    new Transition(ELevel.Level_09_A_GlacialPeak, ELevel.Level_09_B_ElementalSkylands),
                    "Elemental Skylands - Air Shmup"
                },
                {
                    new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_11_B_MusicBox),
                    "Music Box - Left"
                },
                {
                    new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_02_AutumnHills),
                    "Autumn Hills - Portal"
                },
                {
                    new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_05_A_HowlingGrotto),
                    "Howling Grotto - Portal"
                },
                {
                    new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_08_SearingCrags),
                    "Searing Crags - Portal"
                },
                {
                    new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_09_A_GlacialPeak),
                    "Glacial Peak - Portal"
                },
                {
                    new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_04_C_RiviereTurquoise),
                    "Riviere Turquoise - Portal"
                },
                {
                    new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_05_B_SunkenShrine),
                    "Sunken Shrine - Portal"
                },
                {
                    new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_10_A_TowerOfTime),
                    "Tower of Time - Left"
                },
                {
                    new Transition(ELevel.Level_13_TowerOfTimeHQ, ELevel.Level_14_CorruptedFuture),
                    "Corrupted Future"
                },
                {
                    new Transition(ELevel.Level_01_NinjaVillage, ELevel.Level_15_Surf),
                    "Ruxxtin Surfin'"
                },
                {
                    new Transition(ELevel.Level_15_Surf, ELevel.Level_16_Beach),
                    "Beach - Left"
                },
                {
                    new Transition(ELevel.Level_16_Beach, ELevel.Level_17_Volcano),
                    "Fire Mountain - Bottom"
                },
                {
                    new Transition(ELevel.Level_17_Volcano, ELevel.Level_18_Volcano_Chase),
                    "Voodoo Heart - Left"
                },
            };

        public static Transition TransitionFromName(string name)
        {
            return TransitionToEntranceName.FirstOrDefault(x => x.Value == name).Key;
        }

        /// <summary>
        /// I'm doing the magic by determining where the player teleported from as that's definitely the easiest way to
        /// do this. These areas connect to each other twice so I need to use the player pos to determine which one
        /// we're actually using
        /// </summary>
        public static readonly List<string> SpecialEntranceNames = new List<string>
        {
            "Howling Grotto - Right", "Quillshroom Marsh - Left",
            "Quillshroom Marsh - Right", "Searing Crags - Left"
        };

        public static readonly List<string> TransitionNames = new List<string>
        {
            "Ninja Village - Right",
            "Autumn Hills - Left",
            "Autumn Hills - Right",
            "Autumn Hills - Bottom",
            "Forlorn Temple - Left",
            "Forlorn Temple - Bottom",
            "Forlorn Temple - Right",
            "Catacombs - Top Left",
            "Catacombs - Right",
            "Catacombs - Bottom",
            "Catacombs - Bottom Left",
            "Dark Cave - Right",
            "Dark Cave - Left",
            "Riviere Turquoise - Right",
            "Howling Grotto - Left",
            "Howling Grotto - Right",
            "Howling Grotto - Top",
            "Howling Grotto - Bottom",
            "Sunken Shrine - Left",
            "Bamboo Creek - Top Left",
            "Bamboo Creek - Bottom Left",
            "Bamboo Creek - Right",
            "Quillshroom Marsh - Top Left",
            "Quillshroom Marsh - Bottom Left",
            "Quillshroom Marsh - Top Right",
            "Quillshroom Marsh - Bottom Right",
            "Searing Crags - Left",
            "Searing Crags - Bottom",
            "Searing Crags - Right",
            "Searing Crags - Top",
            "Glacial Peak - Bottom",
            "Glacial Peak - Top",
            "Glacial Peak - Left",
            "Elemental Skylands - Air Shmup",
            "Elemental Skylands - Right",
            "Tower HQ",
            "Tower of Time - Left",
            "Corrupted Future",
            "Cloud Ruins - Left",
            "Underworld - Left",
        };
    }
}