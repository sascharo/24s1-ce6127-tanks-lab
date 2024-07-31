using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>StateMachine</c> represents the state machine.
    /// </summary>
    public class StateMachine : MonoBehaviour
    {
        private BaseState m_CurrentState; // Current state.

        /// <summary>
        /// Method <c>Start</c> initialization.
        /// </summary>
        protected virtual void Start()
        {
            m_CurrentState = GetInitialState();
            m_CurrentState?.Enter();
        }

        /// <summary>
        /// Method <c>Update</c> update logic.
        /// </summary>
        protected virtual void Update()
        {
            m_CurrentState?.Update();
        }

        /// <summary>
        /// Method <c>LateUpdate</c> update physics.
        /// </summary>
        protected virtual void LateUpdate()
        {
            m_CurrentState?.LateUpdate();
        }

        /// <summary>
        /// Method <c>OnGUI</c> is called every frame to draw the GUI for debugging.
        /// </summary>
        //private void OnGUI()
        //{
        //    string content = m_CurrentState != null ? m_CurrentState.ToString() : "No current sate";
        //    GUILayout.Label($"<color='fuchsia'><size=35>{content}</size></color>");
        //}

        /// <summary>
        /// Method <c>BaseState</c> get initial state.
        /// </summary>
        protected virtual BaseState GetInitialState()
        {
            return null;
        }

        /// <summary>
        /// Method <c>ChangeState</c> change state.
        /// </summary>
        public void ChangeState(BaseState newState)
        {
            m_CurrentState?.Exit();

            m_CurrentState = newState;
            m_CurrentState.Enter();
        }
    }
}
