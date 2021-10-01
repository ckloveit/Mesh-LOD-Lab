using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoCDClipmapChunkRenderer : MonoBehaviour
{
    int _lodIndex = -1;
    int _totalLodCount = -1;
    float _baseVertDensity = 32f;
    Renderer _thisRend;

    // Start is called before the first frame update
    void Start()
    {
        _thisRend = GetComponent<Renderer>();
    }

    // Called when visible to a camera
    void OnWillRenderObject()
    {
        // per instance data

        // blend closest geometry in/out to avoid pop
        float meshScaleLerp = _lodIndex == 0 ? GeoCDClipmap.Instance.ViewerAltitudeLevelAlpha : 0f;
        // blend furthest normals scale in/out to avoid pop
        float farNormalsWeight = _lodIndex == _totalLodCount - 1 ? GeoCDClipmap.Instance.ViewerAltitudeLevelAlpha : 1f;
        _thisRend.material.SetVector("_InstanceData", new Vector4(meshScaleLerp, farNormalsWeight, _lodIndex));

        // geometry data
        float squareSize = Mathf.Abs(transform.lossyScale.x) / _baseVertDensity;
        float normalScrollSpeed0 = Mathf.Log(1f + 2f * squareSize) * 1.875f;
        float normalScrollSpeed1 = Mathf.Log(1f + 4f * squareSize) * 1.875f;
        _thisRend.material.SetVector("_GeomData", new Vector4(squareSize, normalScrollSpeed0, normalScrollSpeed1, _baseVertDensity));
        _thisRend.material.SetVector("_GeomDataV2", new Vector4(transform.lossyScale.x, transform.lossyScale.x /*normalScrollSpeed0*/, normalScrollSpeed1, _baseVertDensity));

    }
    
    public void SetInstanceData(int lodIndex, int totalLodCount, float baseVertDensity)
    {
        _lodIndex = lodIndex; _totalLodCount = totalLodCount; _baseVertDensity = baseVertDensity;
    }
}
