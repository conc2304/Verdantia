using UnityEngine;

// Activates a second display if it's connected to the system.
public class MultiDisplay : MonoBehaviour
{
    void Start()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate(); // Activate the second display
        }
    }

}
