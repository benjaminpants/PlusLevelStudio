using MTM101BaldAPI.AssetTools;
using PlusLevelStudio.UI;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.Editor.SettingsUI
{
    public class TextureChangerExchangeHandler : EditorOverlayUIExchangeHandler
    {
        UIExchangeHandler parentExchange;
        string currentTexture;
        RawImage preview;
        RawImage[] textureDisplays = new RawImage[12];
        TextMeshProUGUI pageCountText;
        int currentPage = 0;
        public int currentMaxPages => Mathf.CeilToInt((float)LevelStudioPlugin.Instance.selectableTextures.Count / textureDisplays.Length);

        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            for (int i = 0; i < textureDisplays.Length; i++)
            {
                textureDisplays[i] = transform.Find("Tex" + i).GetComponent<RawImage>();
            }
            preview = transform.Find("Preview").GetComponent<RawImage>();
            pageCountText = transform.Find("PageCount").GetComponent<TextMeshProUGUI>();
            if (EditorController.Instance.currentMode.vanillaComplaint)
            {
                transform.Find("CustomTextureButton").gameObject.SetActive(false);
                transform.Find("CustomTextureButton_Collision").gameObject.SetActive(false);
            }
        }

        public void UpdatePreview()
        {
            preview.texture = LevelLoaderPlugin.RoomTextureFromAlias(currentTexture);
        }

        public void RefreshPage(int page)
        {
            int startIndex = page * textureDisplays.Length;
            for (int i = startIndex; i < startIndex + textureDisplays.Length; i++)
            {
                if (i >= LevelStudioPlugin.Instance.selectableTextures.Count)
                {
                    textureDisplays[i - startIndex].gameObject.SetActive(false);
                    continue;
                }
                textureDisplays[i - startIndex].gameObject.SetActive(true);
                textureDisplays[i - startIndex].texture = LevelLoaderPlugin.RoomTextureFromAlias(LevelStudioPlugin.Instance.selectableTextures[i]);
            }
            pageCountText.text = (page + 1) + "/" + currentMaxPages;
        }


        public void Assign(UIExchangeHandler parent, string startingTexture)
        {
            parentExchange = parent;
            currentTexture = startingTexture;
            RefreshPage(0);
            UpdatePreview();
        }

        public void SelectTexture(string id)
        {
            currentTexture = id;
            UpdatePreview();
            parentExchange.SendInteractionMessage("changeTexture", currentTexture);
        }

        public bool CustomTextureSubmitted(string path)
        {
            string id = "cstm_" + Path.GetFileNameWithoutExtension(path);
            string fileName = Path.GetFileName(path); // unfortunately Path.GetRelativePath doesn't exist in .net 2.0
            if (!EditorController.Instance.customContent.textures.ContainsKey(id))
            {
                Texture2D texture = AssetLoader.TextureFromFile(path);
                EditorController.Instance.customContent.textures.Add(id, texture);
                EditorController.Instance.customContentPackage.entries.Add(new EditorCustomContentEntry("texture", id, fileName));
            }
            SelectTexture(id);
            return true;
        }

        public override void SendInteractionMessage(string message, object data)
        {
            if (message.StartsWith("select"))
            {
                int num = int.Parse(message.Replace("select", ""));
                num += currentPage * textureDisplays.Length;
                SelectTexture(LevelStudioPlugin.Instance.selectableTextures[num]);
            }
            switch (message)
            {
                case "nextPage":
                    currentPage = Mathf.Clamp(currentPage + 1, 0, currentMaxPages - 1);
                    RefreshPage(currentPage);
                    break;
                case "prevPage":
                    currentPage = Mathf.Clamp(currentPage - 1, 0, currentMaxPages - 1);
                    RefreshPage(currentPage);
                    break;
                case "customTextures":
                    EditorController.Instance.CreateUIFileBrowser(LevelStudioPlugin.customTexturePath, string.Empty, "png", CustomTextureSubmitted);
                    break;
            }
            base.SendInteractionMessage(message, data);
        }
    }

    public class RoomSettingsExchangeHandler : EditorOverlayUIExchangeHandler
    {
        public RawImage floorImage;
        public RawImage wallImage;
        public RawImage ceilingImage;
        bool somethingChanged = false;
        EditorRoom myRoom;
        int changingFor = 0;
        public void AssignRoom(EditorRoom room)
        {
            myRoom = room;
            UpdateTextures();
        }

        public void ApplyToAllRooms(bool confirm)
        {
            EditorRoom[] nonMatchingRooms = EditorController.Instance.levelData.rooms.Where(x => 
            ((x.textureContainer.floor != myRoom.textureContainer.floor) || 
            (x.textureContainer.wall != myRoom.textureContainer.wall) || 
            (x.textureContainer.floor != myRoom.textureContainer.floor)) && (x != myRoom) && (x.roomType == myRoom.roomType)).ToArray();
            if ((nonMatchingRooms.Length > 0) && !confirm)
            {
                EditorController.Instance.CreateUIPopup(String.Format(LocalizationManager.Instance.GetLocalizedText("Ed_Menu_RoomMassChangeWarning"), nonMatchingRooms.Length), () => { ApplyToAllRooms(true); }, () => { });
                return;
            }
            somethingChanged = true;
            for (int i = 0; i < nonMatchingRooms.Length; i++)
            {
                nonMatchingRooms[i].textureContainer.floor = myRoom.textureContainer.floor;
                nonMatchingRooms[i].textureContainer.wall = myRoom.textureContainer.wall;
                nonMatchingRooms[i].textureContainer.ceiling = myRoom.textureContainer.ceiling;
            }
            EditorController.Instance.RefreshCells();
            EditorController.Instance.levelData.doors.ForEach(x => EditorController.Instance.UpdateVisual(x));
            EditorController.Instance.levelData.windows.ForEach(x => EditorController.Instance.UpdateVisual(x)); // why isn't this working
            EditorController.Instance.UpdateVisualsForRoom(myRoom);
        }

        public void ChangeTextures(string tex)
        {
            switch (changingFor)
            {
                case 0:
                    myRoom.textureContainer.floor = tex;
                    break;
                case 1:
                    myRoom.textureContainer.wall = tex;
                    break;
                case 2:
                    myRoom.textureContainer.ceiling = tex;
                    break;
            }
            EditorController.Instance.RefreshCells();
            EditorController.Instance.levelData.doors.ForEach(x => EditorController.Instance.UpdateVisual(x));
            EditorController.Instance.levelData.windows.ForEach(x => EditorController.Instance.UpdateVisual(x));
            somethingChanged = true;
            UpdateTextures();
            EditorController.Instance.CleanupUnusedContentFromData();
        }

        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            floorImage = transform.Find("FloorTex").GetComponent<RawImage>();
            wallImage = transform.Find("WallTex").GetComponent<RawImage>();
            ceilingImage = transform.Find("CeilTex").GetComponent<RawImage>();
            EditorController.Instance.HoldUndo();
        }

        public override bool OnExit()
        {
            if (somethingChanged)
            {
                EditorController.Instance.AddHeldUndo();
            }
            else
            {
                EditorController.Instance.CancelHeldUndo();
            }
            somethingChanged = false;
            return base.OnExit();
        }

        public void UpdateTextures()
        {
            floorImage.texture = myRoom.floorTex;
            wallImage.texture = myRoom.wallTex;
            ceilingImage.texture = myRoom.ceilTex;
        }

        public void OpenTextureSelector(string texture)
        {
            TextureChangerExchangeHandler exch = EditorController.Instance.CreateUI<TextureChangerExchangeHandler>("TextureSelector");
            exch.Assign(this, texture);
        }

        public override void SendInteractionMessage(string message, object data)
        {
            switch (message)
            {
                case "changeFloorTex":
                    changingFor = 0;
                    OpenTextureSelector(myRoom.textureContainer.floor);
                    break;
                case "changeCeilTex":
                    changingFor = 2;
                    OpenTextureSelector(myRoom.textureContainer.ceiling);
                    break;
                case "changeWallTex":
                    changingFor = 1;
                    OpenTextureSelector(myRoom.textureContainer.wall);
                    break;
                case "changeTexture":
                    ChangeTextures((string)data);
                    break;
                case "changeToMatch":
                    ApplyToAllRooms(false);
                    break;
            }
            base.SendInteractionMessage(message, data);
        }
    }
}
