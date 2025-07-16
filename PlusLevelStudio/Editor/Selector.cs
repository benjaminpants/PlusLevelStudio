using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

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

    public enum MoveAxis
    {
        None = 0,
        Z = 1,
        Y = 2,
        X = 4,
        All = Z | X | Y,
        Horizontal = X | Z,
        Forward = Z
    }

    public class MoveHandles : MonoBehaviour
    {
        public MoveAxis enabledAxis
        {
            get
            {
                return (arrows[0].gameObject.activeSelf ? MoveAxis.Z : MoveAxis.None) | (arrows[1].gameObject.activeSelf ? MoveAxis.Y : MoveAxis.None) | (arrows[2].gameObject.activeSelf ? MoveAxis.X : MoveAxis.None);
            }
            set
            {
                SetArrows(value);
            }
        }
        public void SetArrows(MoveAxis flags)
        {
            arrows[0].gameObject.SetActive(flags.HasFlag(MoveAxis.Z));
            arrows[0].transform.localPosition = Vector3.forward;
            arrows[1].gameObject.SetActive(flags.HasFlag(MoveAxis.Y));
            arrows[1].transform.localPosition = Vector3.up;
            arrows[2].gameObject.SetActive(flags.HasFlag(MoveAxis.X));
            arrows[2].transform.localPosition = Vector3.right;
            lattices[0].gameObject.SetActive(flags.HasFlag(MoveAxis.X) && flags.HasFlag(MoveAxis.Z)); // this lattice moves stuff in the X and Z axis so we need to make sure we are allowed to move both
            lattices[1].gameObject.SetActive(flags.HasFlag(MoveAxis.X) && flags.HasFlag(MoveAxis.Y)); // this lattice moves stuff in the X and Y axis so we need to make sure we are allowed to move both
            lattices[2].gameObject.SetActive(flags.HasFlag(MoveAxis.Z) && flags.HasFlag(MoveAxis.Y)); // this lattice moves stuff in the Z and Y axis so we need to make sure we are allowed to move both
        }

        public HandleArrow[] arrows = new HandleArrow[3];
        public HandleLattice[] lattices = new HandleLattice[3];
        public Selector mySelector;
        public bool worldSpace = false;

        Vector3 currentHandleMouseStart;

        public void ClickBegin(HandleArrow arrow)
        {
            Vector3? start = EditorController.Instance.CastRayToPlane(new Plane(arrow.transform.up, arrow.transform.position), true);
            if (start == null)
            {
                currentHandleMouseStart = Vector3.zero;
                return;
            }
            currentHandleMouseStart = arrow.transform.position - LockPositionOntoForward(arrow.transform, start.Value);
        }

        public void LatticeClickUpdate(Transform lattice)
        {
            Vector3? pos = EditorController.Instance.CastRayToPlane(new Plane(lattice.forward, lattice.position), true);
            if (pos == null)
            {
                return;
            }
            mySelector.UpdateObjectPosition(pos.Value - lattice.transform.position);
        }

        public void ClickUpdate(HandleArrow arrow)
        {
            Vector3? pos = EditorController.Instance.CastRayToPlane(new Plane(arrow.transform.up, arrow.transform.position), true);
            if (pos == null)
            {
                return;
            }
            mySelector.UpdateObjectPosition(LockPositionOntoForward(arrow.transform, pos.Value) - (arrow.transform.position - currentHandleMouseStart));
        }

        public void ClickEnd(HandleArrow arrow)
        {

        }

        protected Vector3 LockPositionOntoForward(Transform relativeTo, Vector3 point)
        {
            // get the forward rotation in local space
            Vector3 localForward = relativeTo.InverseTransformDirection(relativeTo.forward);
            // get our point relative to the transform
            Vector3 localPoint = relativeTo.InverseTransformPoint(point);

            Vector3 output = new Vector3(localPoint.x,localPoint.y, localPoint.z);
            // there should be 2 zeros and one one
            output.Scale(new Vector3(Mathf.Abs(localForward.x), Mathf.Abs(localForward.y), Mathf.Abs(localForward.z)));
            return relativeTo.TransformPoint(output); // convert back to world space
        }

        public void GoToTarget(Transform t)
        {
            transform.position = t.position;
            if (!worldSpace)
            {
                transform.rotation = t.rotation;
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }
        }

    }

    public class HandleLattice : MonoBehaviour, IEditorInteractable
    {
        public MoveHandles myHandles;
        public bool InteractableByTool(EditorTool tool)
        {
            // shouldn't be called by this, since the layer we SHOULD be on doesn't support being clicked by tools
            throw new NotImplementedException();
        }

        public bool OnClicked()
        {
            return true;
            //throw new NotImplementedException();
        }

        public bool OnHeld()
        {
            myHandles.LatticeClickUpdate(transform);
            return true;
            //throw new NotImplementedException();
        }

        public void OnReleased()
        {
            
        }
    }

    public class HandleArrow : MonoBehaviour, IEditorInteractable
    {
        public MoveHandles myHandles;
        public bool InteractableByTool(EditorTool tool)
        {
            // shouldn't be called by this, since the layer we SHOULD be on doesn't support being clicked by tools
            throw new NotImplementedException();
        }

        public bool OnClicked()
        {
            myHandles.ClickBegin(this);
            return true;
        }

        public bool OnHeld()
        {
            myHandles.ClickUpdate(this);
            return true;
        }

        public void OnReleased()
        {
            myHandles.ClickEnd(this);
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
        public SettingsWorldButton gearButton;
        public MoveHandles moveHandles;

        public IntVector2 selectedTile { get; private set; } = new IntVector2(0, 0);

        public RectInt selectedArea { get; private set; } = new RectInt(new Vector2Int(0,0), new Vector2Int(0, 0));

        protected Direction currentArrow = Direction.Null;
        protected IntVector2 currentStartPosition = new IntVector2();
        protected SelectorState state;

        protected Action<IntVector2, IntVector2> resizeAction;
        protected Action<Direction> directionAction;
        public IEditorMovable selectedMovable { get; private set; }


        void Awake()
        {
            UpdateSelectionObjects();
        }

        private void NullActions()
        {
            resizeAction = null;
            directionAction = null;
            if (selectedMovable != null)
            {
                selectedMovable.Unselected();
            }
            selectedMovable = null;
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

        public void SelectObject(IEditorMovable movable, MoveAxis enabledAxis)
        {
            // TODO: REMOVE HACK! ACK ACK ACK ITS SO HACKY IN HERE
            EditorController.Instance.UnhighlightAllCells();
            NullActions();
            selectedMovable = movable;
            state = SelectorState.Object;
            moveHandles.SetArrows(enabledAxis);
            UpdateSelectionObjects();
            selectedMovable.Selected();
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
            NullActions();
            selectedArea = rect;
            state = SelectorState.Area;
            this.resizeAction = resizeAction;
            UpdateSelectionObjects();
            for (int i = 0; i < tileArrows.Length; i++)
            {
                PositionArrow((Direction)i, 0f);
            }
        }

        public void ShowSettings(Vector3 position, Action onClicked)
        {
            // TODO: REMOVE HACK! ACK ACK ACK ITS SO HACKY IN HERE
            EditorController.Instance.UnhighlightAllCells();
            state = SelectorState.Settings;
            gearButton.clickedAction = onClicked;
            UpdateSelectionObjects();
            gearButton.transform.position = position;
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

        public void UpdateObjectPosition(Vector3 offset)
        {
            if (offset.sqrMagnitude == 0) return; // no change made, dont bother sending down
            selectedMovable.MoveUpdate(offset, EditorController.Instance.gridSnap);
        }

        void PositionArrow(Direction d, float additionalDistanceFromEdge)
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
                case SelectorState.Object:
                    Transform tf = selectedMovable.GetTransform();
                    moveHandles.GoToTarget(tf);
                    break;
            }
        }

        protected void UpdateSelectionObjects()
        {
            tileSelector.SetActive(false);
            for (int i = 0; i < tileArrows.Length; i++)
            {
                tileArrows[i].SetActive(false);
            }
            moveHandles.gameObject.SetActive(false);
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
                    moveHandles.gameObject.SetActive(true);
                    break;
            }
        }

    }
}
