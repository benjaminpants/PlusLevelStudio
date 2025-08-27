using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{
    public class MatchBalloonMarker : PositionMarker
    {
        public override void Compile(EditorLevelData data, BaldiLevel compiled)
        {
            ushort roomFromPos = data.RoomIdFromPos(position.ToCellVector(), true);
            if (roomFromPos == 0) throw new Exception("MatchBalloonMarker with no room??? How???");
            compiled.rooms[roomFromPos - 1].basicObjects.Add(new BasicObjectInfo()
            {
                position = position.ToData(),
                prefab = type,
                rotation = new UnityQuaternion()
            });
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            EditorRoom roomFromPos = data.RoomFromPos(position.ToCellVector(), true);
            if (roomFromPos == null) return false;
            if (roomFromPos.activity == null) return false;
            if (roomFromPos.activity.type != "matchmachine") return false;
            return true;
        }
    }
}
