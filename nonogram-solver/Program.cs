//code by tino.heuberger@students.fhnw.ch
using System.Globalization;
using System.Linq;

public class NonoGramSolver
{
		static IEnumerator<string> inputLines = File.ReadLines("nonogram.in").GetEnumerator();
		static string NextLine() { if (!inputLines.MoveNext()) throw new Exception(); return inputLines.Current; }

		public static byte numRows;
		public static byte numColumns;
		public static Dictionary<int, HashSet<byte>>? rowBlocks;
		public static Dictionary<int, HashSet<byte>>? columnBlocks;
		public static Dictionary<int, HashSet<long>>? validRowCombinations;
		public static Dictionary<int, HashSet<long>>? validColumnCombinations;
		public static Dictionary<int, List<long>>? solutions;

		public static void Main()
		{
				inputLines = File.ReadLines("nonogram.in").GetEnumerator();
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				solutions = new();

				// Parsing
				ParseInput();

				// Pre-Processing
				FindValidDimensionCombinations(numRows, validRowCombinations!, rowBlocks!, numColumns);
				FindValidDimensionCombinations(numColumns, validColumnCombinations!, columnBlocks!, numRows);
				for (var i = 0; i < 5; i++)
				{
						FilterImpossibleCombinations(numRows, validRowCombinations, numColumns, validColumnCombinations);
						FilterImpossibleCombinations(numColumns, validColumnCombinations, numRows, validRowCombinations);
				}

				// Solving
				FindSolutions();
				if (solutions.Count == 0)
				{
						return;
				}

				// Output
				OutputSolutions();
		}

		private static void OutputSolutions()
		{
				long[] bitMasks = new long[numColumns];
				for (var i = 0; i < numColumns; i++)
				{
						bitMasks[i] = 0;
						bitMasks[i] |= (1L << i);
				}
				using (var outp = new StreamWriter("nonogram.out"))
				{
						for (var solution = 0; solution < solutions.Count; solution++)
						{
								for (var row = 0; row < solutions[solution].Count; row++)
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

		private static void FilterImpossibleCombinations(byte numXAxis, Dictionary<int, HashSet<long>> validXCombinations, byte numYAxis, Dictionary<int, HashSet<long>> validYCombinations)
		{
				for (var x = 0; x < numXAxis; x++)
				{
						var yBitMask = (1L << x);
						var validXAxisEntries = validXCombinations[x];


						for (var y = 0; y < numYAxis; y++)
						{
								var validYAxisEntries = validYCombinations[y];
								var xBitMask = (1L << y);
								var filledFieldsInXAxis = false;
								var emptyFieldsInXAxis = false;

								foreach (var validXComb in validXAxisEntries)
								{
										if ((validXComb & xBitMask) > 0)
										{
												filledFieldsInXAxis = true;
										}
										else
										{
												emptyFieldsInXAxis = true;
										}

										if (emptyFieldsInXAxis && filledFieldsInXAxis) break;
								}

								// If there are no combinations in the X axis that are filled at the given x/y coordinates
								// we can remove all y combinations that are empty at this location
								if (!filledFieldsInXAxis)
								{
										validXAxisEntries.RemoveWhere(e => (e & yBitMask) > 0);
								}
								// Same for empty fields
								if (!emptyFieldsInXAxis)
								{
										validXAxisEntries.RemoveWhere(e => (e & yBitMask) == 0);
								}

						}
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
				foreach (var combination in validRowCombinations![rowIndex])
				{
						// Pick a valid row combination to try
						boardRows[rowIndex] = combination;
						UpdateColumnBoard(boardColumns, ref boardRows[rowIndex], ref rowIndex);

						var hasValidColumns = IsValidPartialSolution(boardColumns, ref rowIndex);

						if (hasValidColumns && isLastRow)
						{
								solutions.Add(solutions.Count, new List<long>(boardRows.ToArray()));
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
						foreach (var validColumn in validColumnCombinations[columnIndex])
						{
								if ((boardColumns[columnIndex] & relevantRowsBitMask)
												== (validColumn & relevantRowsBitMask))
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
		public static void FindValidDimensionCombinations(int numElements, Dictionary<int, HashSet<long>> validArrays, Dictionary<int, HashSet<byte>> blocks, int otherDimensionLength)
		{
				for (var index = 0; index < numElements; index++)
				{
						if (blocks[index].Count == 0)
						{
								validArrays[index] = new(new[] { 0L });
						}
						else
						{
								HashSet<long> validList = new HashSet<long>();
								FindValidDimensionCombinationsRecursive(blocks[index], 0, 0, validList, 0, otherDimensionLength);
								validArrays.Add(index, validList);
						}
				}
		}

		private static void FindValidDimensionCombinationsRecursive(
						HashSet<byte> blocks,
						int blockIndex,
						long curElement,
						HashSet<long> validList,
						int startIndex,
						int otherDimensionLength)
		{
				var blockLength = blocks.ElementAt(blockIndex);
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
						for (int x = 0; x < blockLength; x++)
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
												start + blockLength + 1, otherDimensionLength);
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
				rowBlocks = new Dictionary<int, HashSet<byte>>(numRows);
				columnBlocks = new Dictionary<int, HashSet<byte>>(numColumns);
				validRowCombinations = new Dictionary<int, HashSet<long>>(numRows);
				validColumnCombinations = new Dictionary<int, HashSet<long>>(numColumns);

				for (int row = 0; row < numRows; row++)
				{
						var rowLine = NextLine().Split(" ");
						rowBlocks[row] = new HashSet<byte>(rowLine.Length);

						for (var block = 0; block < rowLine.Length; block++)
						{
								if (rowLine[block].Length > 0)
										rowBlocks[row].Add(byte.Parse(rowLine[block]));
						}
				}

				for (int column = 0; column < numColumns; column++)
				{
						var columnLine = NextLine().Split(" ");
						columnBlocks[column] = new HashSet<byte>(columnLine.Length);

						for (var block = 0; block < columnLine.Length; block++)
						{
								if (columnLine[block].Length > 0)
										columnBlocks[column].Add(byte.Parse(columnLine[block]));
						}
				}
		}
}
