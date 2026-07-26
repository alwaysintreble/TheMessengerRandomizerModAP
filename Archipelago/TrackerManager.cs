using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using MessengerRando.GameOverrideManagers;

namespace MessengerRando.Archipelago;

public class TrackerManager
{

    private List<string> VisitedEntrances = [];


    public void AddVisitedEntrance(string entrance)
    {
        if (VisitedEntrances.Contains(entrance)) return;

        if (!ArchipelagoClient.Authenticated) return;
        VisitedEntrances = ArchipelagoClient.Session.DataStorage[Scope.Slot, "VisitedEntrances"].To<List<string>>() ?? [];
        if (VisitedEntrances.Contains(entrance)) return;

        Console.WriteLine("Adding visited entrance: " + entrance);
        VisitedEntrances.Add(entrance);
        VisitedEntrances.Sort();
        ArchipelagoClient.Session.DataStorage[Scope.Slot, "VisitedEntrances"] = VisitedEntrances;

        return;
    }

    public void ReconciliateUnlockedPortals()
    {
        if (!ArchipelagoClient.Authenticated) return;

        var unlockedPortals = RandoPortalManager.UnlockedPortals;
        unlockedPortals.UnionWith(ArchipelagoClient.Session.DataStorage[Scope.Slot, "UnlockedPortals"].To<List<string>>() ?? []);

        var unlockedPortalsList = unlockedPortals.ToList();
        unlockedPortalsList.Sort();
        Console.WriteLine($"Updating unlocked portals with Data Storage. Unlocked portals are\n\t{string.Join("\n\t", [.. unlockedPortalsList])}");
        ArchipelagoClient.Session.DataStorage[Scope.Slot, "UnlockedPortals"] = unlockedPortalsList;
    }
}
