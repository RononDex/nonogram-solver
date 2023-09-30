//code by tino.heuberger@students.fhnw.ch
using System.Globalization;
using System.Linq;

public class NonoGramSolver
{
	static IEnumerator<string> inputLines = File.ReadLines("nonogram.in").GetEnumerator();
	static string NextLine() { if (!inputLines.MoveNext()) throw new Exception(); return inputLines.Current; }

	public static byte numRows;
	public static byte numColumns;
	public static List<byte>[]? rowBlocks;
	public static List<byte>[]? columnBlocks;
	public static long[][]? validRowCombinations;
	public static long[][]? validColumnCombinations;
	public static List<long[]> solutions;

	public static void Main()
	{
		inputLines = File.ReadLines("nonogram.in").GetEnumerator();
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		solutions = new();
		using var outp = new StreamWriter("nonogram.out");

		// Parsing
		ParseInput();

		// Pre-Processing
		FindValidDimensionCombinations(numRows, ref validRowCombinations!, rowBlocks!, numColumns);
		FindValidDimensionCombinations(numColumns, ref validColumnCombinations!, columnBlocks!, numRows);

		// Solving
		FindSolutions();
		if (solutions.Count == 0)
		{
			Console.WriteLine("No solution found");
			return;
		}
	}

	private static void FindSolutions()
	{
		var emptyBoardRows = new long[numRows];
		var emptyBoardColumns = new long[numColumns];
		FindSolutionRecursive(0, emptyBoardRows.AsSpan(), emptyBoardColumns.AsSpan());
	}

	private static void FindSolutionRecursive(int rowIndex, Span<long> boardRows, Span<long> boardColumns)
	{
		var isLastRow = rowIndex == numRows - 1;
		for (var combinationIndex = 0; combinationIndex < validRowCombinations![rowIndex].Length; combinationIndex++)
		{
			// Pick a valid row combination to try
			boardRows[rowIndex] = validRowCombinations[rowIndex][combinationIndex];
			UpdateColumnBoard(boardColumns, ref boardRows[rowIndex], ref rowIndex);

			var hasValidColumns = IsValidPartialSolution(boardColumns, ref rowIndex);

			if (hasValidColumns && isLastRow)
			{
				solutions.Add(boardRows.ToArray());
			}
			else if (hasValidColumns && !isLastRow)
			{
				FindSolutionRecursive(rowIndex + 1, boardRows, boardColumns);
			}
		}
	}
	/// <summary>
	/// Validate if the current calculated columns are still valid according to
	/// the precalculated valid column combinations
	/// </summary>
	private static bool IsValidPartialSolution(Span<long> boardColumns, ref int rowIndex)
	{
		long relevantRowsBitMask = 0;
		for (var row = 0; row <= rowIndex; row++)
		{
			relevantRowsBitMask |= (long)1 << row;
		}
		for (var columnIndex = 0; columnIndex < numColumns; columnIndex++)
		{
			bool foundMatch = false;
			for (var validColumnIndex = 0;
				validColumnIndex < validColumnCombinations[columnIndex].Length && !foundMatch;
				validColumnIndex++)
			{
				if ((boardColumns[columnIndex] & relevantRowsBitMask)
								== (validColumnCombinations[columnIndex][validColumnIndex] & relevantRowsBitMask))
				{
					foundMatch = true;
				}
			}

			if (!foundMatch)
			{
				return false;
			}
		}

		return true;
	}

	private static void UpdateColumnBoard(Span<long> boardColumns, ref long newlyChosenRow, ref int rowIndex)
	{
		for (var column = 0; column < numColumns; column++)
		{
			// set bit to 1 on column if row has bit set
			if ((newlyChosenRow & ((long)1 << column)) > 0)
			{
				boardColumns[column] |= ((long)1 << rowIndex);
			}
			else
			{
				// reset value to 0
				boardColumns[column] &= ~((long)1 << rowIndex);
			}
		}
	}

	/// <summary>
	/// Pre-Calculate all valid block positions in this dimension by storing the start positions of every block
	/// This reduces redudancy in backtracking calulations and makes finish validation easier since
	/// we no longer have to validate this dimension
	/// </summary>
	public static void FindValidDimensionCombinations(int numElements, ref long[][] validArrays, List<byte>[] blocks, int otherDimensionLength)
	{
		for (var index = 0; index < numElements; index++)
		{
			if (blocks[index].Count == 0)
			{
				validArrays[index] = new long[] { 0L };
			}
			else
			{
				List<long> validList = new List<long>();
				FindValidDimensionCombinationsRecursive(blocks[index], 0, 0, validList, 0, otherDimensionLength);
				validArrays[index] = validList.ToArray();
			}
		}
	}

	private static void FindValidDimensionCombinationsRecursive(
					List<byte> blocks,
					int blockIndex,
					long curElement,
					List<long> validList,
					int startIndex,
					int otherDimensionLength)
	{
		var blockLength = blocks[blockIndex];
		// Determine where the last possible startIndex for this block Is
		// Corresponds to the last possible index where the remaining blocks (including 1 space in between) can still be placed
		var maxStartIndex =
				otherDimensionLength
				- blocks.Skip(blockIndex + 1).Sum(b => b + 1) // +1 since we need at least one empty space in between each block
				- startIndex
				- blockLength
				+ startIndex;

		for (int start = startIndex; start <= maxStartIndex; start++)
		{
			// set bits for current bar to 1
			for (int x = 0; x < blocks[blockIndex]; x++)
			{
				curElement |= (long)1 << (start + x);
			}

			if (blockIndex == blocks.Count - 1)
			{
				validList.Add(curElement);
			}
			else
			{
				FindValidDimensionCombinationsRecursive(
								blocks,
								blockIndex + 1,
								curElement,
								validList,
								start + blocks[blockIndex] + 1, otherDimensionLength);
			}

			// reset all bits from current block onwards to 0
			long eraseBitMask = 0;
			for (var x = 0; x <= blockLength; x++)
			{
				eraseBitMask |= (long)1 << (x + start);
			}
			curElement &= ~eraseBitMask;
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
		validColumnCombinations = new long[numColumns][];

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
