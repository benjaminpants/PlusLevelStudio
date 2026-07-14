using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlusStudioLevelLoader.Patches
{
    [HarmonyPatch(typeof(PitstopGameManager))]
    [HarmonyPatch("Initialize")]
    class PitstopPosterPatch
    {
        static void Prefix(EnvironmentController ___ec, DirectedIntVector2 ___nextLevelPosterPlacement)
        {
            if (Singleton<CoreGameManager>.Instance.nextLevel == null) return;
            if (!(Singleton<CoreGameManager>.Instance.nextLevel.extraAsset is ExtendedExtraLevelDataAsset extendedData)) return;
            if (extendedData.levelTypePoster == null) return;
            ___ec.BuildPoster(extendedData.levelTypePoster, ___ec.CellFromPosition(___nextLevelPosterPlacement.position), ___nextLevelPosterPlacement.direction);
        }
    }
}
