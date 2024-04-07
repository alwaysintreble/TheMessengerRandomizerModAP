using System.Collections.Generic;
using MessengerRando.Utils;

namespace MessengerRando.GameOverrideManagers
{
    public static class FlavorDialogManager
    {
        private struct DialogBox
        {
            public string Text;
            public CharacterDefinition Speaker;
            public int Delay;

            public DialogBox(string text, ECharacter speaker, int delay)
            {
                Text = text;
                Speaker = Manager<DialogManager>.Instance.GetCharacterDefinition(speaker);
                Delay = delay;
            }
        }

        public static CharacterDefinition ProphetDefinition;
        public static CharacterDefinition MessengerDefinition;

        static List<DialogInfo> ModifyDialogInfo(string locID, List<DialogInfo> infoToReplace)
        {
            switch (locID)
            {
                case "PROPHET_HEY":
                    var prophetHey = infoToReplace[0];
                    prophetHey.text =
                        ProphetHeyReplacements[RandomizerStateManager.SeedRandom.Next(ProphetHeyReplacements.Count)];
                    prophetHey.skippable = false;
                    break;
                case "PROPHET_INTRO_3":
                    ProphetDefinition = infoToReplace[0].characterDefinition;
                    MessengerDefinition = infoToReplace[1].characterDefinition;
                    var newText =
                        ProphetIntroReplacements[
                            RandomizerStateManager.SeedRandom.Next(ProphetIntroReplacements.Count)];
                    var toRemove = 0;
                    var newInfoList = new List<DialogInfo>();
                    foreach (var kvp in newText)
                    {
                        CharacterDefinition replaceChar;
                        switch (kvp.Value)
                        {
                            case "Prophet":
                                replaceChar = ProphetDefinition;
                                break;
                            case "Messenger":
                                replaceChar = MessengerDefinition;
                                break;
                            default:
                                replaceChar = ProphetDefinition;
                                break;
                        }

                        var newInfo = new DialogInfo
                        {
                            text = kvp.Key,
                            textID = "PROPHET_INTRO_3",
                            characterDefinition = replaceChar,
                            skippable = false,
                            autoClose = true,
                            autoCloseDelay = 2
                        };
                        newInfoList.Add(newInfo);
                    }

                    infoToReplace = newInfoList;
                    break;
            }

            return infoToReplace;
        }

        public static void UpdateLoc(Dictionary<string, List<DialogInfo>> loc)
        {
            foreach (var textID in DialogToReplace)
            {
                loc[textID] = ModifyDialogInfo(textID, loc[textID]);
            }
        }

        static readonly List<string> DialogToReplace = new List<string>
        {
            "PROPHET_HEY",
            "PROPHET_INTRO_3",
        };

        static readonly List<string> ProphetHeyReplacements = new List<string>
        {
            "You have been <color=#66ff66>randomized</color>.",
            "Echo!",
        };

        static readonly List<Dictionary<string, string>> ProphetIntroReplacements = new List<Dictionary<string, string>>
        {
            new Dictionary<string, string>
            {
                { "Do you have any extra die?", "Prophet" },
                { "What? I have Quarble I guess.", "Messenger" },
                { "No, like for rolling. I haven't been able to find mine.", "Prophet" },
                { "For what?", "Messenger" },
                { "Well I figure I'll have to make a perception check at some point with all these texts.", "Prophet" },
                { "...", "Messenger" },
                { "Can you at least have a look around to see if you can find them?", "Prophet" },
                { "Sure.", "Messenger" },
                {
                    "<event=OpenPortals>*ahem* <color=#fd465b>BEHOLD</color>, and may you see success on your journey oh yee valiant <color=#6844fc>Messenger</color>.",
                    "Prophet"
                },
                { "Uh. Right. Thanks.", "Messenger" },
            },
            new Dictionary<string, string>
            {
                { "<event=OpenPortals>I don't think we're in <color=#66ff66>Kansas</color> anymore.", "Prophet" },
                { "And I forgot my red shoes.", "Messenger" },
            },
            new Dictionary<string, string>
            {
                { "Do you ever think about what we're doing here?", "Prophet" },
                { "Something about this Music Box thing, right?", "Messenger" },
                { "Nevermind. *ahem* AND THUS THE WHEEL OF TIME CONTINUED TO TURN.", "Prophet" },
                {
                    "AND ALL THOSE WITHIN CONTINUED UNABATED, UNRELENTING ALONG THE CHARIOT OF THEIR ULTIMATE DESTINY.",
                    "Prophet"
                },
                { "...", "Messenger" },
                { "<event=OpenPortals>Look just go find the things, ok?", "Prophet" },
            },
            new Dictionary<string, string>
            {
                { "<event=OpenPortals>I could really go for a pizza right now.", "Prophet" },
            }
        };
    }
}