using System.Collections;

namespace nonogram_solver_tests;

public class PreProcessingTests
{
		[Test]
		public void PreProccesing_WithDuck_FindsCorrectPossibleRows()
		{
				File.Copy("samples/duck.in", "nonogram.in", true);

				NonoGramSolver.ParseInput();
				NonoGramSolver.FindValidDimensionCombinations(
					NonoGramSolver.numRows,
					NonoGramSolver.validRowCombinations!,
					NonoGramSolver.rowBlocks!,
					NonoGramSolver.numColumns);

				Assert.AreEqual(9, NonoGramSolver.validRowCombinations.Count);

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
				Assert.AreEqual(7, NonoGramSolver.validRowCombinations[0].Count);
				Assert.True(NonoGramSolver.validRowCombinations[0].Any(l => l.Equals(0b000000111)));
				Assert.True(NonoGramSolver.validRowCombinations[0].Any(l => l.Equals(0b000001110)));
				Assert.True(NonoGramSolver.validRowCombinations[0].Any(l => l.Equals(0b000011100)));
				Assert.True(NonoGramSolver.validRowCombinations[0].Any(l => l.Equals(0b000111000)));
				Assert.True(NonoGramSolver.validRowCombinations[0].Any(l => l.Equals(0b001110000)));
				Assert.True(NonoGramSolver.validRowCombinations[0].Any(l => l.Equals(0b011100000)));
				Assert.True(NonoGramSolver.validRowCombinations[0].Any(l => l.Equals(0b111000000)));

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
				Assert.AreEqual(21, NonoGramSolver.validRowCombinations[1].Count);
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b000001011)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b000010011)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b000100011)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b001000011)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b010000011)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b100000011)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b000010110)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b000100110)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b001000110)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b010000110)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b100000110)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b000101100)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b001001100)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b010001100)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b100001100)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b001011000)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b010011000)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b100011000)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b010110000)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b100110000)));
				Assert.True(NonoGramSolver.validRowCombinations[1].Any(l => l.Equals(0b101100000)));
		}
}
