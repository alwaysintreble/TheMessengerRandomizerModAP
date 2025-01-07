using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MessengerRando.Utils;
using MessengerRando.Utils.Constants;
using Mod.Courier;
using UnityEngine;

namespace MessengerRando.GameOverrideManagers
{
    public static class TrapManager
    {
        private static bool teleTrapped;
        public static float TrapTimer;
        private static float trapTime = 10.0f;
        private static PlayerController player;
        private static bool currentlyDark;
        private static int queuedDarkness;
        private static int queuedTeleports;
        private static string darknessPath = $"{Courier.ModsFolder}\\TheMessengerRandomizerAP\\Darkness Test.prefab";
        public static Vector3 PreTeleLocation;
        
        public static void StartProphecyCutscene()
        {
            try
            {
                Console.WriteLine("doing a prophecy");
                player = Manager<PlayerManager>.Instance.Player;
                player.graplou.Cancel();
                player.Unduck();
                player.SetInvincibility("prophecy trap", true);
                player.StateMachine.SetState<PlayerCinematicState>();
                player.StopRunning();
                player.BlockInputs("prophecy");
                DoProphecyDialog();
            }
            catch (Exception e)
            {
                e.LogDetailed();
            }
        }

        private static void EndProphecyCutscene(View view)
        {
            player.SetInvincibility("prophecy trap", false);
            player.StateMachine.SetState<PlayerDefaultState>();
            player.UnblockInputs("prophecy");
            Manager<UIManager>.Instance.onScreenOutDone -= EndProphecyCutscene;
        }

        private static void DoProphecyDialog()
        {
            var dialogBox = ScriptableObject.CreateInstance<DialogSequence>();
            dialogBox.dialogID = "PROPHECY_TRAP";
            dialogBox.name = "*ahem*";
            dialogBox.choices = new List<DialogSequenceChoice>();
            var dialogInfoList = new List<DialogInfo>();
            var dialogToAdd = ProphecyTrapDialog[RandomizerStateManager.SeedRandom.Next(ProphecyTrapDialog.Count)];
            var count = 0;
            foreach (var info in dialogToAdd.Select(text => new DialogInfo
                     {
                         text = text,
                         textID = $"PROPHECY_{count}",
                         characterDefinition = FlavorDialogManager.ProphetDefinition,
                         skippable = false,
                         autoClose = true,
                         autoCloseDelay = 1
                     }))
            {
                count++;
                
                dialogInfoList.Add(info);
            }
            FieldInfo dialogInfoListField =
                typeof(DialogSequence).GetField("dialogInfoList", BindingFlags.NonPublic | BindingFlags.Instance);
            dialogInfoListField.SetValue(dialogBox, dialogInfoList);

            var popupParams = new DialogBoxParams(dialogBox);
            Manager<UIManager>.Instance.ShowView<DialogBox>(EScreenLayers.PROMPT, popupParams);
            
            Manager<UIManager>.Instance.onScreenOutDone += EndProphecyCutscene;
        }

        public static void TryTeleportPlayer()
        {
            var level = Manager<LevelManager>.Instance;
            if (AvailableTrapLevels.Contains(level.GetCurrentLevelEnum()) &&
                RandomizerStateManager.IsSafeTeleportState())
            {
                PreTeleLocation = Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition;
                teleTrapped = true;
                var teleportLoc =
                    TeleportTrapLocations.Where(randoLevel =>
                        randoLevel.LevelName.Equals(level.GetCurrentLevelEnum())).ToList()[0];
                RandoLevelManager.TeleportInArea(teleportLoc);
            }
            else
            {
                queuedTeleports++;
            }
        }

        private static void CheckQueuedTeleport()
        {
            if (queuedTeleports == 0) return;
            if (!RandomizerStateManager.IsSafeTeleportState()) return;
            var level = Manager<LevelManager>.Instance;
            if (!AvailableTrapLevels.Contains(level.GetCurrentLevelEnum())) return;
            PreTeleLocation = Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition;
            teleTrapped = true;
            queuedTeleports--;
            var teleportLoc = TeleportTrapLocations
                .Where(randoLevel => randoLevel.LevelName.Equals(level.GetCurrentLevelEnum())).ToList()[0];
            RandoLevelManager.TeleportInArea(teleportLoc);
        }

        public static void ResetPlayerState()
        {
            if (!teleTrapped) return;
            Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition = PreTeleLocation;
            teleTrapped = false;
        }

        public static void StartDarkness()
        {
            if (currentlyDark)
            {
                queuedDarkness++;
                return;
            }

            try
            {
                var darkness = AssetBundle.LoadFromFile(darknessPath);
                UnityEngine.Object.Instantiate(darkness, Manager<PlayerManager>.Instance.Player.transform);
            }
            catch (Exception e)
            {
                e.LogDetailed();
            }
            currentlyDark = true;
        }

        public static void EndDarkness()
        {
            if (!currentlyDark) return;
            if (queuedDarkness > 0)
            {
                queuedDarkness--;
                return;
            }

            Manager<UIManager>.Instance.CloseAllScreensOfType<OptionScreen>(false);
            Manager<UIManager>.Instance.CloseAllScreensOfType<CinematicBordersScreen>(false);
            Manager<UIManager>.Instance.CloseAllScreensOfType<TransitionScreen>(false);
            Manager<UIManager>.Instance.CloseAllScreensOfType<SavingScreen>(false);
            Manager<UIManager>.Instance.CloseAllScreensOfType<LoadingAnimation>(false);
        }

        private static readonly List<List<string>> ProphecyTrapDialog = new List<List<string>>
        {
            new()
            {
                "*ahem* AS WAS FORETOLD IN THE VISIONS. <color=#6844fc>HE</color> CARRIES FORTH THE <color=#00fcfc>MESSAGE</color> FOR ALL THOSE TO BEAR WITNESS",
                "AND THUS HE MASTERED TIME ITSELF (though maybe not those pesky pits as much)."
            },
            new()
            {
                "*ahem* THE MISTS OF LEGEND ARE QUITE COMPLEX AND FOGGY, BUT WE ARE HERE TO GUIDE YOU FURTHER.",
                "ASK THUS AND BE THUSLY PROVIDED YE OF THE SHALLOW CUP FOR HE SHALL JUMP OVER THAT PROJECTILE.",
            },
            new()
            {
                "*ahem* DID YOU EVER HEAR THE TRAGEDY OF <color=#F30709>DARTH PLAGUEIS</color> THE WISE?",
                "I THOUGHT NOT. IT'S NOT A STORY THE JEDI WOULD TELL YOU.",
                "IT'S A SITH LEGEND. <color=#F30709>DARTH PLAGUEIS</color> WAS A DARK LORD OF THE SITH, SO POWERFUL AND SO WISE, HE COULD USE THE FORCE TO INFLUENCE THE MIDICHLORIANS TO CREATE LIFE...",
                "HE HAD SUCH A KNOWLEDGE OF THE DARK SIDE THAT HE COULD EVEN KEEP THE ONES HE CARED ABOUT FROM DYING.",
                "THE DARK SIDE OF THE FORCE IS A PATHWAY TO MANY ABILITIES SOME CONSIDER TO BE UNNATURAL.",
                "HE BECAME SO POWERFUL... THE ONLY THING HE WAS AFRAID OF WAS LOSING HIS POWER, WHICH EVENTUALLY, OF COURSE, HE DID.",
                "UNFORTUNATELY, HE TAUGHT HIS APPRENTICE EVERYTHING HE KNEW, THEN HIS APPRENTICE KILLED HIM IN HIS SLEEP.",
                "<color=#E2A720>IRONIC</color>.",
                "HE COULD SAVE OTHERS FROM DEATH, BUT NOT HIMSELF."
            }
        };

        private static readonly List<LevelConstants.RandoLevel> TeleportTrapLocations =
        [
            new LevelConstants.RandoLevel(ELevel.Level_09_A_GlacialPeak, new Vector3(127, -55)),
            // new LevelConstants.RandoLevel(ELevel.Level_08_SearingCrags, new Vector3(456, 136)),
            new LevelConstants.RandoLevel(ELevel.Level_07_QuillshroomMarsh, new Vector3(797, -38)),
            new LevelConstants.RandoLevel(ELevel.Level_05_B_SunkenShrine, new Vector3(162, -109)),
            new LevelConstants.RandoLevel(ELevel.Level_05_A_HowlingGrotto, new Vector3(253, -75)),
            new LevelConstants.RandoLevel(ELevel.Level_02_AutumnHills, new Vector3(458, -50))
        ];

        private static readonly List<ELevel> AvailableTrapLevels =
            TeleportTrapLocations.Select(level => level.LevelName).ToList();

        public static void UpdateTrapStatus()
        {
            if (TrapTimer < trapTime) return;
            TrapTimer = 0;
            EndDarkness();
            CheckQueuedTeleport();
        }
    }
}