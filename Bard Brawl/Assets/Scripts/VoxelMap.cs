using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMap : MonoBehaviour
{
    public enum MapMarkers
    {
        Open = 0,
        Ground = 1,
        Water = 2,
        ObstacleClimbable = 4,
        ObstacleHoppable = 8,
        ObstacleMovable = 16,
        ObstacleBarrier = 32,
        Stairs = 64
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
