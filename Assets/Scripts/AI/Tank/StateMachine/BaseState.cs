using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>BaseState</c> represents the base state.
    /// </summary>
    public class BaseState
    {
        public string Name; // Name of the state.

        protected StateMachine m_StateMachine; // Reference to the state machine.

        /// <summary>
        /// Constructor <c>BaseState</c> constructor.
        /// </summary>
        public BaseState(string name, StateMachine stateMachine)
        {
            Name = name;
            m_StateMachine = stateMachine;
        }

        /// <summary>
        /// Method <c>Enter</c> on enter.
        /// </summary>
        public virtual void Enter() { }

        /// <summary>
        /// Method <c>Update</c> update logic.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Method <c>LateUpdate</c> update physics.
        /// </summary>
        public virtual void LateUpdate() { }

        /// <summary>
        /// Method <c>Exit</c> on exit.
        /// </summary>
        public virtual void Exit() { }
    }
}
