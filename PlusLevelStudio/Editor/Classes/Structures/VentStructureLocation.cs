using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class VentVisualManager : MonoBehaviour
    {
        public List<Transform> rendererPieces = new List<Transform>();
        public EditorRendererContainer container;
        public Transform ventPieceBendPrefab;
        public Transform ventPieceStraightPrefab;
        public Transform ventPieceVerticalBendPrefab;
        public Transform exitGrateTransform;
        public Transform entryGrate;
        public const float height = 20f;

        public void UpdateNonInsideVisuals(IntVector2 position, Direction direction, IntVector2 endPoint)
        {
            transform.position = position.ToWorld();
            transform.rotation = direction.GetOpposite().ToRotation();
            exitGrateTransform.position = endPoint.ToWorld() + (Vector3.up * 5f);
        }

        public void CleanupInsideVisuals()
        {
            for (int i = 0; i < rendererPieces.Count; i++)
            {
                int indexOf = container.myRenderers.IndexOf(rendererPieces[i].GetComponent<Renderer>());
                container.myRenderers.RemoveAt(indexOf);
                container.defaultHighlights.RemoveAt(indexOf);
                Destroy(rendererPieces[i].gameObject);
            }
            rendererPieces.Clear();
        }

        public void BuildModel(List<IntVector2> points, Direction startDirection)
        {
            CleanupInsideVisuals();
            // TODO: this is a tweaked version of directly compiled vent stuff. this has to be cleaned up
            transform.position = points[0].ToWorld();
            transform.rotation = startDirection.GetOpposite().ToRotation();
            IntVector2 intVector;
            IntVector2 b = new IntVector2(1, 1);
            Direction direction = Direction.North;
            if ((points.Count == 1))
            {
                exitGrateTransform.position = Vector3.down * 1000;
            }
            else
            {
                exitGrateTransform.position = points[points.Count - 1].ToWorld() + (Vector3.up * 5f);
            }
            for (int i = 0; i < points.Count; i++)
            {
                if ((i == points.Count - 1))
                {
                    Transform ventVerticalBendTransform = Instantiate(ventPieceVerticalBendPrefab, transform);
                    ventVerticalBendTransform.transform.position = points[i].ToWorld() + Vector3.up * 10f;
                    ventVerticalBendTransform.transform.rotation = direction.GetOpposite().ToRotation();
                    rendererPieces.Add(ventVerticalBendTransform);
                    container.AddRenderer(ventVerticalBendTransform.GetComponent<Renderer>(), "white");
                    //ventController.renderers[i - 1].Add(ventBendTransform.GetComponent<Renderer>());
                    break;
                }
                Direction direction2 = Direction.North;
                if (points[i].x > points[i + 1].x)
                {
                    b = new IntVector2(-1, 0);
                    direction2 = Direction.West;
                }
                else if (points[i].x < points[i + 1].x)
                {
                    b = new IntVector2(1, 0);
                    direction2 = Direction.East;
                }
                else if (points[i].z > points[i + 1].z)
                {
                    b = new IntVector2(0, -1);
                    direction2 = Direction.South;
                }
                else if (points[i].z < points[i + 1].z)
                {
                    b = new IntVector2(0, 1);
                    direction2 = Direction.North;
                }
                if (i == 0)
                {
                    Transform ventVerticalBendTransform = Instantiate(ventPieceVerticalBendPrefab, transform);
                    ventVerticalBendTransform.transform.position = points[i].ToWorld() + Vector3.up * 10f;
                    ventVerticalBendTransform.transform.rotation = direction2.ToRotation();
                    rendererPieces.Add(ventVerticalBendTransform);
                    container.AddRenderer(ventVerticalBendTransform.GetComponent<Renderer>(), "white");
                    //ventController.renderers[i].Add(ventVerticalBendTransform.GetComponent<Renderer>());
                }
                else
                {
                    int num = Mathf.Abs(direction.GetOpposite() - direction2);
                    Transform ventPieceBendTransform = Instantiate(ventPieceBendPrefab, transform);
                    ventPieceBendTransform.transform.position = points[i].ToWorld() + Vector3.up * height;
                    if (num > 1)
                    {
                        ventPieceBendTransform.transform.rotation = Direction.West.ToRotation();
                    }
                    else
                    {
                        ventPieceBendTransform.transform.rotation = ((Direction)Mathf.Min((int)direction.GetOpposite(), (int)direction2)).ToRotation();
                    }
                    rendererPieces.Add(ventPieceBendTransform);
                    container.AddRenderer(ventPieceBendTransform.GetComponent<Renderer>(), "white");
                }
                intVector = points[i] + b;
                while (intVector != points[i + 1] && (EditorController.Instance.levelData.GetCellSafe(intVector) != null))
                {
                    Transform ventStraightTransform = Instantiate(ventPieceStraightPrefab, transform);
                    ventStraightTransform.transform.position = intVector.ToWorld() + Vector3.up * height;
                    ventStraightTransform.transform.rotation = direction2.ToRotation();
                    rendererPieces.Add(ventStraightTransform);
                    container.AddRenderer(ventStraightTransform.GetComponent<Renderer>(), "white");
                    intVector += b;
                }
                direction = direction2;
            }
        }
    }

    public class VentLocation : IEditorVisualizable, IEditorDeletable, IEditorCellModifier
    {
        public VentStructureLocation owner;
        public List<IntVector2> positions = new List<IntVector2>();
        public Direction direction;
        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.genericStructureDisplays["vent"];
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public void ModifyCells(EditorLevelData data, bool forEditor)
        {
            if (positions.Count == 0) return;
            IntVector2 startPos = positions[0];
            PlusStudioLevelFormat.Cell cell = data.GetCellSafe(startPos);
            if (cell == null) return;
            PlusStudioLevelFormat.Cell cellAcross = data.GetCellSafe(startPos + direction.GetOpposite().ToIntVector2());
            if (cellAcross == null) return;
            cell.roomId = data.RoomIdFromPos(cellAcross.position.ToInt(), forEditor);
            if (cell.roomId == 0)
            {
                cell.roomId = 1;
            }
            Direction rightDir = direction.RotatedRelativeToNorth(direction);
            Direction leftDir = direction.GetOpposite().RotatedRelativeToNorth(direction);
            cell.walls = (Nybble)~direction.GetOpposite().ToBinary();

            // set the other sides as well so we dont get one side walls
            cell = data.GetCellSafe(startPos + direction.ToIntVector2());
            if (cell != null)
            {
                cell.walls |= (Nybble)direction.GetOpposite().ToBinary();
            }
            cell = data.GetCellSafe(startPos + leftDir.ToIntVector2());
            if (cell != null)
            {
                cell.walls |= (Nybble)leftDir.GetOpposite().ToBinary();
            }
            cell = data.GetCellSafe(startPos + rightDir.ToIntVector2());
            if (cell != null)
            {
                cell.walls |= (Nybble)rightDir.GetOpposite().ToBinary();
            }
            cell = data.GetCellSafe(startPos + direction.GetOpposite().ToIntVector2());
            if (cell != null)
            {
                cell.walls &= (Nybble)~direction.ToBinary();
            }
        }

        public void ModifyLightsForEditor(EnvironmentController workerEc)
        {
            throw new NotImplementedException();
        }

        public bool OnDelete(EditorLevelData data)
        {
            return owner.DeleteVent(this);
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.GetComponent<VentVisualManager>().UpdateNonInsideVisuals(positions[0], direction, positions[positions.Count - 1]);
        }
    }

    public class VentStructureLocation : StructureLocation
    {
        public List<VentLocation> ventLocations = new List<VentLocation>();
        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            // none
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            for (int i = 0; i < ventLocations.Count; i++)
            {
                EditorController.Instance.RemoveVisual(ventLocations[i]);
            }
        }

        public bool DeleteVent(VentLocation local)
        {
            ventLocations.Remove(local);
            EditorController.Instance.RemoveVisual(local);
            EditorController.Instance.RefreshCells();
            return true;
        }

        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            StructureInfo info = new StructureInfo(type);
            for (int i = 0; i < ventLocations.Count; i++)
            {
                VentLocation location = ventLocations[i];
                for (int j = 0; j < location.positions.Count; j++)
                {
                    info.data.Add(new StructureDataInfo()
                    {
                        direction = (PlusDirection)location.direction,
                        position = location.positions[j].ToByte(),
                        data = ((j == (location.positions.Count - 1)) ? 1 : 0)
                    });
                }
            }
            return info;
        }

        public override GameObject GetVisualPrefab()
        {
            return null;
        }

        public override void InitializeVisual(GameObject visualObject)
        {
            for (int i = 0; i < ventLocations.Count; i++)
            {
                EditorController.Instance.AddVisual(ventLocations[i]);
            }
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            for (int i = 0; i < ventLocations.Count; i++)
            {
                VentLocation vent = ventLocations[i];
                for (int j = 0; j < vent.positions.Count; j++)
                {
                    vent.positions[j] -= cellOffset;
                }
            }
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            for (int i = 0; i < ventLocations.Count; i++)
            {
                EditorController.Instance.UpdateVisual(ventLocations[i]);
            }
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            if (ventLocations.Count == 0) return false;
            return true;
        }

        const byte version = 0;

        public VentLocation CreateVentLocation()
        {
            return new VentLocation() { owner = this };
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            byte version = reader.ReadByte();
            int ventCount = reader.ReadInt32();
            for (int i = 0; i < ventCount; i++)
            {
                VentLocation ventLocal = CreateVentLocation();
                ventLocal.direction = (Direction)reader.ReadByte();
                int positionCount = reader.ReadInt32();
                for (int j = 0; j < positionCount; j++)
                {
                    ventLocal.positions.Add(reader.ReadByteVector2().ToInt());
                }
                ventLocations.Add(ventLocal);
            }
        }

        public override void ModifyCells(EditorLevelData data, bool forEditor)
        {
            for (int i = 0; i < ventLocations.Count; i++)
            {
                ventLocations[i].ModifyCells(data, forEditor);
            }
        }

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write(version);
            writer.Write(ventLocations.Count);
            for (int i = 0; i < ventLocations.Count; i++)
            {
                writer.Write((byte)ventLocations[i].direction);
                writer.Write(ventLocations[i].positions.Count);
                for (int j = 0; j < ventLocations[i].positions.Count; j++)
                {
                    writer.Write(ventLocations[i].positions[j].ToByte());
                }
            }
        }
    }
}
