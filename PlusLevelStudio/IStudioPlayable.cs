using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio
{
    /// <summary>
    /// An interface that abstracts away the logic for showing levels in the studio Play screen.
    /// </summary>
    public interface IStudioPlayable
    {
        string GetName();
        string GetAuthor();
        string GetLocalizedGamemode();
        EditorCustomContentEntry GetThumbnail();

        void Play();
    }
}
