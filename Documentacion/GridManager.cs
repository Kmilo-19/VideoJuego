using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // ← Agregado para reiniciar escena
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int width = 6;
    public int height = 8;
    public GameObject tilePrefab;
    public float spacing = 1.1f;

    public int maxMoves = 15;
    private int currentMoves;
    public bool gameOver = false;

    public TextMeshProUGUI movesText;
    public TextMeshProUGUI scoreText;

    private int score = 0;
    private GameObject[,] tileGrid;

    void Start()
    {
        currentMoves = maxMoves;
        score = 0;
        GenerateGrid();
        UpdateMovesUI();
        UpdateScoreUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame(); // ← Reinicio con tecla R
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void GenerateGrid()
    {
        tileGrid = new GameObject[width, height];

        Color[] colors = new Color[] {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.magenta,
            Color.cyan
        };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 spawnPosition = new Vector2(x * spacing, y * spacing);
                GameObject tile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity, transform);

                Color randomColor = colors[Random.Range(0, colors.Length)];
                tile.GetComponent<SpriteRenderer>().color = randomColor;

                TileClick tileClick = tile.GetComponent<TileClick>();
                tileClick.gridPosition = new Vector2Int(x, y);
                tileClick.originalColor = randomColor;

                tileGrid[x, y] = tile;
            }
        }
    }

    public GameObject[,] GetGrid() => tileGrid;

    public void CheckDirectMatch(Vector2Int posA, Vector2Int posB)
    {
        if (gameOver) return;

        List<GameObject> matches = new();

        bool isHorizontal = posA.y == posB.y;

        if (isHorizontal)
            matches = GetLineMatch(posA.y, true);
        else
            matches = GetLineMatch(posA.x, false);

        if (matches.Count >= 3)
        {
            foreach (GameObject tile in matches)
            {
                if (tile == null) continue;
                TileClick click = tile.GetComponent<TileClick>();
                if (click != null)
                    tileGrid[click.gridPosition.x, click.gridPosition.y] = null;

                TileEffect effect = tile.GetComponent<TileEffect>();
                if (effect != null)
                    effect.PlayDestroyEffect();
                else
                    Destroy(tile);

                score += 10;
            }

            UpdateScoreUI();
            UseMove();
            Invoke(nameof(DelayedInitialCollapse), 0.4f);
        }
        else
        {
            Debug.Log("No se encontraron coincidencias directas.");
        }
    }

    List<GameObject> GetLineMatch(int fixedIndex, bool horizontal)
    {
        List<GameObject> matchTemp = new();
        List<GameObject> finalMatches = new();
        Color? lastColor = null;

        for (int i = 0; i < (horizontal ? width : height); i++)
        {
            GameObject tile = horizontal ? tileGrid[i, fixedIndex] : tileGrid[fixedIndex, i];
            if (tile == null)
            {
                matchTemp.Clear();
                lastColor = null;
                continue;
            }

            Color tileColor = tile.GetComponent<TileClick>().originalColor;
            if (tileColor == lastColor)
            {
                matchTemp.Add(tile);
            }
            else
            {
                if (matchTemp.Count >= 3)
                    finalMatches.AddRange(matchTemp);

                matchTemp = new List<GameObject> { tile };
                lastColor = tileColor;
            }
        }

        if (matchTemp.Count >= 3)
            finalMatches.AddRange(matchTemp);

        return new List<GameObject>(new HashSet<GameObject>(finalMatches));
    }

    void DelayedInitialCollapse()
    {
        CollapseGrid(true);
    }

    public void CollapseGrid(bool isInitialCollapse)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y < height; y++)
            {
                if (tileGrid[x, y] == null) continue;

                int targetY = y;
                while (targetY > 0 && tileGrid[x, targetY - 1] == null)
                    targetY--;

                if (targetY != y)
                {
                    GameObject tile = tileGrid[x, y];
                    tileGrid[x, targetY] = tile;
                    tileGrid[x, y] = null;

                    tile.GetComponent<TileClick>().gridPosition = new Vector2Int(x, targetY);
                    tile.transform.position = new Vector2(x * spacing, targetY * spacing);
                }
            }
        }

        if (isInitialCollapse)
            Invoke(nameof(RefillGrid), 0.2f);
    }

    void RefillGrid()
    {
        Color[] colors = new Color[] {
            Color.red, Color.green, Color.blue,
            Color.yellow, Color.magenta, Color.cyan
        };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tileGrid[x, y] == null)
                {
                    Vector2 spawnPosition = new Vector2(x * spacing, y * spacing);
                    GameObject tile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity, transform);

                    Color randomColor = colors[Random.Range(0, colors.Length)];
                    tile.GetComponent<SpriteRenderer>().color = randomColor;

                    TileClick tileClick = tile.GetComponent<TileClick>();
                    tileClick.gridPosition = new Vector2Int(x, y);
                    tileClick.originalColor = randomColor;

                    tileGrid[x, y] = tile;
                }
            }
        }

        Invoke(nameof(CheckFullGridForMatches), 0.2f);
    }

    void CheckFullGridForMatches()
    {
        List<Vector2Int> positionsToCheck = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tileGrid[x, y] != null)
                    positionsToCheck.Add(new Vector2Int(x, y));
            }
        }
    }

    public List<GameObject> CheckMatchAt(Vector2Int pos)
    {
        List<GameObject> matches = new();

        matches.AddRange(GetLineMatch(pos.y, true));
        matches.AddRange(GetLineMatch(pos.x, false));

        return new List<GameObject>(new HashSet<GameObject>(matches));
    }

    public void UseMove()
    {
        if (gameOver) return;

        currentMoves--;
        Debug.Log("Movimientos restantes: " + currentMoves);

        UpdateMovesUI();

        if (currentMoves <= 0)
        {
            gameOver = true;
            Debug.Log("GANASTE");
            movesText.text = "GANASTE";
        }
    }

    void UpdateMovesUI()
    {
        if (movesText != null)
        {
            if (gameOver)
                movesText.text = "GANASTE";
            else
                movesText.text = "Movimientos restantes: " + currentMoves.ToString();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Puntaje: " + score.ToString();
    }
    public void QuitGame()
{
    Debug.Log("El juego se está cerrando...");
    Application.Quit();
}

}
