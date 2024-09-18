using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CraneControll : MonoBehaviour
{

    public Transform top;
    public Transform hook;

    public int speed = 10;

    public LineRenderer rope;

    float timer = -1;
    Quaternion rotationTo;
    
    [SerializeField]
    public Transform[] pathToTargets;

    Transform target;

    bool rotateFinished, hookDownFinished, hookUpFinished = false;

    float startHookY;
    

    void Start()
    {
        hookUpFinished = true;
        startHookY = (float)System.Math.Round(hook.position.y, 2);
    }

    void Update()
    {
        if (hookUpFinished && timer < 0)
        {
            bool checkTarget = true;
            while (checkTarget)
            {
                Transform newTarget = pathToTargets[UnityEngine.Random.Range(0, pathToTargets.Length)];
                if (newTarget != target)
                {
                    checkTarget = false;
                    target = newTarget;
                }

            }

            //rotate to target
            Vector3 lookPos = target.position - top.transform.position;
            Quaternion lookRot = Quaternion.LookRotation(lookPos, Vector3.up);
            float eulerY = lookRot.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, eulerY, 0);
            rotationTo = rotation;

            hookUpFinished = false;
            hookDownFinished = false;
        }
       
        //rotate
        top.rotation = Quaternion.RotateTowards(top.rotation, rotationTo, speed * Time.deltaTime);

        if (Mathf.Approximately(Mathf.Abs(top.rotation.eulerAngles.y), Mathf.Abs(rotationTo.eulerAngles.y)))
            rotateFinished = true;

        //hook move to target
        if (rotateFinished && hookDownFinished == false && hookUpFinished == false)
        {
            rotateFinished = false;
            hook.position = Vector3.MoveTowards(hook.position, new Vector3(target.position.x, target.position.y - 0.1f, target.position.z), speed / 10 * Time.deltaTime);
        }

        if (hook.position.y <= target.position.y)
            hookDownFinished = true;

        //hook move back
        if (hookDownFinished)
            hook.position = Vector3.MoveTowards(hook.position, new Vector3(hook.position.x, startHookY + 0.1f, hook.position.z), speed / 20 * Time.deltaTime);

        if (hook.position.y > startHookY && hookDownFinished == true)
        {
            hookDownFinished = false;
            hookUpFinished = true;
            timer = UnityEngine.Random.Range(0f, 1f);
        }

        //additional
        rope.SetPosition(0, hook.position);
        rope.SetPosition(1, new Vector3(hook.position.x, top.position.y, hook.position.z));

        timer -= Time.deltaTime;
    }

}
