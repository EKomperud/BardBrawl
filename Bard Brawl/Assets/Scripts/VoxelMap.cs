using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // map markers

    // ground
    // water
    // 
    // 
    // 
    // 
    // 
    // 
    // 
    // 
    // 
    // 
    // 
    // 
    // 
    // 

    short[,,] map;

    [SerializeField]
    short minX;
    [SerializeField]
    short maxX;
    [SerializeField]
    short minY;
    [SerializeField]
    short maxY;
    [SerializeField]
    short minZ;
    [SerializeField]
    short maxZ;

    // Start is called before the first frame update
    void Start()
    {
        for (short i = minX; i<= maxX; i++) 
        {
            for (short j = minY; j<= maxY; j++)
            {
                for (short k = minZ; k<= maxZ; k++)
                {
                    map[i, j, k] = 0;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
