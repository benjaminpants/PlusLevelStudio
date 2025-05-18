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
        public ByteVector2 origin;
        public ushort roomId;
        public virtual bool editorOnly => false;
        public CellArea(ByteVector2 origin, ushort roomId)
        {
            this.origin = origin;
            this.roomId = roomId;
        }

        public abstract RectInt? rect { get; }

        public abstract void Resize(IntVector2 posDif, IntVector2 sizeDif);

        // TODO: change implementation?
        public bool ResizeWithSafety(IntVector2 posDif, IntVector2 sizeDif)
        {
            Resize(posDif, sizeDif);
            if (!EditorController.Instance.levelData.AreaValid(this))
            {
                Resize(posDif * -1, sizeDif * -1);
                return false;
            }
            return true;
        }

        public virtual bool VectorIsInArea(ByteVector2 vector)
        {
            return CalculateOwnedCells().Contains(vector);
        }

        public virtual void Write(BinaryWriter writer)
        {
            writer.Write(type);
            writer.Write(origin);
            writer.Write(roomId);
        }

        public virtual CellArea ReadInto(BinaryReader reader)
        {
            origin = reader.ReadByteVector2();
            roomId = reader.ReadUInt16();
            return this;
        }

        public virtual bool CollidesWith(CellArea area)
        {
            ByteVector2[] owned = CalculateOwnedCells();
            for (int i = 0; i < owned.Length; i++)
            {
                if (area.VectorIsInArea(owned[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public abstract ByteVector2[] CalculateOwnedCells();
    }

    public class RectCellArea : CellArea
    {
        public override string type => "rect";
        public ByteVector2 size;
        public ByteVector2 corner => origin + (size - ByteVector2.one);
        public override RectInt? rect => new RectInt(origin.ToInt().ToUnityVector(), size.ToInt().ToUnityVector());

        public RectCellArea(ByteVector2 origin, ByteVector2 size, ushort roomId) : base(origin, roomId)
        {
            this.size = size;
        }

        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(size);
        }

        public override CellArea ReadInto(BinaryReader reader)
        {
            base.ReadInto(reader);
            size = reader.ReadByteVector2();
            return this;
        }

        public override bool VectorIsInArea(ByteVector2 vector) //this is quicker than the default VectorIsInArea implementation
        {
            if (!(vector.x >= origin.x)) return false;
            if (!(vector.x < (origin + size).x)) return false;
            if (!(vector.y >= origin.y)) return false;
            if (!(vector.y < (origin + size).y)) return false;
            return true;
        }

        public override ByteVector2[] CalculateOwnedCells()
        {
            List<ByteVector2> vectors = new List<ByteVector2>();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    vectors.Add(origin + new ByteVector2(x, y));
                }
            }
            return vectors.ToArray();
        }

        public override void Resize(IntVector2 posDif, IntVector2 sizeDif)
        {
            origin = (origin.ToInt() - posDif).ToByte();
            size = (size.ToInt() + sizeDif).ToByte();
        }
    }
}
