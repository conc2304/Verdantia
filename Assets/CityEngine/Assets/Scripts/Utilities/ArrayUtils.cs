
using System;
using UnityEngine;

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

    public static T[] Fill<T>(int size, T initialValue)
    {
        T[] array = new T[size];

        for (int i = 0; i < size; i++)
        {
            array[i] = initialValue;
        }

        return array;
    }


    public static T[] Slice<T>(T[] source, int start, int? end = null)
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

    public static float[,] CropMatrixToAspectRatio(float[,] matrix, int textureWidth, int textureHeight)
    {
        int originalRows = matrix.GetLength(0);
        int originalCols = matrix.GetLength(1);

        // Calculate the aspect ratio of the texture
        float textureAspectRatio = (float)textureWidth / textureHeight;

        // Start with the full matrix dimensions
        int croppedRows = originalRows;
        int croppedCols = Mathf.RoundToInt(croppedRows * textureAspectRatio);

        // Adjust dimensions to fit within bounds
        if (croppedCols > originalCols)
        {
            croppedCols = originalCols;
            croppedRows = Mathf.RoundToInt(croppedCols / textureAspectRatio);
        }

        // Ensure rows and columns are within bounds
        if (croppedRows > originalRows)
        {
            croppedRows = originalRows;
            croppedCols = Mathf.RoundToInt(croppedRows * textureAspectRatio);
        }

        // Ensure final cropping dimensions are still in bounds
        croppedRows = Mathf.Clamp(croppedRows, 1, originalRows);
        croppedCols = Mathf.Clamp(croppedCols, 1, originalCols);

        // Calculate starting indices to center the crop
        int startRow = Mathf.Clamp((originalRows - croppedRows) / 2, 0, originalRows - croppedRows);
        int startCol = Mathf.Clamp((originalCols - croppedCols) / 2, 0, originalCols - croppedCols);

        // Create and return the cropped matrix
        return CropMatrix(matrix, startRow, startCol, croppedRows, croppedCols);
    }

    public static float[,] CropMatrix(float[,] matrix, int startRow, int startCol, int numRows, int numCols)
    {
        // Validate inputs
        int originalRows = matrix.GetLength(0);
        int originalCols = matrix.GetLength(1);

        if (startRow < 0 || startCol < 0 || startRow + numRows > originalRows || startCol + numCols > originalCols)
        {
            throw new ArgumentOutOfRangeException("Crop dimensions are out of matrix bounds.");
        }

        // Create the cropped matrix
        float[,] croppedMatrix = new float[numRows, numCols];

        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                croppedMatrix[i, j] = matrix[startRow + i, startCol + j];
            }
        }

        return croppedMatrix;
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
