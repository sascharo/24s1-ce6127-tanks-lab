using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.MultiPlayer
{
    /// <summary>
    /// Class <c>CameraControl</c> this class is used to control the camera.
    /// </summary>
    public class CameraControl : MonoBehaviour
    {
        public float m_DampTime = 0.2f;                 // Approximate time for the camera to refocus.
        public float m_ScreenEdgeBuffer = 4f;           // Space between the top/bottom most target and the screen edge.
        public float m_MinSize = 6.5f;                  // The smallest orthographic size the camera can be.
        [HideInInspector] public Transform[] m_Targets; // All the targets the camera needs to encompass.

        private Camera m_Camera;            // Used for referencing the camera.
        private float m_ZoomSpeed;          // Reference speed for the smooth damping of the orthographic size.
        private Vector3 m_MoveVelocity;     // Reference velocity for the smooth damping of the position.
        private Vector3 m_DesiredPosition;  // The position the camera is moving towards.

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            m_Camera = GetComponentInChildren<Camera>();
        }

        /// <summary>
        /// Method <c>FixedUpdate</c> is called every fixed framerate frame.
        /// </summary>
        private void FixedUpdate()
        {
            // Move the camera towards a desired position.
            Move();
            // Change the size of the camera based.
            Zoom();
        }

        /// <summary>
        /// Method <c>Move</c> is used to move the camera.
        /// </summary>
        private void Move()
        {
            // Find the average position of the targets.
            FindAveragePosition();

            // Smoothly transition to that position.
            transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
        }

        /// <summary>
        /// Method <c>FindAveragePosition</c> is used to find the average position of the targets.
        /// </summary>
        private void FindAveragePosition()
        {
            Vector3 averagePos = new Vector3();
            var numTargets = 0;

            // Go through all the targets and add their positions together.
            for (var i = 0; i < m_Targets.Length; ++i)
            {
                // If the target isn't active, go on to the next one.
                if (!m_Targets[i].gameObject.activeSelf)
                    continue;

                // Add to the average and increment the number of targets in the average.
                averagePos += m_Targets[i].position;
                numTargets++;
            }

            // If there are targets divide the sum of the positions by the number of them to find the average.
            if (numTargets > 0)
                averagePos /= numTargets;

            // Keep the same y value.
            averagePos.y = transform.position.y;

            // The desired position is the average position;
            m_DesiredPosition = averagePos;
        }

        /// <summary>
        /// Method <c>Zoom</c> is used to zoom the camera.
        /// </summary>
        private void Zoom()
        {
            // Find the required size based on the desired position and smoothly transition to that size.
            float requiredSize = FindRequiredSize();
            m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
        }

        /// <summary>
        /// Method <c>FindRequiredSize</c> is used to find the required size of the camera.
        /// </summary>
        private float FindRequiredSize()
        {
            // Find the position the camera rig is moving towards in its local space.
            Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);

            // Start the camera's size calculation at zero.
            var size = 0f;

            // Go through all the targets...
            for (var i = 0; i < m_Targets.Length; ++i)
            {
                // ... and if they aren't active continue on to the next target.
                if (!m_Targets[i].gameObject.activeSelf)
                    continue;

                // Otherwise, find the position of the target in the camera's local space.
                Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);

                // Find the position of the target from the desired position of the camera's local space.
                Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

                // Choose the largest out of the current size and the distance of the tank 'up' or 'down' from the camera.
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

                // Choose the largest out of the current size and the calculated size based on the tank being to the left
                // or right of the camera.
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
            }
        
            // Add the edge buffer to the size.
            size += m_ScreenEdgeBuffer;

            // Make sure the camera's size isn't below the minimum.
            size = Mathf.Max(size, m_MinSize);

            // Return the new size.
            return size;
        }

        /// <summary>
        /// Method <c>SetStartPositionAndSize</c> is used to set the start position and size of the camera.
        /// </summary>
        public void SetStartPositionAndSize()
        {
            // Find the desired position.
            FindAveragePosition();

            // Set the camera's position to the desired position without damping.
            transform.position = m_DesiredPosition;

            // Find and set the required size of the camera.
            m_Camera.orthographicSize = FindRequiredSize();
        }
    }
}
