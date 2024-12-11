using UnityEngine;
using UnityEngine.Events;

/**
Tracks user inactivity and triggers an event after a specified duration of idleness. 
It resets the idle timer whenever input is detected (keyboard, mouse, or touch) 
and invokes a customizable Unity event (onIdle) if the idle duration is exceeded. 
This is used to trigger actions like displaying a screensaver, prompting a message, or initiating specific gameplay behaviors after inactivity. 
**/
public class IdleTimer : MonoBehaviour
{
    public float idleDuration = 60f;
    public UnityEvent onIdle;
    private float idleTimer;
    private bool isIdle = false;

    void Start()
    {
        idleTimer = 0f;
        if (onIdle == null)
        {
            onIdle = new UnityEvent();
        }
    }

    void Update()
    {
        // Check for any input to reset the timer
        if (Input.anyKey || Input.touchCount > 0)
        {
            idleTimer = 0f;
            isIdle = false;
        }
        else
        {
            idleTimer += Time.deltaTime;

            if (!isIdle && idleTimer >= idleDuration)
            {
                isIdle = true;
                TriggerIdleAction();
            }
        }
    }

    private void TriggerIdleAction()
    {
        Debug.Log("Player has been idle for " + idleDuration + " seconds.");
        onIdle?.Invoke();
    }
}
