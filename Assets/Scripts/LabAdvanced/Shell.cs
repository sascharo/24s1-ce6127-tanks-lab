using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.Advanced
{
    public class Shell : MonoBehaviour
    {
        public LayerMask ExplosionMask;         // The layer mask used to filter what the explosion affects, this should be set to "Players".
        public float ExplosionRadius = 5f;      // The maximum distance away from the explosion tanks can be and are still affected.
        public float ExplosionForce = 1000f;    // The amount of force added to a tank at the centre of the explosion.
        public float MaxDamage = 100f;          // The amount of damage done if the explosion is centred on a tank.

        // State machine state
        public enum SMState
        {
            Moving,
            Explode
        }
        protected SMState m_State;              // The current state of the shell.

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

                // Apply damage to tank.
                var tank = collider.GetComponent<Tank>();
                tank.TakeDamage(CalculateDamage(tank.transform.position));
            }

            // Destroy the shell.
            Destroy(gameObject);
        }

        public float CalculateDamage(Vector3 targetPos)
        {
            // Create a vector from the shell to the target.
            Vector3 explosionToTarget = targetPos - transform.position;

            // Get the distance between shell and the target.
            float distance = explosionToTarget.magnitude;

            // Calculate the proportion of the maximum distance (the explosionRadius) the target is away.
            float relativeDistance = (ExplosionRadius - distance) / ExplosionRadius;

            // Damage is proportional to the distance.
            float damage = relativeDistance * MaxDamage;

            return Mathf.Max(0f, damage);
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
