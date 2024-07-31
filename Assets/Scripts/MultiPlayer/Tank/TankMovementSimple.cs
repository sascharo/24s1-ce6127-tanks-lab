using UnityEngine;
using UnityEngine.InputSystem;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.MultiPlayer
{
    /// <summary>
    /// Class <c>TankMovementSimple</c> this class is used to control the tank movement.
    /// </summary>
    public class TankMovementSimple : MonoBehaviour
    {
        public float m_Speed = 12f;                                 // How fast the tank moves forward and back.
        public float m_TurnSpeed = 180f;                            // How fast the tank turns in degrees per second.
        [HideInInspector] public InputActionMap m_InputActionMap;   // Reference to the player number's InputActionMap.

        private InputAction m_MoveTurnAction;   // Input action used to move the tank.
        private Rigidbody m_Rigidbody;          // Rigidbody reference used to move the tank.
        private Vector2 m_MoveTurnInputValue;   // Move and turn compound input value.

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();

            PlayerInput playerInput = GetComponent<PlayerInput>();
            m_InputActionMap = playerInput.actions.FindActionMap("Player1");
            // m_InputActionMap = playerInput.actions.actionMaps[0]; // Alternative way to get the action map.
        }

        /// <summary>
        /// Method <c>OnEnable</c> is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            m_Rigidbody.isKinematic = false;

            m_InputActionMap.Enable();
            m_MoveTurnAction = m_InputActionMap.FindAction("MoveTurn");
            m_MoveTurnInputValue = Vector2.zero;
        }

        /// <summary>
        /// Method <c>OnDisable</c> is called when the behaviour becomes disabled or inactive.
        /// </summary>
        private void OnDisable()
        {
            m_Rigidbody.isKinematic = true;

            m_InputActionMap.Disable();
        }

        /// <summary>
        /// Method <c>Update</c> is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void Update()
        {
            m_MoveTurnInputValue = m_MoveTurnAction.ReadValue<Vector2>();
        }

        /// <summary>
        /// Method <c>FixedUpdate</c> is called every fixed framerate frame.
        /// </summary>
        private void FixedUpdate()
        {
            Vector3 move = transform.forward * m_MoveTurnInputValue.y * m_Speed * Time.deltaTime;
            m_Rigidbody.MovePosition(m_Rigidbody.position + move);

            float turn = m_MoveTurnInputValue.x * m_TurnSpeed * Time.deltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
        }
    }
}
