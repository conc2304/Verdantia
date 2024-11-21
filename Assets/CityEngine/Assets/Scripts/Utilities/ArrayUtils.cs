
using System;

public static class ArrayUtils
{
    public static T[,] MatrixFill<T>(int sizeX, int sizeY, T initialValue)
    {
        T[,] grid = new T[sizeX, sizeY];

        // Optional: Explicitly setting all values to 0 (not necessary as default for int is 0)
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                grid[i, j] = initialValue;
            }
        }

        return grid;
    }

    public static T[] ArrayFill<T>(int size, T initialValue)
    {
        T[] array = new T[size];

        for (int i = 0; i < size; i++)
        {
            array[i] = initialValue;
        }

        return array;
    }


    public static T[] ArraySlice<T>(T[] source, int start, int? end = null)
    {
        // Handle negative start index
        if (start < 0) start = source.Length + start;
        start = Math.Max(start, 0); // Ensure start is at least 0

        // Handle null or negative end index
        int actualEnd = end.HasValue ? end.Value : source.Length;
        if (actualEnd < 0) actualEnd = source.Length + actualEnd;
        actualEnd = Math.Min(actualEnd, source.Length); // Ensure end does not exceed array length

        // Determine the slice length
        int length = Math.Max(actualEnd - start, 0);

        // Create and populate the new array
        T[] result = new T[length];
        Array.Copy(source, start, result, 0, length);

        return result;
    }



    public static int[,] RotateMatrix90DegreesClockwise(int[,] matrix)
    {
        int n = matrix.GetLength(0);  // Number of rows
        int m = matrix.GetLength(1);  // Number of columns

        // Create a new matrix to hold the rotated values
        int[,] rotatedMatrix = new int[m, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                rotatedMatrix[j, n - 1 - i] = matrix[i, j];
            }
        }

        return rotatedMatrix;
    }

    public static float[,] RemovePaddingFromMatrix(float[,] matrix, int[] gridSize, int gridPadding = 2, int tileSize = 10)
    {
        // Calculate the new dimensions without padding
        int newGridLengthX = gridSize[0] / tileSize;
        int newGridLengthZ = gridSize[1] / tileSize;

        // Initialize a new matrix with the adjusted size
        float[,] shiftedMatrix = new float[newGridLengthX, newGridLengthZ];

        // Fill the new matrix by skipping padding rows and columns
        for (int x = 0; x < newGridLengthX; x++)
        {
            for (int z = 0; z < newGridLengthZ; z++)
            {
                // Offset by the padding to avoid it in the original matrix
                shiftedMatrix[x, z] = matrix[x + gridPadding, z + gridPadding];
            }
        }

        return shiftedMatrix;
    }

    public static T[,] TransposeMatrix<T>(T[,] matrix)
    {
        int rows = matrix.GetLength(0);  // Number of rows
        int cols = matrix.GetLength(1);  // Number of columns

        // Create a new matrix with swapped dimensions
        T[,] transposedMatrix = new T[cols, rows];

        // Perform the transposition
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                transposedMatrix[j, i] = matrix[i, j];
            }
        }

        return transposedMatrix;
    }
}
