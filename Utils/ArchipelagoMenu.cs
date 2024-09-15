using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MessengerRando.Archipelago;
using MessengerRando.GameOverrideManagers;
using Mod.Courier;
using Mod.Courier.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MessengerRando.Utils
{
    /// <summary>
    /// Options menu for various archipelago stuff. has options to connect to a multiworld, teleporting while playing etc
    /// </summary>
    public class ArchipelagoMenu
    {
        public static OptionsButtonInfo ArchipelagoMenuButton;
        public static ArchipelagoScreen archipelagoScreen;

        public static SubMenuButtonInfo VersionButton;
        public static SubMenuButtonInfo SeedNumButton;

        public static SubMenuButtonInfo WindmillShurikenToggleButton;
        public static SubMenuButtonInfo TeleportToHqButton;
        public static SubMenuButtonInfo TeleportToNinjaVillage;
        public static SubMenuButtonInfo TeleportToSearingShop;
        public static SubMenuButtonInfo ShuffleMusicButton;

        public static SubMenuButtonInfo ArchipelagoHostButton;
        public static SubMenuButtonInfo ArchipelagoPortButton;
        public static SubMenuButtonInfo ArchipelagoNameButton;
        public static SubMenuButtonInfo ArchipelagoPassButton;
        public static SubMenuButtonInfo ArchipelagoConnectButton;

        public static SubMenuButtonInfo ArchipelagoReleaseButton;
        public static SubMenuButtonInfo ArchipelagoCollectButton;
        public static SubMenuButtonInfo ArchipelagoToggleMessagesButton;
        public static SubMenuButtonInfo ArchipelagoToggleFilterMessagesButton;
        public static SubMenuButtonInfo ArchipelagoToggleHintPopupButton;
        public static SubMenuButtonInfo ArchipelagoStatusButton;
        public static SubMenuButtonInfo ArchipelagoDeathLinkButton;
        public static SubMenuButtonInfo ArchipelagoMessageTimerButton;


        public static void DisplayArchipelagoMenu()
        {
            Manager<UIManager>.Instance.GetView<OptionScreen>().gameObject.SetActive(false);
            if (!ArchipelagoScreen.RandoScreenLoaded)
                archipelagoScreen =
                    ArchipelagoScreen.BuildModOptionScreen(Manager<UIManager>.Instance.GetView<OptionScreen>());

            Courier.UI.ShowView(archipelagoScreen, EScreenLayers.PROMPT, null, false);
        }

        public class ArchipelagoScreen : ModOptionScreen
        {
            public static bool RandoScreenLoaded;
            public static readonly List<OptionsButtonInfo> OptionButtons = new List<OptionsButtonInfo>();

            private static int OptionsCount()
            {
                return OptionButtons.Count(modOptionButton => modOptionButton.gameObject.activeInHierarchy);
            }

            private static int EnabledModOptionsBeforeButton(OptionsButtonInfo button)
            {
                int num = 0;
                foreach (OptionsButtonInfo modOptionButton in OptionButtons)
                {
                    if (modOptionButton.Equals(button))
                        return num;
                    if (modOptionButton.gameObject.activeInHierarchy)
                        ++num;
                }

                return -1;
            }

            public void InitOptionsViewWithModButtons()
            {
                var view = this;
                OptionScreen optionScreen = Manager<UIManager>.Instance.GetView<OptionScreen>();
                Transform parent = view.transform.Find("Container").Find("BackgroundFrame").Find("OptionsFrame")
                    .Find("OptionMenuButtons");
                foreach (OptionsButtonInfo button in OptionButtons)
                {
                    switch (button)
                    {
                        case ToggleButtonInfo _:
                            button.gameObject = Instantiate(optionScreen.fullscreenOption, parent);
                            break;
                        case SubMenuButtonInfo _:
                            button.gameObject = Instantiate(optionScreen.controlsButton.transform.parent.gameObject,
                                parent);
                            break;
                        case MultipleOptionButtonInfo _:
                            button.gameObject = Instantiate(optionScreen.languageOption, parent);
                            break;
                        default:
                            CourierLogger.Log(LogType.Warning, "OptionsMenu",
                                button.GetType() + " not a known type of OptionsButtonInfo!");
                            break;
                    }

                    button.gameObject.transform.SetAsLastSibling();
                    GameObject buttonGameObject = button.gameObject;
                    Func<string> getText = button.GetText;
                    string str = getText?.Invoke() ?? "Nameless Modded Options Button";
                    buttonGameObject.name = str;
                    button.addedTo = view;
                    foreach (TextMeshProUGUI componentsInChild in button.gameObject
                                 .GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (componentsInChild.name.Equals("OptionState") || componentsInChild.name.Equals("Text"))
                            button.stateTextMesh = componentsInChild;
                        if (componentsInChild.name.Equals("OptionName"))
                        {
                            button.nameTextMesh = componentsInChild;
                            TextLocalizer component = componentsInChild.GetComponent<TextLocalizer>();
                            if (component != null)
                                component.locID = "";
                        }
                    }

                    Button component1 = button.gameObject.transform.Find("Button").GetComponent<Button>();
                    component1.onClick = new Button.ButtonClickedEvent();
                    component1.onClick.AddListener(button.onClick);
                    button.OnInit(view);
                }

                parent.Find("Back")?.SetAsLastSibling();
            }

            public new static ArchipelagoScreen BuildModOptionScreen(OptionScreen optionScreen)
            {
                GameObject gameObject = new GameObject();
                ArchipelagoScreen archipelagoScreen = gameObject.AddComponent<ArchipelagoScreen>();
                OptionScreen newScreen = Instantiate(optionScreen);
                archipelagoScreen.name = "ArchipelagoScreen";
                // Swap everything under the option screen to the mod option screen
                // Iterate backwards so elements don't shift as lower ones are removed
                for (int i = newScreen.transform.childCount - 1; i >= 0; i--)
                {
                    newScreen.transform.GetChild(i).SetParent(archipelagoScreen.transform, false);
                }

                archipelagoScreen.optionMenuButtons = archipelagoScreen.transform.Find("Container")
                    .Find("BackgroundFrame")
                    .Find("OptionsFrame").Find("OptionMenuButtons");
                archipelagoScreen.backButton = archipelagoScreen.optionMenuButtons.Find("Back");
                // Delete OptionScreen buttons except for the Back button
                foreach (Transform child in archipelagoScreen.optionMenuButtons.GetChildren())
                {
                    if (!child.Equals(archipelagoScreen.backButton))
                        Destroy(child.gameObject);
                }

                //TODO put back if things brake
                // randoScreen.optionMenuButtons.DetachChildren();
                archipelagoScreen.backButton.SetParent(archipelagoScreen.optionMenuButtons);

                // Make back button take you to the OptionScreen instead of the pause menu
                Button button = archipelagoScreen.backButton.GetComponentInChildren<Button>();
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(archipelagoScreen.BackToOptionMenu);

                archipelagoScreen.InitStuffUnityWouldDo();

                archipelagoScreen.gameObject.SetActive(false);
                RandoScreenLoaded = true;
                return archipelagoScreen;
            }

            private void InitStuffUnityWouldDo()
            {
                //transform.position = new Vector3(0, Math.Max(-90 - heightPerButton * Courier.UI.ModOptionButtons.Count, startYMax));
                backgroundFrame = (RectTransform)transform.Find("Container").Find("BackgroundFrame");
                initialHeight = backgroundFrame.sizeDelta.y;
                gameObject.AddComponent<Canvas>();
            }

            private void Start()
            {
                InitOptions();
            }

            public override void Init(IViewParams screenParams)
            {
                base.Init(screenParams);

                InitOptionsViewWithModButtons();

                // Make the border frames blue
                Sprite borderSprite = backgroundFrame.GetComponent<Image>().sprite =
                    Courier.EmbeddedSprites["Mod.Courier.UI.mod_options_frame"];
                borderSprite.bounds.extents.Set(1.7f, 1.7f, 0.1f);
                borderSprite.texture.filterMode = FilterMode.Point;

                borderSprite = backgroundFrame.Find("OptionsFrame").GetComponent<Image>().sprite =
                    Courier.EmbeddedSprites["Mod.Courier.UI.mod_options_frame"];
                borderSprite.bounds.extents.Set(1.7f, 1.7f, 0.1f);
                borderSprite.texture.filterMode = FilterMode.Point;

                HideUnavailableOptions();
                InitOptions();
                SetInitialSelection();

                // Make the selection frames blue
                foreach (Image image in transform.GetComponentsInChildren<Image>()
                             .Where(c => c.name.Equals("SelectionFrame")))
                {
                    try
                    {
                        if (image.overrideSprite != null && image.overrideSprite.name != "Empty")
                        {
                            RenderTexture rt = new RenderTexture(image.overrideSprite.texture.width,
                                image.overrideSprite.texture.height, 0);
                            RenderTexture.active = rt;
                            Graphics.Blit(image.overrideSprite.texture, rt);

                            Texture2D res = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, true);
                            res.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);

                            Color[] pxls = res.GetPixels();
                            for (int i = 0; i < pxls.Length; i++)
                            {
                                if (Math.Abs(pxls[i].r - .973) < .01 && Math.Abs(pxls[i].g - .722) < .01)
                                {
                                    pxls[i].r = 0;
                                    pxls[i].g = .633f;
                                    pxls[i].b = 1;
                                }
                            }

                            res.SetPixels(pxls);
                            res.Apply();

                            Sprite sprite = image.overrideSprite = Sprite.Create(res,
                                new Rect(0, 0, res.width, res.height), new Vector2(16, 16), 20, 1,
                                SpriteMeshType.FullRect, new Vector4(5, 5, 5, 5));
                            sprite.bounds.extents.Set(.8f, .8f, 0.1f);
                            sprite.texture.filterMode = FilterMode.Point;
                        }
                    }
                    catch (Exception e)
                    {
                        CourierLogger.Log(LogType.Exception, "ArchipelagoScreen",
                            "Image not Read/Writeable when recoloring selection frames in ModOptionScreen");
                        e.LogDetailed();
                    }
                }
            }

            private IEnumerator WaitAndSelectInitialButton()
            {
                yield return null;
                SetInitialSelection();
            }

            private void OnEnable()
            {
                if (transform.parent != null)
                {
                    Manager<UIManager>.Instance.SetParentAndAlign(gameObject, transform.parent.gameObject);
                }

                EventSystem.current.SetSelectedGameObject(null);
            }

            private void OnDisable()
            {
                transform.position = defaultPos;
            }

            private void HideUnavailableOptions()
            {
                foreach (OptionsButtonInfo buttonInfo in OptionButtons)
                {
                    buttonInfo.gameObject.SetActive(buttonInfo.IsEnabled?.Invoke() ?? true);
                }

                foreach (OptionsButtonInfo buttonInfo in Courier.UI.ModOptionButtons)
                {
                    buttonInfo.gameObject.SetActive(false);
                }

                StartCoroutine(WaitAndSelectInitialButton());
                Vector2 sizeDelta = backgroundFrame.sizeDelta;
                backgroundFrame.sizeDelta =
                    new Vector2(sizeDelta.x, 110 + heightPerButton * OptionsCount());
            }

            // ReSharper disable Unity.PerformanceAnalysis
            private void SetInitialSelection()
            {
                GameObject defaultSelectionButton =
                    (initialSelection ? initialSelection : defaultSelection).transform.Find("Button").gameObject;
                defaultSelectionButton.transform.GetComponent<UIObjectAudioHandler>().playAudio = false;
                EventSystem.current.SetSelectedGameObject(defaultSelectionButton);
                defaultSelectionButton.GetComponent<Button>().OnSelect(null);
                defaultSelectionButton.GetComponent<UIObjectAudioHandler>().playAudio = true;
                initialSelection = null;
            }

            public new void GoOffscreenInstant()
            {
                gameObject.SetActive(false);
                RandoScreenLoaded = false;
            }

            // ReSharper disable Unity.PerformanceAnalysis
            public new int GetSelectedButtonIndex()
            {
                if (backButton.Find("Button").gameObject.Equals(EventSystem.current.currentSelectedGameObject))
                    return OptionsCount();
                foreach (OptionsButtonInfo buttonInfo in OptionButtons)
                {
                    if (buttonInfo.gameObject.transform.Find("Button").gameObject
                        .Equals(EventSystem.current.currentSelectedGameObject))
                    {
                        return EnabledModOptionsBeforeButton(buttonInfo);
                    }
                }

                return -1;
            }

            private void LateUpdate()
            {
                if (Manager<InputManager>.Instance.GetBackDown())
                {
                    BackToOptionMenu();
                }

                Vector3 windowOffset =
                    new Vector3(0,
                        Math.Min(GetSelectedButtonIndex(), Math.Max(0, OptionsCount() - 10)) *
                        .9f) - new Vector3(0, Math.Max(0, OptionsCount() - 11) * .45f);
                transform.position = defaultPos + windowOffset;

                foreach (OptionsButtonInfo buttonInfo in OptionButtons)
                {
                    buttonInfo.UpdateNameText();
                }

                // Make the selection frames blue
                // I should figure out if I can avoid doing this in LateUpdate()
                foreach (Image image in transform.GetComponentsInChildren<Image>()
                             .Where(c => c.name.Equals("SelectionFrame")))
                {
                    try
                    {
                        if (image.overrideSprite != null && image.overrideSprite.name.Equals("ShopItemSelectionFrame"))
                        {
                            RenderTexture rt = new RenderTexture(image.overrideSprite.texture.width,
                                image.overrideSprite.texture.height, 0);
                            RenderTexture.active = rt;
                            Graphics.Blit(image.overrideSprite.texture, rt);

                            Texture2D res = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, true);
                            res.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);

                            Color[] pxls = res.GetPixels();
                            for (int i = 0; i < pxls.Length; i++)
                            {
                                if (Math.Abs(pxls[i].r - .973) < .01 && Math.Abs(pxls[i].g - .722) < .01)
                                {
                                    pxls[i].r = 0;
                                    pxls[i].g = .633f;
                                    pxls[i].b = 1;
                                }
                            }

                            res.SetPixels(pxls);
                            res.Apply();

                            Sprite sprite = image.overrideSprite = Sprite.Create(res,
                                new Rect(0, 0, res.width, res.height), new Vector2(16, 16), 20, 1,
                                SpriteMeshType.FullRect, new Vector4(5, 5, 5, 5));
                            sprite.bounds.extents.Set(.8f, .8f, 0.1f);
                            sprite.texture.filterMode = FilterMode.Point;
                        }
                    }
                    catch (Exception e)
                    {
                        CourierLogger.Log(LogType.Exception, "ArchipelagoScreen",
                            "Image not Read/Writeable when recoloring selection frames in ModOptionScreen");
                        e.LogDetailed();
                    }
                }
            }

            private void InitOptions()
            {
                defaultSelection = backButton.gameObject;
                foreach (OptionsButtonInfo buttonInfo in OptionButtons)
                {
                    if (buttonInfo.IsEnabled?.Invoke() ?? true)
                    {
                        defaultSelection = buttonInfo.gameObject;
                        break;
                    }
                }

                backgroundFrame.Find("Title").GetComponent<TextMeshProUGUI>().SetText("Randomizer Options");
                foreach (OptionsButtonInfo buttonInfo in OptionButtons)
                {
                    buttonInfo.UpdateStateText();
                }
            }

            public new void BackToOptionMenu()
            {
                Close(false);
                Manager<UIManager>.Instance.GetView<OptionScreen>().gameObject.SetActive(true);
                ArchipelagoMenuButton.gameObject.transform.Find("Button").GetComponent<UIObjectAudioHandler>()
                    .playAudio = false;
                EventSystem.current.SetSelectedGameObject(ArchipelagoMenuButton.gameObject.transform.Find("Button")
                    .gameObject);
                ArchipelagoMenuButton.gameObject.transform.Find("Button").GetComponent<UIObjectAudioHandler>()
                    .playAudio = true;
            }

            public override void Close(bool transitionOut)
            {
                base.Close(transitionOut);
                RandoScreenLoaded = false;
            }
        }

        private static void RegisterRandoButton(OptionsButtonInfo buttonInfo) =>
            ArchipelagoScreen.OptionButtons.Add(buttonInfo);

        public static SubMenuButtonInfo RegisterSubRandoButton(Func<string> GetText, UnityAction onClick)
        {
            var buttonInfo = new SubMenuButtonInfo(GetText, onClick);
            RegisterRandoButton(buttonInfo);
            return buttonInfo;
        }

        public static ToggleButtonInfo RegisterToggleRandoButton(Func<string> getText, UnityAction onClick,
            Func<ToggleButtonInfo, bool> getState)
        {
            var buttonInfo = new ToggleButtonInfo(getText, onClick, getState,
                optionScreen => Manager<LocalizationManager>.Instance.GetText(ModOptionScreen.onLocID),
                optionScreen => Manager<LocalizationManager>.Instance.GetText(ModOptionScreen.offLocID));
            RegisterRandoButton(buttonInfo);
            return buttonInfo;
        }

        public static TextEntryButtonInfo RegisterTextRandoButton(Func<string> getText, Func<string, bool> onEntry,
            int maxCharacter = 15, Func<string> getEntryText = null, Func<string> getInitialText = null,
            TextEntryButtonInfo.CharsetFlags charset = TextEntryButtonInfo.CharsetFlags.Letter |
                                                       TextEntryButtonInfo.CharsetFlags.Number |
                                                       TextEntryButtonInfo.CharsetFlags.Dash |
                                                       TextEntryButtonInfo.CharsetFlags.Space)
        {
            var buttonInfo =
                new TextEntryButtonInfo(getText, onEntry, maxCharacter, getEntryText, getInitialText, charset);
            RegisterRandoButton(buttonInfo);
            return buttonInfo;
        }

        public static MultipleOptionButtonInfo RegisterMultipleRandoButton(Func<string> getText,
            UnityAction onClick,
            Action<int> onSwitch, Func<MultipleOptionButtonInfo, int> getIndex, Func<int, string> getTextForIndex)
        {
            var buttonInfo = new MultipleOptionButtonInfo(getText, onClick, onSwitch, getIndex, getTextForIndex);
            RegisterRandoButton(buttonInfo);
            return buttonInfo;
        }

        public static string GetTextForResetIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return "No";
                case 1:
                    return "Yes";
            }

            return "???";
        }

        public static void BuildArchipelagoMenu()
        {
            ArchipelagoMenuButton =
                Courier.UI.RegisterSubMenuOptionButton(
                    () => "Archipelago Randomizer",
                    DisplayArchipelagoMenu);

            // These should always be visible
            VersionButton =
                RegisterSubRandoButton(
                    () => "Messenger AP Randomizer: v" + ItemRandomizerUtil.GetModVersion(),
                    null);
            VersionButton.IsEnabled = () => true;

            // only visible while actually in the game
            //Add current seed number button
            SeedNumButton = RegisterSubRandoButton(
                () => "Current seed number: " + APRandomizerMain.GetCurrentSeedNum(),
                null);
            SeedNumButton.IsEnabled = () =>
                ArchipelagoClient.HasConnected && Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE;

            // these should only be visible from the main menu
            //Add Archipelago host button
            ArchipelagoHostButton = RegisterTextRandoButton(
                () => "Enter Archipelago Host Name",
                APRandomizerMain.OnSelectArchipelagoHost,
                30,
                () => "Enter the Archipelago host name.",
                () => ArchipelagoClient.ServerData?.Uri,
                TextEntryButtonInfo.CharsetFlags.Dash | TextEntryButtonInfo.CharsetFlags.Dot |
                TextEntryButtonInfo.CharsetFlags.Letter
                | TextEntryButtonInfo.CharsetFlags.Number | TextEntryButtonInfo.CharsetFlags.Space);
            ArchipelagoHostButton.IsEnabled = () =>
                Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE &&
                (!ArchipelagoClient.Authenticated || !ArchipelagoClient.Offline);

            //Add Archipelago port button
            ArchipelagoPortButton = RegisterTextRandoButton(
                () => "Enter Archipelago Port",
                APRandomizerMain.OnSelectArchipelagoPort,
                5,
                () => "Enter the port for the Archipelago session",
                () => ArchipelagoClient.ServerData?.Port.ToString(),
                TextEntryButtonInfo.CharsetFlags.Number);
            ArchipelagoPortButton.IsEnabled = APRandomizerMain.ArchipelagoPortEnabled;

            //Add archipelago name button
            ArchipelagoNameButton = RegisterTextRandoButton(
                () => "Enter Archipelago Slot Name", APRandomizerMain.OnSelectArchipelagoName,
                16,
                () => "Enter player name:",
                () => ArchipelagoClient.ServerData?.SlotName,
                TextEntryButtonInfo.CharsetFlags.Dash | TextEntryButtonInfo.CharsetFlags.Dot |
                TextEntryButtonInfo.CharsetFlags.Letter
                | TextEntryButtonInfo.CharsetFlags.Number | TextEntryButtonInfo.CharsetFlags.Space);
            ArchipelagoNameButton.IsEnabled = () =>
                Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE &&
                (!ArchipelagoClient.Authenticated || !ArchipelagoClient.Offline);

            //Add archipelago password button
            ArchipelagoPassButton = RegisterTextRandoButton(
                () => "Enter Archipelago Password",
                APRandomizerMain.OnSelectArchipelagoPass,
                30,
                () => "Enter session password:",
                () => ArchipelagoClient.ServerData?.Password);
            ArchipelagoPassButton.IsEnabled = () =>
                Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE &&
                (!ArchipelagoClient.Authenticated || !ArchipelagoClient.Offline);

            //Add Archipelago connection button
            ArchipelagoConnectButton = RegisterSubRandoButton(
                () => "Connect to Archipelago",
                APRandomizerMain.OnSelectArchipelagoConnect);
            ArchipelagoConnectButton.IsEnabled = () =>
                Manager<LevelManager>.Instance.GetCurrentLevelEnum() == ELevel.NONE &&
                (!ArchipelagoClient.Authenticated || !ArchipelagoClient.Offline);

            //Add windmill shuriken toggle button
            WindmillShurikenToggleButton = RegisterSubRandoButton(
                () => Manager<ProgressionManager>.Instance.useWindmillShuriken
                    ? "Active Regular Shurikens"
                    : "Active Windmill Shurikens",
                APRandomizerMain.OnToggleWindmillShuriken);
            WindmillShurikenToggleButton.IsEnabled =
                () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE &&
                      ArchipelagoClient.ServerData?.ReceivedItems != null &&
                      ArchipelagoClient.ServerData.ReceivedItems.ContainsKey(
                          ItemsAndLocationsHandler.ItemFromEItem(EItems.WINDMILL_SHURIKEN));

            //Add teleport to HQ button
            TeleportToHqButton = RegisterSubRandoButton(
                () => "Teleport to HQ",
                APRandomizerMain.OnSelectTeleportToHq);
            TeleportToHqButton.IsEnabled = () =>
                Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE &&
                RandomizerStateManager.IsSafeTeleportState();

            //Add teleport to Ninja Village button
            TeleportToNinjaVillage = RegisterSubRandoButton(
                () => "Teleport to Ninja Village",
                APRandomizerMain.OnSelectTeleportToNinjaVillage);
            TeleportToNinjaVillage.IsEnabled = () =>
                Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE &&
                RandomizerStateManager.IsSafeTeleportState() &&
                ArchipelagoClient.ServerData?.AvailableTeleports != null &&
                ArchipelagoClient.ServerData.AvailableTeleports[0] && RandoLevelManager.RandoLevelMapping == null;

            TeleportToSearingShop = RegisterSubRandoButton(() => "Teleport to Searing Crags",
                APRandomizerMain.OnSelectTeleportToSearing);
            TeleportToSearingShop.IsEnabled = () =>
                Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE &&
                RandomizerStateManager.IsSafeTeleportState() &&
                ArchipelagoClient.ServerData?.AvailableTeleports != null &&
                ArchipelagoClient.ServerData.AvailableTeleports[1] && RandoLevelManager.RandoLevelMapping == null;

            //Add Archipelago status button
            ArchipelagoStatusButton = RegisterSubRandoButton(
                () => ArchipelagoClient.DisplayStatus
                    ? "Hide status information"
                    : "Display status information",
                APRandomizerMain.OnToggleAPStatus);
            ArchipelagoStatusButton.IsEnabled = () => ArchipelagoClient.Authenticated;

            //Add Archipelago message button
            ArchipelagoToggleMessagesButton = RegisterSubRandoButton(
                () => ArchipelagoClient.DisplayAPMessages
                    ? "Hide server messages"
                    : "Display server messages",
                APRandomizerMain.OnToggleAPMessages);
            ArchipelagoToggleMessagesButton.IsEnabled = () => ArchipelagoClient.Authenticated;

            //Add Archipelago filter messages button
            ArchipelagoToggleFilterMessagesButton = RegisterSubRandoButton(
                () => ArchipelagoClient.FilterAPMessages
                    ? "Show all server messages"
                    : "Filter messages to only relevant to me",
                () => ArchipelagoClient.FilterAPMessages = !ArchipelagoClient.FilterAPMessages);
            ArchipelagoToggleFilterMessagesButton.IsEnabled = () =>
                ArchipelagoClient.Authenticated && ArchipelagoClient.DisplayAPMessages;

            //Add Archipelago hint popup button
            ArchipelagoToggleHintPopupButton = RegisterSubRandoButton(
                () => ArchipelagoClient.HintPopUps ? "Disable hint popups" : "Enable hint popups",
                () => ArchipelagoClient.HintPopUps = !ArchipelagoClient.HintPopUps);
            ArchipelagoToggleHintPopupButton.IsEnabled = () => ArchipelagoClient.Authenticated;

            //Add Archipelago message display timer button
            ArchipelagoMessageTimerButton = RegisterTextRandoButton(
                () => "AP Message Display Time",
                APRandomizerMain.OnSelectMessageTimer,
                1,
                () => "Enter amount of time to display Archipelago messages, in seconds",
                () => APRandomizerMain.UpdateTime.ToString(CultureInfo.InvariantCulture), TextEntryButtonInfo.CharsetFlags.Number);
            ArchipelagoMessageTimerButton.IsEnabled =
                () => ArchipelagoClient.Authenticated && ArchipelagoClient.DisplayStatus;

            ShuffleMusicButton = RegisterSubRandoButton(
                () => RandoMusicManager.ShuffleMusic
                    ? "Disable Music Shuffle"
                    : "Enable Music Shuffle",
                () => RandoMusicManager.ShuffleMusic = !RandoMusicManager.ShuffleMusic);
            ShuffleMusicButton.IsEnabled =
                () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE;

            //Add Archipelago death link button
            ArchipelagoDeathLinkButton = RegisterSubRandoButton(
                () => ArchipelagoData.DeathLink
                    ? "Disable Death Link"
                    : "Enable Death Link",
                APRandomizerMain.OnToggleDeathLink);
            ArchipelagoDeathLinkButton.IsEnabled = () => ArchipelagoClient.Authenticated;

            //Add Archipelago release button
            ArchipelagoReleaseButton = RegisterSubRandoButton(
                () => "Release remaining items",
                APRandomizerMain.OnSelectArchipelagoRelease);
            ArchipelagoReleaseButton.IsEnabled = ArchipelagoClient.CanRelease;

            //Add Archipelago collect button
            ArchipelagoCollectButton = RegisterSubRandoButton(
                () => "Collect remaining items",
                APRandomizerMain.OnSelectArchipelagoCollect);
            ArchipelagoCollectButton.IsEnabled = ArchipelagoClient.CanCollect;
        }
    }
}