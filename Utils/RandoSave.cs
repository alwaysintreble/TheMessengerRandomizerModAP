using System;
using MessengerRando.Archipelago;
using Mod.Courier.Save;
using Newtonsoft.Json;
using UnityEngine;

namespace MessengerRando.Utils
{
    /// <summary>
    /// CourierModSave object for the randomizer. Defines the values used for the mod save file.
    /// Due to current limitations of the save file, a single string value is used to capture all of the needed save information. 
    /// Once Courier is able to support more complex object for the save file we can consider refactoring this.
    /// </summary>
    public class RandoSave : CourierModSave
    {
        public string APSaveData = GetSaveData();

        public void Update()
        {
            if (Manager<LevelManager>.Instance.GetCurrentLevelEnum().Equals(ELevel.NONE)) return;
            RandomizerStateManager.Instance.APSave[RandomizerStateManager.Instance.CurrentFileSlot] =
                ArchipelagoClient.ServerData;

            if (ArchipelagoClient.Authenticated && RandomizerStateManager.IsSafeTeleportState())
                ArchipelagoClient.SyncLocations();
            APSaveData = GetSaveData();
        }

        public void ForceUpdate()
        {
            APSaveData = GetSaveData();
        }

        public static string GetSaveData()
        {
            var output =
                $"{RandomizerStateManager.Instance.APSave[1]}|" +
                $"{RandomizerStateManager.Instance.APSave[2]}|" +
                $"{RandomizerStateManager.Instance.APSave[3]}";
            return output;
        }
        
        private const char SplitConst = '|';

        public static void TryLoad(string load)
        {
            Debug.Log("loading rando save data...");
            if (string.IsNullOrEmpty(load))
            {
                Debug.Log("unable to find rando save data to load");
                return;
            }
            try
            {
                var loadedData = load.Split(SplitConst);
                var index = 1;
                foreach (var dataString in loadedData)
                {
                    var loadedAPData = JsonConvert.DeserializeObject<ArchipelagoData>(dataString);
                    RandomizerStateManager.Instance.APSave[index] = loadedAPData;
                    index++;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Failed to load AP Save Data");
                Console.WriteLine(e);
            }
        }
    }
}
