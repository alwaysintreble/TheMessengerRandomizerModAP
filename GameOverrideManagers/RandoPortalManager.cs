using System;
using System.Collections.Generic;
using MessengerRando.Utils.Constants;
using UnityEngine;

namespace MessengerRando.GameOverrideManagers
{
    public static class RandoPortalManager
    {
        public readonly struct Portal
        {
            public readonly int Region;
            public readonly int PortalType;
            public readonly int Index;

            public Portal(int portalWarp)
            {
                var modWarp = portalWarp.ToString();
                if (modWarp.Length == 4)
                {
                    Region = int.Parse(modWarp.Substring(0, 2));
                    PortalType = int.Parse(modWarp[2].ToString());
                    Index = int.Parse(modWarp[3].ToString());
                }
                else if (modWarp.Length == 3)
                {
                    Region = int.Parse(modWarp[0].ToString());
                    PortalType = int.Parse(modWarp[1].ToString());
                    Index = int.Parse(modWarp[2].ToString());
                }
                else if (modWarp.Length == 2)
                {
                    Region = 0;
                    PortalType = int.Parse(modWarp[0].ToString());
                    Index = int.Parse(modWarp[1].ToString());
                }
                else
                {
                    Region = 0;
                    PortalType = 0;
                    Index = int.Parse(modWarp);
                }
            }
        }

        public static bool LeftHQPortal;
        public static bool EnteredTower;
        public static List<string> StartingPortals;
        public static List<Portal> PortalMapping;

        static readonly List<List<List<LevelConstants.RandoLevel>>> AreaCheckpoints =
            new List<List<List<LevelConstants.RandoLevel>>>
            {
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(-20076.46f, -200004)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(-45.5f, -89)),
                        new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(68.5f, -111)),
                        new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(407.5f, -74)),
                        new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(892.5f, -27)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(175, -148)),
                        new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(91.5f, -87)),
                        new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(238.5f, -74)),
                        new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(607.48f, -35)),
                        new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(718.5f, -73), EBits.BITS_8),
                    }
                },
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>(),
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(0.5f, -11)),
                        new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(88.5f, -10)),
                        new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(251.5f, 53)),
                        new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(156, 85)),
                        new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(271.5f, 61)),
                        new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(347.5f, 31)),
                        new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(354.5f, -11)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(124.5f, 47)),
                        new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3(260.5f, 24)),
                    }
                },
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>(),
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3(241.5f, -25)),
                        new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3(731.5f, -75)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3(379.5f, -23)),
                        new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3(5329.5f, -75), EBits.BITS_16),
                        new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3(499.5f, -43)),
                    }
                },
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>(),
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(-28.5f, -19)),
                        new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(92.5f, 25)),
                        new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(379.5f, 25)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(227.5f, -41)),
                        new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3(210.5f, 29)),
                    }
                },
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(241f, -196f)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(26.5f, -27)),
                        new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(310.5f, -115)),
                        new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(541.5f, -123)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(138.5f, -90)),
                        new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(439, -170)),
                    }
                },
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>(),
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(193.5f, -37)),
                        new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(663.5f, -27)),
                        new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(1085.5f, -45)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(161.5f, -54)),
                        new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(409.5f, -42)),
                        new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(916.5f, -26)),
                    }
                },
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(384.5f, 135f)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(61, -27)),
                        new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(147.5f, 69)),
                        new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(226.5f, 151)),
                        new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(282, 23)),
                        new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(380.5f, 309)),
                        new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(119.5f, 248), EBits.BITS_8),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(109.5f, 63)),
                        new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(296.5f, 189f), EBits.BITS_8),
                    }
                },
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(220.6f, 66.5f)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(216.5f, -456)),
                        new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(259.5f, -297)),
                        new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(156.5f, -27)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(227.5f, -405)),
                        new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(251.5f, -235)),
                        new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(195.5f, -131)),
                    }
                },
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>(),
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(71.5f, -11), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(84.5f, 237), EBits.BITS_8),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(38.5f, 21.5f), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(5.5f, 37), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(50.5f, 77), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(57.5f, 85), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(31.5f, 133), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3(58.5f, 165), EBits.BITS_8),
                    }
                },
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>(),
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(-368.5f, -26), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(-140.5f, -26), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(116.5f, -25)),
                        new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(366.5f, -27), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(721.5f, -22), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(816.5f, -26), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(1148.5f, -27), EBits.BITS_8),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(-146, 24), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(164, -21)),
                        new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(769.5f, -26)),
                        new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3(-251, 13)),
                    }
                },
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>(),
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-305, -51), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-186.5f, -24), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-125.5f, -91), EBits.BITS_8),
                        //new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(124.5f, -43)), // barm'athazel which isn't accessible in second quest
                        new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(132.5f, -130)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-226.5f, -89), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(0.5f, 72), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3(-110.5f, -98), EBits.BITS_8),
                    }
                },
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(837, 13.5f)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(804.5f, -40)),
                        new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(499, -132), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(337.5f, -131)),
                        new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(149.5f, -89)),
                        new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(-8.5f, 13)),
                        new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(-259, 7)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3(648.5f, -83), EBits.BITS_8),
                    }
                },
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>(),
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_09_B_ElementalSkylands, new Vector3(-35, 359)),
                        new LevelConstants.RandoLevel(ELevel.Level_09_B_ElementalSkylands, new Vector3(87.5f, 407)),
                        new LevelConstants.RandoLevel(ELevel.Level_09_B_ElementalSkylands, new Vector3(864.5f, 381)),
                        new LevelConstants.RandoLevel(ELevel.Level_09_B_ElementalSkylands, new Vector3(966.3f, 409.4f)),
                        new LevelConstants.RandoLevel(ELevel.Level_09_B_ElementalSkylands, new Vector3(1763.5f, 381)),
                        new LevelConstants.RandoLevel(ELevel.Level_09_B_ElementalSkylands, new Vector3(1909, 411)),
                        new LevelConstants.RandoLevel(ELevel.Level_09_B_ElementalSkylands, new Vector3(2755.5f, 376), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_09_B_ElementalSkylands, new Vector3(2926.5f, 406)),
                        new LevelConstants.RandoLevel(ELevel.Level_09_B_ElementalSkylands, new Vector3(-22.5f, 417)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_09_B_ElementalSkylands, new Vector3(-22.5f, 417)),
                    }
                },
                new List<List<LevelConstants.RandoLevel>>
                {
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(29f, -55f)),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(36.5f, -41)),
                        new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(186.5f, -9), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(8, -65), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(-102.5f, -121), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(100, -81), EBits.BITS_8),
                    },
                    new List<LevelConstants.RandoLevel>
                    {
                        new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(53.5f, -25), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(29.5f, -87), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(166, -178), EBits.BITS_8),
                        new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(92.5f, -100), EBits.BITS_8),
                    }
                }
            };

        private static readonly List<string> Portals = new List<string>
        {
            "Autumn Hills - Portal",
            "Riviere Turquoise - Portal",
            "Howling Grotto - Portal",
            "Sunken Shrine - Portal",
            "Searing Crags - Portal",
            "Glacial Peak - Portal",
        };

        private static readonly List<string> AccessedStartingPortals = new List<string>();
        public static bool ShouldPortalBeOpen(string portal)
        {
            return AccessedStartingPortals.Contains(portal);
        }

        public static void OpenPortalEvent(On.PortalOpeningCutscene.orig_OnOpenPortalEvent orig,
            PortalOpeningCutscene self, string eventid)
        {
            switch (eventid)
            {
                case "AutumnHills":
                    if (StartingPortals.Contains("Autumn Hills Portal"))
                    {
                        orig(self, eventid);
                        AccessedStartingPortals.Add(eventid);
                    }
                    break;
                case "HowlingGrotto":
                    if (StartingPortals.Contains("Howling Grotto Portal"))
                    {
                        orig(self, eventid);
                        AccessedStartingPortals.Add(eventid);
                    }
                    break;
                case "GlacialPeak":
                    if (StartingPortals.Contains("Glacial Peak Portal"))
                    {
                        orig(self, eventid);
                        AccessedStartingPortals.Add(eventid);
                    }
                    break;
                default:
                    orig(self, eventid);
                    break;
            }
        }
        
        private static LevelConstants.RandoLevel GetPortalExit(string enteredPortal)
        {
            Console.WriteLine($"getting portal. entered {enteredPortal}");
            if (enteredPortal.Equals("Tower of Time - Left"))
            {
                EnteredTower = true;
            }
            var portalExit = PortalMapping[Portals.IndexOf(enteredPortal)];
            Console.WriteLine($"{portalExit.Region}, {portalExit.PortalType}, {portalExit.Index}");
            return AreaCheckpoints[portalExit.Region][portalExit.PortalType][portalExit.Index];
        }

        public static void Teleport()
        {
            var currentLevel = Manager<LevelManager>.Instance.GetCurrentLevelEnum();
            if (LevelConstants.TransitionToEntranceName.TryGetValue(
                    new LevelConstants.Transition(ELevel.Level_13_TowerOfTimeHQ,
                        currentLevel), out var portal))
            {
                var newLevel = GetPortalExit(portal);
                if (newLevel.LevelName.Equals(ELevel.Level_09_B_ElementalSkylands))
                    RandoLevelManager.KillManfred = true;
                RandoLevelManager.TeleportInArea(newLevel.LevelName, newLevel.PlayerPos, newLevel.Dimension);
            }
            else
            {
                Console.WriteLine($"unable to find portal for {currentLevel}");
            }
            LeftHQPortal = false;
        }

        public static void LeaveHQ(On.TotHQ.orig_LeaveToLevel orig, TotHQ self, bool playLevelMusic, bool loadingNewLevel)
        {
            Console.WriteLine($"leaving hq.. should be teleporting: {PortalMapping != null}");
            LeftHQPortal = PortalMapping != null && PortalMapping.Count > 0;
            orig(self, playLevelMusic, loadingNewLevel);
        }
    }
}