using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClipmapMeshType
{
    NormalMesh,
    FilterMesh,
    TrimMesh
}

public abstract class ClipmapMeshData
{
    public ClipmapMeshData(int InInnerEdgeNum)
    {
        InnerEdgeNum = InInnerEdgeNum;
    }
    public Vector3[] MeshPoints;
    public int[] MeshIndices;
    public int InnerEdgeNum;// make sure the InnerEdgeNum is even

    protected float Step = 0.1f;

    protected virtual int GetIndex(int RowIndex, int ColIndex)
    {
        return RowIndex * (InnerEdgeNum + 1) + ColIndex;
    }

    public abstract void BuildMesh();
   
    public Mesh GetMesh(string name)
    {
        Mesh mesh = new Mesh();
        mesh.name = name;
        mesh.vertices = MeshPoints;
        mesh.triangles = MeshIndices;
        bool needsNormals = true;
        bool needsTangents = true;
        bool needsBounds = true;
        if (needsNormals)
        {
            mesh.RecalculateNormals();
        }
        if (needsTangents)
        {
            mesh.RecalculateTangents();
        }
        if (needsBounds)
        {
            mesh.RecalculateBounds();
        }
        Bounds bounds = mesh.bounds;
        bounds.size = new Vector3(bounds.size.x, 1.0f, bounds.size.z);
        mesh.bounds = bounds;

        return mesh;
    }
}

public class ClipmapNormalMeshData : ClipmapMeshData
{
    public ClipmapNormalMeshData(int InInnerEdgeNum)
        :base(InInnerEdgeNum)
    { }

    public override void BuildMesh()
    {
        Step = 1.0f / InnerEdgeNum;

        int ColumnNum = InnerEdgeNum + 1;
        int RowNum = InnerEdgeNum + 1;
        MeshPoints = new Vector3[ColumnNum * RowNum];
        // generate vertex points
        for (int Row = 0; Row < RowNum; Row++)
        {
            for (int Col = 0; Col < ColumnNum; Col++)
            {
                MeshPoints[Row * ColumnNum + Col] = new Vector3((Col - InnerEdgeNum / 2) * Step, 0, (Row - InnerEdgeNum / 2) * Step);
            }
        }

        // generate indices
        MeshIndices = new int[6 * (ColumnNum - 1) * (RowNum - 1)];
        int IndicesIndex = 0;
        for (int Row = 0; Row < (RowNum - 1); Row++)
        {
            for (int Col = 0; Col < (ColumnNum - 1); Col++)
            {
                if ((Row % 2 == 0 && Col % 2 == 0) || (Row % 2 == 1 && Col % 2 == 1))
                {
                    MeshIndices[IndicesIndex++] = GetIndex(Row, Col);
                    MeshIndices[IndicesIndex++] = GetIndex(Row + 1, Col);
                    MeshIndices[IndicesIndex++] = GetIndex(Row + 1, Col + 1);

                    MeshIndices[IndicesIndex++] = GetIndex(Row, Col);
                    MeshIndices[IndicesIndex++] = GetIndex(Row + 1, Col + 1);
                    MeshIndices[IndicesIndex++] = GetIndex(Row, Col + 1);
                }
                else
                {
                    MeshIndices[IndicesIndex++] = GetIndex(Row, Col);
                    MeshIndices[IndicesIndex++] = GetIndex(Row + 1, Col);
                    MeshIndices[IndicesIndex++] = GetIndex(Row, Col + 1);

                    MeshIndices[IndicesIndex++] = GetIndex(Row, Col + 1);
                    MeshIndices[IndicesIndex++] = GetIndex(Row + 1, Col);
                    MeshIndices[IndicesIndex++] = GetIndex(Row + 1, Col + 1);
                }
            }
        }
    }
}

public class ClipmapFilterMeshData : ClipmapMeshData
{
    public ClipmapFilterMeshData(int InInnerEdgeNum)
        : base(InInnerEdgeNum)
    { }

    public override void BuildMesh()
    {
        Step = 1.0f / InnerEdgeNum;

        int ColumnNum = InnerEdgeNum + 1;
        int RowNum = 2;
        MeshPoints = new Vector3[ColumnNum * RowNum];
        // generate vertex points
        for (int Row = 0; Row < RowNum; Row++)
        {
            for (int Col = 0; Col < ColumnNum; Col++)
            {
                MeshPoints[Row * ColumnNum + Col] = new Vector3((Col - InnerEdgeNum / 2) * Step, 0, (Row - 1.0f / 2) * Step);
            }
        }

        // generate indices
        MeshIndices = new int[6 * (ColumnNum - 1) * (RowNum - 1)];
        int IndicesIndex = 0;
        for (int Row = 0; Row < (RowNum - 1); Row++)
        {
            for (int Col = 0; Col < (ColumnNum - 1); Col++)
            {
                if ((Row % 2 == 0 && Col % 2 == 0) || (Row % 2 == 1 && Col % 2 == 1))
                {
                    MeshIndices[IndicesIndex++] = GetIndex(Row, Col);
                    MeshIndices[IndicesIndex++] = GetIndex(Row + 1, Col);
                    MeshIndices[IndicesIndex++] = GetIndex(Row, Col + 1);

                    MeshIndices[IndicesIndex++] = GetIndex(Row, Col + 1);
                    MeshIndices[IndicesIndex++] = GetIndex(Row + 1, Col);
                    MeshIndices[IndicesIndex++] = GetIndex(Row + 1, Col + 1);
                }
                else
                {
                    MeshIndices[IndicesIndex++] = GetIndex(Row, Col);
                    MeshIndices[IndicesIndex++] = GetIndex(Row + 1, Col + 1);
                    MeshIndices[IndicesIndex++] = GetIndex(Row, Col + 1);

                    MeshIndices[IndicesIndex++] = GetIndex(Row, Col);
                    MeshIndices[IndicesIndex++] = GetIndex(Row + 1, Col);
                    MeshIndices[IndicesIndex++] = GetIndex(Row + 1, Col + 1);
                }
            }
        }
    }
}


public class ClipmapTrimMeshLPatternData : ClipmapMeshData
{
    public ClipmapTrimMeshLPatternData(int InInnerEdgeNum)
        : base(InInnerEdgeNum)
    { }
    protected int GetTrimIndex(int RowIndex, int ColIndex, int ColumnNum)
    {
        return RowIndex * ColumnNum + ColIndex;
    }
    public override void BuildMesh()
    {
        Step = 1.0f / InnerEdgeNum;

        int ColumnNum = InnerEdgeNum + 1;
        int RowNum = 2;

        /*
                    *
                    *
                    *
             ********
         */
        int UpRowNum = 4 * InnerEdgeNum + 1 + 1;
        int UpColumn = 2;

        int DownRowNum = 2;
        int DownColumnNum = 4 * InnerEdgeNum + 1 + 1 + 1;

        MeshPoints = new Vector3[UpRowNum * UpColumn + DownRowNum * DownColumnNum];
        
        // generate vertex points
        int UpVerticeOffset = UpRowNum * UpColumn;
        for (int Row = 0; Row < UpRowNum; Row++)
        {
            for (int Col = 0; Col < UpColumn; Col++)
            {
                MeshPoints[Row * UpColumn + Col] = new Vector3((2.0f + Step) + Col * Step , 0, 2.0f - Row * Step);
            }
        }
        for (int Row = 0; Row < DownRowNum; Row++)
        {
            for (int Col = 0; Col < DownColumnNum; Col++)
            {
                MeshPoints[UpVerticeOffset + Row * DownColumnNum + Col] = new Vector3(-2.0f + Col * Step , 0, -(2.0f + Step) - Row * Step);
            }
        }

        // generate indices
        MeshIndices = new int[6 * (UpRowNum - 1) * (UpColumn - 1) + 6 * (DownRowNum - 1) * (DownColumnNum - 1)];
        int IndicesIndex = 0;
        for (int Row = 0; Row < (UpRowNum - 1); Row++)
        {
            for (int Col = 0; Col < (UpColumn - 1); Col++)
            {
                if ((Row % 2 == 0 && Col % 2 == 0) || (Row % 2 == 1 && Col % 2 == 1))
                {
                    MeshIndices[IndicesIndex++] = GetTrimIndex(Row, Col, 2);
                    MeshIndices[IndicesIndex++] = GetTrimIndex(Row, Col + 1, 2);
                    MeshIndices[IndicesIndex++] = GetTrimIndex(Row + 1, Col, 2);

                    MeshIndices[IndicesIndex++] = GetTrimIndex(Row, Col + 1, 2);
                    MeshIndices[IndicesIndex++] = GetTrimIndex(Row + 1, Col + 1, 2);
                    MeshIndices[IndicesIndex++] = GetTrimIndex(Row + 1, Col, 2);
                }
                else
                {
                    MeshIndices[IndicesIndex++] = GetTrimIndex(Row, Col, 2);
                    MeshIndices[IndicesIndex++] = GetTrimIndex(Row, Col + 1, 2);
                    MeshIndices[IndicesIndex++] = GetTrimIndex(Row + 1, Col + 1, 2);

                    MeshIndices[IndicesIndex++] = GetTrimIndex(Row, Col, 2);
                    MeshIndices[IndicesIndex++] = GetTrimIndex(Row + 1, Col + 1, 2);
                    MeshIndices[IndicesIndex++] = GetTrimIndex(Row + 1, Col, 2);
                }
            }
        }
        for (int Row = 0; Row < (DownRowNum - 1); Row++)
        {
            for (int Col = 0; Col < (DownColumnNum - 1); Col++)
            {
                if ((Row % 2 == 0 && Col % 2 == 0) || (Row % 2 == 1 && Col % 2 == 1))
                {
                    MeshIndices[IndicesIndex++] = UpVerticeOffset + GetTrimIndex(Row, Col, DownColumnNum);
                    MeshIndices[IndicesIndex++] = UpVerticeOffset + GetTrimIndex(Row, Col + 1, DownColumnNum);
                    MeshIndices[IndicesIndex++] = UpVerticeOffset + GetTrimIndex(Row + 1, Col, DownColumnNum);

                    MeshIndices[IndicesIndex++] = UpVerticeOffset + GetTrimIndex(Row, Col + 1, DownColumnNum);
                    MeshIndices[IndicesIndex++] = UpVerticeOffset + GetTrimIndex(Row + 1, Col + 1, DownColumnNum);
                    MeshIndices[IndicesIndex++] = UpVerticeOffset + GetTrimIndex(Row + 1, Col, DownColumnNum);
                }
                else
                {
                    MeshIndices[IndicesIndex++] = UpVerticeOffset + GetTrimIndex(Row, Col, DownColumnNum);
                    MeshIndices[IndicesIndex++] = UpVerticeOffset + GetTrimIndex(Row, Col + 1, DownColumnNum);
                    MeshIndices[IndicesIndex++] = UpVerticeOffset + GetTrimIndex(Row + 1, Col + 1, DownColumnNum);

                    MeshIndices[IndicesIndex++] = UpVerticeOffset + GetTrimIndex(Row, Col, DownColumnNum);
                    MeshIndices[IndicesIndex++] = UpVerticeOffset + GetTrimIndex(Row + 1, Col + 1, DownColumnNum);
                    MeshIndices[IndicesIndex++] = UpVerticeOffset + GetTrimIndex(Row + 1, Col, DownColumnNum);
                }
            }
        }
    }
}


public enum ENodeNoVisibleType
{
    LTNoVisible,
    LBNoVisible,
    RTNoVisible,
    RBNoVisible,
    None
};

public class ClipmapMeshNode
{
    public GameObject LT = null;
    public GameObject LB = null;
    public GameObject RT = null;
    public GameObject RB = null;

    public ClipmapMeshNode(int LODLevel, ClipmapMeshData clipmapMeshData, string nodename, GameObject ParentObj, Material TerrianMaterial, Vector3 Offset, ENodeNoVisibleType NoVisType)
    {
        float Scale = Mathf.Pow(2.0f, LODLevel);
        Vector3 CurScale = new Vector3(Scale, 1, Scale);
        if(NoVisType != ENodeNoVisibleType.LTNoVisible)
        {
            LT = new GameObject(nodename + "00");
            MeshFilter meshFilter = LT.AddComponent<MeshFilter>();
            meshFilter.mesh = clipmapMeshData.GetMesh(LT.name);
            LT.transform.parent = ParentObj.transform;
            LT.transform.localPosition = (new Vector3(-0.5f, 0, 0.5f)) * Mathf.Pow(2.0f, LODLevel) + Offset;
            LT.transform.localScale = CurScale;
            MeshRenderer meshRenderer = LT.AddComponent<MeshRenderer>();
            meshRenderer.material = TerrianMaterial;
        }
       
        if(NoVisType != ENodeNoVisibleType.LBNoVisible)
        {
            LB = new GameObject(nodename + "01");
            MeshFilter meshFilter = LB.AddComponent<MeshFilter>();
            meshFilter.mesh = clipmapMeshData.GetMesh(LB.name);
            LB.transform.parent = ParentObj.transform;
            LB.transform.localPosition = (new Vector3(-0.5f, 0, -0.5f)) * Mathf.Pow(2.0f, LODLevel) + Offset;
            LB.transform.localScale = CurScale;
            MeshRenderer meshRenderer = LB.AddComponent<MeshRenderer>();
            meshRenderer.material = TerrianMaterial;
        }
       
        if(NoVisType != ENodeNoVisibleType.RTNoVisible)
        {
            RT = new GameObject(nodename + "10");
            MeshFilter meshFilter = RT.AddComponent<MeshFilter>();
            meshFilter.mesh = clipmapMeshData.GetMesh(RT.name);
            RT.transform.parent = ParentObj.transform;
            RT.transform.localPosition = (new Vector3(0.5f, 0, 0.5f)) * Mathf.Pow(2.0f, LODLevel) + Offset;
            RT.transform.localScale = CurScale;
            MeshRenderer meshRenderer = RT.AddComponent<MeshRenderer>();
            meshRenderer.material = TerrianMaterial;
        }

        if (NoVisType != ENodeNoVisibleType.RBNoVisible)
        {
            RB = new GameObject(nodename + "11");
            MeshFilter meshFilter = RB.AddComponent<MeshFilter>();
            meshFilter.mesh = clipmapMeshData.GetMesh(RB.name);
            RB.transform.parent = ParentObj.transform;
            RB.transform.localPosition = (new Vector3(0.5f, 0, -0.5f)) * Mathf.Pow(2.0f, LODLevel) + Offset;
            RB.transform.localScale = CurScale;
            MeshRenderer meshRenderer = RB.AddComponent<MeshRenderer>();
            meshRenderer.material = TerrianMaterial;
        }
    }
}

public enum EClipmapFilterMeshType
{
    LT,
    RT,
    RB,
    LB
}

public class ClipmapFilterMeshNode
{
    public GameObject obj;
    public GameObject obj1;
    public ClipmapFilterMeshNode(int LODLevel, EClipmapFilterMeshType filterMeshType, 
        GameObject ParentObj, ClipmapMeshData clipmapFilterMeshData, GeometryClipmap GeoClipmap)
    {
        float Scale = Mathf.Pow(2.0f, LODLevel); 
        float BaseVertDensity = GeoClipmap.MeshVerticesDensity;
        if (filterMeshType == EClipmapFilterMeshType.LT)
        {
            obj = new GameObject("LOD-" + LODLevel + "-Filter-LT");
            obj.transform.parent = ParentObj.transform;
            obj.transform.localPosition = new Vector3(0.5f / BaseVertDensity, 0, 1.5f) * Mathf.Pow(2.0f, LODLevel);
            obj.transform.localRotation = Quaternion.Euler(0, 90, 0);
            obj.transform.localScale = new Vector3(Scale, 1, Scale);

            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = clipmapFilterMeshData.GetMesh(obj.name);
            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = GeoClipmap.ClipmapFilterMaterial;

            if(LODLevel == 0)
            {
                obj1 = new GameObject("LOD-" + LODLevel + "-Filter-LT1");
                obj1.transform.parent = ParentObj.transform;
                obj1.transform.localPosition = new Vector3(0.5f / BaseVertDensity, 0, 0.5f) * Mathf.Pow(2.0f, LODLevel);
                obj1.transform.localRotation = Quaternion.Euler(0, 90, 0);
                obj1.transform.localScale = new Vector3(Scale, 1, Scale);

                meshFilter = obj1.AddComponent<MeshFilter>();
                meshFilter.mesh = clipmapFilterMeshData.GetMesh(obj1.name);
                meshRenderer = obj1.AddComponent<MeshRenderer>();
                meshRenderer.material = GeoClipmap.ClipmapFilterMaterial;
            }
        }

        if(filterMeshType == EClipmapFilterMeshType.RT)
        {
            obj = new GameObject("LOD-" + LODLevel + "-Filter-RT");
            obj.transform.parent = ParentObj.transform;
            obj.transform.localPosition = new Vector3(1.5f + 1.0f / BaseVertDensity, 0, -0.5f / BaseVertDensity) * Mathf.Pow(2.0f, LODLevel);
            obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            obj.transform.localScale = new Vector3(Scale, 1, Scale);

            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = clipmapFilterMeshData.GetMesh(obj.name);
            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = GeoClipmap.ClipmapFilterMaterial;

            if (LODLevel == 0)
            {
                obj1 = new GameObject("LOD-" + LODLevel + "-Filter-RT1");
                obj1.transform.parent = ParentObj.transform;
                obj1.transform.localPosition = new Vector3(0.5f + 1.0f / BaseVertDensity, 0, -0.5f / BaseVertDensity) * Mathf.Pow(2.0f, LODLevel);
                obj1.transform.localRotation = Quaternion.Euler(0, 0, 0);
                obj1.transform.localScale = new Vector3(Scale, 1, Scale);

                meshFilter = obj1.AddComponent<MeshFilter>();
                meshFilter.mesh = clipmapFilterMeshData.GetMesh(obj1.name);
                meshRenderer = obj1.AddComponent<MeshRenderer>();
                meshRenderer.material = GeoClipmap.ClipmapFilterMaterial;
            }
        }

        if (filterMeshType == EClipmapFilterMeshType.RB)
        {
            obj = new GameObject("LOD-" + LODLevel + "-Filter-RB");
            obj.transform.parent = ParentObj.transform;
            obj.transform.localPosition = new Vector3(0.5f / BaseVertDensity, 0, -(1.5f + 1.0f / BaseVertDensity)) * Mathf.Pow(2.0f, LODLevel);
            obj.transform.localRotation = Quaternion.Euler(0, 90, 0);
            obj.transform.localScale = new Vector3(Scale, 1, Scale);

            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = clipmapFilterMeshData.GetMesh(obj.name);
            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = GeoClipmap.ClipmapFilterMaterial;

            if (LODLevel == 0)
            {
                obj1 = new GameObject("LOD-" + LODLevel + "-Filter-RB1");
                obj1.transform.parent = ParentObj.transform;
                obj1.transform.localRotation = Quaternion.Euler(0, 90, 0);
                obj1.transform.localPosition = new Vector3(0.5f / BaseVertDensity, 0, -(0.5f + 1.0f / BaseVertDensity)) * Mathf.Pow(2.0f, LODLevel);
                obj1.transform.localScale = new Vector3(Scale, 1, Scale);

                meshFilter = obj1.AddComponent<MeshFilter>();
                meshFilter.mesh = clipmapFilterMeshData.GetMesh(obj1.name);
                meshRenderer = obj1.AddComponent<MeshRenderer>();
                meshRenderer.material = GeoClipmap.ClipmapFilterMaterial;
            }
        }

        if (filterMeshType == EClipmapFilterMeshType.LB)
        {
            obj = new GameObject("LOD-" + LODLevel + "-Filter-LB");
            obj.transform.parent = ParentObj.transform;
            obj.transform.localPosition = new Vector3(-1.5f, 0, -0.5f / BaseVertDensity) * Mathf.Pow(2.0f, LODLevel);
            obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            obj.transform.localScale = new Vector3(Scale, 1, Scale);

            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.mesh = clipmapFilterMeshData.GetMesh(obj.name);
            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.material = GeoClipmap.ClipmapFilterMaterial;

            if (LODLevel == 0)
            {
                obj1 = new GameObject("LOD-" + LODLevel + "-Filter-LB1");
                obj1.transform.parent = ParentObj.transform;
                obj1.transform.localPosition = new Vector3(-0.5f, 0, -0.5f / BaseVertDensity) * Mathf.Pow(2.0f, LODLevel);
                obj1.transform.localRotation = Quaternion.Euler(0, 0, 0);
                obj1.transform.localScale = new Vector3(Scale, 1, Scale);

                meshFilter = obj1.AddComponent<MeshFilter>();
                meshFilter.mesh = clipmapFilterMeshData.GetMesh(obj1.name);
                meshRenderer = obj1.AddComponent<MeshRenderer>();
                meshRenderer.material = GeoClipmap.ClipmapFilterMaterial;
            }

        }

    }
}


public class ClipmapTrimLMeshNode
{
    public GameObject obj;
    public ClipmapTrimLMeshNode(int LODLevel, GameObject ParentObj, ClipmapMeshData clipmapFilterMeshData, GeometryClipmap GeoClipmap)
    {
        float Scale = Mathf.Pow(2.0f, LODLevel);
        float BaseVertDensity = GeoClipmap.MeshVerticesDensity;

        obj = new GameObject("LOD-" + LODLevel + "-Trim");
        obj.transform.parent = ParentObj.transform;
        obj.transform.localPosition = Vector3.zero;// new Vector3(0.5f / BaseVertDensity, 0, 1.5f) * Mathf.Pow(2.0f, LODLevel);
        obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
        obj.transform.localScale = new Vector3(Scale, 1, Scale);

        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        meshFilter.mesh = clipmapFilterMeshData.GetMesh(obj.name);
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.material = GeoClipmap.ClipmapTrimLMaterial;
    }
}

public class ClipmapLevelData
{
    public ClipmapMeshNode LT = null;
    public ClipmapMeshNode LB = null;
    public ClipmapMeshNode RT = null;
    public ClipmapMeshNode RB = null;

    public ClipmapFilterMeshNode LTFilterNode = null;
    public ClipmapFilterMeshNode RTFilterNode = null;
    public ClipmapFilterMeshNode RBFilterNode = null;
    public ClipmapFilterMeshNode LBFilterNode = null;

    public ClipmapTrimLMeshNode TrimMeshNode = null;

    public GameObject LevelObj;

    private int CurLODLevel = 0;
    private int MaxLODLvel = 0;
    private float CurBaseVertStep = 0;
    private Vector3 LevelObjLocationPosition = Vector3.zero;

    public ClipmapLevelData(int LODLevel, ClipmapMeshData clipmapMeshData, ClipmapMeshData clipmapFilterMeshData, ClipmapMeshData ClipmapTrimMeshLPatternData, GameObject ParentObj, GeometryClipmap GeoClipmap, ClipmapLevelData PreLevelClipmap)
    {
        CurLODLevel = LODLevel;
        MaxLODLvel = GeoClipmap.LODNum;
        float BaseVertDensity = GeoClipmap.MeshVerticesDensity;
        float BaseVertStep = 1.0f / BaseVertDensity;
        CurBaseVertStep = BaseVertStep;

        float scale = Mathf.Pow(2, LODLevel);
        LevelObj = new GameObject("LOD-" + LODLevel);
        LevelObj.transform.parent = ParentObj.transform;

        GameObject TileObject = new GameObject("LOD-" + LODLevel + "-Tile");
        TileObject.transform.parent = LevelObj.transform;

        // Create Normal Node
        GameObject LTObj = new GameObject("LOD-" + LODLevel + "-LT");
        LTObj.transform.parent = TileObject.transform;
        Vector3 Offset = new Vector3(-1.0f, 0.0f, 1.0f) * Mathf.Pow(2.0f, LODLevel);
        LT = new ClipmapMeshNode(LODLevel, clipmapMeshData, "LT", LTObj, GeoClipmap.ClipmapMaterial, Offset, LODLevel == 0 ? ENodeNoVisibleType.None : ENodeNoVisibleType.RBNoVisible);

        
        GameObject LBObj = new GameObject("LOD-" + LODLevel + "-LB");
        LBObj.transform.parent = TileObject.transform;
        Offset = new Vector3(-1.0f, 0.0f, -1.0f) * Mathf.Pow(2.0f, LODLevel);
        // Consider Filter Mesh Offset
        Offset += new Vector3(0, 0, -1.0f / BaseVertDensity) * Mathf.Pow(2.0f, LODLevel);
        LB = new ClipmapMeshNode(LODLevel, clipmapMeshData, "LB", LBObj, GeoClipmap.ClipmapMaterial, Offset, LODLevel == 0 ? ENodeNoVisibleType.None : ENodeNoVisibleType.RTNoVisible);


        GameObject RTObj = new GameObject("LOD-" + LODLevel + "-RT");
        RTObj.transform.parent = TileObject.transform;
        Offset = new Vector3( 1.0f, 0.0f, 1.0f) * Mathf.Pow(2.0f, LODLevel);
        // Consider Filter Mesh Offset
        Offset += new Vector3(1.0f / BaseVertDensity, 0, 0) * Mathf.Pow(2.0f, LODLevel);
        RT = new ClipmapMeshNode(LODLevel, clipmapMeshData, "RT", RTObj, GeoClipmap.ClipmapMaterial, Offset, LODLevel == 0 ? ENodeNoVisibleType.None : ENodeNoVisibleType.LBNoVisible);
        
        GameObject RBObj = new GameObject("LOD-" + LODLevel + "-RB");

        RBObj.transform.parent = TileObject.transform;
        Offset = new Vector3(1.0f, 0.0f, -1.0f) * Mathf.Pow(2.0f, LODLevel);
        // Consider Filter Mesh Offset
        Offset += new Vector3(0, 0, -1.0f / BaseVertDensity) * Mathf.Pow(2.0f, LODLevel); //
        Offset += new Vector3(1.0f / BaseVertDensity, 0, 0) * Mathf.Pow(2.0f, LODLevel); //

        RB = new ClipmapMeshNode(LODLevel, clipmapMeshData, "RB", RBObj, GeoClipmap.ClipmapMaterial, Offset, LODLevel == 0 ? ENodeNoVisibleType.None : ENodeNoVisibleType.LTNoVisible);

        // Create Filter Node

        LTFilterNode = new ClipmapFilterMeshNode(LODLevel, EClipmapFilterMeshType.LT, TileObject, clipmapFilterMeshData, GeoClipmap);

        RTFilterNode = new ClipmapFilterMeshNode(LODLevel, EClipmapFilterMeshType.RT, TileObject, clipmapFilterMeshData, GeoClipmap);

        RBFilterNode = new ClipmapFilterMeshNode(LODLevel, EClipmapFilterMeshType.RB, TileObject, clipmapFilterMeshData, GeoClipmap);

        LBFilterNode = new ClipmapFilterMeshNode(LODLevel, EClipmapFilterMeshType.LB, TileObject, clipmapFilterMeshData, GeoClipmap);

        // Create TrimMesh L Node
        TrimMeshNode = new ClipmapTrimLMeshNode(LODLevel, LevelObj, ClipmapTrimMeshLPatternData, GeoClipmap);

        // Update Position because of Trim Mesh Rotate
    
        TrimMeshNode.obj.transform.localPosition = new Vector3(0, 0, -BaseVertStep) * Mathf.Pow(2.0f, LODLevel);
        TrimMeshNode.obj.transform.localRotation = Quaternion.Euler(0, 270, 0);

        LevelObj.transform.localPosition = -TrimMeshNode.obj.transform.localPosition;


        LevelObjLocationPosition = LevelObj.transform.localPosition;
    }

    
    public void UpdatePosition(Vector3 cameraPos)
    {
        float Scale = Mathf.Pow(2.0f, CurLODLevel) * CurBaseVertStep;
        float NextScale = 2.0f * Scale;
        Vector3 CurSnappedPos = new Vector3(Mathf.Floor(cameraPos.x / Scale), 0, Mathf.Floor(cameraPos.z / Scale)) * Scale;
        Vector3 NextSnappedPos = new Vector3(Mathf.Floor(cameraPos.x / NextScale), 0, Mathf.Floor(cameraPos.z / NextScale)) * NextScale;

        Vector3 diff = cameraPos - NextSnappedPos;
        int rotatePattern = 0;
        float Threshold = 0.001f;

        rotatePattern |= (diff.x + Threshold) >= Scale ? 0 : 2;
        rotatePattern |= (diff.z + Threshold) >= Scale ? 0 : 1;

        if(CurLODLevel < MaxLODLvel - 1)
        {
            if (rotatePattern == 0)
            {
                // rotate 90
                TrimMeshNode.obj.transform.localRotation = Quaternion.Euler(0, 90, 0);
                TrimMeshNode.obj.transform.localPosition = new Vector3(CurBaseVertStep, 0, 0) * Mathf.Pow(2.0f, CurLODLevel);
            }
            if (rotatePattern == 1)
            {
                // rotate 180
                TrimMeshNode.obj.transform.localRotation = Quaternion.Euler(0, 180, 0);
                TrimMeshNode.obj.transform.localPosition = new Vector3(CurBaseVertStep, 0, -CurBaseVertStep) * Mathf.Pow(2.0f, CurLODLevel);
            }
            if (rotatePattern == 2)
            {
                // rotate 0
                TrimMeshNode.obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                TrimMeshNode.obj.transform.localPosition = new Vector3(0, 0, 0) * Mathf.Pow(2.0f, CurLODLevel);

            }
            if (rotatePattern == 3)
            {
                // rotate 270
                TrimMeshNode.obj.transform.localRotation = Quaternion.Euler(0, 270, 0);
                TrimMeshNode.obj.transform.localPosition = new Vector3(0, 0, -CurBaseVertStep) * Mathf.Pow(2.0f, CurLODLevel);
            }
        }
        LevelObj.transform.localPosition = LevelObjLocationPosition + CurSnappedPos;
    }
}


public class GeometryClipmap : MonoBehaviour
{
    public int LODNum = 5;
    public int MeshVerticesDensity = 16; // edge section number
    public Material ClipmapMaterial;
    public Material ClipmapFilterMaterial;
    public Material ClipmapTrimLMaterial;

    //
    public GameObject DebugTargetObject;

    private ClipmapNormalMeshData clipmapNormalMeshData;
    private ClipmapFilterMeshData clipmapFilterMeshData;
    private ClipmapTrimMeshLPatternData clipmapTrimMeshLPatternData;

    private List<ClipmapLevelData> clipmapLevelDatas;
    // Start is called before the first frame update
    void Start()
    {
        clipmapNormalMeshData = new ClipmapNormalMeshData(MeshVerticesDensity);
        clipmapNormalMeshData.BuildMesh();

        clipmapFilterMeshData = new ClipmapFilterMeshData(MeshVerticesDensity);
        clipmapFilterMeshData.BuildMesh();

        clipmapTrimMeshLPatternData = new ClipmapTrimMeshLPatternData(MeshVerticesDensity);
        clipmapTrimMeshLPatternData.BuildMesh();

        clipmapLevelDatas = new List<ClipmapLevelData>(LODNum);
        for (int i = 0; i < LODNum; i++)
        {
            if(i == 0)
            {
                clipmapLevelDatas.Add(new ClipmapLevelData(i, clipmapNormalMeshData, clipmapFilterMeshData, clipmapTrimMeshLPatternData, this.gameObject, this, null));
            }
            else
            {
                clipmapLevelDatas.Add(new ClipmapLevelData(i, clipmapNormalMeshData, clipmapFilterMeshData, clipmapTrimMeshLPatternData, this.gameObject, this, clipmapLevelDatas[i - 1]));
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        float BaseVertStep = 1.0f / MeshVerticesDensity;
        if (DebugTargetObject)
        {
            for(int i = 0; i < clipmapLevelDatas.Count; i++)
            {
                clipmapLevelDatas[i].UpdatePosition(DebugTargetObject.transform.position);
            }

        }
    }
}
