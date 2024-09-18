using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarNav : MonoBehaviour
{

    public PathTarget currentPathTarget;
    CharacterController controller;

    int target;
    bool chooseBranch;

    float timer, afterStopWait;

    public int priority = 0;

    int waitTooLong = 0;

    private void Awake()
    {
        controller = this.GetComponent<CharacterController>();
    }

    private void Start()
    {
        controller.SetTarget(currentPathTarget.GetPosition());
    }


    void Update()
    {
        //Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z), transform.TransformDirection(Vector3.forward) * 2, Color.yellow);

        if (afterStopWait > 0)
        {
            afterStopWait -= Time.deltaTime;
            if (controller.reachedTarget)
                ReachedTarget();
        }
        else
        {
            RaycastHit hit;

            if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z), transform.TransformDirection(Vector3.forward), out hit, 2))
            {
                CarNav otherCarNav = hit.collider.transform.parent.GetComponent<CarNav>();
                if (hit.collider.transform.parent.tag == "Car")
                {
                    if (otherCarNav.controller.stop)
                    {
                        if (priority < otherCarNav.priority)
                        {
                            DontMove();
                        }
                        else
                        {
                            Move();
                        }
                    }
                    else
                    {
                        DontMove();
                    }
                }
                else
                {
                    DontMove();
                }
            }
            else
            {
                Move();
            }
        }

    }

    void Move()
    {
        waitTooLong = 0;
        controller.stop = false;
        if (controller.reachedTarget)
            ReachedTarget();
    }

    void DontMove()
    {
        waitTooLong += 1;
        if (waitTooLong > 3)
        {
            afterStopWait = -2;
            Move();
        }
        else
        {
            controller.stop = true;
            afterStopWait = 0.4f;
        }
    }

    void ReachedTarget()
    {
        chooseBranch = false;

        if (currentPathTarget.branches != null && currentPathTarget.branches.Count > 0)
        {
            if (Random.Range(0f, 1f) <= currentPathTarget.branchesRatio)
                chooseBranch = true;
            else
                chooseBranch = false;
        }

        if (chooseBranch)
        {
            PathTarget newCurrentPathTarget = null;
            while (newCurrentPathTarget == null && currentPathTarget.branches.Count != 0)
            {
                newCurrentPathTarget = currentPathTarget.branches[Random.Range(0, currentPathTarget.branches.Count - 1)];
                if (newCurrentPathTarget == null)
                    currentPathTarget.branches.Remove(newCurrentPathTarget);
            }

            if (newCurrentPathTarget != null)
                currentPathTarget = newCurrentPathTarget;
            else
                currentPathTarget = currentPathTarget.nextPathTarget;
        }
        else
        {
            if (target == 0)
            {
                if (currentPathTarget.nextPathTarget != null)
                {
                    currentPathTarget = currentPathTarget.nextPathTarget;
                }
                else
                {
                    currentPathTarget = currentPathTarget.previousPathTarget;
                    target = 1;
                }
            }
            else
            {
                if (currentPathTarget.previousPathTarget != null)
                {
                    currentPathTarget = currentPathTarget.previousPathTarget;
                }
                else
                {
                    currentPathTarget = currentPathTarget.nextPathTarget;
                    target = 0;
                }

            }
        }

        if (currentPathTarget != null)
        {
            controller.SetTarget(currentPathTarget.GetPosition());
        }
        else
        {
            Spawner.cars.Remove(this.transform);
            Destroy(this.gameObject);
        }
    }

}
