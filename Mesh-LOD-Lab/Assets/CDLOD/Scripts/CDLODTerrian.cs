//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;


//public struct MapDimensions
//{
//    public float MinX;
//    public float MinY;
//    public float MinZ;
//    public float SizeX;
//    public float SizeY;
//    public float SizeZ;
     
//    public float MaxX() { return MinX + SizeX; }
//    public float MaxY() { return MinY + SizeY; }
//    public float MaxZ() { return MinZ + SizeZ; }
//};

//public class CDLODRendererBatchInfo
//{
//    public CDLODQuadTree.LODSelection CDLODSelection;
//    public int MeshGridDimensions;



//    public int FilterLODLevel;         // only render quad if it is of FilterLODLevel; if -1 then render all
//}


//public class CDLODTerrian : MonoBehaviour
//{
//    public struct Settings
//    {
//        // [DLOD]
//        public int LeafQuadTreeNodeSize;
//        public int RenderGridResolutionMult;
//        public int LODLevelCount;

//        // [Rendering]
//        public float MinViewRange;
//        public float MaxViewRange;
//        public float LODLevelDistanceRatio;

//    };

//    MapDimensions m_MapDims;
//    Settings m_Settings;

//    int m_RasterWidth;
//    int m_RasterHeight;

//    int m_MaxRenderGridResolutionMult;

//    int m_TerrainGridMeshDims;

//    CDLODQuadTree m_terrainQuadTree;
//    [SerializeField]
//    public GameObject TargetObject;
//    public Vector3 GetTargetPosition()
//    {
//        if (TargetObject)
//        {
//            return TargetObject.transform.position;
//        }
//        else
//        {
//            return Vector3.zero;
//        }
//    }
//    private void Initialize()
//    {
//        m_MapDims.MinX = -3500.0f;
//        m_MapDims.MinY = 0;
//        m_MapDims.MinZ = -2550.0f;
//        m_MapDims.SizeX = 7000.0f;
//        m_MapDims.SizeY = 0;
//        m_MapDims.SizeZ = 5110.0f;

//        m_RasterWidth = 701;
//        m_RasterHeight = 512;

//        //CDLOD
//        m_Settings.LeafQuadTreeNodeSize     = 8;
//        m_Settings.RenderGridResolutionMult = 1;
//        m_Settings.LODLevelCount = 5;

//        m_MaxRenderGridResolutionMult = 1;
//        while (m_MaxRenderGridResolutionMult * m_Settings.LeafQuadTreeNodeSize <= 128) m_MaxRenderGridResolutionMult *= 2;
//        m_MaxRenderGridResolutionMult /= 2;

//        m_Settings.MinViewRange = 50.00f;
//        m_Settings.MaxViewRange = 500.00f;
//        m_Settings.LODLevelDistanceRatio = 2.0f;

//        m_TerrainGridMeshDims = m_Settings.LeafQuadTreeNodeSize * m_Settings.RenderGridResolutionMult;

//    }
//    void InitializeRuntimeData()
//    {
//        CDLODQuadTree.CreateDesc createDesc = new CDLODQuadTree.CreateDesc();
//        createDesc.LeafRenderNodeSize = m_Settings.LeafQuadTreeNodeSize;
//        createDesc.LODLevelCount = m_Settings.LODLevelCount;
//        createDesc.MapDims = m_MapDims;
//        createDesc.RasterSizeX = m_RasterWidth;
//        createDesc.RasterSizeY = m_RasterHeight;

//        m_terrainQuadTree = new CDLODQuadTree();
//        m_terrainQuadTree.Create(createDesc);



//    }

//    // Start is called before the first frame update
//    void Start()
//    {
//        Initialize();
//        InitializeRuntimeData();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        //cdlodSelection.
//    }

//    private void RenderTerrian()
//    {   
//        //////////////////////////////////////////////////////////////////////////
//        // Do the terrain LOD selection based on our camera
//        //
//        // this will store the selection of terrain quads that we want to render 
//        CDLODQuadTree.LODSelectionOnStack cdlodSelection = new CDLODQuadTree.LODSelectionOnStack(4096, GetTargetPosition(), 50.000f * 0.95f, m_Settings.LODLevelDistanceRatio);
//        //
//        // do the main selection process...
//        m_terrainQuadTree.LODSelect(cdlodSelection);

//        CDLODRendererBatchInfo cdlodBatchInfo = new CDLODRendererBatchInfo();
//        cdlodBatchInfo.MeshGridDimensions = m_TerrainGridMeshDims;
//        cdlodBatchInfo.CDLODSelection = cdlodSelection;

//        // Render
//        //
//        for (int i = cdlodSelection.GetMinSelectedLevel(); i <= cdlodSelection.GetMaxSelectedLevel(); i++)
//        {
//            cdlodBatchInfo.FilterLODLevel = i;



//        }
//    }
//}
