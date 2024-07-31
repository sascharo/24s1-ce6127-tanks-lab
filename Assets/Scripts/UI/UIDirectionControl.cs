using UnityEngine;

/// <summary>
/// Class <c>UIDirectionControl</c> this class is used to make sure world space UI elements such as the health bar face the correct direction.
/// </summary>
public class UIDirectionControl : MonoBehaviour
{
    public bool m_UseRelativeRotation = true; // Use relative rotation should be used for this gameobject?

    private Quaternion m_RelativeRotation; // The local rotatation at the start of the scene.

    /// <summary>
    /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods is called the first time.
    /// </summary>
    private void Start()
    {
        m_RelativeRotation = transform.parent.localRotation;
    }

    /// <summary>
    /// Method <c>Update</c> is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        if (m_UseRelativeRotation)
            transform.rotation = m_RelativeRotation;
    }
}
