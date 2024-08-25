/*
 * Tank.cs
 * 
 * Controls the behavior of a tank in a Unity game. It handles the tank's movement, rotation, and firing mechanisms using Unity's new input system.
 * 
 * Features:
 * - Movement and rotation control based on player input.
 * - State machine to manage the tank's idle and moving states.
 * - Integration with a firing system to allow the tank to fire projectiles.
 * - Debugging GUI to display the current state of the tank.
 * 
 * Components:
 * - Rigidbody: Used for physics-based movement.
 * - TankFiringSystem: Manages the firing of projectiles.
 * - PlayerInput: Handles player input actions.
 * 
 * Methods:
 * - Awake: Initializes references to components.
 * - Start: Sets up the input action map and enables it.
 * - OnDisable: Disables the input action map.
 * - Update: Handles movement and firing input.
 * - FixedUpdate: Updates movement and rotation based on physics.
 * - Move: Moves the tank forward or backward.
 * - Rotate: Rotates the tank left or right.
 * - FireInput: Handles firing input and triggers the firing system.
 * - OnGUI: Displays the current state for debugging purposes.
 */

using UnityEngine;
using UnityEngine.InputSystem;

namespace CE6127.Tanks.Basic
{
    public class Tank : MonoBehaviour
    {
        public float moveSpeed = 20f;           // The speed of the tank's movement.
        public float rotationSpeed = 90f;       // The speed of the tank's rotation.

        // CurrentTankState machine state.
        public enum TankState
        {
            Idle = 0,   // Default state.
            Moving      // Moving state.
        };
        protected TankState currentState;       // The current state of the tank.

        public TankState CurrentTankState
        {
            // Get the state.
            get { return currentState; }
            // Set the state.
            set
            {
                if (currentState != value)
                {
                    switch (value)
                    {
                        case TankState.Idle:
                            break;
                        case TankState.Moving:
                            break;
                        default:
                            break;
                    }

                    currentState = value;
                }
            }
        }

        protected Rigidbody rbody;              // Reference to the tank's rigidbody.
        protected TankFiringSystem tankFiring;  // Reference to the tank's firing system.

        private InputActionMap inputActionMap;  // Reference to the player number's InputActionMap.
        private InputAction moveTurnAction;     // Input action used to move the tank.
        private Vector2 moveTurnInputValue;     // Move and turn compound input value (new input system).
        private InputAction fireAction;         // Input action used to fire a shell from the tank.
        private bool fireKeyDown;               // Fire key down state (new input system).

        const float k_MaxDepenetrationVelocity = float.PositiveInfinity; // This is to fix a change to the physics engine.

        // Awake is called right at the beginning if the object is active.
        private void Awake()
        {
            rbody = GetComponent<Rigidbody>();

            tankFiring = GetComponent<TankFiringSystem>();
        }

        // Start is called before the first frame update.
        void Start()
        {
            // This line fixes a change to the physics engine.
            Physics.defaultMaxDepenetrationVelocity = k_MaxDepenetrationVelocity;

            // Get the player action map corresponding to the player number.
            inputActionMap = GetComponent<PlayerInput>().actions.FindActionMap("Player1");
            // Enable the action map.
            inputActionMap.Enable();

            // Get the 'MoveTurn' action.
            moveTurnAction = inputActionMap.FindAction("MoveTurn");
            // Zero out the move and turn input value.
            moveTurnInputValue = Vector2.zero;

            // Get the 'Fire' action.
            fireAction = inputActionMap.FindAction("Fire");
            fireKeyDown = false;
        }

        private void OnDisable()
        {
            // Disable the action map.
            inputActionMap.Disable();
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
            moveTurnInputValue = moveTurnAction.ReadValue<Vector2>();

            // Check movement and change states according to it.
            if (Mathf.Abs(moveTurnInputValue.y) > 0.1f || Mathf.Abs(moveTurnInputValue.x) > 0.1f)
                CurrentTankState = TankState.Moving; // Change state to moving.
            else
                CurrentTankState = TankState.Idle; // Change state to idle.
        }

        protected void FireInput()
        {
            // Register a discard event to set fireKeyDown to true.
            fireAction.started += _ => fireKeyDown = true;

            // Fire shots.
            if (fireKeyDown)
            {
                tankFiring.Fire();

                // Reset the fire key down state.
                fireKeyDown = false;
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
            Vector3 moveVect = transform.forward * moveSpeed * Time.deltaTime * moveTurnInputValue.y;
            // Move the tank into position.
            rbody.MovePosition(rbody.position + moveVect);
        }

        // Rotate the tank.
        public void Rotate()
        {
            // Calculate the rotation on dgrees.
            float rotationDegree = rotationSpeed * Time.deltaTime * moveTurnInputValue.x;
            // Convert the Euler rotation to a quaternion.
            Quaternion rotQuat = Quaternion.Euler(0f, rotationDegree, 0f);
            // Rotate the tank.
            rbody.MoveRotation(rbody.rotation * rotQuat);
        }

        // Is called every frame to draw the GUI for debugging.
        private void OnGUI() => GUILayout.Label($"<color='fuchsia'><size=35>{CurrentTankState}</size></color>");
    }
}
