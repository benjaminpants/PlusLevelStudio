using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.UI;
using PlusLevelStudio.Editor;
using PlusLevelStudio.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace PlusLevelStudio.Menus
{

    public class EditorPlayScreenUIHandler : UIExchangeHandler
    {
        public EditorModeSelectionMenu menu;
        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "exit":
                    menu.playOrEditParent.SetActive(true);
                    menu.playParent.SetActive(false);
                    break;
                case "pageUp":
                    ChangePage(-1);
                    break;
                case "pageDown":
                    ChangePage(1);
                    break;
                case "openFolder":
                    Application.OpenURL("file:///" + LevelStudioPlugin.playableLevelPath);
                    break;
            }
        }

        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i] = transform.Find("EditorPlay" + i).GetComponent<EditorPlayLevelButton>();
            }
            pageDisplay = transform.Find("PageDisplay").GetComponent<TextMeshProUGUI>();
            upButton = transform.Find("PageUp").GetComponent<StandardMenuButton>();
            downButton = transform.Find("PageDown").GetComponent<StandardMenuButton>();
            bigText = transform.Find("BigText").GetComponent<TextMeshProUGUI>();
            SetupButtons();
        }


        public List<PlayableEditorLevel> playableLevels = new List<PlayableEditorLevel>();
        public Dictionary<PlayableEditorLevel, Texture2D> playableLevelThumbnails = new Dictionary<PlayableEditorLevel, Texture2D>();
        public Dictionary<PlayableEditorLevel, string> playableToPath = new Dictionary<PlayableEditorLevel, string>();
        public EditorPlayLevelButton[] buttons = new EditorPlayLevelButton[3];
        public TextMeshProUGUI pageDisplay;
        public StandardMenuButton upButton;
        public StandardMenuButton downButton;
        public TextMeshProUGUI bigText;
        FileSystemWatcher watcher;
        public int currentPage;
        public int maxPage => Mathf.CeilToInt(playableLevels.Count / (float)buttons.Length) - 1;

        public void ChangePage(int amount)
        {
            currentPage = Mathf.Clamp(currentPage + amount, 0, maxPage);
            upButton.gameObject.SetActive(currentPage != 0);
            downButton.gameObject.SetActive(currentPage != maxPage);
            pageDisplay.text = (currentPage + 1) + "/" + (maxPage + 1);
            UpdateButtons();
        }

        public void SetupButtons()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;
                buttons[i].playButton.OnPress.AddListener(() => PlayLevel(index));
                buttons[i].discardButton.OnPress.AddListener(() => DiscardLevel(index));
            }
        }

        public void DiscardLevel(int buttonIndex)
        {
            int startIndex = (currentPage * buttons.Length);
            File.Delete(playableToPath[playableLevels[startIndex + buttonIndex]]);
        }

        public void PlayLevel(int buttonIndex)
        {
            int startIndex = (currentPage * buttons.Length);
            PlayableEditorLevel level = playableLevels[startIndex + buttonIndex];
            try
            {
                EditorPlayModeManager.LoadLevel(level, LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].supportsCampaigns ? 2 : 0, false);
            }
            catch (Exception e)
            {
                if (Singleton<EditorPlayModeManager>.Instance != null)
                {
                    Singleton<EditorPlayModeManager>.Instance.CleanupEverything();
                    Destroy(Singleton<EditorPlayModeManager>.Instance.gameObject);
                }
                GenericPopupExchangeHandler handler = UIBuilder.BuildUIFromFile<GenericPopupExchangeHandler>((RectTransform)transform, "ErrorPop", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "1ChoicePopup.json"));

                handler.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = string.Format(LocalizationManager.Instance.GetLocalizedText("Ed_Exception_FileLoad"), e.Message);
            }
        }

        bool shouldRefresh = false;

        void FileChangedAtAll(object sender, FileSystemEventArgs e)
        {
            shouldRefresh = true;
        }

        // these freeze the game...
        // TODO: why?
        /*
        void FileCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created) return;
            AddFromFile(e.FullPath);
            ChangePage(0);
        }

        void FileDeleted(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Deleted) return;
            int toDeleteIndex = playableLevels.FindIndex(x => x.filePath == e.FullPath);
            if (toDeleteIndex == -1)
            {
                UnityEngine.Debug.LogWarning("Tried to handle deletion for file not in list: " + e.FullPath);
                return;
            }
            playableLevels.RemoveAt(toDeleteIndex);
            ChangePage(0);
        }

        void FileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed) return;
            int toDeleteIndex = playableLevels.FindIndex(x => x.filePath == e.FullPath);
            if (toDeleteIndex == -1)
            {
                UnityEngine.Debug.LogWarning("Tried to handle change for file not in list: " + e.FullPath);
                return;
            }
            playableLevels.RemoveAt(toDeleteIndex);
            AddFromFile(e.FullPath, toDeleteIndex);
            ChangePage(0);
        }

        void FileRenamed(object sender, RenamedEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Renamed) return;
            int toChangeIndex = playableLevels.FindIndex(x => x.filePath == e.OldFullPath);
            if (toChangeIndex == -1)
            {
                UnityEngine.Debug.LogWarning("Tried to handle change for file not in list: " + e.FullPath);
                return;
            }
            playableLevels[toChangeIndex].filePath = e.FullPath;
            ChangePage(0);
        }*/

        void Update()
        {
            if (shouldRefresh)
            {
                UnityEngine.Debug.Log("Refreshing..."); // if you remove this debug log everything breaks down and i dont know why.
                UpdateFromFolder();
                shouldRefresh = false;
            }
        }

        public void SetFileWatcherStatus(bool active)
        {
            if (watcher == null)
            {
                if (active)
                {
                    watcher = new FileSystemWatcher(LevelStudioPlugin.playableLevelPath, "*.pbpl");
                    watcher.Created += FileChangedAtAll;
                    watcher.Deleted += FileChangedAtAll;
                    watcher.Changed += FileChangedAtAll;
                    watcher.Renamed += FileChangedAtAll;
                }
                else
                {
                    return;
                }
            }
            watcher.EnableRaisingEvents = active;
        }

        protected void AddFromFile(string path, int index = -1)
        {
            BinaryReader reader = new BinaryReader(System.IO.File.OpenRead(path));
            PlayableEditorLevel level = PlayableEditorLevel.Read(reader);
            level.filePath = path;
            if (index == -1)
            {
                playableLevels.Add(level);
            }
            else
            {
                playableLevels.Insert(index, level);
            }
            if (level.meta.contentPackage.thumbnailEntry != null)
            {
                Texture2D thumbTex = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
                try
                {
                    ImageConversion.LoadImage(thumbTex, level.meta.contentPackage.thumbnailEntry.data);
                    thumbTex.filterMode = FilterMode.Point;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Invalid thumbnail data " + e.ToString());
                    UnityEngine.Object.Destroy(thumbTex);
                    thumbTex = null;
                }
                thumbTex.name = Path.GetFileNameWithoutExtension(path) + "_Thumb";
                playableLevelThumbnails.Add(level, thumbTex);
            }
            playableToPath.Add(level, path);
            reader.Close();
        }

        public void UpdateFromFolder()
        {
            bigText.gameObject.SetActive(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].gameObject.SetActive(false);
            }
            bigText.text = LocalizationManager.Instance.GetLocalizedText("Ed_Menu_LoadingLevels");
            foreach (KeyValuePair<PlayableEditorLevel, Texture2D> item in playableLevelThumbnails)
            {
                Destroy(item.Value);
            }
            playableLevelThumbnails.Clear();
            playableLevels.Clear();
            playableToPath.Clear();
            Directory.CreateDirectory(LevelStudioPlugin.playableLevelPath);
            string[] files = Directory.GetFiles(LevelStudioPlugin.playableLevelPath, "*.pbpl");
            StartCoroutine(LoadEnumerator(files));
        }

        IEnumerator LoadEnumerator(string[] files)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            long msSinceLastYield = 0;
            for (int i = 0; i < files.Length; i++)
            {
                AddFromFile(files[i]);
                if ((stopwatch.ElapsedMilliseconds - msSinceLastYield) > 100)
                {
                    msSinceLastYield = stopwatch.ElapsedMilliseconds;
                    yield return null;
                }
            }
            stopwatch.Stop();
            //UnityEngine.Debug.Log("It took: " + stopwatch.Elapsed.Milliseconds + " ms to read " + files.Length + " files!");
            playableLevels.Sort((a, b) => (a.meta.name.CompareTo(b.meta.name)));
            ChangePage(0);
            yield break;
        }

        public void UpdateButtons()
        {
            bigText.gameObject.SetActive(false);
            int startIndex = (currentPage * buttons.Length);
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].gameObject.SetActive(false);
            }
            if (playableLevels.Count == 0)
            {
                upButton.gameObject.SetActive(false);
                downButton.gameObject.SetActive(false);
                bigText.gameObject.SetActive(true);
                bigText.text = LocalizationManager.Instance.GetLocalizedText("Ed_Menu_NoLevels");
                return;
            }
            for (int i = startIndex; i < Mathf.Min(startIndex + buttons.Length,playableLevels.Count); i++)
            {
                int index = i - startIndex;
                buttons[index].gameObject.SetActive(true);
                buttons[index].UpdateDisplay(playableLevels[i], playableLevelThumbnails[playableLevels[i]]);
            }
        }
    }

    public class EditorPlayLevelButton : MonoBehaviour
    {
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI authorText;
        public TextMeshProUGUI modeText;
        public RawImage thumbnail;
        public StandardMenuButton playButton;
        public StandardMenuButton discardButton;

        public void UpdateDisplay(PlayableEditorLevel level, Texture2D thumb)
        {
            titleText.text = level.meta.name;
            authorText.text = string.Format(LocalizationManager.Instance.GetLocalizedText("Ed_Menu_LevelBy"), level.meta.author);
            modeText.text = LocalizationManager.Instance.GetLocalizedText(LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].nameKey);
            if (thumb == null)
            {
                thumb = LevelStudioPlugin.Instance.assetMan.Get<Texture2D>("IconMissing");
            }
            thumbnail.texture = thumb;
        }
    }
}
