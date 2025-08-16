﻿using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PlusLevelStudio.Ingame
{
    public class EditorMainGameManager : MainGameManager
    {
        protected bool didNoBaldiHack = false;
        public bool baldiGoesToHappyBaldi = true;
        protected Mode originalMode;
        public override void Initialize()
        {
            base.Initialize();
            if (ec.npcsToSpawn.Where(x => x.Character == Character.Baldi).Count() == 0)
            {
                didNoBaldiHack = true;
                Singleton<CoreGameManager>.Instance.currentMode = Mode.Free;
                ec.npcsToSpawn.Add(NPCMetaStorage.Instance.Get(Character.Baldi).value);
                //ec.npcSpawnTile = ec.npcSpawnTile.AddToArray(ec.RandomCell(false,false,true));
            }
        }

        public override void BeginSpoopMode()
        {
            base.BeginSpoopMode();
            if (didNoBaldiHack)
            {
                ec.npcSpawnTile[ec.npcSpawnTile.Length - 1] = ec.RandomCell(false, false, true); // hack!
            }
            if (!baldiGoesToHappyBaldi)
            {
                originalMode = Singleton<CoreGameManager>.Instance.currentMode;
                Singleton<CoreGameManager>.Instance.currentMode = (Mode)(-1); // mess with the mode temporarily to prevent the teleport, because for some reason mystman12 checks if the mode is SPECIFICALLY main
                StartCoroutine(FixMode());
            }
        }

        IEnumerator FixMode()
        {
            yield return null;
            Singleton<CoreGameManager>.Instance.currentMode = originalMode;
        }

        public override void LoadNextLevel()
        {
            Singleton<EditorPlayModeManager>.Instance.Win();
        }
    }
}