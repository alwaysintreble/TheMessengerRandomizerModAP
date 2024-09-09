using System;
using System.Collections.Generic;
using System.Text;
using MessengerRando.Exceptions;
using Mod.Courier.UI;
using static Mod.Courier.UI.TextEntryButtonInfo;

namespace MessengerRando.Utils
{
    public static class RandomizerOptions
    {
        // name
        public static string Name = "Ninja";
        public static bool OnNameEntry(string input)
        {
            Name = input;
            return true;
        }
        
        // seed
        public static string Seed = "";
        public static bool OnSeedEntry(string input)
        {
            Seed = input;
            return true;
        }

        public static void OnGenerateSeed()
        {
            var random = new Random();
            Seed = string.Empty;
            for (var i = 0; i < 10; i++)
            {
                Seed += random.Next(10).ToString();
            }
        }
        
        // spoiler
        public static int SpoilerLevel = 3;
        private static readonly List<string> SpoilerText =
        [
            "No Spoiler",
            "Spoiler without playthrough or paths",
            "Spoiler with playthrough",
            "Spoiler with playthrough and paths"
        ];
        public static string GetSpoilerText()
        {
            return SpoilerText[SpoilerLevel];
        }
        public static void ChangeSpoiler()
        {
            SpoilerLevel = SpoilerLevel == 3 ? 0 : SpoilerLevel + 1;
        }

        public static string OptionsOverride = "";

        public static void OnOptionsOverrideEntry(string input)
        {
            if (input.Length < 1) return;
            var bytes = Encoding.UTF8.GetBytes(input);
            try
            {
                if (input.Length < 18 ||
                    bytes[0] != 0 || bytes[0] != 1 || bytes[0] != 2 || // accessibility
                    bytes[1] != 0 || bytes[1] != 1 ||/* bytes[0] != 2 || bytes[0] != 3 ||*/ // logic
                    bytes[2] != 0 || bytes[2] != 1 || // shards
                    bytes[3] != 0 || bytes[3] != 1 || // lim move
                    bytes[4] != 0 || bytes[4] != 1 || // early med
                    bytes[5] != 3 || bytes[5] != 4 || bytes[5] != 5 || bytes[5] != 6 || // avail portals
                    bytes[6] != 0 || bytes[6] != 1 || bytes[6] != 2 || bytes[6] != 3 || // port shuffle
                    bytes[7] != 0 || bytes[7] != 1 || bytes[7] != 2 || // transition shuffle
                    bytes[8] != 0 || bytes[8] != 1 || // goal
                    bytes[9] != 0 || bytes[9] != 1 || // gauntlet
                    bytes[10] != 0 || bytes[10] != 1 || bytes[10] != 2 || bytes[10] != 3 || bytes[10] != 4 || bytes[10] != 5 || bytes[10] != 6 // notes
                    )
                    throw new RandomizerException($"Invalid OptionsOverride input: {input}");
            }
            catch (Exception e)
            {
                var errorPopup = InitTextEntryPopup(RandoMenu.randoScreen, string.Empty,
                    _ => true, 0, null, CharsetFlags.Space);
                errorPopup.Init("Invalid input. Please check the code and try again.");
                Console.WriteLine(e);
                return;
            }

            OptionsOverride = input;
        }
        
        // accessibility
        private static int accessibility;
        private static readonly List<string> AccessibilityText =
        [
            "Locations Accessibility",
            "Items Accessibility",
            "Minimal Accessibility"
        ];
        public static string GetAccessibilityText()
        {
            return AccessibilityText[accessibility];
        }
        public static void ChangeAccessibility()
        {
            accessibility = accessibility == 2 ? 0 : accessibility + 1;
        }
        
        // logic
        private static int logic;
        private static readonly List<string> LogicText =
        [
            "Normal Logic",
            "Hard Logic",
            "Challenging Logic",
            "oob Logic"
        ];
        public static string GetLogicText()
        {
            return LogicText[logic];
        }
        public static void ChangeLogic()
        {
            switch (logic)
            {
                case 3:
                    logic = 0;
                    break;
                case 1:  // TODO reimplement challenging and oob logic
                    logic = 0;
                    break;
                default:
                    logic++;
                    break;
            }
        }
        
        // shards
        public static bool Shards;

        // limited movement
        public static bool LimMovement;

        // early meditation
        public static bool EarlyMed;
        
        // available portals
        private static int availablePortals = 6;
        public static string GetAvailablePortalsText()
        {
            return $"Available Portals: {availablePortals}";
        }
        public static void ChangeAvailablePortals()
        {
            availablePortals = availablePortals == 6 ? 3 : availablePortals + 1;
        }
        
        // portal shuffle
        private static int portalShuffle = 0;
        private static readonly List<string> PortalShuffleText =
        [
            "No Portal Shuffle",
            "Shuffle Portals with Shops",
            "Shuffle Portals with Shops and Checkpoints",
            "Shuffle Portals Anywhere"
        ];
        public static string GetPortalShuffleText()
        {
            return PortalShuffleText[portalShuffle];
        }
        public static void ChangePortalShuffle()
        {
            portalShuffle = portalShuffle == 3 ? 0 : portalShuffle + 1;
        }
        
        // transition shuffle
        private static int transitionShuffle;
        private static readonly List<string> TransitionShuffleText =
        [
            "No Transition Shuffle",
            "Couple Shuffled Transitions",
            "De-Coupled Shuffled Transitions"
        ];
        public static string GetTransitionText()
        {
            return TransitionShuffleText[transitionShuffle];
        }
        public static void ChangeTransitionShuffle()
        {
            transitionShuffle = transitionShuffle == 2 ? 0 : transitionShuffle + 1;
        }
        
        // goal
        public static bool Goal;
        
        // music box gauntlet
        public static bool MusicBox = true;
        
        // needed notes
        private static int notes = 6;
        public static string GetNotesText()
        {
            return $"Notes Needed for Music Box: {notes}";
        }
        public static void ChangeNotes()
        {
            notes = notes == 6 ? 1 : notes + 1;
        }
        
        // total seals
        public static int TotalSeals = 85;
        public static bool OnTotalSealsEntry(string input)
        {
            TotalSeals = Math.Min(int.Parse(input), 85);
            return true;
        }
        
        // required seals
        public static int RequiredSeals = 100;
        public static bool OnRequiredSealsEntry(string input)
        {
            RequiredSeals = Math.Min(int.Parse(input), 100);
            return true;
        }
        
        // shop price modifier
        public static int ShopPriceMod = 100;
        public static bool OnShopPriceModEntry(string input)
        {
            ShopPriceMod = Math.Min(int.Parse(input), 400);
            return true;
        }

        public static Dictionary<string, string> GetOptions()
        {
            
            return new Dictionary<string, string>
            {
                {"accessibility", accessibility.ToString()},
                {"logic_level", logic.ToString()},
                {"shuffle_shards", Shards ? "true" : "false"},
                {"limited_movement", LimMovement ? "true" : "false"},
                {"early_meditation", EarlyMed ? "true" : "false"},
                {"available_portals", availablePortals.ToString()},
                {"shuffle_portals", portalShuffle.ToString()},
                {"shuffle_transitions", transitionShuffle.ToString()},
                {"goal", Goal ? "1" : "0"},
                {"music_box", MusicBox ? "true" : "false"},
                {"notes_needed", notes.ToString()},
                {"total_seals", TotalSeals.ToString()},
                {"percent_seals_required", RequiredSeals.ToString()},
                {"shop_price", ShopPriceMod.ToString()},
            };
        }
    }
}