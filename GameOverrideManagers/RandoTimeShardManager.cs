using System;
using System.Collections.Generic;
using System.Linq;
using MessengerRando.Archipelago;
using MessengerRando.RO;
using UnityEngine;

namespace MessengerRando.GameOverrideManagers
{
    public class RandoTimeShardManager
    {


        public struct MegaShard
        {
            private readonly ELevel shardRegion;
            private readonly string roomKey;
            public readonly LocationRO Loc;

            public MegaShard(ELevel area, string key, LocationRO loc)
            {
                shardRegion = area;
                roomKey = key;
                Loc = loc;
            }

            public MegaShard(ELevel area, string key)
            {
                shardRegion = area;
                roomKey = key;
                Loc = null;
            }

            public bool Equals(MegaShard other)
            {
                return roomKey == other.roomKey && shardRegion == other.shardRegion;
            }

            public override bool Equals(object obj)
            {
                return obj is MegaShard other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int)shardRegion * 397) ^ (roomKey != null ? roomKey.GetHashCode() : 0);
                }
            }
        }
        
        public static readonly List<MegaShard> MegaShardLookup = new List<MegaShard>
        {
            new MegaShard(ELevel.Level_02_AutumnHills, "268300-108-92", new LocationRO("Autumn Hills Mega Shard")),
            new MegaShard(ELevel.Level_03_ForlornTemple, "-2044420", new LocationRO("Hidden Entrance Mega Shard")),
            new MegaShard(ELevel.Level_04_Catacombs, "108172420", new LocationRO("Catacombs Mega Shard")),
            new MegaShard(ELevel.Level_06_A_BambooCreek, "-84-20420", new LocationRO("Above Entrance Mega Shard")),
            new MegaShard(ELevel.Level_06_A_BambooCreek, "-52-202052", new LocationRO("Abandoned Mega Shard")),
            new MegaShard(ELevel.Level_06_A_BambooCreek, "364396420", new LocationRO("Time Loop Mega Shard")),
            new MegaShard(ELevel.Level_05_A_HowlingGrotto, "108140-108-92", new LocationRO("Bottom Left Mega Shard")),
            new MegaShard(ELevel.Level_05_A_HowlingGrotto, "332364-172-156", new LocationRO("Near Portal Mega Shard")),
            new MegaShard(ELevel.Level_05_A_HowlingGrotto, "460492-76-60", new LocationRO("Pie in the Sky Mega Shard")),
            new MegaShard(ELevel.Level_07_QuillshroomMarsh, "4476-28-12", new LocationRO("Quillshroom Marsh Mega Shard")),
            new MegaShard(ELevel.Level_08_SearingCrags, "236268116132", new LocationRO("Searing Crags Mega Shard")),
            new MegaShard(ELevel.Level_09_A_GlacialPeak, "268300-284-268", new LocationRO("Glacial Peak Mega Shard")),
            new MegaShard(ELevel.Level_11_A_CloudRuins, "-404-372-60-44", new LocationRO("Cloud Entrance Mega Shard")),
            new MegaShard(ELevel.Level_11_A_CloudRuins, "-308-276-44-28", new LocationRO("Time Warp Mega Shard")),
            new MegaShard(ELevel.Level_11_A_CloudRuins, "11321164-124", new LocationRO("Money Farm Room Mega Shard 1")),
            new MegaShard(ELevel.Level_11_A_CloudRuins, "11321164-124", new LocationRO("Money Farm Room Mega Shard 2")),
            new MegaShard(ELevel.Level_12_UnderWorld, "-436-308-60-44", new LocationRO("Under Entrance Mega Shard")),
            new MegaShard(ELevel.Level_12_UnderWorld, "44766884", new LocationRO("Hot Tub Mega Shard")),
            new MegaShard(ELevel.Level_12_UnderWorld, "76108-4420", new LocationRO("Projectile Pit Mega Shard")),
            new MegaShard(ELevel.Level_03_ForlornTemple, "761402036", new LocationRO("Sunny Day Mega Shard")),
            new MegaShard(ELevel.Level_03_ForlornTemple, "268300-44-28", new LocationRO("Down Under Mega Shard")),
            new MegaShard(ELevel.Level_05_B_SunkenShrine, "4476-172-156", new LocationRO("Mega Shard of the Moon")),
            new MegaShard(ELevel.Level_05_B_SunkenShrine, "108140-60-44", new LocationRO("Beginner's Mega Shard")),
            new MegaShard(ELevel.Level_05_B_SunkenShrine, "4476420", new LocationRO("Mega Shard of the Stars")),
            new MegaShard(ELevel.Level_05_B_SunkenShrine, "-52-20-92-76", new LocationRO("Mega Shard of the Sun")),
            new MegaShard(ELevel.Level_04_C_RiviereTurquoise, "780812-124", new LocationRO("Waterfall Mega Shard")),
            new MegaShard(ELevel.Level_04_C_RiviereTurquoise, "1244-124", new LocationRO("Quick Restock Mega Shard 1")),
            new MegaShard(ELevel.Level_04_C_RiviereTurquoise, "1244-124", new LocationRO("Quick Restock Mega Shard 2")),
            new MegaShard(ELevel.Level_09_B_ElementalSkylands, "876908340356", new LocationRO("Earth Mega Shard")),
            new MegaShard(ELevel.Level_09_B_ElementalSkylands, "18361868388404", new LocationRO("Water Mega Shard")),
        };

        public static void BreakShard(MegaShard shardToBreak)
        {
            var location = MegaShardLookup.First(item => item.Equals(shardToBreak)).Loc;
            Console.WriteLine($"Broke Shard {location.LocationName}");
            if (ArchipelagoClient.HasConnected)
            {
                if (location.Equals(new LocationRO("Money Farm Room Mega Shard 1")) &&
                    ArchipelagoClient.ServerData.CheckedLocations.Contains(
                        ItemsAndLocationsHandler.LocationsLookup[new LocationRO("Money Farm Room Mega Shard 1")]))
                    location = new LocationRO("Money Farm Room Mega Shard 2");
                else if (location.Equals(new LocationRO("Quick Restock Mega Shard 1")) &&
                         ArchipelagoClient.ServerData.CheckedLocations.Contains(
                             ItemsAndLocationsHandler.LocationsLookup[new LocationRO("Quick Restock Mega Shard 1")]))
                    location = new LocationRO("Quick Restock Mega Shard 2");
                if (ArchipelagoClient.ServerData.CheckedLocations.Contains(
                        ItemsAndLocationsHandler.LocationsLookup[location])) return;
                ItemsAndLocationsHandler.SendLocationCheck(location);
                if (!ArchipelagoClient.ServerData.LocationToItemMapping.TryGetValue(location, out var randoItem))
                    return;
                var shardSequence = ScriptableObject.CreateInstance<DialogSequence>();
                shardSequence.dialogID = "ARCHIPELAGO_ITEM";
                shardSequence.name = randoItem.RecipientName.Equals(ArchipelagoClient.ServerData.SlotName)
                    ? $"{randoItem.Name}"
                    : $"{randoItem.Name} for {randoItem.RecipientName}";
                shardSequence.choices = new List<DialogSequenceChoice>();
                AwardItemPopupParams challengeAwardItemParams = new AwardItemPopupParams(shardSequence, true);
                Manager<UIManager>.Instance.ShowView<AwardItemPopup>(EScreenLayers.PROMPT, challengeAwardItemParams);
            }
            else
                Manager<InventoryManager>.Instance.AddItem(
                    RandomizerStateManager.Instance.CurrentLocationToItemMapping[location].Item,
                    ItemsAndLocationsHandler.APQuantity
                    );
        }
    }
}