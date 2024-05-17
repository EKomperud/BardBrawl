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

        IEnumerator buildMap = BuildMap_PopupFromBottom();
        StartCoroutine(buildMap);
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
            Transform terrainType = terrainCollection.transform.GetChild(i);
            {
                for (int j = 0; j < terrainType.transform.childCount; j++)
                {
                    Transform voxel = terrainType.transform.GetChild(j);
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
            }
        }

        // Store data for all Prop voxels
        mapProps = new VoxelData[sizeX, sizeY, sizeZ];
        for (int i = 0; i < propsCollection.transform.childCount; i++)
        {
            Transform propType = propsCollection.transform.GetChild(i);
            {
                for (int j = 0; j < propType.transform.childCount; j++)
                {
                    Transform voxel = propType.transform.GetChild(j);
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
        }
    }

    IEnumerator BuildMap_PopupFromBottom()
    {
        int isEven = 1 - (sizeX & 1);
        int startingCoord = (sizeX / 2) - isEven;
        int travelDistance = isEven; //increments by 2 each loop
        Vector3 translation = new Vector3(0.0f, 0.0f, 10f);
        float loweringDistance = 4f;
        float loweringVariance = 0.5f;
        OneWayParabola[,,] travelTrajectories = new OneWayParabola[sizeX, sizeY, sizeZ];


        // Lower voxels in rings, and set up their trajectories
        float xCoefficient = 4f;
        float xShift = 2f;
        float yShift = 0.5f;
        bool negative = true;
        float yLimit = 0f;
        while (startingCoord >= 0)
        {
            int i = 0;
            int k = 0;
            for (i = 0; i < travelDistance; i++)
            {
                VoxelData voxel = mapTerrain[startingCoord + i, 0, startingCoord + k];
                Vector3 posOld = voxel.transform.localPosition;
                OneWayParabola p = new OneWayParabola(xCoefficient, xShift, yShift, negative, yLimit);
                travelTrajectories[startingCoord + i, 0, startingCoord + k] = p;
                voxel.transform.localPosition = new Vector3(posOld.x, p.SetTime(0f), posOld.z);
                //voxel.transform.localPosition = new Vector3(posOld.x, posOld.y - (loweringDistance + (travelDistance * loweringVariance)), posOld.z);
            }
            for (k = 0; k < travelDistance; k++)
            {
                VoxelData voxel = mapTerrain[startingCoord + i, 0, startingCoord + k];
                Vector3 posOld = voxel.transform.localPosition;
                OneWayParabola p = new OneWayParabola(xCoefficient, xShift, yShift, negative, yLimit);
                travelTrajectories[startingCoord + i, 0, startingCoord + k] = p;
                voxel.transform.localPosition = new Vector3(posOld.x, p.SetTime(0f), posOld.z);
                //voxel.transform.localPosition = new Vector3(posOld.x, posOld.y - (loweringDistance + (travelDistance * loweringVariance)), posOld.z);
            }
            for (i = travelDistance; i > 0; i--)
            {
                VoxelData voxel = mapTerrain[startingCoord + i, 0, startingCoord + k];
                Vector3 posOld = voxel.transform.localPosition;
                OneWayParabola p = new OneWayParabola(xCoefficient, xShift, yShift, negative, yLimit);
                travelTrajectories[startingCoord + i, 0, startingCoord + k] = p;
                voxel.transform.localPosition = new Vector3(posOld.x, p.SetTime(0f), posOld.z);
                // voxel.transform.localPosition = new Vector3(posOld.x, posOld.y - (loweringDistance + (travelDistance * loweringVariance)), posOld.z);
            }
            for (k = travelDistance; k > 0; k--)
            {
                VoxelData voxel = mapTerrain[startingCoord + i, 0, startingCoord + k];
                Vector3 posOld = voxel.transform.localPosition;
                OneWayParabola p = new OneWayParabola(xCoefficient, xShift, yShift, negative, yLimit);
                travelTrajectories[startingCoord + i, 0, startingCoord + k] = p;
                voxel.transform.localPosition = new Vector3(posOld.x, p.SetTime(0f), posOld.z);
                //voxel.transform.localPosition = new Vector3(posOld.x, posOld.y - (loweringDistance + (travelDistance * loweringVariance)), posOld.z);
            }
            travelDistance += 2;
            startingCoord--;

            xShift += 0.4f;
            yShift += 0.25f;
            //yLimit += 1f;
        }

        // Raised voxels once per frames, until they are at their original positions
        //bool finished = false;
        //while(!finished)
        //{
        //    finished = true;
        //    foreach (VoxelData voxel in mapTerrain)
        //    {
        //        if (voxel != null)
        //        {
        //            Vector3 posOld = voxel.transform.localPosition;
        //            if (posOld.y < 0f)
        //            {
        //                voxel.transform.Translate(translation * Time.deltaTime);
        //                if (voxel.transform.localPosition.y > 0f)
        //                {
        //                    voxel.transform.localPosition = new Vector3(posOld.x, 0, posOld.z);
        //                }
        //                finished = false;
        //            }
        //        }
        //    }
        //    yield return null;
        //}

        bool f = false;
        float t = 0f;
        while (!f)
        {
            f = true;
            t += Time.deltaTime;
            for (int i = 0; i < sizeX; i++)
            {
                for (int k = 0; k < sizeZ; k++)
                {
                    VoxelData v = mapTerrain[i, 0, k];
                    if (v != null)
                    {
                        Vector3 posOld = v.transform.localPosition;
                        v.transform.localPosition = new Vector3(posOld.x, travelTrajectories[i, 0, k].SetTime(t), posOld.z);
                        if (!travelTrajectories[i, 0, k].CheckAtLimit())
                        {
                            f = false;
                        }
                        //if (Mathf.Approximately(posOld.y, ))
                        //{
                        //    v.transform.Translate(translation * Time.deltaTime);
                        //    if (v.transform.localPosition.y > 0f)
                        //    {
                        //        v.transform.localPosition = new Vector3(posOld.x, 0, posOld.z);
                        //    }
                        //    finished = false;
                        //}
                    }
                }
            }
            yield return null;
        }

        yield break;
    }
    #endregion
}
