using System.Collections.Generic;
using UnityEngine;

namespace MessengerRando.Utils.Constants
{
    public static class BossConstants
    {
        public struct BossLocation
        {
            public readonly ELevel BossRegion;
            public readonly Vector2 PlayerPosition;
            public readonly EBits PlayerDimension;

            public BossLocation(ELevel area, Vector2 pos, EBits dim)
            {
                BossRegion = area;
                PlayerPosition = pos;
                PlayerDimension = dim;
            }
        }

        public static readonly List<string> VanillaBossNames = new List<string>
        {
            "LeafGolem",
            "Necromancer",
            "EmeraldGolem",
            "QueenOfQuills",
            // "Colos_Susses",
            "Manfred",
            // "TowerGolem",
            "DemonGeneral",
            "DemonArtificier",
            "ButterflyMatriarch",
            "Phantom"
        };

        public static readonly Dictionary<string, string> BossCutscenes = new Dictionary<string, string>
        {
            { "LeafGolem", "LeafGolemIntroCutscene" },
            { "Necromancer", "NecromancerIntroCutscene" },
            { "EmeraldGolem", "EmeraldGolemIntroCutscene" },
            { "QueenOfQuills", "QueenOfQuillsIntroCutscene" },
            // {"Colos_Susses", },
            { "Manfred", "ManfredIntroCutscene" },
            // {"TowerGolem", },
            { "DemonGeneral", "DemonGeneralIntroCutscene" },
            { "DemonArtificier", "DemonArtificierIntroCutscene" },
            { "ButterflyMatriarch", "ButterflyMatriarchIntroCutscene" },
            { "Phantom", "PhantomIntroCutscene" }
        };

        public static readonly Dictionary<string, string> RoomToVanillaBoss = new Dictionary<string, string>
        {
            { "908940-28-12", "LeafGolem" },
            { "748780-76-60", "Necromancer" },
            { "556588-140-60", "EmeraldGolem" },
            { "11001132-44-28", "QueenOfQuills" },
            { "332364308324", "Colos_Susses" },
            { "11641228-28-12", "Manfred" },
            // { "108140228244", "TowerGolem" },
            { "140172-44-28", "DemonGeneral" },
            { "396428-12436", "DemonArtificier" },
            { "-308-276420", "ButterflyMatriarch" }
        };

        public static readonly Dictionary<string, BossLocation> BossLocations = new Dictionary<string, BossLocation> 
        {
            { "LeafGolem", new BossLocation(ELevel.Level_02_AutumnHills, new Vector2(908, -27), EBits.BITS_8) },
            { "Necromancer", new BossLocation(ELevel.Level_04_Catacombs, new Vector2(752, -75), EBits.BITS_8) },
            { "EmeraldGolem", new BossLocation(ELevel.Level_05_A_HowlingGrotto, new Vector2(560, -123), EBits.BITS_8) },
            { "QueenOfQuills", new BossLocation( ELevel.Level_07_QuillshroomMarsh, new Vector2(1100, -43), EBits.BITS_8) },
            // { "Colos_Susses", new BossLocation(ELevel.Level_08_SearingCrags, new Vector2(364, 311), EBits.BITS_8) },
            { "Manfred", new BossLocation(ELevel.Level_11_A_CloudRuins, new Vector2(1165, -26), EBits.BITS_16)},
            // { "Tower Golem", new BossLocation(ELevel.Level_10_A_TowerOfTime, new Vector2(108, 237), EBits.BITS_16) },
            { "DemonGeneral", new BossLocation(ELevel.Level_12_UnderWorld, new Vector2(140, -43), EBits.BITS_16) },
            { "DemonArtificier", new BossLocation(ELevel.Level_03_ForlornTemple, new Vector2(396, -11), EBits.BITS_16)},
            { "ButterflyMatriarch", new BossLocation(ELevel.Level_04_C_RiviereTurquoise, new Vector2(-276, 6), EBits.BITS_16) }
        };
    }
}