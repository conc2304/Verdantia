using UnityEngine;
using UnityEditor;


/**
Part of the "City Engine" Asset from the Unity Asset store (unchanged)
Custom editor tool for Unity that visually enhances the PathTarget objects in the Scene view. 
It draws gizmos (visual indicators) to represent the PathTarget's position, width, and connections to other PathTargets (previous, next, and branches). 
The gizmos use different colors to indicate specific connections (e.g., red for previous targets, green for next targets) 
and provide a clear visual aid for designing and debugging path networks in the Unity editor.
**/

[InitializeOnLoad()]
public class PathTargetEditor
{
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    public static void OnDrawSceneGizmo(PathTarget pathTarget, GizmoType gizmoType)
    {
        if ((gizmoType & GizmoType.Selected) != 0)
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.yellow * 0.5f;
        }

        Gizmos.DrawSphere(pathTarget.transform.position, 0.1f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(pathTarget.transform.position + (pathTarget.transform.right * pathTarget.width / 2f),
            pathTarget.transform.position - (pathTarget.transform.right * pathTarget.width / 2f));

        if (pathTarget.previousPathTarget != null)
        {
            Gizmos.color = Color.red;
            Vector3 offset = pathTarget.transform.right * pathTarget.width / 2f;
            Vector3 offsetTo = pathTarget.previousPathTarget.transform.right * pathTarget.previousPathTarget.width / 2f;

            Gizmos.DrawLine(pathTarget.transform.position + offset, pathTarget.previousPathTarget.transform.position + offsetTo);
        }
        if (pathTarget.nextPathTarget != null)
        {
            Gizmos.color = Color.green;
            Vector3 offset = pathTarget.transform.right * -pathTarget.width / 2f;
            Vector3 offsetTo = pathTarget.nextPathTarget.transform.right * -pathTarget.nextPathTarget.width / 2f;

            Gizmos.DrawLine(pathTarget.transform.position + offset, pathTarget.nextPathTarget.transform.position + offsetTo);
        }

        if (pathTarget.branches != null)
        {
            foreach (PathTarget branch in pathTarget.branches)
            {
                if (branch != null)
                    Gizmos.DrawLine(pathTarget.transform.position, branch.transform.position);

            }
        }

    }

}
