using UnityEngine;
using UnityEngine.InputSystem;

using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.Basic
{
    public class Tank : MonoBehaviour
    {
        public float MoveSpeed = 20f;               // The speed of the tank's movement.
        public float RotationSpeed = 90f;           // The speed of the tank's rotation.
        [Header("Input")] 
        public bool InputIsEnabled = true;          // Enable or disable input.

        // State machine state.
        public enum SMState
        {
            Idle = 0,   // Default state.
            Moving      // Moving state.
        };
        protected SMState m_State;                  // The current state of the tank.

        protected Rigidbody m_Rigidbody;            // Reference to the tank's rigidbody.
        protected TankFiringSystem m_TankShot;      // Reference to the tank's firing system.

        private InputActionMap m_InputActionMap;    // Reference to the player number's InputActionMap.
        private InputAction m_MoveTurnAction;       // Input action used to move the tank.
        private Vector2 m_MoveTurnInputValue;       // Move and turn compound input value (new input system).
        private InputAction m_FireAction;           // Input action used to fire a shell from the tank.
        private bool m_FireKeyDown;                 // Fire key down state (new input system).

        const float k_MaxDepenetrationVelocity = float.PositiveInfinity; // This is to fix a change to the physics engine.

        // Awake is called right at the beginning if the object is active.
        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();

            m_TankShot = GetComponent<TankFiringSystem>();
        }

        // Start is called before the first frame update.
        void Start()
        {
            // This line fixes a change to the physics engine.
            Physics.defaultMaxDepenetrationVelocity = k_MaxDepenetrationVelocity;

            // Get the player action map corresponding to the player number.
            m_InputActionMap = GetComponent<PlayerInput>().actions.FindActionMap("Player1");
            // Enable the action map.
            m_InputActionMap.Enable();

            // Get the 'MoveTurn' action.
            m_MoveTurnAction = m_InputActionMap.FindAction("MoveTurn");
            // Zero out the move and turn input value.
            m_MoveTurnInputValue = Vector2.zero;

            // Get the 'Fire' action.
            m_FireAction = m_InputActionMap.FindAction("Fire");
            m_FireKeyDown = false;
        }

        private void OnDisable()
        {
            // Disable the action map.
            m_InputActionMap.Disable();
        }

        // Update is called once per frame.
        void Update()
        {
            MovementInput();
            FireInput();
        }

        protected void MovementInput()
        {
            // Update input.
            m_MoveTurnInputValue = m_MoveTurnAction.ReadValue<Vector2>();

            // Check movement and change states according to it.
            if (Mathf.Abs(m_MoveTurnInputValue.y) > 0.1f || Mathf.Abs(m_MoveTurnInputValue.x) > 0.1f)
                State = SMState.Moving; // Change state to moving.
            else
                State = SMState.Idle; // Change state to idle.
        }

        protected void FireInput()
        {
            // Register a discard event to set m_FireKeyDown to true.
            m_FireAction.started += _ => m_FireKeyDown = true;

            // Fire shots.
            if (m_FireKeyDown)
            {
                m_TankShot.Fire();

                // Reset the fire key down state.
                m_FireKeyDown = false;
            }
        }

        // Physic update. Update regardless of FPS.
        void FixedUpdate()
        {
            Move();
            Rotate();
        }

        // Move the tank based on speed.
        public void Move()
        {
            // Calculate the move vector.
            Vector3 moveVect = transform.forward * MoveSpeed * Time.deltaTime * m_MoveTurnInputValue.y;
            // Move the tank into position.
            m_Rigidbody.MovePosition(m_Rigidbody.position + moveVect);
        }

        // Rotate the tank.
        public void Rotate()
        {
            // Calculate the rotation on dgrees.
            float rotationDegree = RotationSpeed * Time.deltaTime * m_MoveTurnInputValue.x;
            // Convert the Euler rotation to a quaternion.
            Quaternion rotQuat = Quaternion.Euler(0f, rotationDegree, 0f);
            // Rotate the tank.
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * rotQuat);
        }

        public SMState State
        {
            // Get the state.
            get { return m_State; }
            // Set the state.
            set
            {
                if (m_State != value)
                {
                    switch (value)
                    {
                        case SMState.Idle:
                            break;
                        case SMState.Moving:
                            break;
                        default:
                            break;
                    }

                    m_State = value;
                }
            }
        }

        // Is called every frame to draw the GUI for debugging.
        private void OnGUI() => GUILayout.Label($"<color='fuchsia'><size=35>{State}</size></color>");
    }
}
