using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DataTile : Tile
{
    public short markerMask;
    public GameObject[] prefabs;


#if UNITY_EDITOR
    [MenuItem("Assets/Create/2D/Custom Tiles/DataTile")]
    public static void CreateDataTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save DataTile", "New DataTile", "Asset", "Save DataTile", "Assets");
        if (path == "") return;

        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<DataTile>(), path);
    }
#endif
}
