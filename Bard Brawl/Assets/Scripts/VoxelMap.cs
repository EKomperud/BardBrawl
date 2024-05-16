using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainUtils;

public class VoxelMap : MonoBehaviour
{
    public enum MapMarker
    {
        Open = 0,
        SolidTerrain = 1,
        Water = 2,
        Climbable = 4,
        Rollable = 8,
        Pushable = 16,
        Barrier = 32,
        Breakable = 64,
        HeightLow = 128,
        HeightMid = 256,
        HeightHigh = 512,
        OrientationX = 1024,
        OrientationZ = 2048
    }

    VoxelData[,,] mapTerrain;
    VoxelData[,,] mapProps;

    [SerializeField]
    short sizeX;
    [SerializeField]
    short sizeY;
    [SerializeField]
    short sizeZ;

    [SerializeField]
    private GameObject terrainCollection;
    [SerializeField]
    private GameObject propsCollection;

    void Start()
    {
        PopulateMaps();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Setup
    void PopulateMaps()
    {
        // Store data for all Terrain voxels
        mapTerrain = new VoxelData[sizeX, sizeY, sizeZ];
        for (int i = 0; i < terrainCollection.transform.childCount; i++)
        {
            Transform voxel = terrainCollection.transform.GetChild(i);
            VoxelData data = voxel.GetComponent<VoxelData>();
            if (data != null)
            {
                Vector3 coords = voxel.transform.localPosition;
                Vector3Int coordsInt = new Vector3Int
                    (Mathf.FloorToInt(coords.x + 0.02f),
                    Mathf.FloorToInt(coords.y + 0.02f),
                    Mathf.FloorToInt(coords.z + 0.02f));
                mapTerrain[coordsInt.x, coordsInt.y, coordsInt.z] = data;
            }
        }

        // Store data for all Prop voxels
        mapProps = new VoxelData[sizeX, sizeY, sizeZ];
        for (int i = 0; i < propsCollection.transform.childCount; i++)
        {
            Transform voxel = propsCollection.transform.GetChild(i);
            VoxelData data = voxel.GetComponent<VoxelData>();
            if (data != null)
            {
                Vector3 coords = voxel.transform.localPosition;
                Vector3Int coordsInt = new Vector3Int
                    (Mathf.FloorToInt(coords.x + 0.02f),
                    Mathf.FloorToInt(coords.y + 0.02f),
                    Mathf.FloorToInt(coords.z + 0.02f));
                mapProps[coordsInt.x, coordsInt.y, coordsInt.z] = data;
            }
        }
    }

    IEnumerator BuildMap_PopupFromBottom()
    {
        int isEven = 1 - (sizeX & 1);
        int startingCoord = (sizeX / 2) - isEven;
        int workingCoordX = startingCoord;
        int workingCoordZ = startingCoord;
        int travelDistance = isEven; //increments by 2 each loop
        int traveled = 0;
        Vector3 translation = new Vector3(0.0f, 0.5f, 0.0f);

        foreach (VoxelData voxel in mapTerrain)
        {
            Vector3 posOld = voxel.transform.localPosition;
            voxel.transform.localPosition = new Vector3(posOld.x, posOld.y - 10f, posOld.z);
        }

        // while (voxels haven't reached their target destination)
        while (traveled < travelDistance + 1)
        {
            mapTerrain[workingCoordX, 0, workingCoordZ].transform.Translate(translation * Time.deltaTime);
            workingCoordX++;
            traveled++;
        }
    }
    #endregion
}
