using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.Advanced
{
    public class CameraRig : MonoBehaviour
    {
        public float DampTime = 0.2f;                   // Approximate time for the camera to refocus.
        public float ScreenEdgeBuffer = 4f;             // Space between the top/bottom most target and the screen edge.
        public float MinSize = 6.5f;                    // The smallest orthographic size the camera can be.
        [HideInInspector] public Transform[] TankTrans; // All the tank transforms.
        public TankManager TankManager;                 // Reference to the tank manager.

        protected Camera m_Camera;                      // Used for referencing the camera.
        protected Vector3 m_AveragePos;                 // The position the camera is moving towards.
        protected float m_ZoomSpeed;                    // Reference speed for the smooth damping of the orthographic size.
        protected Vector3 m_MoveVelocity;               // Reference velocity for the smooth damping of the position.

        private void Awake()
        {
            m_Camera = GetComponentInChildren<Camera>();
        }

        private void Start()
        {
            TankTrans = TankManager.GetTanksTransform();
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
            transform.position = Vector3.SmoothDamp(transform.position, m_AveragePos, ref m_MoveVelocity, DampTime);
        }

        // Calculate the average position of all tanks.
        private void CalculateAveragePosition()
        {
            Vector3 sumPos = new();
            var activeTanksCount = 0;

            // Loop to sum up all active tanks' position.
            for (var i = 0; i < TankTrans.Length; ++i)
            {
                // Skip non-active object(s).
                if (!TankTrans[i].gameObject.activeSelf)
                    continue;

                sumPos += TankTrans[i].position;
                activeTanksCount++;
            }

            // Get the average by dividing (only if number of active tanks are not zero).
            if (activeTanksCount > 0)
                m_AveragePos = sumPos / activeTanksCount;

            // Retain the Y position.
            m_AveragePos.y = transform.position.y;
        }

        protected void Zoom()
        {
            // Zoom based on the required size.
            m_Camera.orthographicSize = 
                Mathf.SmoothDamp(m_Camera.orthographicSize, GetRequiredSize(), ref m_ZoomSpeed, DampTime);
        }

        protected float GetRequiredSize()
        {
            // Size cannot be smaller than the minimum size.
            float orthoSize = MinSize;

            // Convert average position to local position of camera rig.
            Vector3 localAveragePos = transform.InverseTransformPoint(m_AveragePos);

            // Loop through all tanks and check which one is closest to the edge of the screen.
            foreach (var target in TankTrans)
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
                orthoSize = Mathf.Max(orthoSize, Mathf.Abs(size.x) / m_Camera.aspect);
            }

            orthoSize += ScreenEdgeBuffer;

            return orthoSize;
        }
    }
}
