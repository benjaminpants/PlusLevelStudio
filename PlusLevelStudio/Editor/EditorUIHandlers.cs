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
        Func<string, bool> onSubmit;
        TextMeshProUGUI textbox;

        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            textbox = transform.Find("Textbox").GetComponent<TextMeshProUGUI>();
        }

        public void Setup(string path, string extension, Func<string, bool> action)
        {
            onSubmit = action;
            textbox.text = "MyFirstLevel";
            if (!string.IsNullOrEmpty(EditorController.Instance.currentFileName))
            {
                textbox.text = EditorController.Instance.currentFileName;
            }
        }

        public override void SendInteractionMessage(string message, object data)
        {
            if (message == "submit")
            {
                if (onSubmit(textbox.text))
                {
                    EditorController.Instance.currentFileName = textbox.text;
                }
                base.SendInteractionMessage("exit", null);
                return;
            }
            base.SendInteractionMessage(message, data);
        }
    }

    public class EditorUIGlobalSettingsHandler : UIExchangeHandler
    {
        public bool somethingChanged = false;
        public GlobalSettingsUIExchangeHandler[] pages;
        public string[] pageKeys;
        public TextMeshProUGUI titleText;
        int currentPage = 0;

        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            titleText = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        }

        public void SwitchToPage(int page)
        {
            pages[currentPage].gameObject.SetActive(false);
            currentPage = page;
            pages[page].gameObject.SetActive(true);
            titleText.text = LocalizationManager.Instance.GetLocalizedText(pageKeys[page]);
            pages[page].Refresh();
        }

        public void Open()
        {
            EditorController.Instance.HoldUndo();
            pages[currentPage].Refresh();
            titleText.text = LocalizationManager.Instance.GetLocalizedText(pageKeys[currentPage]);
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "prevPage":
                    SwitchToPage(Mathf.Max(currentPage - 1,0));
                    break;
                case "nextPage":
                    SwitchToPage(Mathf.Min(currentPage + 1, pages.Length - 1));
                    break;
                case "exit":
                    if (somethingChanged)
                    {
                        EditorController.Instance.AddHeldUndo();
                    }
                    else
                    {
                        EditorController.Instance.CancelHeldUndo();
                    }
                    EditorController.Instance.SetChannelsMuted(false);
                    gameObject.SetActive(false);
                    break;
            }
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

        public void PlayLevel()
        {
            EditorController.Instance.PlayLevel();
        }

        public void CloseEditor()
        {
            Destroy(Singleton<EditorController>.Instance.camera.gameObject);
            Singleton<InputManager>.Instance.ActivateActionSet("Interface");
            Singleton<GlobalCam>.Instance.ChangeType(CameraRenderType.Base);
            SceneManager.LoadScene("MainMenu");
        }

        public override void SendInteractionMessage(string message, object data)
        {
            switch (message)
            {
                case "exit":
                    if (EditorController.Instance.hasUnsavedChanges)
                    {
                        EditorController.Instance.CreateUIPopup(LocalizationManager.Instance.GetLocalizedText("Ed_Menu_UnsavedChangesExit"), CloseEditor, () => { });
                    }
                    else
                    {
                        CloseEditor();
                    }
                    break;
                case "play":
                    if (EditorController.Instance.hasUnsavedChanges)
                    {
                        EditorController.Instance.CreateUIPopup(LocalizationManager.Instance.GetLocalizedText("Ed_Menu_UnsavedChanges"), () =>
                        {
                            SendInteractionMessage("saveAndPlay", null);
                        }, PlayLevel);
                    }
                    else
                    {
                        PlayLevel();
                    }
                    break;
                case "load":
                    EditorController.Instance.CreateUIFileBrowser(LevelStudioPlugin.levelFilePath, "ebpl", (string typedName) =>
                    {
                        return EditorController.Instance.LoadEditorLevelFromFile(Path.Combine(LevelStudioPlugin.levelFilePath, typedName + ".ebpl"));
                    });
                    break;
                case "save":
                    EditorController.Instance.CreateUIFileBrowser(LevelStudioPlugin.levelFilePath, "ebpl", (string typedName) =>
                    {
                        EditorController.Instance.SaveEditorLevelToFile(Path.Combine(LevelStudioPlugin.levelFilePath, typedName + ".ebpl"));
                        return true;
                    });
                    break;
                case "saveAndPlay":
                    EditorController.Instance.CreateUIFileBrowser(LevelStudioPlugin.levelFilePath, "ebpl", (string typedName) =>
                    {
                        EditorController.Instance.SaveEditorLevelToFile(Path.Combine(LevelStudioPlugin.levelFilePath, typedName + ".ebpl"));
                        PlayLevel();
                        return true;
                    });
                    break;
                case "saveAndExport":
                    EditorController.Instance.CreateUIFileBrowser(LevelStudioPlugin.levelFilePath, "ebpl", (string typedName) =>
                    {
                        EditorController.Instance.SaveEditorLevelToFile(Path.Combine(LevelStudioPlugin.levelFilePath, typedName + ".ebpl"));
                        EditorController.Instance.Export();
                        return true;
                    });
                    break;
                case "export":
                    EditorController.Instance.ExportWithChecks();
                    break;
                case "undo":
                    EditorController.Instance.PopUndo();
                    break;
                case "toolbox":
                    EditorController.Instance.SwitchToTool(null);
                    EditorController.Instance.uiObjects[1].SetActive(true);
                    EditorController.Instance.uiObjects[1].GetComponent<EditorUIToolboxHandler>().Open();
                    break;
                case "globalSettings":
                    EditorController.Instance.SwitchToTool(null);
                    EditorController.Instance.uiObjects[2].SetActive(true);
                    EditorController.Instance.uiObjects[2].GetComponent<EditorUIGlobalSettingsHandler>().Open();
                    EditorController.Instance.SetChannelsMuted(true);
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

    public abstract class GlobalSettingsUIExchangeHandler : UIExchangeHandler
    {
        public abstract void Refresh();
        public EditorUIGlobalSettingsHandler handler;
    }

    public class DummyGlobalSettingsHandler : GlobalSettingsUIExchangeHandler
    {
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            
        }

        public override void Refresh()
        {
            
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            
        }
    }
}
