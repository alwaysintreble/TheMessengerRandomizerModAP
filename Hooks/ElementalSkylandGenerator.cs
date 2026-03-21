using MessengerRando.Archipelago;
using MessengerRando.GameOverrideManagers;
using MessengerRando.Utils;
using System;
using System.Collections;


namespace MessengerRando.Hooks
{
    public class ElementalSkylandGenerator
    {

        public static void ApplyHooks()
        {
            On.ElementalSkylandGenerator.Start += ElementalSkylandGenerator_Start;
            On.ElementalSkylandGenerator.SetState += ElementalSkylandGenerator_SetState;
            On.ElementalSkylandGenerator.OnLanternHit += ElementalSkylandGenerator_OnLanternHit;
            On.ElementalSkylandGenerator.Shutdown += ElementalSkylandGenerator_Shutdown;
            On.ElementalSkylandGenerator.OnDeactivateDone += ElementalSkylandGenerator_OnDeactivateDone;
            On.ElementalSkylandGenerator.OnDisable += ElementalSkylandGenerator_OnDisable;
        }

        public static void ElementalSkylandGenerator_Start(On.ElementalSkylandGenerator.orig_Start orig, global::ElementalSkylandGenerator self)
        {
            ElementalSkylandGeneratorStateManager.RegisterGenerator(self);
            orig(self);
        }

        public static void ElementalSkylandGenerator_SetState(On.ElementalSkylandGenerator.orig_SetState orig, global::ElementalSkylandGenerator self)
        {
            if (!ArchipelagoClient.HasConnected)
            {
                orig(self);
                return;
            }

            var generatorType = ElementalSkylandGeneratorStateManager.ToGeneratorType(self.name);
            if (Manager<ProgressionManager>.Instance.IsFlagSet(self.deactivatedFlag))
            {
                Console.WriteLine($"Deactivating {self.name}");
                self.animator.SetTrigger("DeactivateInstant");
            }

            bool isLocationSent = ElementalSkylandGeneratorStateManager.IsLocationSent(generatorType);
            if ((generatorType == GeneratorType.FIRE && ElementalSkylandGeneratorStateManager.AreAllGeneratorsShutdownReceived())
                || (generatorType != GeneratorType.FIRE && isLocationSent))
            {
                Console.WriteLine($"Opening door for {self.name}");
                self.wall.gameObject.SetActive(value: false);
            }

            for (int i = 0; i < self.powerLanterns.Count; i++)
            {
                self.powerLanterns[i].SetFull(full: !isLocationSent);
            }
        }

        public static void ElementalSkylandGenerator_OnLanternHit(On.ElementalSkylandGenerator.orig_OnLanternHit orig, global::ElementalSkylandGenerator self, global::Hittable lantern, global::HitData hitData)
        {
            if (!(lantern as Lantern).Full)
            {
                return;
            }

            Manager<AudioManager>.Instance.PlaySoundEffect(self.powerSourceHitSFX);
            bool flag = true;
            for (int i = 0; i < self.powerLanterns.Count; i++)
            {
                if (self.powerLanterns[i].Full && self.powerLanterns[i] != lantern)
                {
                    flag = false;
                    break;
                }
            }

            if (!Manager<ProgressionManager>.Instance.IsFlagSet(self.deactivatedFlag))
            {
                self.animator.SetTrigger("ReceiveHit");
            }
            if (flag)
            {
                ReflectionHelpers.InvokeMethod(self, "Shutdown");
            }
        }

        public static void ElementalSkylandGenerator_Shutdown(On.ElementalSkylandGenerator.orig_Shutdown orig, global::ElementalSkylandGenerator self)
        {
            if (!ArchipelagoClient.HasConnected)
            {
                orig(self);
                return;
            }

            ElementalSkylandGeneratorStateManager.SendLocation(self);

            if (ElementalSkylandGeneratorStateManager.ToGeneratorType(self.name) != GeneratorType.FIRE)
            {
                Manager<AudioManager>.Instance.PlaySoundEffect(self.wallDisappearSFX);
                self.wall.SetActive(value: false);
            }
        }

        public static void ElementalSkylandGenerator_OnDeactivateDone(On.ElementalSkylandGenerator.orig_OnDeactivateDone orig, global::ElementalSkylandGenerator self)
        {
            if (ElementalSkylandGeneratorStateManager.AreAllGeneratorsShutdownReceived())
            {
                ElementalSkylandGeneratorStateManager.OpenFireGeneratorDoor();
            }
            self.StartCoroutine((IEnumerator)ReflectionHelpers.InvokeMethodWithReturn(self, "ShakeCamCoroutine"));
        }

        public static void ElementalSkylandGenerator_OnDisable(On.ElementalSkylandGenerator.orig_OnDisable orig, global::ElementalSkylandGenerator self)
        {
            ElementalSkylandGeneratorStateManager.CleanupGenerator(self);
            orig(self);
        }
    }
}
