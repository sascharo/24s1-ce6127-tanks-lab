/// <remarks>
/// Detailed order of execution for all Unity event functions: 
/// https://docs.unity3d.com/Manual/ExecutionOrder.html
/// </remarks>

using UnityEngine;

public class Tank02AddingPublicVariables : MonoBehaviour
{
    public float Speed = 12f;
    public float m_TurnSpeed = 180f;

    // Awake is called right at the beginning if the object is active.
    private void Awake()
    {
        Debug.Log("Awake");
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Update - delta time: " + Time.deltaTime);
    }

    // Update is called at fixed intervals
    void FixedUpdate()
    {
        Debug.Log("FixedUpdate - delta time: " + Time.deltaTime);
    }
}
