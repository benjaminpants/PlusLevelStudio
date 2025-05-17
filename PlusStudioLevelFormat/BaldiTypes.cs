using System;
using System.Collections.Generic;
using System.Text;

namespace PlusStudioLevelFormat
{
    public class Cell
    {

        public ByteVector2 position;
        public ushort roomId = 0;
        public int type => (roomId == 0) ? 16 : walls;
        public Nybble walls = new Nybble(0);

        public Cell(ByteVector2 pos)
        {
            position = pos;
        }
    }
}
