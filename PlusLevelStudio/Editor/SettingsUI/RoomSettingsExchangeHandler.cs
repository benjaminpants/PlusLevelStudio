using PlusLevelStudio.UI;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
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
        }

        public void UpdatePreview()
        {
            preview.texture = LevelLoaderPlugin.Instance.roomTextureAliases[currentTexture];
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
                textureDisplays[i - startIndex].texture = LevelLoaderPlugin.Instance.roomTextureAliases[LevelStudioPlugin.Instance.selectableTextures[i]];
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

        public override void SendInteractionMessage(string message, object data)
        {
            if (message.StartsWith("select"))
            {
                int num = int.Parse(message.Replace("select", ""));
                num += currentPage * textureDisplays.Length;
                currentTexture = LevelStudioPlugin.Instance.selectableTextures[num];
                UpdatePreview();
                parentExchange.SendInteractionMessage("changeTexture", currentTexture);
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
            }
            base.SendInteractionMessage(message, data);
        }
    }

    public class RoomSettingsExchangeHandler : EditorOverlayUIExchangeHandler
    {
        public RawImage floorImage;
        public RawImage wallImage;
        public RawImage ceilingImage;
        EditorRoom myRoom;
        int changingFor = 0;
        public void AssignRoom(EditorRoom room)
        {
            myRoom = room;
            UpdateTextures();
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
            UpdateTextures();
        }

        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            floorImage = transform.Find("FloorTex").GetComponent<RawImage>();
            wallImage = transform.Find("WallTex").GetComponent<RawImage>();
            ceilingImage = transform.Find("CeilTex").GetComponent<RawImage>();
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
            }
            base.SendInteractionMessage(message, data);
        }
    }
}
