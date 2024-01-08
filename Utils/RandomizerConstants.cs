using System.Collections.Generic;

namespace MessengerRando.Utils
{
    class RandomizerConstants
    {
        public static List<string> GetSpecialTriggerNames()
        {
            List<string> triggersToIgnoreRandoItemLogic = new List<string>();

            //LOAD (initally started as a black list of locations...probably would have been better to make this a whitelist...whatever)
            triggersToIgnoreRandoItemLogic.Add("CorruptedFuturePortal"); //Need to really check for crown and get access to CF
            triggersToIgnoreRandoItemLogic.Add("Lucioles"); //CF Fairy Check
            triggersToIgnoreRandoItemLogic.Add("DecurseQueenCutscene");
            triggersToIgnoreRandoItemLogic.Add("Bridge"); //Forlorn bridge check
            triggersToIgnoreRandoItemLogic.Add("NoUpgrade"); //Dark Cave Candle check
            triggersToIgnoreRandoItemLogic.Add("OverlayArt_16"); //...also Dark Cave Candle check
            //These are for the sprite renderings of phoebes
            triggersToIgnoreRandoItemLogic.Add("PhobekinNecro");
            triggersToIgnoreRandoItemLogic.Add("PhobekinNecro_16");
            triggersToIgnoreRandoItemLogic.Add("PhobekinAcro");
            triggersToIgnoreRandoItemLogic.Add("PhobekinAcro_16");
            triggersToIgnoreRandoItemLogic.Add("PhobekinClaustro");
            triggersToIgnoreRandoItemLogic.Add("PhobekinClaustro_16");
            triggersToIgnoreRandoItemLogic.Add("PhobekinPyro");
            triggersToIgnoreRandoItemLogic.Add("PhobekinPyro_16");
            //Parents of triggers to handle sassy interaction zones
            triggersToIgnoreRandoItemLogic.Add("Colos_8");
            triggersToIgnoreRandoItemLogic.Add("Suses_8");
            triggersToIgnoreRandoItemLogic.Add("Door");
            triggersToIgnoreRandoItemLogic.Add("RuxtinStaff");

            return triggersToIgnoreRandoItemLogic;
        }

        public static Dictionary<string, EItems> GetCutsceneMappings()
        {
            //This is where all the cutscene mappings will live. These mappings will mean that the cutscene requires additional logic to ensure it has "been played" or not.
            Dictionary<string, EItems> cutsceneMappings = new Dictionary<string, EItems>();

            //LOAD
            cutsceneMappings.Add("RuxxtinNoteAndAwardAmuletCutscene", EItems.RUXXTIN_AMULET);

            return cutsceneMappings;

        }
    }
}
