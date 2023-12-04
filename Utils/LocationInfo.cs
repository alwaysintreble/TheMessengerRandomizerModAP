using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using MessengerRando.Archipelago;
using WebSocketSharp;

namespace MessengerRando.Utils
{
    public static class NetworkItemExtensions
    {
        public static string ToReadableString(this NetworkItem item, string otherPlayer = "")
        {
            string text;
            if (item.Player.Equals(ArchipelagoClient.Session.ConnectionInfo.Slot))
                text = item.Location.Equals(-1) ? $"Cheated {item.Colorize()}" : $"Found {item.Colorize()}";
            else
            {
                text = $"Received {item.Colorize()}!";
                return text;
            }
            text += otherPlayer.IsNullOrEmpty()
                ? "!"
                : $" for <color=#{UserConfig.OtherPlayerColor}>{otherPlayer}</color>";
            return text;
        }

        public static string Name(this NetworkItem item) => ArchipelagoClient.Session.Items.GetItemName(item.Item);

        public static string Colorize(this NetworkItem item)
        {
            var color = UserConfig.FillerColor;
            switch (item.Flags)
            {
                case ItemFlags.Advancement:
                    color = UserConfig.AdvancementColor;
                    break;
                case ItemFlags.NeverExclude:
                    color = UserConfig.UsefulColor;
                    break;
                case ItemFlags.Trap:
                    color = UserConfig.TrapColor;
                    break;
            }
            var text = string.IsNullOrEmpty(color) ? item.Name() : $"<color=#{color}>{item.Name()}</color>";

            return text;
        }
    }
}