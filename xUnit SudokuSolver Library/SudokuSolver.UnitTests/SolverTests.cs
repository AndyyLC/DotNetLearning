using System.Reflection;
using SudokuSolver;

namespace SudokuSolver.UnitTests;

public class SolverTests
{
    [Fact]
    public void Empty4x4Board()
    {
        int[,] emtpy = new int[4, 4];
        var solver = new Solver(emtpy);
        Assert.True(solver.IsValid());
    }

    [Fact]
    public void NonSquareBoard()
    {
        int[,] empty = new int[4, 9];
        var solver = new Solver(empty);
        Assert.False(solver.IsValid());
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(4, true)]
    [InlineData(8, false)]
    [InlineData(9, true)]
    [InlineData(10, false)]
    [InlineData(16, true)]
    public void EmpyBoardSizes(int size, bool IsValid)
    {
        int[,] empty = new int[size, size];
        var solver = new Solver(empty);
        Assert.Equal(IsValid, solver.IsValid());
    }
}
