using HarmonyLib;
using PlusLevelStudio.Editor.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Pages
{
    public class PremadeCustomPosterTool : PosterTool
    {
        public string fileName;
        public override string titleKey => fileName;
        public override string descKey => "Ed_Tool_GenericCustomDesc";

        public PremadeCustomPosterTool(string fileName, string type, Sprite sprite) : base(type, sprite)
        {
            this.fileName = fileName;
            frameOverride = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("SlotIndividualCustom");
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
                    customTools.Add(new PremadeCustomPosterTool(filePath, kvp.Key, LevelStudioPlugin.Instance.GenerateOrGetSmallPosterSprite(kvp.Value, false))
                    {
                        frameOverride = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("SlotIndividualCustom")
                    });
                }
            }
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
            customTools.Do(x => CleanupPoster(x));
        }

        public override bool IncludeInAll()
        {
            return true;
        }
    }
}
