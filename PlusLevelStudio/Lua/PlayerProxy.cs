using MoonSharp.Interpreter;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using MTM101BaldAPI;
using MTM101BaldAPI.PlusExtensions;
using MTM101BaldAPI.Registers;

namespace PlusLevelStudio.Lua
{
    [MoonSharpUserData]
    public class PlayerProxy
    {
        PlayerManager pm;

        MovementModifier moveMod;
        PlayerMovementStatModifier statMod;

        public float direction
        {
            get
            {
                return pm.plm.transform.eulerAngles.y;
            }
            set
            {
                pm.plm.transform.eulerAngles = new Vector3(pm.plm.transform.eulerAngles.x, value, pm.plm.transform.eulerAngles.z);
            }
        }

        public Vector3Proxy GetForward()
        {
            return new Vector3Proxy(pm.plm.transform.forward);
        }

        public bool squished
        {
            get
            {
                return pm.plm.Entity.Squished;
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

        public bool IsHidden()
        {
            return pm.Invisible;
        }

        public void Squish(float time)
        {
            pm.plm.Entity.Squish(time);
        }

        public void Unsquish()
        {
            pm.plm.Entity.Unsquish();
        }

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

        public float baseWalkSpeed
        {
            get
            {
                return statMod.baseStats["walkSpeed"];
            }
            set
            {
                statMod.ChangeBaseStat("walkSpeed", value);
            }
        }

        public float baseRunSpeed
        {
            get
            {
                return statMod.baseStats["runSpeed"];
            }
            set
            {
                statMod.ChangeBaseStat("runSpeed", value);
            }
        }

        public float baseStaminaDrop
        {
            get
            {
                return statMod.baseStats["staminaDrop"];
            }
            set
            {
                statMod.ChangeBaseStat("staminaDrop", value);
            }
        }

        public float baseStaminaMax
        {
            get
            {
                return statMod.baseStats["staminaMax"];
            }
            set
            {
                statMod.ChangeBaseStat("staminaMax", value);
            }
        }

        public float baseStaminaRise
        {
            get
            {
                return statMod.baseStats["staminaRise"];
            }
            set
            {
                statMod.ChangeBaseStat("staminaRise", value);
            }
        }

        public float walkSpeed
        {
            get
            {
                return pm.plm.walkSpeed;
            }
        }

        public float runSpeed
        {
            get
            {
                return pm.plm.runSpeed;
            }
        }

        public float staminaDrop
        {
            get
            {
                return pm.plm.staminaDrop;
            }
        }

        public float staminaMax
        {
            get
            {
                return pm.plm.staminaMax;
            }
        }

        public float staminaRise
        {
            get
            {
                return pm.plm.staminaRise;
            }
        }

        public float stamina
        {
            get
            {
                return pm.plm.stamina;
            }
            set
            {
                pm.plm.stamina = value;
            }
        }

        public float staminaNormal
        {
            get
            {
                return pm.plm.stamina / pm.plm.staminaMax;
            }
            set
            {
                pm.plm.stamina = value * pm.plm.staminaMax;
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
                Singleton<CoreGameManager>.Instance.AddPoints(value - currentPoints, pm.playerNumber, (currentPoints != value));
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
            statMod = pm.GetMovementStatModifier();
            moveMod = new MovementModifier(Vector3.zero, 1f);
            moveMod.ignoreGrounded = false;
            moveMod.ignoreAirborne = false;
            pm.plm.Entity.ExternalActivity.moveMods.Add(moveMod);
        }

        public void MakeGuilty(string rule, float time)
        {
            pm.RuleBreak(rule, time);
        }

        public string GetGuilt()
        {
            return pm.ruleBreak;
        }

        public void AddStickerToInventory(string stickerId, bool animation)
        {
            if (!LevelLoaderPlugin.Instance.stickerAliases.ContainsKey(stickerId)) return;
            Singleton<StickerManager>.Instance.AddSticker(LevelLoaderPlugin.Instance.stickerAliases[stickerId], true, false, animation);
        }

        public void RemoveStickerFromInventory(string stickerId)
        {
            if (!LevelLoaderPlugin.Instance.stickerAliases.ContainsKey(stickerId)) return;
            Singleton<StickerManager>.Instance.stickerInventory.Remove(Singleton<StickerManager>.Instance.stickerInventory.Find(x => x.sticker == LevelLoaderPlugin.Instance.stickerAliases[stickerId]));
        }

        public string[] GetInventoryStickers()
        {
            string[] stickerInv = new string[Singleton<StickerManager>.Instance.stickerInventory.Count];
            for (int i = 0; i < Singleton<StickerManager>.Instance.stickerInventory.Count; i++)
            {
                stickerInv[i] = LuaHelpers.GetIDFromSticker(Singleton<StickerManager>.Instance.stickerInventory[i].sticker);
            }
            return stickerInv;
        }

        public string GetInventorySticker(int slot)
        {
            slot -= 1;
            if (slot < 0) return "nothing";
            if (slot >= Singleton<StickerManager>.Instance.stickerInventory.Count) return "nothing";
            return LuaHelpers.GetIDFromSticker(Singleton<StickerManager>.Instance.stickerInventory[slot].sticker);
        }

        public string[] GetActiveStickers()
        {
            string[] stickerInv = new string[Singleton<StickerManager>.Instance.activeStickerData.Length];
            for (int i = 0; i < Singleton<StickerManager>.Instance.activeStickerData.Length; i++)
            {
                stickerInv[i] = LuaHelpers.GetIDFromSticker(Singleton<StickerManager>.Instance.activeStickerData[i].sticker);
            }
            return stickerInv;
        }

        public string GetActiveSticker(int slot)
        {
            slot -= 1;
            if (slot < 0) return "nothing";
            if (slot >= Singleton<StickerManager>.Instance.activeStickerData.Length) return "nothing";
            return LuaHelpers.GetIDFromSticker(Singleton<StickerManager>.Instance.activeStickerData[slot].sticker);
        }

        public int GetStickerValue(string stickerId)
        {
            if (!LevelLoaderPlugin.Instance.stickerAliases.ContainsKey(stickerId)) return 0;
            return Singleton<StickerManager>.Instance.StickerValue(LevelLoaderPlugin.Instance.stickerAliases[stickerId]);
        }

        public void ApplySticker(int slot, string stickerId)
        {
            if (!LevelLoaderPlugin.Instance.stickerAliases.ContainsKey(stickerId)) return;
            Singleton<StickerManager>.Instance.ApplySticker(StickerMetaStorage.Instance.Get(LevelLoaderPlugin.Instance.stickerAliases[stickerId]).value.CreateStateData(0, true, false), Mathf.Clamp(slot - 1, 0, Singleton<StickerManager>.Instance.activeStickerData.Length - 1));
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
