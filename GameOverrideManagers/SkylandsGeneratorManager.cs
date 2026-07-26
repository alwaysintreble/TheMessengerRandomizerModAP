using System;
using System.Collections;
using System.Collections.Generic;
using MessengerRando.Archipelago;
using MessengerRando.RO;
using MessengerRando.Utils;

namespace MessengerRando.GameOverrideManagers;

public class SkylandsGeneratorManager
{

    private readonly Dictionary<GeneratorType, ElementalSkylandGenerator> LoadedGenerators = [];
    private readonly Dictionary<GeneratorType, string> FlagsByGenerator = new()
    {
        { GeneratorType.AIR, Flags.AirGeneratorDeactivated },
        { GeneratorType.EARTH, Flags.EarthGeneratorDeactivated },
        { GeneratorType.WATER, Flags.WaterGeneratorDeactivated },
        { GeneratorType.FIRE, Flags.FireGeneratorDeactivated },
    };
    private readonly Dictionary<GeneratorType, LocationRO> LocationsByGenerator = new()
    {
        { GeneratorType.AIR, new LocationRO("Elemental Skylands - Shutdown Air Generator") },
        { GeneratorType.EARTH, new LocationRO("Elemental Skylands - Shutdown Earth Generator") },
        { GeneratorType.WATER, new LocationRO("Elemental Skylands - Shutdown Water Generator") },
        { GeneratorType.FIRE, new LocationRO("Elemental Skylands - Shutdown Fire Generator") },
    };

    public bool AreGeneratorsShuffled = false;

    public void ApplyHooks()
    {
        On.ElementalSkylandGenerator.Start += ElementalSkylandGenerator_Start;
        On.ElementalSkylandGenerator.SetState += ElementalSkylandGenerator_SetState;
        On.ElementalSkylandGenerator.OnLanternHit += ElementalSkylandGenerator_OnLanternHit;
        On.ElementalSkylandGenerator.Shutdown += ElementalSkylandGenerator_Shutdown;
        On.ElementalSkylandGenerator.OnDeactivateDone += ElementalSkylandGenerator_OnDeactivateDone;
        On.ElementalSkylandGenerator.OnDisable += ElementalSkylandGenerator_OnDisable;
    }

    public void ReceiveGeneratorShutdown(string generatorShutdownItem)
    {
        Console.WriteLine($"Received {generatorShutdownItem}");

        if (AreAllGeneratorsShutdownReceived())
        {
            Console.WriteLine($"All generators already deactivated, so ignoring received shutdown");
            return;
        }

        var generatorType = FindNextGeneratorToShutdown();
        var generatorFlag = FlagsByGenerator[generatorType];
        Manager<ProgressionManager>.Instance.SetFlag(generatorFlag, false);
        Console.WriteLine($"Current flags are [{string.Join(", ", [.. Manager<ProgressionManager>.Instance.flags])}]");

        if (LoadedGenerators.TryGetValue(generatorType, out var generator) && generator != null)
        {
            Console.WriteLine($"Generator loaded, so playing animation");
            Manager<AudioManager>.Instance.PlaySoundEffect(generator.shutdownSFX);
            generator.animator.SetTrigger("Deactivate");
        }
    }

    private void ElementalSkylandGenerator_Start(On.ElementalSkylandGenerator.orig_Start orig, global::ElementalSkylandGenerator self)
    {
        if (!AreGeneratorsShuffled) { orig(self); return; }

        var generatorType = ToGeneratorType(self.name);
        LoadedGenerators[generatorType] = self;
        Console.WriteLine($"Registered {generatorType} generator");

        orig(self);
    }

    private void ElementalSkylandGenerator_SetState(On.ElementalSkylandGenerator.orig_SetState orig, global::ElementalSkylandGenerator self)
    {
        if (!AreGeneratorsShuffled) { orig(self); return; }
        if (!ArchipelagoClient.HasConnected) { orig(self); return; }

        var generatorType = ToGeneratorType(self.name);
        if (Manager<ProgressionManager>.Instance.IsFlagSet(self.deactivatedFlag))
        {
            Console.WriteLine($"Deactivating {self.name}");
            self.animator.SetTrigger("DeactivateInstant");
        }

        bool isLocationSent = IsLocationSent(generatorType);
        if ((generatorType == GeneratorType.FIRE && AreAllGeneratorsShutdownReceived())
            || (generatorType != GeneratorType.FIRE && (isLocationSent || Manager<ProgressionManager>.Instance.IsFlagSet(self.deactivatedFlag))))
        {
            Console.WriteLine($"Opening door for {self.name}");
            self.wall.gameObject.SetActive(value: false);
        }

        for (int i = 0; i < self.powerLanterns.Count; i++)
        {
            self.powerLanterns[i].SetFull(full: !isLocationSent);
        }
    }

    private void ElementalSkylandGenerator_OnLanternHit(On.ElementalSkylandGenerator.orig_OnLanternHit orig, global::ElementalSkylandGenerator self, global::Hittable lantern, global::HitData hitData)
    {
        if (!AreGeneratorsShuffled) { orig(self, lantern, hitData); return; }

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

    private void ElementalSkylandGenerator_Shutdown(On.ElementalSkylandGenerator.orig_Shutdown orig, global::ElementalSkylandGenerator self)
    {
        if (!AreGeneratorsShuffled) { orig(self); return; }
        if (!ArchipelagoClient.HasConnected) { orig(self); return; }

        SendLocation(self);

        if (ToGeneratorType(self.name) != GeneratorType.FIRE && self.wall.activeSelf)
        {
            Manager<AudioManager>.Instance.PlaySoundEffect(self.wallDisappearSFX);
            self.wall.SetActive(value: false);
        }
    }

    private void ElementalSkylandGenerator_OnDeactivateDone(On.ElementalSkylandGenerator.orig_OnDeactivateDone orig, global::ElementalSkylandGenerator self)
    {
        if (!AreGeneratorsShuffled) { orig(self); return; }
        if (AreAllGeneratorsShutdownReceived())
        {
            OpenFireGeneratorDoor();
        }

        if (ToGeneratorType(self.name) != GeneratorType.FIRE && self.wall.activeSelf)
        {
            Manager<AudioManager>.Instance.PlaySoundEffect(self.wallDisappearSFX);
            self.wall.SetActive(value: false);
        }

        self.StartCoroutine((IEnumerator)ReflectionHelpers.InvokeMethodWithReturn(self, "ShakeCamCoroutine"));
    }

    private void ElementalSkylandGenerator_OnDisable(On.ElementalSkylandGenerator.orig_OnDisable orig, global::ElementalSkylandGenerator self)
    {
        if (!AreGeneratorsShuffled) { orig(self); return; }

        var generatorType = ToGeneratorType(self.name);
        LoadedGenerators[generatorType] = null;
        Console.WriteLine($"Cleaned up {generatorType} generator");

        orig(self);
    }

    private void SendLocation(ElementalSkylandGenerator generator)
    {
        var generatorType = ToGeneratorType(generator.name);
        var location = LocationsByGenerator[generatorType];
        Console.WriteLine($"Sending location {location.LocationName}");
        ItemsAndLocationsHandler.SendLocationCheck(location);
    }

    private bool IsLocationSent(GeneratorType generatorType)
    {
        return ItemsAndLocationsHandler.IsLocationChecked(LocationsByGenerator[generatorType]);
    }

    private bool AreAllGeneratorsShutdownReceived()
    {
        var manager = Manager<ProgressionManager>.Instance;
        return manager.IsFlagSet(Flags.AirGeneratorDeactivated)
            && manager.IsFlagSet(Flags.EarthGeneratorDeactivated)
            && manager.IsFlagSet(Flags.WaterGeneratorDeactivated)
            && manager.IsFlagSet(Flags.FireGeneratorDeactivated);
    }

    private void OpenFireGeneratorDoor()
    {
        var fireGenerator = LoadedGenerators[GeneratorType.FIRE];
        Manager<AudioManager>.Instance.PlaySoundEffect(fireGenerator.wallDisappearSFX);
        fireGenerator.wall.SetActive(value: false);
    }
    private GeneratorType FindNextGeneratorToShutdown()
    {
        var manager = Manager<ProgressionManager>.Instance;
        if (manager.IsFlagSet(Flags.WaterGeneratorDeactivated))
        {
            Console.WriteLine($"Water generator already deactivated, so shutting down Fire generator");
            return GeneratorType.FIRE;
        }
        if (manager.IsFlagSet(Flags.EarthGeneratorDeactivated))
        {
            Console.WriteLine($"Earth generator already deactivated, so shutting down Water generator");
            return GeneratorType.WATER;
        }
        if (manager.IsFlagSet(Flags.AirGeneratorDeactivated))
        {
            Console.WriteLine($"Air generator already deactivated, so shutting down Earth generator");
            return GeneratorType.EARTH;
        }

        Console.WriteLine($"No generators already deactivated, so shutting down Air generator");
        return GeneratorType.AIR;
    }

    private static GeneratorType ToGeneratorType(string generator)
    {
        return generator switch
        {
            var g when g.Contains("Air") => GeneratorType.AIR,
            var g when g.Contains("Water") => GeneratorType.WATER,
            var g when g.Contains("Earth") => GeneratorType.EARTH,
            var g when g.Contains("Fire") => GeneratorType.FIRE,
            _ => throw new Exception($"Unknown generator type for generator with name {generator}")
        };
    }
}

enum GeneratorType
{
    AIR, EARTH, WATER, FIRE
}
