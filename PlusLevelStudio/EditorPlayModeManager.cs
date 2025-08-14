using PlusLevelStudio.Editor;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.UIElements;
using UnityEngine;

namespace PlusLevelStudio
{
    public class EditorPlayModeManager : Singleton<EditorPlayModeManager>
    {
        public EditorCustomContent customContent;
        public List<SceneObject> sceneObjectsToCleanUp = new List<SceneObject>();
        public bool returnToEditor = true;
        public string editorLevelToLoad;
        public string editorModeToLoad;
        public void OnExit()
        {
            if (customContent != null)
            {
                customContent.CleanupContent();
            }
            if (!returnToEditor)
            {
                Destroy(gameObject);
                return;
            }
            GoToEditor();
        }

        public void GoToEditor()
        {
            Singleton<MusicManager>.Instance.StopMidi();
            LevelStudioPlugin.Instance.StartCoroutine(LevelStudioPlugin.Instance.LoadEditorScene(editorModeToLoad, editorLevelToLoad == null ? null : Path.Combine(LevelStudioPlugin.levelFilePath, editorLevelToLoad + ".ebpl"), editorLevelToLoad));
            for (int i = 0; i < sceneObjectsToCleanUp.Count; i++)
            {
                GameObject.Destroy(sceneObjectsToCleanUp[i].extraAsset);
                GameObject.Destroy(sceneObjectsToCleanUp[i].levelAsset);
                GameObject.Destroy(sceneObjectsToCleanUp[i]);
            }
            Destroy(gameObject);
        }

        public static void LoadLevel(PlayableEditorLevel level, int lives, bool returnToEditor, string levelToLoad = null, string modeToLoad = "full")
        {
            // we must establish the PlayModeManager first so we can load custom content BEFORE the SceneObject is created.
            EditorPlayModeManager pmm = GameObject.Instantiate<EditorPlayModeManager>(LevelStudioPlugin.Instance.assetMan.Get<EditorPlayModeManager>("playModeManager"));
            pmm.customContent = new EditorCustomContent();
            pmm.customContent.LoadFromPackage(level.meta.contentPackage);
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
                pmm.customContent.gameManagerPre = modifiedManager;
            }
            pmm.sceneObjectsToCleanUp.Add(sceneObj);
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
