using MTM101BaldAPI.AssetTools;
using PlusLevelStudio.Campaigns;
using PlusLevelStudio.Editor;
using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace PlusLevelStudio.Menus
{
    public class EditorCampaignEditorSettingsMenu : UIExchangeHandler
    {
        public PlayableEditorLevel targetLevel;
        public TextMeshProUGUI fieldTripText;
        public int currentFieldtripIndex = 0;
        public string fieldTripId => LevelStudioPlugin.Instance.selectableFieldTrips[currentFieldtripIndex];
        public FieldTripObject currentFieldTrip => LevelStudioPlugin.Instance.fieldTrips[fieldTripId];
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            fieldTripText = transform.Find("FieldTripText").GetComponent<TextMeshProUGUI>();
        }

        public void ChangeTrip(int add)
        {
            currentFieldtripIndex = (currentFieldtripIndex + add) % LevelStudioPlugin.Instance.selectableFieldTrips.Count;
            if (currentFieldtripIndex < 0)
            {
                currentFieldtripIndex = LevelStudioPlugin.Instance.selectableFieldTrips.Count - 1;
            }
            if (fieldTripId == "none")
            {
                fieldTripText.text = LocalizationManager.Instance.GetLocalizedText("Ed_Menu_NoTrip");
                return;
            }
            fieldTripText.text = LocalizationManager.Instance.GetLocalizedText(currentFieldTrip.nameKey);
            targetLevel.meta.playSettings.fieldTrip = fieldTripId;
        }

        public void AssignLevel(PlayableEditorLevel level)
        {
            targetLevel = level;
            currentFieldtripIndex = Mathf.Max(LevelStudioPlugin.Instance.selectableFieldTrips.IndexOf(targetLevel.meta.playSettings.fieldTrip),0);
            ChangeTrip(0);
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "exit":
                    Destroy(gameObject);
                    break;
                case "prevFieldTrip":
                    ChangeTrip(-1);
                    break;
                case "nextFieldTrip":
                    ChangeTrip(1);
                    break;
            }
        }
    }

    // the code here is so jank and needs refactoring
    public class EditorCampaignEditorMenu : UIExchangeHandler
    {
        public Transform upButton;
        public Transform downButton;
        TextMeshProUGUI nameText;
        TextMeshProUGUI authorText;
        TextMeshProUGUI lifeModeText;
        RawImage thumbImage;
        public Texture2D thumbTexture;
        public byte[] thumbData = null;
        bool currentThumbIsPlayerSelected = false;
        public string currentFileName = "MyAwesomeCampaign";

        public EditorCampaignData campaignData = new EditorCampaignData();

        public int currentLifeModeIndex = 0;

        protected class CampaignLevelListing
        {
            public List<Transform> transformsToToggle = new List<Transform>();
            public TextMeshProUGUI levelTitle;
            public TextMeshProUGUI elevatorText;

            public void SetVisible(bool state)
            {
                transformsToToggle.ForEach(x => x.gameObject.SetActive(state));
            }
        }

        public void ChangeLifeMode(int add)
        {
            currentLifeModeIndex = (currentLifeModeIndex + add) % LevelStudioPlugin.Instance.selectableLifeModes.Count;
            if (currentLifeModeIndex < 0)
            {
                currentLifeModeIndex = LevelStudioPlugin.Instance.selectableLifeModes.Count - 1;
            }
            LifeModeData data = LevelStudioPlugin.Instance.lifeModes[LevelStudioPlugin.Instance.selectableLifeModes[currentLifeModeIndex]];
            lifeModeText.text = LocalizationManager.Instance.GetLocalizedText(data.localizationKey);
            campaignData.meta.lifeMode = LevelStudioPlugin.Instance.selectableLifeModes[currentLifeModeIndex];
        }

        protected void PlaySongIfNecessary()
        {
            if (!Singleton<MusicManager>.Instance.MidiPlaying)
            {
                Singleton<MusicManager>.Instance.StopMidi();
                Singleton<MusicManager>.Instance.PlayMidi(LevelStudioPlugin.Instance.editorTracks[UnityEngine.Random.Range(0, LevelStudioPlugin.Instance.editorTracks.Count)], false);
            }
        }

        int page = 0;

        protected List<CampaignLevelListing> campaignListings = new List<CampaignLevelListing>();

        public EditorModeSelectionMenu menu;

        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            for (int i = 0; i < 3; i++)
            {
                campaignListings.Add(new CampaignLevelListing()
                {
                    levelTitle = transform.Find("CampText" + i).GetComponent<TextMeshProUGUI>(),
                    elevatorText = transform.Find("CampFloorTitle" + i).GetComponent<TextMeshProUGUI>(),
                    transformsToToggle = new List<Transform>()
                    {
                        transform.Find("CampText" + i),
                        transform.Find("CampFloorTitle" + i),
                        transform.Find("CampDiscard" + i),
                        transform.Find("CampSettings" + i),
                        transform.Find("CampBG" + i),
                        transform.Find("CampUp" + i),
                        transform.Find("CampDown" + i),
                    }
                });
            }
            upButton = transform.Find("PageUp");
            downButton = transform.Find("PageDown");
            nameText = transform.Find("NameBox").GetComponent<TextMeshProUGUI>();
            authorText = transform.Find("AuthorBox").GetComponent<TextMeshProUGUI>();
            thumbImage = transform.Find("Thumbnail").GetComponent<RawImage>();
            lifeModeText = transform.Find("PlayStyleText").GetComponent<TextMeshProUGUI>();
            ChangePage(0);
            Refresh();
        }

        void OnEnable()
        {
            Singleton<MusicManager>.Instance.StopMidi();
        }

        void Update()
        {
            PlaySongIfNecessary();
        }

        public void UpdateListings()
        {
            for (int i = 0; i < campaignListings.Count; i++)
            {
                int ind = i + (page * campaignListings.Count);
                if (ind >= campaignData.levelNamesAndMeta.Count)
                {
                    campaignListings[i].SetVisible(false);
                    continue;
                }
                campaignListings[i].SetVisible(true);
                campaignListings[i].levelTitle.text = Path.GetFileName(campaignData.levelNamesAndMeta[ind].filePath);
                campaignListings[i].elevatorText.text = campaignData.levelNamesAndMeta[ind].level.data.levelTitle;
            }
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            if (message.StartsWith("discard"))
            {
                int index = (int.Parse(message.Substring(7))) + (page * campaignListings.Count);
                if (index >= campaignData.levelNamesAndMeta.Count) return;
                campaignData.levelNamesAndMeta.RemoveAt(index);
                ChangePage(0);
                Refresh();
                return;
            }
            if (message.StartsWith("moveUp"))
            {
                int index = (int.Parse(message.Substring(6))) + (page * campaignListings.Count);
                if (index >= campaignData.levelNamesAndMeta.Count) return;
                EditorCampaignDataEntry entry = campaignData.levelNamesAndMeta[index];
                int indexToInsertAt = index - 1;
                if (indexToInsertAt < 0) return;
                campaignData.levelNamesAndMeta.RemoveAt(index);
                campaignData.levelNamesAndMeta.Insert(indexToInsertAt, entry);
                ChangePage(0);
                Refresh();
                return;
            }
            if (message.StartsWith("moveDown"))
            {
                int index = (int.Parse(message.Substring(8))) + (page * campaignListings.Count);
                if (index >= campaignData.levelNamesAndMeta.Count) return;
                EditorCampaignDataEntry entry = campaignData.levelNamesAndMeta[index];
                int indexToInsertAt = index + 1;
                if (indexToInsertAt >= campaignData.levelNamesAndMeta.Count) return;
                campaignData.levelNamesAndMeta.RemoveAt(index);
                campaignData.levelNamesAndMeta.Insert(indexToInsertAt, entry);
                ChangePage(0);
                Refresh();
                return;
            }
            if (message.StartsWith("settings"))
            {
                int index = (int.Parse(message.Substring(8))) + (page * campaignListings.Count);
                if (index >= campaignData.levelNamesAndMeta.Count) return;
                EditorCampaignEditorSettingsMenu menu = UIBuilder.BuildUIFromFile<EditorCampaignEditorSettingsMenu>(gameObject.GetComponent<RectTransform>(), "Settings", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "Titlescreen", "CampaignSettings.json"));
                menu.AssignLevel(campaignData.levelNamesAndMeta[index].level);
                return;
            }
            GenericUIFileBrowser fileBrowser;
            switch (message)
            {
                case "exit":
                    gameObject.SetActive(false);
                    menu.editorTypeParent.SetActive(true);
                    Singleton<MusicManager>.Instance.StopMidi();
                    break;
                case "browseLevels":
                    fileBrowser = UIBuilder.BuildUIFromFile<GenericUIFileBrowser>(gameObject.GetComponent<RectTransform>(), "FileBrowser", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "FileBrowser.json"));
                    fileBrowser.Setup(LevelStudioPlugin.levelExportPath, "pbpl", "MyFirstLevel", false, OnLevelSubmit);
                    break;
                case "pageUp":
                    ChangePage(-1);
                    break;
                case "pageDown":
                    ChangePage(1);
                    break;
                case "save":
                    Save();
                    break;
                case "load":
                    Load();
                    break;
                case "export":
                    Export();
                    break;
                case "nameChanged":
                    campaignData.meta.name = (string)data;
                    Refresh();
                    break;
                case "authorChanged":
                    campaignData.meta.author = (string)data;
                    Refresh();
                    break;
                case "changeThumb":
                    fileBrowser = UIBuilder.BuildUIFromFile<GenericUIFileBrowser>(gameObject.GetComponent<RectTransform>(), "FileBrowser", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "FileBrowser.json"));
                    fileBrowser.Setup(LevelStudioPlugin.customThumbnailsPath, "png", string.Empty, false, CustomThumbnailSubmitted);
                    break;
                case "clearThumb":
                    ClearThumbnail();
                    Refresh();
                    break;
                case "prevPlayStyle":
                    ChangeLifeMode(-1);
                    break;
                case "nextPlayStyle":
                    ChangeLifeMode(1);
                    break;
            }
        }

        void ClearThumbnail()
        {
            if (thumbTexture != null)
            {
                GameObject.Destroy(thumbTexture);
            }
            thumbTexture = null;
            thumbData = null;
            currentThumbIsPlayerSelected = false;
            campaignData.thumbPath = string.Empty;
        }

        public void Save()
        {
            GenericUIFileBrowser fb = UIBuilder.BuildUIFromFile<GenericUIFileBrowser>(gameObject.GetComponent<RectTransform>(), "FileBrowser", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "FileBrowser.json"));
            fb.Setup(LevelStudioPlugin.campaignFilePath, "ecbpl", currentFileName, true, SaveFile);
        }

        public void Load()
        {
            GenericUIFileBrowser fb = UIBuilder.BuildUIFromFile<GenericUIFileBrowser>(gameObject.GetComponent<RectTransform>(), "FileBrowser", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "FileBrowser.json"));
            fb.Setup(LevelStudioPlugin.campaignFilePath, "ecbpl", currentFileName, false, LoadFile);
        }

        bool SaveFile(string path)
        {
            BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate, FileAccess.Write));
            campaignData.Write(writer);
            writer.Close();
            currentFileName = Path.GetFileNameWithoutExtension(path);
            return true;
        }

        bool LoadFile(string path)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(path));
            campaignData = EditorCampaignData.Read(reader);
            reader.Close();
            campaignData.levelNamesAndMeta.ForEach(x => x.LoadLevelFromFilePath());
            if (string.IsNullOrEmpty(campaignData.thumbPath))
            {
                ClearThumbnail();
            }
            else
            {
                CustomThumbnailSubmitted(Path.Combine(LevelStudioPlugin.customThumbnailsPath, campaignData.thumbPath));
            }
            Refresh();
            ChangePage(0);
            currentFileName = Path.GetFileNameWithoutExtension(path);
            return true;
        }

        public void TriggerError(string error)
        {

        }

        public bool CustomThumbnailSubmitted(string path)
        {
            string relativePath = PathHelpers.GetRelativePath(LevelStudioPlugin.customThumbnailsPath, path);
            Texture2D verifyTex = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
            byte[] fileData = File.ReadAllBytes(path);
            try
            {
                ImageConversion.LoadImage(verifyTex, fileData);
                verifyTex.filterMode = FilterMode.Point;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                TriggerError("Ed_Exception_FileCorrupted");
                Destroy(verifyTex);
                verifyTex = null;
            }
            if (verifyTex == null) return false;
            if (verifyTex.width != 64 || verifyTex.height != 64)
            {
                Destroy(verifyTex);
                TriggerError("Ed_Error_MustBe64");
                return false;
            }
            thumbTexture = verifyTex;
            thumbData = fileData;
            campaignData.thumbPath = PathHelpers.GetRelativePath(LevelStudioPlugin.customThumbnailsPath,path);
            currentThumbIsPlayerSelected = true;
            Refresh();
            return true;
        }

        public void SearchForAndUseFallbackThumb()
        {
            if (!currentThumbIsPlayerSelected)
            {
                if (campaignData.levelNamesAndMeta.Count == 0)
                {
                    if (thumbTexture != null)
                    {
                        Destroy(thumbTexture);
                        thumbTexture = null;
                        thumbData = null;
                    }
                    return;
                }
                string prevFall = (thumbTexture == null ? string.Empty : thumbTexture.name);
                string levPath = Path.GetFileNameWithoutExtension(campaignData.levelNamesAndMeta[0].filePath);
                if (levPath == prevFall) return; // dont bother
                Texture2D thumb = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
                try
                {
                    ImageConversion.LoadImage(thumb, campaignData.levelNamesAndMeta[0].level.meta.contentPackage.thumbnailEntry.GetData());
                    thumb.filterMode = FilterMode.Point;
                    thumb.name = levPath;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                    Destroy(thumb);
                    thumb = null;
                }
                if (thumb != null)
                {
                    if (thumbTexture != null)
                    {
                        Destroy(thumbTexture);
                    }
                    thumbData = campaignData.levelNamesAndMeta[0].level.meta.contentPackage.thumbnailEntry.GetData();
                    thumbTexture = thumb;
                }
            }
        }

        public void Refresh()
        {
            SearchForAndUseFallbackThumb();
            nameText.text = campaignData.meta.name;
            authorText.text = campaignData.meta.author;
            thumbImage.texture = thumbTexture == null ? LevelStudioPlugin.Instance.assetMan.Get<Texture2D>("IconMissing") : thumbTexture;
            ChangeLifeMode(0);
        }

        public void Export()
        {
            PlayableEditorCampaign campLevel = new PlayableEditorCampaign();
            campLevel.meta.name = campaignData.meta.name;
            campLevel.meta.author = campaignData.meta.author;
            campLevel.meta.lifeMode = campaignData.meta.lifeMode;
            campLevel.ImportLevels(campaignData.levelNamesAndMeta.Select(x => x.level).ToList());
            if (thumbData != null)
            {
                campLevel.contentPackage.thumbnailEntry = new EditorCustomContentEntry("thumbnail", "thumbnnail", thumbData);
            }
            BinaryWriter writer = new BinaryWriter(new FileStream(Path.Combine(LevelStudioPlugin.levelExportPath, currentFileName + ".cbpl"), FileMode.Create, FileAccess.Write));
            campLevel.Write(writer);
            writer.Close();
        }

        public void ChangePage(int amount)
        {
            int maxPage = Mathf.Max(Mathf.CeilToInt((float)campaignData.levelNamesAndMeta.Count / campaignListings.Count) - 1, 0);
            page = Mathf.Clamp(page + amount, 0, maxPage);
            upButton.gameObject.SetActive(page != 0);
            downButton.gameObject.SetActive(page != maxPage);
            //pageDisplay.text = (currentPage + 1) + "/" + (maxPage + 1);
            UpdateListings();
        }

        public bool OnLevelSubmit(string path)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(path));
            EditorCampaignDataEntry entry = new EditorCampaignDataEntry(path, PlayableEditorLevel.Read(reader));
            reader.Close();
            campaignData.levelNamesAndMeta.Add(entry);
            entry.filePath = path;
            ChangePage(0);
            Refresh();
            return true;
        }
    }
}
