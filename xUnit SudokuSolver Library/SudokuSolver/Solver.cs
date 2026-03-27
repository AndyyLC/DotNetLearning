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
        return true;
    }
}
