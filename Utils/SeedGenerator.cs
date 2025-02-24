﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MessengerRando.Archipelago;
using MessengerRando.GameOverrideManagers;
using Microsoft.Win32;
using Mod.Courier.UI;
using WebSocketSharp;

namespace MessengerRando.Utils
{
    public static class SeedGenerator
    {
        public static string ArchipelagoPath = "";
        private delegate void OnGenerateAttempt(bool result);
        private static bool generating;

        public static void GenerateAsync(SubMenuButtonInfo generateButton)
        {
            if (generating) return;
            generating = true;
            Generate(result => OnGenerated(result, generateButton));
        }

        private static void Generate(OnGenerateAttempt attempt)
        {
            attempt(Generate());
        }

        private static bool Generate()
        {
            // find archipelago install path
            if (ArchipelagoPath.IsNullOrEmpty())
            {
                var path = "";
                try
                {
                    var key = Registry.LocalMachine.OpenSubKey(
                        "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
                    foreach (var subkeyName in key.GetSubKeyNames())
                    {
                        var subkey = key.OpenSubKey(subkeyName);
                        var displayName = subkey.GetValue("DisplayName");
                        if (displayName != null && displayName.ToString().Contains("Archipelago "))
                        {
                            Console.WriteLine(displayName);
                            path = subkey.GetValue("InstallLocation").ToString();
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                if (path.IsNullOrEmpty()) return false;
                ArchipelagoPath = path;
            }

            var outDirectory = Directory.GetCurrentDirectory() + "\\Archipelago";
            if (!Directory.Exists(outDirectory) || Directory.GetFiles(outDirectory).Length != 1)
                return false;
            
            var archipelago = new Process();
            
            archipelago.StartInfo.FileName = $"{ArchipelagoPath}\\ArchipelagoGenerate.exe";

            var args =
                "--multi 1 " +
                $"--spoiler {RandomizerOptions.SpoilerLevel} " +
                $"--player_files_path \"{outDirectory}\" " +
                $"--outputpath \"{outDirectory}\\output\"";
            if (!RandomizerOptions.Seed.IsNullOrEmpty())
                args += $" --seed {RandomizerOptions.Seed}";
            if (RandoShopManager.RaceMode)
                args += " --race";
            archipelago.StartInfo.Arguments = args;
            // archipelago.StartInfo.UseShellExecute = false;
            // archipelago.StartInfo.RedirectStandardInput = true;
            // archipelago.StartInfo.RedirectStandardOutput = true;
            // archipelago.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            // archipelago.StartInfo.RedirectStandardError = true;
            // archipelago.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);
            Console.WriteLine("attempting to generate...");
            Console.WriteLine(archipelago.StartInfo.FileName);
            Console.WriteLine(archipelago.StartInfo.Arguments);
            
            archipelago.Start();
            archipelago.WaitForExit();
            
            Console.WriteLine(archipelago.ExitCode);
            if (archipelago.ExitCode == 0)
            {
                RandomizerStateManager.StartOfflineSeed();
                return true;
            }

            return false;
        }

        private static void OnGenerated(bool result, SubMenuButtonInfo generateButton)
        {
            TextEntryPopup generatePopup = TextEntryButtonInfo.InitTextEntryPopup(
                generateButton.addedTo,
                string.Empty,
                entry => true,
                0,
                null,
                TextEntryButtonInfo.CharsetFlags.Space);
            
            generatePopup.Init(result ? "Seed successfully generated!" : "Seed generation failed. Ensure you've output a settings file.");
            generatePopup.gameObject.SetActive(true);
            generating = false;
        }

        private static string GetColor(KeyValuePair<string, List<long>> itemInfo)
        {
            return itemInfo.Value[1] switch
            {
                1 => UserConfig.AdvancementColor,
                2 => UserConfig.UsefulColor,
                4 => UserConfig.TrapColor,
                _ => UserConfig.FillerColor,
            };
        }
        public static string GetOfflineDialog(long locId)
        {
            var itemInfo = ArchipelagoClient.ServerData.LocationData[locId].First();
            var itemName = itemInfo.Key;
            var color = GetColor(itemInfo);

            return $"Found <color=#{color}>{itemName}</color>!";
        }

        public static string GetOfflineShopDescription(long locId)
        {
            var description = "Huh. How did this get here? ";
            var itemInfo = ArchipelagoClient.ServerData.LocationData[locId].First();
            switch (itemInfo.Value[1])
            {
                case 1:
                    description += "Seems important.";
                    break;
                case 2:
                    description += "Seems useful.";
                    break;
                case 3:
                    description += "Seems harmful.";
                    break;
                default:
                    description += "Seems like junk.";
                    break;
            }

            return description;
        }

        public static string GetOfflineShopText(long locId)
        {
            var itemInfo = ArchipelagoClient.ServerData.LocationData[locId].First();
            return $"<color=#{GetColor(itemInfo)}>{itemInfo.Key}</color>";
        }
    }
}