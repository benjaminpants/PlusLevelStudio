using PlusLevelStudio.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            LevelStudioPlugin.Instance.StartCoroutine(LevelStudioPlugin.Instance.LoadEditorScene("full", EditorController.lastPlayedLevel == null ? null : Path.Combine(LevelStudioPlugin.levelFilePath, EditorController.lastPlayedLevel + ".ebpl")));
            Destroy(gameObject);
        }
    }
}
