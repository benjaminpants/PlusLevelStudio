using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor.ModeSettings
{
    public class SpeedyChallengeGameMode : EditorGameMode
    {
        public override void AttemptToUpdateLegacyLevel(EditorController controller, StudioLevelLegacyFlags flagsToHandle)
        {
            if (flagsToHandle.HasFlag(StudioLevelLegacyFlags.BeforeNPCCustom))
            {
                controller.levelData.npcs.ForEach(npc =>
                {
                    if (npc.npc == "baldi")
                    {
                        BaldiProperties baldiProp = (BaldiProperties)npc.properties;
                        int fastBaldiIndex = LevelStudioPlugin.Instance.animCurvesBaldiPrefabsDoNotAddToThis.FindIndex(x => x.name == "FastBaldi");
                        baldiProp.slapPreIndex = fastBaldiIndex;
                        baldiProp.speedPreIndex = fastBaldiIndex;
                        baldiProp.RefreshSlapPre();
                        baldiProp.RefreshSpeedPre();
                    }
                });
            }
        }

        public override void ApplyDefaultNPCProperties(string npc, NPCProperties props)
        {
            if (npc == "baldi")
            {
                BaldiProperties baldiProp = (BaldiProperties)props;
                int fastBaldiIndex = LevelStudioPlugin.Instance.animCurvesBaldiPrefabsDoNotAddToThis.FindIndex(x => x.name == "FastBaldi");
                baldiProp.slapPreIndex = fastBaldiIndex;
                baldiProp.speedPreIndex = fastBaldiIndex;
                baldiProp.RefreshSlapPre();
                baldiProp.RefreshSpeedPre();
            }
        }
    }
}
