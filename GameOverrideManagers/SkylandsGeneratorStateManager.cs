using MessengerRando.Archipelago;
using MessengerRando.RO;
using System;
using System.Collections.Generic;

namespace MessengerRando.GameOverrideManagers
{
    public static class SkylandsGeneratorStateManager
    {

        private static readonly Dictionary<GeneratorType, ElementalSkylandGenerator> LoadedGenerators = [];
        private static readonly Dictionary<GeneratorType, string> FlagsByGenerator = new()
        {
            { GeneratorType.AIR, Flags.AirGeneratorDeactivated },
            { GeneratorType.EARTH, Flags.EarthGeneratorDeactivated },
            { GeneratorType.WATER, Flags.WaterGeneratorDeactivated },
            { GeneratorType.FIRE, Flags.FireGeneratorDeactivated },
        };
        private static readonly Dictionary<GeneratorType, LocationRO> LocationsByGenerator = new()
        {
            { GeneratorType.AIR, new LocationRO("Elemental Skylands - Shutdown Air Generator") },
            { GeneratorType.EARTH, new LocationRO("Elemental Skylands - Shutdown Earth Generator") },
            { GeneratorType.WATER, new LocationRO("Elemental Skylands - Shutdown Water Generator") },
            { GeneratorType.FIRE, new LocationRO("Elemental Skylands - Shutdown Fire Generator") },
        };

        public static void RegisterGenerator(ElementalSkylandGenerator generator)
        {
            var generatorType = ToGeneratorType(generator.name);
            LoadedGenerators[generatorType] = generator;
            Console.WriteLine($"Registered {generatorType} generator");
        }

        public static void CleanupGenerator(ElementalSkylandGenerator generator)
        {
            var generatorType = ToGeneratorType(generator.name);
            LoadedGenerators[generatorType] = null;
            Console.WriteLine($"Cleaned up {generatorType} generator");
        }

        public static void SendLocation(ElementalSkylandGenerator generator)
        {
            var generatorType = ToGeneratorType(generator.name);
            var location = LocationsByGenerator[generatorType];
            Console.WriteLine($"Sending location {location.LocationName}");
            ItemsAndLocationsHandler.SendLocationCheck(location);
        }

        public static bool IsLocationSent(GeneratorType generatorType)
        {
            return ItemsAndLocationsHandler.IsLocationChecked(LocationsByGenerator[generatorType]);
        }

        public static void ReceiveGeneratorShutdown(string generatorShutdownItem)
        {
            Console.WriteLine($"Received {generatorShutdownItem}");

            if (Manager<ProgressionManager>.Instance.IsFlagSet(Flags.FireGeneratorDeactivated))
            {
                Console.WriteLine($"All generators already deactivated, so ignoring received shutdown");
                return;
            }

            var generatorType = FindNextGeneratorToShutdown();
            var generatorFlag = FlagsByGenerator[generatorType];
            Manager<ProgressionManager>.Instance.SetFlag(generatorFlag, false);

            if (LoadedGenerators.TryGetValue(generatorType, out var generator) && generator != null)
            {
                Console.WriteLine($"Generator loaded, so playing animation");
                Manager<AudioManager>.Instance.PlaySoundEffect(generator.shutdownSFX);
                generator.animator.SetTrigger("Deactivate");
            }
        }


        public static bool AreAllGeneratorsShutdownReceived()
        {
            return Manager<ProgressionManager>.Instance.IsFlagSet(Flags.AirGeneratorDeactivated)
                && Manager<ProgressionManager>.Instance.IsFlagSet(Flags.EarthGeneratorDeactivated)
                && Manager<ProgressionManager>.Instance.IsFlagSet(Flags.WaterGeneratorDeactivated)
                && Manager<ProgressionManager>.Instance.IsFlagSet(Flags.FireGeneratorDeactivated);
        }

        public static void OpenFireGeneratorDoor()
        {
            var fireGenerator = LoadedGenerators[GeneratorType.FIRE];
            Manager<AudioManager>.Instance.PlaySoundEffect(fireGenerator.wallDisappearSFX);
            fireGenerator.wall.SetActive(value: false);
        }

        public static GeneratorType ToGeneratorType(string generator)
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

        private static GeneratorType FindNextGeneratorToShutdown()
        {
            if (Manager<ProgressionManager>.Instance.IsFlagSet(Flags.EarthGeneratorDeactivated))
            {
                Console.WriteLine($"Earth generator already deactivated, so shutting down Fire generator");
                return GeneratorType.FIRE;
            }
            if (Manager<ProgressionManager>.Instance.IsFlagSet(Flags.WaterGeneratorDeactivated))
            {
                Console.WriteLine($"Water generator already deactivated, so shutting down Earth generator");
                return GeneratorType.EARTH;
            }
            if (Manager<ProgressionManager>.Instance.IsFlagSet(Flags.AirGeneratorDeactivated))
            {
                Console.WriteLine($"Air generator already deactivated, so shutting down Water generator");
                return GeneratorType.WATER;
            }

            Console.WriteLine($"No generators already deactivated, so shutting down Air generator");
            return GeneratorType.AIR;
        }
    }

    public enum GeneratorType
    {
        AIR, EARTH, WATER, FIRE
    }


}
