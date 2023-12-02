﻿using MessengerRando.Archipelago;
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

            if (!ArchipelagoClient.HasConnected) return;
            //Sets the field info so we can use reflection to get and set the private field.
            FieldInfo dialogByLocIDField = typeof(DialogManager).GetField("dialogByLocID", BindingFlags.NonPublic | BindingFlags.Instance);

            //Gets all loaded dialogs and makes a copy
            Dictionary<string, List<DialogInfo>> Loc = dialogByLocIDField.GetValue(self) as Dictionary<string, List<DialogInfo>>;
            Dictionary<string, List<DialogInfo>> LocCopy = new Dictionary<string, List<DialogInfo>>(Loc);

            //Loop through each dialog replacement - Will output the replacements to log for debugging
            foreach (var replaceableKey in Loc.Select(kvp => kvp.Key).Where(toBeReplaced => ItemDialogID.ContainsValue(toBeReplaced)))
            {
                //Sets them to be all center and no portrait (This really only applies to phobekins but was 
                LocCopy[replaceableKey][0].autoClose = false;
                LocCopy[replaceableKey][0].autoCloseDelay = 0;
                LocCopy[replaceableKey][0].characterDefinition = null;
                LocCopy[replaceableKey][0].forcedPortraitOrientation = 0;
                LocCopy[replaceableKey][0].position = EDialogPosition.CENTER;
                LocCopy[replaceableKey][0].skippable = true;
                if (RandomizerStateManager.Instance.ScoutedLocations != null &&
                    RandomizerStateManager.Instance.IsLocationRandomized(
                        ItemDialogID.First(x => x.Value.Equals(replaceableKey)).Key,
                        out var locationID))
                {
                    LocCopy[replaceableKey][0].text =
                        RandomizerStateManager.Instance.ScoutedLocations[locationID].ToReadableString();
                }

                //This will remove all additional dialog that comes after the initial reward text
                for (int i = LocCopy[replaceableKey].Count - 1; i > 0; i--)
                {
                    LocCopy[replaceableKey].RemoveAt(i);
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
