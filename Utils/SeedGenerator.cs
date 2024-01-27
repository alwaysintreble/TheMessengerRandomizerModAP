using System;
using System.Threading;
using Mod.Courier.UI;

namespace MessengerRando.Utils
{
    public static class SeedGenerator
    {
        private delegate void OnGenerateAttempt(bool result);
        private static bool generating;

        public static void GenerateAsync(SubMenuButtonInfo generateButton)
        {
            if (generating) return;
            generating = true;
            Console.WriteLine("Attempting to generate");
            Generate(result => OnGenerated(result, generateButton));
        }

        private static void Generate(OnGenerateAttempt attempt)
        {
            attempt(Generate());
        }

        private static bool Generate()
        {
            return true;
        }

        private static void OnGenerated(bool result, SubMenuButtonInfo generateButton)
        {
            TextEntryPopup generatePopup = TextEntryButtonInfo.InitTextEntryPopup(
                generateButton.addedTo,
                string.Empty,
                entry => true,
                0,
                null,
                TextEntryButtonInfo.CharsetFlags.Space);
            
            generatePopup.Init(result ? "Seed successfully generated!" : "Seed generation failed");
            generatePopup.gameObject.SetActive(true);
            generating = false;
        }
    }
}