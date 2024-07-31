using UnityEngine;
using UnityEngine.InputSystem;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>TankMovement</c> this class is used to control the movement of the tank.
    /// </summary>
    public class TankMovement : MonoBehaviour
    {
        [HideInInspector] public InputActionMap InputActionMap; // Reference to the player number's InputActionMap

        private GameManager m_GameManager;      // Reference to the GameManager.
        private InputAction m_MoveTurnAction;   // Input action used to move the tank.
        private Rigidbody m_Rigidbody;          // Rigidbody reference used to move the tank.
        private Vector2 m_MoveTurnInputValue;   // Move and turn compound input value.
        private TankSound m_TankSound;          // Reference to the tank's audio source and audio clips.

        /// <summary>
        /// Method <c>MoveTurnSound</c> return the player's input value.
        /// </summary>
        public Vector2 MoveTurnSound() => m_MoveTurnInputValue;

        /// <summary>
        /// Method <c>Awake</c> set up references.
        /// </summary>
        private void Awake()
        {
            m_GameManager = GameManager.Instance;

            m_Rigidbody = GetComponent<Rigidbody>();
            m_TankSound = GetComponent<TankSound>();
        }

        /// <summary>
        /// Method <c>OnEnable</c> when the tank is turned on and make sure it's not kinematic.
        /// </summary>
        private void OnEnable()
        {
            // When the tank is turned on, make sure it's not kinematic.
            m_Rigidbody.isKinematic = false;

            // Enable Action Map.
            InputActionMap.Enable();

            // Get a reference to the move and turn input action.
            m_MoveTurnAction = InputActionMap.FindAction("MoveTurn");

            // Also reset the input values.
            m_MoveTurnInputValue = Vector2.zero;
        }

        /// <summary>
        /// Method <c>Start</c> subscribe to the event.
        /// </summary>
        private void Start()
        {
            m_TankSound.MoveTurnInputCalc += MoveTurnSound;
        }

        /// <summary>
        /// Method <c>OnDisable</c> when the tank is turned off.
        /// </summary>
        private void OnDisable()
        {
            // When the tank is turned off, set it to kinematic so it stops moving.
            m_Rigidbody.isKinematic = true;

            // Disable Action Map.
            InputActionMap.Disable();

            m_TankSound.MoveTurnInputCalc -= MoveTurnSound;
        }
   
        /// <summary>
        /// Method <c>Update</c> store the player's input and make sure the audio for the engine is playing.
        /// </summary>
        private void Update()
        {
            // Store the value of the input.
            m_MoveTurnInputValue = m_MoveTurnAction.ReadValue<Vector2>();
        }

        /// <summary>
        /// Method <c>FixedUpdate</c> move and turn the tank.
        /// </summary>
        private void FixedUpdate()
        {
            // Adjust the rigidbodies position and orientation in FixedUpdate.
            Move();
            Turn();
        }

        /// <summary>
        /// Method <c>Move</c> adjust the position of the tank based on the player's input.
        /// </summary>
        private void Move()
        {
            // Create a vector in the direction the tank is facing with a magnitude based on the input,
            // speed and the time between frames.
            Vector3 move = transform.forward * m_MoveTurnInputValue.y * m_GameManager.Speed * Time.deltaTime;

            // Apply this movement to the rigidbody's position.
            m_Rigidbody.MovePosition(m_Rigidbody.position + move);
        }

        /// <summary>
        /// Method <c>Turn</c> adjust the rotation of the tank based on the player's input.
        /// </summary>
        private void Turn()
        {
            // Determine the number of degrees to be turned based on the input, speed and time between frames.
            var turn = m_MoveTurnInputValue.x * m_GameManager.AngularSpeed * Time.deltaTime;

            // Make this into a rotation in the y axis.
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

            // Apply this rotation to the rigidbody's rotation.
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
        }
    }
}
