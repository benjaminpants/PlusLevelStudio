using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using MoonSharp.Interpreter;
using PlusLevelStudio.Editor;
using PlusStudioLevelLoader;
using UnityEngine;
using System.IO.Compression;
using PlusStudioLevelFormat;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI;
using UnityEngine.Rendering;

namespace PlusLevelStudio.Lua
{
    public class CustomChallengeGameModeSettings : EditorGameModeSettings
    {
        public string luaScript = string.Empty;
        public string fileName = string.Empty;
        public override void ApplySettingsToManager(BaseGameManager manager)
        {
            ((CustomChallengeManager)manager).luaScript = luaScript;
        }

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            if (version == 0)
            {
                fileName = string.Empty;
                luaScript = reader.ReadString();
                return;
            }
            fileName = reader.ReadString();
            if (version == 1)
            {
                luaScript = reader.ReadString();
                return;
            }
            int bytesToRead = reader.ReadInt32();
            // thanks to https://gist.github.com/GoSato/aff1ffd60e0cf2bb3db7615e56ce6c9a
            byte[] source = reader.ReadBytes(bytesToRead);
            using (MemoryStream input = new MemoryStream(source))
            using (MemoryStream output = new MemoryStream())
            using (DeflateStream decompressedDstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                decompressedDstream.CopyTo(output);

                byte[] destination = output.ToArray();

                luaScript = Encoding.UTF8.GetString(destination);
            }
        }

        const byte version = 2;
        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(fileName);
            byte[] source = Encoding.UTF8.GetBytes(luaScript);
            // thanks to https://gist.github.com/GoSato/aff1ffd60e0cf2bb3db7615e56ce6c9a
            using (MemoryStream ms = new MemoryStream())
            using (DeflateStream compressedDStream = new DeflateStream(ms, CompressionMode.Compress, true))
            {
                compressedDStream.Write(source, 0, source.Length);
                compressedDStream.Close();
                byte[] destination = ms.ToArray();
                writer.Write(destination.Length);
                writer.Write(destination);
            }
        }
    }

    public class CustomChallengeManager : BaseGameManager
    {
        public string luaScript;
        public Script script;
        public EditorLuaGameProxy myProxy;
        public TimeScaleModifier timeScaleModifier;
        DynValue updateFunction;
        System.Random myRandom;
        bool globalsDefined = false;

        Vector3Proxy CreateVector(float x, float y, float z)
        {
            return new Vector3Proxy(x, y, z);
        }

        IntVector2Proxy CreateIntVector(int x, int z)
        {
            return new IntVector2Proxy(x, z);
        }

        ColorProxy CreateColor(int r, int g, int b)
        {
            return new ColorProxy(r, g, b);
        }

        double RandomNumber(double min, double max)
        {
            if (min > max) throw new InvalidDataException("Min can't be greater than max!");
            double range = (max - min);
            return min + (myRandom.NextDouble() * range);
        }

        public void InitializeScriptGlobals()
        {
            myProxy = new EditorLuaGameProxy { myManager = this };
            script.Globals["self"] = myProxy;
            script.Globals["Vector3"] = (Func<float, float, float, Vector3Proxy>)CreateVector;
            script.Globals["IntVector2"] = (Func<int, int, IntVector2Proxy>)CreateIntVector;
            script.Globals["Color"] = (Func<int, int, int, ColorProxy>)CreateColor;
            script.Globals["RandomDecimalNumber"] = (Func<double, double, double>)RandomNumber;
            globalsDefined = true;
        }

        public void SetNotebookAngerValue(float val)
        {
            notebookAngerVal = val;
        }

        void Print(string text)
        {
            Debug.Log("(Lua) " + text);
        }

        public override void Initialize()
        {
            base.Initialize();
            myRandom = new System.Random();
            timeScaleModifier = new TimeScaleModifier();
            ec.AddTimeScale(timeScaleModifier);
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

        IEnumerator WaitForNPCsToFinishSpawning()
        {
            while (ec.npcsLeftToSpawn.Count > 0)
            {
                yield return null;
            }
            OnNPCsDoneSpawning();
            yield break;
        }

        bool waitedForInitialSpawn = false;
        public void BeginNPCSpawnWait()
        {
            if (waitedForInitialSpawn) return;
            StartCoroutine(WaitForNPCsToFinishSpawning());
            waitedForInitialSpawn = true;
        }

        public void OnNPCsDoneSpawning()
        {
            if (!globalsDefined)
            {
                return;
            }
            if (script.Globals.Get("AllNPCsSpawned").Type == DataType.Function)
            {
                script.Call(script.Globals["AllNPCsSpawned"]);
            }
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

        public bool OnUseItem(ItemManager manager, ItemObject attempted, int slot)
        {
            if (!globalsDefined) return true;
            string itemId;
            if (attempted == manager.nothing)
            {
                itemId = "nothing";
            }
            else
            {
                itemId = LuaHelpers.GetIDFromItemObject(attempted);
            }
            if (script.Globals.Get("OnItemUse").Type != DataType.Function) return true;
            DynValue shouldUse = script.Call(script.Globals["OnItemUse"], itemId, slot + 1);
            if (shouldUse.Type == DataType.Boolean)
            {
                return shouldUse.Boolean;
            }
            return true;
        }

        protected override void AllNotebooks()
        {
            if (!globalsDefined)
            {
                base.AllNotebooks();
                return;
            }
            allNotebooksFound = true;
            script.Call(script.Globals["AllNotebooks"]);
        }

        public override void NoiseMade(EnvironmentController ec, Vector3 position, int value)
        {
            base.NoiseMade(ec, position, value);
            if (!globalsDefined) return;
            if (script.Globals.Get("NoiseMade").Type != DataType.Function) return;
            script.Call(script.Globals["NoiseMade"], new Vector3Proxy(position), value);
        }

        bool finalExitsTriggered = false;

        public void ActivateExits(bool performEscape)
        {
            if (finalExitsTriggered) return;
            finalExitsTriggered = true;
            ec.SetElevators(true);
            if (!performEscape)
            {
                foreach (Elevator elevator in ec.elevators)
                {
                    elevator.PrepareForExit();
                    elevator.InsideCollider.Enable(true);
                    StartCoroutine(EnterExit(elevator));
                }
                return;
            }
            elevatorsToClose = ec.elevators.Count - 1;
            foreach (Elevator elevator in ec.elevators)
            {
                if (ec.elevators.Count > 1)
                {
                    elevator.PrepareToClose();
                }

                StartCoroutine(ReturnSpawnFinal(elevator));
            }
        }

        public override void CollectNotebook(Notebook notebook)
        {
            base.CollectNotebook(notebook);
            if (!globalsDefined) return;
            if (script.Globals.Get("NotebookCollected").Type != DataType.Function) return;
            script.Call(script.Globals["NotebookCollected"], new Vector3Proxy(notebook.transform.position));
        }


        public void ActivateBonusProblems(bool includeLast)
        {
            foreach (Activity activity in ec.activities)
            {
                if ((activity != lastActivity) || includeLast)
                {
                    activity.Corrupt(false);
                    activity.SetBonusMode(true);
                }
            }
        }

        // returns if the original should run
        private bool GiveRandomStickerLua(StickerPackType packType, int total)
        {
            if (!globalsDefined) return true;
            if (script.Globals.Get("OnGiveRandomSticker").Type != DataType.Function) return true;
            DynValue result = script.Call(script.Globals["OnGiveRandomSticker"], packType.ToStringExtended(), total);
            if (result.Type != DataType.Boolean) return false;
            if (!result.Boolean) return false;
            return true;
        }

        public override void GiveRandomSticker(StickerPackType packType, int total)
        {
            if (GiveRandomStickerLua(packType, total))
            {
                GiveRandomStickerSafer(packType, total);
            }
        }

        public void GiveRandomStickerSafer(StickerPackType packType, int total)
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

        void Update()
        {
            if (!globalsDefined) return;
            script.Call(updateFunction, Time.deltaTime);
        }

        public override void LoadNextLevel()
        {
            if (!globalsDefined)
            {
                Singleton<EditorPlayModeManager>.Instance.Win();
                return;
            }
            if (script.Globals.Get("OnLevelCompleted").Type != DataType.Function) return;
            DynValue returnV = script.Call(script.Globals["OnLevelCompleted"]);
            if (returnV.Type != DataType.String)
            {
                Singleton<EditorPlayModeManager>.Instance.Win();
                return;
            }
            Singleton<EditorPlayModeManager>.Instance.Win(returnV.String);
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

        public int notebookCount
        {
            get
            {
                return myManager.FoundNotebooks;
            }
        }

        public int totalNotebooks
        {
            get
            {
                return myManager.NotebookTotal;
            }
        }

        public float GetNPCTimeScale()
        {
            return myManager.Ec.NpcTimeScale;
        }

        public float GetEnvironmentTimeScale()
        {
            return myManager.Ec.EnvironmentTimeScale;
        }

        public float GetPlayerTimeScale()
        {
            return myManager.Ec.PlayerTimeScale;
        }

        public void OpenExits(bool doEscape)
        {
            myManager.ActivateExits(doEscape);
        }

        public void ActivateBonusProblems(bool includeLast)
        {
            myManager.ActivateBonusProblems(includeLast);
        }

        public List<LightProxy> GetAllLights()
        {
            return myManager.Ec.lights.Select(x => new LightProxy(x)).ToList();
        }

        public List<RoomProxy> GetAllRooms()
        {
            return myManager.Ec.rooms.Select(x => new RoomProxy(x)).ToList();
        }

        public void ForceLose()
        {
            if (Singleton<CoreGameManager>.Instance.disablePause) return;
            Baldi baldi = myManager.Ec.GetBaldi();
            if (baldi == null)
            {
                baldi = (Baldi)myManager.Ec.SpawnNPC(LevelLoaderPlugin.Instance.npcAliases["baldi"], myManager.Ec.CellFromPosition(Singleton<CoreGameManager>.Instance.GetPlayer(0).transform.position).position);
            }
            Singleton<CoreGameManager>.Instance.EndGame(Singleton<CoreGameManager>.Instance.GetPlayer(0).transform, baldi);
        }

        public void ForceWin(string text)
        {
            if (text == null)
            {
                myManager.LoadNextLevel();
            }
            else
            {
                Singleton<EditorPlayModeManager>.Instance.Win(text);
            }
        }

        public void GiveRandomSticker(string packTypeString, int total)
        {
            if (string.IsNullOrEmpty(packTypeString)) return;
            EnumExtensions.GetFromExtendedNameSafe(packTypeString, out StickerPackType? pack);
            if (!pack.HasValue) return;
            myManager.GiveRandomStickerSafer(pack.Value, total);
        }

        public CellProxy GetRandomEntitySafeCell()
        {
            Cell randomCell = myManager.Ec.RandomCell(false, false, true);
            if (randomCell == null) return null;
            return new CellProxy(randomCell);
        }

        public float npcTimeScaleMod
        {
            get
            {
                return myManager.timeScaleModifier.npcTimeScale;
            }
            set
            {
                myManager.timeScaleModifier.npcTimeScale = value;
            }
        }

        public float environmentTimeScaleMod
        {
            get
            {
                return myManager.timeScaleModifier.environmentTimeScale;
            }
            set
            {
                myManager.timeScaleModifier.environmentTimeScale = value;
            }
        }

        public float playerTimeScaleMod
        {
            get
            {
                return myManager.timeScaleModifier.playerTimeScale;
            }
            set
            {
                myManager.timeScaleModifier.playerTimeScale = value;
            }
        }

        public void SpawnNPCs()
        {
            myManager.Ec.SpawnNPCs();
            myManager.BeginNPCSpawnWait();
        }

        public void StartEventTimers()
        {
            if (myManager.Ec.EventsStarted) return;
            myManager.Ec.StartEventTimers();
        }

        private static FieldInfo _eventTime = AccessTools.Field(typeof(RandomEvent), "eventTime");
        private static MethodInfo _EventTimer = AccessTools.Method(typeof(EnvironmentController), "EventTimer");
        public void StartEvent(string eventId, float length, bool doJingle)
        {
            if (!LevelLoaderPlugin.Instance.randomEventAliases.ContainsKey(eventId)) return;
            System.Random controlledRandom = new System.Random();
            RandomEvent newEvent = GameObject.Instantiate<RandomEvent>(LevelLoaderPlugin.Instance.randomEventAliases[eventId], myManager.Ec.transform);
            newEvent.Initialize(myManager.Ec, controlledRandom);
            newEvent.PremadeSetup();
            if (length <= 0f)
            {
                newEvent.SetEventTime(controlledRandom);
            }
            else
            {
                _eventTime.SetValue(newEvent, length);
            }
            if (doJingle)
            {
                IEnumerator numberator = (IEnumerator)_EventTimer.Invoke(myManager.Ec, new object[] { newEvent, 3f, newEvent.Type == RandomEventType.TimeOut });
                myManager.Ec.StartCoroutine(numberator);
            }
            else
            {
                newEvent.Begin();
            }
        }

        private Dictionary<NPC, NPCProxy> proxies = new Dictionary<NPC, NPCProxy>();

        private NPCProxy GetProxyForNPC(NPC npc)
        {
            if (proxies.ContainsKey(npc))
            {
                return proxies[npc];
            }
            NPCProxy proxy;
            if (npc is Baldi)
            {
                proxy = new BaldiProxy((Baldi)npc);
                proxies.Add(npc, proxy);
                return proxy;
            }
            proxy = new NPCProxy(npc);
            proxies.Add(npc, proxy);
            return proxy;
        }

        public void MakeNoise(Vector3Proxy position, int noiseValue)
        {
            myManager.Ec.MakeNoise(position.ToVector(),noiseValue);
        }

        public NPCProxy GetNPC(string npcId)
        {
            List<NPC> allNPCs = myManager.Ec.Npcs;
            for (int i = 0; i < allNPCs.Count; i++)
            {
                if (LevelLoaderPlugin.Instance.npcAliases[npcId].name == allNPCs[i].name.Replace("(Clone)", ""))
                {
                    return GetProxyForNPC(allNPCs[i]);
                }
                if (LevelLoaderPlugin.Instance.npcAliases[npcId].Character == allNPCs[i].Character)
                {
                    return GetProxyForNPC(allNPCs[i]);
                }
            }
            return null;
        }

        public void PlaySoundObject(string sound)
        {
            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(LevelStudioPlugin.Instance.sounds[sound]);
        }

        public void SpawnItemPickup(Vector3Proxy position, string itemId)
        {
            ItemObject obj = LevelLoaderPlugin.Instance.itemObjects[itemId];
            Pickup pickup = myManager.Ec.CreateItem(myManager.Ec.CellFromPosition(position.ToVector()).room, obj, position.ToVector2YAsZ());
            pickup.icon = myManager.Ec.map.AddIcon(pickup.iconPre, pickup.transform, Color.white);
        }

        public CellProxy CellFromPosition(Vector3Proxy proxy)
        {
            Cell cell = myManager.Ec.CellFromPosition(proxy.ToVector());
            if (cell == null) return null;
            return new CellProxy(cell);
        }

        public CellProxy CellFromPosition(IntVector2Proxy proxy)
        {
            Cell cell = myManager.Ec.CellFromPosition(proxy.ToVector());
            if (cell == null) return null;
            return new CellProxy(cell);
        }

        public NPCProxy SpawnNPC(string type, Vector3Proxy position)
        {
            if (!LevelLoaderPlugin.Instance.npcAliases.ContainsKey(type)) return null;
            NPC npc = myManager.Ec.SpawnNPC(LevelLoaderPlugin.Instance.npcAliases[type], myManager.Ec.CellFromPosition(position.ToVector()).position);
            npc.transform.localPosition = position.ToVector();
            return GetProxyForNPC(npc);
        }

        public List<NPCProxy> GetNPCs()
        {
            List<NPC> allNPCs = myManager.Ec.Npcs;
            List<NPCProxy> returnProxies = new List<NPCProxy>();
            for (int i = 0; i < allNPCs.Count; i++)
            {
                returnProxies.Add(GetProxyForNPC(allNPCs[i]));
            }
            return returnProxies;
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
            if (proxies.TryGetValue(myManager.Ec.GetBaldi(), out NPCProxy outBald))
            {
                baldi = (BaldiProxy)outBald;
            }
            else
            {
                baldi = new BaldiProxy(myManager.Ec.GetBaldi());
                proxies.Add(baldi.npc, baldi);
            }
            return baldi;
        }
    }
}
