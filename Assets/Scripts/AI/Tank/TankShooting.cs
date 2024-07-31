using UnityEngine;
using UnityEngine.UI;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>TankShooting</c> this class is used to control the shooting of the tank.
    /// </summary>
    public class TankShooting : MonoBehaviour
    {
        // Cannon fire state.
        private enum FireState
        {
            ReadyToFire,    // Ready to fire.
            OnCooldown      // On cooldown.
        }

        [Tooltip("Force given to the shell if the fire key is not held, and the force given to the shell if the fire key is held for the max charge time in seconds.")]
        public Vector2 LaunchForceMinMax = new(6.5f, 40f);                      // The force given to the shell if the fire button is not held, and the force given to the shell if the fire button is held for the max charge time.
        public float MaxChargeTime = 0.75f;                                     // How long the shell can charge for before it is fired at max force.
        [Tooltip("Time between each shot.")] public float CooldownTime = 0.35f; // The time between each shot.
        [Header("References")]
        [Tooltip("Prefab")] public Rigidbody Shell;                             // Prefab of the shell.
        [Tooltip("Transform")] public Transform FireTransform;                  // A child of the tank where the shells are spawned.
        public Slider AimSlider;                                                // A child of the tank that displays the current launch force.
        [Header("Audio")]
        public AudioSource SFXAudioSource;                                      // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
        public AudioClip ShotChargingAudioClip;                                 // Audio that plays when each shot is charging up.
        public AudioClip ShotFiringAudioClip;                                   // Audio that plays when each shot is fired.

        private float m_CurrentLaunchForce;                 // The force that will be given to the shell when the fire button is released.
        private float m_ChargeSpeed;                        // How fast the launch force increases, based on the max charge time.
        private bool m_Fired;                               // Whether or not the shell has been launched with this button press.
        private TankMovement m_TankMovement;                // Reference to tank's movement script, used to disable and enable control (new input system).
        private bool m_FireKeyDown = false;                 // Button downs state (new input system).
        private bool m_FireKeyUp = false;                   // Button up state (new input system).
        private float m_FireInputValue;                     // Button value (new input system).
        private FireState m_State = FireState.ReadyToFire;  // Current state of the fire button.
        private float m_CooldownCounter;                    // Counter for the cooldown.

        private FireState State
        {
            get { return m_State; }
            set
            {
                if (m_State != value)
                {
                    switch (m_State)
                    {
                        case FireState.ReadyToFire:
                            break;
                        case FireState.OnCooldown:
                            {
                                m_CooldownCounter = CooldownTime;
                                break;
                            }
                        default:
                            break;
                    }

                    m_State = value;
                }
            }
        }

        /// <summary>
        /// Method <c>Awake</c> set the cooldown counter to the cooldown time.
        /// </summary>
        private void Awake()
        {
            m_CooldownCounter = CooldownTime;
        }

        /// <summary>
        /// Method <c>OnEnable</c> set up defaults.
        /// </summary>
        private void OnEnable()
        {
            m_TankMovement = GetComponent<TankMovement>();
            m_FireKeyDown = false;
            m_FireKeyUp = false;
            m_FireInputValue = 0f;

            // When the tank is turned on, reset the launch force and the UI
            m_CurrentLaunchForce = LaunchForceMinMax.x;
            AimSlider.value = LaunchForceMinMax.x;
        }
   
        /// <summary>
        /// Method <c>Start</c> set up the events.
        /// </summary>
        private void Start()
        {
            // Subscribe to the button down and button up events.
            m_TankMovement.InputActionMap.FindAction("Fire").started += _ => m_FireKeyDown = true;
            m_TankMovement.InputActionMap.FindAction("Fire").canceled += _ => m_FireKeyUp = true;

            // The rate that the launch force charges up is the range of possible forces by the max charge time.
            m_ChargeSpeed = (LaunchForceMinMax.y - LaunchForceMinMax.x) / MaxChargeTime;
        }

        /// <summary>
        /// Method <c>Update</c> track the current state of the fire button and make decisions based on the current launch force.
        /// </summary>
        private void Update()
        {
            switch (State)
            {
                case FireState.ReadyToFire:
                    break;
                case FireState.OnCooldown:
                    {
                        m_CooldownCounter -= Time.deltaTime;
                        if (m_CooldownCounter <= 0)
                            State = FireState.ReadyToFire;
                        break;
                    }
                default:
                    break;
            }

            // The slider should have a default value of the minimum launch force.
            AimSlider.value = LaunchForceMinMax.x;

            // Store the value of the input.
            m_FireInputValue = m_TankMovement.InputActionMap.FindAction("Fire").ReadValue<float>();

            // If the max force has been exceeded, the shell hasn't yet been launched, and is not in cooldown state...
            if (m_CurrentLaunchForce >= LaunchForceMinMax.y && !m_Fired && State == FireState.ReadyToFire)
            {
                // ... use the max force and launch the shell.
                m_CurrentLaunchForce = LaunchForceMinMax.y;
                Firing();

                State = FireState.OnCooldown;
            }
            // Otherwise, if the fire button has just started being pressed...
            else if (m_FireKeyDown)
            {
                // ... reset the fired flag and reset the launch force.
                m_Fired = false;
                m_CurrentLaunchForce = LaunchForceMinMax.x;

                // Change the clip to the charging clip and start it playing.
                SFXAudioSource.clip = ShotChargingAudioClip;
                SFXAudioSource.Play();

                // Reset the button down flag.
                m_FireKeyDown = false;
            }
            // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
            else if (m_FireInputValue > 0f && !m_Fired)
            {
                // Increment the launch force and update the slider.
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                AimSlider.value = m_CurrentLaunchForce;
            }
            // Otherwise, if the fire button is released, the shell hasn't been launched yet, and 'OnCooldown' is not the current state...
            else if (m_FireKeyUp && !m_Fired && State == FireState.ReadyToFire)
            {
                // ... launch the shell.
                Firing();

                State = FireState.OnCooldown;

                // Reset the button up flag.
                m_FireKeyUp = false;
            }
        }

        /// <summary>
        /// Method <c>Fire</c> instantiate and launch the shell.
        /// </summary>
        private void Firing()
        {
            // Set the fired flag so only Fire is only called once.
            m_Fired = true;

            // Create an instance of the shell and store a reference to it's rigidbody.
            Rigidbody shellInstance = Instantiate(Shell, FireTransform.position, FireTransform.rotation) as Rigidbody;

            // Set the shell's velocity to the launch force in the fire position's forward direction.
            shellInstance.velocity = m_CurrentLaunchForce * FireTransform.forward; ;

            // Change the clip to the firing clip and play it.
            SFXAudioSource.clip = ShotFiringAudioClip;
            SFXAudioSource.Play();

            // Reset the launch force.  This is a precaution in case of missing button events.
            m_CurrentLaunchForce = LaunchForceMinMax.x;
        }

        /// <summary>
        /// Method <c>OnGUI</c> draw the current state of the fire button for debugging purpose.
        /// </summary>
        // private void OnGUI() => GUILayout.Label($"<color='lime'><size=35>({m_TankMovement.InputActionMap.name}) {State}</size></color>");
    }
}
