using UnityEngine;

public class UV_Randomizer : MonoBehaviour
{
    private Material mat;

    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        mat.mainTextureOffset = new Vector2(0, Random.Range(0, 2048));
    }
}
