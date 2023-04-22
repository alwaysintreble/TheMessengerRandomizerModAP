using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using MessengerRando.Archipelago;

namespace MessengerRando.Utils
{
    public static class NetworkItemExtensions
    {
        public static string ToReadableString(this NetworkItem item)
        {
            var text = $"Found {ColorizeItem(item)}";
            text += item.Player.Equals(ArchipelagoClient.Session.ConnectionInfo.Slot)
                ? "!"
                : $" for <color=#FAFAD2>{ArchipelagoClient.Session.Players.GetPlayerAlias(item.Player)}</color>";
            return text;
        }
        public static string ColorizeItem(this NetworkItem item)
        {
            var color = "00EEEE";
            switch (item.Flags)
            {
                case ItemFlags.Advancement:
                    color = "AF99EF";
                    break;
                case ItemFlags.NeverExclude:
                    color = "6D8BE8";
                    break;
                case ItemFlags.Trap:
                    color = "FA8072";
                    break;
            }
            var itemName = ArchipelagoClient.Session.Items.GetItemName(item.Item);
            var text = string.IsNullOrEmpty(color) ? itemName : $"<color=#{color}>{itemName}</color>";

            return text;
        }
    }
}