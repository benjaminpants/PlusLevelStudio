using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    [Flags]
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

    [Flags]
    public enum RotateAxis
    {
        None = 0,
        Pitch = 1,
        Yaw = 2,
        Roll = 4,
        Full = Pitch | Yaw | Roll,
        Flat = Yaw
    }

    public class MoveHandles : MonoBehaviour
    {

        protected MoveAxis _enabledMove;
        protected RotateAxis _enabledRotate;

        public bool moveEnabled = true;
        public bool rotateEnabled = true;

        public MoveAxis enabledMoveAxis
        {
            get
            {
                return _enabledMove;
            }
        }

        public RotateAxis enabledRotateAxis
        {
            get
            {
                return _enabledRotate;
            }
        }
        public void SetArrows(MoveAxis flags)
        {
            _enabledMove = flags;
            if (!moveEnabled)
            {
                flags = MoveAxis.None;
            }
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

        public void SetRings(RotateAxis flags)
        {
            _enabledRotate = flags;
            if (!rotateEnabled)
            {
                flags = RotateAxis.None;
            }
            // yaw
            rings[0].gameObject.SetActive(flags.HasFlag(RotateAxis.Yaw));
            // pitch
            rings[2].gameObject.SetActive(flags.HasFlag(RotateAxis.Pitch));
            // roll
            rings[1].gameObject.SetActive(flags.HasFlag(RotateAxis.Roll));
        }

        public HandleArrow[] arrows = new HandleArrow[3];
        public HandleLattice[] lattices = new HandleLattice[3];
        public HandleRing[] rings = new HandleRing[3];
        public Selector mySelector;
        public Transform dummyTransform; // used for calculations
        public bool worldSpace = false;

        Vector3 currentHandleMouseStart;
        Plane currentRingPlane;
        Quaternion targetQuaternion;

        public void ClickBegin(HandleArrow arrow)
        {
            Vector3? start = EditorController.Instance.CastMouseRayToPlane(new Plane(arrow.transform.up, arrow.transform.position), true);
            if (start == null)
            {
                currentHandleMouseStart = Vector3.zero;
                return;
            }
            currentHandleMouseStart = arrow.transform.position - LockPositionOntoForward(arrow.transform, start.Value);
        }

        // From Deepseek. I don't like using AI for my code, so I will be EXPLICTELY AND REPEATEDLY mentioning it every time it is included.
        // But. I got really, *really* stuck.
        public Vector2 PointOnPlaneToPlaneSpace(Plane plane, Vector3 point, Vector3 org)
        {
            // Step 1: Get the origin point on the plane (closest to org)
            Vector3 origin = plane.ClosestPointOnPlane(org);

            // Step 2: Compute vector from origin to the target point
            Vector3 vec = point - origin;

            // Step 3: Create and invert rotation aligning Vector3.up to the plane's normal
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, plane.normal);
            Quaternion invRotation = Quaternion.Inverse(rotation);
            Vector3 localVec = invRotation * vec;

            // Step 4: Use X and Z components of the rotated vector as 2D coordinates
            return new Vector2(localVec.x, localVec.z);
        }


        Quaternion startRotation;
        float startAngle = 0f;
        public void RingClickBegin(HandleRing ring)
        {
            currentRingPlane = new Plane(ring.transform.up, ring.transform.position);
            Vector3? start = EditorController.Instance.CastMouseRayToPlane(currentRingPlane, true);
            if (start == null)
            {
                //currentHandleMouseStart = Vector3.zero;
                startAngle = 0f;
                return;
            }
            //currentHandleMouseStart = currentRingPlane.ClosestPointOnPlane(ring.transform.position) - start.Value;

            // calculate the initial angle:
            Vector3 pointOnPlane = PointOnPlaneToPlaneSpace(currentRingPlane, start.Value, transform.position);
            startAngle = Mathf.Atan2(pointOnPlane.x, pointOnPlane.y) * Mathf.Rad2Deg;
            startRotation = targetQuaternion;
        }

        public void RingClickUpdate(HandleRing ring)
        {
            Vector3? pos = EditorController.Instance.CastMouseRayToPlane(currentRingPlane, true);
            if (pos == null)
            {
                return;
            }
            Vector3 pointOnPlane = PointOnPlaneToPlaneSpace(currentRingPlane, pos.Value, transform.position);
            float dir = Mathf.Atan2(pointOnPlane.x, pointOnPlane.y) * Mathf.Rad2Deg; // do not unflip these it breaks it for some reason i know what the documentation says
            dir -= startAngle;
            // apply snapping
            if (angleSnap > 0f)
            {
                dir = Mathf.Round(dir / angleSnap) * angleSnap;
            }

            // only case of hardcoding worldSpace checks
            // TODO: fix, appears to be broken
            if (worldSpace)
            {
                Vector3 rotation = transform.eulerAngles;
                rotation.Scale(ring.axisMultipliers);
                rotation += ring.axisVector * dir;
                Vector3 originalRotation = transform.eulerAngles;
                originalRotation.Scale(ring.axisVector);
                mySelector.UpdateObjectRotation(Quaternion.Euler(rotation + originalRotation));
                return;
            }

            Quaternion finalQuaternion = startRotation;
            finalQuaternion *= Quaternion.AngleAxis(dir, ring.axisVector);
            mySelector.UpdateObjectRotation(finalQuaternion);
        }

        public float gridSnap => EditorController.Instance.gridSnap;
        public float angleSnap => EditorController.Instance.angleSnap;

        // TODO: make these ONLY snap the axis' you are changing.
        public void LatticeClickUpdate(Transform lattice)
        {
            Vector3? pos = EditorController.Instance.CastMouseRayToPlane(new Plane(lattice.forward, lattice.position), true);
            if (pos == null)
            {
                return;
            }

            //mySelector.UpdateObjectPosition(transform.position + (pos.Value - lattice.transform.position).SnapToGrid(gridSnap));
            mySelector.UpdateObjectPosition((transform.position + pos.Value - lattice.transform.position).SnapToGrid(gridSnap));
        }

        public void ClickUpdate(HandleArrow arrow)
        {
            Vector3? pos = EditorController.Instance.CastMouseRayToPlane(new Plane(arrow.transform.up, arrow.transform.position), true);
            if (pos == null)
            {
                return;
            }
            //mySelector.UpdateObjectPosition(transform.position + (LockPositionOntoForward(arrow.transform, pos.Value) - (arrow.transform.position - currentHandleMouseStart)).SnapToGrid(gridSnap));
            mySelector.UpdateObjectPosition((transform.position + LockPositionOntoForward(arrow.transform, pos.Value) - (arrow.transform.position - currentHandleMouseStart)).SnapToGrid(gridSnap));
        }

        protected Vector3 LockPositionOntoForward(Transform relativeTo, Vector3 point)
        {
            // get the forward rotation in local space
            Vector3 localForward = relativeTo.InverseTransformDirection(relativeTo.forward);
            // get our point relative to the transform
            Vector3 localPoint = relativeTo.InverseTransformPoint(point);

            Vector3 output = new Vector3(localPoint.x, localPoint.y, localPoint.z);
            // there should be 2 zeros and one one
            output.Scale(new Vector3(Mathf.Abs(localForward.x), Mathf.Abs(localForward.y), Mathf.Abs(localForward.z)));
            return relativeTo.TransformPoint(output); // convert back to world space
        }

        public void GoToTarget(Transform t)
        {
            transform.position = t.position;
            targetQuaternion = t.rotation;
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
            
        }
    }

    public class HandleRing : MonoBehaviour, IEditorInteractable
    {
        public MoveHandles myHandles;

        public Vector3 axisVector;
        public Vector3 axisMultipliers => new Vector3(1f - axisVector.x, 1f - axisVector.y, 1f - axisVector.z);

        public bool InteractableByTool(EditorTool tool)
        {
            // shouldn't be called by this, since the layer we SHOULD be on doesn't support being clicked by tools
            throw new NotImplementedException();
        }

        public bool OnClicked()
        {
            myHandles.RingClickBegin(this);
            return true;
        }

        public bool OnHeld()
        {
            myHandles.RingClickUpdate(this);
            return true;
        }

        public void OnReleased()
        {
            
        }
    }

}
