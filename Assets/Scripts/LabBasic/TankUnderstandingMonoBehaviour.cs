/// <remarks>
/// Detailed order of execution for all Unity event functions: 
/// https://docs.unity3d.com/Manual/ExecutionOrder.html
/// </remarks>

using UnityEngine;

public class TankUnderstandingMonoBehaviour : MonoBehaviour
{
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
        Debug.Log("Update");
    }
}
