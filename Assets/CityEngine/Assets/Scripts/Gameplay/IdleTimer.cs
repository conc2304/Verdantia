using UnityEngine;
using UnityEngine.Events;

public class IdleTimer : MonoBehaviour
{
    public float idleDuration = 60f;
    public UnityEvent onIdle;
    private float idleTimer;

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
        }
        else
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleDuration)
            {

                idleTimer = 0f;
            }
        }
    }

    private void TriggerIdleAction()
    {
        Debug.Log("Player has been idle for " + idleDuration + " seconds.");
        onIdle.Invoke();
    }
}
