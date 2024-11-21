using System;

// Tri Diagonal Matrix Solver Algorithm
public static class TDMA
{
    public static float[] SolveInPlace(float[] lower, float[] diagonal, float[] upper, float[] rightSide)
    {
        int n = diagonal.Length;
        // In-place modification of diagonal, upper, and right side
        for (int i = 1; i < n; i++)
        {
            float m = lower[i - 1] / diagonal[i - 1];
            diagonal[i] -= m * upper[i - 1];
            rightSide[i] -= m * rightSide[i - 1];
        }

        // Backward substitution
        float[] x = new float[n];
        x[n - 1] = rightSide[n - 1] / diagonal[n - 1];

        for (int i = n - 2; i >= 0; i--)
        {
            x[i] = (rightSide[i] - upper[i] * x[i + 1]) / diagonal[i];
        }

        return x;
    }

    // public static void Main()
    // {
    //     // Example of a tridiagonal system
    //     double[] lower = { 1.0, 1.0, 1.0 };  // Lower diagonal (subdiagonal)
    //     double[] diagonal = { 4.0, 4.0, 4.0, 4.0 };  // Main diagonal
    //     double[] upper = { 1.0, 1.0, 1.0 };  // Upper diagonal (superdiagonal)
    //     double[] b = { 5.0, 5.0, 5.0, 5.0 };  // Right-hand side

    //     double[] solution = SolveInPlace(lower, diagonal, upper, b);

    //     Console.WriteLine("Solution:");
    //     foreach (var val in solution)
    //     {
    //         Console.WriteLine(val);
    //     }
    // }
}
