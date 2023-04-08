using System;
using System.Collections.Generic;
using System.Reflection;
using MessengerRando.Archipelago;
using MessengerRando.RO;

namespace MessengerRando.GameOverrideManagers
{
    public static class RandoShopManager
    {
        public static Dictionary<EShopUpgradeID, int> ShopPrices;
        private static bool figurineOverride;
        public static int GetPrice(On.UpgradeButtonData.orig_GetPrice orig, UpgradeButtonData self)
        {
            //should be able to modify shop prices here
            if (ShopPrices != null && ShopPrices.TryGetValue(self.upgradeID, out var price)) return price;
            return orig(self);
        }

        public static void ShopInit(On.Shop.orig_Init orig, Shop self, ShopParameters parameters, bool fake)
        {
            orig(self, parameters, fake);
            self.armoireInteractionTrigger.gameObject.SetActive(false);
            self.armoire.SetActive(false);
            self.jukebox.SetActive(true);
        }

        public static void UnlockFigurine(On.SousSol.orig_UnlockFigurine orig, SousSol self, EFigurine figurine)
        {
            if (ShopPrices != null && !figurineOverride)
            {
                ItemsAndLocationsHandler.SendLocationCheck(new LocationRO(figurine.ToString()));
            }
            else
            {
                figurineOverride = false;
                orig(self, figurine);
            }
        }

        public static void UnlockFigurine(EFigurine figurine)
        {
            figurineOverride = true;
            Manager<SousSol>.Instance.UnlockFigurine(figurine);
        }

        public static void BuyMoneyWrench(On.BuyMoneyWrenchCutscene.orig_OnBuyWrenchChoice orig,
            BuyMoneyWrenchCutscene self, DialogChoice choice)
        {
            if (ArchipelagoClient.HasConnected && choice.ChoiceInfo.choiceId == "Yes")
            {
                var currentShards = Manager<InventoryManager>.Instance.ItemQuantityByItemId[EItems.TIME_SHARD];
                orig(self, choice);
                Manager<InventoryManager>.Instance.SetItemQuantity(EItems.TIME_SHARD, currentShards);
            }
            else orig(self, choice);
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