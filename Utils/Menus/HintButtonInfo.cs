using System;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using MessengerRando.Archipelago;
using Mod.Courier.UI;
using UnityEngine.Events;

namespace MessengerRando.Utils.Menus;

public class HintButtonInfo : ToggleButtonInfo
{
    private Hint hint;


    public HintButtonInfo(Func<string> text, UnityAction onClick, Func<ToggleButtonInfo, bool> GetState,
        Func<string, string> GetOnText, Func<string, string> GetOffText, Hint hint) : base(text, onClick, GetState,
        GetOnText, GetOffText)
    {
        this.hint = hint;
        this.onClick = UpdateHintStatus;
    }

    public override void UpdateNameText()
    {
        nameTextMesh.fontSize = 5f;
        nameTextMesh.text = GetHintEntryText();
    }

    private string GetHintEntryText()
    {
        if (!ArchipelagoClient.Authenticated) return "";
        var findingPlayerInfo = ArchipelagoClient.Session.Players.GetPlayerInfo(hint.FindingPlayer);
        var receivingPlayerInfo = ArchipelagoClient.Session.Players.GetPlayerInfo(hint.ReceivingPlayer);
        var locName =
            ArchipelagoClient.Session.Locations.GetLocationNameFromId(hint.LocationId, findingPlayerInfo.Game);
        var itemName = ArchipelagoClient.Session.Items.GetItemName(hint.ItemId, receivingPlayerInfo.Game);

        var slot = ArchipelagoClient.Session.ConnectionInfo.Slot;
        var coloredItemName = GetItemColor(itemName, hint.ItemFlags);

        if (hint.FindingPlayer == slot)
        {
            if (hint.ReceivingPlayer == slot)
            {
                return $"Your {coloredItemName} can be found at " + GetLocationColor(locName);
            }

            return $"{ArchipelagoClient.ColorizePlayerName(hint.ReceivingPlayer)}'s " +
                   $"{coloredItemName} can be found at " + GetLocationColor(locName);
        }

        return
            $"Your {coloredItemName} is in {ArchipelagoClient.ColorizePlayerName(hint.FindingPlayer)}'s world " +
            $"at " + GetLocationColor(locName);
    }

    private static string GetItemColor(string itemName, ItemFlags flags)
    {
        var colorString = "<color=#";
        if ((flags & ItemFlags.Advancement) != 0) colorString += UserConfig.AdvancementColor;
        else if ((flags & ItemFlags.NeverExclude) != 0) colorString += UserConfig.UsefulColor;
        else if ((flags & ItemFlags.Trap) != 0) colorString += UserConfig.TrapColor;
        else colorString += UserConfig.FillerColor;
        colorString += $">{itemName}</color>";
        return colorString;
    }

    private string GetLocationColor(string locationName)
    {
        return $"<color=#{UserConfig.LocationColor}>{locationName}</color>";
    }

    private string GetStatusColor()
    {
        string color = UserConfig.UnspecifiedColor;
        switch (hint.Status)
        {
            case HintStatus.Priority:
                color = UserConfig.PriorityColor;
                break;
            case HintStatus.Avoid:
                color = UserConfig.AvoidColor;
                break;
            case HintStatus.NoPriority:
                color = UserConfig.NoPriorityColor;
                break;
        }

        return $"<color=#{color}>{hint.Status}</color>";
    }

    private void UpdateHintStatus()
    {
        if (hint.ReceivingPlayer != ArchipelagoClient.Session.ConnectionInfo.Slot) return;
        switch (hint.Status)
        {
            case HintStatus.Unspecified:
                hint.Status = HintStatus.Priority;
                break;
            case HintStatus.Priority:
                hint.Status = HintStatus.Avoid;
                break;
            case HintStatus.Avoid:
                hint.Status = HintStatus.NoPriority;
                break;
            default:
                hint.Status = HintStatus.Unspecified;
                break;
        }

        if (ArchipelagoClient.Session.RoomState.GeneratorVersion >= new Version(0, 6, 0))
        {
            ArchipelagoClient.Session.Socket.SendPacket(new UpdateHintPacket
                { Location = hint.LocationId, Player = hint.ReceivingPlayer, Status = hint.Status });
        }
        UpdateStateText();
    }

    public override void UpdateStateText()
    {
        stateTextMesh.text = GetStateText();
    }

    public override string GetStateText()
    {
        return $"status: {GetStatusColor()}";
    }

    public void UpdateHint(Hint newHint) => hint = newHint;
}