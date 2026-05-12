using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap Tilemap { get; private set; }
    [SerializeField]
    private Tile _tileUnknown;
    [SerializeField]
    private Tile _tileEmpty;
    [SerializeField]
    private Tile _tileMine;
    [SerializeField]
    private Tile _tileExploded;
    [SerializeField]
    private Tile _tileFlag;
    [SerializeField]
    private Tile[] _tileNumbers;

    void Awake()
    {
        Tilemap = GetComponent<Tilemap>();
    }

    public void Draw(Cell[,] state)
    {
        int rows = state.GetLength(0);
        int cols = state.GetLength(1);
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) {
                Cell cell = state[r, c];
                Tilemap.SetTile(cell.position, GetTile(cell));
            }
        }
    }

    public void DrawDebug(Cell[,] state)
    {
        int rows = state.GetLength(0);
        int cols = state.GetLength(1);
        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) {
                Cell cell = state[r, c];
                Tile tile = cell.type switch
                {
                    Cell.Type.Empty => _tileEmpty,
                    Cell.Type.Mine => _tileMine,
                    Cell.Type.Number => _tileNumbers[cell.number - 1],
                    _ => null,
                };
                Tilemap.SetTile(cell.position, tile);
            }
        }
    }

    private Tile GetTile(Cell cell)
    {
        if (cell.revealed) {
            return cell.type switch
            {
                Cell.Type.Empty => _tileEmpty,
                Cell.Type.Mine => cell.exploded ? _tileExploded : _tileMine,
                Cell.Type.Number => _tileNumbers[cell.number - 1],
                _ => null,
            };
        } else if (cell.flagged) {
            return _tileFlag;
        } else {
            return _tileUnknown;
        }
    }
}