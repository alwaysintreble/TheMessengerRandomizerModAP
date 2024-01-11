using System.Collections.Generic;
using MessengerRando.Utils.Constants;
using UnityEngine;

namespace MessengerRando.GameOverrideManagers
{
    public static class RandoPortalManager
    {
        public readonly struct Portal
        {
            public readonly string Region;
            public readonly string PortalType;
            public readonly int Index;

            public Portal(string region, string portalType, int index)
            {
                Region = region;
                PortalType = portalType;
                Index = index;
            }
        }
        
        public static List<string> StartingPortals;
        public static Dictionary<string, Portal> PortalMapping;

        static readonly Dictionary<string, Dictionary<string, List<LevelConstants.RandoLevel>>> AreaCheckpoints =
            new Dictionary<string, Dictionary<string, List<LevelConstants.RandoLevel>>>
            {
                {
                    "AutumnHills", new Dictionary<string, List<LevelConstants.RandoLevel>>
                    {
                        {
                            "Portal", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(-20076.46f, -200004)),
                            }
                        },
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(-45.5f, -89)),
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(68, -111)),
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(407, -74)),
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(893, -27)),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(175, -148)),
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(92, -87)),
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(267.94f, -67)),
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(608, -35)),
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(718, -61), EBits.BITS_8),
                            }
                        }
                    }
                },
                {
                    "ForlornTemple", new Dictionary<string, List<LevelConstants.RandoLevel>>
                    {
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(1, -11)),
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(89, -10)),
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(252, 53)),
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(156, 85)),
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(272, 61)),
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(347, 31)),
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(355, -11)),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(125, 47)),
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(260, 24)),
                            }
                        }
                    }
                },
                {
                    "Catacombs", new Dictionary<string, List<LevelConstants.RandoLevel>>
                    {
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3(241, -25)),
                                new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3(732, -75)),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3(380, -23)),
                                new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3(530, -75)),
                                new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3(500, -43)),
                            }
                        }
                    }
                },
                {
                    "BambooCreek", new Dictionary<string, List<LevelConstants.RandoLevel>>
                    {
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(-29, -19)),
                                new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(92, 25)),
                                new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(380, 25)),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(227, -41)),
                                new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(210, 29)),
                            }
                        }
                    }
                },
                {
                    "HowlingGrotto", new Dictionary<string, List<LevelConstants.RandoLevel>>
                    {
                        {
                            "Portal", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(241f, -196f)),
                            }
                        },
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(26, -27)),
                                new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(310, -115)),
                                new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(542, -123)),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(138, -90)),
                                new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(439, -170)),
                            }
                        }
                    }
                },
                {
                    "QuillshroomMarsh", new Dictionary<string, List<LevelConstants.RandoLevel>>
                    {
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(194, -37)),
                                new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(663, -27)),
                                new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(1085, -45)),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(161, -54)),
                                new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(410, -42)),
                                new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(916, -26)),
                            }
                        }
                    }
                },
                {
                    "SearingCrags", new Dictionary<string, List<LevelConstants.RandoLevel>>
                    {
                        {
                            "Portal", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(384.5f, 135f)),
                            }
                        },
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(61, -27)),
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(147, 69)),
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(226, 151)),
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(282, 23)),
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(380, 309)),
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(120, 248), EBits.BITS_8),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(109, 63)),
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(296, 189), EBits.BITS_8),
                            }
                        }
                    }
                },
                {
                    "GlacialPeak", new Dictionary<string, List<LevelConstants.RandoLevel>>
                    {
                        {
                            "Portal", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(220.6358f, -67f)),
                            }
                        },
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(216, -456)),
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(260, -297)),
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(156, -27)),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(227, -405)),
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(251, -235)),
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(195, -131)),
                            }
                        }
                    }
                },
                {
                    "TowerOfTime", new Dictionary<string, List<LevelConstants.RandoLevel>>
                    {
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(71, -11), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(84, 237), EBits.BITS_8),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(29, 21), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(5, 37), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(50, 77), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(57, 85), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(31, 133), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(58, 165), EBits.BITS_8),
                            }
                        }
                    }
                },
                {
                    "CloudRuins", new Dictionary<string, List<LevelConstants.RandoLevel>>
                    {
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(-368, -26), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(-116, -35), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(108, -24)),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(355, -27), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(721, -22), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(816, -26), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(1148, -27), EBits.BITS_8),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(-146, 24), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(159, -29)),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(780, -26)),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(-251, 13)),
                            }
                        }
                    }
                },
                {
                    "Underworld", new Dictionary<string, List<LevelConstants.RandoLevel>>
                    {
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-305, -51), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-186, -24), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-125, -91), EBits.BITS_8),
                                //new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(124, -43)), // barm'athaziel which isn't accessible in second quest
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(132, -130)),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-225, -89), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(1, 72), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-110, -98), EBits.BITS_8),
                            }
                        }
                    }
                },
                {
                    "RiviereTurquoise", new Dictionary<string, List<LevelConstants.RandoLevel>>
                    {
                        {
                            "Portal", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(837, 13)),
                            }
                        },
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(804, -40)),
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(499, -132), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(337, -131)),
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(149, -89)),
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(-8, 13)),
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(-259, 7)),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(648, -83), EBits.BITS_8),
                            }
                        }
                    }
                },
                {
                    "SunkenShrine", new Dictionary<string, List<LevelConstants.RandoLevel>>
                    {
                        {
                            "Portal", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(29f, -55f)),
                            }
                        },
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(36, -41)),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(186, -9), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(8, -65), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(-102, -121), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(100, -81), EBits.BITS_8),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(76, -16), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(30, -87), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(166, -178), EBits.BITS_8),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(92, -100), EBits.BITS_8),
                            }
                        }
                    }
                }
            };
        
        public static LevelConstants.RandoLevel GetPortalExit(string enteredPortal)
        {
            var portalExit = PortalMapping[enteredPortal];
            return AreaCheckpoints[portalExit.Region][portalExit.PortalType][portalExit.Index];
        }
    }
}