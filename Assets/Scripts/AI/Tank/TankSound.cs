using System;
using UnityEngine;

using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>TankMovementAI</c> this class is used to control the movement of the tank.
    /// </summary>
    public class TankSound : MonoBehaviour
    {
        public AudioSource DrivingAudioSource;      // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
        public AudioClip EngineIdlingAudioClip;     // Audio to play when the tank isn't moving.
        public AudioClip EngineDrivingAudioClip;    // Audio to play when the tank is moving.
        public float PitchRange = 0.2f;             // The amount by which the pitch of the engine noises can vary.
        public Func<Vector2> MoveTurnInputCalc;     // The function to calculate the move and turn input.

        private Vector2 m_MoveTurnInput;            // The current value of the movement.
        private float m_OriginalPitch;              // The pitch of the audio source at the start of the scene.

#nullable enable
        /// <summary>
        /// Method <c>MoveTurnInputEvalue</c> evaluates the move and turn input.
        /// </summary>
        public void MoveTurnInputEvalue()
        {
            var value = MoveTurnInputCalc?.Invoke();
            if (value != null)
                m_MoveTurnInput = (Vector2)value;
        }
#nullable disable

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods is called the first time.
        /// </summary>
        private void Start()
        {
            // Store the original pitch of the audio source.
            m_OriginalPitch = DrivingAudioSource.pitch;
        }

        /// <summary>
        /// Method <c>Update</c> is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void Update()
        {
            MoveTurnInputEvalue();
            Play();
        }

        /// <summary>
        /// Method <c>SoundFX</c> play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.
        /// </summary>
        private void Play()
        {
            // If there is no input (the tank is stationary)...
            if (Mathf.Abs(m_MoveTurnInput.y) < 0.1f && Mathf.Abs(m_MoveTurnInput.x) < 0.1f)
            {
                // ... and if the audio source is currently playing the driving clip...
                if (DrivingAudioSource.clip == EngineDrivingAudioClip)
                {
                    // ... change the clip to idling and play it.
                    DrivingAudioSource.clip = EngineIdlingAudioClip;
                    DrivingAudioSource.pitch = Random.Range(m_OriginalPitch - PitchRange, m_OriginalPitch + PitchRange);
                    DrivingAudioSource.Play();
                }
            }
            else
            {
                // Otherwise if the tank is moving and if the idling clip is currently playing...
                if (DrivingAudioSource.clip == EngineIdlingAudioClip)
                {
                    // ... change the clip to driving and play.
                    DrivingAudioSource.clip = EngineDrivingAudioClip;
                    DrivingAudioSource.pitch = Random.Range(m_OriginalPitch - PitchRange, m_OriginalPitch + PitchRange);
                    DrivingAudioSource.Play();
                }
            }
        }
    }
}
