// Tri Diagonal Matrix Solver Algorithm

/**
Part of the "City Engine" Asset from the Unity Asset store (unchanged)

Implements the Thomas algorithm (also known as the Tri-Diagonal Matrix Algorithm) 
to solve a system of linear equations represented by a tridiagonal matrix. 
This algorithm solves the system in two main steps: forward elimination (modifying the lower, diagonal, and upper matrices) 
and backward substitution to compute the solution vector. 
It operates in-place to reduce memory usage and returns the solution as an array of floats.
**/
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
}
