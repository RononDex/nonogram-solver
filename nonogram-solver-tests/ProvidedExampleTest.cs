using System.Collections;

namespace nonogram_solver_tests;

public class ProvidedExampleTest
{

		[Test]
		public void Main_WithBunnyExample_ProducedCorrectOutput()
		{
				File.Copy("samples/example1.in", "nonogram.in", true);

				NonoGramSolver.Main();

				Assert.True(FileEquals("samples/example1.out", "nonogram.out"));
		}

		static bool FileEquals(string path1, string path2)
		{
				byte[] file1 = File.ReadAllBytes(path1);
				byte[] file2 = File.ReadAllBytes(path2);
				if (file1.Length == file2.Length)
				{
						for (int i = 0; i < file1.Length; i++)
						{
								if (file1[i] != file2[i])
								{
										return false;
								}
						}
						return true;
				}
				return false;
		}
}
