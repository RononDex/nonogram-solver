//code by tino.heuberger@students.fhnw.ch
using System.Globalization;
using System.Numerics;

public class NonoGramSolver
{
		static IEnumerator<string> inputLines = File.ReadLines("nonogram.in").GetEnumerator();
		static string NextLine() { if (!inputLines.MoveNext()) throw new Exception(); return inputLines.Current; }

		public static ushort numRows;
		public static ushort numColumns;
		public static Dictionary<int, HashSet<ushort>>? rowBlocks;
		public static Dictionary<int, HashSet<ushort>>? columnBlocks;
		public static Dictionary<int, HashSet<BigInteger>>? validRowCombinations;
		public static Dictionary<int, HashSet<BigInteger>>? validColumnCombinations;
		public static Dictionary<int, List<BigInteger>>? solutions;

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

		private static void FilterImpossibleCombinations(int numXAxis, Dictionary<int, HashSet<BigInteger>> validXCombinations, int numYAxis, Dictionary<int, HashSet<BigInteger>> validYCombinations)
		{
				for (ushort x = 0; x < numXAxis; x++)
				{
						var yBitMask = (1L << x);
						var validXAxisEntries = validXCombinations[x];


						for (ushort y = 0; y < numYAxis; y++)
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
				var emptyBoardRows = new BigInteger[numRows];
				var emptyBoardColumns = new BigInteger[numColumns];
				FindSolutionRecursive(0, emptyBoardRows.AsSpan(), emptyBoardColumns.AsSpan());
		}

		private static void FindSolutionRecursive(int rowIndex, Span<BigInteger> boardRows, Span<BigInteger> boardColumns)
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
								solutions.Add((ushort)solutions.Count, new List<BigInteger>(boardRows.ToArray()));
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
		private static bool IsValidPartialSolution(Span<BigInteger> boardColumns, ref int rowIndex)
		{
				BigInteger relevantRowsBitMask = 0;
				for (var row = 0; row <= rowIndex; row++)
				{
						relevantRowsBitMask |= BigInteger.One << row;
				}
				for (ushort columnIndex = 0; columnIndex < numColumns; columnIndex++)
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

		private static void UpdateColumnBoard(Span<BigInteger> boardColumns, ref BigInteger newlyChosenRow, ref int rowIndex)
		{
				for (ushort column = 0; column < numColumns; column++)
				{
						// set bit to 1 on column if row has bit set
						if ((newlyChosenRow & (BigInteger.One << column)) > 0)
						{
								boardColumns[column] |= (BigInteger.One << rowIndex);
						}
						else
						{
								// reset value to 0
								boardColumns[column] &= ~(BigInteger.One << rowIndex);
						}
				}
		}

		/// <summary>
		/// Pre-Calculate all valid block positions in this dimension by storing the start positions of every block
		/// This reduces redudancy in backtracking calulations and makes finish validation easier since
		/// we no longer have to validate this dimension
		/// </summary>
		public static void FindValidDimensionCombinations(int numElements, Dictionary<int, HashSet<BigInteger>> validArrays, Dictionary<int, HashSet<ushort>> blocks, int otherDimensionLength)
		{
				for (ushort index = 0; index < numElements; index++)
				{
						if (blocks[index].Count == 0)
						{
								validArrays[index] = new(new[] { BigInteger.Zero });
						}
						else
						{
								HashSet<BigInteger> validList = new HashSet<BigInteger>();
								FindValidDimensionCombinationsRecursive(blocks[index], 0, 0, validList, 0, otherDimensionLength);
								validArrays.Add(index, validList);
						}
				}
		}

		private static void FindValidDimensionCombinationsRecursive(
						HashSet<ushort> blocks,
						int blockIndex,
						BigInteger curElement,
						HashSet<BigInteger> validList,
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

				for (var start = startIndex; start <= maxStartIndex; start++)
				{
						// set bits for current bar to 1
						for (int x = 0; x < blockLength; x++)
						{
								curElement |= BigInteger.One << (start + x);
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
												start + blockLength + (ushort)1,
												otherDimensionLength);
						}

						// reset all bits from current block onwards to 0
						BigInteger eraseBitMask = 0;
						for (var x = 0; x <= blockLength; x++)
						{
								eraseBitMask |= BigInteger.One << (x + start);
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
				rowBlocks = new Dictionary<int, HashSet<ushort>>(numRows);
				columnBlocks = new Dictionary<int, HashSet<ushort>>(numColumns);
				validRowCombinations = new Dictionary<int, HashSet<BigInteger>>(numRows);
				validColumnCombinations = new Dictionary<int, HashSet<BigInteger>>(numColumns);

				for (int row = 0; row < numRows; row++)
				{
						var rowLine = NextLine().Split(" ");
						rowBlocks[row] = new HashSet<ushort>(rowLine.Length);

						for (var block = 0; block < rowLine.Length; block++)
						{
								if (rowLine[block].Length > 0)
										rowBlocks[row].Add(byte.Parse(rowLine[block]));
						}
				}

				for (int column = 0; column < numColumns; column++)
				{
						var columnLine = NextLine().Split(" ");
						columnBlocks[column] = new HashSet<ushort>(columnLine.Length);

						for (var block = 0; block < columnLine.Length; block++)
						{
								if (columnLine[block].Length > 0)
										columnBlocks[column].Add(byte.Parse(columnLine[block]));
						}
				}
		}
}
