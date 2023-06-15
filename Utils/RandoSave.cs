using MessengerRando.Archipelago;
using Mod.Courier.Save;

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
            if (ArchipelagoClient.HasConnected)
            {
                RandomizerStateManager.Instance.APSave[RandomizerStateManager.Instance.CurrentFileSlot] =
                    ArchipelagoClient.ServerData;
                
                if (ArchipelagoClient.Authenticated)
                    ArchipelagoClient.SyncLocations();
            }
            APSaveData = GetSaveData();
            
        }

        public static string GetSaveData()
        {
            var output = "";
            for (int i = 1; i <= 3; i++)
            {
                output += $"{RandomizerStateManager.Instance.APSave[i]}|";
            }

            return output;
        }
    }
}
