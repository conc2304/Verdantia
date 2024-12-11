using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
Part of the "City Engine" Asset from the Unity Asset store (unchanged)
defines a PathTarget, a node in a network of paths used for navigation in the game. 
It includes functionality for branching and random position selection within a defined width.
**/

public class PathTarget : MonoBehaviour
{

    public PathTarget previousPathTarget;
    public PathTarget nextPathTarget;

    [Range(0f, 7f)]
    public float width = 1;

    public List<PathTarget> branches = new List<PathTarget>();

    [Range(0f, 1f)]
    public float branchesRatio = 0.5f;

    Vector3 minBound;
    Vector3 maxBound;

    public Vector3 GetPosition()
    {
        minBound = transform.position + transform.right * width / 2f;
        maxBound = transform.position - transform.right * width / 2f;

        return Vector3.Lerp(minBound, maxBound, Random.Range(0f, 1f));
    }

}
