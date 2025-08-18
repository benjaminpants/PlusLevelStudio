using MTM101BaldAPI.UI;
using PlusLevelStudio.Editor.SettingsUI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace PlusLevelStudio.Editor.Tools.Customs
{
    public class BulletinBoardPosterTool : CustomTextPosterTool
    {
        public override string id => "custom_bulletinposter";

        public override string basePosterId => "cstm_bulletin_";

        public override string posterEntryType => "bulletinposter";

        public override CustomPosterSettingsUI CreatePosterSettings()
        {
            return EditorController.Instance.CreateUI<CustomPosterSettingsUI>("BulletInCustomizer");
        }

        public override PosterObject GeneratePoster(string id, string text)
        {
            return LevelStudioPlugin.Instance.GenerateBulletInPoster(currentId, text, BaldiFonts.ComicSans18);
        }
    }
}
