using System;
using Mod.Courier.UI;

namespace MessengerRando.Utils
{
    public static class OptionsExporter
    {
        private delegate void OnExport(bool result);
        private static bool exporting;

        public static void ExportAsync(SubMenuButtonInfo exportButton)
        {
            if (exporting) return;
            exporting = true;
            Console.WriteLine("Exporting options");
            Export(result => OnExported(result, exportButton));
        }

        private static void Export(OnExport attempt)
        {
            attempt(Export());
        }

        private static bool Export()
        {
            return true;
        }

        private static void OnExported(bool result, SubMenuButtonInfo exportButton)
        {
            TextEntryPopup generatePopup = TextEntryButtonInfo.InitTextEntryPopup(
                exportButton.addedTo,
                string.Empty,
                entry => true,
                0,
                null,
                TextEntryButtonInfo.CharsetFlags.Space);
            
            generatePopup.Init(result ? "Options successfully exported!" : "Options export failed");
            generatePopup.gameObject.SetActive(true);
            exporting = false;
        }
    }
}