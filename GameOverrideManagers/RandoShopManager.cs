using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Archipelago.MultiClient.Net.Enums;
using MessengerRando.Archipelago;
using MessengerRando.RO;
using MessengerRando.Utils;
using Mod.Courier;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MessengerRando.GameOverrideManagers
{
    public static class RandoShopManager
    {
        public static Dictionary<EShopUpgradeID, int> ShopPrices;
        public static Dictionary<EFigurine, int> FigurePrices;
        private static readonly Queue FigurineQueue = new Queue();
        private static bool figurineOverride;
        private static int wrenchPrice;
        public static bool RaceMode;
        public static bool ShopHints = true;

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
            Object.FindObjectOfType<SousSol>().UnlockFigurine(figurine);
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static ShopListItemData GetFigurineData(On.IronHoodShopScreen.orig_GetFigurineData orig,
            IronHoodShopScreen self,
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

        public static void UpgradeButton_Refresh(On.UpgradeButton.orig_Refresh orig, UpgradeButton self)
        {
            try
            {
                orig(self);
                var icon = Courier.LoadFromAssetBundles<Sprite>(Application.streamingAssetsPath + "/AP Icon.prefab");
                var sprite = Resources.Load<Sprite>(Application.streamingAssetsPath + "/AP Icon.prefab");
                self.icon.sprite = icon;
            }
            catch (Exception e)
            {
                e.LogDetailed();
            }
        }

        public static string GetText(On.LocalizationManager.orig_GetText orig, LocalizationManager self, string locid)
        {
            // if (!InShop()) return orig(self, locid);
            Console.WriteLine($"Requesting text for {locid}");
            if (!ArchipelagoClient.HasConnected) return orig(self, locid);
            var locType = TextType.None;
            var lookupName = string.Empty;
            if (locid.Contains("DESCRIPTION"))
            {
                Console.WriteLine("description");
                locType = TextType.Description;
                lookupName = locid.Replace("_DESCRIPTION", string.Empty);
            }
            else if (locid.Contains("NAME"))
            {
                Console.WriteLine("name");
                locType = TextType.Name;
                lookupName = locid.Replace("_NAME", string.Empty);
            }

            if (locType.Equals(TextType.None))
                return orig(self, locid);

            if (!ShopTextToItems.TryGetValue(lookupName, out var itemType))
            {
                return orig(self, locid);
            }

            if (RaceMode || !ShopHints) return locType.Equals(TextType.Name) ? "Unknown Item" : "???";

            var locationID = ItemsAndLocationsHandler.LocationFromEItem(itemType);
            switch (itemType)
            {
                case EItems.HEART_CONTAINER when lookupName.Contains("SECOND"):
                    ItemsAndLocationsHandler.LocationsLookup.TryGetValue(
                        new LocationRO("HP_UPGRADE_2", EItems.HEART_CONTAINER), out locationID);
                    break;
                case EItems.SHURIKEN_UPGRADE when lookupName.Contains("SECOND"):
                    ItemsAndLocationsHandler.LocationsLookup.TryGetValue(new LocationRO("SHURIKEN_UPGRADE_2",
                        EItems.SHURIKEN_UPGRADE), out locationID);
                    break;
            }

            if (ArchipelagoClient.Offline)
            {
                return locType.Equals(TextType.Description)
                    ? SeedGenerator.GetOfflineShopDescription(locationID)
                    : SeedGenerator.GetOfflineShopText(locationID);
            }

            var itemOnLocation = RandomizerStateManager.Instance.ScoutedLocations[locationID];
            if (locType.Equals(TextType.Name) && ArchipelagoClient.Authenticated)
            {
                if(!ArchipelagoClient.ServerData.CheckedLocations.Contains(locationID))
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                        ArchipelagoClient.Session.Locations.ScoutLocationsAsync(
                            null, HintCreationPolicy.CreateAndAnnounceOnce, locationID));
                }
            }

            return locType.Equals(TextType.Name)
                ? itemOnLocation.Colorize()
                : itemOnLocation.GetShopDescription();
        }

        private enum TextType
        {
            None,
            Name,
            Description,
        }

        private static readonly Dictionary<string, EItems> ShopTextToItems = new()
        {
            { "MAP_PORTALS", EItems.MAP_TIME_WARP },
            { "CHALLENGE_ROOM_WORLDMAP", EItems.MAP_POWER_SEAL_TOTAL },
            { "CHALLENGE_ROOM_LOCALMAP", EItems.MAP_POWER_SEAL_PINS },
            { "DAMAGE_REDUCTION", EItems.DAMAGE_REDUCTION },
            { "ENEMY_HP_DROP", EItems.ENEMY_DROP_HP },
            { "ENEMY_KI_DROP", EItems.ENEMY_DROP_MANA },
            { "CHECKPOINT_UPGRADE", EItems.CHECKPOINT_UPGRADE },
            { "FIRST_HP_UPGRADE", EItems.HEART_CONTAINER },
            { "SECOND_HP_UPGRADE", EItems.HEART_CONTAINER },
            { "LAST_HP_UPGRADE", EItems.POTION_FULL_HEAL_AND_HP_UPGRADE },
            { "FIRST_KI_UPGRADE", EItems.SHURIKEN_UPGRADE },
            { "SECOND_KI_UPGRADE", EItems.SHURIKEN_UPGRADE },
            { "QUARBLE_DISCOUNT", EItems.QUARBLE_DISCOUNT_50 },
            { "AIR_RECOVER", EItems.AIR_RECOVER },
            { "UNLOCK_SHURIKEN", EItems.SHURIKEN },
            { "ATTACK_PROJECTILE", EItems.ATTACK_PROJECTILES },
            { "POWER_ATTACK", EItems.CHARGED_ATTACK },
            { "SWIM_DASH", EItems.SWIM_DASH },
            { "GLIDE_ATTACK", EItems.GLIDE_ATTACK },
        };

        private static bool InShop()
        {
            var currentLev = Manager<LevelManager>.Instance.GetCurrentLevelEnum();
            return currentLev is ELevel.NONE or ELevel.Level_13_TowerOfTimeHQ;
        }
    }
}