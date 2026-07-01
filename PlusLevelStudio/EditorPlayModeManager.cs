using PlusLevelStudio.Editor;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.UIElements;
using UnityEngine;
using TMPro;
using PlusLevelStudio.Campaigns;
using System.Reflection;
using HarmonyLib;

namespace PlusLevelStudio
{
    public class EditorPlayModeManager : Singleton<EditorPlayModeManager>
    {
        public EditorCustomContent customContent;
        public ChallengeWin winScreen;
        public TextMeshProUGUI winText;
        public List<SceneObject> sceneObjectsToCleanUp = new List<SceneObject>();
        public List<PlaymodeSettingsMeta> sceneObjectSettings = new List<PlaymodeSettingsMeta>();
        public SceneObject pitstopScene;
        public bool returnToEditor = false;
        public string editorLevelToLoad;
        public string editorModeToLoad;
        public bool waitingForCreation = false;

        public PlaymodeSettingsMeta GetSettingsFor(SceneObject obj)
        {
            int index = sceneObjectsToCleanUp.IndexOf(obj);
            if (index < 0) return new PlaymodeSettingsMeta();
            PlaymodeSettingsMeta settings = sceneObjectSettings[index];
            return settings == null ? new PlaymodeSettingsMeta() : settings;
        }
        
        public void CleanupEverything()
        {
            if (customContent != null)
            {
                customContent.CleanupContent();
            }
            for (int i = 0; i < sceneObjectsToCleanUp.Count; i++)
            {
                GameObject.Destroy(sceneObjectsToCleanUp[i].extraAsset);
                GameObject.Destroy(sceneObjectsToCleanUp[i].levelAsset);
                GameObject.Destroy(sceneObjectsToCleanUp[i]);
            }
        }

        public void OnExit()
        {
            CleanupEverything();
            if (!returnToEditor)
            {
                Destroy(gameObject);
                return;
            }
            GoToEditor();
        }



        static FieldInfo _elevatorScreen = AccessTools.Field(typeof(BaseGameManager), "elevatorScreen");
        static FieldInfo _elevatorScreenPre = AccessTools.Field(typeof(BaseGameManager), "elevatorScreenPre");
        static FieldInfo _time = AccessTools.Field(typeof(BaseGameManager), "time");
        static MethodInfo _PrepareToLoad = AccessTools.Method(typeof(BaseGameManager), "PrepareToLoad");
        // I hate copying big chunks of vanilla bb+ code like this, however in this particular case it is warranted due to this behavior needing to be extracted and modified for the sake of playmode.
        public void CampaignLoadNextLevel(BaseGameManager man, EnvironmentController ec)
        {
            Singleton<HighlightManager>.Instance.Highlight("steam_completed", Singleton<LocalizationManager>.Instance.GetLocalizedText("Steam_Highlight_Win"), string.Format(Singleton<LocalizationManager>.Instance.GetLocalizedText("Steam_Highlight_Win_Desc"), 0), 2U, 0f, 0f, TimelineEventClipPriority.Standard);
            int num = 0;
            Baldi baldi = ec.GetBaldi();
            if (baldi != null)
            {
                num = ec.NavigableDistance(ec.CellFromPosition(Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position), ec.CellFromPosition(baldi.transform.position), PathType.Nav);
                if (num < 0)
                {
                    num = ec.NavigableDistance(ec.CellFromPosition(Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position), ec.CellFromPosition(baldi.transform.position), PathType.Const) * 2;
                    if (num < 0)
                    {
                        num = 100;
                    }
                }
            }
            int stickerBonuses = Singleton<CoreGameManager>.Instance.GetStickerBonuses(ec.RemainingTime, num, ec.map.PlayerDiscoveredCells);
            Singleton<CoreGameManager>.Instance.AddPoints(stickerBonuses, 0, false, false, true);
            Singleton<CoreGameManager>.Instance.saveMapAvailable = false;
            Singleton<CoreGameManager>.Instance.saveMapPurchased = false;
            for (int i = 0; i < 2 - Singleton<CoreGameManager>.Instance.Attempts; i++)
            {
                Singleton<CoreGameManager>.Instance.AddPoints(Singleton<CoreGameManager>.Instance.GetPointsThisLevel(0), 0, false, false, false);
            }
            _PrepareToLoad.Invoke(man, null);
            ElevatorScreen elevatorScreen = UnityEngine.Object.Instantiate<ElevatorScreen>((ElevatorScreen)_elevatorScreenPre.GetValue(man));
            _elevatorScreen.SetValue(man, elevatorScreen);
            elevatorScreen.OnLoadReady += () =>
            {
                CampaignLoadReady(man);
            };
            elevatorScreen.Initialize();
            // grades arent used anymore why is this here.
            /*if (this.problems > 0)
            {
                Singleton<CoreGameManager>.Instance.GradeVal += -Mathf.RoundToInt(this.gradeValue * ((float)this.correctProblems / (float)this.problems * 2f - 1f));
            }*/
            elevatorScreen.ShowResults((float)_time.GetValue(man), Mathf.RoundToInt((float)stickerBonuses * Singleton<CoreGameManager>.Instance.YtpMultiplier));
        }

        protected void CampaignLoadReady(BaseGameManager man)
        {
            man.StopAllCoroutines();
            man.Ec.gameObject.SetActive(false);
            Singleton<CoreGameManager>.Instance.PrepareForReload();
            Singleton<CoreGameManager>.Instance.tripPlayed = false;
            Singleton<SubtitleManager>.Instance.DestroyAll();
            // actually load new level
            CampaignLoadLevel(Singleton<CoreGameManager>.Instance.sceneObject.nextLevel, false);
        }

        public void CampaignRestartLevel(BaseGameManager man)
        {
            Singleton<CoreGameManager>.Instance.saveMapAvailable = true;
            _PrepareToLoad.Invoke(man, null);
            ElevatorScreen elevatorScreen = UnityEngine.Object.Instantiate<ElevatorScreen>((ElevatorScreen)_elevatorScreenPre.GetValue(man));
            _elevatorScreen.SetValue(man, elevatorScreen);
            elevatorScreen.OnLoadReady += () =>
            {
                CampaignRestartLevelReady(man);
            };
            elevatorScreen.Initialize();
        }

        protected void CampaignRestartLevelReady(BaseGameManager man)
        {
            Singleton<CoreGameManager>.Instance.PrepareForReload();
            Singleton<CoreGameManager>.Instance.BackupMap(man.Ec.map);
            Singleton<CoreGameManager>.Instance.RestorePlayers();
            CampaignLoadLevel(Singleton<CoreGameManager>.Instance.sceneObject, true);
        }

        public void CampaignLoadLevel(SceneObject sceneObj, bool restarting)
        {
            Singleton<CoreGameManager>.Instance.nextLevel = sceneObj;
            SceneObject sceneObjToLoad = sceneObj;
            if (GetSettingsFor(sceneObj).hasPitstop) // TODO: implement finalLevel.
            {
                sceneObjToLoad = pitstopScene;
            }
            Singleton<CoreGameManager>.Instance.sceneObject = sceneObjToLoad;
            Singleton<AdditiveSceneManager>.Instance.LoadScene("Game");
        }

        public void Win(string text = "Congratulation! You won!")
        {
            Singleton<MusicManager>.Instance.StopMidi();
            Singleton<MusicManager>.Instance.StopFile();
            AudioListener.pause = true;
            Time.timeScale = 0f;
            Singleton<CoreGameManager>.Instance.disablePause = true;
            winText.text = text;
            winScreen.gameObject.SetActive(true);
        }

        public void GoToEditor()
        {
            Singleton<MusicManager>.Instance.StopMidi();
            LevelStudioPlugin.Instance.StartCoroutine(LevelStudioPlugin.Instance.LoadEditorScene(editorModeToLoad, editorLevelToLoad == null ? null : Path.Combine(LevelStudioPlugin.levelFilePath, editorLevelToLoad + ".ebpl"), editorLevelToLoad));
            Destroy(gameObject);
        }

        void Update()
        {
            if (waitingForCreation)
            {
                if (Singleton<BaseGameManager>.Instance != null)
                {
                    waitingForCreation = false;
                    if (Singleton<BaseGameManager>.Instance is IStudioLegacyKnowledgable)
                    {
                        ((IStudioLegacyKnowledgable)Singleton<BaseGameManager>.Instance).legacyFlags |= this.customContent.legacyFlags;
                    }
                }
            }
        }

        public static void LoadCampaign(PlayableEditorCampaign campaign, int lives, LifeMode mode)
        {
            EditorPlayModeManager pmm = GameObject.Instantiate<EditorPlayModeManager>(LevelStudioPlugin.Instance.assetMan.Get<EditorPlayModeManager>("playModeManager"));
            pmm.waitingForCreation = true;
            pmm.customContent = new EditorCustomContent(campaign.contentPackage);
            List<SceneObject> createdObjects = new List<SceneObject>();
            for (int i = 0; i < campaign.levels.Count; i++)
            {
                PlayableEditorLevel level = campaign.levels[i];
                SceneObject sceneObj = LevelImporter.CreateSceneObject(level.data);
                if (i != 0)
                {
                    createdObjects[i - 1].nextLevel = sceneObj;
                }
                createdObjects.Add(sceneObj);
                sceneObj.manager = LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].prefab;
                if (level.meta.modeSettings != null)
                {
                    BaseGameManager modifiedManager = GameObject.Instantiate<BaseGameManager>(LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].prefab, MTM101BaldAPI.MTM101BaldiDevAPI.prefabTransform);
                    modifiedManager.name = modifiedManager.name.Replace("(Clone)", "_Customized_" + i);
                    level.meta.modeSettings.ApplySettingsToManager(modifiedManager);
                    sceneObj.manager = modifiedManager;
                    pmm.customContent.gameManagerPre.Add(modifiedManager);
                }
                pmm.sceneObjectsToCleanUp.Add(sceneObj);
                pmm.sceneObjectSettings.Add(new PlaymodeSettingsMeta());
            }

            // load stuff now
            GameLoader loader = GameObject.Instantiate<GameLoader>(LevelStudioPlugin.Instance.assetMan.Get<GameLoader>("gameLoaderPrefab"));
            ElevatorScreen screen = GameObject.Instantiate<ElevatorScreen>(LevelStudioPlugin.Instance.assetMan.Get<ElevatorScreen>("elevatorScreenPrefab"));
            loader.AssignElevatorScreen(screen);
            loader.Initialize(lives);
            loader.SetMode(0);
            loader.LoadLevel(createdObjects[0]);
            screen.Initialize();
            loader.SetSave(false);
            Singleton<CoreGameManager>.Instance.lifeMode = mode;
        }

        public static void LoadLevel(PlayableEditorLevel level, int lives, bool returnToEditor, string levelToLoad = null, string modeToLoad = "full")
        {
            // we must establish the PlayModeManager first so we can load custom content BEFORE the SceneObject is created.
            if (Singleton<HighScoreManager>.Instance != null)
            {
                Singleton<HighScoreManager>.Instance.currentLevelId = string.Empty; // no.
            }
            EditorPlayModeManager pmm = GameObject.Instantiate<EditorPlayModeManager>(LevelStudioPlugin.Instance.assetMan.Get<EditorPlayModeManager>("playModeManager"));
            pmm.waitingForCreation = true;
            pmm.customContent = new EditorCustomContent(level.meta.contentPackage);
            SceneObject sceneObj = LevelImporter.CreateSceneObject(level.data);
            sceneObj.manager = LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].prefab;
            GameLoader loader = GameObject.Instantiate<GameLoader>(LevelStudioPlugin.Instance.assetMan.Get<GameLoader>("gameLoaderPrefab"));
            ElevatorScreen screen = GameObject.Instantiate<ElevatorScreen>(LevelStudioPlugin.Instance.assetMan.Get<ElevatorScreen>("elevatorScreenPrefab"));
            pmm.returnToEditor = returnToEditor;
            if (level.meta.modeSettings != null)
            {
                BaseGameManager modifiedManager = GameObject.Instantiate<BaseGameManager>(LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].prefab, MTM101BaldAPI.MTM101BaldiDevAPI.prefabTransform);
                modifiedManager.name = modifiedManager.name.Replace("(Clone)", "_Customized");
                level.meta.modeSettings.ApplySettingsToManager(modifiedManager);
                sceneObj.manager = modifiedManager;
                pmm.customContent.gameManagerPre.Add(modifiedManager);
            }
            pmm.sceneObjectsToCleanUp.Add(sceneObj);
            pmm.sceneObjectSettings.Add(new PlaymodeSettingsMeta(level.meta.playSettings));
            pmm.editorLevelToLoad = levelToLoad;
            pmm.editorModeToLoad = modeToLoad;
            loader.AssignElevatorScreen(screen);
            loader.Initialize(lives);
            loader.SetMode(0);
            loader.LoadLevel(sceneObj);
            screen.Initialize();
            loader.SetSave(false);
        }
    }
}
