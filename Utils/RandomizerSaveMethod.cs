using System;
using MessengerRando.Archipelago;
using Newtonsoft.Json;

namespace MessengerRando.Utils
{
    public static class RandomizerSaveMethod
    {
        private const char SplitConst = '|';

        public static void TryLoad(string load)
        {
            if (string.IsNullOrEmpty(load)) return;
            try
            {
                var loadedData = load.Split(SplitConst);
                var index = 1;
                foreach (var dataString in loadedData)
                {
                    RandomizerStateManager.Instance.APSave[index] =
                        JsonConvert.DeserializeObject<ArchipelagoData>(dataString);
                    index++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load AP Save Data");
                Console.WriteLine(e);
            }
        }
    }
}
