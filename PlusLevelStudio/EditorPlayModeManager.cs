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
        public void OnExit()
        {
            GoToEditor();
        }

        public void GoToEditor()
        {
            Singleton<MusicManager>.Instance.StopMidi();
            LevelStudioPlugin.Instance.StartCoroutine(LevelStudioPlugin.Instance.LoadEditorScene("full", EditorController.lastPlayedLevel == null ? null : Path.Combine(LevelStudioPlugin.levelFilePath, EditorController.lastPlayedLevel + ".ebpl"), EditorController.lastPlayedLevel));
            Destroy(gameObject);
        }

        // this is hacky and gross
        public static void LoadLevel(GameLoader gameLoaderPrefab, ElevatorScreen elevatorScreenPrefab, EditorPlayModeManager editorPlayModePre, PlayableEditorLevel level)
        {
            SceneObject sceneObj = LevelImporter.CreateSceneObject(level.data);
            sceneObj.manager = LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].prefab;
            GameLoader loader = GameObject.Instantiate<GameLoader>(gameLoaderPrefab);
            ElevatorScreen screen = GameObject.Instantiate<ElevatorScreen>(elevatorScreenPrefab);
            GameObject.Instantiate<EditorPlayModeManager>(editorPlayModePre);
            loader.AssignElevatorScreen(screen);
            loader.Initialize(0);
            loader.SetMode(0);
            loader.LoadLevel(sceneObj);
            screen.Initialize();
            loader.SetSave(false);
        }
    }
}
