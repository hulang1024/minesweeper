using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    private int _rows = 9;
    [SerializeField]
    private int _cols = 9;
    [SerializeField]
    private int _mineCount = 10;
    private Board _board;
    private Cell[,] _state;
    private bool gameOver;
    private bool _isDebugDraw;

    void OnValidate()
    {
        _mineCount = Mathf.Clamp(_mineCount, 0, _rows * _cols);
    }
    
    void Awake()
    {
        _board = GetComponentInChildren<Board>();
    }

    void Start()
    {
        NewGame();
    }

    void Update()
    {
        if (!gameOver) {
            if (Input.GetMouseButtonDown(1)) {
                Flag();
            } else if (Input.GetMouseButtonDown(0)) {
                Reveal();
            }
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            NewGame();
        } else if (Input.GetKeyDown(KeyCode.V)) {
            _isDebugDraw = !_isDebugDraw;
            if (_isDebugDraw) {
                _board.DrawDebug(_state);
            } else {
                _board.Draw(_state);
            }
        }
    }

    private void Flag()
    {
        var cell = GetCellAtMousePosition();
        if (cell.HasValue) {
            var c = cell.Value;
            if (!c.revealed) {
                c.flagged = !c.flagged;
                _state[c.position.y, c.position.x] = c;
                _board.Draw(_state);
            }
        }
    }

    private void Reveal()
    {
        var naCell = GetCellAtMousePosition();
        if (naCell.HasValue) {
            var cell = naCell.Value;
            if (cell.revealed) {
                if (cell.type == Cell.Type.Number) {
                    int mineCount = CountMines(cell.position.y, cell.position.x);
                    if (mineCount == cell.number) {
                        ShowSafe(cell.position.y, cell.position.x);
                    }
                } else {
                    // TODO: 高亮周围格子
                }
            } else if (cell.flagged) {
                cell.flagged = false;
                _state[cell.position.y, cell.position.x] = cell;
            } else {
                if (cell.type == Cell.Type.Mine) {
                    Explode(cell.position.y, cell.position.x);
                } else {
                    cell.revealed = true;
                    if (cell.type == Cell.Type.Empty) {
                        ShowSafe(cell.position.y, cell.position.x);
                    }
                    _state[cell.position.y, cell.position.x] = cell;
                }
            }
            _board.Draw(_state);
        }
    }

    private void ShowSafe(int centerRow, int centerCol)
    {
        bool skip = false;
        void Show(int sr, int sc) {
            if (skip) return;
            int r = centerRow + sr;
            int c = centerCol + sc;
            if (!(0 <= r && r < _rows && 0 <= c && c < _cols))
                return;
            var cell = _state[r, c];
            if (cell.type == Cell.Type.Mine) {
                return;
            }
            if (!cell.flagged && !cell.revealed) {
                cell.revealed = true;
                _state[r, c] = cell;
                if (cell.type == Cell.Type.Empty) {
                    ShowSafe(r, c);
                }
            }
        }
        Show(-1, -1);
        Show(-1, 0);
        Show(-1, 1);
        Show(0, 1);
        Show(1, 1);
        Show(1, 0);
        Show(1, -1);
        Show(0, -1);
    }

    private void Explode(int centerRow, int centerCol)
    {
        gameOver = true;
        for (int r = 0; r < _rows; r++) {
            for (int c = 0; c < _cols; c++) {
                var cell = _state[r, c];
                if (cell.type == Cell.Type.Mine) {
                    cell.revealed = true;
                    cell.flagged = false;
                    if (r == centerRow && c == centerCol) {
                        cell.exploded = true;
                    }
                    _state[r, c] = cell;
                }
            }
        }
    }

    public void NewGame()
    {
        gameOver = false;
        _state = new Cell[_rows, _cols];
        GenerateCells();
        GenerateMines();
        GenerateNumbers();
        Camera.main.transform.position = new Vector3(_cols / 2, _rows / 2, -10);
        _board.Draw(_state);
    }

    private void GenerateCells()
    {
        for (int r = 0; r < _rows; r++) {
            for (int c = 0; c < _cols; c++) {
                Cell cell = new()
                {
                    position = new Vector3Int(c, r, 0),
                    type = Cell.Type.Empty,
                };
                _state[r, c] = cell;
            }
        }
    }

    private void GenerateMines()
    {
        for (int i = 0; i < _mineCount; i++) {
            int r = Random.Range(0, _rows);
            int c = Random.Range(0, _cols);
             while (_state[r, c].type == Cell.Type.Mine) {
                c++;
                if (c >= _cols) {
                    c = 0;
                    r++;
                    if (r >= _rows) {
                        r = 0;
                    }
                }
             }
            _state[r, c].type = Cell.Type.Mine;
        }
    }

    private void GenerateNumbers()
    {
        for (int r = 0; r < _rows; r++) {
            for (int c = 0; c < _cols; c++) {
                ref Cell cell = ref _state[r, c];
                if (cell.type == Cell.Type.Mine) {
                    continue;
                }
                int mineCount = CountMines(r, c);
                if (mineCount > 0) {
                    cell.type = Cell.Type.Number;
                    cell.number = mineCount;
                }
            }
        }
    }

    private int CountMines(int centerRow, int centerCol)
    {
        int count = 0;
        bool IsMine(int sr, int sc) {
            int r = centerRow + sr;
            int c = centerCol + sc;
            return 0 <= r && r < _rows && 0 <= c && c < _cols && _state[r, c].type == Cell.Type.Mine;
        }
        if (IsMine(-1, -1)) count++;
        if (IsMine(-1, 0)) count++;
        if (IsMine(-1, 1)) count++;
        if (IsMine(0, 1)) count++;
        if (IsMine(1, 1)) count++;
        if (IsMine(1, 0)) count++;
        if (IsMine(1, -1)) count++;
        if (IsMine(0, -1)) count++;
        return count;
    }
    private Cell? GetCellAtMousePosition()
    {
        var worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var cellPosition = _board.Tilemap.WorldToCell(worldPosition);
        return GetCell(cellPosition.y, cellPosition.x);
    }

    private Cell? GetCell(int r, int c)
    {
        if (0 <= r && r < _rows && 0 <= c && c < _cols)
            return _state[r, c];
        return null;
    }
}
