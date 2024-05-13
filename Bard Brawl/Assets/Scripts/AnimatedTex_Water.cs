using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedTex_Water : MonoBehaviour
{
    private MeshRenderer r;
    private Material m;
    private float offset;
    private float t;

    [SerializeField]
    float fps;

    [SerializeField]
    short frames;

    float offsetConstant;
    float spf;

    // Start is called before the first frame update
    void Start()
    {
        r = GetComponent<MeshRenderer>();
        m = r.sharedMaterial;
        spf = 1 / fps;
        offsetConstant = 1f / (float)frames;
        offset = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;

        if (t >= spf)
        {
            offset += offsetConstant;
            if (Mathf.Approximately(offset, 1))
            {
                offset = 0;
            }
            m.mainTextureOffset = new Vector2(offset, 0f);

            t -= spf;
        }
    }
}
