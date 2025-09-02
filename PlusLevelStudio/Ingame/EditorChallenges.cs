﻿using HarmonyLib;
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
            Singleton<EditorPlayModeManager>.Instance.Win();
        }

        protected override void ExitedSpawn()
        {
            base.ExitedSpawn();
            ec.StartEventTimers();
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
        protected override void ExitedSpawn()
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
            ec.StartEventTimers();
        }
        public override void LoadNextLevel()
        {
            Singleton<EditorPlayModeManager>.Instance.Win();
        }
    }
}
