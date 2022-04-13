using UnityEngine;

public class Game : MonoBehaviour
{
    public static int width = 16;
    public static int height = 16;
    public static int mineCount = 32;

    [SerializeField] private GameObject winText;
    [SerializeField] private GameObject lostText;
    [SerializeField] private GameObject buttons;
    private Board board;
    private Cell[,] state;
    private bool gameover;

    private void Awake()
    {
        board = FindObjectOfType<Board>();
        Camera.main.orthographicSize = width > height ? width / 2 + 1 : height / 2 + 1;
    }

    private void Start()
    {
        NewGame();
    }

    private void Update()
    {
        if (!gameover)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Flag();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Reveal();
            }
        }
    }

    private void NewGame()
    {
        //2 dimensional array
        state = new Cell[width, height];
        gameover = false;

        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        //Offset the camera correctly, so that the board can be seen
        Camera.main.transform.position = new Vector3(width / 2, height / 2, -10);

        board.Draw(state);
    }

    private void GenerateCells()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                state[x, y] = cell;
            }
        }
    }

    private void GenerateMines()
    {
        for(int i = 0; i < mineCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            //Check, if current cell already is a mine, if it is one, move to the next cell
            while(state[x, y].type == Cell.Type.Mine)
            {
                x++;

                //Go to the next row, when reaching the edge of the board
                if(x >= width)
                {
                    x = 0;
                    y++;

                    //Go to the bottom of the board, when reaching the top
                    if(y >= height)
                    {
                        y = 0;
                    }
                }
            }

            state[x, y].type = Cell.Type.Mine;
        }
    }

    private void GenerateNumbers()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                //Skip this cell, if it is a mine...
                if(cell.type == Cell.Type.Mine)
                {
                    continue;
                }

                //...otherwise print the number of mines, that surround the cell on it
                cell.number = CountMines(x, y);
                if(cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }

                state[x, y] = cell;
            }
        }
    }

    private int CountMines(int cellX, int cellY)
    {
        int count = 0;

        for(int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for(int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if(adjacentX == 0 && adjacentY == 0)
                {
                    continue;
                }

                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if(GetCell(x, y).type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void Flag()
    {
        //Get the clicked cell
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed) return;

        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);
    }

    private void Reveal()
    {
        //Get the clicked cell
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged) return;

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Explode(cell);
                break;
            case Cell.Type.Empty:
                Flood(cell);
                CheckWinCondition();
                break;
            default:
                cell.revealed = true;
                state[cellPosition.x, cellPosition.y] = cell;
                CheckWinCondition();
                break;
        }
        
        board.Draw(state);
    }

    private void Flood(Cell cell)
    {
        if (cell.revealed) return;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;

        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell;

        if(cell.type == Cell.Type.Empty)
        {
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x + 1, cell.position.y));
            Flood(GetCell(cell.position.x, cell.position.y + 1));
            Flood(GetCell(cell.position.x, cell.position.y - 1));
        }
    }

    private void Explode(Cell cell)
    {
        gameover = true;

        cell.revealed = true;
        cell.exploded = true;
        state[cell.position.x, cell.position.y] = cell;

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                cell = state[x, y];

                if(cell.type == Cell.Type.Mine)
                {
                    cell.revealed = true;
                    state[x, y] = cell;
                }
            }
        }

        lostText.SetActive(true);
        buttons.SetActive(true);
    }

    private void CheckWinCondition()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if(cell.type != Cell.Type.Mine && !cell.revealed)
                {
                    return;
                }
            }
        }

        gameover = true;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    cell.flagged = true;
                    state[x, y] = cell;
                }
            }
        }

        winText.SetActive(true);
        buttons.SetActive(true);
    }

    private Cell GetCell(int x, int y)
    {
        if(IsValidCell(x, y))
        {
            return state[x, y];
        }
        else
        {
            return new Cell();
        }
    }

    private bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
