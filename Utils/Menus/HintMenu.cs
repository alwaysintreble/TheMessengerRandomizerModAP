using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using MessengerRando.Archipelago;
using Mod.Courier;
using Mod.Courier.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace MessengerRando.Utils.Menus
{
    public class HintMenu
    {
        public static OptionsButtonInfo ArchipelagoHintMenuButton;
        private static HintScreen hintScreen;

        public static SubMenuButtonInfo archipelagoHintButton;
        public static SubMenuButtonInfo hintMenuTitleButton;

        private static Dictionary<long, List<OptionsButtonInfo>> hintButtons = new();
        
        public static void DisplayHintMenu()
        {
            Manager<UIManager>.Instance.GetView<OptionScreen>().gameObject.SetActive(false);
            if (!HintScreen.ConnectScreenLoaded)
                hintScreen = HintScreen.BuildModOptionScreen(Manager<UIManager>.Instance.GetView<OptionScreen>());
            
            Courier.UI.ShowView(hintScreen, EScreenLayers.PROMPT, null, false);
        }

        public class HintScreen : ModOptionScreen
        {
            // public new float heightPerButton = 30f;
            public static bool ConnectScreenLoaded;
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
                        case HintButtonInfo _:
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

            public new static HintScreen BuildModOptionScreen(OptionScreen optionScreen)
            {
                GameObject gameObject = new GameObject();
                HintScreen randoScreen = gameObject.AddComponent<HintScreen>();
                OptionScreen newScreen = Instantiate(optionScreen);
                randoScreen.name = "ConnectScreen";
                // Swap everything under the option screen to the mod option screen
                // Iterate backwards so elements don't shift as lower ones are removed
                for (int i = newScreen.transform.childCount - 1; i >= 0; i--)
                {
                    newScreen.transform.GetChild(i).SetParent(randoScreen.transform, false);
                }

                randoScreen.optionMenuButtons = randoScreen.transform.Find("Container").Find("BackgroundFrame")
                    .Find("OptionsFrame").Find("OptionMenuButtons");
                randoScreen.backButton = randoScreen.optionMenuButtons.Find("Back");
                // Delete OptionScreen buttons except for the Back button
                foreach (Transform child in randoScreen.optionMenuButtons.GetChildren())
                {
                    if (!child.Equals(randoScreen.backButton))
                        Destroy(child.gameObject);
                }

                //TODO put back if things brake
                // randoScreen.optionMenuButtons.DetachChildren();
                randoScreen.backButton.SetParent(randoScreen.optionMenuButtons);

                // Make back button take you to the OptionScreen instead of the pause menu
                Button button = randoScreen.backButton.GetComponentInChildren<Button>();
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(randoScreen.BackToOptionMenu);

                randoScreen.InitStuffUnityWouldDo();

                randoScreen.gameObject.SetActive(false);
                ConnectScreenLoaded = true;
                return randoScreen;
            }

            private void InitStuffUnityWouldDo()
            {
                // transform.position = new Vector3(0, Math.Max(-90 - heightPerButton * OptionsCount(), startYMax));
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
                        CourierLogger.Log(LogType.Exception, "ConnectScreen",
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
                ConnectScreenLoaded = false;
            }

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
                    buttonInfo.nameTextMesh.fontSize = 5f;
                    // buttonInfo.nameTextMesh.margin = new Vector4(0, 0, -15f, 0);
                    // var bounds = buttonInfo.nameTextMesh.margin;
                    // if (bounds.extents.x < 500f)
                    // {
                    //     bounds.extents.Set(120f, 12f, 0f);
                    // }
                    // Debug.Log($"button bounds extent: {bounds.extents.x}, {bounds.extents.y}, {bounds.extents.z}");
                    // Debug.Log($"button bounds size: {bounds.x}, {bounds.y}, {bounds.z}");
                    // buttonInfo.nameTextMesh.bounds = new ;
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
                        CourierLogger.Log(LogType.Exception, "RandoScreen",
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

                backgroundFrame.Find("Title").GetComponent<TextMeshProUGUI>().SetText("Archipelago Hint Menu");
                foreach (OptionsButtonInfo buttonInfo in OptionButtons)
                {
                    buttonInfo.UpdateStateText();
                }
            }

            public new void BackToOptionMenu()
            {
                Close(false);
                Manager<UIManager>.Instance.GetView<OptionScreen>().gameObject.SetActive(true);
                ArchipelagoHintMenuButton.gameObject.transform.Find("Button").GetComponent<UIObjectAudioHandler>()
                    .playAudio = false;
                EventSystem.current.SetSelectedGameObject(ArchipelagoHintMenuButton.gameObject.transform.Find("Button")
                    .gameObject);
                ArchipelagoHintMenuButton.gameObject.transform.Find("Button").GetComponent<UIObjectAudioHandler>()
                    .playAudio = true;
            }

            public override void Close(bool transitionOut)
            {
                base.Close(transitionOut);
                ConnectScreenLoaded = false;
            }
        }

        private static void RegisterRandoButton(OptionsButtonInfo buttonInfo) =>
            HintScreen.OptionButtons.Add(buttonInfo);

        public static SubMenuButtonInfo RegisterSubRandoButton(Func<string> GetText, UnityAction onClick)
        {
            var buttonInfo = new SubMenuButtonInfo(GetText, onClick);
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

        public static void onHintsUpdated(Hint[] hints)
        {
            Console.WriteLine("hints updated");
            foreach (var hint in hints)
            {
#if DEBUG
                Console.WriteLine(hint.ItemId);
                Console.WriteLine(hint.Status);
                Console.WriteLine(hint.Found);
#endif
                try
                {
                    if (hintButtons.ContainsKey(hint.LocationId))
                        UpdateHintEntry(hint);
                    else
                    {
                        if (hint.Found) continue;
                        if (hint.Status.Equals(HintStatus.Found))
                            hint.Status = HintStatus.Unspecified;
                        AddNewHintEntry(hint);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        
        private static void AddNewHintEntry(Hint hint)
        {
            if (hint.FindingPlayer.Equals(ArchipelagoClient.Session.ConnectionInfo.Slot) &&
                ItemsAndLocationsHandler.ShopLocation(hint.LocationId, out var shopLoc)) return;

            var newHint = new HintButtonInfo(null, null, null, null, null, hint);
            RegisterRandoButton(newHint);

            var blankSpace = RegisterSubRandoButton(() => "", null);
            hintButtons.Add(hint.LocationId, [newHint, blankSpace]);
        }

        public static void UpdateHintEntry(Hint hint)
        {
            var hintButtonSet = hintButtons[hint.LocationId];
            HintButtonInfo hintButton = (HintButtonInfo)hintButtonSet[0];
            if (hint.Found)
            {
                hintButton.IsEnabled = () => false;
                hintButtonSet[1].IsEnabled = () => false;
                return;
            }
            if (hint.Status.Equals(HintStatus.Found))
                hint.Status = HintStatus.Unspecified;
            hintButton.UpdateHint(hint);
            if (HintScreen.ConnectScreenLoaded)
            {
                hintButton.UpdateNameText();
                hintButton.UpdateStateText();
            }
        }

        public static void BuildHintMenu()
        {

            ArchipelagoHintMenuButton =
                Courier.UI.RegisterSubMenuOptionButton(() => "Hint Menu", DisplayHintMenu);
            ArchipelagoHintMenuButton.IsEnabled = () => ArchipelagoClient.Authenticated;
            AddBaseButtons();
        }

        private static void AddBaseButtons()
        {
            //Add Archipelago hint button
            archipelagoHintButton = RegisterTextRandoButton(
                () => "Hint for an item",
                APRandomizerMain.OnSelectArchipelagoHint,
                30,
                () => "Enter item name:");
            archipelagoHintButton.IsEnabled = ArchipelagoClient.CanHint;
            
            var blankSpace = RegisterSubRandoButton(() => "", null);
            blankSpace.IsEnabled = () => true;
        }

        public static void ReBuildHintMenu()
        {
            HintScreen.OptionButtons.Clear();
            hintButtons.Clear();
            AddBaseButtons();
        }
    }
}