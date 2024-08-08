using UnityEngine;
using UnityEngine.UIElements;

public class IsometricCameraController : MonoBehaviour
{
    public IsometricGameBoard gameBoard;
    public float cameraHeight = 10.0f;
    public float cameraDistance = 10.0f;
    public float cameraAngle = 45.0f;

    void Start()
    {
        if (gameBoard != null)
        {
             CenterCameraOnBoard();
        }
        else
        {
            Debug.LogError("IsometricGameBoard reference not set in the inspector.");
        }
    }

    void CenterCameraOnBoard()
    {
        // Calculate the center of the board
        float boardWidth = gameBoard.columns * gameBoard.cellWidth;
        float boardHeight = gameBoard.rows * gameBoard.cellHeight;
        Vector3 boardCenter = new Vector3(boardWidth / 2, boardHeight / 2, 0);

        // Position the camera
        float angleRad = cameraAngle * Mathf.Deg2Rad;
        float x = boardCenter.x - cameraDistance * Mathf.Cos(angleRad);
        float y = boardCenter.y - cameraDistance * Mathf.Sin(angleRad);

        transform.position = new Vector3(x, y, -cameraHeight);
        transform.LookAt(boardCenter);
    }
}
