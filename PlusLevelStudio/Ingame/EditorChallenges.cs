using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PlusLevelStudio.Ingame
{
    public class EditorGrappleChallengeManager : GrappleChallengeManager
    {
        public override void LoadNextLevel()
        {
            Singleton<EditorPlayModeManager>.Instance.Win();
        }

        public override void ExitedSpawn()
        {
            base.ExitedSpawn();
            ec.StartEventTimers();
        }

        public override void GiveRandomSticker(StickerPackType packType, int total)
        {
            if (packType == StickerPackType.Bonus)
            {
                if (Singleton<CoreGameManager>.Instance.sceneObject.potentialStickers.Where(x => StickerMetaStorage.Instance.Get(x.selection).flags.HasFlag(StickerFlags.IsBonus)).Count() == 0)
                {
                    base.GiveRandomSticker(StickerPackType.Normal, 1);
                    return;
                }
            }
            base.GiveRandomSticker(packType, total);
        }
    }

    public class EditorStealthyChallengeManager : StealthyChallengeManager
    {
        public bool giveChalkErasers = true;
        static FieldInfo _allKnowing = AccessTools.Field(typeof(Principal), "allKnowing");
        public override void LoadNextLevel()
        {
            Singleton<EditorPlayModeManager>.Instance.Win();
        }

        static FieldInfo _ignorePlayerOnSpawn = AccessTools.Field(typeof(NPC), "ignorePlayerOnSpawn");
        public override void ExitedSpawn()
        {
            Dictionary<NPC, bool> toRevert = new Dictionary<NPC, bool>();
            // change all the principal prefabs to ignore the player so we dont have any issues with that
            for (int i = 0; i < ec.npcsToSpawn.Count; i++)
            {
                if (ec.npcsToSpawn[i].Character == Character.Principal)
                {
                    if (!toRevert.ContainsKey(ec.npcsToSpawn[i]))
                    {
                        toRevert.Add(ec.npcsToSpawn[i], (bool)_ignorePlayerOnSpawn.GetValue(ec.npcsToSpawn[i]));
                    }
                    _ignorePlayerOnSpawn.SetValue(ec.npcsToSpawn[i], true);
                }
            }
            base.ExitedSpawn();
            // change it back
            for (int i = 0; i < ec.npcsToSpawn.Count; i++)
            {
                if (ec.npcsToSpawn[i].Character == Character.Principal)
                {
                    _ignorePlayerOnSpawn.SetValue(ec.npcsToSpawn[i], toRevert[ec.npcsToSpawn[i]]);
                }
            }
            foreach (NPC npc in ec.Npcs)
            {
                if ((npc.Character == Character.Principal) && (npc is Principal))
                {
                    _allKnowing.SetValue(npc, true);
                }
            }
            ec.StartEventTimers();
        }

        static FieldInfo _chalkEraser = AccessTools.Field(typeof(StealthyChallengeManager), "chalkEraser");
        public override void Initialize()
        {
            if (!giveChalkErasers)
            {
                _chalkEraser.SetValue(this, ItemMetaStorage.Instance.FindByEnum(Items.None).value);
            }
            base.Initialize();
        }

        public override void GiveRandomSticker(StickerPackType packType, int total)
        {
            if (packType == StickerPackType.Bonus)
            {
                if (Singleton<CoreGameManager>.Instance.sceneObject.potentialStickers.Where(x => StickerMetaStorage.Instance.Get(x.selection).flags.HasFlag(StickerFlags.IsBonus)).Count() == 0)
                {
                    base.GiveRandomSticker(StickerPackType.Normal, 1);
                    return;
                }
            }
            base.GiveRandomSticker(packType, total);
        }
    }

    public class EditorSpeedyChallengeManager : SpeedyChallengeManager, IStudioLegacyKnowledgable
    {
        public StudioLevelLegacyFlags legacyFlags { get; set; } = StudioLevelLegacyFlags.None;

        public override void ExitedSpawn()
        {
            if (legacyFlags.HasFlag(StudioLevelLegacyFlags.BeforeNPCCustom))
            {
                // hack for old levels that were made before customizable baldi speed was implemented
                for (int i = 0; i < ec.npcsToSpawn.Count; i++)
                {
                    if (ec.npcsToSpawn[i].Character == Character.Baldi)
                    {
                        ec.npcsToSpawn[i] = NPCMetaStorage.Instance.Get(Character.Baldi).prefabs["FastBaldi"];
                    }
                }
            }
            base.ExitedSpawn();
            ec.StartEventTimers();
        }
        public override void LoadNextLevel()
        {
            Singleton<EditorPlayModeManager>.Instance.Win();
        }

        public override void GiveRandomSticker(StickerPackType packType, int total)
        {
            if (packType == StickerPackType.Bonus)
            {
                if (Singleton<CoreGameManager>.Instance.sceneObject.potentialStickers.Where(x => StickerMetaStorage.Instance.Get(x.selection).flags.HasFlag(StickerFlags.IsBonus)).Count() == 0)
                {
                    base.GiveRandomSticker(StickerPackType.Normal, 1);
                    return;
                }
            }
            base.GiveRandomSticker(packType, total);
        }
    }
}
