using HarmonyLib;
using PlusLevelStudio.Editor.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlusLevelStudio.Editor.Pages
{
    public class PremadeCustomImagePosterTool : PosterTool, IDeletableTool
    {
        public string fileName;
        public string assetType;
        public override string titleKey => fileName;
        public override string descKey => "Ed_Tool_GenericCustomDesc";

        public PremadeCustomImagePosterTool(string fileName, string assetType, string type, Sprite sprite) : base(type, sprite)
        {
            this.assetType = assetType;
            this.fileName = fileName;
            frameOverride = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("SlotIndividualCustom");
        }

        public void RequestDelete(bool shouldConfirm)
        {
            EditorCustomContentHandler handler = EditorController.Instance.customContent.GetHandlerFor(assetType);
            if (!shouldConfirm)
            {
                handler.EditorRemoveAllUsing(EditorController.Instance, EditorController.Instance.customContentPackage, type);
                EditorController.Instance.ForceToolsRefresh();
                return;
            }
            int usingCount = handler.EditorUsingElementCount(EditorController.Instance, type);
            if (usingCount == 0) { RequestDelete(false); return; }
            EditorController.Instance.CreateUIPopup(String.Format(LocalizationManager.Instance.GetLocalizedText("Ed_Menu_CustomAssetDeleteWarning"), usingCount), () => { RequestDelete(false); }, null);
        }
    }

    public class PremadeCustomPosterTool : PosterTool, IDeletableTool
    {
        public string title;
        public string text;
        public string assetType;
        public override string titleKey => title;
        public override string descKey => text;

        public PremadeCustomPosterTool(string title, string text, string assetType, string type, Sprite sprite) : base(type, sprite)
        {
            this.assetType = assetType;
            this.title = title;
            this.text = text;
            frameOverride = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("SlotIndividualCustom");
        }

        public void RequestDelete(bool shouldConfirm)
        {
            EditorCustomContentHandler handler = EditorController.Instance.customContent.GetHandlerFor(assetType);
            if (!shouldConfirm)
            {
                handler.EditorRemoveAllUsing(EditorController.Instance, EditorController.Instance.customContentPackage, type);
                EditorController.Instance.ForceToolsRefresh();
                return;
            }
            int usingCount = handler.EditorUsingElementCount(EditorController.Instance, type);
            if (usingCount == 0) { RequestDelete(false); return; }
            EditorController.Instance.CreateUIPopup(String.Format(LocalizationManager.Instance.GetLocalizedText("Ed_Menu_CustomAssetDeleteWarning"), usingCount), () => { RequestDelete(false); }, null);
        }
    }

    public class CustomPosterSubpage : AbstractToolboxSubPage
    {
        public List<EditorTool> tools = new List<EditorTool>();
        public List<PosterTool> customTools = new List<PosterTool>();

        public override bool Add(EditorTool tool)
        {
            tools.Add(tool);
            return true;
        }

        public override bool AddToStart(EditorTool tool)
        {
            tools.Insert(0, tool);
            return true;
        }

        public override int GetCount(EditorController controller)
        {
            return tools.Count + customTools.Count;
        }

        public override string GetName()
        {
            return LocalizationManager.Instance.GetLocalizedText("Ed_SubCategory_Custom");
        }

        public void RefreshForTextPosterHandler(EditorController controller, Dictionary<string, PosterObject> stpr, string handlerName, string namePre, HashSet<string> foundKeys)
        {
            foreach (var kvp in stpr)
            {
                foundKeys.Add(kvp.Key);
                if (customTools.Find(x => kvp.Key == x.type) == null)
                {
                    customTools.Add(new PremadeCustomPosterTool(namePre + kvp.Key.GetHashCode(), kvp.Value.textData[0].textKey, handlerName, kvp.Key, LevelStudioPlugin.Instance.GenerateOrGetSmallPosterSprite(kvp.Value, false))
                    {
                        frameOverride = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("SlotIndividualCustom")
                    });
                }
            }
        }

        public void CheckForAndRefreshToolList(EditorController controller)
        {
            if (controller == null) return;
            CustomImagePosterContentHandler imageHandler = (CustomImagePosterContentHandler)controller.customContent.GetHandlerFor("imageposter");
            HashSet<string> foundKeys = new HashSet<string>();
            foreach (var kvp in imageHandler.extend.dictionary)
            {
                foundKeys.Add(kvp.Key);
                if (customTools.Find(x => kvp.Key == x.type) == null)
                {
                    EditorCustomContentEntry entry = controller.customContentPackage.entries.Find(x => x.id == kvp.Key);
                    string filePath = kvp.Key;
                    if (entry.usingFilePath)
                    {
                        filePath = entry.filePath;
                    }
                    customTools.Add(new PremadeCustomImagePosterTool(filePath, "imageposter", kvp.Key, LevelStudioPlugin.Instance.GenerateOrGetSmallPosterSprite(kvp.Value, false))
                    {
                        frameOverride = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("SlotIndividualCustom")
                    });
                }
            }
            RefreshForTextPosterHandler(controller, ((CustomTextPosterContentHandler)controller.customContent.GetHandlerFor("baldisaysposter")).extend.dictionary, "baldisaysposter", "Baldi Says #", foundKeys);
            RefreshForTextPosterHandler(controller, ((CustomTextPosterContentHandler)controller.customContent.GetHandlerFor("chalkboardposter")).extend.dictionary, "chalkboardposter", "Chalkboard #", foundKeys);
            RefreshForTextPosterHandler(controller, ((CustomTextPosterContentHandler)controller.customContent.GetHandlerFor("bulletinposter")).extend.dictionary, "bulletinposter", "Bulletin #", foundKeys);
            RefreshForTextPosterHandler(controller, ((CustomTextPosterContentHandler)controller.customContent.GetHandlerFor("bulletinsmallposter")).extend.dictionary, "bulletinsmallposter", "Bulletin Small #", foundKeys);
            List<PosterTool> toClean = new List<PosterTool>();
            foreach (PosterTool pTool in customTools)
            {
                if (!foundKeys.Contains(pTool.type))
                {
                    toClean.Add(pTool);
                }
            }
            toClean.Do(x => CleanupPoster(x));
        }

        public void CleanupPoster(PosterTool tool)
        {
            GameObject.Destroy(tool.sprite.texture);
            GameObject.Destroy(tool.sprite);
            customTools.Remove(tool);
            EditorController.Instance?.PurgeFromToolbar(tool);
        }

        public override EditorTool[] GetTools(EditorController controller)
        {
            List<EditorTool> combinedTools = new List<EditorTool>();
            CheckForAndRefreshToolList(controller);
            combinedTools.AddRange(customTools);
            combinedTools.Reverse();
            combinedTools.AddRange(tools);
            return combinedTools.ToArray();
        }

        public override bool InsertRangeAfterId(string id, IEnumerable<EditorTool> tools)
        {
            throw new NotImplementedException();
        }

        public override AbstractToolboxSubPage MakeCopy()
        {
            return new CustomPosterSubpage()
            {
                tools = new List<EditorTool>(tools)
            };
        }

        public override bool Remove(EditorTool tool)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveById(string toolId)
        {
            throw new NotImplementedException();
        }

        public override void ResetState()
        {
            while (customTools.Count > 0)
            {
                CleanupPoster(customTools[0]);
            }
        }

        public override bool IncludeInAll()
        {
            return true;
        }
    }
}
