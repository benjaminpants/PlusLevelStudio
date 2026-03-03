using MTM101BaldAPI.AssetTools;
using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class TutorialEditorController : EditorController
    {
        public bool screenLocked = false;
        public bool movementLocked = false;

        public void LockScreen(bool locked)
        {
            screenLocked = locked;
            CursorController.Instance.DisableClick(locked);
            if (locked)
            {
                selector.DisableSelection();
            }
        }

        public void LockMovement(bool locked)
        {
            movementLocked = locked;
        }

        public void LockEverything(bool locked)
        {
            LockScreen(locked);
            LockMovement(locked);
        }

        public void ToggleBottomButtons(bool saveLoadVisible, bool globalVisible)
        {
            uiObjects[0].transform.Find("PlayButton").gameObject.SetActive(saveLoadVisible);
            uiObjects[0].transform.Find("SaveButton").gameObject.SetActive(saveLoadVisible);
            uiObjects[0].transform.Find("ExportButton").gameObject.SetActive(saveLoadVisible);
            uiObjects[0].transform.Find("LoadButton").gameObject.SetActive(saveLoadVisible);
            uiObjects[0].transform.Find("globalSettingsButton").gameObject.SetActive(globalVisible);
        }

        public override bool MovementAndToolsAllowed()
        {
            return base.MovementAndToolsAllowed() && !movementLocked;
        }

        public override void SwitchToTool(EditorTool tool)
        {
            if (screenLocked) return;
            base.SwitchToTool(tool);
        }

        protected override void HandleClicking()
        {
            if (screenLocked) return;
            base.HandleClicking();
        }

        protected override void PlaySongIfNecessary()
        {
            if (!Singleton<MusicManager>.Instance.MidiPlaying)
            {
                Singleton<MusicManager>.Instance.StopMidi();
                Singleton<MusicManager>.Instance.PlayMidi("Tutorial_MMP_Corrected", false);
            }
        }

        public override void EditorModeAssigned()
        {
            base.EditorModeAssigned();
            ToggleBottomButtons(false,false);
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            uiObjects[3] = UIBuilder.BuildUIFromFile<DummyUIExchangeHandler>(canvas.GetComponent<RectTransform>(), "Main", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "Tutorial", "Baldi.json")).gameObject;
            CursorController.Instance.transform.SetAsLastSibling();
        }
    }
}
