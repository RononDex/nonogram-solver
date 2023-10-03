//code by tino.heuberger@students.fhnw.ch
using System.Globalization;
using System.Numerics;

public class NonoGramSolver
{
	static IEnumerator<string> inputLines;
	static string NextLine() { if (!inputLines.MoveNext()) throw new Exception(); return inputLines.Current; }

	public static ushort numRows;
	public static ushort numColumns;
	public static Dictionary<int, List<ushort>>? rowBlocks;
	public static Dictionary<int, List<ushort>>? columnBlocks;
	public static Dictionary<int, HashSet<Int128>>? validRowCombinations;
	public static Dictionary<int, HashSet<Int128>>? validColumnCombinations;
	public static Int128[][] validColumnCombinationsFinal;
	public static Int128[][] validRowCombinationsFinal;
	public static Dictionary<int, List<Int128>>? solutions;

	public static Int128 rowBitMask;
	public static Int128 columnBitMask;

	public static void Main()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

		// Parsing
		ParseInput();

		// Pre-Processing
		FindValidCombinations();
		int numberOfIterations = (int)MathF.Sqrt(numColumns * numRows);
		for (var i = 0; i < numberOfIterations; i++)
		{
			FilterImpossibleCombinations(numRows, validRowCombinations, numColumns, validColumnCombinations);
		}

		// Store valid column and row combinations in arrays now, making iterating over them much much faster
		validColumnCombinationsFinal = new Int128[numColumns][];
		for (var i = 0; i < numColumns; i++)
		{
			validColumnCombinationsFinal[i] = new Int128[validColumnCombinations[i].Count];
			validColumnCombinations[i].CopyTo(validColumnCombinationsFinal[i]);
		}
		validRowCombinationsFinal = new Int128[numRows][];
		for (var i = 0; i < numRows; i++)
		{
			validRowCombinationsFinal[i] = new Int128[validRowCombinations[i].Count];
			validRowCombinations[i].CopyTo(validRowCombinationsFinal[i]);
		}

		// Solving
		FindSolutions();

#if DEBUG
		Console.WriteLine($"Found {solutions.Count} solutions");
#endif
		if (solutions.Count == 0)
		{
			return;
		}

		// Output
		OutputSolutions();
	}

	private static void OutputSolutions()
	{
		Int128[] bitMasks = new Int128[numColumns];
		for (var i = 0; i < numColumns; i++)
		{
			bitMasks[i] = Int128.Zero;
			bitMasks[i] |= Int128.One << i;
		}
		using (var outp = new StreamWriter("nonogram.out"))
		{
			for (ushort solution = 0; solution < solutions.Count; solution++)
			{
				for (ushort row = 0; row < solutions[solution].Count; row++)
				{
					var chars = new char[numColumns];
					for (var x = 0; x < numColumns; x++)
					{
						chars[x] = (solutions[solution][row] & bitMasks[x]) > 0 ? '#' : '.';
					}

					outp.Write(chars);
				}
				outp.Write("\r\n");
			}
		}
	}

	public static void FilterImpossibleCombinations(int numXAxis, Dictionary<int, HashSet<Int128>> validXCombinations, int numYAxis, Dictionary<int, HashSet<Int128>> validYCombinations)
	{
		var knownOnesX = new Int128[numXAxis];
		var knownZerosX = new Int128[numXAxis];
		for (var i = 0; i < numXAxis; i++)
		{
			knownOnesX[i] = rowBitMask;
			knownZerosX[i] = rowBitMask;

			foreach (var validCombination in validXCombinations[i])
			{
				knownOnesX[i] &= validCombination;
				knownZerosX[i] &= ~validCombination;
			}

			knownZerosX[i] &= rowBitMask;
		}
		var knownOnesY = new Int128[numYAxis];
		var knownZerosY = new Int128[numYAxis];
		for (var i = 0; i < numYAxis; i++)
		{
			knownOnesY[i] = columnBitMask;
			knownZerosY[i] = columnBitMask;

			foreach (var validCombination in validYCombinations[i])
			{
				knownOnesY[i] &= validCombination;
				knownZerosY[i] &= ~validCombination;
			}
			knownZerosY[i] &= columnBitMask;
		}
		// var knownOnesX = validXCombinations.Select(e => e.Value.Aggregate(rowBitMask, (x, y) => x & y)).ToArray();
		// var knownZerosX = validXCombinations.Select(e => e.Value.Select(e => (~e) & rowBitMask).Aggregate(rowBitMask, (x, y) => x & y)).ToArray();
		// var knownOnesY = validYCombinations.Select(e => e.Value.Aggregate(columnBitMask, (x, y) => x & y)).ToArray();
		// var knownZerosY = validYCombinations.Select(e => e.Value.Select(e => (~e) & columnBitMask).Aggregate(columnBitMask, (x, y) => x & y)).ToArray();

		ushort smallerDimensionIndex = numColumns < numRows ? numColumns : numRows;
		smallerDimensionIndex--;

		SyncKnownFields(knownOnesX, knownZerosX, knownZerosY, knownOnesY, ref smallerDimensionIndex);

		for (ushort x = 0; x < numXAxis; x++)
		{
			validRowCombinations[x].RemoveWhere(e => (~e & knownOnesX[x]) != 0);
			validRowCombinations[x].RemoveWhere(e => (e & knownZerosX[x]) != 0);
		}
		for (ushort y = 0; y < numYAxis; y++)
		{
			validColumnCombinations[y].RemoveWhere(e => (~e & knownOnesY[y]) != 0);
			validColumnCombinations[y].RemoveWhere(e => (e & knownZerosY[y]) != 0);
		}
	}

	public static void FindSolutions()
	{
		var emptyBoardRows = new Int128[numRows];
		var emptyBoardColumns = new Int128[numColumns];
		var currentColumnStartIndices = new int[numColumns];
		FindSolutionRecursive(0, emptyBoardRows.AsSpan(), emptyBoardColumns.AsSpan(), currentColumnStartIndices);
	}

	private static void FindSolutionRecursive(int rowIndex, Span<Int128> boardRows, Span<Int128> boardColumns, int[] currentColumnStartIndices)
	{
		var isLastRow = rowIndex == numRows - 1;
		for (int combinationIndex = 0; combinationIndex < validRowCombinationsFinal![rowIndex].Length; combinationIndex++)
		{
			var copyOfCurrentColumnStartIndices = new int[currentColumnStartIndices.Length];
			Array.Copy(currentColumnStartIndices, copyOfCurrentColumnStartIndices, currentColumnStartIndices.Length);
			var combination = validRowCombinationsFinal[rowIndex][combinationIndex];
			Int128? previousCombination = null;
			if (combinationIndex > 0)
			{
				previousCombination = validRowCombinationsFinal[rowIndex][combinationIndex - 1];
			}
			// Pick a valid row combination to try
			boardRows[rowIndex] = combination;
			UpdateColumnBoard(boardColumns, ref boardRows[rowIndex], ref rowIndex);

			var hasValidColumns = IsValidPartialSolution(boardColumns, boardRows, ref rowIndex, copyOfCurrentColumnStartIndices, previousCombination);

			if (hasValidColumns && isLastRow)
			{
				solutions.Add((ushort)solutions.Count, new List<Int128>(boardRows.ToArray()));
			}
			else if (hasValidColumns && !isLastRow)
			{
				FindSolutionRecursive(rowIndex + 1, boardRows, boardColumns, copyOfCurrentColumnStartIndices);
			}
		}
	}
	/// <summary>
	/// Validate if the current calculated columns are still valid according to
	/// the precalculated valid column combinations
	/// </summary>
	private static bool IsValidPartialSolution(Span<Int128> boardColumns, Span<Int128> boardRows, ref int rowIndex, int[] currentColumnStartIndices, Int128? previousCombination)
	{
		var relevantRowsBitMask = Int128.Zero;
		for (var row = 0; row <= rowIndex; row++)
		{
			relevantRowsBitMask |= Int128.One << row;
		}

		var differencesFromLastRowCombination = ~(Int128.Zero);
		if (previousCombination != null)
		{
			differencesFromLastRowCombination = boardRows[rowIndex] ^ previousCombination.Value;
		}

		for (ushort columnIndex = 0; columnIndex < numColumns; columnIndex++)
		{
			// Only validate if something changed in this column
			/* if ((differencesFromLastRowCombination & (Int128.One << columnIndex)) != 0) */
			/* { */
				bool foundMatch = false;
				for (var validColumnIndex = currentColumnStartIndices[columnIndex]; validColumnIndex < validColumnCombinationsFinal[columnIndex].Length; validColumnIndex++)
				{
					var validColumn = validColumnCombinationsFinal[columnIndex][validColumnIndex];
					if ((boardColumns[columnIndex] & relevantRowsBitMask)
									== (validColumn & relevantRowsBitMask))
					{
						foundMatch = true;

						// update currentColumnStartIndices so we don't have to check the previous columns again
						// as those will all fail for future checks
						currentColumnStartIndices[columnIndex] = validColumnIndex;
						break;
					}
				/* } */

				if (!foundMatch)
				{
					return false;
				}
			}
		}

		return true;
	}

	private static void UpdateColumnBoard(Span<Int128> boardColumns, ref Int128 newlyChosenRow, ref int rowIndex)
	{
		for (ushort column = 0; column < numColumns; column++)
		{
			// set bit to 1 on column if row has bit set
			if ((newlyChosenRow & (Int128.One << column)) != 0)
			{
				boardColumns[column] |= Int128.One << rowIndex;
			}
			else
			{
				// reset value to 0
				boardColumns[column] &= ~(Int128.One << rowIndex);
			}
		}
	}

	/// <summary>
	/// Pre-Calculate all valid block positions in this dimension by storing the start positions of every block
	/// This reduces redudancy in backtracking calulations and makes finish validation easier since
	/// we no longer have to validate this dimension
	/// </summary>
	public static void FindValidCombinations()
	{
		ushort biggerDimensionSize = numColumns > numRows ? numColumns : numRows;
		ushort smallerDimensionSize = numColumns < numRows ? numColumns : numRows;
		var knownFieldsWithOnesByRows = new Int128[numRows].AsSpan();
		var knownFieldsWithZerosByRows = new Int128[numRows].AsSpan();
		var knownFieldsWithZerosByColumns = new Int128[numColumns].AsSpan();
		var knownFieldsWithOnesByColumns = new Int128[numColumns].AsSpan();

		for (ushort index = 0; index < biggerDimensionSize; index++)
		{
			if (index < numRows)
			{
				// Create initial value with all bits active
				// Known zeros / ones will be marked with an active bit after the recursive call below
				var summedRowsWithOnes = (~Int128.Zero) & rowBitMask;
				var summedRowsWithZeros = ~(Int128.Zero) & rowBitMask;

				HashSet<Int128> validList = new HashSet<Int128>();
				FindValidDimensionCombinationsRecursive(
								rowBlocks[index],
								0,
								0,
								validList,
								0,
								numColumns,
								ref summedRowsWithOnes,
								ref summedRowsWithZeros,
								ref knownFieldsWithOnesByRows[index],
								ref knownFieldsWithZerosByRows[index]);
				validRowCombinations.Add(index, validList);

				knownFieldsWithOnesByRows[index] = summedRowsWithOnes;
				knownFieldsWithZerosByRows[index] = summedRowsWithZeros;

				if (index < smallerDimensionSize)
				{
					SyncKnownFields(
						knownFieldsWithOnesByRows,
						knownFieldsWithZerosByRows,
						knownFieldsWithZerosByColumns,
						knownFieldsWithOnesByColumns,
						ref index);
				}
			}
			if (index < numColumns)
			{
				// Create initial value with all bits active
				// Known zeros / ones will be marked with an active bit after the recursive call below
				var summedColumnsWithOnes = ~(Int128.Zero) & columnBitMask;
				var summedColumnsWithZeros = ~(Int128.Zero) & columnBitMask;

				HashSet<Int128> validList = new HashSet<Int128>();
				FindValidDimensionCombinationsRecursive(
								columnBlocks[index],
								0,
								0,
								validList,
								0,
								numRows,
								ref summedColumnsWithOnes,
								ref summedColumnsWithZeros,
								ref knownFieldsWithOnesByColumns[index],
								ref knownFieldsWithZerosByColumns[index]);
				validColumnCombinations.Add(index, validList);

				knownFieldsWithOnesByColumns[index] = summedColumnsWithOnes;
				knownFieldsWithZerosByColumns[index] = summedColumnsWithZeros;

				if (index < smallerDimensionSize)
				{
					SyncKnownFields(knownFieldsWithOnesByRows, knownFieldsWithZerosByRows, knownFieldsWithZerosByColumns, knownFieldsWithOnesByColumns, ref index);
				}
			}
		}
	}

	private static void SyncKnownFields(
		Span<Int128> knownFieldsWithOnesByRows,
		Span<Int128> knownFieldsWithZerosByRows,
		Span<Int128> knownFieldsWithZerosByColumns,
		Span<Int128> knownFieldsWithOnesByColumns,
		ref ushort index)
	{
		for (ushort row = 0; row <= index; row++)
		{
			for (ushort column = 0; column <= index; column++)
			{
				if ((knownFieldsWithOnesByRows[row] & (Int128.One << column)) != 0)
				{
					knownFieldsWithOnesByColumns[column] |= Int128.One << row;
				}
				if ((knownFieldsWithZerosByRows[row] & (Int128.One << column)) != 0)
				{
					knownFieldsWithZerosByColumns[column] |= Int128.One << row;
				}
				if ((knownFieldsWithOnesByColumns[column] & (Int128.One << row)) != 0)
				{
					knownFieldsWithOnesByRows[row] |= Int128.One << column;
				}
				if ((knownFieldsWithZerosByColumns[column] & (Int128.One << row)) != 0)
				{
					knownFieldsWithZerosByRows[row] |= Int128.One << column;
				}
			}
		}
	}

	private static void FindValidDimensionCombinationsRecursive(
						List<ushort> blocks,
						int blockIndex,
						Int128 curElement,
						HashSet<Int128> validList,
						int startIndex,
						int otherDimensionLength,
						ref Int128 knownOnes,
						ref Int128 knownZeros,
						ref Int128 alreadyKnownOnes,
						ref Int128 alreadyKnownZeros)
	{
		if (blocks.Count == 0)
		{
			knownZeros = 0;
			for (int x = 0; x < otherDimensionLength; x++)
			{
				knownZeros |= 1 << x;
			}
			validList.Add(Int128.Zero);
			knownOnes = 0;
			return;
		}
		var blockLength = blocks.ElementAt(blockIndex);
		// Determine where the last possible startIndex for this block Is
		// Corresponds to the last possible index where the remaining blocks (including 1 space in between) can still be placed
		var maxStartIndex =
				otherDimensionLength
				- blocks.Skip(blockIndex + 1).Sum(b => b + 1) // +1 since we need at least one empty space in between each block
				- startIndex
				- blockLength
				+ startIndex;

		for (var start = startIndex; start <= maxStartIndex; start++)
		{
			// set bits for current bar to 1
			for (int x = 0; x < blockLength; x++)
			{
				curElement |= Int128.One << (start + x);
			}

			if (blockIndex == blocks.Count - 1)
			{
				// Make sure that we are not violating any already known bits
				if ((alreadyKnownOnes & ~curElement) == 0 && (alreadyKnownZeros & curElement) == 0)
				{
					knownOnes &= curElement;
					knownZeros &= ~curElement;
					validList.Add(curElement);
				}
			}
			else
			{
				FindValidDimensionCombinationsRecursive(
								blocks,
								blockIndex + 1,
								curElement,
								validList,
								start + blockLength + (ushort)1,
								otherDimensionLength,
								ref knownOnes,
								ref knownZeros,
								ref alreadyKnownOnes,
								ref alreadyKnownZeros);
			}

			// reset all bits from current block onwards to 0
			Int128 eraseBitMask = 0;
			for (var x = 0; x <= blockLength; x++)
			{
				eraseBitMask |= Int128.One << (x + start);
			}
			curElement &= ~eraseBitMask;
		}
	}

	/// <summary>
	/// Parses the input files and fills static instance variables (numColumns, numRows, rowBlocks, columnBlocks)
	/// </summary>
	public static void ParseInput()
	{
		inputLines = File.ReadLines("nonogram.in").GetEnumerator();
		var firstLine = NextLine().Split(" ");
		numColumns = byte.Parse(firstLine[0]);
		numRows = byte.Parse(firstLine[1]);
		rowBlocks = new Dictionary<int, List<ushort>>(numRows);
		columnBlocks = new Dictionary<int, List<ushort>>(numColumns);
		validRowCombinations = new Dictionary<int, HashSet<Int128>>(numRows);
		validColumnCombinations = new Dictionary<int, HashSet<Int128>>(numColumns);
		solutions = new();

		for (int row = 0; row < numRows; row++)
		{
			var rowLine = NextLine().Split(" ");
			rowBlocks[row] = new List<ushort>(rowLine.Length);

			for (var block = 0; block < rowLine.Length; block++)
			{
				if (rowLine[block].Length > 0)
					rowBlocks[row].Add(byte.Parse(rowLine[block]));
			}
		}

		for (int column = 0; column < numColumns; column++)
		{
			var columnLine = NextLine().Split(" ");
			columnBlocks[column] = new List<ushort>(columnLine.Length);

			for (var block = 0; block < columnLine.Length; block++)
			{
				if (columnLine[block].Length > 0)
					columnBlocks[column].Add(byte.Parse(columnLine[block]));
			}
		}


		rowBitMask = Int128.Zero;
		for (var x = 0; x < numColumns; x++)
		{
			rowBitMask |= Int128.One << x;
		}
		columnBitMask = Int128.Zero;
		for (var y = 0; y < numRows; y++)
		{
			columnBitMask |= Int128.One << y;
		}
	}
}
