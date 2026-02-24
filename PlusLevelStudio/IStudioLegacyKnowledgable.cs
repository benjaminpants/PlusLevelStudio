using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio
{
    /// <summary>
    /// An interface that a few classes can inherit (EditorLevelData, any class inheriting from BaseGameManager) to know if the level existed before certain features were implemented.
    /// </summary>
    public interface IStudioLegacyKnowledgable
    {
        StudioLevelLegacyFlags legacyFlags { get; set; }
    }

    /// <summary>
    /// Flags that represent
    /// </summary>
    [Flags]
    public enum StudioLevelLegacyFlags
    {
        None = 0,
        BeforeNPCCustom = 1,
    }
}
