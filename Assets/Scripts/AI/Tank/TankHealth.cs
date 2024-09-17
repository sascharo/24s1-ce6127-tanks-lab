/*
 * -------------------------------------------------------------------------
 * File: TankHealth.cs
 * 
 * This script is responsible for managing the health system of a tank in the game. 
 * It defines the starting health of the tank, manages its current health, and 
 * updates the UI to reflect any changes in health.
 * 
 * Components:
 * - `HealthSlider`: A UI slider showing the current health.
 * - `HealthSliderFillImage`: The fill image of the slider that changes color.
 * - `TankExplosionPrefab`: A prefab used to show explosion effects when the tank is destroyed.
 * 
 * Additional Details:
 * - This class interacts with the Unity UI and audio system to provide visual and audio feedback 
 *   when the tank takes damage or is destroyed.
 * -------------------------------------------------------------------------
 */

using UnityEngine;
using UnityEngine.UI;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>TankHealth</c> this class is used to manage the health of the tank.
    /// </summary>
    public class TankHealth : MonoBehaviour
    {
        public float StartingHealth = 100f;         // The amount of health each tank starts with.
        public Slider HealthSlider;                 // The slider to represent how much health the tank currently has.
        public Image HealthSliderFillImage;         // The image component of the slider.
        public Color FullHealthColor = Color.green; // The color the health bar will be when on full health.
        public Color ZeroHealthColor = Color.red;   // The color the health bar will be when on no health.
        public GameObject TankExplosionPrefab;      // A prefab that will be instantiated in Awake, then used whenever the tank dies.

        private AudioSource m_ExplosionAudio;        // The audio source to play when the tank explodes.
        private ParticleSystem m_ExplosionParticles; // The particle system the will play when the tank is destroyed.
        private float m_CurrentHealth;               // How much health the tank currently has.
        private bool m_Dead;                         // Has the tank been reduced beyond zero health yet?

        // Property to get and set m_CurrentHealth.
        public float CurrentHealth
        {
            get { return m_CurrentHealth; }
            set
            {
                m_CurrentHealth = value;

                // Update the health slider's value and color.
                SetHealthUI();
            }
        }

        /// <summary>
        /// Method <c>Awake</c> instantiate the explosion prefab and get a reference to the particle system and audio source on it.
        /// </summary>
        private void Awake()
        {
            // Instantiate the explosion prefab and get a reference to the particle system on it.
            m_ExplosionParticles = Instantiate(TankExplosionPrefab).GetComponent<ParticleSystem>();
            // Get a reference to the audio source on the instantiated prefab.
            m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();
            // Disable the prefab so it can be activated when it's required.
            m_ExplosionParticles.gameObject.SetActive(false);
        }

        /// <summary>
        /// Method <c>OnEnable</c> when the tank is enabled, reset the tank's health and whether or not it's dead.
        /// </summary>
        private void OnEnable()
        {
            // When the tank is enabled, reset the tank's health and whether or not it's dead.
            CurrentHealth = StartingHealth;
            m_Dead = false;
        }

        /// <summary>
        /// Method <c>TakeDamage</c> adjust the tank's current health, update the UI based on the new health and check whether or not the tank is dead.
        /// </summary>
        public void TakeDamage(float amount)
        {
            // Reduce current health by the amount of damage done.
            CurrentHealth -= amount;

            // If the current health is at or below zero and it has not yet been registered, call OnDeath.
            if (CurrentHealth <= 0f && !m_Dead)
                OnTermination();
        }

        /// <summary>
        /// Method <c>SetHealthUI</c> adjust the value and colour of the slider.
        /// </summary>
        private void SetHealthUI()
        {
            // Set the slider's value appropriately.
            HealthSlider.value = m_CurrentHealth;

            // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
            HealthSliderFillImage.color = Color.Lerp(ZeroHealthColor, FullHealthColor, m_CurrentHealth / StartingHealth);
        }

        /// <summary>
        /// Method <c>OnTermination</c> play the effects for the death of the tank and deactivate it.
        /// </summary>
        private void OnTermination()
        {
            // Set the flag so that this function is only called once.
            m_Dead = true;

            // Move the instantiated explosion prefab to the tank's position and turn it on.
            m_ExplosionParticles.transform.position = transform.position;
            m_ExplosionParticles.gameObject.SetActive(true);
            // Play the particle system of the tank exploding.
            m_ExplosionParticles.Play();
            // Play the tank explosion sound effect.
            m_ExplosionAudio.Play();

            // Turn the tank off.
            gameObject.SetActive(false);
        }
    }
}
