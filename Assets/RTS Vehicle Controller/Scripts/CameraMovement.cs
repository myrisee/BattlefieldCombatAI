using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

    public int TerrainHalfSize = 500;
    public float MovementMargin = 0.1f;
    public float MovementSpeed = 2.5f;
    public int ZoomSpeed = 20;
    public int MinHeight = 30, MaxHeight = 100;

    private int maxAllowedMovement;
    // 0 for true, >0 for false
    private bool isMovementAllowed = true;
    private Vector2 screenSize;

    void Start()
    {
        // Calculate maximum camera movement distance based on camera FOV and maximum height
        maxAllowedMovement = TerrainHalfSize;// -cameraMovementMargin;
        screenSize.x = Screen.width;
        screenSize.y = Screen.height;
    }

	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
            isMovementAllowed = !isMovementAllowed;

        if (!isMovementAllowed)
            return;

        Vector3 mousePos = Input.mousePosition;

        Vector3 cameraPos = transform.position + Camera.main.transform.forward * Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed;
        // Zoom camera along its forward axis
        if (cameraPos.y > MaxHeight || cameraPos.y < MinHeight)
            cameraPos = transform.position;
        if (mousePos.x < screenSize.x*MovementMargin && mousePos.x > 0)
        {
            cameraPos -= Vector3.right * MovementSpeed * (1 - mousePos.x/(screenSize.x * MovementMargin));
        }
        if (mousePos.x > Screen.width - screenSize.x * MovementMargin && mousePos.x < Screen.width)
        {
            cameraPos += Vector3.right * MovementSpeed * (1-(Screen.width - mousePos.x) / (screenSize.x * MovementMargin));
        }
        if (mousePos.y < screenSize.y * MovementMargin && mousePos.y > 0)
        {
            cameraPos -= Vector3.forward * MovementSpeed * (1-mousePos.y / (screenSize.y * MovementMargin));
        }
        if (mousePos.y > Screen.height - screenSize.y * MovementMargin && mousePos.y < Screen.height)
        {
            cameraPos += Vector3.forward * MovementSpeed * (1-(Screen.height - mousePos.y) / (screenSize.y * MovementMargin));
        }

        // Check terrain boundaries
        if (cameraPos.x > -maxAllowedMovement && cameraPos.x < maxAllowedMovement &&
            cameraPos.z > -maxAllowedMovement && cameraPos.z < maxAllowedMovement)
            transform.position = cameraPos;
    }

}
