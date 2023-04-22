using System;
using System.Collections.Generic;
using MessengerRando.Archipelago;
using MessengerRando.Utils;
using Object = UnityEngine.Object;

namespace MessengerRando.GameOverrideManagers
{
    public class RandoPowerSealManager
    {
        public static readonly List<string> Goals = new List<string> { "power_seal_hunt" };
        private int amountPowerSealsCollected;

        public RandoPowerSealManager(int requiredPowerSeals)
        {
            Manager<ProgressionManager>.Instance.powerSealTotal = requiredPowerSeals;
            amountPowerSealsCollected = ArchipelagoClient.ServerData.PowerSealsCollected;
        }

        public void AddPowerSeal() => ArchipelagoClient.ServerData.PowerSealsCollected = ++amountPowerSealsCollected;


        public void OnShopChestOpen(On.ShopChestOpenCutscene.orig_OnChestOpened orig, ShopChestOpenCutscene self)
        {
            if (Goals.Contains(RandomizerStateManager.Instance.Goal) &&
                RandomizerStateManager.IsSafeTeleportState())
            {
                //going to attempt to teleport the player to the ending sequence when they open the chest
                OnShopChestOpen();
                self.EndCutScene();
            }
            else orig(self);
            self.EndCutScene();
        }

        public void OnShopChestOpen(On.ShopChestChangeShurikenCutscene.orig_Play orig, ShopChestChangeShurikenCutscene self)
        {
            if (Goals.Contains(RandomizerStateManager.Instance.Goal) &&
                RandomizerStateManager.IsSafeTeleportState())
            {
                OnShopChestOpen();
                self.EndCutScene();
            }
            else orig(self);
            self.EndCutScene();
        }

        private void OnShopChestOpen()
        {
            try
            {
                Object.FindObjectOfType<Shop>().LeaveToCurrentLevel();
                RandoLevelManager.SkipMusicBox();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Assigns our total power seal count to the game and then returns the value. Unsure if the assignment is safe
        /// here, but trying it so it'll show the required count in the dialog.
        /// </summary>
        /// <returns></returns>
        public int AmountPowerSealsCollected() => amountPowerSealsCollected;
    }
}