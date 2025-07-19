using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PlusLevelStudio.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

namespace PlusLevelStudio.Editor
{
    public class EditorUIFileBrowser : EditorOverlayUIExchangeHandler
    {
        Action<string> onSubmit;
        TextMeshProUGUI textbox;

        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            textbox = transform.Find("Textbox").GetComponent<TextMeshProUGUI>();
        }

        public void Setup(string path, string filename, Action<string> action)
        {
            onSubmit = action;
            textbox.text = EditorController.Instance.currentFileName;
        }

        public override void SendInteractionMessage(string message, object data)
        {
            if (message == "submit")
            {
                onSubmit(textbox.text);
                base.SendInteractionMessage("exit", null);
                return;
            }
            base.SendInteractionMessage(message, data);
        }
    }

    public class EditorUIToolboxHandler : UIExchangeHandler
    {
        public string currentCategory = "tools";
        public int currentMaxPages => Mathf.CeilToInt((float)EditorController.Instance.currentMode.availableTools[currentCategory].Count / hotSlots.Length);
        public int currentPage = 0;
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        public TextMeshProUGUI pageCountText;
        public TextMeshProUGUI[] texts = new TextMeshProUGUI[10];
        public HotSlotScript[] hotSlots = new HotSlotScript[30];

        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            HotSlotScript[] slots = GetComponentsInChildren<HotSlotScript>();
            title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
            description = transform.Find("Description").GetComponent<TextMeshProUGUI>();
            hotSlots = new HotSlotScript[slots.Length];
            foreach (HotSlotScript slot in slots)
            {
                hotSlots[slot.slotIndex] = slot;
            }
            pageCountText = transform.Find("PageNumber").GetComponent<TextMeshProUGUI>();
            // this code is horrible
            texts[0] = transform.Find("Category0").GetComponent<TextMeshProUGUI>();
            texts[1] = transform.Find("Category1").GetComponent<TextMeshProUGUI>();
            texts[2] = transform.Find("Category2").GetComponent<TextMeshProUGUI>();
            texts[3] = transform.Find("Category3").GetComponent<TextMeshProUGUI>();
            texts[4] = transform.Find("Category4").GetComponent<TextMeshProUGUI>();
            texts[5] = transform.Find("Category5").GetComponent<TextMeshProUGUI>();
            texts[6] = transform.Find("Category6").GetComponent<TextMeshProUGUI>();
            texts[7] = transform.Find("Category7").GetComponent<TextMeshProUGUI>();
            texts[8] = transform.Find("Category8").GetComponent<TextMeshProUGUI>();
            texts[9] = transform.Find("Category9").GetComponent<TextMeshProUGUI>();
        }

        public void SetTip(EditorTool tool)
        {
            if (tool == null)
            {
                title.text = "";
                description.text = "";
                return;
            }
            title.text = LocalizationManager.Instance.GetLocalizedText(tool.titleKey);
            description.text = LocalizationManager.Instance.GetLocalizedText(tool.descKey);
        }

        public void Open()
        {
            RefreshPage(currentPage);
            SetTip(null);
            RefreshCategories();
        }

        public void SwitchCategory(int index)
        {
            if (index >= EditorController.Instance.currentMode.categoryOrder.Length)
            {
                return;
            }
            currentPage = 0;
            currentCategory = EditorController.Instance.currentMode.categoryOrder[index];
            RefreshPage(0);
            SetTip(null);
        }

        public void RefreshCategories()
        {
            for (int i = 0; i < texts.Length; i++)
            {
                if (i >= EditorController.Instance.currentMode.categoryOrder.Length)
                {
                    texts[i].text = "";
                    continue;
                }
                texts[i].text = LocalizationManager.Instance.GetLocalizedText("Ed_Category_" + EditorController.Instance.currentMode.categoryOrder[i]);
            }
        }

        public void RefreshPage(int page)
        {
            int startIndex = page * hotSlots.Length;
            List<EditorTool> toolList = EditorController.Instance.currentMode.availableTools[currentCategory];
            for (int i = startIndex; i < startIndex + hotSlots.Length; i++)
            {
                if (i >= toolList.Count)
                {
                    hotSlots[i - startIndex].currentTool = null;
                    continue;
                }
                hotSlots[i - startIndex].currentTool = toolList[i];
            }
            pageCountText.text = (page + 1) + "/" + currentMaxPages;
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            if (message.StartsWith("switchCategory"))
            {
                int categoryIndex = int.Parse(message.Substring(message.Length - 1));
                SwitchCategory(categoryIndex);

            }
            switch (message)
            {
                case "show":
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        if (transform.GetChild(i).gameObject == (data == null ? null : (GameObject)data)) continue;
                        transform.GetChild(i).gameObject.SetActive(true);
                    }
                    break;
                case "hide":
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        if (transform.GetChild(i).gameObject == (data == null ? null : (GameObject)data)) continue;
                        transform.GetChild(i).gameObject.SetActive(false);
                    }
                    break;
                case "exit":
                    gameObject.SetActive(false);
                    break;
                case "tip":
                    SetTip((data == null ? null : (EditorTool)data));
                    break;
                case "nextPage":
                    currentPage = Mathf.Clamp(currentPage + 1, 0, currentMaxPages - 1);
                    RefreshPage(currentPage);
                    break;
                case "prevPage":
                    currentPage = Mathf.Clamp(currentPage - 1, 0, currentMaxPages - 1);
                    RefreshPage(currentPage);
                    break;
            }
        }
    }

    public class EditorUIMainHandler : UIExchangeHandler
    {
        public TextMeshProUGUI gridScaleTextBox;
        public TextMeshProUGUI angleSnapTextBox;
        public List<GameObject> translationSettings = new List<GameObject>();
        bool settingsHidden = true;

        public override bool GetStateBoolean(string key)
        {
            switch (key)
            {
                case "worldSpace":
                    return EditorController.Instance.selector.moveHandles.worldSpace;
                case "localSpace":
                    return !EditorController.Instance.selector.moveHandles.worldSpace;
                case "moveEnabled":
                    return EditorController.Instance.selector.moveHandles.moveEnabled;
                case "rotateEnabled":
                    return EditorController.Instance.selector.moveHandles.rotateEnabled;
            }
            return false;
        }

        public override void OnElementsCreated()
        {
            HotSlotScript[] foundSlotScripts = transform.GetComponentsInChildren<HotSlotScript>();
            for (int i = 0; i < foundSlotScripts.Length; i++)
            {
                EditorController.Instance.hotSlots[foundSlotScripts[i].slotIndex] = foundSlotScripts[i];
            }
            gridScaleTextBox = transform.Find("MoveGridSizeBox").GetComponentInChildren<TextMeshProUGUI>();
            angleSnapTextBox = transform.Find("AngleSnapBox").GetComponentInChildren<TextMeshProUGUI>();
            translationSettings.Add(transform.Find("GridGauge").gameObject);
            translationSettings.Add(transform.Find("MoveGridSizeText").gameObject);
            translationSettings.Add(transform.Find("MoveGridSizeBox").gameObject);
            translationSettings.Add(transform.Find("AngleSnapText").gameObject);
            translationSettings.Add(transform.Find("AngleSnapBox").gameObject);
            translationSettings.Add(transform.Find("WorldSpaceButton").gameObject);
            translationSettings.Add(transform.Find("LocalSpaceButton").gameObject);
            translationSettings.Add(transform.Find("MoveVisibleButton").gameObject);
            translationSettings.Add(transform.Find("RotateVisibleButton").gameObject);
            translationSettings.ForEach(x => x.SetActive(false));
            // EditorController exists by this point
            gridScaleTextBox.text = EditorController.Instance.gridSnap.ToString();
            angleSnapTextBox.text = EditorController.Instance.angleSnap.ToString();
        }

        public override void SendInteractionMessage(string message, object data)
        {
            switch (message)
            {
                case "exit":
                    Destroy(Singleton<EditorController>.Instance.camera.gameObject);
                    Singleton<InputManager>.Instance.ActivateActionSet("Interface");
                    Singleton<GlobalCam>.Instance.ChangeType(CameraRenderType.Base);
                    SceneManager.LoadScene("MainMenu");
                    break;
                case "play":
                    EditorController.Instance.CompileAndPlay();
                    break;
                case "load":
                    EditorController.Instance.CreateUIFileBrowser(LevelStudioPlugin.levelFilePath, "ebpl", (string typedName) =>
                    {
                        BinaryReader reader = new BinaryReader(new FileStream(Path.Combine(LevelStudioPlugin.levelFilePath, typedName + ".ebpl"), FileMode.Open, FileAccess.Read));
                        EditorController.Instance.LoadEditorLevel(EditorLevelData.ReadFrom(reader), true);
                        reader.Close();
                    });
                    break;
                case "save":
                    EditorController.Instance.CreateUIFileBrowser(LevelStudioPlugin.levelFilePath, "ebpl", (string typedName) =>
                    {
                        Directory.CreateDirectory(LevelStudioPlugin.levelFilePath);
                        BinaryWriter writer = new BinaryWriter(new FileStream(Path.Combine(LevelStudioPlugin.levelFilePath, typedName + ".ebpl"), FileMode.Create, FileAccess.Write));
                        EditorController.Instance.levelData.Write(writer);
                        writer.Close();
                    });
                    break;
                case "undo":
                    EditorController.Instance.PopUndo();
                    break;
                case "toolbox":
                    EditorController.Instance.SwitchToTool(null);
                    EditorController.Instance.uiObjects[1].SetActive(true);
                    EditorController.Instance.uiObjects[1].GetComponent<EditorUIToolboxHandler>().Open();
                    break;
                case "changeGridScale":
                    if (float.TryParse((string)data, out float result))
                    {
                        EditorController.Instance.gridSnap = result;
                    }
                    gridScaleTextBox.text = EditorController.Instance.gridSnap.ToString();
                    break;
                case "changeAngleSnap":
                    if (float.TryParse((string)data, out float angleResult))
                    {
                        EditorController.Instance.angleSnap = angleResult;
                    }
                    angleSnapTextBox.text = EditorController.Instance.angleSnap.ToString();
                    break;
                case "showTranslateSettings":
                    if (settingsHidden)
                    {
                        translationSettings.ForEach(x => x.SetActive(true));
                    }
                    settingsHidden = false;
                    break;
                case "hideTranslateSettings":
                    if (!settingsHidden)
                    {
                        translationSettings.ForEach(x => x.SetActive(false));
                    }
                    settingsHidden = true;
                    break;
                case "switchToWorld":
                    EditorController.Instance.selector.moveHandles.worldSpace = true;
                    break;
                case "switchToLocal":
                    EditorController.Instance.selector.moveHandles.worldSpace = false;
                    break;
                case "toggleMove":
                    EditorController.Instance.selector.moveHandles.moveEnabled = !EditorController.Instance.selector.moveHandles.moveEnabled;
                    EditorController.Instance.selector.moveHandles.SetArrows(EditorController.Instance.selector.moveHandles.enabledMoveAxis);
                    break;
                case "toggleRotate":
                    EditorController.Instance.selector.moveHandles.rotateEnabled = !EditorController.Instance.selector.moveHandles.rotateEnabled;
                    EditorController.Instance.selector.moveHandles.SetRings(EditorController.Instance.selector.moveHandles.enabledRotateAxis);
                    break;
            }
        }
    }
}
