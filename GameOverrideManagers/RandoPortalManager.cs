using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace MessengerRando.GameOverrideManagers
{
    public class RandoPortalManager
    {
        public static bool LimitedPortals;
        public static List<TowerOfTimePortal> OpenPortals;


        public static void CreatePortals()
        {
            Console.WriteLine("Creating portals");
            try
            {
                var portals = Object.FindObjectsOfType<TowerOfTimePortal>();
                foreach (var VARIABLE in portals)
                {
                    Console.WriteLine(VARIABLE.name);
                }
                // var portalCutscene = Object.FindObjectOfType<PortalOpeningCutscene>();
                // var rivierePortal = Object.FindObjectOfType<RiviereTurquoisePortalOpeningCutscene>().portal;
                // var sunkenShrinePortal = Object.FindObjectOfType<SunkenShrinePortalOpeningCutscene>().portal;
                // var searingCragsPortal = Object.FindObjectOfType<SearingCragsPortalOpeningCutscene>().portal;
                // OpenPortals = new List<TowerOfTimePortal>
                // {
                //     portalCutscene.autumnHillsPortal, rivierePortal, portalCutscene.howlingGrottoPortal, sunkenShrinePortal,
                //     searingCragsPortal, portalCutscene.glacialPeakPortal
                // };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            var rng = new Random();
            var n = OpenPortals.Count;
            while (n > 1)
            {
                var k = rng.Next(n);
                n--;
                var value = OpenPortals[k];
                OpenPortals[k] = OpenPortals[n];
                OpenPortals[n] = value;
            }

            foreach (var portal in OpenPortals)
            {
                Console.WriteLine(portal.name);
            }
        }
        
        public static void OpenPortalEvent(On.PortalOpeningCutscene.orig_OnOpenPortalEvent orig, PortalOpeningCutscene self, string eventid)
        {
            // orig(self, eventid);
            switch (eventid)
            {
                case "AutumnHills": // first portal
                    UnlockPortal(self, OpenPortals[0]);
                    break;
                case "HowlingGrotto":
                    UnlockPortal(self, OpenPortals[1]);
                    break;
                case "GlacialPeak":
                    UnlockPortal(self, OpenPortals[2]);
                    break;
                default:
                    orig(self, eventid);
                    break;
            }
        }

        private static void UnlockPortal(PortalOpeningCutscene cutscene, TowerOfTimePortal portal)
        {
            portal.locked = false;
            portal.animator.SetTrigger("Open");
            Manager<AudioManager>.Instance.PlaySoundEffect(cutscene.portalOpeningSFX);
        }

    }
}