using UnityEngine;
using UnityEngine.Events;

public class IdleTimer : MonoBehaviour
{
    public float idleDuration = 60f; // Time in seconds before the idle action is triggered
    public UnityEvent onIdle; // Event to trigger on idle
    private float idleTimer; // Tracks time since last activity

    void Start()
    {
        idleTimer = 0f; // Initialize timer
        if (onIdle == null)
        {
            onIdle = new UnityEvent();
        }
    }

    void Update()
    {
        // Check for any input to reset the timer
        if (Input.anyKey || Input.mousePosition != Vector3.zero)
        {
            idleTimer = 0f; // Reset idle timer on activity
        }
        else
        {
            idleTimer += Time.deltaTime; // Increment idle timer
        }

        // Check if idle duration has been exceeded
        if (idleTimer >= idleDuration)
        {
            TriggerIdleAction();
            idleTimer = 0f; // Reset the timer after triggering idle action
        }
    }

    private void TriggerIdleAction()
    {
        Debug.Log("Player has been idle for " + idleDuration + " seconds.");
        onIdle.Invoke(); // Trigger the assigned idle action/event
    }
}
