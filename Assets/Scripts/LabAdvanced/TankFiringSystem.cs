using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.Advanced
{
    public class TankFiringSystem : MonoBehaviour
    {
        public float Cooldown = 0.5f;           // The time between each shot.
        public Rigidbody ShellPrefabRigidbody;  // The Prefab's Rigidbody of the shell.
        public float LaunchForce = 15f;         // The force given to the shell when firing.
        public Transform SpawnPoint;            // The position and direction of the shell when firing.

        // The state of the firing system.
        public enum FireState
        {
            ReadyToFire,
            OnCooldown
        }
        protected FireState m_State = FireState.ReadyToFire;    // The current state of the firing system.

        protected float m_CooldownCounter;                      // The counter for the cooldown time.

        private Tank m_Tank; // Reference to the tank that owns this firing system.

        private void Awake()
        {
            m_Tank = GetComponent<Tank>();
        }

        private void Start()
        {
            // Set the cooldown counter to the cooldown time.
            m_CooldownCounter = Cooldown;
        }

        // Update is called once per frame.
        void Update()
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
        }

        public Rigidbody Fire()
        {
            if (State == FireState.ReadyToFire)
            {
                // Change state.
                State = FireState.OnCooldown;

                // Spawn shell by creating an instance of the shell and store a reference to it's rigidbody.
                var shell = Instantiate(ShellPrefabRigidbody, SpawnPoint.position, SpawnPoint.rotation) as Rigidbody;

                // Set the shell's velocity to the launch force in the fire position's forward direction.
                shell.velocity = LaunchForce * SpawnPoint.forward;

                return shell;
            }

            return null;
        }

        public FireState State
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
                                m_CooldownCounter = Cooldown;
                                break;
                            }
                        default:
                            break;
                    }

                    m_State = value;
                }
            }
        }

        private void OnGUI()
        {
            var nl = string.Concat(System.Linq.Enumerable.Repeat("\n\n", m_Tank.PlayerNum));
            var str = $"{nl}\t\t\t\t\t<color='red'><size=35>{State}</size></color>";
            GUILayout.Label(str);
        }
    }
}
