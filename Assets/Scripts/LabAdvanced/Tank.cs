using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.Advanced
{
    public class Tank : MonoBehaviour
    {
        public float MoveSpeed = 20f;                               // How fast the tank moves forward and back.
        public float RotationSpeed = 90f;                           // How fast the tank turns in degrees per second.
        [Header("Audio")]
        [Tooltip("Engine Idle")] public AudioSource MovementSFX;    // Reference to the audio source used to play the engine sound.
        [Tooltip("Shot Firing")] public AudioSource TankSFX;        // Reference to the audio source used to play the shooting sound.
        public AudioClip ClipIdle;                                  // Audio clip for when the tank isn't moving.
        public AudioClip ClipMoving;                                // Audio clip for when the tank is moving.
        public AudioClip ClipShotFired;                             // Audio clip for when the tank is shooting.
        public AudioClip ClipTankExplode;                           // Audio clip for when the tank explodes.
        public float PitchRange = 0.2f;                             // The amount by which the pitch of the engine noises can vary.
        [Header("Input")]
        public bool InputIsEnabled = true;                          // Whether or not input is enabled.
        [HideInInspector] public int PlayerNum;                     // This specifies which player this the manager for.
        [Header("Health")]
        public float MaxHealth = 100f;                              // The maximum health of the tank.
        public float Health = 100f;                                 // The current health of the tank.

        // Delegates.
        public Action<Tank> DestroyTank;                            // Delegate called when the tank is destroyed.

        // State machine state.
        public enum SMState
        {
            Idle = 0,       // The tank is idle. Default state.
            Moving,         // The tank is moving.
            TakingDamage,   // The tank is taking damage.
            Destroyed,      // The tank is destroyed.
            Inactive        // The tank is inactive.
        };
        protected SMState m_State;                  // The current state of the tank.

        // Audio.
        protected float m_OriginalPitch;            // The pitch of the audio source at the start of the scene.

        private Rigidbody m_Rigidbody;              // Reference used to move the tank.
        private TankFiringSystem m_TankShot;        // Reference to the tank's shooting script, used to disable and enable the shell.
        private InputActionMap m_InputActionMap;    // Reference to the player number's InputActionMap.
        private InputAction m_MoveTurnAction;       // Input action used to move the tank.
        private Vector2 m_MoveTurnInputValue;       // Move and turn compound input value (new input system).
        private InputAction m_FireAction;           // Input action used to fire a shell from the tank.
        private bool m_FireKeyDown;                 // Fire key down state (new input system).

        // Awake is called right at the beginning if the object is active.
        private void Awake()
        {
            m_Rigidbody =  GetComponent<Rigidbody>();
            m_TankShot = GetComponent<TankFiringSystem>();

            Health = MaxHealth;
        }

        // Start is called before the first frame update.
        void Start()
        {
            // Get the player action map corresponding to the player number.
            m_InputActionMap = GetComponent<PlayerInput>().actions.FindActionMap($"Player{PlayerNum + 1}");
            // Enable the action map.
            m_InputActionMap.Enable();

            // Get the 'MoveTurn' action.
            m_MoveTurnAction = m_InputActionMap.FindAction("MoveTurn");
            m_MoveTurnInputValue = Vector2.zero;

            // Get the 'Fire' action.
            m_FireAction = m_InputActionMap.FindAction("Fire");
            m_FireKeyDown = false;

            // Remember the initial audio pitch.
            m_OriginalPitch = MovementSFX.pitch;
        }

        private void OnDisable()
        {
            // Disable the action map.
            m_InputActionMap.Disable();
        }

        // Update is called once per frame.
        void Update()
        {
            switch (State)
            {
                case SMState.Idle:
                // Go to Moving state if Idle.
                case SMState.Moving:
                    {
                        // Check if input is enabled.
                        if (InputIsEnabled)
                        {
                            MovementInput();
                            FireInput();
                        }
                        break;
                    }
                case SMState.TakingDamage:
                    break;
                case SMState.Destroyed:
                    break;
                case SMState.Inactive:
                    break;
                default:
                    break;
            }
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
                if (m_TankShot.Fire())
                    PlaySFX(ClipShotFired);

                // Reset the fire key down state.
                m_FireKeyDown = false;
            }
        }

        protected void ChangeMovementAudio(AudioClip clip)
        {
            if (MovementSFX.clip != clip)
            {
                MovementSFX.clip = clip;
                MovementSFX.pitch = m_OriginalPitch + Random.Range(-PitchRange, PitchRange);
                MovementSFX.Play();
            }
        }

        protected void PlaySFX(AudioClip clip)
        {
            TankSFX.clip = clip;
            TankSFX.pitch = m_OriginalPitch + Random.Range(-PitchRange, PitchRange);
            TankSFX.Play();
        }

        // Physic update; update regardless of FPS.
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
            var rotQuat = Quaternion.Euler(0f, rotationDegree, 0f);
            // Rotate the tank.
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * rotQuat);
        }

        public void TakeDamage(float damage)
        {
            if (m_State != SMState.Inactive || m_State != SMState.Destroyed)
            {
                Health -= damage;
                if (Health > 0f)
                    State = SMState.TakingDamage;
                else
                    State = SMState.Destroyed;
            }
        }

        protected void Destroyed()
        {
            PlaySFX(ClipTankExplode);

            StartCoroutine(ChangeState(SMState.Inactive, 1f));
        }

        public void Restart(Vector3 pos, Quaternion rot)
        {
            // Reset position, rotation, and health.
            transform.position = pos;
            transform.rotation = rot;
            Health = MaxHealth;

            // Diable kinematic and activate the gameobject and input.
            m_Rigidbody.isKinematic = false;
            gameObject.SetActive(true);
            InputIsEnabled = true;

            // Change state.
            State = SMState.Idle;
        }

        private IEnumerator ChangeState(SMState state, float delay = 0f)
        {
            // Delay.
            yield return new WaitForSeconds(delay);

            // Change state.
            State = state;
        }

        public SMState State
        {
            get { return m_State; }
            set
            {
                if (m_State != value)
                {
                    switch (value)
                    {
                        case SMState.Idle:
                            {
                                ChangeMovementAudio(ClipIdle);
                                break;
                            }
                        case SMState.Moving:
                            {
                                ChangeMovementAudio(ClipMoving);
                                break;
                            }
                        case SMState.TakingDamage:
                            {
                                StartCoroutine(ChangeState(SMState.Idle, 1f));
                                break;
                            }
                        case SMState.Destroyed:
                            {
                                Destroyed();
                                break;
                            }
                        case SMState.Inactive:
                            {
                                gameObject.SetActive(false);
                                DestroyTank?.Invoke(obj: this);
                                m_Rigidbody.isKinematic = true;
                                InputIsEnabled = false;
                                break;
                            }
                        default:
                            break;
                    }

                    m_State = value;
                }
            }
        }

        // Is called every frame to draw the GUI for debugging.
        private void OnGUI()
        {
            var nl = string.Concat(System.Linq.Enumerable.Repeat("\n\n", PlayerNum));
            var str = $"{nl}<color='fuchsia'><size=35>{PlayerNum}: {State}</size></color>";
            GUILayout.Label(str);
        }
    }
}
