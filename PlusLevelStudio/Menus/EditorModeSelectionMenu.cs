using MTM101BaldAPI;
using MTM101BaldAPI.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PlusLevelStudio.Menus
{
    public partial class EditorModeSelectionMenu : MonoBehaviour
    {
        public GameObject mainMenu;

        public GameObject playOrEditParent;

        public GameObject playParent;

        public GameObject editorTypeParent;
        public GameObject restrictedTypeParent;
        public EditorPlayScreenManager playScreenManager;

        internal static EditorModeSelectionMenu Build()
        {
            Canvas canvas = UIHelpers.CreateBlankUIScreen("EditorModeSelection", true, false);
            UIHelpers.AddCursorInitiatorToCanvas(canvas);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Singleton<GlobalCam>.Instance.Cam;
            canvas.planeDistance = 0.31f;

            Image bgImage = new GameObject("BG").AddComponent<Image>();
            bgImage.transform.SetParent(canvas.transform, false);
            bgImage.transform.localPosition = new Vector3(0f, 0f, 0f);
            bgImage.rectTransform.anchorMin = Vector2.one / 2f;
            bgImage.rectTransform.anchorMax = Vector2.one / 2f;
            bgImage.rectTransform.sizeDelta = new Vector2(480f, 360f);
            bgImage.sprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("ChalkBackground");

            EditorModeSelectionMenu emms = canvas.gameObject.AddComponent<EditorModeSelectionMenu>();


            // create the main mode selection
            emms.playOrEditParent = new GameObject("PlayOrEdit");
            emms.playOrEditParent.transform.SetParent(canvas.transform, false);
            emms.playOrEditParent.transform.localPosition = Vector3.zero;

            TextMeshProUGUI editText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans36, "Edit", emms.playOrEditParent.transform, Vector3.zero);
            editText.rectTransform.sizeDelta = new Vector2(100f,64f);
            editText.alignment = TextAlignmentOptions.Center;
            editText.transform.localPosition += Vector3.down * 64f;
            editText.raycastTarget = true;
            StandardMenuButton editButton = editText.gameObject.ConvertToButton<StandardMenuButton>();
            editButton.underlineOnHigh = true;
            editButton.transitionOnPress = true;
            editButton.transitionTime = 0.0167f;
            editButton.transitionType = UiTransition.Dither;
            editButton.OnPress.AddListener(() =>
            {
                emms.editorTypeParent.SetActive(true);
                emms.playOrEditParent.SetActive(false);
                //LevelStudioPlugin.Instance.GoToEditor();
            });


            TextMeshProUGUI playText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans36, "Play", emms.playOrEditParent.transform, Vector3.zero);
            playText.rectTransform.sizeDelta = new Vector2(100f, 64f);
            playText.alignment = TextAlignmentOptions.Center;
            playText.transform.localPosition += Vector3.up * 64f;
            playText.raycastTarget = true;
            StandardMenuButton playButton = playText.gameObject.ConvertToButton<StandardMenuButton>();
            playButton.underlineOnHigh = true;
            playButton.transitionOnPress = true;
            playButton.transitionTime = 0.0167f;
            playButton.transitionType = UiTransition.Dither;
            playButton.OnPress.AddListener(() =>
            {
                emms.playParent.SetActive(true);
                emms.playScreenManager.UpdateFromFolder();
                emms.playScreenManager.ChangePage(0);
                emms.playScreenManager.SetFileWatcherStatus(true);
                emms.playOrEditParent.SetActive(false);
            });

            AddBackButton(emms.playOrEditParent.transform, () =>
            {
                emms.playScreenManager.SetFileWatcherStatus(false);
                emms.gameObject.SetActive(false);
                emms.mainMenu.SetActive(true);
            });


            // create the play menu

            emms.playParent = new GameObject("PlayScreen");
            emms.playParent.SetActive(false);
            emms.playParent.transform.SetParent(canvas.transform, false);
            emms.playParent.transform.localPosition = Vector3.zero;

            CreatePlayModeMenu(emms);

            AddBackButton(emms.playParent.transform, () =>
            {
                emms.playParent.SetActive(false);
                emms.playOrEditParent.SetActive(true);
            });

            emms.editorTypeParent = new GameObject("EditorTypeSelection");
            emms.editorTypeParent.SetActive(false);
            emms.editorTypeParent.transform.SetParent(canvas.transform, false);
            emms.editorTypeParent.transform.localPosition = Vector3.zero;

            AddBackButton(emms.editorTypeParent.transform, () =>
            {
                emms.playOrEditParent.SetActive(true);
                emms.editorTypeParent.SetActive(false);
            });

            CreateMenuButton(emms.editorTypeParent.transform, "FullButton", "Full", new Vector3(0f, 64f, 0f), () => { LevelStudioPlugin.Instance.GoToEditor("full"); });
            CreateMenuButton(emms.editorTypeParent.transform, "ComplaintButton", "Compliant", new Vector3(0f, 0f, 0f), () => { LevelStudioPlugin.Instance.GoToEditor("compliant"); });
            CreateMenuButton(emms.editorTypeParent.transform, "RoomsButton", "Rooms", new Vector3(0f, -64f, 0f), () => { LevelStudioPlugin.Instance.GoToEditor("rooms"); });

            UIHelpers.AddBordersToCanvas(canvas);
            return emms;
        }

        // yoinked from classic reimplemented
        static StandardMenuButton CreateMenuButton(Transform parent, string name, string text, Vector3 localPosition, UnityAction action)
        {
            Image mainButton = new GameObject(name).AddComponent<Image>();
            mainButton.transform.SetParent(parent, false);
            mainButton.transform.localPosition = new Vector3(0f, 0f, 0f);
            mainButton.rectTransform.anchorMin = Vector2.one / 2f;
            mainButton.rectTransform.anchorMax = Vector2.one / 2f;
            mainButton.rectTransform.sizeDelta = new Vector2(140f, 38f);
            mainButton.color = Color.clear;
            TextMeshProUGUI textObj = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans24, text, mainButton.transform, Vector3.zero, false);
            textObj.rectTransform.sizeDelta = new Vector2(200f, 40f);
            textObj.alignment = TextAlignmentOptions.Center;


            mainButton.transform.localPosition = localPosition;
            StandardMenuButton but = mainButton.gameObject.ConvertToButton<StandardMenuButton>();
            but.text = textObj;
            but.underlineOnHigh = true;
            but.OnPress.AddListener(action);
            return but;
        }

        static StandardMenuButton AddBackButton(Transform parent, UnityAction evnt)
        {
            Image backImage = new GameObject("Back").AddComponent<Image>();
            backImage.transform.SetParent(parent, false);
            backImage.rectTransform.pivot = Vector2.zero;
            backImage.transform.localPosition = new Vector3(-240f, 148f, 0f);
            backImage.rectTransform.anchorMin = Vector2.one / 2f;
            backImage.rectTransform.anchorMax = Vector2.one / 2f;
            backImage.rectTransform.sizeDelta = new Vector2(32f, 32f);
            backImage.sprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("BackArrow");

            StandardMenuButton backButton = backImage.gameObject.ConvertToButton<StandardMenuButton>();

            backButton.swapOnHigh = true;
            backButton.unhighlightedSprite = backImage.sprite;
            backButton.highlightedSprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("BackArrowHighlight");
            backButton.transitionOnPress = true;
            backButton.transitionTime = 0.0167f;
            backButton.transitionType = UiTransition.Dither;

            backButton.OnPress.AddListener(evnt);

            return backButton;
        }
    }
}
