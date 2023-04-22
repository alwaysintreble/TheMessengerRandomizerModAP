using MessengerRando.Archipelago;
using MessengerRando.RO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MessengerRando.Utils
{
    /// <summary>
    /// A static class to handle replacement of Dialogs for items.
    /// Could be made better by adding the award text string to the LocationRO for each item 
    /// </summary>
    public static class DialogChanger
    {
        public static readonly Dictionary<EItems, string> ItemDialogID = new Dictionary<EItems, string>
        {
            { EItems.CLIMBING_CLAWS, "AWARD_GRIMPLOU" },
            { EItems.WINGSUIT, "AWARD_WINGSUIT" },
            { EItems.GRAPLOU, "AWARD_ROPE_DART" },
            { EItems.FAIRY_BOTTLE, "AWARD_FAIRY" },
            { EItems.MAGIC_BOOTS, "AWARD_MAGIC_BOOTS" },
            { EItems.SEASHELL, "AWARD_MAGIC_SEASHELL" },
            { EItems.RUXXTIN_AMULET, "AWARD_AMULET" },
            { EItems.TEA_SEED, "AWARD_SEED" },
            { EItems.TEA_LEAVES, "AWARD_ASTRAL_LEAVES" },
            { EItems.POWER_THISTLE, "AWARD_THISTLE" },
            { EItems.CANDLE, "AWARD_CANDLE" },
            { EItems.DEMON_KING_CROWN, "AWARD_CROWN" },
            { EItems.WINDMILL_SHURIKEN, "AWARD_WINDMILL" },
            { EItems.KEY_OF_HOPE, "AWARD_KEY_OF_HOPE" },
            { EItems.KEY_OF_STRENGTH, "AWARD_KEY_OF_STRENGTH" },
            { EItems.KEY_OF_CHAOS, "AWARD_KEY_OF_CHAOS" },
            { EItems.KEY_OF_LOVE, "AWARD_KEY_OF_LOVE" },
            { EItems.KEY_OF_SYMBIOSIS, "AWARD_KEY_OF_SYMBIOSIS" },
            { EItems.KEY_OF_COURAGE, "AWARD_KEY_OF_COURAGE"},
            { EItems.SUN_CREST, "AWARD_SUN_CREST" },
            { EItems.MOON_CREST, "AWARD_MOON_CREST" },
            { EItems.PYROPHOBIC_WORKER, "FIND_PYRO" },
            { EItems.ACROPHOBIC_WORKER, "FIND_ACRO" },
            { EItems.NECROPHOBIC_WORKER, "NECRO_PHOBEKIN_DIALOG" },
            { EItems.CLAUSTROPHOBIC_WORKER, "FIND_CLAUSTRO" },
        };

        /// <summary>
        /// The initial generation of the dictionary of dialog replacement based on the currently randomized item locations
        /// </summary>
        /// <returns>A Dictionary containing keys of locationdialogID and values of replacementdialogID</returns>
        /// 
        public static Dictionary<string, string> GenerateDialogMappingforItems()
        {
            Dictionary<string, string> dialogmap = new Dictionary<string, string>();

            foreach (var pair in ItemDialogID)
            {
                dialogmap[pair.Value] = "ARCHIPELAGO_ITEM";
            }

            return dialogmap;
        }

        public static void CreateDialogBox(string text)
        {
            Console.WriteLine($"Drawing text box for {text}");
            var dialogBox = ScriptableObject.CreateInstance<DialogSequence>();
            dialogBox.dialogID = "ARCHIPELAGO_ITEM";
            dialogBox.name = text;
            dialogBox.choices = new List<DialogSequenceChoice>();
            var popupParams = new AwardItemPopupParams(dialogBox, true);
            Manager<UIManager>.Instance.ShowView<AwardItemPopup>(EScreenLayers.PROMPT, popupParams, true);
        }

        /// <summary>
        /// Runs whenever the locale is loaded\changed. This should allow it to work in any language.
        /// Works by loading and replacing all dialogs and then using reflection to call the onlanguagechanged event on the localization manager to update all dialog to the correct text.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="language"></param>
        public static void LoadDialogs_Elanguage(On.DialogManager.orig_LoadDialogs_ELanguage orig, DialogManager self, ELanguage language)
        {

            //Loads all the original dialog
            orig(self, language);

            if (ArchipelagoClient.HasConnected)
            {
                //Sets the field info so we can use reflection to get and set the private field.
                FieldInfo dialogByLocIDField = typeof(DialogManager).GetField("dialogByLocID", BindingFlags.NonPublic | BindingFlags.Instance);

                //Gets all loaded dialogs and makes a copy
                Dictionary<string, List<DialogInfo>> Loc = dialogByLocIDField.GetValue(self) as Dictionary<string, List<DialogInfo>>;
                Dictionary<string, List<DialogInfo>> LocCopy = new Dictionary<string, List<DialogInfo>>(Loc);


                //Before we randomize get some fixed GOT ITEM text to replace text for Phoebekins
                // List<DialogInfo> awardTextDialogList = Manager<DialogManager>.Instance.GetDialog("AWARD_GRIMPLOU");
                // string awardText = awardTextDialogList[0].text;
                // int replaceindexstart = awardText.IndexOf(">", 1);
                // int replaceindexend = awardText.IndexOf("<", replaceindexstart);
                // string toreplace = awardText.Substring(replaceindexstart + 1, replaceindexend - replaceindexstart - 1);
                //
                // //Phobekin text
                // string phobeText = Manager<LocalizationManager>.Instance.GetText("UI_PHOBEKINS_TITLE").ToLower();
                // phobeText = char.ToUpper(phobeText[0]) + phobeText.Substring(1); //Ugly way to uppercase the first letter.


                //Load the randomized mappings for an IF check so it doesn't run randomizer logic and replace itself with itself.

                //Loop through each dialog replacement - Will output the replacements to log for debugging
                foreach (KeyValuePair<string, List<DialogInfo>> KVP in Loc)
                {
                    string tobereplacedKey = KVP.Key;
                    string replacewithKey = "ARCHIPELAGO_ITEM";


                    if (ItemDialogID.ContainsValue(tobereplacedKey))
                    {
                        //Sets them to be all center and no portrait (This really only applies to phobekins but was 
                        LocCopy[tobereplacedKey][0].autoClose = false;
                        LocCopy[tobereplacedKey][0].autoCloseDelay = 0;
                        LocCopy[tobereplacedKey][0].characterDefinition = null;
                        LocCopy[tobereplacedKey][0].forcedPortraitOrientation = 0;
                        LocCopy[tobereplacedKey][0].position = EDialogPosition.CENTER;
                        LocCopy[tobereplacedKey][0].skippable = true;
                        if (RandomizerStateManager.Instance.ScoutedLocations != null &&
                            RandomizerStateManager.Instance.IsLocationRandomized(
                                ItemDialogID.First(x => x.Value.Equals(tobereplacedKey)).Key,
                                out var locationID))
                        {
                            LocCopy[tobereplacedKey][0].text =
                                RandomizerStateManager.Instance.ScoutedLocations[locationID].ToReadableString();
                        }

                        //This will replace the dialog for a phobekin to be its name in an award text
                        // switch (replacewithKey)
                        // {
                        //     case "FIND_ACRO":
                        //         string acro = Manager<LocalizationManager>.Instance.GetText("PHOBEKIN_ACRO_NAME");
                        //         acro = acro.Replace("<color=#00fcfc>", "");
                        //         acro = acro.Replace("</color>", "");
                        //         LocCopy[tobereplacedKey][0].text = awardText.Replace(toreplace, acro + " " + phobeText);
                        //         break;
                        //     case "FIND_PYRO":
                        //         string pyro = Manager<LocalizationManager>.Instance.GetText("PHOBEKIN_PYRO_NAME");
                        //         pyro = pyro.Replace("<color=#00fcfc>", "");
                        //         pyro = pyro.Replace("</color>", "");
                        //         LocCopy[tobereplacedKey][0].text = awardText.Replace(toreplace, pyro + " " + phobeText);
                        //         break;
                        //     case "FIND_CLAUSTRO":
                        //         string claustro = Manager<LocalizationManager>.Instance.GetText("PHOBEKIN_CLAUSTRO_NAME");
                        //         claustro = claustro.Replace("<color=#00fcfc>", "");
                        //         claustro = claustro.Replace("</color>", "");
                        //         LocCopy[tobereplacedKey][0].text = awardText.Replace(toreplace, claustro + " " + phobeText);
                        //         break;
                        //     case "NECRO_PHOBEKIN_DIALOG":
                        //         string necro = Manager<LocalizationManager>.Instance.GetText("PHOBEKIN_NECRO_NAME");
                        //         necro = necro.Replace("<color=#00fcfc>", "");
                        //         necro = necro.Replace("</color>", "");
                        //         LocCopy[tobereplacedKey][0].text = awardText.Replace(toreplace, necro + " " + phobeText);
                        //         break;
                        // }

                        //This will remove all additional dialog that comes after the initial reward text
                        for (int i = LocCopy[tobereplacedKey].Count - 1; i > 0; i--)
                        {
                            LocCopy[tobereplacedKey].RemoveAt(i);
                        }
                    }
                }
                //Sets the replacements
                dialogByLocIDField.SetValue(self, LocCopy);

                //There is probably a better way to do this but I chose to use reflection to call all onLanguageChanged events to update the localization completely.
                if (Manager<LocalizationManager>.Instance != null)
                {
                    Type type = typeof(LocalizationManager);
                    FieldInfo field = type.GetField("onLanguageChanged", BindingFlags.NonPublic | BindingFlags.Instance);
                    MulticastDelegate eventDelegate = field.GetValue(Manager<LocalizationManager>.Instance) as MulticastDelegate;


                    if (eventDelegate != null)
                    {
                        foreach (Delegate eventHandler in eventDelegate.GetInvocationList())
                        {
                            eventHandler.Method.Invoke(eventHandler.Target, null);
                        }
                    }
                }
            }
        }
    }
}
