/// <remarks>
/// Detailed order of execution for all Unity event functions: 
/// https://docs.unity3d.com/Manual/ExecutionOrder.html
/// </remarks>

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class <c>TankMovementSimple</c> this class is used to control the tank movement.
/// </summary>
public class Tank03ScriptingTheTankMovement : MonoBehaviour
{
    public float speed = 12f;                               // How fast the tank moves forward and back.
    public float turnSpeed = 180f;                          // How fast the tank turns in degrees per second.
    [HideInInspector] public InputActionMap inputActionMap; // Reference to the player number's inputActionMap.

    private InputAction moveTurnAction; // Input action used to move the tank.
    private Rigidbody rbody;            // Rigidbody reference used to move the tank.
    private Vector2 moveTurnInputValue; // Move and turn compound input value.

    /// <summary>
    /// Method <c>Awake</c> is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();

        PlayerInput playerInput = GetComponent<PlayerInput>();
        inputActionMap = playerInput.actions.FindActionMap("Player1");
        // inputActionMap = playerInput.actions.actionMaps[0]; // Alternative way to get the action map.
    }

    /// <summary>
    /// Method <c>OnEnable</c> is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        rbody.isKinematic = false;

        inputActionMap.Enable();
        moveTurnAction = inputActionMap.FindAction("MoveTurn");
        moveTurnInputValue = Vector2.zero;
    }

    /// <summary>
    /// Method <c>OnDisable</c> is called when the behaviour becomes disabled or inactive.
    /// </summary>
    private void OnDisable()
    {
        rbody.isKinematic = true;

        inputActionMap.Disable();
    }

    /// <summary>
    /// Method <c>Update</c> is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        moveTurnInputValue = moveTurnAction.ReadValue<Vector2>();
    }

    /// <summary>
    /// Method <c>FixedUpdate</c> is called every fixed framerate frame.
    /// </summary>
    private void FixedUpdate()
    {
        Vector3 move = transform.forward * moveTurnInputValue.y * speed * Time.deltaTime;
        rbody.MovePosition(rbody.position + move);

        float turn = moveTurnInputValue.x * turnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rbody.MoveRotation(rbody.rotation * turnRotation);
    }
}
