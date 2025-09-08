using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio
{
    public class EditorLoadException : Exception
    {
        private string unlocalizedString;
        private string[] formats = new string[0];
        public EditorLoadException(string unlocalizedString)
        {
            this.unlocalizedString = unlocalizedString;
        }

        public EditorLoadException(string localizedString, string[] formats) : this(localizedString)
        {
            this.formats = formats;
        }

        public override string Message
        {
            get
            {
                if (LocalizationManager.Instance == null) return unlocalizedString;
                return string.Format(LocalizationManager.Instance.GetLocalizedText(unlocalizedString), formats);
            }
        }
    }
}
