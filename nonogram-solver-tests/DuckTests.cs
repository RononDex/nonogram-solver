using System.Numerics;
using static NonoGramSolver;

namespace nonogram_solver_tests;

public class DuckTests
{
    private static Dictionary<int, List<uint>> solution_rows = new Dictionary<int, List<uint>>() {
                {0, new List<uint>(new uint[] {
                        0b000001110,
                        0b000001011,
                        0b011001110,
                        0b011001100,
                        0b011111100,
                        0b001111101,
                        0b000111111,
                        0b000010000,
                        0b000011000,
                 })},
        };

    private static Dictionary<int, List<uint>> solution_cols = new Dictionary<int, List<uint>>() {
                {0, new List<uint>(new uint[] {
                        0b001100010,
                        0b001000111,
                        0b001111101,
                        0b101111111,
                        0b111110000,
                        0b001110000,
                        0b000111100,
                        0b000011100,
                        0b000000000,
                 })},
        };

    [Test]
    public void Test_Duck_ParsesFileCorrectly()
    {
        File.Copy("samples/duck.in", "nonogram.in", true);

        NonoGramSolver.ParseInput();

        Assert.AreEqual(NonoGramSolver.numColumns, 9);
        Assert.AreEqual(NonoGramSolver.numRows, 9);
        Assert.AreEqual(NonoGramSolver.rowBlocks.Count, 9);
        Assert.AreEqual(NonoGramSolver.columnBlocks.Count, 9);
        Assert.AreEqual(NonoGramSolver.rowBlocks[0], new HashSet<byte>(new byte[] { 3 }));
        Assert.AreEqual(NonoGramSolver.rowBlocks[1], new HashSet<byte>(new byte[] { 2, 1 }));
        Assert.AreEqual(NonoGramSolver.rowBlocks[2], new HashSet<byte>(new byte[] { 3, 2 }));
        Assert.AreEqual(NonoGramSolver.rowBlocks[3], new HashSet<byte>(new byte[] { 2, 2 }));
        Assert.AreEqual(NonoGramSolver.rowBlocks[4], new HashSet<byte>(new byte[] { 6 }));
        Assert.AreEqual(NonoGramSolver.rowBlocks[5], new HashSet<byte>(new byte[] { 1, 5 }));
        Assert.AreEqual(NonoGramSolver.rowBlocks[6], new HashSet<byte>(new byte[] { 6 }));
        Assert.AreEqual(NonoGramSolver.rowBlocks[7], new HashSet<byte>(new byte[] { 1 }));
        Assert.AreEqual(NonoGramSolver.rowBlocks[8], new HashSet<byte>(new byte[] { 2 }));
        Assert.AreEqual(NonoGramSolver.columnBlocks[0], new HashSet<byte>(new byte[] { 1, 2 }));
        Assert.AreEqual(NonoGramSolver.columnBlocks[1], new HashSet<byte>(new byte[] { 3, 1 }));
        Assert.AreEqual(NonoGramSolver.columnBlocks[2], new HashSet<byte>(new byte[] { 1, 5 }));
        Assert.AreEqual(NonoGramSolver.columnBlocks[3], new HashSet<byte>(new byte[] { 7, 1 }));
        Assert.AreEqual(NonoGramSolver.columnBlocks[4], new HashSet<byte>(new byte[] { 5 }));
        Assert.AreEqual(NonoGramSolver.columnBlocks[5], new HashSet<byte>(new byte[] { 3 }));
        Assert.AreEqual(NonoGramSolver.columnBlocks[6], new HashSet<byte>(new byte[] { 4 }));
        Assert.AreEqual(NonoGramSolver.columnBlocks[7], new HashSet<byte>(new byte[] { 3 }));
        Assert.AreEqual(NonoGramSolver.columnBlocks[8], new HashSet<byte>(new byte[] { }));
    }

    [Test]
    public void Test_DuckFindValidCombinations_StillContainsValidSolution()
    {
        File.Copy("samples/duck.in", "nonogram.in", true);
        NonoGramSolver.ParseInput();


        var solver = new GenericNonoGramSolver<uint>(NonoGramSolver.numRows, NonoGramSolver.numColumns, NonoGramSolver.rowBlocks, NonoGramSolver.columnBlocks);
        solver.FindValidCombinations();

        VerifySolutionIsStillInValidCombinations(solver);

        for (var i = 0; i < 10; i++)
        {
            solver.FilterImpossibleCombinations(NonoGramSolver.numRows, solver.validRowCombinations, solver.numColumns, solver.validColumnCombinations);

            VerifySolutionIsStillInValidCombinations(solver);
        }

        solver.FindSolutions();

        Assert.AreEqual(1, solver.solutions.Count);
        for (var row = 0; row < solution_rows.Count; row++)
        {
            Assert.AreEqual(solution_rows[0][row], solver.solutions[0][row]);
        }
    }

    private static void VerifySolutionIsStillInValidCombinations(GenericNonoGramSolver<uint> solver)
    {
        for (var row = 0; row < solution_rows[0].Count; row++)
        {
            Assert.True(solver.validRowCombinations[row].Any(r => r.Equals(solution_rows[0][row])));
        }

        for (var col = 0; col < solution_cols[0].Count; col++)
        {
            Assert.True(solver.validColumnCombinations[col].Any(r => r.Equals(solution_cols[0][col])));
        }
    }
}
