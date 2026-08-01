using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using MessengerRando.GameOverrideManagers;

namespace MessengerRando.Archipelago
{
    public class TrackerManager
    {

        private List<string> visitedEntrances = [];

        public bool Synced { get; private set; } = false;

        private readonly HashSet<string> unsentVisitedEntrances = [];
        private bool needsUnlockedPortalsReconciliation = true;
        private ELevel lastKnownCurrentRegion = ELevel.NONE;

        public void ReSync()
        {
            Synced = true;
            Console.WriteLine("Re-syncing tracker data from Data Storage");

            visitedEntrances = ArchipelagoClient.Session.DataStorage[Scope.Slot, "VisitedEntrances"].To<List<string>>() ?? [];
            unsentVisitedEntrances.RemoveWhere(visitedEntrances.Contains);
            if (unsentVisitedEntrances.Count > 0)
            {
                Console.WriteLine($"Adding visited entrances:\n\t{string.Join("\n\t", [.. unsentVisitedEntrances])}");
                visitedEntrances.AddRange(unsentVisitedEntrances);
                visitedEntrances.Sort();
                ArchipelagoClient.Session.DataStorage[Scope.Slot, "VisitedEntrances"] = visitedEntrances;
                unsentVisitedEntrances.Clear();
            }

            if (needsUnlockedPortalsReconciliation) ReconciliateUnlockedPortals();
            if (lastKnownCurrentRegion != ELevel.NONE) SetCurrentRegion(lastKnownCurrentRegion);
        }


        public void AddVisitedEntrance(string entrance)
        {
            if (visitedEntrances.Contains(entrance)) return;

            if (ArchipelagoClient.Offline) return;
            if (!ArchipelagoClient.Authenticated)
            {
                Synced = false;
                unsentVisitedEntrances.Add(entrance);
                return;
            }

            visitedEntrances = ArchipelagoClient.Session.DataStorage[Scope.Slot, "VisitedEntrances"].To<List<string>>() ?? [];
            if (visitedEntrances.Contains(entrance)) return;

            Console.WriteLine("Adding visited entrance: " + entrance);
            visitedEntrances.Add(entrance);
            visitedEntrances.Sort();
            ArchipelagoClient.Session.DataStorage[Scope.Slot, "VisitedEntrances"] = visitedEntrances;

            return;
        }

        public void ReconciliateUnlockedPortals()
        {
            if (ArchipelagoClient.Offline) return;
            if (!ArchipelagoClient.Authenticated)
            {
                Synced = false;
                needsUnlockedPortalsReconciliation = true;
                return;
            }

            var unlockedPortals = RandoPortalManager.UnlockedPortals;
            unlockedPortals.UnionWith(ArchipelagoClient.Session.DataStorage[Scope.Slot, "UnlockedPortals"].To<List<string>>() ?? []);

            var unlockedPortalsList = unlockedPortals.ToList();
            unlockedPortalsList.Sort();
            Console.WriteLine($"Updating unlocked portals with Data Storage. Unlocked portals are\n\t{string.Join("\n\t", [.. unlockedPortalsList])}");
            ArchipelagoClient.Session.DataStorage[Scope.Slot, "UnlockedPortals"] = unlockedPortalsList;
        }

        public void SetCurrentRegion(ELevel level)
        {
            if (ArchipelagoClient.Offline) return;
            if (!ArchipelagoClient.Authenticated)
            {
                Synced = false;
                lastKnownCurrentRegion = level;
                return;
            }

            ArchipelagoClient.Session.DataStorage[Scope.Slot, "CurrentRegion"] = level.ToString();
        }
    }
}
