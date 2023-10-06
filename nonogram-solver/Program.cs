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
            var type = typeof(IBinaryInteger<BigInteger>);

            var largerDimensionSize = numColumns > numRows ? numColumns : numRows;
            if (largerDimensionSize <= 16)
            {
                var solver = new GenericNonoGramSolver<ushort>(numRows, numColumns, rowBlocks!, columnBlocks!);
                solver.FindSolutions();
            }
            else if (largerDimensionSize <= 32)
            {
                var solver = new GenericNonoGramSolver<uint>(numRows, numColumns, rowBlocks!, columnBlocks!);
                solver.FindSolutions();
            }
            else if (largerDimensionSize <= 64)
            {
                var solver = new GenericNonoGramSolver<ulong>(numRows, numColumns, rowBlocks!, columnBlocks!);
                solver.FindSolutions();
            }
            else if (largerDimensionSize <= 128)
            {
                var solver = new GenericNonoGramSolver<UInt128>(numRows, numColumns, rowBlocks!, columnBlocks!);
                solver.FindSolutions();
            }
            else
            {
                var solver = new GenericNonoGramSolver<BigInteger>(numRows, numColumns, rowBlocks!, columnBlocks!);
                solver.FindSolutions();
            }
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


    public class GenericNonoGramSolver<T> where T : IBinaryInteger<T>
    {
        public ushort numRows;
        public ushort numColumns;
        public Dictionary<int, ushort[]> rowBlocks;
        public Dictionary<int, ushort[]> columnBlocks;
        public Dictionary<int, HashSet<T>> validRowCombinations;
        public Dictionary<int, HashSet<T>> validColumnCombinations;
        public T[][] validColumnCombinationsFinal;
        public T[][] validRowCombinationsFinal;
        public Dictionary<int, List<T>> solutions;

        public T rowBitMask;
        public T columnBitMask;

        public GenericNonoGramSolver(ushort numRows, ushort numColumn, Dictionary<int, ushort[]> rowBlocks, Dictionary<int, ushort[]> columnBlocks)
        {
            this.numColumns = numColumn;
            this.numRows = numRows;
            this.rowBlocks = rowBlocks;
            this.columnBlocks = columnBlocks;

            validRowCombinations = new Dictionary<int, HashSet<T>>(numRows);
            validColumnCombinations = new Dictionary<int, HashSet<T>>(numColumns);
            solutions = new();

            rowBitMask = T.Zero;
            for (var x = 0; x < numColumns; x++)
            {
                rowBitMask |= T.One << x;
            }
            columnBitMask = T.Zero;
            for (var y = 0; y < numRows; y++)
            {
                columnBitMask |= T.One << y;
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



        public int FilterImpossibleCombinations(int numXAxis, Dictionary<int, HashSet<T>> validXCombinations, int numYAxis, Dictionary<int, HashSet<T>> validYCombinations)
        {
            var knownOnesX = new T[numXAxis];
            var knownZerosX = new T[numXAxis];
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
            var knownOnesY = new T[numYAxis];
            var knownZerosY = new T[numYAxis];
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
                removedEntities += validRowCombinations[x].RemoveWhere(e => (~e & knownOnesX[x]) != T.Zero);
                removedEntities += validRowCombinations[x].RemoveWhere(e => (e & knownZerosX[x]) != T.Zero);
            }
            for (ushort y = 0; y < numYAxis; y++)
            {
                removedEntities += validColumnCombinations[y].RemoveWhere(e => (~e & knownOnesY[y]) != T.Zero);
                removedEntities += validColumnCombinations[y].RemoveWhere(e => (e & knownZerosY[y]) != T.Zero);
            }

            return removedEntities;
        }

        public void FindSolutionsEntryPoint()
        {
            // Store valid column and row combinations in arrays now, making iterating over them much much faster
            validColumnCombinationsFinal = new T[numColumns][];
            for (var i = 0; i < numColumns; i++)
            {
                validColumnCombinationsFinal[i] = new T[validColumnCombinations[i].Count];
                validColumnCombinations[i].CopyTo(validColumnCombinationsFinal[i]);
            }
            validRowCombinationsFinal = new T[numRows][];
            for (var i = 0; i < numRows; i++)
            {
                validRowCombinationsFinal[i] = new T[validRowCombinations[i].Count];
                validRowCombinations[i].CopyTo(validRowCombinationsFinal[i]);
            }

            var emptyBoardRows = new T[numRows];
            var emptyBoardColumns = new T[numColumns];
            var currentColumnStartIndices = new ushort[numColumns];
            FindSolutionRecursive(0, emptyBoardRows.AsSpan(), emptyBoardColumns.AsSpan(), currentColumnStartIndices);
        }

        private void FindSolutionRecursive(int rowIndex, Span<T> boardRows, Span<T> boardColumns, Span<ushort> currentColumnStartIndices)
        {
            var isLastRow = rowIndex == numRows - 1;
            var knownValidZerosInRow = T.Zero;
            var knownValidOnesInRow = T.Zero;

            // Shortcut if there is only one valid combination
            if (validRowCombinationsFinal[rowIndex].Length == 1)
            {
                boardRows[rowIndex] = validRowCombinationsFinal[rowIndex][0];
                UpdateColumnBoard(boardColumns, ref boardRows[rowIndex], ref rowIndex);
                if (isLastRow)
                {
                    solutions.Add((ushort)solutions.Count, new List<T>(boardRows.ToArray()));
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
                    solutions.Add((ushort)solutions.Count, new List<T>(boardRows.ToArray()));
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
                Span<T> boardColumns,
                Span<T> boardRows,
                ref int rowIndex,
                Span<ushort> currentColumnStartIndices,
                ref T knownValidOnes,
                ref T knownValidZeros)
        {
            var relevantRowsBitMask = T.Zero;
            for (var row = 0; row <= rowIndex; row++)
            {
                relevantRowsBitMask |= T.One << row;
            }

            var columnsToCheck = (~(knownValidOnes | knownValidZeros)) & rowBitMask;

            for (ushort columnIndex = 0; columnIndex < numColumns; columnIndex++)
            {
                var slice = validColumnCombinationsFinal[columnIndex].AsSpan().Slice(currentColumnStartIndices[columnIndex]);

                // Only validate if something changed in this column
                if ((columnsToCheck & (T.One << columnIndex)) != T.Zero)
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
                            var bitMask = T.One << columnIndex;
                            if ((boardRows[rowIndex] & bitMask) != T.Zero)
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

        private void UpdateColumnBoard(Span<T> boardColumns, ref T newlyChosenRow, ref int rowIndex)
        {
            for (ushort column = 0; column < numColumns; column++)
            {
                // set bit to 1 on column if row has bit set
                if ((newlyChosenRow & (T.One << column)) != T.Zero)
                {
                    boardColumns[column] |= T.One << rowIndex;
                }
                else
                {
                    // reset value to 0
                    boardColumns[column] &= ~(T.One << rowIndex);
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
            var knownFieldsWithOnesByRows = new T[numRows].AsSpan();
            var knownFieldsWithZerosByRows = new T[numRows].AsSpan();
            var knownFieldsWithZerosByColumns = new T[numColumns].AsSpan();
            var knownFieldsWithOnesByColumns = new T[numColumns].AsSpan();

            for (int index = 0; index < biggerDimensionSize; index++)
            {
                if (index < numRows)
                {
                    // Create initial value with all bits active
                    // Known zeros / ones will be marked with an active bit after the recursive call below
                    var summedRowsWithOnes = ~(T.Zero) & rowBitMask;
                    var summedRowsWithZeros = ~(T.Zero) & rowBitMask;

                    var validList = new HashSet<T>();
                    FindValidDimensionCombinationsRecursive(
                                    rowBlocks[index],
                                    0,
                                    T.Zero,
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
                    var summedColumnsWithOnes = ~(T.Zero) & columnBitMask;
                    var summedColumnsWithZeros = ~(T.Zero) & columnBitMask;

                    var validList = new HashSet<T>();
                    FindValidDimensionCombinationsRecursive(
                                    columnBlocks[index],
                                    0,
                                    T.Zero,
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
            Span<T> knownFieldsWithOnesByRows,
            Span<T> knownFieldsWithZerosByRows,
            Span<T> knownFieldsWithZerosByColumns,
            Span<T> knownFieldsWithOnesByColumns,
            ref int index)
        {
            for (int row = 0; row <= index; row++)
            {
                for (int column = 0; column <= index; column++)
                {
                    if ((knownFieldsWithOnesByRows[row] & (T.One << column)) != T.Zero)
                    {
                        knownFieldsWithOnesByColumns[column] |= T.One << row;
                    }
                    if ((knownFieldsWithZerosByRows[row] & (T.One << column)) != T.Zero)
                    {
                        knownFieldsWithZerosByColumns[column] |= T.One << row;
                    }
                    if ((knownFieldsWithOnesByColumns[column] & (T.One << row)) != T.Zero)
                    {
                        knownFieldsWithOnesByRows[row] |= T.One << column;
                    }
                    if ((knownFieldsWithZerosByColumns[column] & (T.One << row)) != T.Zero)
                    {
                        knownFieldsWithZerosByRows[row] |= T.One << column;
                    }
                }
            }
        }

        private void FindValidDimensionCombinationsRecursive(
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
            T[] bitMasks = new T[numColumns];
            for (var i = 0; i < numColumns; i++)
            {
                bitMasks[i] = T.Zero;
                bitMasks[i] |= T.One << i;
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
                            chars[x] = (solutions[solution][row] & bitMasks[x]) != T.Zero ? '#' : '.';

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
