using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.UI;
using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PlusLevelStudio.Menus
{
    public class EditorModeSelectionMenu : MonoBehaviour
    {
        public GameObject mainMenu;

        public GameObject playOrEditParent;

        public GameObject playParent;

        public GameObject editorTypeParent;
        public GameObject restrictedTypeParent;
        public EditorPlayScreenUIHandler playScreenManager;

        public TextMeshProUGUI editModeDescription;

        internal static EditorModeSelectionMenu Build()
        {
            Canvas canvas = UIHelpers.CreateBlankUIScreen("EditorModeSelection", true, false);
            UIHelpers.AddCursorInitiatorToCanvas(canvas);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Singleton<GlobalCam>.Instance.Cam;
            canvas.planeDistance = 0.31f;

            EditorModeSelectionMenu emms = canvas.gameObject.AddComponent<EditorModeSelectionMenu>();

            UIElementBuilder.usingRegularAssetMan = true;
            EditorOrPlaySelectionUIHandler edOrPModeSelec = UIBuilder.BuildUIFromFile<EditorOrPlaySelectionUIHandler>(canvas.GetComponent<RectTransform>(), "PlayOrEdit", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "Titlescreen", "PlayOrEditPicker.json"));
            edOrPModeSelec.menu = emms;
            emms.playOrEditParent = edOrPModeSelec.gameObject;

            EditorModeSelectionUIHandler modeSelec = UIBuilder.BuildUIFromFile<EditorModeSelectionUIHandler>(canvas.GetComponent<RectTransform>(), "EditorTypeSelection", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "Titlescreen", "ModePicker.json"));
            modeSelec.menu = emms;
            emms.editorTypeParent = modeSelec.gameObject;
            emms.editorTypeParent.SetActive(false);

            EditorPlayScreenUIHandler playModeMen = UIBuilder.BuildUIFromFile<EditorPlayScreenUIHandler>(canvas.GetComponent<RectTransform>(), "PlayScreen", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "Titlescreen", "PlayMenu.json"));
            playModeMen.menu = emms;
            emms.playParent = playModeMen.gameObject;
            emms.playParent.SetActive(false);
            emms.playScreenManager = playModeMen;

            UIElementBuilder.usingRegularAssetMan = false;

            UIHelpers.AddBordersToCanvas(canvas);

            return emms;
        }
    }
}
