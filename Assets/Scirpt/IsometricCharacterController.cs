using System.Collections.Generic;
using UnityEngine;

public class IsometricCharacterController : MonoBehaviour
{
    private Vector3 targetPosition;
    private bool movingToFixedPoint = false;
    private IsometricGameBoard gameBoard;
    private Vector2Int boardPosition; // Position of the character on the board

    public float moveSpeed = 5f; // Speed of movement

    public delegate void MovementCompleteHandler(IsometricCharacterController character);
    public event MovementCompleteHandler OnMovementComplete;

    // Initialize the controller with the game board reference and board position
    public void Initialize(IsometricGameBoard board, Vector2Int position)
    {
        gameBoard = board;
        boardPosition = position;
    }

    void Update()
    {       
        HandleInput();
        if (movingToFixedPoint)
        {
            MoveTowardsFixedPoint();
        }        
    }

    public void SetTargetPosition(Vector3 position)
    {
        if (movingToFixedPoint)
        {
            FinalizeMovement();
        }

        targetPosition = position;

        if (position == Vector3.zero)
        {
            movingToFixedPoint = false;
        }
        else
        {
            if (!IsometricGameBoard.IsPositionOccupied(position)) // Calling the static method correctly
            {
                movingToFixedPoint = true;
            }
            else
            {
                Debug.Log("Cannot move character to the target position as it is occupied.");
                movingToFixedPoint = false;
            }
        }
    }

    void MoveTowardsFixedPoint()
    {
        Vector3 currentPosition = transform.position;

        // Determine if movement should be along X or Z axis
        bool moveAlongX = Mathf.Abs(targetPosition.x - currentPosition.x) > Mathf.Abs(targetPosition.z - currentPosition.z);
        bool moveAlongZ = !moveAlongX;

        // Get the next board position in the intended direction
        Vector2Int nextBoardPosition = GetNextBoardPosition(moveAlongX, moveAlongZ);

        // Check if the next position is occupied
        if (IsometricGameBoard.GetCharacterMatrix()[nextBoardPosition.x, nextBoardPosition.y] == 1)
        {
            // Find an alternative path if the next position is occupied
            Dictionary<string, bool> adjacentCells = gameBoard.CheckAdjacentCells(boardPosition);
            if (moveAlongX)
            {
                if (adjacentCells["Up"] == false)
                {
                    moveAlongZ = true;
                }
                else if (adjacentCells["Down"] == false)
                {
                    moveAlongZ = true;
                }
                else
                {
                    movingToFixedPoint = false;
                    return;
                }
            }
            else if (moveAlongZ)
            {
                if (adjacentCells["Right"] == false)
                {
                    moveAlongX = true;
                }
                else if (adjacentCells["Left"] == false)
                {
                    moveAlongX = true;
                }
                else
                {
                    movingToFixedPoint = false;
                    return;
                }
            }
        }

        if (moveAlongX)
        {
            if (Mathf.Abs(targetPosition.x - currentPosition.x) > 0.1f)
            {
                // Move along the X axis
                transform.position = Vector3.MoveTowards(currentPosition, new Vector3(targetPosition.x, currentPosition.y, currentPosition.z), Time.deltaTime * moveSpeed);
            }
            else
            {
                // Switch to Z axis movement
                if (Mathf.Abs(targetPosition.z - currentPosition.z) > 0.1f)
                {
                    // Move along the Z axis
                    transform.position = Vector3.MoveTowards(currentPosition, new Vector3(currentPosition.x, currentPosition.y, targetPosition.z), Time.deltaTime * moveSpeed);
                }
                else
                {
                    // Stop moving when the target position is reached
                    FinalizeMovement();
                }
            }
        }
        else if (moveAlongZ)
        {
            if (Mathf.Abs(targetPosition.z - currentPosition.z) > 0.1f)
            {
                // Move along the Z axis
                transform.position = Vector3.MoveTowards(currentPosition, new Vector3(currentPosition.x, currentPosition.y, targetPosition.z), Time.deltaTime * moveSpeed);
            }
            else
            {
                // Switch to X axis movement
                if (Mathf.Abs(targetPosition.x - currentPosition.x) > 0.1f)
                {
                    // Move along the X axis
                    transform.position = Vector3.MoveTowards(currentPosition, new Vector3(targetPosition.x, currentPosition.y, currentPosition.z), Time.deltaTime * moveSpeed);
                }
                else
                {
                    // Stop moving when the target position is reached
                    FinalizeMovement();
                }
            }
        }
    }

    private Vector2Int GetNextBoardPosition(bool moveAlongX, bool moveAlongZ)
    {
        if (moveAlongX)
        {
            return new Vector2Int(boardPosition.x, boardPosition.y + (targetPosition.x > transform.position.x ? 1 : -1));
        }
        else if (moveAlongZ)
        {
            return new Vector2Int(boardPosition.x + (targetPosition.z > transform.position.z ? 1 : -1), boardPosition.y);
        }
        return boardPosition;
    }

    private void FinalizeMovement()
    {
        // Stop moving and finalize position
        transform.position = targetPosition;
        movingToFixedPoint = false;

        // Calculate new board position
        Vector2Int newBoardPosition = gameBoard.GetBoardPositionFromWorldPosition(targetPosition);

        // Update the character matrix in IsometricGameBoard
        gameBoard.UpdateCharacterMatrix(boardPosition, newBoardPosition);

        // Add the color of the moved character to the movedCharacterColors list
        Renderer characterRenderer = GetComponent<Renderer>();
        if (characterRenderer != null)
        {
            Color characterColor = characterRenderer.material.color;
            gameBoard.AddMovedCharacterColor(characterColor);
        }

        // Update the current board position
        boardPosition = newBoardPosition;

        gameBoard.Check();
        // Print moved character colors
        gameBoard.PrintMovedCharacterColors();

        // Notify that movement is complete
        OnMovementComplete?.Invoke(this);
    }

    private void HandleInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }
#endif
#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleClick(Input.GetTouch(0).position);
        }
#endif
    }

    private void HandleClick(Vector3 clickPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(clickPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                // Notify the CharacterManager that this character was clicked
                CharacterManager characterManager = FindObjectOfType<CharacterManager>();
                if (characterManager != null)
                {
                    characterManager.HandleCharacterClick(this);
                }
            }
        }
    }

    public Vector2Int GetBoardPosition()
    {
        return boardPosition;
    }
}