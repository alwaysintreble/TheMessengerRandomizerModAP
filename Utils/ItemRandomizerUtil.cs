using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Mod.Courier;
using Mod.Courier.Module;
using MessengerRando.RO;
using MessengerRando.Exceptions;


namespace MessengerRando.Utils
{
    //This class will be responsible for handling the randomization of items to locations and generating the mapping dictionary.
    public class ItemRandomizerUtil
    {
        //Used to represent all the required items to complete this seed, along with what they currently block. This is to prevent self locks. 
        private static Dictionary<RandoItemRO, HashSet<RandoItemRO>> requiredItems = new Dictionary<RandoItemRO, HashSet<RandoItemRO>>();

        /// <summary>
        /// Gets the current version number for the mod.
        /// </summary>
        /// <returns>the version number or "Unknown" if it has trouble getting the version number.</returns>
        public static string GetModVersion()
        {
            string version = "Unknown";

            foreach (CourierModuleMetadata modMetadata in Courier.Mods)
            {
                if ("TheMessengerRandomizerAP".Equals(modMetadata.Name))
                {
                    version = modMetadata.VersionString;
                }
            }
            return version;
        }
        
        /// <summary>
        /// Loads mappings file from disk.
        /// </summary>
        /// <param name="fileSlot">file slot number(1/2/3)</param>
        /// <returns>String containing encrypted mappings contents from file</returns>
        public static string LoadMappingsFromFile(int fileSlot)
        {
            //Get a handle on the necessary mappings file
            Console.WriteLine($"Attempting to load mappings from file for file slot '{fileSlot}'");
            return File.ReadAllText($@"Mods\TheMessengerRandomizerMappings\MessengerRandomizerMapping_{fileSlot}.txt");
        }

        /// <summary>
        /// Performs decryption of seed info that would have been previously recieved from mappings file.
        /// </summary>
        /// <param name="b64SeedInfo">Encypted mappings string to decrypt</param>
        /// <returns>Decrypted string of mappings.</returns>
        public static string DecryptSeedInfo(string b64SeedInfo)
        {
            //We'll need to take the b64 string and decrypt it so we can get the seed info.

            byte[] bytes = Convert.FromBase64String(b64SeedInfo);

            string seedInfo = Encoding.ASCII.GetString(bytes);

            Console.WriteLine($"Decoded seed info string: '{seedInfo}'");

            return seedInfo;
        }

        /// <summary>
        /// Parses a seed info string into a SeedRO object.
        /// </summary>
        /// <param name="fileSlot">Fileslot number to add to seed object(1/2/3)</param>
        /// <param name="seedInfo">Unparsed, decypted seed info string</param>
        /// <returns>SeedRO object representing this seed.</returns>
        public static SeedRO ParseSeed(int fileSlot, string seedInfo)
        {
            //Break up mapping string
            string[] fullSeedInfoArr = seedInfo.Split('|');

            string mappingText = fullSeedInfoArr[0].Substring(fullSeedInfoArr[0].IndexOf('=') + 1);
            Console.WriteLine($"Mapping text: '{mappingText}'");

            string settingsText = fullSeedInfoArr[1];
            Console.WriteLine($"Settings text: '{settingsText}'");

            string seedTypeText = fullSeedInfoArr[2];
            Console.WriteLine($"Seed Type text: '{seedTypeText}'");

            string seedNumStr = fullSeedInfoArr[3];
            Console.WriteLine($"Seed Number text: '{seedTypeText}'");

            
            //Settings
            Dictionary<SettingType, SettingValue> settings = new Dictionary<SettingType, SettingValue>();
            string[] settingsArr = settingsText.Split(',');

            foreach (string setting in settingsArr)
            {
                Console.WriteLine($"Settings - Working with: '{setting}'");
                string[] settingKV = setting.Split('=');
                settings.Add((SettingType) Enum.Parse(typeof(SettingType), settingKV[0]), (SettingValue) Enum.Parse(typeof(SettingValue), settingKV[1]));
            }

            //Seedtype
            string seedTypeStr = seedTypeText.Substring(seedTypeText.IndexOf('=') + 1);
            SeedType seedType = (SeedType)Enum.Parse(typeof(SeedType), seedTypeStr);

            //Seed Number

            int seedNum = Int32.Parse(seedNumStr.Substring(seedNumStr.IndexOf('=') + 1));

            return new SeedRO(fileSlot,seedType,seedNum, settings, null, mappingText);
        }
        
        private static bool HasAdditionalItemsForBeatableSeedCheck(EItems[] additionalLocationRequiredItems, SamplePlayerRO player)
        {
            bool hasAdditionalItems = true;

            //Check each item in the list of required items for this location
            foreach (EItems item in additionalLocationRequiredItems)
            {
                bool itemFound = false;
                //Check each item the player has
                foreach (RandoItemRO playerItem in player.AdditionalItems)
                {
                    if (playerItem.Item.Equals(item))
                    {
                        //We have this required item
                        itemFound = true;
                        break;
                    }
                }
                if (!itemFound)
                {
                    //We were missing at least one required item
                    hasAdditionalItems = false;
                    break;
                }
            }
            return hasAdditionalItems;
        }

        private static void CollectItemForBeatableSeedCheck(RandoItemRO itemToCollect, ref SamplePlayerRO player)
        {
            //Handle the various types of items
            switch(itemToCollect.Item)
            {
                case EItems.WINGSUIT:
                    player.HasWingsuit = true;
                    break;
                case EItems.GRAPLOU:
                    player.HasRopeDart = true;
                    break;
                case EItems.MAGIC_BOOTS:
                    player.HasNinjaTabis = true;
                    break;
                case EItems.KEY_OF_CHAOS:
                    player.NoteCount++;
                    break;
                case EItems.KEY_OF_COURAGE:
                    player.NoteCount++;
                    break;
                case EItems.KEY_OF_HOPE:
                    player.NoteCount++;
                    break;
                case EItems.KEY_OF_LOVE:
                    player.NoteCount++;
                    break;
                case EItems.KEY_OF_STRENGTH:
                    player.NoteCount++;
                    break;
                case EItems.KEY_OF_SYMBIOSIS:
                    player.NoteCount++;
                    break;
                default:
                    //Some other item, just throw it in
                    player.AdditionalItems.Add(itemToCollect);
                    break;
            }
        }
    }
}
