using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools.Customs
{
    public class CustomPosterTool : PlaceAndRotateTool
    {
        public override string id => "custom_imageposter";
        EditorUIFileBrowser currentBrowser;
        bool imageSelected = false;
        bool onWaitFrame = false;
        string lastUsedFile = "myPoster";
        string currentId = string.Empty;

        public CustomPosterTool()
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/" + id);
        }

        public override void Begin()
        {
            base.Begin();
            currentBrowser = EditorController.Instance.CreateUIFileBrowser(LevelStudioPlugin.customPostersPath, lastUsedFile, "png", false, OnSubmit);
        }

        public bool OnSubmit(string path)
        {
            currentId = "cstm_simple_" + Path.GetFileNameWithoutExtension(path);
            string fileName = Path.GetFileName(path);
            // check to make sure the entry doesn't already exist
            if (EditorController.Instance.customContentPackage.entries.Find(x => x.id == currentId) != null)
            {
                lastUsedFile = fileName;
                imageSelected = true;
                onWaitFrame = true;
                return true;
            }
            Texture2D texture = AssetLoader.TextureFromFile(path);
            if ((texture.width != 256) || (texture.height != 256))
            {
                UnityEngine.Object.Destroy(texture);
                EditorController.Instance.CreateUIOnePopup("Ed_Error_MustBe256");
                return false;
            }
            lastUsedFile = fileName;
            imageSelected = true;
            onWaitFrame = true;
            EditorCustomContentEntry entry = new EditorCustomContentEntry("imageposter", currentId, fileName);
            PosterObject posterObj = ObjectCreators.CreatePosterObject(texture, new PosterTextData[0]);
            posterObj.name = entry.id;
            EditorController.Instance.customContent.posters.Add(entry.id, posterObj);
            EditorController.Instance.customContentPackage.entries.Add(entry);
            return true;
        }

        public override void Exit()
        {
            imageSelected = false;
            currentId = string.Empty;
            currentBrowser = null;
            base.Exit();
        }

        public override void Update()
        {
            if (onWaitFrame)
            {
                onWaitFrame = false;
                return;
            }
            if (!imageSelected)
            {
                EditorController.Instance.selector.DisableSelection();
                if (currentBrowser == null)
                {
                    EditorController.Instance.SwitchToTool(null);
                }
                return;
            }
            base.Update();
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            if (EditorController.Instance.levelData.WallFree(position, dir, false))
            {
                EditorController.Instance.AddUndo();
                PosterPlacement poster = new PosterPlacement();
                poster.position = position;
                poster.direction = dir;
                poster.type = currentId;
                EditorController.Instance.levelData.posters.Add(poster);
                EditorController.Instance.AddVisual(poster);
                EditorController.Instance.CleanupUnusedContentFromData();
                return true;
            }
            return false;
        }

        public override bool MousePressed()
        {
            if (!imageSelected) return false;
            if (onWaitFrame) return false;
            return base.MousePressed();
        }

        public override bool ValidLocation(IntVector2 position)
        {
            if (!base.ValidLocation(position)) return false;
            for (int i = 0; i < 4; i++)
            {
                if (EditorController.Instance.levelData.WallFree(position, (Direction)i, false)) return true;
            }
            return false;
        }
    }
}
