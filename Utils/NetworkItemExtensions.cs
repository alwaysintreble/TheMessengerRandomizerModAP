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
            if (!otherPlayer.IsNullOrEmpty())
            {
                text = $"Found {item.Colorize()} for <color=#{UserConfig.OtherPlayerColor}>{otherPlayer}</color>!";
                return text;
            }
            var ownPlayer = ArchipelagoClient.Session.ConnectionInfo.Slot;
            if (item.Player.Equals(ownPlayer))
            {
                text = $"Found {item.Colorize()}!";
            }
            else if (item.Player.Equals(-1))
            {
                text = $"Cheated {item.Colorize()}";
            }
            else
            {
                text = $"Received {item.Colorize()} from {item.PlayerColor()}!";
                return text;
            }
            return text;
        }

        public static string Name(this NetworkItem item) => ArchipelagoClient.Session.Items.GetItemName(item.Item);

        public static string Colorize(this NetworkItem item)
        {
            var color = item.Color();
            var text = color.IsNullOrEmpty() ? item.Name() : $"<color=#{color}>{item.Name()}</color>";

            return text;
        }

        private static string PlayerColor(this NetworkItem item)
        {
            return $"<color=#{UserConfig.OtherPlayerColor}>" +
                   $"{ArchipelagoClient.Session.Players.GetPlayerAlias(item.Player)}</color>";
        }

        public static string GetShopDescription(this NetworkItem item)
        {
            var description =
                !ArchipelagoClient.Authenticated ||
                item.Player.Equals(ArchipelagoClient.Session.ConnectionInfo.Slot)
                    ? "Huh. How did this get here? "
                    : $"Looks like {item.PlayerColor()} lost this. ";
            
            switch (item.Flags)
            {
                case ItemFlags.Advancement:
                    description += "Seems important.";
                    break;
                case ItemFlags.NeverExclude:
                    description += "Seems useful.";
                    break;
                case ItemFlags.Trap:
                    description += "Seems harmful.";
                    break;
                default:
                    description += "Seems like junk.";
                    break;
            }
            return description;
        }

        private static string Color(this NetworkItem item)
        {
            switch (item.Flags)
            {
                case ItemFlags.Advancement:
                    return UserConfig.AdvancementColor;
                case ItemFlags.NeverExclude:
                    return UserConfig.UsefulColor;
                case ItemFlags.Trap:
                    return UserConfig.TrapColor;
                default:
                    return UserConfig.FillerColor;
            }
        }
    }
}