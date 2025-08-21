using MTM101BaldAPI;
using MTM101BaldAPI.UI;
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
    public partial class EditorModeSelectionMenu : MonoBehaviour
    {
        internal static void CreatePlayModeMenu(EditorModeSelectionMenu menu)
        {
            TextMeshProUGUI BIGText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans36, "No Levels Found", menu.playParent.transform, Vector3.zero);
            BIGText.rectTransform.sizeDelta = new Vector2(200f, 64f);
            BIGText.alignment = TextAlignmentOptions.Center;
            menu.playScreenManager = menu.playParent.AddComponent<EditorPlayScreenManager>();
            menu.playScreenManager.buttons[0] = CreateLevelPlay(menu.playParent.transform, Vector2.up * 70f);
            menu.playScreenManager.buttons[1] = CreateLevelPlay(menu.playParent.transform, Vector2.zero);
            menu.playScreenManager.buttons[2] = CreateLevelPlay(menu.playParent.transform, Vector2.down * 70f);
            menu.playScreenManager.SetupButtons();

            // create page up and page down buttons
            Vector2 center = Vector2.one / 2f;

            GameObject pageUpBase = new GameObject("PageUp");
            pageUpBase.transform.SetParent(menu.playParent.transform, false);
            pageUpBase.transform.localEulerAngles = new Vector3(0f,0f,-90f);
            Image pageUpImage = pageUpBase.AddComponent<Image>();
            pageUpImage.sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("MenuArrowLeft");
            pageUpImage.rectTransform.anchorMin = center;
            pageUpImage.rectTransform.anchorMax = center;
            pageUpImage.rectTransform.pivot = center;
            pageUpImage.rectTransform.sizeDelta = Vector2.one * 32;
            pageUpImage.rectTransform.anchoredPosition = new Vector2(0f, 120f);
            StandardMenuButton pageUpButton = pageUpImage.gameObject.ConvertToButton<StandardMenuButton>();
            pageUpButton.unhighlightedSprite = pageUpImage.sprite;
            pageUpButton.swapOnHigh = true;
            pageUpButton.highlightedSprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("MenuArrowLeftHigh");

            GameObject pageDownBase = new GameObject("PageDown");
            pageDownBase.transform.SetParent(menu.playParent.transform, false);
            pageDownBase.transform.localEulerAngles = new Vector3(0f, 0f, -90f);
            Image pageDownImage = pageDownBase.AddComponent<Image>();
            pageDownImage.sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("MenuArrowRight");
            pageDownImage.rectTransform.anchorMin = center;
            pageDownImage.rectTransform.anchorMax = center;
            pageDownImage.rectTransform.pivot = center;
            pageDownImage.rectTransform.sizeDelta = Vector2.one * 32;
            pageDownImage.rectTransform.anchoredPosition = new Vector2(0f, -120f);
            StandardMenuButton pageDownButton = pageDownImage.gameObject.ConvertToButton<StandardMenuButton>();
            pageDownButton.unhighlightedSprite = pageDownImage.sprite;
            pageDownButton.swapOnHigh = true;
            pageDownButton.highlightedSprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("MenuArrowRightHigh");

            pageUpButton.OnPress.AddListener(() => menu.playScreenManager.ChangePage(-1));
            pageDownButton.OnPress.AddListener(() => menu.playScreenManager.ChangePage(1));

            menu.playScreenManager.upButton = pageUpButton;
            menu.playScreenManager.downButton = pageDownButton;

            menu.playScreenManager.pageDisplay = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans18, "0/100", menu.playParent.transform, Vector3.zero);
            menu.playScreenManager.pageDisplay.name = "PageDisplay";
            menu.playScreenManager.pageDisplay.rectTransform.sizeDelta = new Vector2(72f, 20f);
            menu.playScreenManager.pageDisplay.alignment = TextAlignmentOptions.Right;
            menu.playScreenManager.pageDisplay.rectTransform.anchoredPosition = new Vector2(96f,120f);
            menu.playScreenManager.upButton = pageUpButton;
            menu.playScreenManager.downButton = pageDownButton;

            menu.playScreenManager.bigText = BIGText;

            /*TextMeshProUGUI openFolderButton = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans18, "Open Folder", menu.playParent.transform, Vector3.zero);
            openFolderButton.name = "OpenFolderButton";
            openFolderButton.rectTransform.sizeDelta = new Vector2(72f, 20f);
            openFolderButton.alignment = TextAlignmentOptions.Left;
            openFolderButton.rectTransform.anchoredPosition = new Vector2(-96f, -120f);
            openFolderButton.gameObject.ConvertToButton<StandardMenuButton>();*/
        }

        internal static EditorPlayLevelButton CreateLevelPlay(Transform parent, Vector2 anchoredPosition)
        {
            Vector2 center = Vector2.one / 2f;
            GameObject playBase = new GameObject("EditorPlay");
            playBase.transform.SetParent(parent, false);
            RectTransform rectTransform = playBase.AddComponent<RectTransform>();
            rectTransform.anchorMin = center;
            rectTransform.anchorMax = center;
            rectTransform.pivot = center;
            rectTransform.sizeDelta = new Vector2(284f,68f);
            rectTransform.anchoredPosition = anchoredPosition;
            Image backImage = playBase.AddComponent<Image>();
            //backImage.color = new Color(0f,0f,0f, 0.5f);
            backImage.sprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("PlayLevelBorder");
            TextMeshProUGUI titleText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.BoldComicSans12, "My Awesome Level!", playBase.transform, Vector3.zero, false);
            titleText.name = "Title";
            titleText.rectTransform.anchorMin = center;
            titleText.rectTransform.anchorMax = center;
            titleText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            titleText.rectTransform.sizeDelta = new Vector2(164f,18f);
            titleText.rectTransform.anchoredPosition = new Vector2(14f,24f);
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.richText = false;
            titleText.overflowMode = TextOverflowModes.Ellipsis;

            TextMeshProUGUI authorText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans12, "By MissingTextureMan101", playBase.transform, Vector3.zero, false);
            authorText.name = "Author";
            authorText.rectTransform.anchorMin = center;
            authorText.rectTransform.anchorMax = center;
            authorText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            authorText.rectTransform.sizeDelta = new Vector2(136f, 16f);
            authorText.rectTransform.anchoredPosition = new Vector2(0f, 6f);
            authorText.alignment = TextAlignmentOptions.Left;
            authorText.overflowMode = TextOverflowModes.Ellipsis;
            authorText.richText = false;

            TextMeshProUGUI modeText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans12, "Hide & Seek", playBase.transform, Vector3.zero, false);
            modeText.name = "ModeTitle";
            modeText.rectTransform.anchorMin = center;
            modeText.rectTransform.anchorMax = center;
            modeText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            modeText.rectTransform.sizeDelta = new Vector2(136f, 16f);
            modeText.rectTransform.anchoredPosition = new Vector2(0f, -24f);
            modeText.alignment = TextAlignmentOptions.Left;


            GameObject playButtonBase = new GameObject("PlayButton");
            playButtonBase.transform.SetParent(playBase.transform, false);
            Image playButtonImage = playButtonBase.AddComponent<Image>();
            playButtonImage.sprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("MPlayButton");
            playButtonImage.rectTransform.anchorMin = Vector2.one;
            playButtonImage.rectTransform.anchorMax = Vector2.one;
            playButtonImage.rectTransform.pivot = Vector2.one;
            playButtonImage.rectTransform.sizeDelta = Vector2.one * 32;
            playButtonImage.rectTransform.anchoredPosition = new Vector2(-2f, -2f);
            StandardMenuButton playButton = playButtonImage.gameObject.ConvertToButton<StandardMenuButton>();
            playButton.unhighlightedSprite = playButtonImage.sprite;
            playButton.swapOnHigh = true;
            playButton.highlightedSprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("MPlayButtonHover");
            GameObject discardButtonBase = new GameObject("DiscardButton");
            discardButtonBase.transform.SetParent(playBase.transform, false);
            Image discardButtonImage = discardButtonBase.AddComponent<Image>();
            discardButtonImage.sprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("MDiscardButton");
            discardButtonImage.rectTransform.anchorMin = Vector2.right;
            discardButtonImage.rectTransform.anchorMax = Vector2.right;
            discardButtonImage.rectTransform.pivot = Vector2.right;
            discardButtonImage.rectTransform.sizeDelta = Vector2.one * 32;
            discardButtonImage.rectTransform.anchoredPosition = new Vector2(-2f, -2f);
            StandardMenuButton discardButton = discardButtonImage.gameObject.ConvertToButton<StandardMenuButton>();
            discardButton.unhighlightedSprite = discardButtonImage.sprite;
            discardButton.swapOnHigh = true;
            discardButton.highlightedSprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("MDiscardButtonHover");

            GameObject thumbBase = new GameObject("Thumbnail");
            thumbBase.transform.SetParent(playBase.transform, false);
            RawImage thumbImage = thumbBase.AddComponent<RawImage>();
            thumbImage.rectTransform.anchorMin = Vector2.zero;
            thumbImage.rectTransform.anchorMax = Vector2.zero;
            thumbImage.rectTransform.pivot = Vector2.zero;
            thumbImage.rectTransform.sizeDelta = Vector2.one * 64;
            thumbImage.rectTransform.anchoredPosition = new Vector2(2f,2f);
            EditorPlayLevelButton button = playBase.AddComponent<EditorPlayLevelButton>();
            button.titleText = titleText;
            button.authorText = authorText;
            button.modeText = modeText;
            button.thumbnail = thumbImage;
            button.playButton = playButton;
            button.discardButton = discardButton;
            return button;
        }
    }

    public class EditorPlayScreenManager : MonoBehaviour
    {
        public List<PlayableEditorLevel> playableLevels = new List<PlayableEditorLevel>();
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
            EditorPlayModeManager.LoadLevel(playableLevels[startIndex + buttonIndex], 0, false); // i would love to start the player with more than 1 life here but. UGH
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
            for (int i = 0; i < playableLevels.Count; i++)
            {
                if (playableLevels[i].texture != null)
                {
                    Destroy(playableLevels[i].texture);
                }
            }
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
                buttons[index].UpdateDisplay(playableLevels[i]);
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

        public void UpdateDisplay(PlayableEditorLevel level)
        {
            titleText.text = level.meta.name;
            authorText.text = "By " + level.meta.author;
            modeText.text = LocalizationManager.Instance.GetLocalizedText(LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].nameKey);
            thumbnail.texture = (level.texture == null) ? LevelStudioPlugin.Instance.assetMan.Get<Texture2D>("IconMissing") : level.texture;
        }
    }
}
