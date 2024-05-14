using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileDataMapping : MonoBehaviour
{
    [SerializeField]
    private Tilemap map;

    [SerializeField]
    private List<TileData> tileDatas;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = map.WorldToCell(mousePosition);

            TileBase clickedTile = map.GetTile(gridPosition);

            print("At position " + gridPosition + " there is a " + clickedTile);
        }
    }
}
