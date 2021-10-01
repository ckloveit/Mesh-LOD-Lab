//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

////////////////////////////////////////////////////////////////////////////
//// Main class for storing and working with CDLOD quadtree
////////////////////////////////////////////////////////////////////////////
//public class CDLODQuadTree
//{
//    public const int c_maxLODLevels = 15;

//    public struct CreateDesc
//    {
//        public int LeafRenderNodeSize;

//        // The number of LOD levels possible - quad tree will have one level more.
//        public int LODLevelCount;

//        // Heightmap world dimensions
//        public MapDimensions MapDims;

//        // RasterSizeX
//        public int RasterSizeX;
//        public int RasterSizeY;
//    };

//    public struct SelectedNode
//    {
//        public int X;
//        public int Y;
//        public short Size;

//        public bool TL;
//        public bool TR;
//        public bool BL;
//        public bool BR;
//        public float MinDistToCamera;     // this field is valid only if LODSelection::m_sortByDistance is enabled
//        public int LODLevel;

//        public SelectedNode(Node node, int LODLevel, bool tl, bool tr, bool bl, bool br)
//        {
//            this.LODLevel = LODLevel;
//            TL = tl;
//            TR = tr;
//            BL = bl;
//            BR = br;
//            this.X = node.X;
//            this.Y = node.Y;
//            this.Size = node.Size;
//            MinDistToCamera = 0;
//        }

//        public void GetAABB(int rasterSizeX, int rasterSizeY, MapDimensions mapDims) { }
//    };

//    public class LODSelection
//    {
//        // Input
//        public SelectedNode[] m_selectionBuffer;
//        public int m_maxSelectionCount;
//        public Vector3 m_observerPos = new Vector3();
//        public float m_visibilityDistance;
//        public float m_LODDistanceRatio;
//        public float m_morphStartRatio;                  // [0, 1] - when to start morphing to the next (lower-detailed) LOD level; default is 0.667 - first 0.667 part will not be morphed, and the morph will go from 0.667 to 1.0
//        public bool m_sortByDistance;

//        // Output 
//        public CDLODQuadTree m_quadTree;
//        public float[] m_visibilityRanges = new float[c_maxLODLevels];
//        public float[] m_morphEnd = new float[c_maxLODLevels];
//        public float[] m_morphStart = new float[c_maxLODLevels];
//        public int m_selectionCount;
//        public bool m_visDistTooSmall;
//        public int m_minSelectedLODLevel;
//        public int m_maxSelectedLODLevel;

//        public LODSelection(int maxSelectionCount, Vector3 observerPos, float visibilityDistance, float LODDistanceRatio, float morphStartRatio = 0.66f, bool sortByDistance = false)
//        {
            
//        }

//        public CDLODQuadTree GetQuadTree()              { return m_quadTree; }

//        public void GetMorphConsts(int LODLevel, float[] consts )
//        {
//            float mStart = m_morphStart[LODLevel];
//            float mEnd = m_morphEnd[LODLevel];
//            //
//            const float errorFudge = 0.01f;
//            mEnd = Mathf.Lerp(mEnd, mStart, errorFudge);
//            //
//            consts[0] = mStart;
//            consts[1] = 1.0f / (mEnd - mStart);
//            //
//            consts[2] = mEnd / (mEnd - mStart);
//            consts[3] = 1.0f / (mEnd - mStart);
//        }

//        public SelectedNode[] GetSelection()             { return m_selectionBuffer; }
//        public int GetSelectionCount()                  { return m_selectionCount; }

//        public float GetLODDistanceRatio()      { return m_LODDistanceRatio; }
//        public float[] GetLODLevelRanges()        { return m_morphEnd; }

//        // Ugly brute-force mechanism - could be replaced by deterministic one possibly
//        public bool IsVisDistTooSmall()        { return m_visDistTooSmall; }

//        public int GetMinSelectedLevel()      { return m_minSelectedLODLevel; }
//        public int GetMaxSelectedLevel()      { return m_maxSelectedLODLevel; }
//    };

//    public class LODSelectionOnStack : LODSelection
//    {
//        private SelectedNode[] m_selectionBufferOnStack;

//        public LODSelectionOnStack(int SelectionCount, Vector3 observerPos, float visibilityDistance, float LODDistanceRatio, float morphStartRatio = 0.66f, bool sortByDistance = false )
//             : base(SelectionCount, observerPos, visibilityDistance, LODDistanceRatio, morphStartRatio, sortByDistance )
//        {
//            m_selectionBufferOnStack = new SelectedNode[SelectionCount];
//            m_selectionBuffer = m_selectionBufferOnStack;

//        }

//    };

//    // Although relatively small (28 bytes) the Node struct can use a lot of memory when used on
//    // big datasets and high granularity settings (big depth).
//    // For example, for a terrain of 16384*8192 with a leaf node size of 32, it will consume
//    // around 4.6Mb.
//    //
//    // However, most of its values can be created implicitly at runtime, while the only ones needed to
//    // be stored are MinZ/MaxZ values. StreamingCDLOD demo uses such implicit storage.

//    public class Node
//    {
//        public enum LODSelectResult
//        {
//            IT_Undefined,
//            IT_OutOfFrustum,
//            IT_OutOfRange,
//            IT_Selected,
//        };

//        public class LODSelectInfo
//        {
//            public LODSelection SelectionObj;
//            public int SelectionCount;
//            public int StopAtLevel;
//            public int RasterSizeX;
//            public int RasterSizeY;
//            public MapDimensions MapDims;
//        };

//        public short X;
//        public short Y;
//        public short Size;

//        public short Level;            // Caution: highest bit here is used to mark leaf nodes

//        // When IsLeaf() these can be reused for something else (currently they store float heights for ray triangle test 
//        // but that could be stored in a matrix without 4x redundancy like here)
//        // Also, these could/should be indices into CDLODQuadTree::m_allNodesBuffer - no additional memory will then be used
//        // if compiled for 64bit, and they could also be unsigned short-s if having less 65535 nodes
//        public Node SubTL = null;
//        public Node SubTR = null;
//        public Node SubBL = null;
//        public Node SubBR = null;

//        // Level 0 is a root node, and level 'LodLevel-1' is a leaf node. So the actual 
//        // LOD level equals 'LODLevelCount - 1 - Node::GetLevel()'.
//        public short GetLevel()        { return (short)(Level & 0x7FFF); }
//        public bool  IsLeaf()           { return (short)(Level & 0x8000) != 0; }

//        public void Create(int x, int y, int size, int level, CreateDesc createDesc, Node[] allNodesBuffer, ref int allNodesBufferLastIndex )
//        {
//            X = (short)x;
//            Y = (short)y;
//            Level = (short)level;
//            Size = (short)size;

//            int rasterSizeX = createDesc.RasterSizeX;
//            int rasterSizeY = createDesc.RasterSizeY;

//            SubTL = null;
//            SubTR = null;
//            SubBL = null;
//            SubBR = null;

//            // Are we done yet?
//            if (size == (createDesc.LeafRenderNodeSize))
//            {
//                // Mark leaf node!
//                Level = (short)(Level | 0x8000);

//            }else
//            {
//                int subSize = size / 2;
//                SubTL = allNodesBuffer[allNodesBufferLastIndex++];
//                SubTL.Create(x, y, subSize, level + 1, createDesc, allNodesBuffer, ref allNodesBufferLastIndex);
                
//                if ((x + subSize) < rasterSizeX)
//                {
//                    SubTR = allNodesBuffer[allNodesBufferLastIndex++];
//                    SubTR.Create(x + subSize, y, subSize, level + 1, createDesc, allNodesBuffer, ref allNodesBufferLastIndex);
//                }

//                if ((y + subSize) < rasterSizeY)
//                {
//                    SubBL = allNodesBuffer[allNodesBufferLastIndex++];
//                    SubBL.Create(x, y + subSize, subSize, level + 1, createDesc, allNodesBuffer, ref allNodesBufferLastIndex);
//                }

//                if (((x + subSize) < rasterSizeX) && ((y + subSize) < rasterSizeY))
//                {
//                    SubBR = allNodesBuffer[allNodesBufferLastIndex++];
//                    SubBR.Create(x + subSize, y + subSize, subSize, level + 1, createDesc, allNodesBuffer, ref allNodesBufferLastIndex);
//                }
//            }
//        }

//        public LODSelectResult LODSelect(LODSelectInfo lodSelectInfo, bool parentCompletelyInFrustum)
//        {
//            int maxSelectionCount = lodSelectInfo.SelectionObj[0].m_maxSelectionCount;
//            float[] lodRanges = lodSelectInfo.SelectionObj[0].m_visibilityRanges;

//            float distanceLimit = lodRanges[this.GetLevel()];

//            LODSelectResult SubTLSelRes = LODSelectResult.IT_Undefined;
//            LODSelectResult SubTRSelRes = LODSelectResult.IT_Undefined;
//            LODSelectResult SubBLSelRes = LODSelectResult.IT_Undefined;
//            LODSelectResult SubBRSelRes = LODSelectResult.IT_Undefined;

//            if (this.GetLevel() != lodSelectInfo.StopAtLevel)
//            {
//                if (SubTL != null) SubTLSelRes = this.SubTL.LODSelect(lodSelectInfo, false);
//                if (SubTR != null) SubTRSelRes = this.SubTR.LODSelect(lodSelectInfo, false);
//                if (SubBL != null) SubBLSelRes = this.SubBL.LODSelect(lodSelectInfo, false);
//                if (SubBR != null) SubBRSelRes = this.SubBR.LODSelect(lodSelectInfo, false);
//            }

//            // We don't want to select sub nodes that are invisible (out of frustum) or are selected;
//            // (we DO want to select if they are out of range, since we are not)
//            bool bRemoveSubTL = (SubTLSelRes == LODSelectResult.IT_OutOfFrustum) || (SubTLSelRes == LODSelectResult.IT_Selected);
//            bool bRemoveSubTR = (SubTRSelRes == LODSelectResult.IT_OutOfFrustum) || (SubTRSelRes == LODSelectResult.IT_Selected);
//            bool bRemoveSubBL = (SubBLSelRes == LODSelectResult.IT_OutOfFrustum) || (SubBLSelRes == LODSelectResult.IT_Selected);
//            bool bRemoveSubBR = (SubBRSelRes == LODSelectResult.IT_OutOfFrustum) || (SubBRSelRes == LODSelectResult.IT_Selected);

//            if (!(bRemoveSubTL && bRemoveSubTR && bRemoveSubBL && bRemoveSubBR) && (lodSelectInfo.SelectionCount < maxSelectionCount))
//            {
//                int LODLevel = lodSelectInfo.StopAtLevel - this.GetLevel();
//                lodSelectInfo.SelectionObj[0].m_selectionBuffer[lodSelectInfo.SelectionCount++] =
//                    new SelectedNode(this, LODLevel, !bRemoveSubTL, !bRemoveSubTR, !bRemoveSubBL, !bRemoveSubBR);

//                return LODSelectResult.IT_Selected;
//            }

//            // if any of child nodes are selected, then return selected - otherwise all of them are out of frustum, so we're out of frustum too
//            if ((SubTLSelRes == LODSelectResult.IT_Selected) || (SubTRSelRes == LODSelectResult.IT_Selected) || 
//                (SubBLSelRes == LODSelectResult.IT_Selected) || (SubBRSelRes == LODSelectResult.IT_Selected))
//                return LODSelectResult.IT_Selected;
//            else
//                return LODSelectResult.IT_OutOfFrustum;
//        }

//    };


//    private CreateDesc m_desc;

//    private Node[] m_allNodesBuffer;
//    private int m_allNodesCount;

//    private Node[,] m_topLevelNodes;
//    private int m_topNodeSize;
//    private int m_topNodeCountX;
//    private int m_topNodeCountY;

//    private int m_leafNodeSize;    // leaf node size

//    private int m_rasterSizeX;
//    private int m_rasterSizeY;
//    public int GetRasterSizeX()                             { return m_rasterSizeX; }
//    public int GetRasterSizeY()                             { return m_rasterSizeY; }

//    public MapDimensions GetWorldMapDims()                  { return m_desc.MapDims; }

//    private float m_leafNodeWorldSizeX;
//    private float m_leafNodeWorldSizeY;
//    float[] m_LODLevelNodeDiagSizes= new float[c_maxLODLevels];

//    public CDLODQuadTree()
//    {
//        m_desc = new CreateDesc();

//    }

//    public bool Create(CreateDesc desc )
//    {
//        m_desc = desc;
//        m_rasterSizeX = desc.RasterSizeX;
//        m_rasterSizeY = desc.RasterSizeY;
//        if (m_rasterSizeX > 65535 || m_rasterSizeY > 65535)
//        {
//            return false;
//        }
//        if (m_desc.LODLevelCount > c_maxLODLevels)
//        {
//            return false;
//        }
//        //////////////////////////////////////////////////////////////////////////
//        // Determine how many nodes will we use, and the size of the top (root) tree
//        // node.
//        //
//        m_leafNodeWorldSizeX = desc.LeafRenderNodeSize * desc.MapDims.SizeX / (float)m_rasterSizeX;
//        m_leafNodeWorldSizeY = desc.LeafRenderNodeSize * desc.MapDims.SizeY / (float)m_rasterSizeY;
//        //
//        int totalNodeCount = 0;
//        //
//        m_topNodeSize = desc.LeafRenderNodeSize;
//        for (int i = 0; i < m_desc.LODLevelCount; i++)
//        {
//            if (i != 0)
//            {
//                m_topNodeSize *= 2;
//                m_LODLevelNodeDiagSizes[i] = 2 * m_LODLevelNodeDiagSizes[i - 1];
//            }

//            int nodeCountX = (m_rasterSizeX - 1) / m_topNodeSize + 1;
//            int nodeCountY = (m_rasterSizeY - 1) / m_topNodeSize + 1;

//            totalNodeCount += (nodeCountX) * (nodeCountY);
//        }
//        //////////////////////////////////////////////////////////////////////////
//        // Initialize the tree memory, create tree nodes, and extract min/max Zs (heights)
//        //
//        m_allNodesBuffer = new Node[totalNodeCount];
//        int nodeCounter = 0;
//        //
//        m_topNodeCountX = (m_rasterSizeX - 1) / m_topNodeSize + 1;
//        m_topNodeCountY = (m_rasterSizeY - 1) / m_topNodeSize + 1;
//        m_topLevelNodes = new Node[m_topNodeCountX, m_topNodeCountY];
//        for (int y = 0; y < m_topNodeCountY; y++)
//        {
//            for (int x = 0; x < m_topNodeCountX; x++)
//            {
//                m_topLevelNodes[y, x] = m_allNodesBuffer[nodeCounter];
//                nodeCounter++;

//                m_topLevelNodes[y, x].Create(x * m_topNodeSize, y * m_topNodeSize, m_topNodeSize, 0, m_desc, m_allNodesBuffer, ref nodeCounter);
//            }
//        }
//        m_allNodesCount = nodeCounter;

//        return true;
//    }


//    public void LODSelect(LODSelection selectionObj)
//    {
//        Vector3 cameraPos = selectionObj.m_observerPos;
//        float visibilityDistance = selectionObj.m_visibilityDistance;
//        int layerCount = m_desc.LODLevelCount;

//        float LODNear = 0;
//        float LODFar = visibilityDistance;
//        float detailBalance = selectionObj.m_LODDistanceRatio;

//        float total = 0;
//        float currentDetailBalance = 1.0f;

//        selectionObj.m_quadTree = this;
//        selectionObj.m_visDistTooSmall = false;

//        for (int i = 0; i < layerCount; i++)
//        {
//            total += currentDetailBalance;
//            currentDetailBalance *= detailBalance;
//        }

//        float sect = (LODFar - LODNear) / total;

//        float prevPos = LODNear;
//        currentDetailBalance = 1.0f;
//        for (int i = 0; i < layerCount; i++)
//        {
//            selectionObj.m_visibilityRanges[layerCount - i - 1] = prevPos + sect * currentDetailBalance;
//            prevPos = selectionObj.m_visibilityRanges[layerCount - i - 1];
//            currentDetailBalance *= detailBalance;
//        }
//        prevPos = LODNear;
//        for (int i = 0; i < layerCount; i++)
//        {
//            int index = layerCount - i - 1;
//            selectionObj.m_morphEnd[i] = selectionObj.m_visibilityRanges[index];
//            selectionObj.m_morphStart[i] = prevPos + (selectionObj.m_morphEnd[i] - prevPos) * selectionObj.m_morphStartRatio;

//            prevPos = selectionObj.m_morphStart[i];
//        }
//        Node.LODSelectInfo lodSelInfo = new Node.LODSelectInfo();
//        lodSelInfo.RasterSizeX = m_rasterSizeX;
//        lodSelInfo.RasterSizeY = m_rasterSizeY;
//        lodSelInfo.MapDims = m_desc.MapDims;
//        lodSelInfo.SelectionCount = 0;
//        lodSelInfo.SelectionObj = selectionObj;
//        lodSelInfo.StopAtLevel = layerCount - 1;

//        for (int y = 0; y < m_topNodeCountY; y++)
//        {
//            for (int x = 0; x < m_topNodeCountX; x++)
//            {
//                m_topLevelNodes[y,x].LODSelect(lodSelInfo, false);
//            }
//        }

//        selectionObj.m_maxSelectedLODLevel = 0;
//        selectionObj.m_minSelectedLODLevel = c_maxLODLevels;
//        selectionObj.m_selectionCount = lodSelInfo.SelectionCount;

//    }

//    public int GetLODLevelCount()                    { return m_desc.LODLevelCount; }
//}
