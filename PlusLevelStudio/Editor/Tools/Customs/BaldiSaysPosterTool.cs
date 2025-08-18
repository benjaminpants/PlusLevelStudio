using PlusLevelStudio.Editor.SettingsUI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace PlusLevelStudio.Editor.Tools.Customs
{
    public class BaldiSaysPosterTool : CustomTextPosterTool
    {
        public override string id => "custom_baldisaysposter";

        public override string basePosterId => "cstm_baldisays_";

        public override string posterEntryType => "baldisaysposter";

        public override CustomPosterSettingsUI CreatePosterSettings()
        {
            return EditorController.Instance.CreateUI<CustomPosterSettingsUI>("BaldiSaysCustomizer");
        }

        public override PosterObject GeneratePoster(string id, string text)
        {
            return LevelStudioPlugin.Instance.GenerateBaldiSaysPoster(currentId, text);
        }
    }
}
