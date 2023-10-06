//code by tino.heuberger@students.fhnw.ch
using System.Globalization;
using System.Numerics;

public class NonoGramSolver
{

    static IEnumerator<string> inputLines;
    static string NextLine() { if (!inputLines.MoveNext()) throw new Exception(); return inputLines.Current; }
    public static ushort numRows;
    public static ushort numColumns;
    public static Dictionary<int, ushort[]>? rowBlocks;
    public static Dictionary<int, ushort[]>? columnBlocks;

    public static void Main()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        checked
        {
            // Parsing
            ParseInput();

            if (numColumns <= 8)
            {
                if (numRows <= 8) SolveNonoGram<byte, byte>();
                else if (numRows <= 16) SolveNonoGram<byte, ushort>();
                else if (numRows <= 32) SolveNonoGram<byte, uint>();
                else if (numRows <= 64) SolveNonoGram<byte, ulong>();
                else if (numRows <= 128) SolveNonoGram<byte, UInt128>();
                else SolveNonoGram<byte, BigInteger>();
            }
            if (numColumns <= 16)
            {
                if (numRows <= 8) SolveNonoGram<ushort, byte>();
                else if (numRows <= 16) SolveNonoGram<ushort, ushort>();
                else if (numRows <= 32) SolveNonoGram<ushort, uint>();
                else if (numRows <= 64) SolveNonoGram<ushort, ulong>();
                else if (numRows <= 128) SolveNonoGram<ushort, UInt128>();
                else SolveNonoGram<ushort, BigInteger>();
            }
            else if (numColumns <= 32)
            {
                if (numRows <= 8) SolveNonoGram<uint, byte>();
                else if (numRows <= 16) SolveNonoGram<uint, ushort>();
                else if (numRows <= 32) SolveNonoGram<uint, uint>();
                else if (numRows <= 64) SolveNonoGram<uint, ulong>();
                else if (numRows <= 128) SolveNonoGram<uint, UInt128>();
                else SolveNonoGram<uint, BigInteger>();
            }
            else if (numColumns <= 64)
            {
                if (numRows <= 8) SolveNonoGram<ulong, byte>();
                else if (numRows <= 16) SolveNonoGram<ulong, ushort>();
                else if (numRows <= 32) SolveNonoGram<ulong, uint>();
                else if (numRows <= 64) SolveNonoGram<ulong, ulong>();
                else if (numRows <= 128) SolveNonoGram<ulong, UInt128>();
                else SolveNonoGram<ulong, BigInteger>();
            }
            else if (numColumns <= 128)
            {
                if (numRows <= 8) SolveNonoGram<UInt128, byte>();
                else if (numRows <= 16) SolveNonoGram<UInt128, ushort>();
                else if (numRows <= 32) SolveNonoGram<UInt128, uint>();
                else if (numRows <= 64) SolveNonoGram<UInt128, ulong>();
                else if (numRows <= 128) SolveNonoGram<UInt128, UInt128>();
                else SolveNonoGram<UInt128, BigInteger>();
            }
            else
            {
                if (numRows <= 8) SolveNonoGram<BigInteger, byte>();
                else if (numRows <= 16) SolveNonoGram<BigInteger, ushort>();
                else if (numRows <= 32) SolveNonoGram<BigInteger, uint>();
                else if (numRows <= 64) SolveNonoGram<BigInteger, ulong>();
                else if (numRows <= 128) SolveNonoGram<BigInteger, UInt128>();
                else SolveNonoGram<BigInteger, BigInteger>();
            }
        }
    }

    public static void SolveNonoGram<TRow, TColumn>()
        where TRow : IBinaryInteger<TRow>
        where TColumn : IBinaryInteger<TColumn>
    {
        var solver = new GenericNonoGramSolver<TRow, TColumn>(numRows, numColumns, rowBlocks!, columnBlocks!);
        solver.FindSolutions();
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
        rowBlocks = new Dictionary<int, ushort[]>(numRows);
        columnBlocks = new Dictionary<int, ushort[]>(numColumns);

        for (int row = 0; row < numRows; row++)
        {
            var rowLine = NextLine().Split(" ");
            rowBlocks[row] = new ushort[rowLine.Length];

            for (var block = 0; block < rowLine.Length; block++)
            {
                if (rowLine[block].Length > 0)
                    rowBlocks[row][block] = UInt16.Parse(rowLine[block]);
            }
        }

        for (int column = 0; column < numColumns; column++)
        {
            var columnLine = NextLine().Split(" ");
            columnBlocks[column] = new ushort[columnLine.Length];

            for (var block = 0; block < columnLine.Length; block++)
            {
                if (columnLine[block].Length > 0)
                    columnBlocks[column][block] = UInt16.Parse(columnLine[block]);
            }
        }
    }


    public class GenericNonoGramSolver<TRow, TColumn>
        where TRow : IBinaryInteger<TRow>
        where TColumn : IBinaryInteger<TColumn>
    {
        public ushort numRows;
        public ushort numColumns;
        public Dictionary<int, ushort[]> rowBlocks;
        public Dictionary<int, ushort[]> columnBlocks;
        public Dictionary<int, HashSet<TRow>> validRowCombinations;
        public Dictionary<int, HashSet<TColumn>> validColumnCombinations;
        public TColumn[][] validColumnCombinationsFinal;
        public TRow[][] validRowCombinationsFinal;
        public Dictionary<int, List<TRow>> solutions;

        public TRow rowBitMask;
        public TColumn columnBitMask;

        public GenericNonoGramSolver(ushort numRows, ushort numColumn, Dictionary<int, ushort[]> rowBlocks, Dictionary<int, ushort[]> columnBlocks)
        {
            this.numColumns = numColumn;
            this.numRows = numRows;
            this.rowBlocks = rowBlocks;
            this.columnBlocks = columnBlocks;

            validRowCombinations = new Dictionary<int, HashSet<TRow>>(numRows);
            validColumnCombinations = new Dictionary<int, HashSet<TColumn>>(numColumns);
            solutions = new();

            rowBitMask = TRow.Zero;
            for (var x = 0; x < numColumns; x++)
            {
                rowBitMask |= TRow.One << x;
            }
            columnBitMask = TColumn.Zero;
            for (var y = 0; y < numRows; y++)
            {
                columnBitMask |= TColumn.One << y;
            }
        }

        public void FindSolutions()
        {
            // PreProcessing
            FindValidCombinations();
            int numberOfIterations = (int)MathF.Sqrt(numColumns * numRows);
            for (var i = 0; i < numberOfIterations; i++)
            {
                var removedCombinations = FilterImpossibleCombinations(numRows, validRowCombinations, numColumns, validColumnCombinations);
                if (removedCombinations == 0)
                {
#if DEBUG
                    Console.WriteLine($"No more filtered possibilities after {i + 1} iterations");
#endif
                    break;
                }
            }

            // Solving
            FindSolutionsEntryPoint();
#if DEBUG
            Console.WriteLine($"Found {solutions.Count} solutions");
#endif
            OutputSolutions();
        }



        public int FilterImpossibleCombinations(int numXAxis, Dictionary<int, HashSet<TRow>> validXCombinations, int numYAxis, Dictionary<int, HashSet<TColumn>> validYCombinations)
        {
            var knownOnesX = new TRow[numXAxis];
            var knownZerosX = new TRow[numXAxis];
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
            var knownOnesY = new TColumn[numYAxis];
            var knownZerosY = new TColumn[numYAxis];
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

            int smallerDimensionIndex = numYAxis < numXAxis ? numYAxis : numXAxis;
            smallerDimensionIndex--;

            SyncKnownFields(knownOnesX, knownZerosX, knownZerosY, knownOnesY, ref smallerDimensionIndex);

            int removedEntities = 0;
            for (ushort x = 0; x < numXAxis; x++)
            {
                removedEntities += validRowCombinations[x].RemoveWhere(e => (~e & knownOnesX[x]) != TRow.Zero);
                removedEntities += validRowCombinations[x].RemoveWhere(e => (e & knownZerosX[x]) != TRow.Zero);
            }
            for (ushort y = 0; y < numYAxis; y++)
            {
                removedEntities += validColumnCombinations[y].RemoveWhere(e => (~e & knownOnesY[y]) != TColumn.Zero);
                removedEntities += validColumnCombinations[y].RemoveWhere(e => (e & knownZerosY[y]) != TColumn.Zero);
            }

            return removedEntities;
        }

        public void FindSolutionsEntryPoint()
        {
            // Store valid column and row combinations in arrays now, making iterating over them much much faster
            validColumnCombinationsFinal = new TColumn[numColumns][];
            for (var i = 0; i < numColumns; i++)
            {
                validColumnCombinationsFinal[i] = new TColumn[validColumnCombinations[i].Count];
                validColumnCombinations[i].CopyTo(validColumnCombinationsFinal[i]);
            }
            validRowCombinationsFinal = new TRow[numRows][];
            for (var i = 0; i < numRows; i++)
            {
                validRowCombinationsFinal[i] = new TRow[validRowCombinations[i].Count];
                validRowCombinations[i].CopyTo(validRowCombinationsFinal[i]);
            }

            var emptyBoardRows = new TRow[numRows];
            var emptyBoardColumns = new TColumn[numColumns];
            var currentColumnStartIndices = new ushort[numColumns];
            FindSolutionRecursive(0, emptyBoardRows.AsSpan(), emptyBoardColumns.AsSpan(), currentColumnStartIndices);
        }

        private void FindSolutionRecursive(int rowIndex, Span<TRow> boardRows, Span<TColumn> boardColumns, Span<ushort> currentColumnStartIndices)
        {
            var isLastRow = rowIndex == numRows - 1;
            var knownValidZerosInRow = TRow.Zero;
            var knownValidOnesInRow = TRow.Zero;

            // Shortcut if there is only one valid combination
            if (validRowCombinationsFinal[rowIndex].Length == 1)
            {
                boardRows[rowIndex] = validRowCombinationsFinal[rowIndex][0];
                UpdateColumnBoard(boardColumns, ref boardRows[rowIndex], ref rowIndex);
                if (isLastRow)
                {
                    solutions.Add((ushort)solutions.Count, new List<TRow>(boardRows.ToArray()));
                }
                else
                {
                    FindSolutionRecursive(rowIndex + 1, boardRows, boardColumns, currentColumnStartIndices);
                }
                return;
            }

            for (int combinationIndex = 0; combinationIndex < validRowCombinationsFinal![rowIndex].Length; combinationIndex++)
            {
                var copyOfCurrentColumnStartIndices = new ushort[currentColumnStartIndices.Length].AsSpan();
                currentColumnStartIndices.CopyTo(copyOfCurrentColumnStartIndices);
                var combination = validRowCombinationsFinal[rowIndex][combinationIndex];
                if (combinationIndex > 0)
                {
                    // Reset known bits that have changed since last combination in this row
                    var previousCombination = validRowCombinationsFinal[rowIndex][combinationIndex - 1];
                    var bitsThatDidNotChange = ~(combination ^ previousCombination);
                    knownValidOnesInRow &= bitsThatDidNotChange;
                    knownValidZerosInRow &= bitsThatDidNotChange;
                }
                // Pick a valid row combination to try
                boardRows[rowIndex] = combination;
                UpdateColumnBoard(boardColumns, ref boardRows[rowIndex], ref rowIndex);

                var hasValidColumns = IsValidPartialSolution(
                        boardColumns,
                        boardRows,
                        ref rowIndex,
                        copyOfCurrentColumnStartIndices,
                        ref knownValidOnesInRow,
                        ref knownValidZerosInRow);

                if (hasValidColumns && isLastRow)
                {
                    solutions.Add((ushort)solutions.Count, new List<TRow>(boardRows.ToArray()));
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
        private bool IsValidPartialSolution(
                Span<TColumn> boardColumns,
                Span<TRow> boardRows,
                ref int rowIndex,
                Span<ushort> currentColumnStartIndices,
                ref TRow knownValidOnes,
                ref TRow knownValidZeros)
        {
            var relevantRowsBitMask = TColumn.Zero;
            for (var row = 0; row <= rowIndex; row++)
            {
                relevantRowsBitMask |= TColumn.One << row;
            }

            var columnsToCheck = (~(knownValidOnes | knownValidZeros)) & rowBitMask;

            for (ushort columnIndex = 0; columnIndex < numColumns; columnIndex++)
            {
                var slice = validColumnCombinationsFinal[columnIndex].AsSpan().Slice(currentColumnStartIndices[columnIndex]);

                // Only validate if something changed in this column
                if ((columnsToCheck & (TRow.One << columnIndex)) != TRow.Zero)
                {
                    var foundMatch = false;
                    for (ushort validColumnIndex = 0; validColumnIndex < slice.Length; validColumnIndex++)
                    {
                        var validColumn = slice[validColumnIndex];
                        if ((boardColumns[columnIndex] & relevantRowsBitMask)
                                        == (validColumn & relevantRowsBitMask))
                        {
                            // update currentColumnStartIndices so we don't have to check the previous columns again
                            // as those will all fail for future checks
                            currentColumnStartIndices[columnIndex] += validColumnIndex;

                            // mark this column as known to be valid
                            var bitMask = TRow.One << columnIndex;
                            if ((boardRows[rowIndex] & bitMask) != TRow.Zero)
                                knownValidOnes |= bitMask;
                            else
                                knownValidZeros |= bitMask;

                            foundMatch = true;
                            break;
                        }

                    }

                    if (!foundMatch)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void UpdateColumnBoard(Span<TColumn> boardColumns, ref TRow newlyChosenRow, ref int rowIndex)
        {
            for (ushort column = 0; column < numColumns; column++)
            {
                // set bit to 1 on column if row has bit set
                if ((newlyChosenRow & (TRow.One << column)) != TRow.Zero)
                {
                    boardColumns[column] |= TColumn.One << rowIndex;
                }
                else
                {
                    // reset value to 0
                    boardColumns[column] &= ~(TColumn.One << rowIndex);
                }
            }
        }

        /// <summary>
        /// Pre-Calculate all valid block positions in this dimension by storing the start positions of every block
        /// This reduces redudancy in backtracking calulations and makes finish validation easier since
        /// we no BigIntegerer have to validate this dimension
        /// </summary>
        public void FindValidCombinations()
        {
            ushort biggerDimensionSize = numColumns > numRows ? numColumns : numRows;
            ushort smallerDimensionSize = numColumns < numRows ? numColumns : numRows;
            var knownFieldsWithOnesByRows = new TRow[numRows].AsSpan();
            var knownFieldsWithZerosByRows = new TRow[numRows].AsSpan();
            var knownFieldsWithZerosByColumns = new TColumn[numColumns].AsSpan();
            var knownFieldsWithOnesByColumns = new TColumn[numColumns].AsSpan();

            for (int index = 0; index < biggerDimensionSize; index++)
            {
                if (index < numRows)
                {
                    // Create initial value with all bits active
                    // Known zeros / ones will be marked with an active bit after the recursive call below
                    var summedRowsWithOnes = ~(TRow.Zero) & rowBitMask;
                    var summedRowsWithZeros = ~(TRow.Zero) & rowBitMask;

                    var validList = new HashSet<TRow>();
                    FindValidDimensionCombinationsRecursive<TRow>(
                                    rowBlocks[index],
                                    0,
                                    TRow.Zero,
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
                    var summedColumnsWithOnes = ~(TColumn.Zero) & columnBitMask;
                    var summedColumnsWithZeros = ~(TColumn.Zero) & columnBitMask;

                    var validList = new HashSet<TColumn>();
                    FindValidDimensionCombinationsRecursive<TColumn>(
                                    columnBlocks[index],
                                    0,
                                    TColumn.Zero,
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
                        SyncKnownFields(
                                knownFieldsWithOnesByRows,
                                knownFieldsWithZerosByRows,
                                knownFieldsWithZerosByColumns,
                                knownFieldsWithOnesByColumns,
                                ref index);

                    }

                    // Remove already clearly invalid possibilities
                    // Helpt to reduce the memory spike at the beginning when loading
                    // all valid combinations
                    FilterImpossibleCombinations(
                            index + 1 > numRows ? numRows : index + 1,
                            validRowCombinations,
                            index + 1 > numColumns ? numColumns : index + 1,
                            validColumnCombinations);
                }
            }
        }

        private void SyncKnownFields(
            Span<TRow> knownFieldsWithOnesByRows,
            Span<TRow> knownFieldsWithZerosByRows,
            Span<TColumn> knownFieldsWithZerosByColumns,
            Span<TColumn> knownFieldsWithOnesByColumns,
            ref int index)
        {
            for (int row = 0; row <= index; row++)
            {
                for (int column = 0; column <= index; column++)
                {
                    if ((knownFieldsWithOnesByRows[row] & (TRow.One << column)) != TRow.Zero)
                    {
                        knownFieldsWithOnesByColumns[column] |= TColumn.One << row;
                    }
                    if ((knownFieldsWithZerosByRows[row] & (TRow.One << column)) != TRow.Zero)
                    {
                        knownFieldsWithZerosByColumns[column] |= TColumn.One << row;
                    }
                    if ((knownFieldsWithOnesByColumns[column] & (TColumn.One << row)) != TColumn.Zero)
                    {
                        knownFieldsWithOnesByRows[row] |= TRow.One << column;
                    }
                    if ((knownFieldsWithZerosByColumns[column] & (TColumn.One << row)) != TColumn.Zero)
                    {
                        knownFieldsWithZerosByRows[row] |= TRow.One << column;
                    }
                }
            }
        }

        private void FindValidDimensionCombinationsRecursive<T>(
                            ushort[] blocks,
                            int blockIndex,
                            T curElement,
                            HashSet<T> validList,
                            int startIndex,
                            int otherDimensionLength,
                            ref T knownOnes,
                            ref T knownZeros,
                            ref T alreadyKnownOnes,
                            ref T alreadyKnownZeros)
            where T : IBinaryInteger<T>
        {
            if (blocks.Length == 0)
            {
                knownZeros = T.Zero;
                for (int x = 0; x < otherDimensionLength; x++)
                {
                    knownZeros |= T.One << x;
                }
                validList.Add(T.Zero);
                knownOnes = T.Zero;
                return;
            }
            var blockLength = blocks[blockIndex];
            var remainingBlocksSizes = 0;
            for (var i = blockIndex + 1; i < blocks.Length; i++)
            {
                remainingBlocksSizes += blocks[i] + 1;
            }

            // Determine where the last possible startIndex for this block Is
            // Corresponds to the last possible index where the remaining blocks (including 1 space in between) can still be placed
            var maxStartIndex =
                    otherDimensionLength
                    - remainingBlocksSizes
                    - startIndex
                    - blockLength
                    + startIndex;

            for (var start = startIndex; start <= maxStartIndex; start++)
            {
                // set bits for current bar to 1
                for (int x = 0; x < blockLength; x++)
                {
                    curElement |= T.One << (start + x);
                }

                if (blockIndex == blocks.Length - 1)
                {
                    // Make sure that we are not violating any already known bits
                    if ((alreadyKnownOnes & ~curElement) == T.Zero && (alreadyKnownZeros & curElement) == T.Zero)
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
                T eraseBitMask = T.Zero;
                for (var x = 0; x <= blockLength; x++)
                {
                    eraseBitMask |= T.One << (x + start);
                }
                curElement &= ~eraseBitMask;
            }
        }

        private void OutputSolutions()
        {
            TRow[] bitMasks = new TRow[numColumns];
            for (var i = 0; i < numColumns; i++)
            {
                bitMasks[i] = TRow.Zero;
                bitMasks[i] |= TRow.One << i;
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
                            chars[x] = (solutions[solution][row] & bitMasks[x]) != TRow.Zero ? '#' : '.';

                        }

                        outp.Write(chars);
#if DEBUG
                        Console.WriteLine(chars);
#endif
                    }
                    outp.Write("\n");
#if DEBUG
                    Console.WriteLine();
#endif
                }
            }
        }
    }
}
