using PlusLevelStudio.Editor.SettingsUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor.Tools.Customs
{
    public abstract class CustomTextPosterTool : CustomPosterTool
    {
        public abstract string basePosterId { get; }
        public abstract string posterEntryType { get; }
        public override abstract string id { get; }

        public abstract CustomPosterSettingsUI CreatePosterSettings();

        public override void Begin()
        {
            CustomPosterSettingsUI posterSettings = CreatePosterSettings();
            posterSettings.onSubmit = OnSubmit;
            if (!string.IsNullOrEmpty(lastUsedFile))
            {
                posterSettings.typableText.text = lastUsedFile;
            }
            currentPopup = posterSettings;
        }

        public abstract PosterObject GeneratePoster(string id, string text);

        public override bool OnSubmit(string text)
        {
            currentId = basePosterId + text.Replace(" ", "_").Replace("<", "_").Replace(">", "_"); //not really the best way of calculating an id but honestly really the only one i have
            posterSelected = true;
            onWaitFrame = true;
            lastUsedFile = text;
            if (EditorController.Instance.customContentPackage.entries.Find(x => x.id == currentId) != null)
            {
                return true;
            }
            EditorCustomContentEntry entry = new EditorCustomContentEntry(posterEntryType, currentId, Encoding.Unicode.GetBytes(text));
            EditorController.Instance.customContent.GetForEntryType(posterEntryType).Add(entry.id, GeneratePoster(currentId, text));
            EditorController.Instance.customContentPackage.entries.Add(entry);
            return true;
        }
    }
}
