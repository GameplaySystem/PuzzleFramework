using UnityEngine;

namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Rule-free pointer geometry for board-plane dragging. Callers own input sampling,
    /// target hit tests and the choice of board plane.
    /// </summary>
    public static class BoardPointerProjection
    {
        /// <summary>Projects a ray onto the caller's board plane.</summary>
        public static bool TryProject(
            Ray pointerRay,
            Vector3 planeOrigin,
            Vector3 planeNormal,
            out Vector3 boardWorldPosition)
        {
            boardWorldPosition = default;
            if (planeNormal.sqrMagnitude <= Mathf.Epsilon)
            {
                return false;
            }

            Plane plane = new(planeNormal.normalized, planeOrigin);
            if (!plane.Raycast(pointerRay, out float distance) || distance < 0f)
            {
                return false;
            }

            boardWorldPosition = pointerRay.GetPoint(distance);
            return true;
        }

        /// <summary>Preserves the point at which the pointer grabbed a draggable object.</summary>
        public static Vector3 CaptureOffset(Vector3 objectWorldPosition, Vector3 pointerWorldPosition)
        {
            return objectWorldPosition - pointerWorldPosition;
        }

        /// <summary>Applies the captured offset to a later pointer projection.</summary>
        public static Vector3 ApplyOffset(Vector3 pointerWorldPosition, Vector3 capturedOffset)
        {
            return pointerWorldPosition + capturedOffset;
        }
    }
}
