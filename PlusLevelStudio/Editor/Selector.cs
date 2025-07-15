using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace PlusLevelStudio.Editor
{

    public enum SelectorState
    {
        /// <summary>
        /// Nothing is selected or displayed.
        /// </summary>
        None,
        /// <summary>
        /// A tile is selected, show the tile selection box.
        /// </summary>
        Tile,
        /// <summary>
        /// An area is selected. Show the resize handles.
        /// </summary>
        Area,
        /// <summary>
        /// A direction is waiting to be selected. Show the resize handles in a 1x1 box.
        /// </summary>
        Direction,
        /// <summary>
        /// An object is selected. Show the 3D rotation handles and the settings.
        /// </summary>
        Object,
        /// <summary>
        /// An object that only can have its settings changed. Show the settings.
        /// </summary>
        Settings
    }

    public enum SelectorObjectFlags
    {
        None = 0,
        NorthMoveArrow = 1,
        EastMoveArrow = 2,
        SouthMoveArrow = 4,
        WestMoveArrow = 8,
        UpMoveArrow = 16,
        DownMoveArrow = 32,
        PitchRotation = 64,
        YawRotation = 128,
        RollRotation = 256,
        Move2D = NorthMoveArrow | EastMoveArrow | SouthMoveArrow | WestMoveArrow,
        MoveAll = Move2D | UpMoveArrow | DownMoveArrow,
        MoveAndRotate = MoveAll | RotateAll,
        RotateAll = PitchRotation | YawRotation | RollRotation
    }

    public enum SelectorArrowDirection
    {
        Up,
        Down,
        North,
        South,
        East,
        West
    }

    public class ObjectSelectorArrow : MonoBehaviour, IEditorInteractable
    {
        public Selector selector;
        public int index;

        public bool InteractableByTool(EditorTool tool)
        {
            return false;
        }

        public bool OnClicked()
        {
            return selector.ObjectArrowClicked(index);
        }

        public bool OnHeld()
        {
            return selector.ObjectArrowHeld(index);
        }

        public void OnReleased()
        {
            selector.ObjectArrowReleased(index);
        }
    }

    public class SelectorArrow : MonoBehaviour, IEditorInteractable
    {
        public Selector selector;
        public Direction direction;

        public bool InteractableByTool(EditorTool tool)
        {
            return true;
        }

        public bool OnClicked()
        {
            return selector.TileArrowClicked(direction);
        }

        public bool OnHeld()
        {
            return selector.TileArrowHeld();
        }

        public void OnReleased()
        {
            selector.TileArrowReleased();
        }
    }

    public class Selector : MonoBehaviour
    {
        public GameObject tileSelector;
        public GameObject[] tileArrows = new GameObject[4];
        public GameObject[] objectArrows = new GameObject[6];
        protected static SelectorObjectFlags[] objectArrowOrder = new SelectorObjectFlags[6]
        {
            SelectorObjectFlags.UpMoveArrow,
            SelectorObjectFlags.DownMoveArrow,
            SelectorObjectFlags.NorthMoveArrow,
            SelectorObjectFlags.SouthMoveArrow,
            SelectorObjectFlags.EastMoveArrow,
            SelectorObjectFlags.WestMoveArrow,
        };
        public SettingsWorldButton gearButton;

        public Plane horizontalPlane;
        public Plane verticalPlane;

        public IntVector2 selectedTile { get; private set; } = new IntVector2(0, 0);

        public RectInt selectedArea { get; private set; } = new RectInt(new Vector2Int(0,0), new Vector2Int(0, 0));

        protected Direction currentArrow = Direction.Null;
        protected IntVector2 currentStartPosition = new IntVector2();
        protected SelectorState state;
        protected SelectorObjectFlags objectMoveFlags = SelectorObjectFlags.None;

        protected Action<IntVector2, IntVector2> resizeAction;
        protected Action<Direction> directionAction;
        protected IEditorObjectMovable currentMovableObject = null;


        void Awake()
        {
            UpdateSelectionObjects();
            horizontalPlane = new Plane(Vector3.up, 0f);
            verticalPlane = new Plane(); // placeholder
        }

        private void NullActions()
        {
            resizeAction = null;
            directionAction = null;
            if (currentMovableObject != null)
            {
                currentMovableObject.MoveHighlight(false);
                currentMovableObject = null;
            }
        }

        /// <summary>
        /// The current state of the selector.
        /// </summary>
        public SelectorState currentState => state;

        /// <summary>
        /// Removes the active selection
        /// </summary>
        public void DisableSelection()
        {
            state = SelectorState.None;
            NullActions();
            UpdateSelectionObjects();
        }

        /// <summary>
        /// Selects the specified tile.
        /// </summary>
        /// <param name="tile"></param>
        public void SelectTile(IntVector2 tile)
        {
            selectedTile = tile;
            state = SelectorState.Tile;
            NullActions();
            UpdateSelectionObjects();
        }

        /// <summary>
        /// Places the Selector at the specified tile and shows the arrows.
        /// directionSelectAction is called when one of the arrows is clicked.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="directionSelectAction"></param>
        public void SelectRotation(IntVector2 tile, Action<Direction> directionSelectAction)
        {
            selectedTile = tile;
            selectedArea = new RectInt(new Vector2Int(tile.x,tile.z), new Vector2Int(1,1));
            state = SelectorState.Direction;
            NullActions();
            directionAction = directionSelectAction;
            UpdateSelectionObjects();
            for (int i = 0; i < tileArrows.Length; i++)
            {
                PositionArrow((Direction)i, 0f);
            }
        }

        /// <summary>
        /// Select the area specified by Rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="resizeAction">First parameter is the change in size, the second parameter is the change in position.</param>
        public void SelectArea(RectInt rect, Action<IntVector2, IntVector2> resizeAction)
        {
            selectedArea = rect;
            state = SelectorState.Area;
            NullActions();
            this.resizeAction = resizeAction;
            UpdateSelectionObjects();
            for (int i = 0; i < tileArrows.Length; i++)
            {
                PositionArrow((Direction)i, 0f);
            }
        }

        /// <summary>
        /// Displays the settings icon at the chosen position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="onClicked"></param>
        public void ShowSettings(Vector3 position, Action onClicked)
        {
            // TODO: REMOVE HACK! ACK ACK ACK ITS SO HACKY IN HERE
            EditorController.Instance.UnhighlightAllCells();
            NullActions();
            state = SelectorState.Settings;
            gearButton.clickedAction = onClicked;
            UpdateSelectionObjects();
            gearButton.transform.position = position;
        }

        public void SelectObject(IEditorObjectMovable movable, SelectorObjectFlags flags)
        {
            // TODO: REMOVE HACK! ACK ACK ACK ITS SO HACKY IN HERE
            EditorController.Instance.UnhighlightAllCells();
            NullActions();
            state = SelectorState.Object;
            objectMoveFlags = flags;
            currentMovableObject = movable;
            movable.MoveHighlight(true);
            UpdateSelectionObjects();
        }


        public bool ObjectArrowClicked(int index)
        {
            currentMovableObject.MoveStart();
            return true;
        }

        public bool ObjectArrowHeld(int index)
        {
            horizontalPlane.distance = -objectArrows[index].transform.position.y;
            Vector3? position = EditorController.Instance.CastRayToPlane(index >= 2 ? horizontalPlane : verticalPlane, true);
            if (position == null) return true;
            Vector3 offset = LockPositionOntoAxis((SelectorArrowDirection)index, position.Value) - LockPositionOntoAxis((SelectorArrowDirection)index, objectArrows[index].transform.position);
            if (offset.sqrMagnitude == 0f) return true;
            currentMovableObject.Move(offset);
            PositionObjectArrows();
            return true;
        }

        public void ObjectArrowReleased(int index)
        {
            currentMovableObject.MoveEnd();
        }

        public bool TileArrowClicked(Direction d)
        {
            if (state == SelectorState.Direction)
            {
                directionAction.Invoke(d);
                return false;
            }
            currentArrow = d;
            currentStartPosition = Singleton<EditorController>.Instance.mouseGridPosition;
            return true;
        }


        public bool TileArrowHeld()
        {
            switch (state)
            {
                default:
                    break;
                case SelectorState.Area:
                    PositionArrow(currentArrow, (Singleton<EditorController>.Instance.mouseGridPosition - currentStartPosition).DistanceInDirection(currentArrow) * 10f);
                    break;
            }
            return true;
        }

        protected void PositionArrow(Direction d, float additionalDistanceFromEdge)
        {
            Vector3 movement = d.ToVector3();
            Vector3 center = (new Vector3(selectedArea.center.x, 0f, selectedArea.center.y) * 10f);

            tileArrows[(int)d].transform.position = center + (movement * 5f * (selectedArea.size.ToMystVector().GetValueForDirection(d) + 1)) + (movement * additionalDistanceFromEdge) + (Vector3.up * 0.01f);
        }

        public void TileArrowReleased()
        {
            EditorExtensions.CalculateDifferencesForHandleDrag(currentArrow, (Singleton<EditorController>.Instance.mouseGridPosition - currentStartPosition).DistanceInDirection(currentArrow), out IntVector2 sizeDif, out IntVector2 posDif);
            PositionArrow(currentArrow, 0f);
            if (resizeAction != null)
            {
                resizeAction.Invoke(sizeDif, posDif);
            }
            currentArrow = Direction.Null;
        }

        void Update()
        {
            switch (state)
            {
                case SelectorState.None:
                    break;
                case SelectorState.Direction:
                case SelectorState.Tile:
                    transform.position = selectedTile.ToWorld() + (Vector3.up * 0.01f);
                    break;
            }
        }

        protected void PositionObjectArrow(SelectorArrowDirection direction, Vector3 origin, float dist)
        {
            switch (direction)
            {
                case SelectorArrowDirection.Up:
                    objectArrows[0].transform.position = origin + (Vector3.up * (dist));
                    break;
                case SelectorArrowDirection.Down:
                    objectArrows[1].transform.position = origin + (Vector3.down * (dist));
                    break;
                case SelectorArrowDirection.North:
                    objectArrows[2].transform.position = origin + (Vector3.forward * (dist));
                    break;
                case SelectorArrowDirection.South:
                    objectArrows[3].transform.position = origin + (Vector3.back * (dist));
                    break;
                case SelectorArrowDirection.East:
                    objectArrows[4].transform.position = origin + (Vector3.right * (dist));
                    break;
                case SelectorArrowDirection.West:
                    objectArrows[5].transform.position = origin + (Vector3.left * (dist));
                    break;
            }
        }

        protected Vector3 LockPositionOntoAxis(SelectorArrowDirection direction, Vector3 pos)
        {
            switch (direction)
            {
                // i wanted to use .Scale but that affects the original vec3 and i'd really rather not.
                case SelectorArrowDirection.Up:
                    return new Vector3(0f, pos.y, 0f);
                case SelectorArrowDirection.Down:
                    return new Vector3(0f, pos.y, 0f);
                case SelectorArrowDirection.North:
                    return new Vector3(0f, 0f, pos.z);
                case SelectorArrowDirection.South:
                    return new Vector3(0f, 0f, pos.z);
                case SelectorArrowDirection.East:
                    return new Vector3(pos.x, 0f, 0f);
                case SelectorArrowDirection.West:
                    return new Vector3(pos.x, 0f, 0f);
            }
            return Vector3.zero;
        }

        protected void PositionObjectArrows()
        {
            Bounds bounds = currentMovableObject.GetBounds();
            // up
            objectArrows[0].transform.position = bounds.center + (Vector3.up * (bounds.extents.y + 2f));
            // down
            objectArrows[1].transform.position = bounds.center + (Vector3.down * (bounds.extents.y + 2f));
            // north
            objectArrows[2].transform.position = bounds.center + (Vector3.forward * (bounds.extents.z + 2f));
            // south
            objectArrows[3].transform.position = bounds.center + (Vector3.back * (bounds.extents.z + 2f));
            // east
            objectArrows[4].transform.position = bounds.center + (Vector3.right * (bounds.extents.x + 2f));
            // west
            objectArrows[5].transform.position = bounds.center + (Vector3.left * (bounds.extents.x + 2f));
        }

        protected void UpdateSelectionObjects()
        {
            tileSelector.SetActive(false);
            for (int i = 0; i < tileArrows.Length; i++)
            {
                tileArrows[i].SetActive(false);
            }
            for (int i = 0; i < objectArrows.Length; i++)
            {
                objectArrows[i].SetActive(false);
            }
            gearButton.gameObject.SetActive(false);
            switch (state)
            {
                case SelectorState.None:
                    break;
                case SelectorState.Tile:
                    tileSelector.SetActive(true);
                    break;
                case SelectorState.Area:
                    for (int i = 0; i < tileArrows.Length; i++)
                    {
                        tileArrows[i].SetActive(true);
                    }
                    break;
                case SelectorState.Direction:
                    tileSelector.SetActive(true);
                    for (int i = 0; i < tileArrows.Length; i++)
                    {
                        tileArrows[i].SetActive(true);
                    }
                    break;
                case SelectorState.Settings:
                    gearButton.gameObject.SetActive(true);
                    break;
                case SelectorState.Object:
                    for (int i = 0; i < objectArrows.Length; i++)
                    {
                        if (objectMoveFlags.HasFlag(objectArrowOrder[i]))
                        {
                            objectArrows[i].SetActive(true);
                        }
                    }
                    PositionObjectArrows();
                    break;
            }
        }

    }
}
