using NUnit.Framework;
using PuzzleFramework.CoreBoard;
using PuzzleFramework.Presentation;
using UnityEngine;

namespace PuzzleFramework.Tests
{
    public sealed class PerspectiveBoardCameraPositionerTests
    {
        [TestCase(GridCellAnchor.Center)]
        [TestCase(GridCellAnchor.Corner)]
        public void TryPosition_FitsAngledXzBoardAtPortraitAspect(GridCellAnchor anchor)
        {
            GameObject cameraObject = new("Camera framing test");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = false;
                camera.fieldOfView = 60f;
                camera.aspect = 0.5f;
                camera.nearClipPlane = 0.3f;
                camera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
                Quaternion originalRotation = camera.transform.rotation;
                GridWorldLayout layout = new(Vector3.zero, Vector2.one,
                    Vector3.right, Vector3.forward, anchor);

                Assert.IsTrue(PerspectiveBoardCameraPositioner.TryPosition(
                    camera, layout, 6, 10, 1.1f, 2f, out string failure), failure);
                Assert.That(Quaternion.Angle(originalRotation, camera.transform.rotation),
                    Is.LessThan(0.0001f));

                float firstCorner = anchor == GridCellAnchor.Center ? -0.5f : 0f;
                AssertCornerFits(camera, layout.BoardLocalToWorld(
                    new Vector2(firstCorner, firstCorner)));
                AssertCornerFits(camera, layout.BoardLocalToWorld(
                    new Vector2(firstCorner + 6f, firstCorner)));
                AssertCornerFits(camera, layout.BoardLocalToWorld(
                    new Vector2(firstCorner, firstCorner + 10f)));
                AssertCornerFits(camera, layout.BoardLocalToWorld(
                    new Vector2(firstCorner + 6f, firstCorner + 10f)));
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void TryPosition_RejectsOrthographicCamera()
        {
            GameObject cameraObject = new("Orthographic camera framing test");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                GridWorldLayout layout = new(Vector3.zero, Vector2.one,
                    Vector3.right, Vector3.forward, GridCellAnchor.Corner);

                Assert.IsFalse(PerspectiveBoardCameraPositioner.TryPosition(
                    camera, layout, 6, 6, 1.1f, 2f, out string failure));
                StringAssert.Contains("perspective", failure);
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }

        private static void AssertCornerFits(Camera camera, Vector3 world)
        {
            Vector3 viewport = camera.WorldToViewportPoint(world);
            Assert.That(viewport.z, Is.GreaterThan(camera.nearClipPlane));
            Assert.That(viewport.x, Is.InRange(0f, 1f));
            Assert.That(viewport.y, Is.InRange(0f, 1f));
        }
    }
}

