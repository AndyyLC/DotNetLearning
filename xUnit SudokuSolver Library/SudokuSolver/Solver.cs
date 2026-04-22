using System.Data;
using System.Drawing;

namespace SudokuSolver;

public class Solver
{
    private readonly IBoard _board;
    private List<(int zeroRow, int zeroCol)> zeroSpots = new();

    public Solver(IBoard board)
    {
        _board = board;
    }

    public bool IsValid()
    {
        int size = _board.Size;
        var usedSet = new HashSet<int>();
        for (int row = 0; row < size; row++)
        {
            usedSet.Clear();
            for (int col = 0; col < size; col++)
            {
                int num = _board[row, col]; //num is equal to the current value when looking through the board
                if (num == 0) //0 is considered an empty value and will skip over it
                {
                    continue;
                }
                if (usedSet.Contains(num)) //if set already contains value then it means the number is already in the row, meaning we duplicate numbers in a row 
                {
                    return false;
                }
                usedSet.Add(num); //add number to set
            }
        }

        for (int col = 0; col < size; col++)
        {
            usedSet.Clear();
            for (int row = 0; row < size; row++)
            {
                int num = _board[row, col];
                if (num == 0)
                {
                    continue;
                }
                if (usedSet.Contains(num))
                {
                    return false;
                }
                usedSet.Add(num);
            }
        }

        int sqrt = _board.GridSize;
        for (int grid = 0; grid < size; grid++)
        {
            usedSet.Clear();
            int startCol = (grid % sqrt) * sqrt; //Modulus gives 0,1,0,1
            int startRow = (grid / sqrt) * sqrt; //divsion give 0,0,1,1
            for (int cell = 0; cell < size; cell++)
            {
                int col = startCol + (cell % sqrt);
                int row = startRow + (cell / sqrt);
                int num = _board[row, col];
                if (num == 0)
                {
                    continue;
                }
                if (usedSet.Contains(num))
                {
                    return false;
                }
                usedSet.Add(num);
            }
        }
        return true;
    }
    public bool IsSolvable()
    {
        zeroSpots.Clear();
        if (_board == null || !IsValid())
        {
            return false;
        }
        else
        {
            FindZeros(zeroSpots);
            if (zeroSpots.Count != 0)
            {
                return Solve(0);
            }
        }
        return false;
    }

    public bool Solve(int index)
    {
        if (index == zeroSpots.Count)
            return true;
        var (row, col) = zeroSpots[index];
        if (_board[row, col] == 0)
        {
            for (int i = 1; i <= _board.Size; i++)
            {
                _board[row, col] = i;
                if (!IsValid())
                {
                    _board[row, col] = 0;
                    continue;
                }
                if (Solve(index + 1))
                {
                    return true;
                }
                else
                {
                    _board[row, col] = 0;
                }   
            }
            return false;
        }
        return true;
    }

    public void FindZeros(List<(int zeroRow, int zeroCol)> zeroSpots)
    {
        for (int row = 0; row < _board.Size; row++)
        {
            for (int col = 0; col < _board.Size; col++)
            {
                if (_board[row, col] == 0)
                {
                    zeroSpots.Add((row, col));
                }
            }
        }
    }
}
