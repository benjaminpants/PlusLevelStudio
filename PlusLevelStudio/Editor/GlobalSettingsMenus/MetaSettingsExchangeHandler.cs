using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace PlusLevelStudio.Editor.GlobalSettingsMenus
{
    public class MetaSettingsExchangeHandler : GlobalSettingsUIExchangeHandler
    {
        TextMeshProUGUI titleText;
        TextMeshProUGUI authorText;
        RawImage thumbnailImage;
        Texture2D cachedThumbnail = null;
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            titleText = transform.Find("LevelTitle").GetComponent<TextMeshProUGUI>();
            authorText = transform.Find("AuthorName").GetComponent<TextMeshProUGUI>();
            thumbnailImage = transform.Find("Thumbnail").GetComponent<RawImage>();
        }

        public override void Cleanup()
        {
            if (cachedThumbnail != null)
            {
                Destroy(cachedThumbnail);
                thumbnailImage.texture = null;
                cachedThumbnail = null;
            }
        }

        public override void Refresh()
        {
            titleText.text = EditorController.Instance.levelData.meta.name;
            authorText.text = EditorController.Instance.levelData.meta.author;
            if (cachedThumbnail != null)
            {
                Destroy(cachedThumbnail);
                cachedThumbnail = null;
            }
            if (EditorController.Instance.levelData.meta.contentPackage.thumbnailEntry != null)
            {
                cachedThumbnail = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
                try
                {
                    ImageConversion.LoadImage(cachedThumbnail, EditorController.Instance.levelData.meta.contentPackage.thumbnailEntry.GetData());
                    cachedThumbnail.filterMode = FilterMode.Point;
                    cachedThumbnail.name = "CachedThumbnail";
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                    Destroy(cachedThumbnail);
                    cachedThumbnail = null;
                }
            }
            if (cachedThumbnail == null)
            {
                cachedThumbnail = EditorController.Instance.GenerateThumbnail();
            }
            thumbnailImage.texture = cachedThumbnail;
        }

        public bool CustomThumbnailSubmitted(string path)
        {
            Texture2D verifyTex = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
            try
            {
                ImageConversion.LoadImage(verifyTex, File.ReadAllBytes(path));
                verifyTex.filterMode = FilterMode.Point;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                EditorController.Instance.TriggerError("Ed_Exception_FileCorrupted");
                Destroy(verifyTex);
                verifyTex = null;
            }
            if (verifyTex == null) return false;
            if (verifyTex.width != 64 || verifyTex.height != 64)
            {
                Destroy(verifyTex);
                EditorController.Instance.TriggerError("Ed_Error_MustBe64");
                return false;
            }
            Destroy(verifyTex);
            EditorController.Instance.levelData.meta.contentPackage.thumbnailEntry = new EditorCustomContentEntry()
            {
                contentType = "thumbnail",
                id = "thumbnail",
                filePath = path
            };
            Refresh();
            handler.somethingChanged = true;
            return true;
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "titleChanged":
                    EditorController.Instance.levelData.meta.name = (string)data;
                    handler.somethingChanged = true;
                    break;
                case "authorChanged":
                    EditorController.Instance.levelData.meta.author = (string)data;
                    handler.somethingChanged = true;
                    break;
                case "changeThumb":
                    EditorController.Instance.CreateUIFileBrowser(LevelStudioPlugin.customThumbnailsPath, string.Empty, "png", false, CustomThumbnailSubmitted);
                    break;
                case "clearThumb":
                    EditorController.Instance.levelData.meta.contentPackage.thumbnailEntry = null;
                    handler.somethingChanged = true;
                    Refresh();
                    break;
            }
        }
    }
}
