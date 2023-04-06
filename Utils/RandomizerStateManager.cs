using System;
using System.Collections.Generic;
using MessengerRando.Archipelago;
using MessengerRando.Utils;
using MessengerRando.RO;
using MessengerRando.GameOverrideManagers;
using UnityEngine;

namespace MessengerRando
{
    class RandomizerStateManager
    {
        public static RandomizerStateManager Instance { private set; get; }
        public Dictionary<LocationRO, RandoItemRO> CurrentLocationToItemMapping { set; get; }
        public Dictionary<int, List<string>> DefeatedBosses;

        public bool IsRandomizedFile { set; get; }
        public int CurrentFileSlot { set; get; }

        public RandoPowerSealManager PowerSealManager;
        public RandoBossManager BossManager;

        public string Goal;
        public bool SkipMusicBox = false;
        public bool SkipPhantom = false;
        public bool MegaShards = false;

        private Dictionary<int, SeedRO> seeds;

        private Dictionary<EItems, bool> noteCutsceneTriggerStates;
        public Dictionary<string, string> CurrentLocationDialogtoRandomDialogMapping { set; get; }
        //This overrides list will be used to track items that, during the giving of items in any particular moment, need to ignore rando logic and just hand the item over.
        private List<EItems> temporaryRandoOverrides;

        public static void Initialize()
        {
            if(Instance == null)
            {
                Instance = new RandomizerStateManager();
            }
        }

        private RandomizerStateManager()
        {
            //Create initial values for the state machine
            seeds = new Dictionary<int, SeedRO>();

            

            ResetRandomizerState();
            initializeCutsceneTriggerStates();
            temporaryRandoOverrides = new List<EItems>();
        }

        private void initializeCutsceneTriggerStates()
        {
            noteCutsceneTriggerStates = new Dictionary<EItems, bool>();
            noteCutsceneTriggerStates.Add(EItems.KEY_OF_CHAOS, false);
            noteCutsceneTriggerStates.Add(EItems.KEY_OF_COURAGE, false);
            noteCutsceneTriggerStates.Add(EItems.KEY_OF_HOPE, false);
            noteCutsceneTriggerStates.Add(EItems.KEY_OF_LOVE, false);
            noteCutsceneTriggerStates.Add(EItems.KEY_OF_STRENGTH, false);
            noteCutsceneTriggerStates.Add(EItems.KEY_OF_SYMBIOSIS, false);
        }

        /// <summary>
        /// Add seed to state's collection of seeds, providing all the necessary info to create the SeedRO object.
        /// </summary>
        public void AddSeed(int fileSlot, SeedType seedType, int seed, Dictionary<SettingType, SettingValue> settings, List<RandoItemRO> collectedItems, string mappingJson)
        {
            AddSeed(new SeedRO(fileSlot, seedType, seed, settings, collectedItems, mappingJson));
        }

        /// <summary>
        /// Add seed to state's collection of seeds.
        /// </summary>
        public void AddSeed(SeedRO seed)
        {
            seeds[seed.FileSlot] = seed;
        }

        public SeedRO GetSeedForFileSlot(int fileSlot)
        {
            if (!seeds.ContainsKey(fileSlot))
            {
                seeds[fileSlot] = new SeedRO(fileSlot, SeedType.None, 0, null, null, null);
            }
            return seeds[fileSlot];
        }

        /// <summary>
        /// Reset's the state's seed for the provided file slot. This will replace the seed with an empty seed, telling the mod this fileslot is not randomized.
        /// </summary>
        public void ResetSeedForFileSlot(int fileSlot)
        {
            //Simply keeping resetting logic here in case I want to change it i'll only do so here
            Debug.Log($"Resetting file slot '{fileSlot}'");
            if (seeds.ContainsKey(fileSlot))
            {
                seeds[fileSlot] = new SeedRO(fileSlot, SeedType.None, 0, null, null, null);
                if (DefeatedBosses == null) DefeatedBosses = new Dictionary<int, List<string>>();
                DefeatedBosses[fileSlot] = new List<string>();
            }
            Debug.Log("File slot reset complete.");
        }

        /// <summary>
        /// Checks to see if a seed exists for the given file slot.
        /// </summary>
        /// <returns>true if a seed was found and that the seed has a non-zero seed number and that seed does not have a NONE seed type. False otherwise.</returns>
        public bool HasSeedForFileSlot(int fileSlot)
        {
            return seeds.ContainsKey(fileSlot) &&
                   seeds[fileSlot].Seed != 0 &&
                   seeds[fileSlot].SeedType != SeedType.None;
        }

        public void ResetRandomizerState()
        {
            CurrentLocationToItemMapping = new Dictionary<LocationRO, RandoItemRO>();
            this.IsRandomizedFile = false;
        }

        public bool IsNoteCutsceneTriggered(EItems note)
        {
            return this.noteCutsceneTriggerStates[note];
        }

        public void SetNoteCutsceneTriggered(EItems note)
        {
            this.noteCutsceneTriggerStates[note] = true;
        }

        public void AddTempRandoItemOverride(EItems randoItem)
        {
            temporaryRandoOverrides.Add(randoItem);
        }

        public void RemoveTempRandoItemOverride(EItems randoItem)
        {
            temporaryRandoOverrides.Remove(randoItem);
        }

        public bool HasTempOverrideOnRandoItem(EItems randoItem)
        {
            return temporaryRandoOverrides.Contains(randoItem);
        }

        public bool IsSafeTeleportState()
        {
            //Unsafe teleport states are shops/hq/boss fights
            return !(Manager<TotHQ>.Instance.root.gameObject.activeInHierarchy ||
                     Manager<Shop>.Instance.gameObject.activeInHierarchy ||
                     Manager<GameManager>.Instance.IsCutscenePlaying() ||
                     Manager<PlayerManager>.Instance.Player.IsInvincible() ||
                     Manager<PlayerManager>.Instance.Player.InputBlocked() || 
                     Manager<PlayerManager>.Instance.Player.IsKnockedBack);
        }

        /// <summary>
        /// Check through the mappings for any location that is represented by vanilla location item(since that is the key used to uniquely identify locations).
        /// </summary>
        /// <param name="vanillaLocationItem">EItem being used to look up location.</param>
        /// <param name="locationFromItem">Out parameter used to return the location found.</param>
        /// <returns>true if location was found, otherwise false(location item will be null in this case)</returns>
        public bool IsLocationRandomized(EItems vanillaLocationItem, out LocationRO locationFromItem)
        {
            bool isLocationRandomized = false;
            locationFromItem = null;

            if (!ArchipelagoClient.HasConnected) return isLocationRandomized;
            locationFromItem = ItemsAndLocationsHandler.ArchipelagoLocations.Find(location =>
                location.PrettyLocationName.Equals(vanillaLocationItem.ToString()));
            if (locationFromItem != null &&
                ArchipelagoClient.ServerData.LocationToItemMapping.ContainsKey(locationFromItem))
                isLocationRandomized = true;
            return isLocationRandomized;
        }

        /// <summary>
        /// Helper method to log out the current mappings all nicely for review
        /// </summary>
        public void LogCurrentMappings()
        {
            if(CurrentLocationToItemMapping != null)
            {
                Debug.Log("----------------BEGIN Current Mappings----------------");
                foreach (LocationRO check in this.CurrentLocationToItemMapping.Keys)
                {
                    Debug.Log($"Check '{check.PrettyLocationName}'({check.LocationName}) contains Item '{CurrentLocationToItemMapping[check]}' for {CurrentLocationToItemMapping[check].RecipientName}");
                }
                Debug.Log("----------------END Current Mappings----------------");
            }
            else
            {
                Debug.Log("Location mappings were not set for this seed.");
            }

            if (CurrentLocationDialogtoRandomDialogMapping != null)
            {
                Debug.Log("----------------BEGIN Current Dialog Mappings----------------");
                foreach (KeyValuePair<string, string> KVP in CurrentLocationDialogtoRandomDialogMapping)
                {
                    Debug.Log($"Dialog '{KVP.Value}' is located at Check '{KVP.Key}'");
                }
                Debug.Log("----------------END Current Dialog Mappings----------------");
            }
            else
            {
                Debug.Log("Dialog mappings were not set for this seed.");
            }
        }

    }
}
