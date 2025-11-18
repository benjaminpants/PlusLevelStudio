using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PlusStudioLevelFormat;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusLevelStudio.Editor
{
    public class RoomEditorController : EditorController
    {
        public override void PreFileLoad()
        {
            Dictionary<string, string> obsoleteToMarkerDict = new Dictionary<string, string>()
            {
                { "technical_potentialdoor", "potentialdoor"},
                { "technical_forceddoor", "forceddoor" },
                { "technical_nosafe", null}, // we have a special case to handle this
                { "technical_lightspot", "lightspot" }
            };
            bool converted = false;
            for (int i = (levelData.structures.Count - 1); i >= 0; i--)
            {
                if (obsoleteToMarkerDict.ContainsKey(levelData.structures[i].type))
                {
                    converted = true;
                    LegacyRoomTechnicalStructure old = (LegacyRoomTechnicalStructure)levelData.structures[i];
                    levelData.structures.RemoveAt(i);
                    if (old.type != "technical_nosafe")
                    {
                        RoomCellMarker marker = (RoomCellMarker)LevelStudioPlugin.Instance.ConstructMarkerOfType(obsoleteToMarkerDict[old.type]);
                        marker.position = old.position;
                        levelData.markers.Add(marker);
                    }
                    else
                    {
                        levelData.markers.Add(new EntityUnsafeCellLocation() { position = old.position });
                        levelData.markers.Add(new EventUnsafeCellLocation() { position = old.position });
                    }
                }
            }
            base.PreFileLoad();
            if (converted)
            {
                CreateUIOnePopup(LocalizationManager.Instance.GetLocalizedText("Ed_Menu_FileConverted"), null);
            }
        }

        public override void Export()
        {
            List<BaldiRoomAsset> roomAssets = new List<BaldiRoomAsset>();
            BaldiLevel baseLevel = Compile(); // we will be manipulating this because its going to get discarded anyway


            // find the technical structures
            List<RoomCellMarker> markers = levelData.markers.Where(x => x is RoomCellMarker).Select(x => x as RoomCellMarker).ToList();

            for (int i = 0; i < baseLevel.rooms.Count; i++)
            {
                RoomInfo room = baseLevel.rooms[i];
                if (i == 0) continue; // first room is always the regular hallway which is not available.
                BaldiRoomAsset roomAsset = new BaldiRoomAsset();
                roomAsset.name = room.name;
                roomAsset.type = room.type;
                roomAsset.textureContainer = new TextureContainer(room.textureContainer);
                roomAsset.windowType = "standard";
                List<RoomCellInfo> cells = new List<RoomCellInfo>();
                List<ByteVector2> originalOwnedCells = new List<ByteVector2>();
                for (int x = 0; x < baseLevel.cells.GetLength(0); x++)
                {
                    for (int y = 0; y < baseLevel.cells.GetLength(1); y++)
                    {
                        if (baseLevel.cells[x, y].roomId == (i + 1))
                        {
                            originalOwnedCells.Add(new ByteVector2((byte)x, (byte)y));
                            cells.Add(new RoomCellInfo()
                            {
                                walls = baseLevel.cells[x, y].walls,
                                position = baseLevel.cells[x, y].position,
                                coverage = baseLevel.coverage[x, y]
                            });
                            if (baseLevel.entitySafeCells[x, y])
                            {
                                roomAsset.entitySafeCells.Add(new ByteVector2(x, y));
                            }
                            if (baseLevel.eventSafeCells[x, y])
                            {
                                roomAsset.eventSafeCells.Add(new ByteVector2(x, y));
                            }
                            if (baseLevel.secretCells[x, y])
                            {
                                roomAsset.secretCells.Add(new ByteVector2(x, y));
                            }
                        }
                    }
                }
                if (cells.Count == 0) continue; // dont.
                // calculate the offset and apply it
                ByteVector2 smallestCell = new ByteVector2(byte.MaxValue,byte.MaxValue);
                for (int j = 0; j < cells.Count; j++)
                {
                    if ((cells[j].position.x < smallestCell.x))
                    {
                        smallestCell = new ByteVector2(cells[j].position.x, smallestCell.y);
                    }
                    if ((cells[j].position.y < smallestCell.y))
                    {
                        smallestCell = new ByteVector2(smallestCell.x, cells[j].position.y);
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
                for (int j = 0; j < roomAsset.secretCells.Count; j++)
                {
                    roomAsset.secretCells[j] = (roomAsset.secretCells[j].ToInt() - offset).ToByte();
                }
                for (int j = 0; j < room.basicObjects.Count; j++)
                {
                    room.basicObjects[j].position = new UnityVector3(room.basicObjects[j].position.x - (offset.x * 10f), room.basicObjects[j].position.y, room.basicObjects[j].position.z - (offset.z * 10f));
                    roomAsset.basicObjects.Add(room.basicObjects[j]);
                }
                for (int j = 0; j < room.items.Count; j++)
                {
                    room.items[j].position = new UnityVector2(room.items[j].position.x - (offset.x * 10f), room.items[j].position.y - (offset.z * 10f));
                    roomAsset.items.Add(room.items[j]);
                }
                for (int j = 0; j < room.itemSpawns.Count; j++)
                {
                    room.itemSpawns[j].position = new UnityVector2(room.itemSpawns[j].position.x - (offset.x * 10f), room.itemSpawns[j].position.y - (offset.z * 10f));
                    roomAsset.itemSpawns.Add(room.itemSpawns[j]);
                }
                for (int j = 0; j < baseLevel.lights.Count; j++)
                {
                    if (originalOwnedCells.Contains(baseLevel.lights[j].position))
                    {
                        roomAsset.lights.Add(new LightInfo()
                        {
                            prefab = baseLevel.lights[j].prefab,
                            color = baseLevel.lights[j].color,
                            strength = baseLevel.lights[j].strength,
                            position = (baseLevel.lights[j].position.ToInt() - offset).ToByte()
                        });
                    }
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
                // Hallway exclusive features
                if (roomAsset.type == "hall")
                {
                    // ** Make all the border have no walls
                    foreach (var cell in roomAsset.cells)
                    {
                        for (int z = 0; z < 4; z++)
                        {
                            var dir = (Direction)z;
                            var neighborPos = (cell.position.ToInt() + dir.ToIntVector2()).ToByte();
                            int mask = 1 << z;
                            // If there's a wall on that side and no adjacent cell, remove the wall bit and clear coverage
                            if (((cell.walls & mask) != 0) && !roomAsset.cells.Exists(checkCell => checkCell.position == neighborPos))
                            {
                                cell.walls = new Nybble(cell.walls & ~mask);
                                cell.coverage &= ~(PlusCellCoverage)dir.ToCoverage();
                            }
                        }
                    }
                }

                roomAsset.activity = room.activity;
                if (roomAsset.activity != null)
                {
                    roomAsset.activity.position = new UnityVector3(roomAsset.activity.position.x - (offset.x * 10f), roomAsset.activity.position.y, roomAsset.activity.position.z - (offset.z * 10f));
                }
                for (int j = 0; j < markers.Count; j++)
                {
                    if (markers[j].CaresAboutRoom(levelData, baseLevel, offset, roomAsset))
                    {
                        markers[j].CompileIntoRoom(levelData, baseLevel, offset, roomAsset);
                    }
                }
                if (string.IsNullOrEmpty(room.name))
                {
                    roomAsset.name = room.type + "_" + i + "_" + roomAsset.cells.Count + "_" + (roomAsset.activity == null ? "null" : roomAsset.activity.type);
                }
                roomAssets.Add(roomAsset);
            }

            if (roomAssets.Count == 0) return;
            if (roomAssets.Count == 1)
            {
                Directory.CreateDirectory(LevelStudioPlugin.levelExportPath);
                BinaryWriter writer = new BinaryWriter(new FileStream(Path.Combine(LevelStudioPlugin.levelExportPath, currentFileName + ".rbpl"), FileMode.Create, FileAccess.Write));
                roomAssets[0].Write(writer);
                writer.Close();
                Application.OpenURL("file://" + LevelStudioPlugin.levelExportPath);
                return;
            }
            string roomsPath = Path.Combine(LevelStudioPlugin.levelExportPath, currentFileName);
            Directory.CreateDirectory(roomsPath);
            for (int i = 0; i < roomAssets.Count; i++)
            {
                BinaryWriter writer = new BinaryWriter(new FileStream(Path.Combine(roomsPath, roomAssets[i].name + ".rbpl"), FileMode.Create, FileAccess.Write));
                roomAssets[i].Write(writer);
                writer.Close();
            }
            Application.OpenURL("file://" + roomsPath);
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
