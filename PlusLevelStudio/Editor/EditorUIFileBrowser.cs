using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class EditorUIFileBrowser : EditorOverlayUIExchangeHandler
    {
        enum FileType
        {
            File,
            Folder,
            Special
        }

        struct BrowserFile : IComparable
        {
            public string name;
            public DateTime lastModified;
            public FileType type;

            public BrowserFile(string name, DateTime lastModified, FileType type)
            {
                this.name = name;
                this.lastModified = lastModified;
                this.type = type;
            }

            public int CompareTo(object obj)
            {
                if (obj is BrowserFile)
                {
                    if (((BrowserFile)obj).type == FileType.Special) return 1;
                    if (((BrowserFile)obj).type == FileType.Folder && (type == FileType.File)) return 1;
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
        string basePath;
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
            this.basePath = path;
            this.extension = extension;
            onSubmit = action;
            textbox.text = "MyFirstLevel";
            selectFileText.text = String.Format(LocalizationManager.Instance.GetLocalizedText("Ed_Menu_SelectFile"), extension);
            if (!string.IsNullOrEmpty(startingFile))
            {
                string[] paths = startingFile.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                if (paths.Length == 0)
                {
                    textbox.text = startingFile;
                }
                else
                {
                    textbox.text = paths[paths.Length - 1]; // last one is the actual filename
                    for (int i = 0; i < paths.Length - 1; i++) // dont want to go to the last one
                    {
                        this.path = Path.Combine(this.path, paths[i] + Path.DirectorySeparatorChar);
                    }
                }    
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
                files.Add(new BrowserFile(Path.GetFileNameWithoutExtension(paths[i]), File.GetLastWriteTime(paths[i]), FileType.File));
            }
            string[] directories = Directory.GetDirectories(path);
            for (int i = 0; i < directories.Length; i++)
            {
                files.Add(new BrowserFile(Path.GetFileNameWithoutExtension(directories[i]), Directory.GetLastWriteTime(directories[i]), FileType.Folder));
            }
            if (!PathHelpers.PathEquals(path,basePath))
            {
                files.Add(new BrowserFile("../", DateTime.Now, FileType.Special));
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
                    BrowserFile f = files[offset + i];
                    switch (f.type)
                    {
                        case FileType.File:
                            fileTexts[i].text = files[offset + i].name + " - " + files[offset + i].lastModified.ToShortDateString();
                            break;
                        case FileType.Folder:
                            fileTexts[i].text = files[offset + i].name + "/ - " + files[offset + i].lastModified.ToShortDateString();
                            break;
                        case FileType.Special:
                            fileTexts[i].text = files[offset + i].name;
                            break;
                    }
                }
            }
        }

        public void HandleSpecialFileInteraction(string name)
        {
            if (name == "../")
            {
                path = Directory.GetParent(PathHelpers.PathNormalize(path)).FullName;
                UpdateFiles();
            }
        }

        public override void SendInteractionMessage(string message, object data)
        {
            if (message.StartsWith("pressed"))
            {
                int index = int.Parse(message.Replace("pressed", ""));
                if (index + offset < files.Count)
                {
                    string folderOff = PathHelpers.GetRelativePath(basePath, path);
                    BrowserFile file = files[index + offset];
                    switch (file.type)
                    {
                        case FileType.File:
                            textbox.text = file.name;
                            break;
                        case FileType.Folder:
                            path = Path.Combine(path, file.name);
                            UpdateFiles();
                            break;
                        case FileType.Special:
                            HandleSpecialFileInteraction(file.name);
                            break;
                    }
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
                        SendInteractionMessage("exit", null);
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

    public class GenericUIFileBrowser : EditorUIFileBrowser
    {
        public override void SendInteractionMessage(string message, object data)
        {
            if (message == "exit")
            {
                Destroy(gameObject);
                return;
            }
            base.SendInteractionMessage(message, data);
        }
    }
}
