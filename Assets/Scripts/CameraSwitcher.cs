using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera camera1; // Drag the first camera into this field in the Inspector
    public Camera camera2; // Drag the second camera into this field in the Inspector

    void Start()
    {
        // Ensure only one camera is active at the start
        camera1.enabled = true;
        camera2.enabled = false;
    }

    void Update()
    {
        // Switch cameras when the player presses the "C" key
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCameras();
        }
    }

    void SwitchCameras()
    {
        // Toggle the enabled state of both cameras
        camera1.enabled = !camera1.enabled;
        camera2.enabled = !camera2.enabled;
    }
}
