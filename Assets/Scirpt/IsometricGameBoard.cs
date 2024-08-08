using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Runtime.CompilerServices;

public class IsometricGameBoard : MonoBehaviour
{
    public GameObject cellPrefab;
    public List<GameObject> characterPrefabs;
    public int rows = 9;
    public int columns = 6;
    public float cellWidth = 1.0f;
    public float cellHeight = 0.5f;
    public int numberOfCharactersToSpawn = 15;

    private readonly Color[] charactersColors = { Color.blue, Color.yellow, Color.magenta };
    private readonly Color[] cellsColors = { Color.red, Color.green };
    private static int[,] characterMatrix;
    private static HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    private static Dictionary<Vector3, List<Color>> positionColors = new Dictionary<Vector3, List<Color>>(); 
    private Dictionary<Color, int> movedCharacterColors = new Dictionary<Color, int>();
    private int characterSum = 0;

    public TextMeshProUGUI score;
    public TextMeshProUGUI openSpaces;
    private int score_count;
    public GameObject gameOverPanel;
    

    private CharacterManager characterManager;

    void Start()
    {
        characterManager = FindObjectOfType<CharacterManager>();
        if (characterManager == null)
        {
            Debug.LogError("CharacterManager not found in the scene.");
        }
        occupiedPositions = new HashSet<Vector3>();
        positionColors = new Dictionary<Vector3, List<Color>>();
        CreateBoard();
        SpawnCharactersOnTiles();
    }
    
    public void Check()
    {
        if (GetNumberOfCharactersOnBoard() == 0)
        {
            // Activate Game Over panel
            gameOverPanel.SetActive(true);
            gameOverPanel.GetComponentInChildren<TextMeshProUGUI>().text = score_count.ToString();
            // Handle the case when there are no characters on the board
            Debug.Log("No characters left on the board.");
        }
    }

    void CreateBoard()
    {
        Vector3[,] tilePositions = new Vector3[rows, columns];
        Color[,] cellColors = new Color[rows, columns];

        if (characterMatrix == null)
        {
            characterMatrix = new int[rows, columns];
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);
                float x = col * cellWidth;
                float z = row * cellHeight;
                cell.transform.position = new Vector3(x, -0.8f, z);

                tilePositions[row, col] = cell.transform.position;

                Color color = GetCellColor(row, col, cellColors);
                Renderer renderer = cell.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = color;
                }
                cellColors[row, col] = color;
            }
        }

        SetRowToSpecialValue(rows - 1);
    }

    void SpawnCharactersOnTiles()
    {
        if (characterPrefabs.Count == 0 || charactersColors.Length == 0) return;

        int spawnedCharacters = 0;
        List<Color> colorPool = new List<Color>();

        // Ensure numberOfCharactersToSpawn is a multiple of 3
        if (numberOfCharactersToSpawn % 3 != 0)
        {
            Debug.LogError("numberOfCharactersToSpawn must be a multiple of 3.");
            return;
        }

        // Create a color pool with sets of 3 same colors
        int setsOfThree = numberOfCharactersToSpawn / 3;
        for (int i = 0; i < setsOfThree; i++)
        {
            Color color = charactersColors[i % charactersColors.Length];
            for (int j = 0; j < 3; j++)
            {
                colorPool.Add(color);
            }
        }

        // Shuffle the color pool to randomize the color placement
        for (int i = colorPool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Color temp = colorPool[i];
            colorPool[i] = colorPool[j];
            colorPool[j] = temp;
        }

        // Iterate through the board and spawn characters with the shuffled colors
        for (int row = 0; row < rows && spawnedCharacters < numberOfCharactersToSpawn; row++)
        {
            for (int col = 0; col < columns && spawnedCharacters < numberOfCharactersToSpawn; col++)
            {
                Vector3 cellPosition = new Vector3(col * cellWidth, 0, row * cellHeight);
                int randomCharacterIndex = Random.Range(0, characterPrefabs.Count);
                GameObject characterPrefab = characterPrefabs[randomCharacterIndex];

                Vector3 spawnPosition = new Vector3(cellPosition.x, 0, cellPosition.z);
                GameObject character = Instantiate(characterPrefab, spawnPosition, Quaternion.identity, transform);

                Color characterColor = colorPool[spawnedCharacters];
                Renderer characterRenderer = character.GetComponent<Renderer>();
                if (characterRenderer != null)
                {
                    characterRenderer.material.color = characterColor;
                }

                characterMatrix[row, col] = 1;

                IsometricCharacterController controller = character.GetComponent<IsometricCharacterController>();
                if (controller == null)
                {
                    controller = character.AddComponent<IsometricCharacterController>();
                }
                controller.Initialize(this, new Vector2Int(row, col));

                characterManager.RegisterCharacter(controller);

                if (!positionColors.ContainsKey(spawnPosition))
                {
                    positionColors[spawnPosition] = new List<Color>();
                }
                positionColors[spawnPosition].Add(characterColor);

                spawnedCharacters++;
            }
        }
    }


    public void UpdateCharacterMatrix(Vector2Int oldPosition, Vector2Int newPosition)
    {
        if (oldPosition.x >= 0 && oldPosition.x < characterMatrix.GetLength(0) &&
            oldPosition.y >= 0 && oldPosition.y < characterMatrix.GetLength(1))
        {
            characterMatrix[oldPosition.x, oldPosition.y] = 0;
            occupiedPositions.Remove(GetWorldPositionFromBoardPosition(oldPosition));
        }

        if (newPosition.x >= 0 && newPosition.x < characterMatrix.GetLength(0) &&
            newPosition.y >= 0 && newPosition.y < characterMatrix.GetLength(1))
        {
            characterMatrix[newPosition.x, newPosition.y] = 1;
            occupiedPositions.Add(GetWorldPositionFromBoardPosition(newPosition));
        }

        PrintCharacterMatrix();
    }

    public Vector2Int GetBoardPositionFromWorldPosition(Vector3 worldPosition)
    {
        int row = Mathf.RoundToInt(worldPosition.z / cellHeight);
        int col = Mathf.RoundToInt(worldPosition.x / cellWidth);
        return new Vector2Int(row, col);
    }

    private void PrintCharacterMatrix()
    {
        string matrixString = "";
        for (int row = 0; row < characterMatrix.GetLength(0); row++)
        {
            for (int col = 0; col < characterMatrix.GetLength(1); col++)
            {
                matrixString += characterMatrix[row, col] + " ";
            }
            matrixString += "\n";
        }
        Debug.Log(matrixString);
    }

    Color GetCellColor(int row, int col, Color[,] cellColors)
    {
        // Get colors of adjacent cells
        Color left = (col > 0) ? cellColors[row, col - 1] : Color.clear;
        Color below = (row > 0) ? cellColors[row - 1, col] : Color.clear;

        // Choose a color different from adjacent cells
        foreach (Color color in cellsColors)
        {
            if (color != left && color != below)
            {
                return color;
            }
        }

        // Default to first color if no other options
        return cellsColors[0];
    }

    // Static method to access characterMatrix from anywhere in the game
    public static int[,] GetCharacterMatrix()
    {
        return characterMatrix;
    }

    // Method to set the nth row of characterMatrix to a special value (e.g., 2)
    public void SetRowToSpecialValue(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= rows)
        {
            Debug.LogError("Invalid row index.");
            return;
        }

        for (int col = 0; col < columns; col++)
        {
            characterMatrix[rowIndex, col] = 2;

            // Update the visual appearance (color) of the corresponding cell
            GameObject cell = GetCellAtPosition(new Vector2Int(rowIndex, col));
            if (cell != null)
            {
                Renderer renderer = cell.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.white; // Set color to white
                }
            }
        }
    }

    // Static method to check if a position is occupied
    public static bool IsPositionOccupied(Vector3 position)
    {
        return occupiedPositions.Contains(position);
    }

    // Static method to mark a position as occupied
    public static void MarkPositionOccupied(Vector3 position)
    {
        occupiedPositions.Add(position);
    }

    // Helper method to get the cell GameObject at a specific board position
    private GameObject GetCellAtPosition(Vector2Int position)
    {
        Transform boardTransform = transform;
        Vector3 cellPosition = new Vector3(position.y * cellWidth, -0.8f, position.x * cellHeight);
        foreach (Transform child in boardTransform)
        {
            if (child.position == cellPosition)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public bool CheckCharactersInRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= rows)
        {
            Debug.LogError("Invalid row index.");
            return false;
        }

        for (int col = 0; col < columns; col++)
        {
            if (characterMatrix[rowIndex, col] == 1)
            {
                // Do something if needed
            }

            // Update the visual appearance (color) of the corresponding cell
            GameObject cell = GetCellAtPosition(new Vector2Int(rowIndex, col));
            if (cell != null)
            {
                Renderer renderer = cell.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.white; // Set color to white
                }
            }
        }

        return false;
    }

    public void AddMovedCharacterColor(Color color)
    {
        characterSum++;
       
        if (movedCharacterColors.ContainsKey(color))
        {
            movedCharacterColors[color]++;
        }
        else
        {
            movedCharacterColors[color] = 1;
        }       
        // Check if any color has reached the count of 3
        if (movedCharacterColors[color] >= 3)
        {
            DestroyCharactersByColor(color);
            characterSum -=3;
            score_count += 5;
            score.text = score_count.ToString();
        }
        openSpaces.text = "Open Spaces: " + (4 - characterSum).ToString();
        if (characterSum == 4)
        {
            // Activate Game Over panel
            gameOverPanel.SetActive(true);
            TextMeshProUGUI[] textComponents = gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>();
            textComponents[0].text = score_count.ToString();
            textComponents[1].text = "You Lost";

        }
        
    }

    private void DestroyCharactersByColor(Color color)
    {
        List<Transform> charactersToDestroy = new List<Transform>();

        // Find characters with the specified color and add them to the list
        foreach (Transform child in transform)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null && renderer.material.color == color)
            {
                charactersToDestroy.Add(child);
            }
        }

        // Destroy characters and update characterMatrix and occupiedPositions
        foreach (Transform character in charactersToDestroy)
        {
            Vector2Int boardPosition = GetBoardPositionFromWorldPosition(character.position);
            if(boardPosition.x == rows-1)
            {
                characterMatrix[boardPosition.x, boardPosition.y] = 2;
                occupiedPositions.Remove(character.position);
                Destroy(character.gameObject);
            }
            
        }

        // Reset color count in movedCharacterColors
        if (movedCharacterColors.ContainsKey(color))
        {
            movedCharacterColors[color] = 0;
        }

        // Optionally, you can print the updated character matrix
        PrintCharacterMatrix();
    }


    public void PrintMovedCharacterColors()
    {
        string colorsString = "Moved Character Colors: ";
        foreach (var entry in movedCharacterColors)
        {
            colorsString += $"{entry.Key} (Count: {entry.Value}), ";
        }
        Debug.Log(colorsString.TrimEnd(',', ' '));
    }

    public Vector3 GetWorldPositionFromBoardPosition(Vector2Int boardPosition)
    {
        return new Vector3(boardPosition.x * cellWidth, 0, boardPosition.y * cellHeight);
    }

    // To check adjacent cells
    public Dictionary<string, bool> CheckAdjacentCells(Vector2Int position)
    {
        Dictionary<string, bool> adjacentCellsStatus = new Dictionary<string, bool>
        {
            { "Up", false },
            { "Right", false },
            { "Down", false },
            { "Left", false }
        };

        // Check Up
        if (position.x > 0)
        {
            if (position.x + 1 == -1)
            {
                adjacentCellsStatus["Up"] = true;
            }
            else
            {
                adjacentCellsStatus["Up"] = characterMatrix[position.x + 1, position.y] == 1;
            }

        }

        // Check Right
        if (position.y < columns - 1)
        {
            if (position.y + 1 == -1)
            {
                adjacentCellsStatus["Right"] = true;
            }
            else
            {
                adjacentCellsStatus["Right"] = characterMatrix[position.x, position.y + 1] == 1;
            }
        }

        // Check Down
        if (position.x < rows - 1)
        {
            if (position.x - 1 == -1)
            {
                adjacentCellsStatus["Down"] = true;
            }
            else
            {
                adjacentCellsStatus["Down"] = characterMatrix[position.x - 1, position.y] == 1;
            }

        }

        // Check Left
        if (position.y > 0)
        {
            if (position.y + 1 == -1)
            {
                adjacentCellsStatus["Left"] = true;
            }
            else
            {
                adjacentCellsStatus["Left"] = characterMatrix[position.x, position.y - 1] == 1;
            }

        }

        return adjacentCellsStatus;
    }
    // Method to get the number of characters on the board
    public int GetNumberOfCharactersOnBoard()
    {
        int count = 0;
        for (int row = 0; row < characterMatrix.GetLength(0); row++)
        {
            for (int col = 0; col < characterMatrix.GetLength(1); col++)
            {
                if (characterMatrix[row, col] == 1)
                {
                    count++;
                }
            }
        }
        return count;
    }
}