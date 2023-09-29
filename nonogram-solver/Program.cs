//code by tino.heuberger@students.fhnw.ch
using System.Globalization;
using System.Linq;

public class NonoGramSolver
{
    static readonly IEnumerator<string> inputLines = File.ReadLines("nonogram.in").GetEnumerator();
    static string NextLine() { if (!inputLines.MoveNext()) throw new Exception(); return inputLines.Current; }

    public static byte numRows;
    public static byte numColumns;
    public static List<byte>[] rowBlocks;
    public static List<byte>[] columnBlocks;
    public static long[][] validRowCombinations;

    public static void Main()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        using var outp = new StreamWriter("nonogram.out");

        ParseInput();

        // Pre-Processing
        FindValidRowCombinations();

        FindSolution();
    }

    private static void FindSolution()
    {
        var board = new long[numRows];
		FindSolutionRecursive(0, 0, board.AsSpan());
    }

    private static void FindSolutionRecursive(int rowIndex, int combinationIndex, Span<long> currentBoard)
    {
    }

    /// <summary>
    /// Pre-Calculate all valid block positions in this row by storing the start positions of every block
    /// This reduces redudancy in backtracking calulations and makes finish validation easier since
    /// we no longer have to validate the rows
    /// </summary>
    public static void FindValidRowCombinations()
    {
        for (int row = 0; row < numRows; row++)
        {
            if (rowBlocks[row].Count == 0)
            {
                validRowCombinations[row] = new long[] { 0L };
            }
            else
            {
                List<long> validRows = new List<long>();
                FindValidRowCombinationsRecursive(rowBlocks[row], 0, 0, validRows, 0);
                validRowCombinations[row] = validRows.ToArray();
            }
        }
    }

    private static void FindValidRowCombinationsRecursive(
                    List<byte> rowBlocks,
                    int block,
                    long curRow,
                    List<long> validRows,
                    int startIndex)
    {
        // Determine where the last possible startIndex for this block Is
        // Corresponds to the last possible index where the remaining blocks (including 1 space in between) can still be placed
        var maxStartIndex =
                numColumns
                - rowBlocks.Skip(block + 1).Sum(b => b + 1) // +1 since we need at least one empty space in between each block
                - startIndex
                - rowBlocks[block]
                + startIndex;

        for (int start = startIndex; start <= maxStartIndex; start++)
        {
            // set bits for current bar to 1
            for (int x = 0; x < rowBlocks[block]; x++)
            {
                curRow ^= (1 << (start + x));
            }

            if (block == rowBlocks.Count - 1)
            {
                validRows.Add(curRow);
            }
            else
            {
                FindValidRowCombinationsRecursive(rowBlocks, block + 1, curRow, validRows, start + rowBlocks[block] + 1);
            }

            // reset all bits from current block onwards to 0
            long eraseBitMask = 0;
            for (var x = 0; x <= startIndex; x++)
            {
                eraseBitMask ^= (1 << x);
            }
            curRow &= eraseBitMask;
        }
    }

    /// <summary>
    /// Parses the input files and fills static instance variables (numColumns, numRows, rowBlocks, columnBlocks)
    /// </summary>
    public static void ParseInput()
    {
        var firstLine = NextLine().Split(" ");
        numColumns = byte.Parse(firstLine[0]);
        numRows = byte.Parse(firstLine[1]);
        rowBlocks = new List<byte>[numRows];
        columnBlocks = new List<byte>[numColumns];
        validRowCombinations = new long[numRows][];

        for (int row = 0; row < numRows; row++)
        {
            var rowLine = NextLine().Split(" ");
            rowBlocks[row] = new List<byte>();

            for (var block = 0; block < rowLine.Length; block++)
            {
                if (rowLine[block].Length > 0)
                    rowBlocks[row].Add(byte.Parse(rowLine[block]));
            }
        }

        for (int column = 0; column < numColumns; column++)
        {
            var columnLine = NextLine().Split(" ");
            columnBlocks[column] = new List<byte>();

            for (var block = 0; block < columnLine.Length; block++)
            {
                if (columnLine[block].Length > 0)
                    columnBlocks[column].Add(byte.Parse(columnLine[block]));
            }
        }
    }
}
