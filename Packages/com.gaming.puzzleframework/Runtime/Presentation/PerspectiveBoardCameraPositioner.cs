using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Presentation
{
    /// <summary>
    /// Positions a perspective camera so a padded logical board rectangle fits while preserving
    /// the camera's rotation and lens settings.
    /// </summary>
    public static class PerspectiveBoardCameraPositioner
    {
        private const float MinimumTangent = 0.0001f;

        /// <summary>
        /// Moves <paramref name="targetCamera"/> along its current forward axis until every padded
        /// board corner fits the current perspective aspect ratio.
        /// </summary>
        public static bool TryPosition(
            Camera targetCamera,
            GridWorldLayout worldLayout,
            int boardWidth,
            int boardHeight,
            float framingPadding,
            float minimumDistance,
            out string failureReason)
        {
            if (targetCamera == null)
            {
                failureReason = "A target camera is required for board framing.";
                return false;
            }

            if (targetCamera.orthographic)
            {
                failureReason =
                    "Board camera positioning requires a perspective camera because position " +
                    "does not change orthographic framing.";
                return false;
            }

            if (boardWidth <= 0 || boardHeight <= 0)
            {
                failureReason = "Board camera positioning requires positive board dimensions.";
                return false;
            }

            float aspect = targetCamera.aspect;
            float verticalTangent = Mathf.Tan(targetCamera.fieldOfView * Mathf.Deg2Rad * 0.5f);
            float horizontalTangent = verticalTangent * aspect;
            if (aspect <= 0f ||
                verticalTangent <= MinimumTangent ||
                horizontalTangent <= MinimumTangent)
            {
                failureReason = "The target camera has invalid perspective projection settings.";
                return false;
            }

            float safePadding = Mathf.Max(1f, framingPadding);
            float halfBoardWidth = boardWidth * worldLayout.CellSize.x * 0.5f * safePadding;
            float halfBoardHeight = boardHeight * worldLayout.CellSize.y * 0.5f * safePadding;
            float anchorOffset = worldLayout.CellAnchor == GridCellAnchor.Center ? 0.5f : 0f;
            Vector3 boardCenter = worldLayout.BoardOrigin +
                                  worldLayout.BoardXAxis *
                                  ((boardWidth * 0.5f - anchorOffset) * worldLayout.CellSize.x) +
                                  worldLayout.BoardYAxis *
                                  ((boardHeight * 0.5f - anchorOffset) * worldLayout.CellSize.y);

            Quaternion worldToCameraRotation = Quaternion.Inverse(targetCamera.transform.rotation);
            float requiredDistance = Mathf.Max(
                minimumDistance,
                targetCamera.nearClipPlane + 0.01f);

            for (int xSign = -1; xSign <= 1; xSign += 2)
            {
                for (int ySign = -1; ySign <= 1; ySign += 2)
                {
                    Vector3 cornerOffset =
                        worldLayout.BoardXAxis * (halfBoardWidth * xSign) +
                        worldLayout.BoardYAxis * (halfBoardHeight * ySign);
                    Vector3 cameraLocalOffset = worldToCameraRotation * cornerOffset;

                    requiredDistance = Mathf.Max(
                        requiredDistance,
                        Mathf.Abs(cameraLocalOffset.x) / horizontalTangent - cameraLocalOffset.z,
                        Mathf.Abs(cameraLocalOffset.y) / verticalTangent - cameraLocalOffset.z,
                        targetCamera.nearClipPlane + 0.01f - cameraLocalOffset.z);
                }
            }

            targetCamera.transform.position =
                boardCenter - targetCamera.transform.forward * requiredDistance;
            failureReason = string.Empty;
            return true;
        }
    }
}

