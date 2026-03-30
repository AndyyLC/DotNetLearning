using System.Data;
using System.Drawing;

namespace SudokuSolver;

public class Solver
{
    private readonly IBoard _board;

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
}
