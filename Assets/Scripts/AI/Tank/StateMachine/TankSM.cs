using System.Linq;
using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>TankSM</c> state machine for the tank.
    /// </summary>
    internal class TankSM : StateMachine
    {
        protected internal struct States
        {
            // States:
            public IdleState Idle;
            public PatrollingState Patrolling;

            internal States(TankSM sm)
            {
                Idle = new IdleState(sm);
                Patrolling = new PatrollingState(sm);
            }
        }

        public States m_States;
        [HideInInspector] public GameManager GameManager;           // Reference to the GameManager.
        [HideInInspector] public NavMeshAgent NavMeshAgent;         // Reference to the NavMeshAgent.
        [Header("Patrolling")]
        [Tooltip("Minimum and maximum time delay for patrolling wait.")]
        public Vector2 PatrolWaitTime = new(1.5f, 3.5f);            // A minimum and maximum time delay for patrolling wait.
        [Tooltip("Minimum and maximum circumradius of the area to patrol at a given update time.")]
        public Vector2 PatrolMaxDist = new(15f, 30f);               // A minimum and maximum circumradius of the area to patrol.
        [Range(0f, 2f)] public float PatrolNavMeshUpdate = 0.2f;    // A delay between each parolling path update.
        [Header("Targeting")]
        [Tooltip("Minimum and maximum range for the targeting range.")]
        public Vector2 StartToTargetDist = new(28f, 35f);           // A minimum and maximum range for the targeting range.
        [HideInInspector] public float TargetDistance;              // The distance between the tank and the target.
        [Tooltip("Minimum and maximum range for the stopping range.")]
        public Vector2 StopAtTargetDist = new(18f, 22f);            // A minimum and maximum range for the stopping range.
        [HideInInspector] public float StopDistance;                // The distance between the tank and the target.
        [Range(0f, 2f)] public float TargetNavMeshUpdate = 0.2f;    // A delay between each targeting path update.
        [Header("Blending")]
        [Range(0f, 1f)] public float OrientSlerpScalar = 0.2f;      // A scalar for the slerp.
        // [Header("Target")]
        [HideInInspector] public Transform Target;                  // Reference to the target's transform.
        // [Header("NavMesh")]
        [HideInInspector] public float NavMeshUpdateDeadline;       // The time when the next path update is due.
        [Header("Firing")]
        [Tooltip("Minimum and maximum cooldown time delay between each firing in seconds.")]
        public Vector2 FireInterval = new(0.7f, 2.5f);              // A minimum and maximum cooldown time delay between each firing.
        [Tooltip("Force given to the shell if the fire button is not held, and the force given to the shell if the fire button is held for the max charge time in seconds.")]
        public Vector2 LaunchForceMinMax = new(6.5f, 30f);          // The force given to the shell if the fire button is not held, and the force given to the shell if the fire button is held for the max charge time.
        [Header("References")]
        [Tooltip("Prefab")] public Rigidbody Shell;                 // Prefab of the shell.
        [Tooltip("Transform")] public Transform FireTransform;      // A child of the tank where the shells are spawned.
        // public Slider AimSlider;                                 // A child of the tank that displays the current launch force.
        [Header("Firing Audio")]
        public AudioSource SFXAudioSource;                          // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
        // public AudioClip ShotChargingAudioClip;                  // Audio that plays when each shot is charging up.
        public AudioClip ShotFiringAudioClip;                       // Audio that plays when each shot is fired.

        private bool m_Started = false; // Whether the tank has started moving.
        private Rigidbody m_Rigidbody;  // Reference used to the tank's regidbody.
        private TankSound m_TankSound;  // Reference used to play sound effects.

        /// <summary>
        /// Method <c>MoveTurnSound</c> returns the current tank's velocity.
        /// </summary>
        private Vector2 MoveTurnSound() => new Vector2(Mathf.Abs(NavMeshAgent.velocity.x), Mathf.Abs(NavMeshAgent.velocity.z));

        /// <summary>
        /// Method <c>GetInitialState</c> returns the initial state of the state machine.
        /// </summary>
        protected override BaseState GetInitialState() => m_States.Idle;

        /// <summary>
        /// Method <c>SetNavMeshAgent</c> sets the NavMeshAgent's speed and angular speed.
        /// </summary>
        private void SetNavMeshAgent()
        {
            NavMeshAgent.speed = GameManager.Speed;
            NavMeshAgent.angularSpeed = GameManager.AngularSpeed;
        }

        /// <summary>
        /// Method <c>SetStopDistanceToZero</c> sets the NavMeshAgent's stopping distance to zero.
        /// </summary>
        public void SetStopDistanceToZero() => NavMeshAgent.stoppingDistance = 0f;

        /// <summary>
        /// Method <c>SetStopDistanceToTarget</c> sets the NavMeshAgent's stopping distance to the target's distance.
        /// </summary>
        public void SetStopDistanceToTarget() => NavMeshAgent.stoppingDistance = StopDistance;

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            m_States = new States(this);

            GameManager = GameManager.Instance;

            m_Rigidbody = GetComponent<Rigidbody>();
            NavMeshAgent = GetComponent<NavMeshAgent>();
            m_TankSound = GetComponent<TankSound>();

            SetNavMeshAgent();

            TargetDistance = Random.Range(StartToTargetDist.x, StartToTargetDist.y);
            StopDistance = Random.Range(StopAtTargetDist.x, StopAtTargetDist.y);

            SetStopDistanceToTarget();

            var tankManagers = GameManager.PlayerPlatoon.Tanks.Take(1);
            if (tankManagers.Count() != 0)
                Target = tankManagers.First().Instance.transform;
            else
                Debug.LogError("'Player Platoon' is empty!");
        }

        /// <summary>
        /// Method <c>OnEnable</c> is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            // When the tank is turned on, make sure it's not kinematic.
            m_Rigidbody.isKinematic = false;
        }

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private new void Start()
        {
            // base.Start(); // Moved to Update.

            m_TankSound.MoveTurnInputCalc += MoveTurnSound;
        }

        /// <summary>
        /// Method <c>OnDisable</c> is called when the behaviour becomes disabled or inactive.
        /// </summary>
        private void OnDisable()
        {
            // When the tank is turned off, set it to kinematic so it stops moving.
            m_Rigidbody.isKinematic = true;

            m_TankSound.MoveTurnInputCalc -= MoveTurnSound;
        }

        /// <summary>
        /// Method <c>Update</c> is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        private new void Update()
        {
            if (!m_Started && GameManager.IsRoundPlaying)
            {
                m_Started = true;
                base.Start();
            }
            else if (GameManager.IsRoundPlaying)
            {
                base.Update();
            }
            else
            {
                m_Started = false;
                StopAllCoroutines();
            }
        }

        /// <summary>
        /// Method <c>LaunchProjectile</c> instantiate and launch the shell.
        /// </summary>
        public void LaunchProjectile(float launchForce = 1f)
        {
            launchForce = Mathf.Min(Mathf.Max(LaunchForceMinMax.x, launchForce), LaunchForceMinMax.y);

            // Set the fired flag so only Fire is only called once.
            // m_Fired = true;

            // Create an instance of the shell and store a reference to it's rigidbody.
            Rigidbody shellInstance = Instantiate(Shell, FireTransform.position, FireTransform.rotation) as Rigidbody;

            // Set the shell's velocity to the launch force in the fire position's forward direction.
            shellInstance.velocity = launchForce * FireTransform.forward; ;

            // Change the clip to the firing clip and play it.
            SFXAudioSource.clip = ShotFiringAudioClip;
            SFXAudioSource.Play();
        }
    }
}
