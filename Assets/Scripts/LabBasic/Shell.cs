using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.Basic
{
    public class Shell : MonoBehaviour
    {
        public LayerMask ExplosionMask;         // The layers that will be affected by the explosion.
        public float ExplosionRadius = 5f;      // The radius of the explosion.
        public float ExplosionForce = 1000f;    // The force of the explosion.
        public float MaxDamage = 100f;          // The maximum damage of the explosion.

        // The state of the shell.
        public enum SMState
        {
            Moving,
            Explode
        }
        protected SMState m_State; // The current state of the shell.

        // Called when collide with other collider.
        protected void OnTriggerEnter(Collider other)
        {
            if (State == SMState.Moving)
                State = SMState.Explode;
        }

        private void FixedUpdate()
        {
            switch (m_State)
            {
                case SMState.Moving:
                    break;
                case SMState.Explode:
                    break;
                default:
                    break;
            }
        }

        public void Explosion()
        {
            // Get all the tanks caught in the explosion.
            Collider[] tankColliders = Physics.OverlapSphere(transform.position, ExplosionRadius, ExplosionMask);

            // Loop through the collider to apply force and damage.
            foreach (var collider in tankColliders)
            {
                // Apply physics to the tank.
                var rBody = collider.GetComponent<Rigidbody>();
                if (rBody == null)
                    continue;
                rBody.AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius);
            }

            // Destroy the shell.
            Destroy(gameObject);
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
                        case SMState.Moving:
                            break;
                        case SMState.Explode:
                            {
                                Explosion();
                                break;
                            }
                        default:
                            break;
                    }

                    m_State = value;
                }
            }
        }
    }
}
