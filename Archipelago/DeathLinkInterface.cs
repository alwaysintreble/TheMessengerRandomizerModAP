using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

namespace MessengerRando.Archipelago
{
    public class DeathLinkInterface
    {
        public DeathLinkService DeathLinkService;
        public PlayerController Player;
        private readonly List<DeathLink> deathLinks = new List<DeathLink>();
        private bool receivedDeath;
        private System.Random random;
        private int deathsSent;
        private List<string> genericDeathCauses;
        private List<string> projectileDeathCauses;
        private List<string> spikeDeathCauses;
        private List<string> pitfallDeathCauses;
        private List<string> squishDeathCauses;
        private List<string> frequentDeathCauses;

        public DeathLinkInterface()
        {
            try
            {
                Console.WriteLine($"Initializing death link service... Should be set to {ArchipelagoData.DeathLink}");
                DeathLinkService = ArchipelagoClient.Session.CreateDeathLinkService();
                DeathLinkService.OnDeathLinkReceived += DeathLinkReceived;
                GenerateFunnyCauses();
                random = new System.Random();

                if (ArchipelagoData.DeathLink)
                    DeathLinkService.EnableDeathLink();
                else
                    DeathLinkService.DisableDeathLink();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void DeathLinkReceived(DeathLink deathLink)
        {
            receivedDeath = true;
            deathLinks.Add(deathLink);
            Console.WriteLine($"Received Death Link from: {deathLink.Source} due to {deathLink.Cause}");
        }

        public void KillPlayer()
        {
            try
            {
                if (deathLinks.Count > 0) receivedDeath = true;
                if (!receivedDeath) return;
                var cause = deathLinks[0].Cause;
                if (cause.IsNullOrEmpty()) cause = $"{deathLinks[0].Source} sent you pain from afar.";
                DialogSequence receivedDeathDialog = ScriptableObject.CreateInstance<DialogSequence>();
                receivedDeathDialog.dialogID = "DEATH_LINK";
                receivedDeathDialog.name = cause;
                receivedDeathDialog.choices = new List<DialogSequenceChoice>();
                AwardItemPopupParams receivedDeathParams = new AwardItemPopupParams(receivedDeathDialog, false);
                Manager<UIManager>.Instance.ShowView<AwardItemPopup>(EScreenLayers.PROMPT, receivedDeathParams);
                Player.Kill(EDeathType.GENERIC, null);
                deathLinks.RemoveAt(0);
                receivedDeath = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendDeathLink(EDeathType deathType, Transform killer)
        {
            try
            {
                if (!ArchipelagoData.DeathLink || receivedDeath) return;
                deathsSent++;
                Console.WriteLine("Sharing death with your friends...");
                var alias = ArchipelagoClient.Session.Players.GetPlayerAliasAndName(ArchipelagoClient.Session.ConnectionInfo.Slot);
                // Debug.Log(killer.GetType());
                var cause = GetDeathLinkCause(deathType);
                DeathLinkService.SendDeathLink(new DeathLink(ArchipelagoClient.ServerData.SlotName, alias + cause));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
            }
        }

        private void GenerateFunnyCauses()
        {
            genericDeathCauses = new List<string> 
            {
                " dropped their message.",
                " forgot to jump.",
                " just wanted to see Quarble.",
                " thought it was a simulation.",
                " blames that on input delay.",
                $" has had their life saved {deathsSent} times.",
                " zigged when they should have zagged.",
                " let Quarble win another bet.",
                " wasn't able to get a selfie with Quarble.",
                " meant to do that.",
                " is playing with their feet.",
            };
            
            projectileDeathCauses = new List<string>
            {
                " thought that was a health potion.",
                " didn't dodge.",
                " walked into that one.",
                " forgot the 5 d's of dealing with projectiles.",
                " visited the Shopkeeper, but they don't carry awareness upgrades.",
                " forgot to tell the enemies not to shoot The Messenger.",
            };

            spikeDeathCauses = new List<string>
            {
                " didn't avoid the spikes.",
                " didn't watch their step.",
                " lacks aichmophobia.",
                " needs an item to reduce spike damage.",
                " saved Quarble from a very boring dinner.",
            };

            pitfallDeathCauses = new List<string>
            {
                " just fell in a pit.",
                " definitely fell due to input lag.",
                " thought that was a secret passage.",
                " wanted to see how long until the death quotes loop.",
            };

            squishDeathCauses = new List<string>
            {
                " died in a painful way.",
                " had to confirm that one.",
                " wasn't forgiven by the moving blocks.",
                " got crushed.",
                " was pressed.",
            };

            frequentDeathCauses = new List<string>
            {
                " deserves a discount at this point.",
                " won't give Quarble a break.",
                " is considering the package offer.",
                " is keeping Quarble busy.",
                " is overheating Quarble's ring.",
                " is letting Quarble do all the work.",
                " is keeping Quarble from his family.",
            };
        }

        private string GetDeathLinkCause(EDeathType deathType)
        {
            try
            {
                switch (deathType)
                {
                    case EDeathType.INTRO:
                        return " is a new customer!";
                    case EDeathType.GENERIC:
                        return genericDeathCauses[random.Next(genericDeathCauses.Count)];
                    case EDeathType.PROJECTILE:
                        return projectileDeathCauses[random.Next(projectileDeathCauses.Count)];
                    case EDeathType.SPIKES:
                        return spikeDeathCauses[random.Next(spikeDeathCauses.Count)];
                    case EDeathType.PITFALL:
                        return pitfallDeathCauses[random.Next(pitfallDeathCauses.Count)];
                    case EDeathType.SQUISH:
                        return squishDeathCauses[random.Next(squishDeathCauses.Count)];
                    case EDeathType.FREQUENT:
                        return frequentDeathCauses[random.Next(frequentDeathCauses.Count)];
                    default:
                        throw new ArgumentOutOfRangeException(nameof(deathType));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(genericDeathCauses);
                Console.WriteLine(projectileDeathCauses);
                Console.WriteLine(spikeDeathCauses);
                Console.WriteLine(pitfallDeathCauses);
                Console.WriteLine(squishDeathCauses);
                Console.WriteLine(frequentDeathCauses);
                Console.WriteLine(random);
                return " is a menace";
            }
        }
    }
}
