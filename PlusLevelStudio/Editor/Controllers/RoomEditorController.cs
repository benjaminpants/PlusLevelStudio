using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlusLevelStudio.Editor
{
    public class RoomEditorController : EditorController
    {
        static List<BaldiRoomAsset> testExport = new List<BaldiRoomAsset>();
        public override void Export()
        {
            List<BaldiRoomAsset> roomAssets = new List<BaldiRoomAsset>();
            BaldiLevel baseLevel = Compile(); // we will be manipulating this because its going to get discarded anyway


            // find the technical structures
            List<RoomTechnicalStructureBase> technicalStuff = levelData.structures.Where(x => x is RoomTechnicalStructureBase).Select(x => x as RoomTechnicalStructureBase).ToList();

            for (int i = 0; i < baseLevel.rooms.Count; i++)
            {
                RoomInfo room = baseLevel.rooms[i];
                if (room.type == "hall") continue; // place the holder
                BaldiRoomAsset roomAsset = new BaldiRoomAsset();
                roomAsset.name = room.type;
                roomAsset.textureContainer = new TextureContainer(room.textureContainer);
                roomAsset.windowType = "standard";
                List<RoomCellInfo> cells = new List<RoomCellInfo>();
                List<ByteVector2> originalOwnedCells = new List<ByteVector2>();
                for (int x = 0; x < baseLevel.cells.GetLength(0); x++)
                {
                    for (int y = 0; y < baseLevel.cells.GetLength(1); y++)
                    {
                        if (baseLevel.cells[x,y].roomId == (i + 1))
                        {
                            originalOwnedCells.Add(new ByteVector2((byte)x, (byte)y));
                            cells.Add(new RoomCellInfo()
                            {
                                walls = baseLevel.cells[x, y].walls,
                                position = baseLevel.cells[x, y].position,
                                coverage = baseLevel.coverage[x,y]
                            });
                            if (baseLevel.entitySafeCells[x,y])
                            {
                                roomAsset.entitySafeCells.Add(new ByteVector2(x,y));
                            }
                            if (baseLevel.eventSafeCells[x, y])
                            {
                                roomAsset.eventSafeCells.Add(new ByteVector2(x, y));
                            }
                        }
                    }
                }
                if (cells.Count == 0) continue; // dont.
                // calculate the offset and apply it
                ByteVector2 smallestCell = cells[0].position;
                for (int j = 0; j < cells.Count; j++)
                {
                    if ((cells[j].position.x < smallestCell.x) || (cells[j].position.y < smallestCell.y))
                    {
                        smallestCell = cells[j].position;
                    }
                }
                IntVector2 offset = smallestCell.ToInt();
                for (int j = 0; j < cells.Count; j++)
                {
                    cells[j].position = (cells[j].position.ToInt() - offset).ToByte();
                }
                roomAsset.cells = cells;
                for (int j = 0; j < roomAsset.entitySafeCells.Count; j++)
                {
                    roomAsset.entitySafeCells[j] = (roomAsset.entitySafeCells[j].ToInt() - offset).ToByte();
                }
                for (int j = 0; j < roomAsset.eventSafeCells.Count; j++)
                {
                    roomAsset.eventSafeCells[j] = (roomAsset.eventSafeCells[j].ToInt() - offset).ToByte();
                }
                for (int j = 0; j < room.basicObjects.Count; j++)
                {
                    room.basicObjects[j].position = new UnityVector3(room.basicObjects[j].position.x - (offset.x * 10f), room.basicObjects[j].position.y, room.basicObjects[j].position.z - (offset.z * 10f));
                    roomAsset.basicObjects.Add(room.basicObjects[j]);
                }
                for (int j = 0; j < room.items.Count; j++)
                {
                    roomAsset.items.Add(room.items[j]);
                }
                for (int j = 0; j < baseLevel.posters.Count; j++)
                {
                    if (originalOwnedCells.Contains(baseLevel.posters[j].position))
                    {
                        roomAsset.posters.Add(new PosterInfo()
                        {
                            poster = baseLevel.posters[j].poster,
                            direction = baseLevel.posters[j].direction,
                            position = (baseLevel.posters[j].position.ToInt() - offset).ToByte()
                        });
                    }
                }
                roomAsset.activity = room.activity;
                for (int j = 0; j < technicalStuff.Count; j++)
                {
                    if (technicalStuff[j].CaresAboutRoom(levelData, baseLevel, offset, roomAsset))
                    {
                        technicalStuff[j].CompileIntoRoom(levelData, baseLevel, offset, roomAsset);
                    }
                }
                roomAssets.Add(roomAsset);
            }

            testExport = roomAssets;
        }

        public override void PlayLevel()
        {
            // no
        }

        public override void EditorModeAssigned()
        {
            base.EditorModeAssigned();
            levelData.minLightColor = UnityEngine.Color.white;
        }
    }
}
