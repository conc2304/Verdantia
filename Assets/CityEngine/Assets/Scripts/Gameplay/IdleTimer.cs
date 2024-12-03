using UnityEngine;
using UnityEngine.Events;

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
