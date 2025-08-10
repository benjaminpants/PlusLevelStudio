using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MoonSharp.Interpreter;
using PlusLevelStudio.Editor;
using UnityEngine;
using static UnityEngine.LowLevel.PlayerLoopSystem;

namespace PlusLevelStudio.Lua
{
    public class CustomChallengeGameModeSettings : EditorGameModeSettings
    {
        public string luaScript;
        public override void ApplySettingsToManager(BaseGameManager manager)
        {
            ((CustomChallengeManager)manager).luaScript = luaScript;
        }

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            luaScript = reader.ReadString();
        }

        const byte version = 0;
        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(luaScript);
        }
    }

    public class CustomChallengeManager : BaseGameManager
    {
        public string luaScript;
        public Script script;
        public EditorLuaGameProxy myProxy;
        DynValue updateFunction;
        bool globalsDefined = false;

        public void InitializeScriptGlobals()
        {
            myProxy = new EditorLuaGameProxy { myManager = this };
            script.Globals["self"] = myProxy;
            globalsDefined = true;
        }

        public void SetNotebookAngerValue(float val)
        {
            notebookAngerVal = val;
        }

        void Print(string text)
        {
            Debug.Log("Lua: " + text);
        }

        public override void Initialize()
        {
            base.Initialize();
            PlayerManager pm = Singleton<CoreGameManager>.Instance.GetPlayer(0);
            script = new Script(CoreModules.Preset_HardSandbox);
            script.Options.DebugPrint = Print;
            script.DoString(luaScript);
            DynValue ppDyn = script.Call(script.Globals["SetupPlayerProperties"]);
            if (ppDyn.Type == DataType.Table)
            {
                Table propertyTable = ppDyn.Table;
                if (propertyTable.Get("walkSpeed").Type == DataType.Number)
                {
                    pm.plm.walkSpeed = (float)propertyTable.Get("walkSpeed").Number;
                }
                if (propertyTable.Get("runSpeed").Type == DataType.Number)
                {
                    pm.plm.runSpeed = (float)propertyTable.Get("runSpeed").Number;
                }
                if (propertyTable.Get("staminaDrop").Type == DataType.Number)
                {
                    pm.plm.staminaDrop = (float)propertyTable.Get("staminaDrop").Number;
                }
                if (propertyTable.Get("staminaMax").Type == DataType.Number)
                {
                    pm.plm.staminaMax = (float)propertyTable.Get("staminaMax").Number;
                }
                if (propertyTable.Get("staminaRise").Type == DataType.Number)
                {
                    pm.plm.staminaRise = (float)propertyTable.Get("staminaRise").Number;
                }
            }
            ec.map.CompleteMap();
            updateFunction = script.Globals.Get("Update");
            InitializeScriptGlobals(); // initialize these after the initial stats have been decided
            script.Globals["player"] = new PlayerProxy(pm);
            script.Call(script.Globals["Initialize"]);
        }

        protected override void ExitedSpawn()
        {
            base.ExitedSpawn();
            script.Call(script.Globals["ExitedSpawn"]);
        }

        public override void AngerBaldi(float val)
        {
            if (!globalsDefined)
            {
                base.AngerBaldi(val);
                return;
            }
            if (script.Globals.Get("AngerBaldi").Type != DataType.Function)
            {
                base.AngerBaldi(val);
                return;
            }
            DynValue returnVal = script.Call(script.Globals["AngerBaldi"], val);
            if (returnVal.Type == DataType.Number)
            {
                base.AngerBaldi((float)returnVal.Number);
                return;
            }
            base.AngerBaldi(0);
        }

        public override void CollectNotebooks(int count)
        {
            base.CollectNotebooks(count);
            if (!globalsDefined) return;
            if (script.Globals.Get("NotebookCollected").Type != DataType.Function) return;
            for (int i = 0; i < count; i++)
            {
                script.Call(script.Globals["NotebookCollected"]);
            }
        }

        void Update()
        {
            if (!globalsDefined) return;
            script.Call(updateFunction, Time.deltaTime);
        }

        public override void LoadNextLevel()
        {
            Singleton<CoreGameManager>.Instance.Quit();
        }
    }

    [MoonSharpUserData]
    public class EditorLuaGameProxy
    {
        [MoonSharpHidden]
        public CustomChallengeManager myManager;

        private BaldiProxy baldi;


        public float notebookAngerVal
        {
            get
            {
                return myManager.NotebookAngerVal;
            }
            set
            {
                myManager.SetNotebookAngerValue(value);
            }
        }

        public void SpawnNPCs()
        {
            myManager.Ec.SpawnNPCs();
        }

        public void StartEventTimers()
        {
            if (myManager.Ec.EventsStarted) return;
            myManager.Ec.StartEventTimers();
        }

        public BaldiProxy GetBaldi()
        {
            if (baldi != null)
            {
                if (baldi.npc != null)
                {
                    return baldi;
                }    
            }
            if (myManager.Ec.GetBaldi() == null) return null;
            baldi = new BaldiProxy(myManager.Ec.GetBaldi());
            return baldi;
        }
    }
}
