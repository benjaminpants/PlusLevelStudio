using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PlusLevelStudio.Ingame
{
    public class EditorGrappleChallengeManager : GrappleChallengeManager
    {
        public override void LoadNextLevel()
        {
            Singleton<CoreGameManager>.Instance.Quit();
        }
    }

    public class EditorStealthyChallengeManager : StealthyChallengeManager
    {
        public bool giveChalkErasers = true;
        static FieldInfo _allKnowing = AccessTools.Field(typeof(Principal), "allKnowing");
        public override void LoadNextLevel()
        {
            Singleton<CoreGameManager>.Instance.Quit();
        }

        protected override void ExitedSpawn()
        {
            base.ExitedSpawn();
            foreach (NPC npc in this.ec.Npcs)
            {
                if (npc is Principal)
                {
                    _allKnowing.SetValue(npc, true);
                }
            }
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
    }

    public class EditorSpeedyChallengeManager : SpeedyChallengeManager
    {
        protected override void ExitedSpawn()
        {
            // hack until customizable baldi speeds are implemented
            for (int i = 0; i < ec.npcsToSpawn.Count; i++)
            {
                if (ec.npcsToSpawn[i].Character == Character.Baldi)
                {
                    ec.npcsToSpawn[i] = NPCMetaStorage.Instance.Get(Character.Baldi).prefabs["FastBaldi"];
                }
            }
            base.ExitedSpawn();
        }
        public override void LoadNextLevel()
        {
            Singleton<CoreGameManager>.Instance.Quit();
        }
    }
}
