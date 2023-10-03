//code by tino.heuberger@students.fhnw.ch
using System.Globalization;
using System.Runtime.Intrinsics;

public class NonoGramSolver
{
    static IEnumerator<string> inputLines;
    static string NextLine() { if (!inputLines.MoveNext()) throw new Exception(); return inputLines.Current; }

    public static ushort numRows;
    public static ushort numColumns;
    public static Dictionary<int, ushort[]>? rowBlocks;
    public static Dictionary<int, ushort[]>? columnBlocks;
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
        checked
        {
            // Parsing
            ParseInput();

            // Pre-Processing
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
            FindSolutions();

#if DEBUG
            Console.WriteLine($"Found {solutions.Count} solutions");
#endif
            if (solutions.Count == 0)
            {
                return;
            }
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
                outp.Write("\n");
            }
        }
    }

    public static int FilterImpossibleCombinations(int numXAxis, Dictionary<int, HashSet<Int128>> validXCombinations, int numYAxis, Dictionary<int, HashSet<Int128>> validYCombinations)
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

        int smallerDimensionIndex = numYAxis < numXAxis ? numYAxis : numXAxis;
        smallerDimensionIndex--;

        SyncKnownFields(knownOnesX, knownZerosX, knownZerosY, knownOnesY, ref smallerDimensionIndex);

        int removedEntities = 0;
        for (ushort x = 0; x < numXAxis; x++)
        {
            removedEntities += validRowCombinations[x].RemoveWhere(e => (~e & knownOnesX[x]) != 0);
            removedEntities += validRowCombinations[x].RemoveWhere(e => (e & knownZerosX[x]) != 0);
        }
        for (ushort y = 0; y < numYAxis; y++)
        {
            removedEntities += validColumnCombinations[y].RemoveWhere(e => (~e & knownOnesY[y]) != 0);
            removedEntities += validColumnCombinations[y].RemoveWhere(e => (e & knownZerosY[y]) != 0);
        }

        return removedEntities;
    }

    public static void FindSolutions()
    {
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

        var emptyBoardRows = new Int128[numRows];
        var emptyBoardColumns = new Int128[numColumns];
        var currentColumnStartIndices = new ushort[numColumns];
        FindSolutionRecursive(0, emptyBoardRows.AsSpan(), emptyBoardColumns.AsSpan(), currentColumnStartIndices);
    }

    private static void FindSolutionRecursive(int rowIndex, Span<Int128> boardRows, Span<Int128> boardColumns, Span<ushort> currentColumnStartIndices)
    {
        var isLastRow = rowIndex == numRows - 1;
        var knownValidZerosInRow = Int128.Zero;
        var knownValidOnesInRow = Int128.Zero;

        // Shortcut if there is only one valid combination
        if (validRowCombinationsFinal[rowIndex].Length == 1)
        {
            boardRows[rowIndex] = validRowCombinationsFinal[rowIndex][0];
            UpdateColumnBoard(boardColumns, ref boardRows[rowIndex], ref rowIndex);
            if (isLastRow)
            {
                solutions.Add((ushort)solutions.Count, new List<Int128>(boardRows.ToArray()));
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
    private static bool IsValidPartialSolution(
            Span<Int128> boardColumns,
            Span<Int128> boardRows,
            ref int rowIndex,
            Span<ushort> currentColumnStartIndices,
            ref Int128 knownValidOnes,
            ref Int128 knownValidZeros)
    {
        var relevantRowsBitMask = Int128.Zero;
        for (var row = 0; row <= rowIndex; row++)
        {
            relevantRowsBitMask |= Int128.One << row;
        }

        var columnsToCheck = (~(knownValidOnes | knownValidZeros)) & rowBitMask;

        for (ushort columnIndex = 0; columnIndex < numColumns; columnIndex++)
        {
            var slice = validColumnCombinationsFinal[columnIndex].AsSpan().Slice(currentColumnStartIndices[columnIndex]);

            // Only validate if something changed in this column
            if ((columnsToCheck & (Int128.One << columnIndex)) != 0)
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
                        var bitMask = Int128.One << columnIndex;
                        if ((boardRows[rowIndex] & bitMask) != 0)
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

        for (int index = 0; index < biggerDimensionSize; index++)
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

    private static void SyncKnownFields(
        Span<Int128> knownFieldsWithOnesByRows,
        Span<Int128> knownFieldsWithZerosByRows,
        Span<Int128> knownFieldsWithZerosByColumns,
        Span<Int128> knownFieldsWithOnesByColumns,
        ref int index)
    {
        for (int row = 0; row <= index; row++)
        {
            for (int column = 0; column <= index; column++)
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
                        ushort[] blocks,
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
        if (blocks.Length == 0)
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
                curElement |= Int128.One << (start + x);
            }

            if (blockIndex == blocks.Length - 1)
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
        rowBlocks = new Dictionary<int, ushort[]>(numRows);
        columnBlocks = new Dictionary<int, ushort[]>(numColumns);
        validRowCombinations = new Dictionary<int, HashSet<Int128>>(numRows);
        validColumnCombinations = new Dictionary<int, HashSet<Int128>>(numColumns);
        solutions = new();

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
