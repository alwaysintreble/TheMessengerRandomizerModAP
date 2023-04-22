using Mod.Courier;
using Mod.Courier.Module;


namespace MessengerRando.Utils
{
    //This class will be responsible for handling the randomization of items to locations and generating the mapping dictionary.
    public class ItemRandomizerUtil
    {
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
    }
}
