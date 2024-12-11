
/**
Part of the "City Engine" Asset from the Unity Asset store (unchanged)
data structure used for saving and loading building-related information in a city-building game. 
It stores building properties such as indices, levels, positions, rotations, and timers. 
Initializes the data from an array of building properties, converting positional and rotational data into arrays for serialization. 
Facilitates saving and restoring building states during gameplay.
**/

[System.Serializable]
public class BuildingData
{
    public int length;
    public int[] buildingIndex;
    public int[] level;
    public float[][] position;
    public float[][] rotation;
    public float[] timer;

    public BuildingData(SaveDataTrigger.SaveProperties[] buildingProperties)
    {
        length = buildingProperties.Length;

        buildingIndex = new int[length];
        level = new int[length];
        position = new float[length][];
        rotation = new float[length][];
        timer = new float[length];
        for (int i = 0; i < length; i++)
        {
            buildingIndex[i] = buildingProperties[i].index;

            timer[i] = buildingProperties[i].timer;

            position[i] = new float[3];
            position[i][0] = buildingProperties[i].x;
            position[i][1] = buildingProperties[i].y;
            position[i][2] = buildingProperties[i].z;

            rotation[i] = new float[3];
            rotation[i][0] = buildingProperties[i].rot_x;
            rotation[i][1] = buildingProperties[i].rot_y;
            rotation[i][2] = buildingProperties[i].rot_z;
        }
    }

    public void SetArray(int length)
    {
        buildingIndex = new int[length];
        level = new int[length];
        position = new float[length][];
        rotation = new float[length][];
        timer = new float[length];
    }

}
