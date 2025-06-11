using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Range(5, 250)]
    public int mazeWidth = 5, mazeHeight = 5;
    public int startX, startY;
    public int exitX = -1, exitY = -1; // -1 means auto-place at opposite corner
    MazeCell[,] maze;

    Vector2Int currentCell;

    public MazeCell[,] GetMaze()
    {
        maze = new MazeCell[mazeWidth, mazeHeight];

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                maze[x, y] = new MazeCell(x, y);
            }
        }

        // Auto-place exit at opposite corner if not specified
        if (exitX == -1 || exitY == -1)
        {
            exitX = (startX == 0) ? mazeWidth - 1 : 0;
            exitY = (startY == 0) ? mazeHeight - 1 : 0;
        }

        CarvePath(startX, startY);

        // Mark the exit cell
        if (exitX >= 0 && exitX < mazeWidth && exitY >= 0 && exitY < mazeHeight)
        {
            maze[exitX, exitY].isExit = true;
        }

        return maze;
    }

    List<Direction> directions = new List<Direction>
    {
        Direction.Up, Direction.Down, Direction.Left, Direction.Right,
    };

    List<Direction> GetRandomDirections()
    {
        List<Direction> dir = new List<Direction>(directions);

        List<Direction> rndDir = new List<Direction>();

        while (dir.Count > 0)
        {
            int rnd = Random.Range(0, dir.Count);
            rndDir.Add(dir[rnd]);
            dir.RemoveAt(rnd);
        }

        return rndDir;
    }

    bool IsCellValid(int x, int y)
    {
        if (x < 0 || y < 0 || x > mazeWidth - 1 || y > mazeHeight - 1 || maze[x, y].visited)
            return false;
        else return true;
    }

    Vector2Int CheckNeighbors()
    {
        List<Direction> rndDir = GetRandomDirections();
        for (int i = 0; i < rndDir.Count; i++)
        {
            Vector2Int neighbor = currentCell;

            switch (rndDir[i])
            {
                case Direction.Up:
                    neighbor.y++;
                    break;
                case Direction.Down:
                    neighbor.y--;
                    break;
                case Direction.Right:
                    neighbor.x++;
                    break;
                case Direction.Left:
                    neighbor.x--;
                    break;
            }
            if (IsCellValid(neighbor.x, neighbor.y)) return neighbor;
        }
        return currentCell;
    }

    void BreakWalls(Vector2Int primaryCell, Vector2Int secondaryCell)
    {
        if (primaryCell.x > secondaryCell.x)
        {
            maze[primaryCell.x, primaryCell.y].leftWall = false;
        }
        else if (primaryCell.x < secondaryCell.x)
        {
            maze[secondaryCell.x, secondaryCell.y].leftWall = false;
        }
        else if (primaryCell.y < secondaryCell.y)
        {
            maze[primaryCell.x, primaryCell.y].topWall = false;
        }
        else if (primaryCell.y > secondaryCell.y)
        {
            maze[secondaryCell.x, secondaryCell.y].topWall = false;
        }
    }

    void CarvePath(int x, int y)
    {
        if (x < 0 || y < 0 || x > mazeWidth - 1 || y > mazeHeight - 1)
        {
            x = y = 0;
            Debug.Log("Invalid starting positions");
        }

        currentCell = new Vector2Int(x, y);

        List<Vector2Int> path = new List<Vector2Int>();

        bool deadEnd = false;
        while (!deadEnd)
        {
            Vector2Int nextCell = CheckNeighbors();

            if (nextCell == currentCell)
            {
                for (int i = path.Count - 1; i >= 0; i--)
                {
                    currentCell = path[i];
                    path.RemoveAt(i);
                    nextCell = CheckNeighbors();

                    if (nextCell != currentCell) break;
                }

                if (nextCell == currentCell) deadEnd = true;
            }
            else
            {
                BreakWalls(currentCell, nextCell);
                maze[currentCell.x, currentCell.y].visited = true;
                currentCell = nextCell;
                path.Add(currentCell);
            }
        }
    }
}

public enum Direction
{
    Up, Down, Left, Right,
}

public class MazeCell
{
    public bool visited;
    public int x, y;

    public bool topWall;
    public bool leftWall;
    public bool isExit;     // Mark exit cells

    public Vector2Int position
    {
        get
        {
            return new Vector2Int(x, y);
        }
    }

    public MazeCell(int x, int y)
    {
        this.x = x;
        this.y = y;

        visited = false;
        isExit = false;

        topWall = leftWall = true;
    }
}