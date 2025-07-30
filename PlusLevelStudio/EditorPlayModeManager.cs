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
        public void OnExit()
        {
            GoToEditor();
        }

        public void GoToEditor()
        {
            Singleton<MusicManager>.Instance.StopMidi();
            LevelStudioPlugin.Instance.StartCoroutine(LevelStudioPlugin.Instance.LoadEditorScene("full", EditorController.lastPlayedLevel == null ? null : Path.Combine(LevelStudioPlugin.levelFilePath, EditorController.lastPlayedLevel + ".ebpl"), EditorController.lastPlayedLevel));
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
            Destroy(gameObject);
        }

        // this is hacky and gross
        public static void LoadLevel(GameLoader gameLoaderPrefab, ElevatorScreen elevatorScreenPrefab, EditorPlayModeManager editorPlayModePre, PlayableEditorLevel level)
        {
            SceneObject sceneObj = LevelImporter.CreateSceneObject(level.data);
            sceneObj.manager = LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].prefab;
            GameLoader loader = GameObject.Instantiate<GameLoader>(gameLoaderPrefab);
            ElevatorScreen screen = GameObject.Instantiate<ElevatorScreen>(elevatorScreenPrefab);
            EditorPlayModeManager pmm = GameObject.Instantiate<EditorPlayModeManager>(editorPlayModePre);
            pmm.customContent = new EditorCustomContent(); // TODO: get this from PlayableEditorLevel somehow
            if (level.meta.modeSettings != null)
            {
                BaseGameManager modifiedManager = GameObject.Instantiate<BaseGameManager>(LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].prefab, MTM101BaldAPI.MTM101BaldiDevAPI.prefabTransform);
                modifiedManager.name = modifiedManager.name.Replace("(Clone)", "_Customized");
                level.meta.modeSettings.ApplySettingsToManager(modifiedManager);
                sceneObj.manager = modifiedManager;
                pmm.customContent.gameManagerPre = modifiedManager;
            }
            pmm.sceneObjectsToCleanUp.Add(sceneObj);
            loader.AssignElevatorScreen(screen);
            loader.Initialize(0);
            loader.SetMode(0);
            loader.LoadLevel(sceneObj);
            screen.Initialize();
            loader.SetSave(false);
        }
    }
}
