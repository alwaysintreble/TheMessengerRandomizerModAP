using System;
using System.Collections.Generic;
using MessengerRando.Utils.Constants;
using UnityEngine;
using Object = UnityEngine.Object;

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
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3()),
                            }
                        },
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3()),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3()),
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
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3()),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_03_ForlornTemple, new Vector3()),
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
                                new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3()),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_04_Catacombs, new Vector3()),
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
                                new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3()),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_06_A_BambooCreek, new Vector3()),
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
                                new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3()),
                            }
                        },
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3()),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3()),
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
                                new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3()),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3()),
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
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3()),
                            }
                        },
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3()),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3()),
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
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3()),
                            }
                        },
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3()),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3()),
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
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3()),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_10_A_TowerOfTime, new Vector3()),
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
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3()),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_11_A_CloudRuins, new Vector3()),
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
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3()),
                            }
                        },
                        {
                            "Checkpoints", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_12_UnderWorld, new Vector3()),
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
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3()),
                            }
                        },
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3()),
                            }
                        },
                        {
                            "Checkpoitns", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_04_C_RiviereTurquoise, new Vector3()),
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
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3()),
                            }
                        },
                        {
                            "Shops", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3()),
                            }
                        },
                        {
                            "Checkpoitns", new List<LevelConstants.RandoLevel>
                            {
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3()),
                                new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3()),
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