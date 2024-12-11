using UnityEngine;

/**
Ensures that the attached object follows the position and rotation of a specified camera, creating a synchronized view.
Used to render the camera view from the game to the Touch Gui Trackpad
**/
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
