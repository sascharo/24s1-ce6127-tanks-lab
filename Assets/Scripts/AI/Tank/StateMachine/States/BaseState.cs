/// <remarks>
/// <para>
/// Reflection should be used cautiously due to its performance overhead and the loss of compile-time type safety.
/// It is slower compared to direct access via properties, methods, or fields since it involves runtime type inspection.
/// Moreover, reflection can lead to less maintainable and harder-to-debug code, as it bypasses standard access
/// mechanisms and encapsulation principles.
/// </para>
/// <para>
/// * Performance: Reflection is considerably slower than direct field access, impacting application performance.
/// * Encapsulation: It bypasses access modifiers, potentially breaking encapsulation and leading to unintended consequences.
/// * Maintainability: Code using reflection can be less readable and harder to maintain, especially for developers unfamiliar with it.
/// * Type safety: Reflection bypasses compile-time type checks, increasing the risk of runtime errors.
/// </para>
/// <para>
/// Instead of reflection, it is recommended to use classical getters and setters or public properties to access
/// and manipulate field values. These provide better performance, type safety, and allow for encapsulation.
/// Reflection may be useful in scenarios where dynamic type access is required, such as in frameworks or libraries,
/// but should be avoided in general application logic.
/// </para>
/// </remarks>

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
