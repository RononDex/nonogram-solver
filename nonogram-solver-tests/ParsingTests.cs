namespace nonogram_solver_tests;

public class ParsingTests
{
    [Test]
    public void Test_Duck_ParsesFileCorrectly()
    {
        File.Move("samples/duck.in", "nonogram.in", true);

		NonoGramSolver.ParseInput();

		Assert.AreEqual(NonoGramSolver.numColumns, 9);
		Assert.AreEqual(NonoGramSolver.numRows, 9);
		Assert.AreEqual(NonoGramSolver.rowBlocks.Length, 9);
		Assert.AreEqual(NonoGramSolver.columnBlocks.Length, 9);
		Assert.AreEqual(NonoGramSolver.rowBlocks[0], new byte[] { 3 });
		Assert.AreEqual(NonoGramSolver.rowBlocks[1], new byte[] { 2, 1 });
		Assert.AreEqual(NonoGramSolver.rowBlocks[2], new byte[] { 3, 2 });
		Assert.AreEqual(NonoGramSolver.rowBlocks[3], new byte[] { 2, 2 });
		Assert.AreEqual(NonoGramSolver.rowBlocks[4], new byte[] { 6 });
		Assert.AreEqual(NonoGramSolver.rowBlocks[5], new byte[] { 1, 5 });
		Assert.AreEqual(NonoGramSolver.rowBlocks[6], new byte[] { 6 });
		Assert.AreEqual(NonoGramSolver.rowBlocks[7], new byte[] { 1 });
		Assert.AreEqual(NonoGramSolver.rowBlocks[8], new byte[] { 2 });
		Assert.AreEqual(NonoGramSolver.columnBlocks[0], new byte[] { 1, 2 });
		Assert.AreEqual(NonoGramSolver.columnBlocks[1], new byte[] { 3, 1 });
		Assert.AreEqual(NonoGramSolver.columnBlocks[2], new byte[] { 1, 5 });
		Assert.AreEqual(NonoGramSolver.columnBlocks[3], new byte[] { 7, 1 });
		Assert.AreEqual(NonoGramSolver.columnBlocks[4], new byte[] { 5 });
		Assert.AreEqual(NonoGramSolver.columnBlocks[5], new byte[] { 3 });
		Assert.AreEqual(NonoGramSolver.columnBlocks[6], new byte[] { 4 });
		Assert.AreEqual(NonoGramSolver.columnBlocks[7], new byte[] { 3 });
		Assert.AreEqual(NonoGramSolver.columnBlocks[8], new byte[] { });
    }
}
