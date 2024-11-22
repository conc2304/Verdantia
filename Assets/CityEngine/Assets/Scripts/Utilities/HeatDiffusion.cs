using Unity.Mathematics;
using UnityEngine;

public class HeatDiffusion : MonoBehaviour
{
    // NOTE these values are stable
    public float diffusionRate = 20f;
    public float dissipationRate = 0.999f;
    public float sunHeatBase { get; private set; } = 36f;
    public float heatAddRange = 0.05f;
    public float initialTemp = 67f;
    public float timeStep = 0.1f;

    private void Start()
    {
        // we are apply sun heat 2x (once for each tranposition step)
        sunHeatBase = initialTemp / 2;
    }

    public float[] GetColumnTemperatures(
        float[,] tempMatrix,
        float[,] heatContributionGrid,
        int calculateColumn,
        int gridSizeX = 100, int gridSizeZ = 100)
    {
        Debug.Log("GetColumnTemperatures");
        if (tempMatrix == null || tempMatrix.Length == 0)
        {
            tempMatrix = ArrayUtils.MatrixFill(gridSizeX, gridSizeZ, initialTemp);
        }

        float A = diffusionRate * timeStep / 2;
        float B = 1 + (2 * A) + (dissipationRate * timeStep / 4);
        float C = 2 - B;

        // 3 vectors | for the lower, upper and diagonal for the solver
        // int gridSize = gridSizeX * gridSizeZ;

        float[] lower = ArrayUtils.Fill(gridSizeZ - 1, -A);
        lower[gridSizeZ - 2] *= 2;

        float[] diagonal = ArrayUtils.Fill(gridSizeZ, B);
        float[] upper = ArrayUtils.Fill(gridSizeZ - 1, -A);
        upper[0] *= 2;

        // Form the right hand side of the system 
        float[] rightSide = new float[gridSizeZ];

        float heatContribution;
        // skip first and last
        for (int i = 0; i < gridSizeZ - 1; i++)
        {
            // Add heat sources and sinks
            heatContribution = heatContributionGrid[i, calculateColumn] + sunHeatBase;

            rightSide[i] = (heatContribution * timeStep) + (C * tempMatrix[i, calculateColumn]);

            if (calculateColumn == 0)
            {
                rightSide[i] += 2 * A * tempMatrix[i, 1];
            }
            else if (calculateColumn == gridSizeZ - 1)
            {
                rightSide[i] += 2 * A * tempMatrix[i, gridSizeZ - 2];
            }
            else
            {
                rightSide[i] += A * (tempMatrix[i, calculateColumn - 1] + tempMatrix[i, calculateColumn + 1]);
            }
        }

        // return new temperures at calculate column
        // Debug.Log($"lower : {lower.Length}  @ {calculateColumn}");
        // Debug.Log($"diagonal : {diagonal.Length}  @ {calculateColumn}");
        // Debug.Log($"upper : {upper.Length}  @ {calculateColumn}");
        // Debug.Log($"rightSide : {rightSide.Length}  @ {calculateColumn}");
        return TDMA.SolveInPlace(lower, diagonal, upper, rightSide);
    }

    public float[,] GetCityTempGrid(
        float[,] currentTemps,
        float[,] heatContributionGrid,
        int gridSizeX = 100, int gridSizeZ = 100)
    {

        if (currentTemps == null || currentTemps.Length == 0)
        {
            currentTemps = ArrayUtils.MatrixFill(gridSizeX, gridSizeZ, initialTemp);
        }

        sunHeatBase = initialTemp / 2;


        float[,] newTemps = new float[gridSizeX, gridSizeZ];
        for (int t = 1; t <= 2; t++)
        {
            // Run once for each row, and once for each column by transposing the 

            for (int i = 0; i < gridSizeZ; i++)
            {
                // For each column, calculate the new temperatures and assign them to the newTemps matrix
                float[] columnTemps = GetColumnTemperatures(currentTemps, heatContributionGrid, i, gridSizeX, gridSizeZ);
                for (int j = 0; j < gridSizeX; j++)
                {
                    newTemps[j, i] = columnTemps[j];
                }
            }

            heatContributionGrid = ArrayUtils.TransposeMatrix(heatContributionGrid);
            currentTemps = ArrayUtils.TransposeMatrix(newTemps);
        }

        return currentTemps;
    }
}
