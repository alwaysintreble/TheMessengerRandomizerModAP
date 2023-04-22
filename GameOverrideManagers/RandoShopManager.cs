using System;
using System.Collections;
using System.Collections.Generic;
using MessengerRando.Archipelago;
using MessengerRando.RO;
using UnityEngine;

namespace MessengerRando.GameOverrideManagers
{
    public static class RandoShopManager
    {
        public static Dictionary<EShopUpgradeID, int> ShopPrices;
        public static Dictionary<EFigurine, int> FigurePrices;
        public static Queue FigurineQueue = new Queue();
        private static bool figurineOverride;
        private static int wrenchPrice;
        
        public static int GetPrice(On.UpgradeButtonData.orig_GetPrice orig, UpgradeButtonData self)
        {
            //modify shop prices here
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
            UnityEngine.Object.FindObjectOfType<SousSol>().UnlockFigurine(figurine);
        }

        public static void GoToSousSol(On.GoToSousSolCutscene.orig_EndCutScene orig, GoToSousSolCutscene self,
            bool camera, bool borders, bool transition)
        {
            orig(self, camera, borders, transition);
            
            try
            {
                while (FigurineQueue.Count > 0)
                {
                    var figurine = (EFigurine)FigurineQueue.Dequeue();
                    Debug.Log($"Unlocking {figurine}");
                    UnlockFigurine(figurine);
                }   
            } catch (Exception e) {Console.WriteLine(e);}
        }

        public static ShopListItemData GetFigurineData(On.IronHoodShopScreen.orig_GetFigurineData orig, IronHoodShopScreen self,
            FigurineDefinition figurineDefinition)
        {
            var figurine = figurineDefinition.figurineID;
            if (FigurePrices.TryGetValue(figurine, out var cost)) figurineDefinition.cost = cost;
            return orig(self, figurineDefinition);
        }

        public static void BuyMoneyWrench(On.BuyMoneyWrenchCutscene.orig_OnBuyWrenchChoice orig,
            BuyMoneyWrenchCutscene self, DialogChoice choice)
        {
            orig(self, choice);
            if (ArchipelagoClient.HasConnected && choice.ChoiceInfo.choiceId == "Yes")
            {
                wrenchPrice = Manager<InventoryManager>.Instance.timeShardsDroppedInSink;
            }
        }

        public static void EndMoneyWrenchCutscene(On.BuyMoneyWrenchCutscene.orig_EndCutsceneOnDialogDone orig,
            BuyMoneyWrenchCutscene self, View dialogBox)
        {
            orig(self, dialogBox);
            Manager<InventoryManager>.Instance.CollectTimeShard(wrenchPrice);
            wrenchPrice = 0;
        }

        public static void UnclogSink(On.MoneySinkUnclogCutscene.orig_OnDialogOutDone orig,
            MoneySinkUnclogCutscene self, View dialog)
        {
            orig(self, dialog);
            Debug.Log("Unclogged that dang sink");
            var wrenchID = ItemsAndLocationsHandler.ItemFromEItem(EItems.MONEY_WRENCH);
            if (!ArchipelagoClient.ServerData.ReceivedItems
                    .ContainsKey(wrenchID))
                Manager<ProgressionManager>.Instance.UnsetFlag(Flags.MoneySinkUnclogged);
        }
        
        public static bool IsStoryUnlocked(On.UpgradeButtonData.orig_IsStoryUnlocked orig, UpgradeButtonData self)
        {
            //Checking if this particular upgrade is the glide attack
            //Unlock the glide attack (no need to keep it hidden, player can just buy it whenever they want.
            var isUnlocked = EShopUpgradeID.GLIDE_ATTACK.Equals(self.upgradeID) || orig(self);
            return isUnlocked;
        }
    }
}