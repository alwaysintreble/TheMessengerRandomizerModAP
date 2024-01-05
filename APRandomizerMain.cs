using System;
using System.Collections.Generic;
using System.IO;
using MessengerRando.Archipelago;
using MessengerRando.Overrides;
using MessengerRando.Utils;
using MessengerRando.RO;
using Mod.Courier;
using Mod.Courier.Module;
using Mod.Courier.UI;
using MonoMod.Cil;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Mod.Courier.UI.TextEntryButtonInfo;
using TMPro;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using MessengerRando.GameOverrideManagers;
using WebSocketSharp;

namespace MessengerRando 
{
    /// <summary>
    /// Where it all begins! This class defines and injects all the necessary for the mod.
    /// </summary>
    public class APRandomizerMain : CourierModule
    {
        private float updateTimer;
        private float updateTime = 3.0f;
        private bool clearedData = false;

        private RandomizerStateManager randoStateManager;

        TextEntryButtonInfo resetRandoSaveFileButton;
  
        SubMenuButtonInfo versionButton;
        SubMenuButtonInfo seedNumButton;

        SubMenuButtonInfo windmillShurikenToggleButton;
        SubMenuButtonInfo teleportToHqButton;
        SubMenuButtonInfo teleportToNinjaVillage;
        SubMenuButtonInfo shuffleMusicButton;

        //Archipelago buttons
        SubMenuButtonInfo archipelagoHostButton;
        SubMenuButtonInfo archipelagoPortButton;
        SubMenuButtonInfo archipelagoNameButton;
        SubMenuButtonInfo archipelagoPassButton;
        SubMenuButtonInfo archipelagoConnectButton;
        SubMenuButtonInfo archipelagoReleaseButton;
        SubMenuButtonInfo archipelagoCollectButton;
        SubMenuButtonInfo archipelagoHintButton;
        SubMenuButtonInfo archipelagoToggleMessagesButton;
        SubMenuButtonInfo archipelagoToggleFilterMessagesButton;
        SubMenuButtonInfo archipelagoToggleHintPopupButton;
        SubMenuButtonInfo archipelagoStatusButton;
        SubMenuButtonInfo archipelagoDeathLinkButton;
        SubMenuButtonInfo archipelagoMessageTimerButton;

        private TextMeshProUGUI apTextDisplay8;
        private TextMeshProUGUI apTextDisplay16;
        private TextMeshProUGUI apMessagesDisplay8;
        private TextMeshProUGUI apMessagesDisplay16;

        //Set up save data
        public override Type ModuleSaveType => typeof(RandoSave);
        public RandoSave Save => (RandoSave)ModuleSave;

        public override void Load()
        {
            Console.WriteLine("Randomizer loading and ready to try things!");
          
            //Initialize the randomizer state manager
            randoStateManager = new RandomizerStateManager();

            //Add Randomizer Version button
            versionButton = Courier.UI.RegisterSubMenuModOptionButton(
                () => "Messenger AP Randomizer: v" + ItemRandomizerUtil.GetModVersion(),
                null);

            //Add current seed number button
            seedNumButton = Courier.UI.RegisterSubMenuModOptionButton(
                () => "Current seed number: " + GetCurrentSeedNum(),
                null);

            //Add Reset rando mod button
            resetRandoSaveFileButton = Courier.UI.RegisterTextEntryModOptionButton(
                () => "Reset Randomizer File Slot", OnRandoFileResetConfirmation,
                1,
                () => "Are you sure you wish to reset your save file for randomizer play?(y/n)",
                () => "n",
                CharsetFlags.Letter);

            //Add windmill shuriken toggle button
            windmillShurikenToggleButton = Courier.UI.RegisterSubMenuModOptionButton(
                () => Manager<ProgressionManager>.Instance.useWindmillShuriken
                    ? "Active Regular Shurikens"
                    : "Active Windmill Shurikens",
                OnToggleWindmillShuriken);

            //Add teleport to HQ button
            teleportToHqButton = Courier.UI.RegisterSubMenuModOptionButton(
                () => "Teleport to HQ",
                OnSelectTeleportToHq);

            //Add teleport to Ninja Village button
            teleportToNinjaVillage = Courier.UI.RegisterSubMenuModOptionButton(
                () => "Teleport to Ninja Village",
                OnSelectTeleportToNinjaVillage);

            //Add Archipelago host button
            archipelagoHostButton = Courier.UI.RegisterTextEntryModOptionButton(
                () => "Enter Archipelago Host Name",
                OnSelectArchipelagoHost,
                30,
                () => "Enter the Archipelago host name.",
                () => ArchipelagoClient.ServerData?.Uri,
                CharsetFlags.Dash | CharsetFlags.Dot | CharsetFlags.Letter
                | CharsetFlags.Number | CharsetFlags.Space);

            //Add Archipelago port button
            archipelagoPortButton = Courier.UI.RegisterTextEntryModOptionButton(
                () => "Enter Archipelago Port",
                OnSelectArchipelagoPort,
                5,
                () => "Enter the port for the Archipelago session",
                () => ArchipelagoClient.ServerData?.Port.ToString(),
                CharsetFlags.Number);

            //Add archipelago name button
            archipelagoNameButton = Courier.UI.RegisterTextEntryModOptionButton(
                () => "Enter Archipelago Slot Name", OnSelectArchipelagoName,
                16,
                () => "Enter player name:",
                () => ArchipelagoClient.ServerData?.SlotName,
                CharsetFlags.Dash | CharsetFlags.Dot | CharsetFlags.Letter
                | CharsetFlags.Number | CharsetFlags.Space);

            //Add archipelago password button
            archipelagoPassButton = Courier.UI.RegisterTextEntryModOptionButton(
                () => "Enter Archipelago Password", OnSelectArchipelagoPass,
                30,
                () => "Enter session password:",
                () => ArchipelagoClient.ServerData?.Password);

            //Add Archipelago connection button
            archipelagoConnectButton = Courier.UI.RegisterSubMenuModOptionButton(
                () => "Connect to Archipelago",
                OnSelectArchipelagoConnect);

            //Add Archipelago status button
            archipelagoStatusButton = Courier.UI.RegisterSubMenuModOptionButton(
                () => ArchipelagoClient.DisplayStatus
                    ? "Hide status information"
                    : "Display status information",
                OnToggleAPStatus);
            
            //Add Archipelago message button
            archipelagoToggleMessagesButton = Courier.UI.RegisterSubMenuModOptionButton(
                () => ArchipelagoClient.DisplayAPMessages
                    ? "Hide server messages"
                    : "Display server messages",
                OnToggleAPMessages);
            
            //Add Archipelago filter messages button
            archipelagoToggleFilterMessagesButton = Courier.UI.RegisterSubMenuModOptionButton(
                () => ArchipelagoClient.FilterAPMessages
                    ? "Show all server messages"
                    : "Filter messages to only relevant to me",
                () => ArchipelagoClient.FilterAPMessages = !ArchipelagoClient.FilterAPMessages);
            
            //Add Archipelago hint popup button
            archipelagoToggleHintPopupButton = Courier.UI.RegisterSubMenuModOptionButton(
                () => ArchipelagoClient.HintPopUps ? "Disable hint popups" : "Enable hint popups",
                () => ArchipelagoClient.HintPopUps = !ArchipelagoClient.HintPopUps);

            //Add Archipelago message display timer button
            archipelagoMessageTimerButton = Courier.UI.RegisterTextEntryModOptionButton(
                () => "AP Message Display Time",
                entry => OnSelectMessageTimer(entry),
                1,
                () => "Enter amount of time to display Archipelago messages, in seconds",
                () => updateTime.ToString(), CharsetFlags.Number);

            //Add Archipelago death link button
            archipelagoDeathLinkButton = Courier.UI.RegisterSubMenuModOptionButton(
                () => ArchipelagoData.DeathLink
                    ? "Disable Death Link"
                    : "Enable Death Link",
                OnToggleDeathLink);

            //Add Archipelago hint button
            archipelagoHintButton = Courier.UI.RegisterTextEntryModOptionButton(
                () => "Hint for an item",
                OnSelectArchipelagoHint,
                30,
                () => "Enter item name:");

            //Add Archipelago release button
            archipelagoReleaseButton = Courier.UI.RegisterSubMenuModOptionButton(
                () => "Release remaining items",
                OnSelectArchipelagoRelease);

            //Add Archipelago collect button
            archipelagoCollectButton = Courier.UI.RegisterSubMenuModOptionButton(
                () => "Collect remaining items",
                OnSelectArchipelagoCollect);

            shuffleMusicButton = Courier.UI.RegisterSubMenuModOptionButton(
                () => RandoMusicManager.ShuffleMusic
                    ? "Disable Music Shuffle"
                    : "Enable Music Shuffle",
                () => RandoMusicManager.ShuffleMusic = !RandoMusicManager.ShuffleMusic);

            //Plug in my code :3
            On.InventoryManager.AddItem += InventoryManager_AddItem;
            On.ProgressionManager.SetChallengeRoomAsCompleted += ProgressionManager_SetChallengeRoomAsCompleted;
            On.HasItem.IsTrue += HasItem_IsTrue;
            On.AwardNoteCutscene.ShouldPlay += AwardNoteCutscene_ShouldPlay;
            On.CutsceneHasPlayed.IsTrue += CutsceneHasPlayed_IsTrue;
            On.SaveGameSelectionScreen.OnLoadGame += SaveGameSelectionScreen_OnLoadGame;
            On.SaveGameSelectionScreen.OnNewGame += SaveGameSelectionScreen_OnNewGame;
            On.BackToTitleScreen.GoBackToTitleScreen += PauseScreen_OnQuitToTitle;
            On.NecrophobicWorkerCutscene.Play += NecrophobicWorkerCutscene_Play;
            IL.RuxxtinNoteAndAwardAmuletCutscene.Play += RuxxtinNoteAndAwardAmuletCutscene_Play;
            On.DialogCutscene.Play += DialogCutscene_Play;
            On.CatacombLevelInitializer.OnBeforeInitDone += CatacombLevelInitializer_OnBeforeInitDone;
            On.DialogManager.LoadDialogs_ELanguage += DialogChanger.LoadDialogs_Elanguage;
            // // shop management
            On.UpgradeButtonData.GetPrice += RandoShopManager.GetPrice;
            On.BuyMoneyWrenchCutscene.OnBuyWrenchChoice += RandoShopManager.BuyMoneyWrench;
            On.BuyMoneyWrenchCutscene.EndCutsceneOnDialogDone += RandoShopManager.EndMoneyWrenchCutscene;
            On.MoneySinkUnclogCutscene.OnDialogOutDone += RandoShopManager.UnclogSink;
            On.GoToSousSolCutscene.EndCutScene += RandoShopManager.GoToSousSol;
            On.IronHoodShopScreen.GetFigurineData += RandoShopManager.GetFigurineData;
            On.SousSol.UnlockFigurine += RandoShopManager.UnlockFigurine;
            On.Shop.Init += RandoShopManager.ShopInit;
            On.JukeboxTrack.IsUnlocked += (orig, self) => true;
            On.AudioManager.PlayMusic += RandoMusicManager.OnPlayMusic;
            On.UpgradeButtonData.IsStoryUnlocked += RandoShopManager.IsStoryUnlocked;
            // boss management
            On.ProgressionManager.HasDefeatedBoss +=
                (orig, self, bossName) => RandoBossManager.HasBossDefeated(bossName);
            On.ProgressionManager.HasEverDefeatedBoss +=
                (orig, self, bossName) => RandoBossManager.HasBossDefeated(bossName);
            On.ProgressionManager.SetBossAsDefeated +=
                (orig, self, bossName) => RandoBossManager.SetBossAsDefeated(bossName);
            // level teleporting etc management
            On.Level.ChangeRoom += RandoRoomManager.Level_ChangeRoom;
            //These functions let us override and manage power seals ourselves with 'fake' items
            On.ProgressionManager.TotalPowerSealCollected += ProgressionManager_TotalPowerSealCollected;
            On.ShopChestOpenCutscene.OnChestOpened += (orig, self) =>
                RandomizerStateManager.Instance.PowerSealManager?.OnShopChestOpen(orig, self);
            On.ShopChestChangeShurikenCutscene.Play += (orig, self) =>
                RandomizerStateManager.Instance.PowerSealManager?.OnShopChestOpen(orig, self);
            //update loops for Archipelago
            Courier.Events.PlayerController.OnUpdate += PlayerController_OnUpdate;
            On.InGameHud.OnGUI += InGameHud_OnGUI;
            On.SaveManager.DoActualSaving += SaveManager_DoActualSave;
            On.PlayerController.Die += OnPlayerDie;
            On.Quarble.OnDeathScreenDone += OnDeathScreenDone;
            // On.MegaTimeShard.NextState += RandoTimeShardManager.NextState;
            // On.MegaTimeShard.ReceiveHit += RandoTimeShardManager.ReceiveHit;
            On.MegaTimeShard.OnBreakDone += MegaTimeShard_OnBreakDone;
            On.DialogSequence.GetDialogList += DialogSequence_GetDialogList;
            On.LevelManager.EndLevelLoading += RandoLevelManager.EndLevelLoading;
            On.Cutscene.Play += Cutscene_Play;
            //temp add
            #if DEBUG
            On.InventoryManager.GetItemQuantity += InventoryManager_GetItemQuantity;
            On.PhantomIntroCutscene.OnEnterRoom += PhantomIntro_OnEnterRoom; //this lets us skip the phantom fight
            On.UIManager.ShowView += UIManager_ShowView;
            On.MusicBox.SetNotesState += MusicBox_SetNotesState;
            On.PowerSeal.OnEnterRoom += PowerSeal_OnEnterRoom;
            On.LevelManager.LoadLevel += RandoLevelManager.LoadLevel;
            #endif

            Console.WriteLine("Randomizer finished loading!");
        }

        public override void Initialize()
        {
            //I only want the generate seed/enter seed mod options available when not in the game.
            resetRandoSaveFileButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE;
            //Also the AP buttons
            archipelagoHostButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE && !ArchipelagoClient.Authenticated;
            archipelagoPortButton.IsEnabled = () => !ArchipelagoClient.Authenticated &&
                                                    (Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE
                                                    || (Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE
                                                        && ArchipelagoClient.HasConnected));
            archipelagoNameButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE && !ArchipelagoClient.Authenticated;
            archipelagoPassButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE && !ArchipelagoClient.Authenticated;
            archipelagoConnectButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE && !ArchipelagoClient.Authenticated;

            //Options I only want working while actually in the game
            seedNumButton.IsEnabled = () => ArchipelagoClient.HasConnected && randoStateManager.CurrentFileSlot != 0;
            windmillShurikenToggleButton.IsEnabled = () => ArchipelagoClient.ServerData?.ReceivedItems != null &&
                                                           ArchipelagoClient.ServerData.ReceivedItems.ContainsKey(
                                                               ItemsAndLocationsHandler.ItemFromEItem(
                                                                   EItems.WINDMILL_SHURIKEN));
            teleportToHqButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE &&
                                                 RandomizerStateManager.IsSafeTeleportState();
            teleportToNinjaVillage.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() != 
                ELevel.NONE && Manager<ProgressionManager>.Instance.HasCutscenePlayed("ElderAwardSeedCutscene") && 
                RandomizerStateManager.IsSafeTeleportState();
            shuffleMusicButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE;
            
            //These AP buttons can exist in or out of game
            archipelagoStatusButton.IsEnabled = () => ArchipelagoClient.Authenticated;
            archipelagoToggleMessagesButton.IsEnabled = () => ArchipelagoClient.Authenticated;
            archipelagoToggleFilterMessagesButton.IsEnabled = () =>
                ArchipelagoClient.Authenticated && ArchipelagoClient.DisplayAPMessages;
            archipelagoToggleHintPopupButton.IsEnabled = () => ArchipelagoClient.Authenticated;
            archipelagoDeathLinkButton.IsEnabled = () => ArchipelagoClient.Authenticated;
            archipelagoMessageTimerButton.IsEnabled = () => ArchipelagoClient.DisplayStatus;
            archipelagoHintButton.IsEnabled = ArchipelagoClient.CanHint;
            archipelagoReleaseButton.IsEnabled = ArchipelagoClient.CanRelease;
            archipelagoCollectButton.IsEnabled = ArchipelagoClient.CanCollect;

            #if DEBUG
            SceneManager.sceneLoaded += OnSceneLoadedRando;
            #endif
            
            //Options always available
            versionButton.IsEnabled = () => true;
            
            //load config
            Debug.Log("Loading config from APConfig.toml");
            try
            {
                var path = Courier.ModsFolder.Replace("/Mods", "");
                UserConfig.ReadConfig(path);
            }
            catch (Exception e) {Console.Write(e);}
        }

        //temp function for seal research
        void PowerSeal_OnEnterRoom(On.PowerSeal.orig_OnEnterRoom orig, PowerSeal self, bool teleportedInRoom)
        {
            //just print out some info for me
            Console.WriteLine($"Entered power seal room: {Manager<Level>.Instance.GetRoomAtPosition(self.transform.position).roomKey}");
            orig(self, teleportedInRoom);
        }

        List<DialogInfo> DialogSequence_GetDialogList(On.DialogSequence.orig_GetDialogList orig, DialogSequence self)
        {
            //Using this function to add some of my own dialog stuff to the game.
            if (ArchipelagoClient.HasConnected &&
                new[] { "ARCHIPELAGO_ITEM", "DEATH_LINK" }.Contains(self.dialogID))
            {
                var dialogInfoList = new List<DialogInfo>();
                var dialog = new DialogInfo();
                switch (self.dialogID)
                {
                    case "ARCHIPELAGO_ITEM":
                        dialog.text = self.name;
                        break;
                    case "DEATH_LINK":
                        dialog.text = $"Deathlink: {self.name}";
                        break;
                }

                dialogInfoList.Add(dialog);

                return dialogInfoList;
            }

            return orig(self);
        }

        void InventoryManager_AddItem(On.InventoryManager.orig_AddItem orig, InventoryManager self, EItems itemId, int quantity)
        {
            if (!itemId.Equals(EItems.TIME_SHARD))
            {
                Debug.Log($"Called InventoryManager_AddItem method. Looking to give x{quantity} amount of item '{itemId}'.");
                if (quantity == ItemsAndLocationsHandler.APQuantity)
                {
                    orig(self, itemId, 1);
                    return;
                }
                if (randoStateManager.IsLocationRandomized(itemId, out var randoItemCheck))
                {
                    if (itemId.Equals(EItems.CANDLE) &&
                        randoStateManager.IsLocationRandomized(EItems.TEA_SEED, out var seedCheck) &&
                        !RandomizerStateManager.HasCompletedCheck(seedCheck))
                    {
                        ItemsAndLocationsHandler.SendLocationCheck(seedCheck);
                    }
                    ItemsAndLocationsHandler.SendLocationCheck(randoItemCheck);
                    return;
                }
            }
            //Call original add with items
            orig(self, itemId, quantity);
            
        }

        void ProgressionManager_SetChallengeRoomAsCompleted(On.ProgressionManager.orig_SetChallengeRoomAsCompleted orig, ProgressionManager self, string roomKey)
        {
            //if this is a rando file, go ahead and give the item we expect to get
            if (ArchipelagoClient.HasConnected)
            {
                var powerSealLocation =
                    ItemsAndLocationsHandler.ArchipelagoLocations.Find(loc => loc.Equals(roomKey));
                ItemsAndLocationsHandler.SendLocationCheck(powerSealLocation);
            }
            //For now calling the orig method once we are done so the game still things we are collecting seals. We can change this later.
            orig(self, roomKey);
        }

        bool HasItem_IsTrue(On.HasItem.orig_IsTrue orig, HasItem self)
        {
            bool hasItem = false;
            //Check to make sure this is an item that was randomized and make sure we are not ignoring this specific trigger check
            if (ArchipelagoClient.HasConnected && randoStateManager.IsLocationRandomized(self.item, out var check) && !RandomizerConstants.GetSpecialTriggerNames().Contains(self.Owner.name))
            {
                if (self.transform.parent != null && "InteractionZone".Equals(self.Owner.name) && RandomizerConstants.GetSpecialTriggerNames().Contains(self.transform.parent.name) && EItems.KEY_OF_LOVE != self.item)
                {
                    //Special triggers that need to use normal logic, call orig method. This also includes the trigger check for the key of love on the sunken door because yeah.
                    Console.WriteLine($"While checking if player HasItem in an interaction zone, found parent object '{self.transform.parent.name}' in ignore logic. Calling orig HasItem logic.");
                    return orig(self);
                }

                //OLD WAY
                //Don't actually check for the item i have, check to see if I have the item that was at it's location.
                //int itemQuantity = Manager<InventoryManager>.Instance.GetItemQuantity(randoStateManager.CurrentLocationToItemMapping[check].Item);
                
                //NEW WAY
                //Don't actually check for the item I have, check to see if I have done this check before. We'll do this by seeing if the item at its location has been collected yet or not
                int itemQuantity = RandomizerStateManager.HasCompletedCheck(check) ? 1 : 0;
                if (self.item.Equals(EItems.CANDLE) && itemQuantity == 1)
                {
                    var seed = ItemsAndLocationsHandler.LocationFromEItem(EItems.TEA_SEED);
                    var leaves = ItemsAndLocationsHandler.LocationFromEItem(EItems.TEA_LEAVES);
                    itemQuantity = RandomizerStateManager.HasCompletedCheck(seed) &&
                                   RandomizerStateManager.HasCompletedCheck(leaves)
                        ? 1
                        : 0;
                }
                
                switch (self.conditionOperator)
                {
                    case EConditionOperator.LESS_THAN:
                        hasItem = itemQuantity < self.quantityToHave;
                        break;
                    case EConditionOperator.LESS_OR_EQUAL:
                        hasItem = itemQuantity <= self.quantityToHave;
                        break;
                    case EConditionOperator.EQUAL:
                        hasItem = itemQuantity == self.quantityToHave;
                        break;
                    case EConditionOperator.GREATER_OR_EQUAL:
                        hasItem = itemQuantity >= self.quantityToHave;
                        break;
                    case EConditionOperator.GREATER_THAN:
                        hasItem = itemQuantity > self.quantityToHave;
                        break;
                }
                return hasItem;
            }
            Console.WriteLine("HasItem check was not randomized. Doing vanilla checks.");
            Debug.Log($"Is randomized file : '{ArchipelagoClient.HasConnected}' | Is location '{self.item}' randomized: '{randoStateManager.IsLocationRandomized(self.item, out check)}' | Not in the special triggers list: '{!RandomizerConstants.GetSpecialTriggerNames().Contains(self.Owner.name)}'|");
            return orig(self);
            
        }
        
        int InventoryManager_GetItemQuantity(On.InventoryManager.orig_GetItemQuantity orig, InventoryManager self, EItems item)
        {
            //Just doing some logging here
            if (EItems.NONE.Equals(item))
            {
                Console.WriteLine($"INVENTORYMANAGER_GETITEMQUANTITY CALLED! Let's learn some stuff. Item: '{item}' | Quantity of said item: '{orig(self, item)}'");
            }
            //Manager<LevelManager>.Instance.onLevelLoaded
            return orig(self, item);
        }

        bool AwardNoteCutscene_ShouldPlay(On.AwardNoteCutscene.orig_ShouldPlay orig, AwardNoteCutscene self)
        {
            //Need to handle note cutscene triggers so they will play as long as I dont have the actual item it grants
            if (!randoStateManager.IsLocationRandomized(self.noteToAward, out var noteCheck))
                return orig(self);
            var shouldPlay = !ArchipelagoClient.ServerData.CheckedLocations.Contains(noteCheck);
            if (shouldPlay)
                ItemsAndLocationsHandler.SendLocationCheck(noteCheck);
            return shouldPlay;
        }

        bool CutsceneHasPlayed_IsTrue(On.CutsceneHasPlayed.orig_IsTrue orig, CutsceneHasPlayed self)
        {
            if (RandomizerConstants.GetCutsceneMappings().ContainsKey(self.cutsceneId) &&
                randoStateManager.IsLocationRandomized(RandomizerConstants.GetCutsceneMappings()[self.cutsceneId],
                    out var cutsceneCheck))
            {
                return RandomizerStateManager.HasCompletedCheck(cutsceneCheck);
            }
            return orig(self);
        }

        void SaveGameSelectionScreen_OnLoadGame(On.SaveGameSelectionScreen.orig_OnLoadGame orig, SaveGameSelectionScreen self, int slotIndex)
        {
            //slotIndex is 0-based, going to increment it locally to keep things simple.
            randoStateManager.CurrentFileSlot = slotIndex + 1;

            //This is probably a bad way to do this
            try
            {
                if (!clearedData)
                    RandoSave.TryLoad(Save.APSaveData);
                if (ArchipelagoData.LoadData(randoStateManager.CurrentFileSlot))
                {
                    Manager<DialogManager>.Instance.LoadDialogs(Manager<LocalizationManager>.Instance.CurrentLanguage);
                    //The player is connected to an Archipelago server and trying to load a save file so check it's valid
                    Console.WriteLine($"Successfully loaded Archipelago seed {randoStateManager.CurrentFileSlot}");
                    Console.WriteLine("Current Inventory:");
                    foreach (var item in randoStateManager.APSave[randoStateManager.CurrentFileSlot].ReceivedItems.Keys)
                    {
                        Console.WriteLine($"{item}: {randoStateManager.APSave[randoStateManager.CurrentFileSlot].ReceivedItems[item]}");
                    }
                }
                else if (ArchipelagoClient.Authenticated &&
                         string.IsNullOrEmpty(randoStateManager.APSave[randoStateManager.CurrentFileSlot].SlotName))
                {
                    ArchipelagoClient.ServerData.StartNewSeed();
                    //We force a reload of all dialog when loading the game
                    try
                    {
                        Manager<DialogManager>.Instance.LoadDialogs(Manager<LocalizationManager>.Instance.CurrentLanguage);   
                    } catch (Exception e){Console.WriteLine(e);}
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (!ArchipelagoClient.HasConnected)
            {
                Console.WriteLine(
                    $"This file slot ({randoStateManager.CurrentFileSlot}) has no seed generated or is not " +
                    "a randomized file. Resetting the mappings and putting game items back to normal.");
                ArchipelagoClient.ServerData = new ArchipelagoData();
            }

            orig(self, slotIndex);
            RandomizerStateManager.Instance.InGame = true;
            Manager<AudioManager>.Instance.levelMusicShuffle = RandoMusicManager.ShuffleMusic;
            RandoMusicManager.BuildMusicLibrary();
        }

        void SaveGameSelectionScreen_OnNewGame(On.SaveGameSelectionScreen.orig_OnNewGame orig, SaveGameSelectionScreen self, SaveSlotUI slot)
        {
            orig(self, slot);
        }

        void PauseScreen_OnQuitToTitle(On.BackToTitleScreen.orig_GoBackToTitleScreen orig)
        {
            if (ArchipelagoClient.HasConnected)
            {
                if (ArchipelagoClient.Authenticated)
                {
                    ArchipelagoClient.Disconnect();
                }
                ArchipelagoClient.HasConnected = false;
                RandoBossManager.DefeatedBosses = new List<string>();
            }
            randoStateManager = new RandomizerStateManager();
            ArchipelagoClient.ServerData = null;
            
            clearedData = false;
            orig();
            Console.WriteLine("returned to title");
            RandomizerStateManager.Instance.InGame = false;
        }

        //Fixing necro cutscene check
        void CatacombLevelInitializer_OnBeforeInitDone(On.CatacombLevelInitializer.orig_OnBeforeInitDone orig, CatacombLevelInitializer self)
        {
            //check to see if we already have the item at Necro check
            if(ArchipelagoClient.HasConnected)
            {
                if (!RandomizerStateManager.HasCompletedCheck(
                        ItemsAndLocationsHandler.LocationsLookup[new LocationRO("Necro")]))
                    self.necrophobicWorkerCutscene.Play();
                else self.necrophobicWorkerCutscene.phobekin.gameObject.SetActive(false);
                //Call our overriden fixing function
                RandoCatacombLevelInitializer.FixPlayerStuckInChallengeRoom();
            }
            else
            {
                //we are not rando here, call orig method
                orig(self);
            }
            
        }

        // Breaking into Necro cutscene to fix things
        void NecrophobicWorkerCutscene_Play(On.NecrophobicWorkerCutscene.orig_Play orig, NecrophobicWorkerCutscene self)
        {
            //Cutscene moves Ninja around, lets see if i can stop it by making that "location" the current location the player is.
            self.playerStartPosition = UnityEngine.Object.FindObjectOfType<PlayerController>().transform;
            orig(self);
        }

        void RuxxtinNoteAndAwardAmuletCutscene_Play(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(55)))
            {
                cursor.EmitDelegate<Func<EItems, EItems>>(GetRandoItemByItem);
            }
            
        }

        void MegaTimeShard_OnBreakDone(On.MegaTimeShard.orig_OnBreakDone orig, MegaTimeShard self)
        {
            var currentLevel = Manager<LevelManager>.Instance.GetCurrentLevelEnum();
            var currentRoom = Manager<Level>.Instance.CurrentRoom.roomKey;
            RandoTimeShardManager.BreakShard(new RandoTimeShardManager.MegaShard(currentLevel, currentRoom));
            orig(self);
        }

        int ProgressionManager_TotalPowerSealCollected(On.ProgressionManager.orig_TotalPowerSealCollected orig,
            ProgressionManager self)
        {
            return randoStateManager.PowerSealManager?.AmountPowerSealsCollected() ?? orig(self);
        }

        void Cutscene_Play(On.Cutscene.orig_Play orig, Cutscene self)
        {
            var eventName = self.GetType().ToString();
#if DEBUG
            Console.WriteLine($"Playing cutscene: {eventName}");
#endif
            if (ArchipelagoClient.EventsICareAbout.Contains(eventName))
            {
                ArchipelagoClient.Session.DataStorage[Scope.Slot, "Events"] +=
                    new List<string> { eventName };
            }
            orig(self);
        }

        void PhantomIntro_OnEnterRoom(On.PhantomIntroCutscene.orig_OnEnterRoom orig, PhantomIntroCutscene self,
            bool teleportedInRoom)
        {
            if (randoStateManager.SkipPhantom)
            {
                Manager<AudioManager>.Instance.StopMusic();
                UnityEngine.Object.FindObjectOfType<PhantomOutroCutscene>().Play();
            }
            else
            {
                orig(self, teleportedInRoom);
            }
        }


        View UIManager_ShowView(On.UIManager.orig_ShowView orig, UIManager self, Type viewType,
            EScreenLayers layer, IViewParams screenParams, bool transitionIn, AnimatorUpdateMode animUpdateMode)
        {
            Console.WriteLine($"viewType {viewType}");
            Console.WriteLine($"layer {layer}");
            Console.WriteLine($"params {screenParams}");
            Console.WriteLine($"transition {transitionIn}");
            Console.WriteLine($"updateMode {animUpdateMode}");
            return orig(self, viewType, layer, screenParams, transitionIn, animUpdateMode);
        }

        void DialogCutscene_Play(On.DialogCutscene.orig_Play orig, DialogCutscene self)
        {
            // Console.WriteLine($"Playing dialog cutscene: {self}");
            //ruxxtin cutscene is being a bitch so just gonna hard code around it here.
            if (ArchipelagoClient.HasConnected && self.name.Equals("ReadNote"))
            {
                if (randoStateManager.IsLocationRandomized(EItems.RUXXTIN_AMULET, out var locID))
                {
                    if (!RandomizerStateManager.HasCompletedCheck(locID))
                    {
                        ItemsAndLocationsHandler.SendLocationCheck(locID);
                    }
                }
            }
            orig(self);
        }

        void MusicBox_SetNotesState(On.MusicBox.orig_SetNotesState orig, MusicBox self)
        {
            // this determines which notes should be shown present in the music box
            orig(self);
        }

        bool OnRandoFileResetConfirmation(string answer)
        {
            
            if(!"y".Equals(answer.ToLowerInvariant()))
            {
                return true;
            }

            ArchipelagoData.ClearData();
            randoStateManager = new RandomizerStateManager();
            clearedData = true;
            try
            {
                Save.APSaveData = RandoSave.GetSaveData();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            string path = Application.persistentDataPath + "/SaveGame.txt";
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(RandomizerConstants.SAVE_FILE_STRING);
            }
            
            Manager<SaveManager>.Instance.LoadSaveGame();
            //Delete the existing save file selection ui since it really wants to hold on to the previous saves data.
            GameObject.Destroy(Manager<UIManager>.Instance.GetView<SaveGameSelectionScreen>().gameObject);
            //Reinit the save file selection ui.
            SaveGameSelectionScreen selectionScreen = Manager<UIManager>.Instance.ShowView<SaveGameSelectionScreen>(EScreenLayers.MAIN, null, false, AnimatorUpdateMode.Normal);
            selectionScreen.GoOffscreenInstant();

            return true;
        }

        public static void OnToggleWindmillShuriken()
        {
            //Toggle Shuriken
            Manager<ProgressionManager>.Instance.useWindmillShuriken = !Manager<ProgressionManager>.Instance.useWindmillShuriken;
            //Update UI
            InGameHud view = Manager<UIManager>.Instance.GetView<InGameHud>();
            if (view != null)
            {
                view.UpdateShurikenVisibility();
            }
        }

        void OnSelectTeleportToHq()
        {

            //Properly close out of the mod options and get the game state back together
            Manager<PauseManager>.Instance.Resume();
            Manager<UIManager>.Instance.GetView<OptionScreen>().Close(false);                
            Console.WriteLine("Teleporting to HQ!");
            Courier.UI.ModOptionScreen.Close(false);

            //Fade the music out because musiception is annoying
            Manager<AudioManager>.Instance.FadeMusicVolume(1f, 0f, true);

            //Load the HQ
            Manager<TowerOfTimeHQManager>.Instance.TeleportInToTHQ(true, ELevelEntranceID.ENTRANCE_A, null, null, true);
        }

        void OnSelectTeleportToNinjaVillage()
        {
            Console.WriteLine("Attempting to teleport to Ninja Village.");
            
            // Properly close out of the mod options and get the game state back together
            Manager<PauseManager>.Instance.Resume();
            Manager<UIManager>.Instance.GetView<OptionScreen>().Close(false);
            Courier.UI.ModOptionScreen.Close(false);
            EBits dimension = Manager<DimensionManager>.Instance.currentDimension;

            //Fade the music out because musiception is annoying
            Manager<AudioManager>.Instance.FadeMusicVolume(1f, 0f, true);

            //Load to Ninja Village
            Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition = new Vector2(-153.3f, -56.5f);
            LevelLoadingInfo levelLoadingInfo = new LevelLoadingInfo("Level_01_NinjaVillage_Build", false, true, LoadSceneMode.Single, ELevelEntranceID.NONE, dimension);
            Manager<LevelManager>.Instance.LoadLevel(levelLoadingInfo);
        }

        bool OnSelectArchipelagoHost(string answer)
        {
            if (answer == null) return true;
            if (ArchipelagoClient.ServerData == null) ArchipelagoClient.ServerData = new ArchipelagoData();
            var uri = answer;
            ArchipelagoClient.ServerData.Uri = uri;
            return true;
        }

        bool OnSelectArchipelagoPort(string answer)
        {
            if (answer == null) return true;
            if (ArchipelagoClient.ServerData == null) ArchipelagoClient.ServerData = new ArchipelagoData();
            int.TryParse(answer, out var port);
            ArchipelagoClient.ServerData.Port = port;
            return true;
        }

        bool OnSelectArchipelagoName(string answer)
        {
            if (answer == null) return true;
            if (ArchipelagoClient.ServerData == null) ArchipelagoClient.ServerData = new ArchipelagoData();
            ArchipelagoClient.ServerData.SlotName = answer;
            return true;
        }

        bool OnSelectArchipelagoPass(string answer)
        {
            if (answer == null) return true;
            if (ArchipelagoClient.ServerData == null) ArchipelagoClient.ServerData = new ArchipelagoData();
            ArchipelagoClient.ServerData.Password = answer;
            return true;
        }

        void OnSelectArchipelagoConnect()
        {
            if (ArchipelagoClient.ServerData == null) ArchipelagoClient.ServerData = new ArchipelagoData();
            if (ArchipelagoClient.ServerData.SlotName.IsNullOrEmpty())
            {
                if (!UserConfig.SlotName.IsNullOrEmpty())
                {
                    ArchipelagoClient.ServerData.Uri = UserConfig.HostName;
                    ArchipelagoClient.ServerData.Port = UserConfig.Port;
                    ArchipelagoClient.ServerData.SlotName = UserConfig.SlotName;
                    ArchipelagoClient.ServerData.Password = UserConfig.Password;
                }
                return;
            }
            
            ArchipelagoClient.ConnectAsync(archipelagoConnectButton);
        }

        void OnSelectArchipelagoRelease()
        {
            ArchipelagoClient.Session.Socket.SendPacket(new SayPacket { Text = "!release" });
        }

        void OnSelectArchipelagoCollect()
        {
            ArchipelagoClient.Session.Socket.SendPacket(new SayPacket { Text = "!collect" });
        }

        bool OnSelectArchipelagoHint(string answer)
        {
            ArchipelagoClient.Session.Socket.SendPacket(new SayPacket { Text = $"!hint {answer}" });
            return true;
        }

        static void OnToggleAPStatus()
        {
            ArchipelagoClient.DisplayStatus = !ArchipelagoClient.DisplayStatus;
        }

        static void OnToggleAPMessages()
        {
            ArchipelagoClient.DisplayAPMessages = !ArchipelagoClient.DisplayAPMessages;
        }

        static void OnToggleDeathLink()
        {
            ArchipelagoData.DeathLink = !ArchipelagoData.DeathLink;
            if (ArchipelagoData.DeathLink) ArchipelagoClient.DeathLinkHandler.DeathLinkService.EnableDeathLink();
            else ArchipelagoClient.DeathLinkHandler.DeathLinkService.DisableDeathLink();
        }

        bool OnSelectMessageTimer(string answer)
        {
            if (answer == null) return true;
            if (ArchipelagoClient.ServerData == null) ArchipelagoClient.ServerData = new ArchipelagoData();
            int.TryParse(answer, out var newTime);
            updateTime = newTime;
            return true;
        }

        /// <summary>
        /// Delegate function for getting rando item. This can be used by IL hooks that need to make this call later.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private EItems GetRandoItemByItem(EItems item)
        {
            if (!randoStateManager.IsLocationRandomized(item, out var ruxxAmuletLocation)) return item;
            Console.WriteLine($"IL Wackiness -- Checking for Item '{item}' | Rando item to return '{randoStateManager.ScoutedLocations[ruxxAmuletLocation].Item}'");

            return EItems.POTION;
        }

        private string GetCurrentSeedNum()
        {
            string seedNum = "Unknown";

            if (ArchipelagoClient.HasConnected)
            {
                seedNum = ArchipelagoClient.ServerData.SeedName;
            }

            return seedNum;
        }

        private void OnSceneLoadedRando(Scene scene, LoadSceneMode mode)
        {
            Console.WriteLine($"Scene loaded: '{scene.name}'");
        }

        private void PlayerController_OnUpdate(PlayerController controller)
        {
            if (!ArchipelagoClient.HasConnected || randoStateManager.CurrentFileSlot == 0) return;
            ArchipelagoClient.DeathLinkHandler.Player = controller;
            if (RandomizerStateManager.IsSafeTeleportState() && !Manager<PauseManager>.Instance.IsPaused)
                ArchipelagoClient.DeathLinkHandler.KillPlayer();
            // ItemsAndLocationsHandler.UnlockItems();
            //This updates every {updateTime} seconds
            updateTimer += Time.deltaTime;
            if (!(updateTimer >= updateTime)) return;
            apMessagesDisplay16.text = apMessagesDisplay8.text = ArchipelagoClient.UpdateMessagesText();
            updateTimer = 0;
            if (Manager<PlayerManager>.Instance.Player.InputBlocked() ||
                Manager<GameManager>.Instance.IsCutscenePlaying()) return;
            ArchipelagoClient.UpdateArchipelagoState();
        }

        private void InGameHud_OnGUI(On.InGameHud.orig_OnGUI orig, InGameHud self)
        {
            orig(self);
            if (apTextDisplay8 == null)
            {
                apTextDisplay8 = UnityEngine.Object.Instantiate(self.hud_8.coinCount, self.hud_8.gameObject.transform);
                apTextDisplay16 = UnityEngine.Object.Instantiate(self.hud_16.coinCount, self.hud_16.gameObject.transform);
                apTextDisplay8.transform.Translate(0f, -110f, 0f);
                apTextDisplay16.transform.Translate(0f, -110f, 0f);
                apTextDisplay16.fontSize = apTextDisplay8.fontSize = UserConfig.StatusTextSize;
                apTextDisplay16.alignment = apTextDisplay8.alignment = TextAlignmentOptions.TopRight;
                apTextDisplay16.enableWordWrapping = apTextDisplay8.enableWordWrapping = true;
                apTextDisplay16.color = apTextDisplay8.color = Color.white;

                apMessagesDisplay8 = UnityEngine.Object.Instantiate(self.hud_8.coinCount, self.hud_8.gameObject.transform);
                apMessagesDisplay16 = UnityEngine.Object.Instantiate(self.hud_16.coinCount, self.hud_16.gameObject.transform);
                apMessagesDisplay8.transform.Translate(0f, -200f, 0f);
                apMessagesDisplay16.transform.Translate(0f, -200f, 0f);
                apMessagesDisplay16.fontSize = apMessagesDisplay8.fontSize = UserConfig.MessageTextSize;
                apMessagesDisplay16.alignment = apMessagesDisplay8.alignment = TextAlignmentOptions.BottomRight;
                apMessagesDisplay16.enableWordWrapping = apMessagesDisplay16.enableWordWrapping = true;
                apMessagesDisplay16.color = apMessagesDisplay8.color = Color.green;
                apMessagesDisplay16.text = apMessagesDisplay8.text = string.Empty;
            }
            //This updates every frame
            apTextDisplay16.fontSize = apTextDisplay8.fontSize = UserConfig.StatusTextSize;
            apMessagesDisplay16.fontSize = apMessagesDisplay8.fontSize = UserConfig.MessageTextSize;
            apTextDisplay16.text = apTextDisplay8.text = ArchipelagoClient.UpdateStatusText();
        }

        private void SaveManager_DoActualSave(On.SaveManager.orig_DoActualSaving orig, SaveManager self, bool applySaveDelay = true)
        {
            if (ArchipelagoClient.HasConnected)
            {
                // The game calls the save method after the ending cutscene before rolling credits
                if (ArchipelagoClient.Authenticated
                    && Manager<LevelManager>.Instance.GetCurrentLevelEnum().Equals(ELevel.Level_Ending))
                {
                    ArchipelagoClient.UpdateClientStatus(ArchipelagoClientState.ClientGoal);
                }
                if (randoStateManager.CurrentFileSlot == 0) return;
                var saveSlot = self.GetCurrentSaveGameSlot();
                var playerAlias =
                    ArchipelagoClient.Session.Players.GetPlayerAlias(ArchipelagoClient.Session.ConnectionInfo.Slot);
                saveSlot.SlotName = playerAlias;
            }
            Save?.Update();
            orig(self, applySaveDelay);
        }

        private void OnPlayerDie(On.PlayerController.orig_Die orig, PlayerController self, EDeathType deathType,
            Transform killedBy)
        {
            try
            {
                Manager<UIManager>.Instance.CloseAllScreensOfType<AwardItemPopup>(false);
                ArchipelagoClient.DeathLinkHandler.SendDeathLink(deathType, killedBy);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            orig(self, deathType, killedBy);
        }

        private void OnDeathScreenDone(On.Quarble.orig_OnDeathScreenDone orig, Quarble self)
        {
            orig(self);
            // this code doesn't expect music shuffle lmao
            Manager<AudioManager>.Instance.StopMusic();
        }
    }
}
