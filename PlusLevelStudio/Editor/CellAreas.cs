using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    // TODO: decide if this class should be modified to use IntVector2's instead of ByteVector2s
    public abstract class CellArea
    {
        public abstract string type { get; }
        public IntVector2 origin;
        public ushort roomId;
        public virtual bool editorOnly => false;
        public CellArea(IntVector2 origin, ushort roomId)
        {
            this.origin = origin;
            this.roomId = roomId;
        }

        /// <summary>
        /// The rect for when this CellArea is selected. Return null to prevent the resize handles from showing up.
        /// </summary>
        public abstract RectInt? rect { get; }

        /// <summary>
        /// Called when this Area is being resized.
        /// </summary>
        /// <param name="sizeDif"></param>
        /// <param name="posDif"></param>
        /// <returns></returns>
        public abstract bool Resize(IntVector2 sizeDif, IntVector2 posDif);

        // TODO: change implementation?
        public bool ResizeWithSafety(IntVector2 sizeDif, IntVector2 posDif)
        {
            if (!Resize(sizeDif, posDif)) return false;
            if (!EditorController.Instance.levelData.AreaValid(this))
            {
                Resize(sizeDif * -1, posDif * -1);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns if the specified IntVector2 is inside of this area.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public virtual bool VectorIsInArea(IntVector2 vector)
        {
            return CalculateOwnedCells().Contains(vector);
        }

        public virtual void Write(BinaryWriter writer)
        {
            writer.Write(type);
            writer.Write(roomId);
            writer.Write(origin.ToByte());
        }

        public virtual CellArea ReadInto(BinaryReader reader)
        {
            roomId = reader.ReadUInt16();
            origin = reader.ReadByteVector2().ToInt();
            return this;
        }

        public virtual bool CollidesWith(CellArea area)
        {
            IntVector2[] owned = CalculateOwnedCells();
            for (int i = 0; i < owned.Length; i++)
            {
                if (area.VectorIsInArea(owned[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public abstract IntVector2[] CalculateOwnedCells();
    }

    public class RectCellArea : CellArea
    {
        public override string type => "rect";
        public IntVector2 size;
        public override RectInt? rect => new RectInt(origin.ToUnityVector(), size.ToUnityVector());

        public RectCellArea(IntVector2 origin, IntVector2 size, ushort roomId) : base(origin, roomId)
        {
            this.size = size;
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(size.ToByte());
        }

        public override CellArea ReadInto(BinaryReader reader)
        {
            base.ReadInto(reader);
            size = reader.ReadByteVector2().ToInt();
            return this;
        }

        public override bool VectorIsInArea(IntVector2 vector) //this is quicker than the default VectorIsInArea implementation
        {
            if (!(vector.x >= origin.x)) return false;
            if (!(vector.x < (origin + size).x)) return false;
            if (!(vector.z >= origin.z)) return false;
            if (!(vector.z < (origin + size).z)) return false;
            return true;
        }

        public override IntVector2[] CalculateOwnedCells()
        {
            List<IntVector2> vectors = new List<IntVector2>();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.z; y++)
                {
                    vectors.Add(origin + new IntVector2(x, y));
                }
            }
            return vectors.ToArray();
        }

        public override bool Resize(IntVector2 sizeDif, IntVector2 posDif)
        {
            IntVector2 newSize = (size + sizeDif);
            if (newSize.x <= 0 || newSize.z <= 0) return false; // if this shrinks us into being 0x0, return false
            origin += posDif;
            size = newSize;
            return true;
        }
    }
}
