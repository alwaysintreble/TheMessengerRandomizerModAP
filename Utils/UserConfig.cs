using System;
using System.IO;
using Tommy;
using UnityEngine;

namespace MessengerRando.Utils
{
    public static class UserConfig
    {
        private static string configFileName = "APConfig.toml";
        public static string HostName = "archipelago.gg";
        public static int Port = 38281;
        public static string SlotName = "";
        public static string Password = "";
        public static float StatusTextSize = 4.0f;
        public static float MessageTextSize = 4.2f;
        public static string AdvancementColor = "AF99EF";
        public static string UsefulColor = "6D8BE8";
        public static string TrapColor = "FA8072";
        public static string FillerColor = "00EEEE";
        public static string PlayerColor = "EE00EE";
        public static string OtherPlayerColor = "FAFAD2";
        public static string LocationColor = "00FF7F";

        public static void ReadConfig(string path)
        {
            path += "\\" + configFileName;
            if (!File.Exists(path))
            {
                GenerateConfig(path);
                return;
            }
            var table = TOML.Parse(new StreamReader(File.OpenRead(path)));
            HostName = table.get_Item("host_name").AsString.Value;
            Port = (int)table.get_Item("port").AsInteger.Value;
            SlotName = table.get_Item("slot_name").AsString.Value;
            Password = table.get_Item("password").AsString.Value;
            StatusTextSize = table.get_Item("status_text_size") ?? StatusTextSize;
            MessageTextSize = table.get_Item("message_text_size") ?? MessageTextSize;
            AdvancementColor = table.get_Item("advancement_color") ?? AdvancementColor;
            UsefulColor = table.get_Item("useful_color") ?? UsefulColor;
            TrapColor = table.get_Item("trap_color") ?? TrapColor;
            FillerColor = table.get_Item("filler_color") ?? FillerColor;
            PlayerColor = table.get_Item("player_color") ?? PlayerColor;
            OtherPlayerColor = table.get_Item("other_player_color") ?? OtherPlayerColor;
            LocationColor = table.get_Item("location_color") ?? LocationColor;
        }

        public static void GenerateConfig(string path)
        {
            var config = new TomlTable
            {
                ["title"] = "TheMessengerRandomizerModAP Configuration File",
                ["host_name"] = new TomlString
                {
                    Value = "archipelago.gg",
                    Comment = "Connection info defined here is only used if no slot name is entered in the in-game menu.\n" +
                              "If a slot name is defined here, then all information here is used."
                },
                ["port"] = new TomlInteger
                {
                    Value = 38281
                },
                ["slot_name"] = new TomlString
                {
                    Value = ""
                },
                ["password"] = new TomlString
                {
                    Value = ""
                },
                ["status_text_size"] = new TomlFloat
                {
                    Value = 4.0,
                    Comment = "Size of the text for the current Archipelago status (Connected and hint info)"
                },
                ["message_text_size"] = new TomlFloat
                {
                    Value = 4.2,
                    Comment = "Size of the text for messages from the Archipelago server"
                },
                ["advancement_color"] = new TomlString
                {
                    Value = "AF99EF",
                    Comment = "Hex ID of the color used for Progression items sent and received"
                },
                ["useful_color"] = new TomlString
                {
                    Value = "6D8BE8",
                    Comment = "Hex ID of the color used for Useful items sent and received"
                },
                ["trap_color"] = new TomlString
                {
                    Value = "FA8072",
                    Comment = "Hex ID of the color used for Trap items sent and received"
                },
                ["filler_color"] = new TomlString
                {
                    Value = "00EEEE",
                    Comment = "Hex ID of the color used for Filler items sent and received"
                },
                ["player_color"] = new TomlString
                {
                    Value = "EE00EE",
                    Comment = "Hex ID of the color used for your own name"
                },
                ["other_player_color"] = new TomlString
                {
                    Value = "FAFAD2",
                    Comment = "Hex ID of the color used for other players"
                },
                ["location_color"] = new TomlString
                {
                    Value = "00FF7F",
                    Comment = "Hex ID of the color used for locations"
                }
            };
            try
            {
                using (StreamWriter writer = File.CreateText(path))
                {
                    config.ToTomlString(writer);
                    writer.Flush();
                }                
            }
            catch (Exception e) {Console.Write(e);}
        }
    }
}