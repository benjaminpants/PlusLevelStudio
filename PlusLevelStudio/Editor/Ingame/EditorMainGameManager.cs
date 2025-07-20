using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PlusLevelStudio.Editor.Ingame
{
    public class EditorMainGameManager : MainGameManager
    {
        public override void Initialize()
        {
            base.Initialize();
            if (ec.npcsToSpawn.Where(x => x.Character == Character.Baldi).Count() == 0)
            {
                Singleton<CoreGameManager>.Instance.currentMode = Mode.Free;
                ec.npcsToSpawn.Add(NPCMetaStorage.Instance.Get(Character.Baldi).value);
                ec.npcSpawnTile = ec.npcSpawnTile.AddToArray(ec.RandomCell(false,false,true));
            }
        }

        public override void LoadNextLevel()
        {
            Singleton<CoreGameManager>.Instance.Quit();
            Singleton<MusicManager>.Instance.StopMidi();
            LevelStudioPlugin.Instance.StartCoroutine(LevelStudioPlugin.Instance.LoadEditorScene(EditorController.lastPlayedLevel == null ? null : Path.Combine(LevelStudioPlugin.levelFilePath, EditorController.lastPlayedLevel + ".ebpl")));
        }
    }
}
