using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlusLevelStudio.Editor
{
    [Obsolete("For legacy or placeholder purposes! Do not use!")]
    public class FullStateAction : AbstractEditorAction
    {
        MemoryStream beforeState;
        MemoryStream afterState;
        bool ignorePerform = false;

        public FullStateAction(EditorController controller)
        {
            beforeState = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(beforeState, Encoding.Default, true);
            controller.levelData.Write(writer);
            beforeState.Seek(0, SeekOrigin.Begin);
            writer.Close();
        }

        public void SaveAfterState(EditorController controller)
        {
            afterState = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(afterState, Encoding.Default, true);
            controller.levelData.Write(writer);
            afterState.Seek(0, SeekOrigin.Begin);
            writer.Close();
            ignorePerform = true;
        }

        public void LoadFromState(EditorController controller, MemoryStream state)
        {
            BinaryReader reader = new BinaryReader(state, Encoding.UTF8, true);

            // attempt to preserve toolbar, as a reload may clear out some custom content tools
            for (int i = 0; i < controller.hotSlots.Length; i++)
            {
                if (controller.hotSlots[i].currentTool == null)
                {
                    controller.currentFile.meta.toolbarTools[i] = "";
                }
                else
                {
                    controller.currentFile.meta.toolbarTools[i] = controller.hotSlots[i].currentTool.id;
                }
            }

            controller.LoadEditorLevel(EditorLevelData.ReadFrom(reader), false);
            controller.LoadToolbar(controller.currentFile.meta.toolbarTools);


            state.Seek(0, SeekOrigin.Begin);
            reader.Dispose();
        }

        public override void Perform(EditorController controller)
        {
            // adding a new action calls it instantly
            if (ignorePerform)
            {
                ignorePerform = false;
                return;
            }
            LoadFromState(controller, afterState);
        }

        public override void Undo(EditorController controller)
        {
            LoadFromState(controller, beforeState);
        }
    }
}
