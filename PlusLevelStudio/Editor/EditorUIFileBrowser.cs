using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class EditorUIFileBrowser : EditorOverlayUIExchangeHandler
    {
        struct BrowserFile : IComparable
        {
            public string name;
            public DateTime lastModified;

            public BrowserFile(string name, DateTime lastModified)
            {
                this.name = name;
                this.lastModified = lastModified;
            }

            public int CompareTo(object obj)
            {
                if (obj is BrowserFile)
                {
                    return ((BrowserFile)obj).lastModified.CompareTo(lastModified);
                }
                return obj.ToString().CompareTo(name);
            }
        }
        Func<string, bool> onSubmit;
        TextMeshProUGUI textbox;
        TextMeshProUGUI selectFileText;
        TextMeshProUGUI[] fileTexts = new TextMeshProUGUI[11];
        string path;
        string extension;
        bool allowNonExistantFiles = false;
        int offset = 0;
        List<BrowserFile> files = new List<BrowserFile>();

        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            textbox = transform.Find("Textbox").GetComponent<TextMeshProUGUI>();
            selectFileText = transform.Find("SelectFile").GetComponent<TextMeshProUGUI>();
            for (int i = 0; i < fileTexts.Length; i++)
            {
                fileTexts[i] = transform.Find("Entry" + i).GetComponent<TextMeshProUGUI>();
            }
        }

        public void Setup(string path, string extension, string startingFile, bool allowNonExistantFiles, Func<string, bool> action)
        {
            this.path = path;
            this.extension = extension;
            onSubmit = action;
            textbox.text = "MyFirstLevel";
            selectFileText.text = String.Format(LocalizationManager.Instance.GetLocalizedText("Ed_Menu_SelectFile"), extension);
            if (!string.IsNullOrEmpty(startingFile))
            {
                textbox.text = startingFile;
            }
            this.allowNonExistantFiles = allowNonExistantFiles;
            UpdateFiles();
        }

        public void UpdateFiles()
        {
            files.Clear();
            string[] paths = Directory.GetFiles(path, "*." + extension);
            for (int i = 0; i < paths.Length; i++)
            {
                files.Add(new BrowserFile(Path.GetFileNameWithoutExtension(paths[i]), File.GetLastWriteTime(paths[i])));
            }
            files.Sort();
            UpdateTexts(0);
        }

        public void UpdateTexts(int addend)
        {
            offset = Mathf.Clamp(offset + addend, 0, Mathf.Max(0, files.Count - fileTexts.Length));
            for (int i = 0; i < fileTexts.Length; i++)
            {
                int offIndex = offset + i;
                if (offIndex >= files.Count)
                {
                    fileTexts[i].text = "";
                }
                else
                {
                    fileTexts[i].text = files[offset + i].name + " - " + files[offset + i].lastModified.ToShortDateString();
                }
            }
        }

        public override void SendInteractionMessage(string message, object data)
        {
            if (message.StartsWith("pressed"))
            {
                int index = int.Parse(message.Replace("pressed", ""));
                if (index + offset < files.Count)
                {
                    textbox.text = files[index + offset].name;
                }
            }
            switch (message)
            {
                case "submit":
                    string generatedPath = Path.Combine(path, textbox.text + "." + extension);
                    if ((!allowNonExistantFiles) && (!File.Exists(generatedPath)))
                    {
                        EditorController.Instance.TriggerError("Ed_Error_FileNoExist");
                        return;
                    }
                    if (onSubmit(generatedPath))
                    {
                        base.SendInteractionMessage("exit", null);
                    }
                    break;
                case "up":
                    UpdateTexts(-fileTexts.Length);
                    break;
                case "down":
                    UpdateTexts(fileTexts.Length);
                    break;
                case "openFolder":
                    Application.OpenURL("file:///" + path);
                    break;
            }
            base.SendInteractionMessage(message, data);
        }
    }
}
