using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumansNav : MonoBehaviour
{

    public PathTarget currentPathTarget;
    CharacterController controller;

    int target, changeTarget;
    bool chooseBranch;

    private void Awake()
    {
        controller = this.GetComponent<CharacterController>();
    }

    private void Start()
    {
        target = Random.Range(0, 2);
        controller.SetTarget(currentPathTarget.GetPosition());
    }

    void Update()
    {
        if (controller.reachedTarget)
        {
            chooseBranch = false;

            if (currentPathTarget.branches != null && currentPathTarget.branches.Count > 0)
            {
                if (Random.Range(0.1f, 1.0f) <= currentPathTarget.branchesRatio)
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

                if(newCurrentPathTarget != null)
                    currentPathTarget = newCurrentPathTarget;
                else
                    currentPathTarget = currentPathTarget.nextPathTarget;
            }
            else
            {
                changeTarget = Random.Range(0, 5);
                if (changeTarget == 0)
                {
                    if (target == 0)
                        target = 1;
                    if (target == 1)
                        target = 0;
                }

                if (target == 1)
                {
                    if (currentPathTarget.nextPathTarget != null)
                    {
                        currentPathTarget = currentPathTarget.nextPathTarget;
                        target = 1;
                    }
                    else
                    {
                        currentPathTarget = currentPathTarget.previousPathTarget;
                        target = 0;
                    }
                }
                else
                {
                    if (currentPathTarget.previousPathTarget != null)
                    {
                        currentPathTarget = currentPathTarget.previousPathTarget;
                        target = 0;
                    }
                    else
                    {
                        currentPathTarget = currentPathTarget.nextPathTarget;
                        target = 1;
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
}
