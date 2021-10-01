using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 learn from https://github.com/Shahbazcubix/oceanresearch
 */
public class GeoCDClipmap : MonoBehaviour
{
    public bool _scaleHoriz = true;
    public bool _scaleHorizSmoothTransition = true;
    [Delayed]
    [Tooltip("The scale of the ocean is clamped at this value to prevent the ocean being scaled too small when approached by the camera.")]
    public float _minScale = 8f;//128f;

    public float _maxScale = 256f;//-1f;

    [Header("Geometry Params")]
    [SerializeField]
    [Tooltip("Side dimension in quads of an ocean tile.")]
    float _baseVertDensity = 32f;
    [SerializeField]
    [Tooltip("Number of ocean tile scales/LODs to generate.")]
    int _lodCount = 5;
    [SerializeField]
    [Tooltip("Generate a wide strip of triangles at the outer edge to extend ocean to edge of view frustum")]
    bool _generateSkirt = true;

    [Tooltip("the target camera view point")]
    public Transform _viewpoint;


    float _viewerAltitudeLevelAlpha = 0f;
    public float ViewerAltitudeLevelAlpha { get { return _viewerAltitudeLevelAlpha; } }

    static GeoCDClipmap _instance;
    public static GeoCDClipmap Instance { get { return _instance != null ? _instance : (_instance = FindObjectOfType<GeoCDClipmap>()); } }

    GeoCDClipmapBuilder _oceanBuilder;
    public GeoCDClipmapBuilder Builder { get { return _oceanBuilder; } }

    void Start()
    {
        _instance = this;

        _oceanBuilder = GetComponent<GeoCDClipmapBuilder>();
        if(_oceanBuilder && _oceanBuilder._chunkPrefab)
        {
            _oceanBuilder.GenerateMesh(MakeBuildParams());
        }
    }

    void LateUpdate()
    {
        if(!_viewpoint)
            return;

        transform.position = _viewpoint.transform.position;

        // set global shader params
        Shader.SetGlobalVector("_OceanCenterPosWorld", transform.position);
        Shader.SetGlobalFloat("_Scale", Mathf.Abs(transform.lossyScale.x));

        // 0.4f is the "best" value when base mesh density is 8. Scaling down from there produces results similar to
        // hand crafted values which looked good when the ocean is flat.
        float _lodAlphaBlackPointFade = 0.4f / (_baseVertDensity / 8f);
        // We could calculate this in the shader, but we can save two subtractions this way.
        float _lodAlphaBlackPointWhitePointFade = 1f - _lodAlphaBlackPointFade - _lodAlphaBlackPointFade;
        Shader.SetGlobalFloat("_lodAlphaBlackPointFade", _lodAlphaBlackPointFade);
        Shader.SetGlobalFloat("_lodAlphaBlackPointWhitePointFade", _lodAlphaBlackPointWhitePointFade);

        // consider scale 
        // scale ocean mesh based on camera height to keep uniform detail
        const float HEIGHT_LOD_MUL = 1f; //0.0625f;
        float camY = Mathf.Abs(_viewpoint.position.y - transform.position.y);
        float level = camY * HEIGHT_LOD_MUL;
        level = Mathf.Max(level, _minScale);
        if (_maxScale != -1f) level = Mathf.Min(level, 1.99f * _maxScale);

        float l2 = Mathf.Log(level) / Mathf.Log(2f);
        float l2f = Mathf.Floor(l2);

        _viewerAltitudeLevelAlpha = _scaleHorizSmoothTransition ? l2 - l2f : 0f;

        float scale = Mathf.Pow(2f, l2f);

        transform.localScale = new Vector3(scale, 1f, scale);
    }

    GeoCDClipmapBuilder.Params MakeBuildParams()
    {
        GeoCDClipmapBuilder.Params parms = new GeoCDClipmapBuilder.Params();
        parms._baseVertDensity = _baseVertDensity;
        parms._lodCount = _lodCount;
        parms._generateSkirt = _generateSkirt;
        return parms;
    }
}
