using System;
using System.IO;
using MessengerRando.Archipelago;
using MessengerRando.GameOverrideManagers;
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
        public static string PriorityColor = "AF99EF";
        public static string AvoidColor = "FA8072";
        public static string NoPriorityColor = "00EEEE";
        public static string UnspecifiedColor = "FFFFFF";
        
        private static TomlTable configTable;

        public static void ReadConfig(string path)
        {
            path += "\\" + configFileName;
            if (!File.Exists(path))
            {
                GenerateConfig(path);
                return;
            }

            configTable = TOML.Parse(new StreamReader(File.OpenRead(path)));
            HostName = configTable.get_Item("host_name").AsString.Value;
            Port = (int)configTable.get_Item("port").AsInteger.Value;
            SlotName = configTable.get_Item("slot_name").AsString.Value;
            Password = configTable.get_Item("password").AsString.Value;
            try
            {
                StatusTextSize = (float)configTable.get_Item("status_text_size").AsFloat.Value;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                Debug.Log("failed to get status size as float, trying int");
                StatusTextSize = configTable.get_Item("status_text_size").AsInteger.Value;
            }

            try
            {
                MessageTextSize = (float)configTable.get_Item("message_text_size").AsFloat.Value;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                Debug.Log("failed to get message size as float, trying int");
                MessageTextSize = configTable.get_Item("message_text_size").AsInteger.Value;
            }

            try
            {
                AdvancementColor = configTable.get_Item("advancement_color").AsString.Value;
                UsefulColor = configTable.get_Item("useful_color").AsString.Value;
                TrapColor = configTable.get_Item("trap_color").AsString.Value;
                FillerColor = configTable.get_Item("filler_color").AsString.Value;
                PlayerColor = configTable.get_Item("player_color").AsString.Value;
                OtherPlayerColor = configTable.get_Item("other_player_color").AsString.Value;
                LocationColor = configTable.get_Item("location_color").AsString.Value;
                RandoMusicManager.ShuffleMusic = configTable.get_Item("music_shuffle").AsBoolean.Value;
                ArchipelagoClient.DisplayStatus = configTable.get_Item("show_status_info").AsBoolean.Value;
                ArchipelagoClient.DisplayAPMessages = configTable.get_Item("show_server_messages").AsBoolean.Value;
                ArchipelagoClient.FilterAPMessages = configTable.get_Item("filter_server_messages").AsBoolean.Value;
                ArchipelagoClient.HintPopUps = configTable.get_Item("hint_popups").AsBoolean.Value;
                APRandomizerMain.UpdateTime = (float)configTable.get_Item("message_display_timer").AsFloat.Value;
                RandoShopManager.ShopHints = configTable.get_Item("shop_hints").AsBoolean.Value;
                PriorityColor = configTable.get_Item("priority_color").AsString.Value;
                AvoidColor = configTable.get_Item("avoid_color").AsString.Value;
                NoPriorityColor = configTable.get_Item("no_priority_color").AsString.Value;
                UnspecifiedColor = configTable.get_Item("unspecified_color").AsString.Value;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private static void GenerateConfig(string path)
        {
            configTable = new TomlTable
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
                    Value = 4,
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
                ["priority_color"] = new TomlString
                {
                    Value = "AF99EF",
                    Comment = "Hex ID of the color used for Priority hints"
                },
                ["avoid_color"] = new TomlString
                {
                    Value = "FA8072",
                    Comment = "Hex ID of the color used for Avoid hints"
                },
                ["no_priority_color"] = new TomlString
                {
                    Value = "00EEEE",
                    Comment = "Hex ID of the color used for No Priority hints"
                },
                ["unspecified_color"] = new TomlString
                {
                    Value = "000000",
                    Comment = "Hex ID of the color used for Unspecified hints"
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
                },
                ["music_shuffle"] = new TomlBoolean
                {
                    Comment = "Whether music should be randomized. Very rudimentary currently.\n" +
                              "A random track whenever the music changes, such as loading into a level,\n" +
                              "returning to HQ, or dying. The separate dimensions also have separate random tracks."
                },
                ["show_status_info"] = new TomlBoolean
                {
                    Value = true,
                    Comment = "Shows current Archipelago status, including current connection status and hint points."
                },
                ["show_server_messages"] = new TomlBoolean
                {
                    Value = true,
                    Comment = "Shows messages received from the server under current status."
                },
                ["filter_server_messages"] = new TomlBoolean
                {
                    Comment = "Filters the received messages to only those relevant to you."
                },
                ["hint_popups"] = new TomlBoolean
                {
                    Value = true,
                    Comment = "Shows message popups for when a new hint is received concerning you."
                },
                ["message_display_timer"] = new TomlFloat
                {
                    Value = 3f,
                    Comment = "How long to display message popups and server messages.\n" +
                              "Supports decimal places. e.g. 2.5 for 2.5 seconds"
                },
                ["shop_hints"] = new TomlBoolean
                {
                    Value = true,
                    Comment = "Whether items in the shop should display the item, its importance, and hint it out.\n" +
                              "Force disabled in race mode."
                }
            };
            SaveConfig(path);
        }

        public static void UpdateConfig(string path)
        {
            path += "\\" + configFileName;
            configTable.set_Item("host_name", new TomlString
            {
                Value = "archipelago.gg",
                Comment =
                    "Connection info defined here is only used if no slot name is entered in the in-game menu.\n" +
                    "If a slot name is defined here, then all information here is used."
            });
            configTable.set_Item("port", 38281);
            configTable.set_Item("slot_name", "");
            configTable.set_Item("password", "");
            configTable.set_Item("music_shuffle", new TomlBoolean
            {
                Value = RandoMusicManager.ShuffleMusic,
                Comment = "Whether music should be randomized. Very rudimentary currently.\n" +
                          "A random track whenever the music changes, such as loading into a level,\n" +
                          "returning to HQ, or dying. The separate dimensions also have separate random tracks."
            });
            configTable.set_Item("show_status_info", new TomlBoolean
            {
                Value = ArchipelagoClient.DisplayStatus,
                Comment = "Shows current Archipelago status, including current connection status and hint points."
            });
            configTable.set_Item("show_server_messages", new TomlBoolean
            {
                Value = ArchipelagoClient.DisplayAPMessages,
                Comment = "Shows messages received from the server under current status."
            });
            configTable.set_Item("filter_server_messages", new TomlBoolean
            {
                Value = ArchipelagoClient.FilterAPMessages,
                Comment = "Filters the received messages to only those relevant to you."
            });
            configTable.set_Item("hint_popups", new TomlBoolean
            {
                Value = ArchipelagoClient.HintPopUps,
                Comment = "Shows message popups for when a new hint is received concerning you."
            });
            configTable.set_Item("message_display_timer", new TomlFloat
            {
                Value = APRandomizerMain.UpdateTime,
                Comment = "How long to display message popups and server messages.\n" +
                          "Supports decimal places. e.g. 2.5 for 2.5 seconds"
            });
            configTable.set_Item("shop_hints", new TomlBoolean
            {
                Value = RandoShopManager.ShopHints,
                Comment = "Whether items in the shop should display the item, its importance, and hint it out.\n" +
                          "Force disabled in race mode."
            });
            
            SaveConfig(path);
        }
        
        private static void SaveConfig(string path)
        {
            try
            {
                using StreamWriter writer = File.CreateText(path);
                configTable.ToTomlString(writer);
                writer.Flush();
            }
            catch (Exception e) {Console.Write(e);}
        }
    }
}