using UnityEngine;

public class TileClick : MonoBehaviour
{
    public Vector2Int gridPosition;
    public Color originalColor;

    private SpriteRenderer sr;
    private static GridManager gridManager;

    private Vector3 mouseStartPos;
    private bool isDragging = false;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;

        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();
    }

    private void OnMouseDown()
    {
        isDragging = true;
        mouseStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;
        Vector3 mouseEndPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mouseEndPos - mouseStartPos;

        if (direction.magnitude < 0.2f) return; // arrastre muy corto, lo ignoramos

        Vector2Int targetPos = gridPosition;

        // Detectar dirección principal del arrastre
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            targetPos.x += (direction.x > 0) ? 1 : -1;
        }
        else
        {
            targetPos.y += (direction.y > 0) ? 1 : -1;
        }

        GameObject[,] grid = gridManager.GetGrid();

        if (IsInsideGrid(targetPos, grid))
        {
            GameObject otherTile = grid[targetPos.x, targetPos.y];
            if (otherTile != null)
            {
                TileClick otherClick = otherTile.GetComponent<TileClick>();
                TrySwapTiles(this, otherClick);
            }
        }
    }

    void TrySwapTiles(TileClick a, TileClick b)
    {
        // Guardamos posiciones
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;
        Vector2Int gridPosA = a.gridPosition;
        Vector2Int gridPosB = b.gridPosition;

        // Hacemos el swap
        a.transform.position = posB;
        b.transform.position = posA;

        a.gridPosition = gridPosB;
        b.gridPosition = gridPosA;

        GameObject[,] grid = gridManager.GetGrid();
        grid[a.gridPosition.x, a.gridPosition.y] = a.gameObject;
        grid[b.gridPosition.x, b.gridPosition.y] = b.gameObject;

        // Verificamos match
        var matchA = gridManager.CheckMatchAt(a.gridPosition);
        var matchB = gridManager.CheckMatchAt(b.gridPosition);

        if (matchA.Count < 3 && matchB.Count < 3)
        {
            // No hubo match → revertimos
            a.transform.position = posA;
            b.transform.position = posB;

            a.gridPosition = gridPosA;
            b.gridPosition = gridPosB;

            grid[a.gridPosition.x, a.gridPosition.y] = a.gameObject;
            grid[b.gridPosition.x, b.gridPosition.y] = b.gameObject;

            Debug.Log("Movimiento revertido: no se formó match");
        }
        else
        {
            // Hubo match → seguimos con destrucción
            gridManager.CheckDirectMatch(a.gridPosition, b.gridPosition);
        }
    }

    bool IsInsideGrid(Vector2Int pos, GameObject[,] grid)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < grid.GetLength(0) && pos.y < grid.GetLength(1);
    }
}
