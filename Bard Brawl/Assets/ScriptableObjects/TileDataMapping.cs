using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileDataMapping : MonoBehaviour
{
    [SerializeField]
    private Tilemap map;

    // Start is called before the first frame update
    void Start()
    {
        for (int x = map.cellBounds.xMin; x < map.cellBounds.xMax; x++)
        {
            TileBase tile = map.GetTile(new Vector3Int(x, 0, 0));
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
