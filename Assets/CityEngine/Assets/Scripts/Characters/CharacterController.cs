using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
Part of the "City Engine" Asset from the Unity Asset store (unchanged)
Controls the movement and rotation of characters in the game, providing smooth navigation toward a designated target.
**/
public class CharacterController : MonoBehaviour
{

    public Vector3 target;
    public bool reachedTarget;
    public float stopDistance = 0.2f;
    public float speedMovement = 2;
    public float speedRotation = 280;

    public bool stop;

    Vector3 velocity, previuosPosition;
    Quaternion targetRotation;
    float startSpeedMovement;
    private void Start()
    {
        startSpeedMovement = speedMovement + UnityEngine.Random.Range(-0.5f, 0.5f);
        speedMovement = startSpeedMovement;
    }

    void Update()
    {
        if (stop)
        {
            if (startSpeedMovement > 0)
                startSpeedMovement -= Time.deltaTime * 10;
        }
        else
        {
            if (startSpeedMovement < speedMovement)
                startSpeedMovement += Time.deltaTime;
        }
        if (startSpeedMovement < 0)
            startSpeedMovement = 0;


        if (this.transform.position != target)
        {
            Vector3 targetDirection = target - transform.position;
            //targetDirection.y = 0;

            float destinationDistance = targetDirection.magnitude;

            if (destinationDistance >= stopDistance)
            {
                //make smooth rotation
                targetRotation = Quaternion.LookRotation(targetDirection);

                //if(target)

                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speedRotation * Time.deltaTime);
                transform.Translate(Vector3.forward * startSpeedMovement * Time.deltaTime);
                reachedTarget = false;
            }
            else
            {
                reachedTarget = true;
            }

            /*velocity = (transform.position - previuosPosition) / Time.deltaTime;
            velocity.y = 0;
            var velocityMagnitude = velocity.magnitude;
            velocity = velocity.normalized;*/

            //animations
        }
    }

    /*void LateUpdate()
    {
        previuosPosition = this.transform.position;
    }*/

    public void SetTarget(Vector3 target)
    {
        this.target = target;
        reachedTarget = false;
    }
}
