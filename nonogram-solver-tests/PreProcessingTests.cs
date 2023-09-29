using System.Collections;

namespace nonogram_solver_tests;

public class PreProcessingTests
{
    [Test]
    public void PreProccesing_WithDuck_FindsCorrectPossibleRows()
    {
        File.Move("samples/duck.in", "nonogram.in", true);

        NonoGramSolver.ParseInput();
        NonoGramSolver.FindValidRowCombinations();

        Assert.AreEqual(9, NonoGramSolver.validRowCombinations.Length);

        /* Possible solutions for first row
                ###------
                -###-----
                --###----
                ---###---
                ----###--
                -----###-
                ------###
         index  012345678
        */
        Assert.AreEqual(7, NonoGramSolver.validRowCombinations[0].Length);
        Assert.AreEqual(0b000000111, NonoGramSolver.validRowCombinations[0][0]);
        Assert.AreEqual(0b000001110, NonoGramSolver.validRowCombinations[0][1]);
        Assert.AreEqual(0b000011100, NonoGramSolver.validRowCombinations[0][2]);
        Assert.AreEqual(0b000111000, NonoGramSolver.validRowCombinations[0][3]);
        Assert.AreEqual(0b001110000, NonoGramSolver.validRowCombinations[0][4]);
        Assert.AreEqual(0b011100000, NonoGramSolver.validRowCombinations[0][5]);
        Assert.AreEqual(0b111000000, NonoGramSolver.validRowCombinations[0][6]);

        /* Possible solutions for first row
                ##-#-----
                ##--#----
                ##---#---
                ##----#--
                ##-----#-
                ##------#
                -##-#----
                -##--#---
                -##---#--
                -##----#-
                -##-----#
                --##-#---
                --##--#--
                --##---#-
                --##----#
                ---##-#--
                ---##--#-
                ---##---#
                ----##-#-
                ----##--#
                -----##-#
         index  012345678
        */
        Assert.AreEqual(21, NonoGramSolver.validRowCombinations[1].Length);
        Assert.AreEqual(0b000001011, NonoGramSolver.validRowCombinations[1][0]);
        Assert.AreEqual(0b000010011, NonoGramSolver.validRowCombinations[1][1]);
        Assert.AreEqual(0b000100011, NonoGramSolver.validRowCombinations[1][2]);
        Assert.AreEqual(0b001000011, NonoGramSolver.validRowCombinations[1][3]);
        Assert.AreEqual(0b010000011, NonoGramSolver.validRowCombinations[1][4]);
        Assert.AreEqual(0b100000011, NonoGramSolver.validRowCombinations[1][5]);
        Assert.AreEqual(0b000010110, NonoGramSolver.validRowCombinations[1][6]);
        Assert.AreEqual(0b000100110, NonoGramSolver.validRowCombinations[1][7]);
        Assert.AreEqual(0b001000110, NonoGramSolver.validRowCombinations[1][8]);
        Assert.AreEqual(0b010000110, NonoGramSolver.validRowCombinations[1][9]);
        Assert.AreEqual(0b100000110, NonoGramSolver.validRowCombinations[1][10]);
        Assert.AreEqual(0b000101100, NonoGramSolver.validRowCombinations[1][11]);
        Assert.AreEqual(0b001001100, NonoGramSolver.validRowCombinations[1][12]);
        Assert.AreEqual(0b010001100, NonoGramSolver.validRowCombinations[1][13]);
        Assert.AreEqual(0b100001100, NonoGramSolver.validRowCombinations[1][14]);
        Assert.AreEqual(0b001011000, NonoGramSolver.validRowCombinations[1][15]);
        Assert.AreEqual(0b010011000, NonoGramSolver.validRowCombinations[1][16]);
        Assert.AreEqual(0b100011000, NonoGramSolver.validRowCombinations[1][17]);
        Assert.AreEqual(0b010110000, NonoGramSolver.validRowCombinations[1][18]);
        Assert.AreEqual(0b100110000, NonoGramSolver.validRowCombinations[1][19]);
        Assert.AreEqual(0b101100000, NonoGramSolver.validRowCombinations[1][20]);
    }
}
