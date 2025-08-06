using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class CompliantEditorController : EditorController
    {
        public override void Export()
        {
            BaldiLevel level = Compile();
            Directory.CreateDirectory(LevelStudioPlugin.levelExportPath);

            BinaryWriter writer = new BinaryWriter(new FileStream(Path.Combine(LevelStudioPlugin.levelExportPath, currentFileName + ".bpl"), FileMode.Create, FileAccess.Write));
            level.Write(writer);
            writer.Close();
            Application.OpenURL("file://" + LevelStudioPlugin.levelExportPath);
        }
    }
}
