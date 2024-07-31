using UnityEngine;
using UnityEngine.UI;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.MultiPlayer
{
    /// <summary>
    /// Class <c>TankShooting</c> this class is used to control the shooting of the tank.
    /// </summary>
    public class TankShooting : MonoBehaviour
    {
        private enum FireState
        {
            ReadyToFire,
            OnCooldown
        }

        public Rigidbody m_Shell;               // The Prefab's Rigidbody of the shell.
        public Transform m_FireTransform;       // A child of the tank where the shells are spawned.
        public Slider m_AimSlider;              // A child of the tank that displays the current launch force.
        public AudioSource m_ShootingAudio;     // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
        public AudioClip m_ChargingClip;        // Audio that plays when each shot is charging up.
        public AudioClip m_FireClip;            // Audio that plays when each shot is fired.
        public float m_MinLaunchForce = 15f;    // The force given to the shell if the fire button is not held.
        public float m_MaxLaunchForce = 30f;    // The force given to the shell if the fire button is held for the max charge time.
        public float m_MaxChargeTime = 0.75f;   // How long the shell can charge for before it is fired at max force.
        [Tooltip("Time between each shot.")] public float CooldownTime = 0.05f; // The time between each shot.

        private float m_CurrentLaunchForce;                 // The force that will be given to the shell when the fire button is released.
        private float m_ChargeSpeed;                        // How fast the launch force increases, based on the max charge time.
        private bool m_Fired;                               // Whether or not the shell has been launched with this button press.
        private TankMovement m_TankMovement;                // Reference to tank's movement script, used to disable and enable control.
        private bool m_FireKeyDown;                         // Fire button down state.
        private bool m_FireKeyUp;                           // Fire button up state.
        private float m_FireInputValue;                     // Fire button value.
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
        /// Method <c>OnEnable</c> set the tank movement reference.
        /// </summary>
        private void OnEnable()
        {
            m_TankMovement = GetComponent<TankMovement>();
            m_FireKeyDown = false;
            m_FireKeyUp = false;
            m_FireInputValue = 0f;

            // When the tank is turned on, reset the launch force and the UI
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_AimSlider.value = m_MinLaunchForce;
        }
   
        /// <summary>
        /// Method <c>Start</c> register key events and set the charge speed.
        /// </summary>
        private void Start()
        {
            // Subscribe to the button down and button up events.
            m_TankMovement.m_InputActionMap.FindAction("Fire").started += _ => m_FireKeyDown = true;
            m_TankMovement.m_InputActionMap.FindAction("Fire").canceled += _ => m_FireKeyUp = true;

            // The rate that the launch force charges up is the range of possible forces by the max charge time.
            m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
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
            m_AimSlider.value = m_MinLaunchForce;

            // Store the value of the input.
            m_FireInputValue = m_TankMovement.m_InputActionMap.FindAction("Fire").ReadValue<float>();

            // If the max force has been exceeded, the shell hasn't yet been launched, and is not in cooldown state...
            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired && State == FireState.ReadyToFire)
            {
                // ... use the max force and launch the shell.
                m_CurrentLaunchForce = m_MaxLaunchForce;
                Fire();

                // Set the State back to cooldown.
                State = FireState.OnCooldown;
            }
            // Otherwise, if the fire button has just started being pressed...
            else if (m_FireKeyDown)
            {
                // ... reset the fired flag and reset the launch force.
                m_Fired = false;
                m_CurrentLaunchForce = m_MinLaunchForce;

                // Change the clip to the charging clip and start it playing.
                m_ShootingAudio.clip = m_ChargingClip;
                m_ShootingAudio.Play();

                // Reset the button down flag.
                m_FireKeyDown = false;
            }
            // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
            else if (m_FireInputValue > 0f && !m_Fired)
            {
                // Increment the launch force and update the slider.
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                m_AimSlider.value = m_CurrentLaunchForce;
            }
            // Otherwise, if the fire button is released, the shell hasn't been launched yet, and 'OnCooldown' is not the current state...
            else if (m_FireKeyUp && !m_Fired && State == FireState.ReadyToFire)
            {
                // ... launch the shell.
                Fire();

                // Set the State back to cooldown.
                State = FireState.OnCooldown;

                // Reset the button up flag.
                m_FireKeyUp = false;
            }
        }

        /// <summary>
        /// Method <c>Fire</c> instantiate and launch the shell.
        /// </summary>
        private void Fire()
        {
            // Set the fired flag so only Fire is only called once.
            m_Fired = true;

            // Create an instance of the shell and store a reference to it's rigidbody.
            Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

            // Set the shell's velocity to the launch force in the fire position's forward direction.
            shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward; ;

            // Change the clip to the firing clip and play it.
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play();

            // Reset the launch force.  This is a precaution in case of missing button events.
            m_CurrentLaunchForce = m_MinLaunchForce;
        }

        /// <summary>
        /// Method <c>OnGUI</c> draw the current state of the fire button for debugging purposes.
        /// </summary>
        private void OnGUI()
        {
            var num = m_TankMovement.m_InputActionMap.name[m_TankMovement.m_InputActionMap.name.Length - 1] - '0';
            var nl = string.Concat(System.Linq.Enumerable.Repeat("\n\n", num - 1));
            var str = $"{nl}<color='red'><size=35>{num}: {State}</size></color>";
            GUILayout.Label(str);
        }
    }
}
