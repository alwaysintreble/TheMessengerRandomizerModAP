using System.Collections.Generic;

namespace MessengerRando.GameOverrideManagers
{
    public static class RandoShopManager
    {
        public static Dictionary<EShopUpgradeID, int> shopPrices;
        public static int GetPrice(On.UpgradeButtonData.orig_GetPrice orig, UpgradeButtonData self)
        {
            //should be able to modify shop prices here
            if (shopPrices != null && shopPrices.TryGetValue(self.upgradeID, out var price)) return price;
            return orig(self);
        }
        
        public static bool IsStoryUnlocked(On.UpgradeButtonData.orig_IsStoryUnlocked orig, UpgradeButtonData self)
        {
            // probably want to fully override this when doing shop rando to force people to buy upgrades in a
            // particular order?
            
            //Checking if this particular upgrade is the glide attack
            //Unlock the glide attack (no need to keep it hidden, player can just buy it whenever they want.
            var isUnlocked = EShopUpgradeID.GLIDE_ATTACK.Equals(self.upgradeID) || orig(self);

            //I think there is where I can catch things like checks for the wingsuit attack upgrade.
            // Console.WriteLine($"Checking upgrade '{self.upgradeID}'. Is story unlocked: {isUnlocked}");

            return isUnlocked;
        }
    }
}