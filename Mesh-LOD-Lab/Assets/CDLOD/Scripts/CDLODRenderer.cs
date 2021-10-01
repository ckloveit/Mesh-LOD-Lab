//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class CDLODGridMesh
//{
//    public int dimension;
//    public Mesh mesh;
//    public void SetDimension(int dim) { dimension = dim; GenerateMesh()}
//    public int GetDimension() { return dimension; }

//    private void GenerateMesh()
//    {
//        int gridDim = dimension;

//        int totalVertices = (gridDim + 1) * (gridDim + 1);

//        int totalIndices = gridDim * gridDim * 2 * 3;

//        int vertDim = gridDim + 1;
//        Vector3[] vertices = new Vector3[totalVertices];

//        for (int y = 0; y < vertDim; y++)
//        {
//            for (int x = 0; x < vertDim; x++)
//            {
//                vertices[x + vertDim * y] = new Vector3(x / (float)(gridDim), 0, y / (float)(gridDim));
//            }
//        }
//        int index = 0;

//        int halfd = (vertDim / 2);
//        int fulld = gridDim;
//        int[] indices = new int[totalIndices];
//        for (int y = 0; y < halfd; y++)
//        {
//            for (int x = 0; x < halfd; x++)
//            {
//                indices[index++] = (x + vertDim * y); 
//                indices[index++] = ((x + 1) + vertDim * y);
//                indices[index++] = (x + vertDim * (y + 1));
//                indices[index++] = ((x + 1) + vertDim * y); 
//                indices[index++] = ((x + 1) + vertDim * (y + 1)); 
//                indices[index++] = (x + vertDim * (y + 1));
//            }
//        }

//        // Top right part
//        for (int y = 0; y < halfd; y++)
//        {
//            for (int x = halfd; x < fulld; x++)
//            {
//                indices[index++] = (x + vertDim * y); 
//                indices[index++] = ((x + 1) + vertDim * y);
//                indices[index++] = (x + vertDim * (y + 1));
//                indices[index++] = ((x + 1) + vertDim * y); 
//                indices[index++] = ((x + 1) + vertDim * (y + 1)); 
//                indices[index++] = (x + vertDim * (y + 1));
//            }
//        }

//        // Bottom left part
//        for (int y = halfd; y < fulld; y++)
//        {
//            for (int x = 0; x < halfd; x++)
//            {
//                indices[index++] = (x + vertDim * y); 
//                indices[index++] = ((x + 1) + vertDim * y);
//                indices[index++] = (x + vertDim * (y + 1));
//                indices[index++] = ((x + 1) + vertDim * y); 
//                indices[index++] = ((x + 1) + vertDim * (y + 1));
//                indices[index++] = (x + vertDim * (y + 1));
//            }
//        }

//        // Bottom right part
//        for (int y = halfd; y < fulld; y++)
//        {
//            for (int x = halfd; x < fulld; x++)
//            {
//                indices[index++] = (x + vertDim * y); 
//                indices[index++] = ((x + 1) + vertDim * y); 
//                indices[index++] = (x + vertDim * (y + 1));
//                indices[index++] = ((x + 1) + vertDim * y); 
//                indices[index++] = ((x + 1) + vertDim * (y + 1)); 
//                indices[index++] = (x + vertDim * (y + 1));
//            }
//        }

//        mesh = new Mesh();
//        mesh.name = "Dimension" + dimension + "X" + dimension;
//        mesh.vertices = vertices;
//        mesh.triangles = indices;
//        bool needsNormals = true;
//        bool needsTangents = true;
//        bool needsBounds = true;
//        if (needsNormals)
//        {
//            mesh.RecalculateNormals();
//        }
//        if (needsTangents)
//        {
//            mesh.RecalculateTangents();
//        }
//        if (needsBounds)
//        {
//            mesh.RecalculateBounds();
//        }
//    }
//}

//public class CDLODRenderer
//{
//    public CDLODGridMesh[] cdLODGridMeshes;

//    public CDLODRenderer()
//    {
//        cdLODGridMeshes = new CDLODGridMesh[7];

//        for(int i =0, dim = 2; i < 7; i++, dim *= 2)
//        {
//            cdLODGridMeshes[i] = new CDLODGridMesh();
//            cdLODGridMeshes[i].SetDimension(dim);
//        }
//    }

//    public CDLODGridMesh PickGridMesh(int dimension)
//    {
//        for(int i =0;i < cdLODGridMeshes.Length; i++)
//        {
//            if(cdLODGridMeshes[i].GetDimension() == dimension)
//            {
//                return cdLODGridMeshes[i];
//            }
//        }
//        return null;
//    }

//    public void Render(CDLODRendererBatchInfo batchInfo)
//    {
//        CDLODGridMesh gridMesh = PickGridMesh(batchInfo.MeshGridDimensions);
//        if (gridMesh == null)
//            return;

//        //////////////////////////////////////////////////////////////////////////
//        CDLODQuadTree.SelectedNode[] selectionArray = batchInfo.CDLODSelection.GetSelection();
//        int selectionCount = batchInfo.CDLODSelection.GetSelectionCount();

//        int qtRasterX = batchInfo.CDLODSelection.GetQuadTree().GetRasterSizeX();
//        int qtRasterY = batchInfo.CDLODSelection.GetQuadTree().GetRasterSizeY();
//        MapDimensions mapDims = batchInfo.CDLODSelection.GetQuadTree().GetWorldMapDims();
//        int prevMorphConstLevelSet = -1;
//        for (int i = 0; i < selectionCount; i++)
//        {
//            CDLODQuadTree.SelectedNode nodeSel = selectionArray[i];
//            // Filter out the node if required
//            if (batchInfo.FilterLODLevel != -1 && batchInfo.FilterLODLevel != nodeSel.LODLevel)
//                continue;









//        }





//    }
//}
