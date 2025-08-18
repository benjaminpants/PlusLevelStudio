using MTM101BaldAPI.UI;
using PlusLevelStudio.Editor.SettingsUI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace PlusLevelStudio.Editor.Tools.Customs
{
    public class BulletinBoardSmallPosterTool : CustomTextPosterTool
    {
        public override string id => "custom_bulletinsmallposter";

        public override string basePosterId => "cstm_bulletinsmall_";

        public override string posterEntryType => "bulletinsmallposter";

        public override CustomPosterSettingsUI CreatePosterSettings()
        {
            return EditorController.Instance.CreateUI<CustomPosterSettingsUI>("BulletInSmallCustomizer");
        }

        public override PosterObject GeneratePoster(string id, string text)
        {
            return LevelStudioPlugin.Instance.GenerateBulletInPoster(currentId, text, BaldiFonts.ComicSans12);
        }
    }
}
