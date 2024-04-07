using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using MessengerRando.Archipelago;
using MessengerRando.GameOverrideManagers;
using MessengerRando.Overrides;
using MessengerRando.RO;
using MessengerRando.Utils;
using MessengerRando.Utils.Constants;
using Mod.Courier;
using Mod.Courier.Module;
using MonoMod.Cil;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using Object = UnityEngine.Object;

namespace MessengerRando 
{
    /// <summary>
    /// Where it all begins! This class defines and injects all the necessary for the mod.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class APRandomizerMain : CourierModule
    {
        private float updateTimer;
        public static float UpdateTime = 3.0f;

        private RandomizerStateManager randoStateManager;

        private TextMeshProUGUI apTextDisplay8;
        private TextMeshProUGUI apTextDisplay16;
        private TextMeshProUGUI apMessagesDisplay8;
        private TextMeshProUGUI apMessagesDisplay16;

        //Set up save data
        public override Type ModuleSaveType => typeof(RandoSave);
        // ReSharper disable once MemberCanBePrivate.Global
        public RandoSave Save => (RandoSave)ModuleSave;

        public override void Load()
        {
            Console.WriteLine("Randomizer loading and ready to try things!");

            //Initialize the randomizer state manager
            randoStateManager = new RandomizerStateManager();
            
            //Plug in my code :3
            On.InventoryManager.AddItem += InventoryManager_AddItem;
            On.ProgressionManager.SetChallengeRoomAsCompleted += ProgressionManager_SetChallengeRoomAsCompleted;
            On.HasItem.IsTrue += HasItem_IsTrue;
            On.AwardNoteCutscene.ShouldPlay += AwardNoteCutscene_ShouldPlay;
            On.CutsceneHasPlayed.IsTrue += CutsceneHasPlayed_IsTrue;
            On.SaveGameSelectionScreen.OnLoadGame += SaveGameSelectionScreen_OnLoadGame;
            On.SaveGameSelectionScreen.OnNewGame += SaveGameSelectionScreen_OnNewGame;
            On.SaveGameSelectionScreen.ConfirmSaveDelete += SaveGameSelectionScreen_ConfirmSaveDelete;
            On.SaveGameSelectionScreen.OnDeleteChoiceDone += SaveGameSelectionScreen_OnDelete;
            On.BackToTitleScreen.GoBackToTitleScreen += PauseScreen_OnQuitToTitle;
            On.NecrophobicWorkerCutscene.Play += NecrophobicWorkerCutscene_Play;
            IL.RuxxtinNoteAndAwardAmuletCutscene.Play += RuxxtinNoteAndAwardAmuletCutscene_Play;
            On.DialogCutscene.Play += DialogCutscene_Play;
            On.CatacombLevelInitializer.OnBeforeInitDone += CatacombLevelInitializer_OnBeforeInitDone;
            On.DialogManager.LoadDialogs_ELanguage += DialogChanger.LoadDialogs_Elanguage;
            On.OptionScreen.OnEnable += OnOptionScreenEnable;
            On.LostWoods.SetAsSolved += LostWoodsManager.OnSetAsSolved;
            // shop management
            On.UpgradeButtonData.GetPrice += RandoShopManager.GetPrice;
            On.LocalizationManager.GetText += RandoShopManager.GetText;
            // On.UpgradeButton.Refresh += RandoShopManager.UpgradeButton_Refresh;
            On.BuyMoneyWrenchCutscene.OnBuyWrenchChoice += RandoShopManager.BuyMoneyWrench;
            On.BuyMoneyWrenchCutscene.EndCutsceneOnDialogDone += RandoShopManager.EndMoneyWrenchCutscene;
            On.MoneySinkUnclogCutscene.OnDialogOutDone += RandoShopManager.UnclogSink;
            On.GoToSousSolCutscene.EndCutScene += RandoShopManager.GoToSousSol;
            On.IronHoodShopScreen.GetFigurineData += RandoShopManager.GetFigurineData;
            On.SousSol.UnlockFigurine += RandoShopManager.UnlockFigurine;
            On.Shop.Init += RandoShopManager.ShopInit;
            On.JukeboxTrack.IsUnlocked += (orig, self) => true;
            On.UpgradeButtonData.IsStoryUnlocked += RandoShopManager.IsStoryUnlocked;
            On.AudioManager.PlayMusic += RandoMusicManager.OnPlayMusic;
            // boss management
            On.ProgressionManager.HasDefeatedBoss +=
                (orig, self, bossName) => RandoBossManager.HasBossDefeated(bossName);
            On.ProgressionManager.HasEverDefeatedBoss +=
                (orig, self, bossName) => RandoBossManager.HasBossDefeated(bossName);
            On.ProgressionManager.SetBossAsDefeated +=
                (orig, self, bossName) => RandoBossManager.SetBossAsDefeated(bossName);
            // level teleporting etc management
            On.Level.ChangeRoom += RandoRoomManager.Level_ChangeRoom;
            On.LevelManager.LoadLevel += RandoLevelManager.LoadLevel;
            On.LevelManager.EndLevelLoading += RandoLevelManager.EndLevelLoading;
            On.ElementalSkylandsLevelInitializer.OnBeforeInitDone += RandoLevelManager.ElementalSkylandsInit;
            // On.PortalOpeningCutscene.OnOpenPortalEvent += RandoPortalManager.OpenPortalEvent;
            On.TotHQ.LeaveToLevel += RandoPortalManager.LeaveHQ;
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
            On.Cutscene.Play += Cutscene_Play;
            On.PlayerController.Awake += OnPlayerController_Awake;
            //temp add
            #if DEBUG
            On.PhantomIntroCutscene.OnEnterRoom += PhantomIntro_OnEnterRoom; //this lets us skip the phantom fight
            On.UIManager.ShowView += UIManager_ShowView;
            On.MusicBox.SetNotesState += MusicBox_SetNotesState;
            On.PowerSeal.OnEnterRoom += PowerSeal_OnEnterRoom;
            #endif

            Console.WriteLine("Randomizer finished loading!");
        }

        private void OnPlayerController_Awake(On.PlayerController.orig_Awake orig, PlayerController self)
        {
            // try {
            //     GameObject darkness = Courier.LoadFromAssetBundles<GameObject>("Assets/PrefabInstance/modded/DarkCave_LightStencil.prefab");
            //     Object.Instantiate(darkness, self.transform);
            // } catch(Exception e) {
            //     e.LogDetailed();
            // }

            orig(self);
        }

        private void OnOptionScreenEnable(On.OptionScreen.orig_OnEnable orig, OptionScreen self)
        {
            RandoSave.TryLoad(Save.APSaveData);
            orig(self);
        }

        public override void Initialize()
        {
            #if DEBUG
            SceneManager.sceneLoaded += OnSceneLoadedRando;
            #endif
            
            //load config
            Debug.Log("Loading config from APConfig.toml");
            try
            {
                var path = Courier.ModsFolder.Replace("/Mods", "");
                UserConfig.ReadConfig(path);
            }
            catch (Exception e) {Console.Write(e);}
            ArchipelagoMenu.BuildArchipelagoMenu();
            RandoMenu.BuildRandoMenu();
            HintMenu.BuildHintMenu();
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
                        Manager<DialogManager>.Instance.LoadDialogs(Manager<LocalizationManager>.Instance
                            .CurrentLanguage);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else if (ArchipelagoClient.Offline)
                {
                    if (ItemsAndLocationsHandler.ItemsLookup == null)
                        ItemsAndLocationsHandler.Initialize();
                    Manager<DialogManager>.Instance.LoadDialogs(Manager<LocalizationManager>.Instance.CurrentLanguage);
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
            Manager<AudioManager>.Instance.levelMusicShuffle = RandoMusicManager.ShuffleMusic;
            RandoMusicManager.BuildMusicLibrary();
        }

        void SaveGameSelectionScreen_OnNewGame(On.SaveGameSelectionScreen.orig_OnNewGame orig, SaveGameSelectionScreen self, SaveSlotUI slot)
        {
            Console.WriteLine("trying to load new game");
            try
            {
                Environment.GetCommandLineArgs();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (ArchipelagoClient.Authenticated)
                RandomizerStateManager.InitializeNewSecondQuest(self, slot.slotIndex);
            else if (ArchipelagoClient.Offline)
            {
                try
                {
                    RandomizerStateManager.InitializeNewSecondQuest(self, slot.slotIndex);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else if (Environment.GetCommandLineArgs().Length > 0)
            {
                Console.WriteLine("loading new game save... found command line args");
                foreach (var arg in Environment.GetCommandLineArgs())
                {
                    Console.WriteLine(arg);
                    if (!arg.Contains("archipelago")) continue;
                    var uri = new Uri(arg);
                    ArchipelagoClient.ServerData = new ArchipelagoData();
                    var userInfo = uri.UserInfo.Split(':');
                    ArchipelagoClient.ServerData.SlotName = userInfo[0];
                    ArchipelagoClient.ServerData.Password = userInfo[1] == "None" ? "" : userInfo[1];
                    ArchipelagoClient.ServerData.Uri = uri.Host;
                    ArchipelagoClient.ServerData.Port = uri.Port;
                    Console.WriteLine(ArchipelagoClient.ServerData.SlotName);
                    Console.WriteLine(ArchipelagoClient.ServerData.Password);
                    Console.WriteLine(ArchipelagoClient.ServerData.Uri);
                    Console.WriteLine(ArchipelagoClient.ServerData.Port);
                    var result = ArchipelagoClient.Connect();
                    if (ArchipelagoClient.Authenticated)
                    {
                        RandomizerStateManager.InitializeNewSecondQuest(self, slot.slotIndex);
                        return;
                    }
                    break;
                }
                orig(self, slot);
            }
            else
                orig(self, slot);
        }

        private void SaveGameSelectionScreen_ConfirmSaveDelete(On.SaveGameSelectionScreen.orig_ConfirmSaveDelete orig, SaveGameSelectionScreen self, SaveSlotUI slot)
        {
            randoStateManager.CurrentFileSlot = slot.slotIndex + 1;
            orig(self, slot);
        }

        private void SaveGameSelectionScreen_OnDelete(On.SaveGameSelectionScreen.orig_OnDeleteChoiceDone orig, SaveGameSelectionScreen self, bool delete)
        {
            if (delete)
            {
                RandoSave.TryLoad(Save.APSaveData);
                randoStateManager.APSave[randoStateManager.CurrentFileSlot] = new ArchipelagoData();
                Save?.ForceUpdate();
            }
            orig(self, delete);
        }
        
        void PauseScreen_OnQuitToTitle(On.BackToTitleScreen.orig_GoBackToTitleScreen orig)
        {
            if (ArchipelagoClient.HasConnected)
            {
                ArchipelagoClient.Disconnect();
                ArchipelagoClient.HasConnected = false;
                ArchipelagoClient.Offline = false;
                RandoBossManager.DefeatedBosses = new List<string>();
                RandoPortalManager.StartingPortals = null;
                RandoPortalManager.PortalMapping = null;
                RandoLevelManager.RandoLevelMapping = null;
                Manager<ProgressionManager>.Instance.powerSealTotal = 0;
            }
            randoStateManager = new RandomizerStateManager();
            ArchipelagoClient.ServerData = new ArchipelagoData();
            orig();
            Console.WriteLine("returned to title");
            Manager<UIManager>.Instance.GetView<InGameHud>().UpdateShurikenVisibility();
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
        private static void NecrophobicWorkerCutscene_Play(On.NecrophobicWorkerCutscene.orig_Play orig, NecrophobicWorkerCutscene self)
        {
            //Cutscene moves Ninja around, lets see if i can stop it by making that "location" the current location the player is.
            if (ArchipelagoClient.HasConnected)
                self.playerStartPosition = Object.FindObjectOfType<PlayerController>().transform;
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
            if (eventName == "PortalOpeningCutscene")
            {
                if (RandoPortalManager.StartingPortals != null)
                {
                    var progManager = Manager<ProgressionManager>.Instance;
                    foreach (var portal in RandoPortalManager.StartingPortals.Where(
                                 portal => !portal.StartsWith("Autumn")
                                           && !portal.StartsWith("Howling")
                                           && !portal.StartsWith("Glacial")))
                    {
                        var cutsceneName = $"{portal.Replace(" ", "")}OpeningCutscene";
                        progManager.cutscenesPlayed.Add(cutsceneName);
                    }
                }
            }
            if (ArchipelagoClient.EventsICareAbout.Contains(eventName) && ArchipelagoClient.Authenticated)
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
                Object.FindObjectOfType<PhantomOutroCutscene>().Play();
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
        
        public static void OnToggleWindmillShuriken()
        {
            Manager<ProgressionManager>.Instance.useWindmillShuriken = !Manager<ProgressionManager>.Instance.useWindmillShuriken;
            InGameHud view = Manager<UIManager>.Instance.GetView<InGameHud>();
            if (view != null)
                view.UpdateShurikenVisibility();
        }

        public static void OnSelectTeleportToHq()
        {
            Console.WriteLine("Teleporting to HQ!");       
#if DEBUG
            var position = Manager<PlayerManager>.Instance.Player.transform.position;
            Console.WriteLine($"{position.x} {position.y} {position.z}");
#endif
            ArchipelagoMenu.archipelagoScreen.Close(false);

            RandoLevelManager.CleanupBeforeTeleport();
            //Load the HQ
            Manager<TowerOfTimeHQManager>.Instance.TeleportInToTHQ(true, ELevelEntranceID.ENTRANCE_A, null);
        }

        public static void OnSelectTeleportToNinjaVillage()
        {
            Console.WriteLine("Teleporting to Ninja Village.");
            ArchipelagoMenu.archipelagoScreen.Close(false);
            EBits dimension = Manager<DimensionManager>.Instance.currentDimension;

            RandoLevelManager.CleanupBeforeTeleport();
            //Load to Ninja Village
            Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition = new Vector2(-153.3f, -56.5f);
            LevelLoadingInfo levelLoadingInfo = new LevelLoadingInfo("Level_01_NinjaVillage_Build", false, true, LoadSceneMode.Single, ELevelEntranceID.NONE, dimension);
            Manager<LevelManager>.Instance.LoadLevel(levelLoadingInfo);
        }

        public static bool OnSelectArchipelagoHost(string answer)
        {
            if (answer == null) return true;
            if (ArchipelagoClient.ServerData == null) ArchipelagoClient.ServerData = new ArchipelagoData();
            var uri = answer;
            ArchipelagoClient.ServerData.Uri = uri;
            return true;
        }

        public static bool OnSelectArchipelagoPort(string answer)
        {
            if (answer == null) return true;
            if (ArchipelagoClient.ServerData == null) ArchipelagoClient.ServerData = new ArchipelagoData();
            int.TryParse(answer, out var port);
            ArchipelagoClient.ServerData.Port = port;
            return true;
        }

        public static bool ArchipelagoPortEnabled()
        {
            if (Manager<LevelManager>.Instance.GetCurrentLevelEnum().Equals(ELevel.NONE))
            {
                return !ArchipelagoClient.HasConnected;
            }

            return ArchipelagoClient.HasConnected && !ArchipelagoClient.Authenticated && !ArchipelagoClient.Offline;
        }

        public static bool OnSelectArchipelagoName(string answer)
        {
            if (answer == null) return true;
            if (ArchipelagoClient.ServerData == null) ArchipelagoClient.ServerData = new ArchipelagoData();
            ArchipelagoClient.ServerData.SlotName = answer;
            return true;
        }

        public static bool OnSelectArchipelagoPass(string answer)
        {
            if (answer == null) return true;
            if (ArchipelagoClient.ServerData == null) ArchipelagoClient.ServerData = new ArchipelagoData();
            ArchipelagoClient.ServerData.Password = answer;
            return true;
        }

        public static void OnSelectArchipelagoConnect()
        {
            if (ArchipelagoClient.ServerData == null) ArchipelagoClient.ServerData = new ArchipelagoData();
            if (!UserConfig.SlotName.IsNullOrEmpty())
            {
                ArchipelagoClient.ServerData.Uri = UserConfig.HostName;
                ArchipelagoClient.ServerData.Port = UserConfig.Port;
                ArchipelagoClient.ServerData.SlotName = UserConfig.SlotName;
                ArchipelagoClient.ServerData.Password = UserConfig.Password;
            }
            else if (ArchipelagoClient.ServerData.SlotName.IsNullOrEmpty())
            {
                return;
            }
            
            ArchipelagoClient.ConnectAsync(ArchipelagoMenu.ArchipelagoConnectButton);
        }

        public static void OnSelectArchipelagoRelease()
        {
            ArchipelagoClient.Session.Socket.SendPacket(new SayPacket { Text = "!release" });
        }

        public static void OnSelectArchipelagoCollect()
        {
            ArchipelagoClient.Session.Socket.SendPacket(new SayPacket { Text = "!collect" });
        }

        public static bool OnSelectArchipelagoHint(string answer)
        {
            ArchipelagoClient.Session.Socket.SendPacket(new SayPacket { Text = $"!hint {answer}" });
            return true;
        }

        public static void OnToggleAPStatus()
        {
            ArchipelagoClient.DisplayStatus = !ArchipelagoClient.DisplayStatus;
        }

        public static void OnToggleAPMessages()
        {
            ArchipelagoClient.DisplayAPMessages = !ArchipelagoClient.DisplayAPMessages;
        }

        public static void OnToggleDeathLink()
        {
            ArchipelagoData.DeathLink = !ArchipelagoData.DeathLink;
            if (ArchipelagoData.DeathLink) ArchipelagoClient.DeathLinkHandler.DeathLinkService.EnableDeathLink();
            else ArchipelagoClient.DeathLinkHandler.DeathLinkService.DisableDeathLink();
        }

        public static bool OnSelectMessageTimer(string answer)
        {
            if (answer == null) return true;
            if (ArchipelagoClient.ServerData == null) ArchipelagoClient.ServerData = new ArchipelagoData();
            int.TryParse(answer, out var newTime);
            UpdateTime = newTime;
            return true;
        }

        /// <summary>
        /// Delegate function for getting rando item. This can be used by IL hooks that need to make this call later.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private EItems GetRandoItemByItem(EItems item)
        {
            return !randoStateManager.IsLocationRandomized(item, out var ruxxAmuletLocation) ? item : EItems.POTION;
        }

        public static string GetCurrentSeedNum()
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
            if (ArchipelagoClient.Authenticated && ArchipelagoClient.DeathLinkHandler.Player == null)
                ArchipelagoClient.DeathLinkHandler.Player = controller;
            if (RandomizerStateManager.IsSafeTeleportState() && !Manager<PauseManager>.Instance.IsPaused && ArchipelagoClient.Authenticated)
                ArchipelagoClient.DeathLinkHandler.KillPlayer();
            //This updates every {updateTime} seconds
            updateTimer += Time.deltaTime;
            TrapManager.TrapTimer += Time.deltaTime;
            if (!(updateTimer >= UpdateTime)) return;
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
                apTextDisplay8 = Object.Instantiate(self.hud_8.coinCount, self.hud_8.gameObject.transform);
                apTextDisplay16 = Object.Instantiate(self.hud_16.coinCount, self.hud_16.gameObject.transform);
                apTextDisplay8.transform.Translate(0f, -110f, 0f);
                apTextDisplay16.transform.Translate(0f, -110f, 0f);
                apTextDisplay16.fontSize = apTextDisplay8.fontSize = UserConfig.StatusTextSize;
                apTextDisplay16.alignment = apTextDisplay8.alignment = TextAlignmentOptions.TopRight;
                apTextDisplay16.enableWordWrapping = apTextDisplay8.enableWordWrapping = true;
                apTextDisplay16.color = apTextDisplay8.color = Color.white;

                apMessagesDisplay8 = Object.Instantiate(self.hud_8.coinCount, self.hud_8.gameObject.transform);
                apMessagesDisplay16 = Object.Instantiate(self.hud_16.coinCount, self.hud_16.gameObject.transform);
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
            // var checkpoint = Manager<ProgressionManager>.Instance.checkpointSaveInfo;
            // var pos = checkpoint.loadedLevelPlayerPosition;
            // Console.WriteLine($"{checkpoint.loadedLevelCheckpointIndex} \n{checkpoint.playerFacingDirection}\n {pos.x} {pos.y} {pos.z}");
            orig(self, applySaveDelay);
            if (!ArchipelagoClient.HasConnected) return;
            Save?.Update();
            try
            {
                if (RandoPortalManager.LeftHQPortal)
                {
                    RandoPortalManager.Teleport();
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (LostWoodsManager.NeedsUnsolved)
            {
                LostWoodsManager.UnsolveLostWoods();
            }
            else if (LostWoodsManager.NeedsSolved)
            {
                // just keep trying to solve it until it eventually works lmao
                LostWoodsManager.SolveLostWoods();
            }
                
            // The game calls the save method after the ending cutscene before rolling credits
            if (ArchipelagoClient.Authenticated
                && Manager<LevelManager>.Instance.GetCurrentLevelEnum().Equals(ELevel.Level_Ending))
            {
                ArchipelagoClient.UpdateClientStatus(ArchipelagoClientState.ClientGoal);
            }
        }

        private void OnPlayerDie(On.PlayerController.orig_Die orig, PlayerController self, EDeathType deathType,
            Transform killedBy)
        {
            try
            {
                TrapManager.ResetPlayerState();
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
