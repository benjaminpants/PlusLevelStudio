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
    public class PlayerProxy
    {
        [MoonSharpHidden]
        PlayerManager pm;

        public PlayerProxy(PlayerManager pm)
        {
            this.pm = pm;
        }

        public void MakeGuilty(string rule, float time)
        {
            pm.RuleBreak(rule, time);
        }

        public void SetItem(string itemId, int slot)
        {
            if (!LevelLoaderPlugin.Instance.itemObjects.ContainsKey(itemId)) return;
            pm.itm.SetItem(LevelLoaderPlugin.Instance.itemObjects[itemId], slot - 1);
        }

        public void AddItem(string itemId)
        {
            if (!LevelLoaderPlugin.Instance.itemObjects.ContainsKey(itemId)) return;
            pm.itm.AddItem(LevelLoaderPlugin.Instance.itemObjects[itemId]);
        }

        public string GetItem(int slot)
        {
            slot -= 1;
            if (slot > pm.itm.maxItem) return "none";
            if (slot < 0) return "none";
            foreach (KeyValuePair<string, ItemObject> kvp in LevelLoaderPlugin.Instance.itemObjects)
            {
                if (kvp.Value == pm.itm.items[slot])
                {
                    return kvp.Key;
                }
                if (kvp.Value.itemType == pm.itm.items[slot].itemType)
                {
                    return kvp.Key;
                }
            }
            return "unknown";
        }

        public void RemoveItemSlot(int slot)
        {
            pm.itm.RemoveItemSlot(slot - 1);
        }

        public void SetSlotCount(int count)
        {
            for (int i = 0; i < pm.itm.items.Length; i++)
            {
                if (i > count)
                {
                    pm.itm.items[i] = pm.itm.nothing;
                }
            }
            pm.itm.maxItem = Mathf.Max(count - 1, -1);
            Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).UpdateInventorySize(pm.itm.maxItem + 1);
            pm.itm.selectedItem = Mathf.Min(pm.itm.selectedItem, Mathf.Max(pm.itm.maxItem, 0));
            pm.itm.UpdateSelect();
        }

        public void LockItemSlot(int slot)
        {
            pm.itm.LockSlot(slot - 1, true);
        }
        public void UnlockItemSlot(int slot)
        {
            pm.itm.LockSlot(slot - 1, false);
        }

        public void UseItem(string itemId)
        {
            if (!LevelLoaderPlugin.Instance.itemObjects.ContainsKey(itemId)) return;
            ItemObject toUse = LevelLoaderPlugin.Instance.itemObjects[itemId];
            Item item = GameObject.Instantiate<Item>(toUse.item);
            item.Use(pm);
        }
    }

    [MoonSharpUserData]
    public class NPCProxy
    {
        [MoonSharpHidden]
        public NPC npc;


        public void AddArrow(int r, int g, int b)
        {
            Entity npcEnt = npc.GetComponent<Entity>();
            if (npcEnt == null) return;
            npc.ec.map.AddArrow(npcEnt, new Color(r / 255f, g / 255f, b / 255f));
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
            foreach (KeyValuePair<string, NPC> kvp in LevelLoaderPlugin.Instance.npcAliases)
            {
                if (kvp.Value.name == npc.name.Replace("(Clone)", ""))
                {
                    id = kvp.Key;
                    return;
                }
            }
            foreach (KeyValuePair<string, NPC> kvp in LevelLoaderPlugin.Instance.npcAliases)
            {
                if (kvp.Value.Character == npc.Character)
                {
                    id = kvp.Key;
                    return;
                }
            }
            id = "unknown";
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
    }
}
