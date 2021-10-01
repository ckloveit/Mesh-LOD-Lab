using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDLODMeshData
{
    public CDLODMeshData(int InInnerEdgeNum)
    {
        InnerEdgeNum = InInnerEdgeNum;
    }
    public Vector3[] MeshPoints;
    public int[] MeshIndices;
    public int InnerEdgeNum;// make sure the InnerEdgeNum is even

    private float Step = 0.1f;

    public float GetMeshExtent()
    {
        return InnerEdgeNum * Step;
    }

    public float GetMeshHalfExtent()
    {
        return InnerEdgeNum * Step * 0.5f;
    }

    private int GetIndex(int RowIndex, int ColIndex)
    {
        return RowIndex * (InnerEdgeNum + 1) + ColIndex;
    }
    
    public void BuildMesh()
    {
        MeshPoints = new Vector3[(InnerEdgeNum + 1) * (InnerEdgeNum + 1)];
        Step = 1.0f / InnerEdgeNum;
        // generate vertex points
        for (int Row = 0; Row < InnerEdgeNum + 1; Row++)
        {
            for (int Col = 0; Col < InnerEdgeNum + 1; Col++)
            {
                //MeshPoints[Row * (InnerEdgeNum + 1) + Col] = new Vector3(Col * 1.0f / InnerEdgeNum, 0, Row * 1.0f / InnerEdgeNum);
                MeshPoints[Row * (InnerEdgeNum + 1) + Col] = new Vector3((Col - InnerEdgeNum / 2.0f) * Step, 0, (Row - InnerEdgeNum / 2.0f) * Step);
            }
        }

        // generate indices
        MeshIndices = new int[6 * InnerEdgeNum * InnerEdgeNum];
        int IndicesIndex = 0;
        for (int Row = 0; Row < InnerEdgeNum; Row++)
        {
            for (int Col = 0; Col < InnerEdgeNum; Col++)
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
        return mesh;
    }
}


public class CDLODSectionNode
{
    public GameObject Obj;

    public int LodIndex = 0;
    public float Scale = 1.0f;

    private CDLODRender cdLodRender;
    private Vector3 LocalPosition;

    public CDLODSectionNode SubTLNode = null;
    public CDLODSectionNode SubTRNode = null;
    public CDLODSectionNode SubBLNode = null;
    public CDLODSectionNode SubBRNode = null;

    public CDLODSectionNode(int NodeX, int NodeY, int InLodIndex, Transform ParentTransform , CDLODRender InCDLODRender, Vector3 InLocalPosition, 
        float InScale = 1.0f, float InLocalScale = 1.0f)
    {
        Obj = new GameObject("Mesh_" + InLodIndex + "_"+ NodeX + "X" + NodeY);
        Obj.AddComponent<MeshFilter>();
        Obj.AddComponent<MeshRenderer>();
        Obj.transform.parent = ParentTransform;
        Obj.transform.localPosition = InLocalPosition;
        Obj.transform.localScale = Vector3.one * InLocalScale;

        cdLodRender = InCDLODRender;
        LodIndex = InLodIndex;
        LocalPosition = InLocalPosition;
        Scale = InScale;

        Obj.GetComponent<MeshRenderer>().material = new Material(InCDLODRender.WireframeMaterial);
        Obj.GetComponent<MeshFilter>().mesh = InCDLODRender.CDLODRawMeshData.GetMesh(Obj.name);
    }

    void DestroyChildNode()
    {
        if (SubTLNode != null) GameObject.Destroy(SubTLNode.Obj);
        if (SubTRNode != null) GameObject.Destroy(SubTRNode.Obj);
        if (SubBLNode != null) GameObject.Destroy(SubBLNode.Obj);
        if (SubBRNode != null) GameObject.Destroy(SubBRNode.Obj);
    }

    public void GenerateLODNode()
    {
        //DestroyChildNode();
        if (cdLodRender.TargetObject && cdLodRender.LodDistances.Length > 0)
        {
           if(LodIndex < cdLodRender.LodDistances.Length - 1)
           {
                SplitNode();

                // childnode update
                SubTLNode.GenerateLODNode();
                SubTRNode.GenerateLODNode();
                SubBLNode.GenerateLODNode();
                SubBRNode.GenerateLODNode();
            }
        }
    }

    void SplitNode()
    {
        Vector3 CurPos = Vector3.zero;
        Vector3 ParentLocation = Vector3.zero;// LocalPosition;
        CurPos.x = ParentLocation.x - 0.25f * cdLodRender.CDLODRawMeshData.GetMeshExtent();
        CurPos.y = ParentLocation.y;
        CurPos.z = ParentLocation.z - 0.25f * cdLodRender.CDLODRawMeshData.GetMeshExtent();
        SubTLNode = new CDLODSectionNode(0, 0, LodIndex + 1, Obj.transform ,cdLodRender, CurPos, 0.5f * Scale, 0.5f);

        CurPos.x = ParentLocation.x + 0.25f * cdLodRender.CDLODRawMeshData.GetMeshExtent();
        CurPos.y = ParentLocation.y;
        CurPos.z = ParentLocation.z - 0.25f * cdLodRender.CDLODRawMeshData.GetMeshExtent();
        SubTRNode = new CDLODSectionNode(1, 0, LodIndex + 1, Obj.transform, cdLodRender, CurPos, 0.5f * Scale, 0.5f);

        CurPos.x = ParentLocation.x - 0.25f * cdLodRender.CDLODRawMeshData.GetMeshExtent();
        CurPos.y = ParentLocation.y;
        CurPos.z = ParentLocation.z + 0.25f * cdLodRender.CDLODRawMeshData.GetMeshExtent();
        SubBLNode = new CDLODSectionNode(1, 0, LodIndex + 1, Obj.transform, cdLodRender, CurPos, 0.5f * Scale, 0.5f);

        CurPos.x = ParentLocation.x + 0.25f * cdLodRender.CDLODRawMeshData.GetMeshExtent();
        CurPos.y = ParentLocation.y;
        CurPos.z = ParentLocation.z + 0.25f * cdLodRender.CDLODRawMeshData.GetMeshExtent();
        SubBRNode = new CDLODSectionNode(1, 1, LodIndex + 1, Obj.transform, cdLodRender, CurPos, 0.5f * Scale, 0.5f);
    }

    public enum LODSelectResult
    {
        IT_Undefined,
        IT_Selected,
        IT_OutOfFrustum,
    }

    public void HideLOD()
    {
        Obj.GetComponent<MeshRenderer>().enabled = false;
        if (SubTLNode != null) SubTLNode.HideLOD();
        if (SubTRNode != null) SubTRNode.HideLOD();
        if (SubBLNode != null) SubBLNode.HideLOD();
        if (SubBRNode != null) SubBRNode.HideLOD();
    }

    public LODSelectResult LODSelect(ref List<CDLODSectionNode> NeedRenderNodes)
    {
        Vector3 TargetPosition = cdLodRender.GetTargetPosition();

        LODSelectResult SubTLSelRes = LODSelectResult.IT_Undefined;
        LODSelectResult SubTRSelRes = LODSelectResult.IT_Undefined;
        LODSelectResult SubBLSelRes = LODSelectResult.IT_Undefined;
        LODSelectResult SubBRSelRes = LODSelectResult.IT_Undefined;

        if (LodIndex != cdLodRender.LodNum - 1)
        {
            float NextDistanceLimit = cdLodRender.LodDistances[LodIndex + 1];
            float Dist = Vector3.Distance(TargetPosition, Obj.transform.position);
            if(Dist < NextDistanceLimit)
            {
                // can consider child node
                if (SubTLNode != null) SubTLSelRes = this.SubTLNode.LODSelect(ref NeedRenderNodes);
                if (SubTRNode != null) SubTRSelRes = this.SubTRNode.LODSelect(ref NeedRenderNodes);
                if (SubBLNode != null) SubBLSelRes = this.SubBLNode.LODSelect(ref NeedRenderNodes);
                if (SubBRNode != null) SubBRSelRes = this.SubBRNode.LODSelect(ref NeedRenderNodes);
            }

        }

        // We don't want to select sub nodes that are invisible (out of frustum) or are selected;
        // (we DO want to select if they are out of range, since we are not)

        bool bRemoveSubTL = (SubTLSelRes == LODSelectResult.IT_OutOfFrustum) || (SubTLSelRes == LODSelectResult.IT_Selected);
        bool bRemoveSubTR = (SubTRSelRes == LODSelectResult.IT_OutOfFrustum) || (SubTRSelRes == LODSelectResult.IT_Selected);
        bool bRemoveSubBL = (SubBLSelRes == LODSelectResult.IT_OutOfFrustum) || (SubBLSelRes == LODSelectResult.IT_Selected);
        bool bRemoveSubBR = (SubBRSelRes == LODSelectResult.IT_OutOfFrustum) || (SubBRSelRes == LODSelectResult.IT_Selected);

        if (!(bRemoveSubTL && bRemoveSubTR && bRemoveSubBL && bRemoveSubBR))
        {
            Obj.GetComponent<MeshRenderer>().enabled = true;
            NeedRenderNodes.Add(this);
            return LODSelectResult.IT_Selected;
        }

        // if any of child nodes are selected, then return selected - otherwise all of them are out of frustum, so we're out of frustum too
        if ((SubTLSelRes == LODSelectResult.IT_Selected) || (SubTRSelRes == LODSelectResult.IT_Selected) || 
            (SubBLSelRes == LODSelectResult.IT_Selected) || (SubBRSelRes == LODSelectResult.IT_Selected))
            return LODSelectResult.IT_Selected;
        else
            return LODSelectResult.IT_OutOfFrustum;
    }
}


public class CDLODRender : MonoBehaviour
{
    [SerializeField]
    public int SectionRow = 3;
    [SerializeField]
    public int SectionColumn = 3;

   // [SerializeField]
   // public int SectionInnerPointNum = 7;//31;

   // [SerializeField]
    //public int LodNum = 3;

    [SerializeField]
    public float[] LodDistances;

    public int LodNum
    {
        get { return LodDistances.Length; }
    }

    [SerializeField]
    public Material WireframeMaterial;

    [SerializeField]
    public float DebugMorphDistance = 1.0f;

    public CDLODMeshData CDLODRawMeshData;

    //
    public GameObject TargetObject;

    //
    public GameObject DebugTestDistanceObject;


    public List<CDLODSectionNode> LODSectionNodes = new List<CDLODSectionNode>();

    [SerializeField]
    public int InnerEdgeNum = 8;

    public Vector3 GetTargetPosition()
    {
        if(TargetObject)
        {
            return TargetObject.transform.position;
        }
        else
        {
            return Vector3.zero;
        }
    }
    // Start is called before the first frame update
    void Start() 
    {
        //LodNum = Mathf.Max(LodNum, 1);
        InnerEdgeNum = InnerEdgeNum - InnerEdgeNum % 2;// SectionInnerPointNum + SectionInnerPointNum % 2;

        CDLODRawMeshData = new CDLODMeshData(InnerEdgeNum);
        CDLODRawMeshData.BuildMesh();
            
        for (int Row = 0; Row < SectionRow; Row++)
        {
            for (int Col = 0; Col < SectionColumn; Col++)
            {
                Vector3 LocalPos = new Vector3();
                LocalPos.x = (Col - SectionRow / 2) * CDLODRawMeshData.GetMeshExtent() + CDLODRawMeshData.GetMeshExtent() * 0.5f;
                LocalPos.z = (Row - SectionRow / 2) * CDLODRawMeshData.GetMeshExtent() + CDLODRawMeshData.GetMeshExtent() * 0.5f;
                LocalPos.y = 0;
                CDLODSectionNode Node = new CDLODSectionNode(Row, Col, 0, transform, this, LocalPos, 1.0f, 1.0f);
                LODSectionNodes.Add(Node);
            }
        }

        for (int i = 0; i < LODSectionNodes.Count; i++)
        {
            LODSectionNodes[i].GenerateLODNode();
        }

    }

    float DebugDistance = 0;

    Vector4 GetMorphConsts(float[] morphStart, float[] morphEnd, int LODLevel)
    {
        Vector4 consts = new Vector4();

        float mStart = morphStart[LODLevel];
        float mEnd = morphEnd[LODLevel];
        //
        const float errorFudge = 0.01f;
        mEnd = Mathf.Lerp(mEnd, mStart, errorFudge);
        //
        consts[0] = mStart;
        consts[1] = 1.0f / (mEnd - mStart);
        //
        consts[2] = mEnd / (mEnd - mStart);
        consts[3] = 1.0f / (mEnd - mStart);
        return consts;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < LODSectionNodes.Count; i++)
        {
            LODSectionNodes[i].HideLOD();
        }


        //Morph 
        float visibilityDistance = LodDistances[0];
        float LODNear = 0;
        float LODFar = visibilityDistance;
        float detailBalance = 2.0f;


        float total = 0;
        float currentDetailBalance = 1.0f;

        for (int i = 0; i < LodDistances.Length; i++)
        {
            total += currentDetailBalance;
            currentDetailBalance *= detailBalance;
        }
        float sect = (LODFar - LODNear) / total;
        float prevPos = LODNear;
        currentDetailBalance = 1.0f;
        float[] visibilityRanges = new float[LodDistances.Length];
        for (int i = 0; i < LodDistances.Length; i++)
        {
            visibilityRanges[LodDistances.Length - i - 1] = prevPos + sect * currentDetailBalance;
            prevPos = visibilityRanges[LodDistances.Length - i - 1];
            currentDetailBalance *= detailBalance;
        }
        prevPos = LODNear;

        
        float[] morphStart = new float[LodDistances.Length];
        float[] morphEnd = new float[LodDistances.Length];
        float morphStartRatio = DebugMorphDistance;// 0.4f;// 0.666f;
        for (int i = 0; i < LodDistances.Length; i++)
        {
            int index = LodDistances.Length - i - 1;
            morphEnd[i] = visibilityRanges[index];
            morphStart[i] = prevPos + (morphEnd[i] - prevPos) * morphStartRatio;

            prevPos = morphStart[i];
        }

        for(int i =0;i < LodDistances.Length; i++)
        {
            //LodDistances[i] = visibilityRanges[i];
        }

        // Select
        List<CDLODSectionNode> NeedRenderNodes = new List<CDLODSectionNode>();
        for (int i = 0; i < LODSectionNodes.Count; i++)
        {
            LODSectionNodes[i].LODSelect(ref NeedRenderNodes);
        }


        for (int i =0;i < NeedRenderNodes.Count; i++)
        {
            int LodIndex = NeedRenderNodes[i].LodIndex;
            int MorphIndex = LodDistances.Length - 1 - LodIndex;
            Vector4 MorphConsts = GetMorphConsts(morphStart, morphEnd, MorphIndex);
            NeedRenderNodes[i].Obj.GetComponent<MeshRenderer>().material.SetVector("g_morphConsts", MorphConsts);

            Vector4 GridDims = new Vector4();
            GridDims[0] = InnerEdgeNum;
            GridDims[1] = InnerEdgeNum * 0.5f;
            GridDims[2] = 2.0f / InnerEdgeNum;
            GridDims[3] = 0;

            NeedRenderNodes[i].Obj.GetComponent<MeshRenderer>().material.SetVector("g_gridDim", GridDims);

            Vector4 QuadScale = new Vector4();
            QuadScale[0] = NeedRenderNodes[i].Obj.transform.lossyScale.x;
            QuadScale[1] = NeedRenderNodes[i].Obj.transform.lossyScale.x;
            QuadScale[2] = LodIndex;
            QuadScale[3] = 0;
            NeedRenderNodes[i].Obj.GetComponent<MeshRenderer>().material.SetVector("g_quadScale", QuadScale);

            NeedRenderNodes[i].Obj.GetComponent<MeshRenderer>().material.SetVector("g_ObserverPos", GetTargetPosition());
            NeedRenderNodes[i].Obj.GetComponent<MeshRenderer>().material.SetFloat("g_DebugMorphDistance", DebugMorphDistance);
            
        }
        

        DebugDistance = Vector3.Distance(GetTargetPosition(), DebugTestDistanceObject.transform.position);
        Debug.Log("DebugDistance = " + DebugDistance);
    }

}
