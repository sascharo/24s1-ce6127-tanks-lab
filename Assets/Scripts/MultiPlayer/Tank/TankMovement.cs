using UnityEngine;
using UnityEngine.InputSystem;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.MultiPlayer
{
    /// <summary>
    /// Class <c>TankMovement</c> this class is used to control the movement of the tank.
    /// </summary>
    public class TankMovement : MonoBehaviour
    {
        public float m_Speed = 12f;                                 // How fast the tank moves forward and back.
        public float m_TurnSpeed = 180f;                            // How fast the tank turns in degrees per second.
        public AudioSource m_MovementAudio;                         // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
        public AudioClip m_EngineIdling;                            // Audio to play when the tank isn't moving.
        public AudioClip m_EngineDriving;                           // Audio to play when the tank is moving.
        public float m_PitchRange = 0.2f;                           // The amount by which the pitch of the engine noises can vary.
        [HideInInspector] public InputActionMap m_InputActionMap;   // Reference to the player number's InputActionMap

        private InputAction m_MoveTurnAction;   // Input action used to move the tank.
        private Rigidbody m_Rigidbody;          // Rigidbody reference used to move the tank.
        private Vector2 m_MoveTurnInputValue;   // Move and turn compound input value.
        private float m_OriginalPitch;          // The pitch of the audio source at the start of the scene.

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Method <c>OnEnable</c> is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            // When the tank is turned on, make sure it's not kinematic.
            m_Rigidbody.isKinematic = false;

            // Enable Action Map.
            m_InputActionMap.Enable();

            // Get a reference to the move and turn input action.
            m_MoveTurnAction = m_InputActionMap.FindAction("MoveTurn");


            // Also reset the input values.
            m_MoveTurnInputValue = Vector2.zero;
        }

        /// <summary>
        /// Method <c>OnDisable</c> is called when the behaviour becomes disabled or inactive.
        /// </summary>
        private void OnDisable()
        {
            // When the tank is turned off, set it to kinematic so it stops moving.
            m_Rigidbody.isKinematic = true;

            // Disable Action Map.
            m_InputActionMap.Disable();
        }

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            // Store the original pitch of the audio source.
            m_OriginalPitch = m_MovementAudio.pitch;
        }
   
        /// <summary>
        /// Method <c>Update</c> store the player's input and make sure the audio for the engine is playing.
        /// </summary>
        private void Update()
        {
            // Store the value of the input.
            m_MoveTurnInputValue = m_MoveTurnAction.ReadValue<Vector2>();

            EngineAudio();
        }

        /// <summary>
        /// Method <c>EngineAudio</c> play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.
        /// </summary>
        private void EngineAudio()
        {
            // If there is no input (the tank is stationary)...
            if (Mathf.Abs(m_MoveTurnInputValue.y) < 0.1f && Mathf.Abs(m_MoveTurnInputValue.x) < 0.1f)
            {
                // ... and if the audio source is currently playing the driving clip...
                if (m_MovementAudio.clip == m_EngineDriving)
                {
                    // ... change the clip to idling and play it.
                    m_MovementAudio.clip = m_EngineIdling;
                    m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play();
                }
            }
            else
            {
                // Otherwise if the tank is moving and if the idling clip is currently playing...
                if (m_MovementAudio.clip == m_EngineIdling)
                {
                    // ... change the clip to driving and play.
                    m_MovementAudio.clip = m_EngineDriving;
                    m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play();
                }
            }
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
            Vector3 move = transform.forward * m_MoveTurnInputValue.y * m_Speed * Time.deltaTime;

            // Apply this movement to the rigidbody's position.
            m_Rigidbody.MovePosition(m_Rigidbody.position + move);
        }

        /// <summary>
        /// Method <c>Turn</c> adjust the rotation of the tank based on the player's input.
        /// </summary>
        private void Turn()
        {
            // Determine the number of degrees to be turned based on the input, speed and time between frames.
            var turn = m_MoveTurnInputValue.x * m_TurnSpeed * Time.deltaTime;

            // Make this into a rotation in the y axis.
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

            // Apply this rotation to the rigidbody's rotation.
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
        }
    }
}
