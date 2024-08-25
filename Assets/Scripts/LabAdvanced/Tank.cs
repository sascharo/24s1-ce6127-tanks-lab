/*
 * Tank.cs
 * 
 * This script controls the behavior of a tank in a Unity game. It handles movement, rotation, firing, and state management using Unity's new input system.
 * 
 * Features:
 * - Movement and rotation control based on player input.
 * - State machine to manage the tank's idle, moving, taking damage, destroyed, and inactive states.
 * - Integration with a firing system to allow the tank to fire projectiles.
 * - Audio management for engine sounds and firing sounds.
 * - Health management and damage handling.
 * 
 * Components:
 * - float moveSpeed: Speed at which the tank moves forward and backward.
 * - float rotationSpeed: Speed at which the tank rotates.
 * - AudioSource sourceEngineIdleSFX: Audio source for engine idle sound.
 * - AudioSource sourceShotFiringSFX: Audio source for shot firing sound.
 * - AudioClip clipIdle: Audio clip for idle state.
 * - AudioClip clipMoving: Audio clip for moving state.
 * - AudioClip clipShotFired: Audio clip for firing state.
 * - AudioClip clipTankExplode: Audio clip for explosion.
 * - float pitchRange: Range for varying the pitch of engine sounds.
 * - bool inputIsEnabled: Flag to enable or disable input.
 * - int playerNum: Player number for identifying the tank.
 * - float maxHealth: Maximum health of the tank.
 * - float health: Current health of the tank.
 * - Action<Tank> DestroyTank: Delegate called when the tank is destroyed.
 * - TankState currentState: Current state of the tank.
 * - float originalPitch: Original pitch of the engine sound.
 * - Rigidbody RBody: Reference to the tank's Rigidbody component.
 * - TankFiringSystem TankFiring: Reference to the tank's firing system.
 * - InputActionMap inputActionMap: Input action map for player controls.
 * - InputAction moveTurnAction: Input action for movement and turning.
 * - Vector2 moveTurnInputValue: Input value for movement and turning.
 * - InputAction fireAction: Input action for firing.
 * - bool fireKeyDown: Flag to check if the fire key is pressed.
 * 
 * Methods:
 * - Awake(): Initializes the tank's health.
 * - Start(): Sets up input actions and audio pitch.
 * - OnDisable(): Disables the input action map.
 * - Update(): Handles state transitions and input.
 * - MovementInput(): Processes movement input.
 * - FireInput(): Processes firing input.
 * - ChangeMovementAudio(AudioClip clip): Changes the movement audio clip.
 * - PlaySFX(AudioClip clip): Plays a sound effect.
 * - FixedUpdate(): Handles physics updates.
 * - Move(): Moves the tank.
 * - Rotate(): Rotates the tank.
 * - TakeDamage(float damage): Applies damage to the tank.
 * - Destroyed(): Handles the tank's destruction.
 * - Restart(Vector3 pos, Quaternion rot): Restarts the tank at a given position and rotation.
 * - ChangeState(TankState state, float delay): Changes the tank's state after a delay.
 * - OnGUI(): Draws the GUI for debugging.
 */

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

using Random = UnityEngine.Random;

namespace CE6127.Tanks.Advanced
{
    public class Tank : MonoBehaviour
    {
        public float moveSpeed = 20f;           // How fast the tank moves forward and back.
        public float rotationSpeed = 90f;       // How fast the tank turns in degrees per second.
        [Header("Audio")]
        [Tooltip("`AudioSource` Engine Idle")]
        public AudioSource sourceEngineIdleSFX; // Reference to the audio source used to play the engine sound.
        [Tooltip("`AudioSource` Shot Firing")]
        public AudioSource sourceShotFiringSFX; // Reference to the audio source used to play the shooting sound.
        public AudioClip clipIdle;              // Audio clip for when the tank isn't moving.
        public AudioClip clipMoving;            // Audio clip for when the tank is moving.
        public AudioClip clipShotFired;         // Audio clip for when the tank is shooting.
        public AudioClip clipTankExplode;       // Audio clip for when the tank explodes.
        public float pitchRange = 0.2f;         // The amount by which the pitch of the engine noises can vary.
        [Header("Input")]
        public bool inputIsEnabled = true;      // Whether or not input is enabled.
        [HideInInspector] public int playerNum; // This specifies which player this the manager for.
        [Header("Health")]
        public float maxHealth = 100f;          // The maximum health of the tank.
        public float health = 100f;             // The current health of the tank.

        // Delegates.
        public Action<Tank> DestroyTank; // Delegate called when the tank is destroyed.

        // Available state machine states.
        public enum TankState
        {
            Idle = 0,     // The tank is idle. Default state.
            Moving,       // The tank is moving.
            TakingDamage, // The tank is taking damage.
            Destroyed,    // The tank is destroyed.
            Inactive      // The tank is inactive.
        };
        protected TankState currentState; // The current state of the tank.

        private IEnumerator ChangeState(TankState state, float delay = 0f)
        {
            // Delay.
            yield return new WaitForSeconds(delay);

            // Change state.
            CurrentTankState = state;
        }

        public TankState CurrentTankState
        {
            get { return currentState; }
            set
            {
                if (currentState != value)
                {
                    switch (value)
                    {
                        case TankState.Idle:
                            {
                                ChangeMovementAudio(clipIdle);
                                break;
                            }
                        case TankState.Moving:
                            {
                                ChangeMovementAudio(clipMoving);
                                break;
                            }
                        case TankState.TakingDamage:
                            {
                                StartCoroutine(ChangeState(TankState.Idle, 1f));
                                break;
                            }
                        case TankState.Destroyed:
                            {
                                Destroyed();
                                break;
                            }
                        case TankState.Inactive:
                            {
                                gameObject.SetActive(false);
                                DestroyTank?.Invoke(obj: this);
                                RBody.isKinematic = true;
                                inputIsEnabled = false;
                                break;
                            }
                        default:
                            break;
                    }

                    currentState = value;
                }
            }
        }

        // Audio.
        protected float originalPitch;          // The pitch of the audio source at the start of the scene.

        private Rigidbody RBody { get { return GetComponent<Rigidbody>(); } } // Reference used to move the tank.
        private TankFiringSystem TankFiring { get { return GetComponent<TankFiringSystem>(); } } // Reference to the tank's shooting script, used to disable and enable the shell.
        
        private InputActionMap inputActionMap;  // Reference to the player number's InputActionMap.
        private InputAction moveTurnAction;     // Input action used to move the tank.
        private Vector2 moveTurnInputValue;     // Move and turn compound input value (new input system).
        private InputAction fireAction;         // Input action used to fire a shell from the tank.
        private bool fireKeyDown;               // Fire key down state (new input system).

        // Awake is called right at the beginning if the object is active.
        private void Awake()
        {
            health = maxHealth;
        }

        // Start is called before the first frame update.
        void Start()
        {
            // Get the player action map corresponding to the player number.
            inputActionMap = GetComponent<PlayerInput>().actions.FindActionMap($"Player{playerNum + 1}");
            // Enable the action map.
            inputActionMap.Enable();

            // Get the 'MoveTurn' action.
            moveTurnAction = inputActionMap.FindAction("MoveTurn");
            moveTurnInputValue = Vector2.zero;

            // Get the 'Fire' action.
            fireAction = inputActionMap.FindAction("Fire");
            fireKeyDown = false;

            // Remember the initial audio pitch.
            originalPitch = sourceEngineIdleSFX.pitch;
        }

        private void OnDisable()
        {
            // Disable the action map.
            inputActionMap.Disable();
        }

        // Update is called once per frame.
        void Update()
        {
            switch (CurrentTankState)
            {
                case TankState.Idle:
                // Go to Moving state if Idle.
                case TankState.Moving:
                    {
                        // Check if input is enabled.
                        if (inputIsEnabled)
                        {
                            MovementInput();
                            FireInput();
                        }
                        break;
                    }
                case TankState.TakingDamage:
                    break;
                case TankState.Destroyed:
                    break;
                case TankState.Inactive:
                    break;
                default:
                    break;
            }
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
                if (TankFiring.Fire())
                    PlaySFX(clipShotFired);

                // Reset the fire key down state.
                fireKeyDown = false;
            }
        }

        protected void ChangeMovementAudio(AudioClip clip)
        {
            if (sourceEngineIdleSFX.clip != clip)
            {
                sourceEngineIdleSFX.clip = clip;
                sourceEngineIdleSFX.pitch = originalPitch + Random.Range(-pitchRange, pitchRange);
                sourceEngineIdleSFX.Play();
            }
        }

        protected void PlaySFX(AudioClip clip)
        {
            sourceShotFiringSFX.clip = clip;
            sourceShotFiringSFX.pitch = originalPitch + Random.Range(-pitchRange, pitchRange);
            sourceShotFiringSFX.Play();
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
            Vector3 moveVect = transform.forward * moveSpeed * Time.deltaTime * moveTurnInputValue.y;
            // Move the tank into position.
            RBody.MovePosition(RBody.position + moveVect);
        }

        // Rotate the tank.
        public void Rotate()
        {
            // Calculate the rotation on dgrees.
            float rotationDegree = rotationSpeed * Time.deltaTime * moveTurnInputValue.x;
            // Convert the Euler rotation to a quaternion.
            var rotQuat = Quaternion.Euler(0f, rotationDegree, 0f);
            // Rotate the tank.
            RBody.MoveRotation(RBody.rotation * rotQuat);
        }

        public void TakeDamage(float damage)
        {
            if (currentState != TankState.Inactive || currentState != TankState.Destroyed)
            {
                health -= damage;
                if (health > 0f)
                    CurrentTankState = TankState.TakingDamage;
                else
                    CurrentTankState = TankState.Destroyed;
            }
        }

        protected void Destroyed()
        {
            PlaySFX(clipTankExplode);

            StartCoroutine(ChangeState(TankState.Inactive, 1f));
        }

        public void Restart(Vector3 pos, Quaternion rot)
        {
            // Reset position, rotation, and health.
            transform.position = pos;
            transform.rotation = rot;
            health = maxHealth;

            // Diable kinematic and activate the gameobject and input.
            RBody.isKinematic = false;
            gameObject.SetActive(true);
            inputIsEnabled = true;

            // Change state.
            CurrentTankState = TankState.Idle;
        }

        // Is called every frame to draw the GUI for debugging.
        private void OnGUI()
        {
            var nl = string.Concat(System.Linq.Enumerable.Repeat("\n\n", playerNum));
            var str = $"{nl}<color='fuchsia'><size=35>{playerNum}: {CurrentTankState}</size></color>";
            GUILayout.Label(str);
        }
    }
}
