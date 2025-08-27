using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using PlusStudioLevelLoader;
using UnityEngine;

namespace PlusLevelStudio.Lua
{
    [MoonSharpUserData]
    public class NPCProxy
    {
        [MoonSharpHidden]
        public NPC npc;

        public override string ToString()
        {
            return id + "," + npc.name;
        }

        public Vector3Proxy position
        {
            get
            {
                return new Vector3Proxy(npc.transform.position);
            }
            set
            {
                Entity npcEnt = npc.GetComponent<Entity>();
                if (npcEnt == null)
                {
                    npc.transform.position = value.ToVector();
                    return;
                }
                npcEnt.Teleport(value.ToVector());
            }
        }

        public float direction
        {
            get
            {
                return npc.transform.eulerAngles.y;
            }
            set
            {
                npc.transform.eulerAngles = new Vector3(npc.transform.eulerAngles.x, value, npc.transform.eulerAngles.z);
            }
        }

        public string objectName
        {
            get
            {
                return npc.name;
            }
            set
            {
                npc.name = value;
            }
        }

        public Vector3Proxy GetForward()
        {
            return new Vector3Proxy(npc.transform.forward);
        }

        public void AddArrow(int r, int g, int b)
        {
            Entity npcEnt = npc.GetComponent<Entity>();
            if (npcEnt == null) return;
            npc.ec.map.AddArrow(npcEnt, new Color(r / 255f, g / 255f, b / 255f));
        }

        public bool IsHidden()
        {
            Entity npcEnt = npc.GetComponent<Entity>();
            if (npcEnt == null) return false;
            return npcEnt.Hidden;
        }

        public bool squished
        {
            get
            {
                Entity npcEnt = npc.GetComponent<Entity>();
                if (npcEnt == null) return false;
                return npcEnt.Squished;
            }
            set
            {
                if (value)
                {
                    Squish(float.MaxValue);
                }
                else
                {
                    Unsquish();
                }
            }
        }

        public void Squish(float time)
        {
            Entity npcEnt = npc.GetComponent<Entity>();
            if (npcEnt == null) return;
            npcEnt.Squish(time);
        }

        public void Unsquish()
        {
            Entity npcEnt = npc.GetComponent<Entity>();
            if (npcEnt == null) return;
            npcEnt.Unsquish();
        }

        public string id { get; private set; }

        MovementModifier moveMod;

        public float moveSpeedMultiplier
        {
            get
            {
                if (moveMod == null)
                {
                    return 1f;
                }
                return moveMod.movementMultiplier;
            }
            set
            {
                if (moveMod == null)
                {
                    Entity npcEntity = npc.GetComponent<Entity>();
                    if (npcEntity == null) return;
                    moveMod = new MovementModifier(Vector3.zero, value);
                    moveMod.ignoreAirborne = false;
                    moveMod.ignoreGrounded = false;
                    npc.GetComponent<Entity>().ExternalActivity.moveMods.Add(moveMod);
                }
                moveMod.movementMultiplier = value;
            }
        }

        public NPCProxy(NPC npc)
        {
            this.npc = npc;
            id = LuaHelpers.GetIDFromNPC(npc);
        }
    }

    [MoonSharpUserData]
    public class BaldiProxy : NPCProxy
    {

        private Baldi baldi => (Baldi)npc;
        public BaldiProxy(Baldi baldi) : base(baldi)
        {
        }

        public void AddAnger(float amount)
        {
            baldi.GetAngry(amount);
        }

        public void SetAnger(float amount)
        {
            baldi.SetAnger(amount);
        }

        public void Praise(float time)
        {
            baldi.Praise(time);
        }
    }
}
