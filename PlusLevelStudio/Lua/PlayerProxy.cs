using MoonSharp.Interpreter;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Lua
{
    [MoonSharpUserData]
    public class PlayerProxy
    {
        [MoonSharpHidden]
        PlayerManager pm;

        MovementModifier moveMod;

        public float moveSpeedMultiplier
        {
            get
            {
                return moveMod.movementMultiplier;
            }
            set
            {
                moveMod.movementMultiplier = value;
            }
        }

        public int points
        {
            get
            {
                return Singleton<CoreGameManager>.Instance.GetPoints(pm.playerNumber);
            }
            set
            {
                int currentPoints = Singleton<CoreGameManager>.Instance.GetPoints(pm.playerNumber);
                Singleton<CoreGameManager>.Instance.AddPoints(value - currentPoints, pm.playerNumber, true);
            }
        }

        public Vector3Proxy position
        {
            get
            {
                return new Vector3Proxy(pm.transform.position);
            }
            set
            {
                pm.plm.Entity.Teleport(value.ToVector());
            }
        }

        public int slotCount
        {
            get
            {
                return pm.itm.maxItem + 1;
            }
            set
            {
                SetSlotCount(value);
            }
        }

        public PlayerProxy(PlayerManager pm)
        {
            this.pm = pm;
            moveMod = new MovementModifier(Vector3.zero, 1f);
            moveMod.ignoreGrounded = false;
            moveMod.ignoreAirborne = false;
            pm.plm.Entity.ExternalActivity.moveMods.Add(moveMod);
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

        public bool HasItem(string itemId)
        {
            if (!LevelLoaderPlugin.Instance.itemObjects.ContainsKey(itemId)) return false;
            return pm.itm.Has(LevelLoaderPlugin.Instance.itemObjects[itemId].itemType);
        }

        public void RemoveItemOfID(string itemId)
        {
            if (!LevelLoaderPlugin.Instance.itemObjects.ContainsKey(itemId)) return;
            pm.itm.Remove(LevelLoaderPlugin.Instance.itemObjects[itemId].itemType);
        }

        public void RemoveItem(int slot)
        {
            pm.itm.RemoveItem(slot - 1);
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
            return LuaHelpers.GetIDFromItemObject(pm.itm.items[slot]);
        }

        public void RemoveItemSlot(int slot)
        {
            pm.itm.RemoveItemSlot(slot - 1);
        }

        public void SetSlotCount(int count)
        {
            for (int i = 0; i < pm.itm.items.Length; i++)
            {
                if (i >= count)
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
}
