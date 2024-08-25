/*
 * Shell.cs
 * 
 * This script controls the behavior of a shell in a Unity game. It manages the shell's state, handles collisions, and triggers explosions.
 * 
 * Features:
 * - State management for the shell (Moving, Explode).
 * - Collision detection to trigger state changes.
 * - Explosion mechanism affecting objects within a specified radius.
 * - Application of explosion force to affected objects.
 * - Calculation of damage based on distance from the explosion center.
 * 
 * Components:
 * - LayerMask explosionMask: Defines the layers affected by the explosion.
 * - float explosionRadius: The radius within which objects are affected by the explosion.
 * - float explosionForce: The force applied to objects within the explosion radius.
 * - float maxDamage: The maximum damage dealt by the explosion.
 * 
 * Methods:
 * - OnTriggerEnter(Collider other): Detects collisions and changes the shell's state to Explode.
 * - FixedUpdate(): Manages state transitions.
 * - Explosion(): Applies explosion force to objects within the explosion radius and destroys the shell.
 * - CalculateDamage(Vector3 targetPos): Calculates the damage based on the distance from the explosion center.
 * - CurrentShellState (Property): Gets or sets the current state of the shell and triggers state-specific actions.
 */

using UnityEngine;

namespace CE6127.Tanks.Advanced
{
    public class Shell : MonoBehaviour
    {
        public LayerMask explosionMask;         // The layer mask used to filter what the explosion affects, this should be set to "Players".
        public float explosionRadius = 5f;      // The maximum distance away from the explosion tanks can be and are still affected.
        public float explosionForce = 1000f;    // The amount of force added to a tank at the centre of the explosion.
        public float maxDamage = 100f;          // The amount of damage done if the explosion is centred on a tank.

        // CurrentShellState machine state
        public enum ShellState
        {
            Moving,
            Explode
        }
        protected ShellState currentState; // The current state of the shell.

        public ShellState CurrentShellState
        {
            get { return currentState; }
            set
            {
                if (currentState != value)
                {
                    switch (value)
                    {
                        case ShellState.Moving:
                            break;
                        case ShellState.Explode:
                            {
                                Explosion();
                                break;
                            }
                        default:
                            break;
                    }

                    currentState = value;
                }
            }
        }

        // Called when collide with other collider.
        protected void OnTriggerEnter(Collider other)
        {
            if (CurrentShellState == ShellState.Moving)
                CurrentShellState = ShellState.Explode;
        }

        private void FixedUpdate()
        {
            switch (currentState)
            {
                case ShellState.Moving:
                    break;
                case ShellState.Explode:
                    break;
                default:
                    break;
            }
        }

        public void Explosion()
        {
            // Get all the tanks caught in the explosion.
            Collider[] tankColliders = Physics.OverlapSphere(transform.position, explosionRadius, explosionMask);

            // Loop through the collider to apply force and damage.
            foreach (var collider in tankColliders)
            {
                // Apply physics to the tank.
                var rBody = collider.GetComponent<Rigidbody>();
                if (rBody == null)
                    continue;
                rBody.AddExplosionForce(explosionForce, transform.position, explosionRadius);

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
            float relativeDistance = (explosionRadius - distance) / explosionRadius;

            // Damage is proportional to the distance.
            float damage = relativeDistance * maxDamage;

            return Mathf.Max(0f, damage);
        }
    }
}
