/*
 * CameraRig.cs
 * 
 * This script manages the camera behavior in a Unity game. It ensures the camera smoothly follows and zooms to fit all active tanks within the view.
 * 
 * Features:
 * - Smooth camera movement to follow the average position of all active tanks.
 * - Dynamic zoom to ensure all active tanks are within the camera's view.
 * - Adjustable damp time, screen edge buffer, and minimum camera size.
 * 
 * Components:
 * - float dampTime: Approximate time for the camera to refocus.
 * - float screenEdgeBuffer: Space between the top/bottom most target and the screen edge.
 * - float minSize: The smallest orthographic size the camera can be.
 * - Transform[] tankTransform: All the tank transforms.
 * - TankManager tankManager: Reference to the tank manager.
 * - Camera cam: Used for referencing the camera.
 * - Vector3 averagePos: The position the camera is moving towards.
 * - float zoomSpeed: Reference speed for the smooth damping of the orthographic size.
 * - Vector3 moveVelocity: Reference velocity for the smooth damping of the position.
 * 
 * Methods:
 * - Awake: Initializes the camera reference.
 * - Start: Retrieves tank transforms from the tank manager.
 * - FixedUpdate: Updates the camera's position and size.
 * - Move: Smoothly transitions the camera to the average position of active tanks.
 * - CalculateAveragePosition: Calculates the average position of all active tanks.
 * - Zoom: Smoothly adjusts the camera's orthographic size.
 * - GetRequiredSize: Calculates the required orthographic size to fit all active tanks within the view.
 */

using UnityEngine;

namespace CE6127.Tanks.Advanced
{
    public class CameraRig : MonoBehaviour
    {
        public float dampTime = 0.2f;       // Approximate time for the camera to refocus.
        public float screenEdgeBuffer = 4f; // Space between the top/bottom most target and the screen edge.
        public float minSize = 6.5f;        // The smallest orthographic size the camera can be.
        [HideInInspector] public Transform[] tankTransform; // All the tank transforms.
        public TankManager tankManager;     // Reference to the tank manager.

        protected Camera cam;               // Used for referencing the camera.
        protected Vector3 averagePos;       // The position the camera is moving towards.
        protected float zoomSpeed;          // Reference speed for the smooth damping of the orthographic size.
        protected Vector3 moveVelocity;     // Reference velocity for the smooth damping of the position.

        private void Awake()
        {
            cam = GetComponentInChildren<Camera>();
        }

        private void Start()
        {
            tankTransform = tankManager.GetTanksTransform();
        }

        private void FixedUpdate()
        {
            // Move the camera towards a desired position.
            Move();
            // Change the size of the camera based.
            Zoom();
        }

        protected void Move()
        {
            // Find the average position of the targets.
            CalculateAveragePosition();

            // Smoothly transition to that position.
            transform.position = Vector3.SmoothDamp(transform.position, averagePos, ref moveVelocity, dampTime);
        }

        // Calculate the average position of all tanks.
        private void CalculateAveragePosition()
        {
            Vector3 sumPos = new();
            var activeTanksCount = 0;

            // Loop to sum up all active tanks' position.
            for (var i = 0; i < tankTransform.Length; ++i)
            {
                // Skip non-active object(s).
                if (!tankTransform[i].gameObject.activeSelf)
                    continue;

                sumPos += tankTransform[i].position;
                activeTanksCount++;
            }

            // Get the average by dividing (only if number of active tanks are not zero).
            if (activeTanksCount > 0)
                averagePos = sumPos / activeTanksCount;

            // Retain the Y position.
            averagePos.y = transform.position.y;
        }

        protected void Zoom()
        {
            // Zoom based on the required size.
            cam.orthographicSize = 
                Mathf.SmoothDamp(cam.orthographicSize, GetRequiredSize(), ref zoomSpeed, dampTime);
        }

        protected float GetRequiredSize()
        {
            // Size cannot be smaller than the minimum size.
            float orthoSize = minSize;

            // Convert average position to local position of camera rig.
            Vector3 localAveragePos = transform.InverseTransformPoint(averagePos);

            // Loop through all tanks and check which one is closest to the edge of the screen.
            foreach (var target in tankTransform)
            {
                // Skip any tanks that are not active.
                if (!target.gameObject.activeSelf)
                    continue;

                // Get the local position of the target relative to the camera.
                Vector3 targetLocalPos = transform.InverseTransformPoint(target.position);
                // Calculate the size require for this tank.
                Vector3 size = targetLocalPos - localAveragePos;

                // Compare size on both axis and retain the bigger one.
                // Y-axis:
                orthoSize = Mathf.Max(orthoSize, Mathf.Abs(size.y));
                // X-axis:
                orthoSize = Mathf.Max(orthoSize, Mathf.Abs(size.x) / cam.aspect);
            }

            orthoSize += screenEdgeBuffer;

            return orthoSize;
        }
    }
}
