using System.Reflection;
using SudokuSolver;

namespace SudokuSolver.UnitTests;

public class SolverTests
{
    [Fact]
    public void Empty4x4Board()
    {
        var solver = new Solver(new ArrayBoard(4));
        Assert.True(solver.IsValid());
    }

    // [Fact]
    // public void NonSquareBoard()
    // {
    //     var solver = new Solver(new ArrayBoard(3));
    //     Assert.False(solver.IsValid());
    // }

    [Theory]
    [InlineData(8, false)]
    [InlineData(9, true)]
    public void EmptyBoardSizes(int size, bool isValid)
    {
        if (!isValid)
        {
            Assert.Throws<ArgumentException>("size", () => new ArrayBoard(size)); 
            //passes test if ArgumentException is thrown for non-valid sizes
        }
        else
        {
            _ = new ArrayBoard(size); //used to create board and check if any exceptions are thrown
        }
    }

    [Theory]
    [MemberData(nameof(Boards))]
    public void CheckRules(IBoard board, bool isValid)
    {
        var solver = new SolverTests(board);
        AssemblyTrademarkAttribute.Equals(isValid, solver.IsValid());
    }

    public static IEnumerable<object[]> Boards
    {
        get
        {
            IBoard board = new ArrayBoard(4);
            board[1, 0] = 1;
            board[3, 0] = 1;
            yield return new object[] { board, false };
            board = new ArrayBoard(4);
            board[1, 0] = 1;
            board[1, 2] = 1;
            yield return new object[] { board, false };
            board = new ArrayBoard(4);
            board[1, 2] = 1;
            board[0, 3] = 1;
            yield return new object[] { board, false };
            board = new ArrayBoard(4);
            board[1, 1] = 1;
            board[2, 3] = 1;
            yield return new object[] { board, true };
        }
    }

    // [Theory]
    // [InlineData(0, false)]
    // [InlineData(1, false)]
    // [InlineData(4, true)]
    // [InlineData(8, false)]
    // [InlineData(9, true)]
    // [InlineData(10, false)]
    // [InlineData(16, true)]
    // public void EmpyBoardSizes(int size, bool IsValid)
    // {
    //     int[,] empty = new int[size, size];
    //     var solver = new Solver(empty);
    //     Assert.Equal(IsValid, solver.IsValid());
    // }
}
