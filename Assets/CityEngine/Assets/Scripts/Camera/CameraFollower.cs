using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Camera originalCamera;

    void LateUpdate()
    {
        if (originalCamera != null)
        {
            // Copy the position and rotation of the original camera
            transform.position = originalCamera.transform.position;
            transform.rotation = originalCamera.transform.rotation;
        }
    }
}
