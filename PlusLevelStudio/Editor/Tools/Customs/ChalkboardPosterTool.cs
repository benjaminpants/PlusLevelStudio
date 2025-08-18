using PlusLevelStudio.Editor.SettingsUI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace PlusLevelStudio.Editor.Tools.Customs
{
    public class ChalkboardPosterTool : CustomTextPosterTool
    {
        public override string id => "custom_chalkboardposter";

        public override string basePosterId => "cstm_chalk_";

        public override string posterEntryType => "chalkboardposter";

        public override CustomPosterSettingsUI CreatePosterSettings()
        {
            return EditorController.Instance.CreateUI<CustomPosterSettingsUI>("ChalkBoardCustomizer");
        }

        public override PosterObject GeneratePoster(string id, string text)
        {
            return LevelStudioPlugin.Instance.GenerateChalkPoster(currentId, text);
        }
    }
}
