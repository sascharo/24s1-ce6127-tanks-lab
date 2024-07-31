using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>ShellExplosionAI</c> handles the explosion of the shell.
    /// </summary>
    public class ShellExplosion : MonoBehaviour
    {
        public LayerMask PlayerTankMask;        // Used to filter what the explosion affects, this should be set to "Players".
        public LayerMask AITankMask;            // Used to filter what the explosion affects, this should be set to "AI".
        [Tooltip("Reference to the particles that will play on explosion.")] public ParticleSystem ShellParticleExplosion;      // Reference to the particles that will play on explosion.
        [Tooltip("Reference to the audio from the Prefab that will play on explosion.")] public AudioSource ShellAudioSource;   // Reference to the audio from the Prefab that will play on explosion.
        public float MaxDamage = 12.5f;         // The amount of damage done if the explosion is centred on a tank.
        public float ExplosionForce = 999f;     // The amount of force added to a tank at the centre of the explosion.
        public float MaxLifeTime = 2f;          // The time in seconds before the shell is removed.
        public float ExplosionRadius = 4f;      // The maximum distance away from the explosion tanks can be and are still affected.

        private GameManager m_GameManager; // Reference to the GameManager script.

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            m_GameManager = GameManager.Instance;
        }

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods is called the first time.
        /// </summary>
        private void Start()
        {
            // If it isn't destroyed by then, destroy the shell after it's lifetime.
            Destroy(gameObject, MaxLifeTime);
        }

        /// <summary>
        /// Method <c>OnTriggerEnter</c> find all the tanks in an area around the shell and damage them.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            // Collect all the colliders in a sphere from the shell's current position to a radius of the explosion radius.
            Collider[] collidersPlayers = Physics.OverlapSphere(transform.position, ExplosionRadius, PlayerTankMask);
            Collider[] collidersAI = Physics.OverlapSphere(transform.position, ExplosionRadius, AITankMask);
            Collider[] colliders = new Collider[collidersPlayers.Length + collidersAI.Length];
            collidersPlayers.CopyTo(colliders, 0);
            collidersAI.CopyTo(colliders, collidersPlayers.Length);

            // Go through all the colliders...
            for (var i = 0; i < colliders.Length; ++i)
            {
                // ... and find their rigidbody.
                Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

                // If they don't have a rigidbody, go on to the next collider.
                if (!targetRigidbody)
                    continue; // Don't execute the rest of the code in this iteration of the loop, but continue with the next iteration.

                // Add an explosion force.
                targetRigidbody.AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius);

                // Find the TankHealth script associated with the rigidbody.
                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

                // If there is no TankHealth script attached to the gameobject, go on to the next collider.
                if (!targetHealth)
                    continue; // Don't execute the rest of the code in this iteration of the loop, but continue with the next iteration.

                // Calculate the amount of damage the target should take based on it's distance from the shell.
                float damage = CalculateDamage(targetRigidbody.position);

                // Deal this damage to the tank.
                targetHealth.TakeDamage(damage);
            }

            // Unparent the particles from the shell.
            ShellParticleExplosion.transform.parent = null;

            // Play the particle system.
            ShellParticleExplosion.Play();

            // Play the explosion sound effect.
            if (m_GameManager.PlayShellSFX)
                ShellAudioSource.Play();

            // Once the particles have finished, destroy the gameobject they are on (ParticleSystem.duration is obsolete, use main.duration instead).
            Destroy(ShellParticleExplosion.gameObject, ShellParticleExplosion.main.duration);

            // Destroy the shell.
            Destroy(gameObject);
        }

        /// <summary>
        /// Method <c>CalculateDamage</c> calculate the amount of damage a target should take based on it's position.
        /// </summary>
        private float CalculateDamage(Vector3 targetPosition)
        {
            // Create a vector from the shell to the target.
            Vector3 explosionToTarget = targetPosition - transform.position;

            // Calculate the distance from the shell to the target.
            float explosionDistance = explosionToTarget.magnitude;

            // Calculate the proportion of the maximum distance (the explosionRadius) the target is away.
            float relativeDistance = (ExplosionRadius - explosionDistance) / ExplosionRadius;

            // Calculate damage as this proportion of the maximum possible damage.
            float damage = relativeDistance * MaxDamage;

            // Make sure that the minimum damage is always 0.
            damage = Mathf.Max(0f, damage);

            // Return the amount of damage the target should take.
            return damage;
        }
    }
}
