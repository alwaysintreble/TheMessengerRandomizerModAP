using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using MessengerRando.Archipelago;
using WebSocketSharp;

namespace MessengerRando.Utils;

public static class ItemInfoExtensions
{
    public static string ToReadableString(this ItemInfo item, string otherPlayer = "")
    {
        string text;
        if (!otherPlayer.IsNullOrEmpty())
        {
            text = $"Found {item.Colorize()} for <color=#{UserConfig.OtherPlayerColor}>{otherPlayer}</color>!";
            return text;
        }

        if (item.OwnItem())
        {
            text = $"Found {item.Colorize()}!";
        }
        else if (item.Player.Slot.Equals(-1))
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

    public static string Colorize(this ItemInfo item)
    {
        var color = item.Color();
        var text = $"<color=#{color}>{item.ItemDisplayName}</color>";

        return text;
    }

    private static string PlayerColor(this ItemInfo item)
    {
        return $"<color=#{UserConfig.OtherPlayerColor}>" +
               $"{ArchipelagoClient.Session.Players.GetPlayerAlias(item.Player)}</color>";
    }

    public static string GetShopDescription(this ItemInfo item)
    {
        var description =
            !ArchipelagoClient.Authenticated ||
            item.Player.Slot.Equals(ArchipelagoClient.Session.ConnectionInfo.Slot)
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

    private static string Color(this ItemInfo item)
    {
        if ((item.Flags & ItemFlags.Advancement) != 0) return UserConfig.AdvancementColor;
        if ((item.Flags & ItemFlags.NeverExclude) != 0) return UserConfig.UsefulColor;
        if ((item.Flags & ItemFlags.Trap) != 0) return UserConfig.TrapColor;
        return UserConfig.FillerColor;
    }

    public static bool OwnItem(this ItemInfo item) =>
        item.Player.Slot.Equals(ArchipelagoClient.Session.ConnectionInfo.Slot);

    public static string ColorizeLocation(this ItemInfo item) =>
        $"<color=#{UserConfig.LocationColor}>{item.LocationDisplayName}</color>";
}