//code by tino.heuberger@students.fhnw.ch
using System.Globalization;
using System.Linq;

class NonoGramSolver
{
    static readonly IEnumerator<string> inputLines = File.ReadLines("samples/nonogram.in").GetEnumerator();
    static string NextLine() { if (!inputLines.MoveNext()) throw new Exception(); return inputLines.Current; }

    static byte numRows;
    static byte numColumns;
    static List<byte>[] rowBlocks;
    static List<byte>[] columnBlocks;
    static long[][] validRowCombinations;

    static void Main()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        using var outp = new StreamWriter("nonogram.out");

        ParseInput();

        // Pre-Processing
        FindValidRowCombinations();
    }

    private static void FindValidRowCombinations()
    {
        // Pre-Calculate all valid block positions in this row by storing the start positions of every block
        // This reduces redudancy in backtracking calulations and makes finish validation easier since 
        // we no longer have to validate the rows
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
        // Is this correct?
        // Determine where the last possible startIndex for this block Is
        // Corresponds to the last possible index where the remaining blocks (including 1 space in between) can still be placed
        var maxStartIndex = numColumns - 1 - rowBlocks.Skip(block).Sum(b => b) - (rowBlocks.Count - block - 2);

        for (int start = startIndex; start < maxStartIndex; startIndex++)
        {
            for (int x = 0; x < rowBlocks[block]; x++)
            {
                curRow ^= (1 << (startIndex + x));
            }

			if (block == rowBlocks.Count - 1) {
					validRows.Add(curRow);
			}
			else {
					FindValidRowCombinationsRecursive(rowBlocks, block + 1, curRow, validRows, start + rowBlocks[block] + 1);
			}
        }
    }

    private static void ParseInput()
    {
        var firstLine = NextLine().Split(" ");
        numColumns = byte.Parse(firstLine[0]);
        numRows = byte.Parse(firstLine[1]);
        rowBlocks = new List<byte>[numRows];
        columnBlocks = new List<byte>[numColumns];
        validRowCombinations = new long[numRows][];

        NextLine(); //One empty line expected
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

        NextLine(); //One empty line expected
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
