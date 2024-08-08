using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public List<Vector2Int> fixedCharacterMatrixIndices = new List<Vector2Int>();
    private List<IsometricCharacterController> characters = new List<IsometricCharacterController>();
    private IsometricCharacterController selectedCharacter;

    private IsometricGameBoard gameBoard;

    void Start()
    {
        gameBoard = FindObjectOfType<IsometricGameBoard>(); // Get the instance of IsometricGameBoard
        if (gameBoard == null)
        {
            Debug.LogError("IsometricGameBoard not found in the scene.");
            return; // Exit if gameBoard is not found
        }

        InitializeFixedCharacterMatrixIndices();
    }

    void InitializeFixedCharacterMatrixIndices()
    {
        // Define fixed character positions as matrix indices
        fixedCharacterMatrixIndices.Add(new Vector2Int(1, 8));
        fixedCharacterMatrixIndices.Add(new Vector2Int(2, 8));
        fixedCharacterMatrixIndices.Add(new Vector2Int(3, 8));
        fixedCharacterMatrixIndices.Add(new Vector2Int(4, 8));
    }

    public void RegisterCharacter(IsometricCharacterController character)
    {
        if (character == null) return;

        characters.Add(character);
        AssignTargetPosition(character);

        // Subscribe to movement completion event
        character.OnMovementComplete += HandleMovementComplete;
    }

    public void HandleCharacterClick(IsometricCharacterController clickedCharacter)
    {
        if (clickedCharacter == null) return;

        // Deselect previous character if any
        if (selectedCharacter != null)
        {
            selectedCharacter.SetTargetPosition(Vector3.zero);
        }

        selectedCharacter = clickedCharacter;

        // Check if the character is blocked by any adjacent character
        Vector2Int clickedPosition = clickedCharacter.GetBoardPosition();
        Dictionary<string, bool> adjacentCells = gameBoard.CheckAdjacentCells(clickedPosition);
        Debug.Log("clickedPosition" + clickedPosition);
        bool isBlocked;

        if (clickedPosition.x == 0 && clickedPosition.y == 0)
        {
            isBlocked = adjacentCells["Up"] && adjacentCells["Right"];
        }
        else if (clickedPosition.x == 0 && clickedPosition.y == 5)
        {
            isBlocked = adjacentCells["Up"] && adjacentCells["Right"];
        }
        else if (clickedPosition.x == 0)
        {
            isBlocked = adjacentCells["Up"] && adjacentCells["Down"] && adjacentCells["Right"];
        }
        else if (clickedPosition.x == 5)
        {
            isBlocked = adjacentCells["Up"] && adjacentCells["Down"] && adjacentCells["Left"];
        }
        else if (clickedPosition.y == 0)
        {
            isBlocked = adjacentCells["Left"] && adjacentCells["Right"] && adjacentCells["Down"];
        }
        else if (clickedPosition.y == 5)
        {
            isBlocked = adjacentCells["Left"] && adjacentCells["Right"] && adjacentCells["Up"];
        }
        else
        {
            isBlocked = adjacentCells["Up"] && adjacentCells["Left"] && adjacentCells["Right"] && adjacentCells["Down"];
        }

        if (!isBlocked)
        {
            AssignTargetPosition(selectedCharacter);
        }
        else
        {
            Debug.Log("Character is blocked by adjacent character and cannot move.");
        }


    }

    void AssignTargetPosition(IsometricCharacterController character)
    {
        if (selectedCharacter == character)
        {
            foreach (Vector2Int matrixIndex in fixedCharacterMatrixIndices)
            {
                Vector3 position = gameBoard.GetWorldPositionFromBoardPosition(matrixIndex);
                if (!IsometricGameBoard.IsPositionOccupied(position))
                {
                    character.SetTargetPosition(position);
                    IsometricGameBoard.MarkPositionOccupied(position);
                    return;
                }
            }

            // If no position is available
            Debug.Log("No available position for character to move.");
        }
    }

    private void HandleMovementComplete(IsometricCharacterController character)
    {
        if (character == null) return;

        // Determine the row index of the character
        Vector2Int position = character.GetBoardPosition();
        int rowIndex = position.x;

        gameBoard.Check();
        // Implement any additional logic based on the row index if needed
        // For example, you could update some game state or UI element here
    }
}