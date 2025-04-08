using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MeshGenerator{

    public static MeshData GenerateMeshData(float[,] noiseMap, int lod){
        int width = noiseMap.GetLength(0)-2*lod;
        int length = noiseMap.GetLength(1)-2*lod;

        int vertexCountZ = (width-1)/lod + 1;
        int vertexCountX = (length-1)/lod + 1;

    
        int[,] verticesIndex = new int[vertexCountX+2, vertexCountZ+2];
        int vertexIndex = 0;
        int borderVertexIndex = -1;
        for(int i0=-1; i0<vertexCountX+1; i0++){
            for(int i1=-1; i1<vertexCountZ+1; i1++){

                if(i0==-1 || i1==-1 || i0==vertexCountX || i1==vertexCountZ){      
                    verticesIndex[i0+1, i1+1] = borderVertexIndex--;
                } else {
                    verticesIndex[i0+1, i1+1] = vertexIndex++;
                }
            }
        }


        Vector3[] vertices = new Vector3[vertexCountX*vertexCountZ];
        Vector2[] uv = new Vector2[vertexCountX*vertexCountZ];
        int[] triangles = new int[(width-1)*(length-1)*6];
        int triangleIndex = 0;

        Vector3[] borderVertices = new Vector3[-borderVertexIndex-1];
        int[] borderTriangles = new int[6*(2*vertexCountX + 2*vertexCountZ)];
        int borderTriangleIndex = 0;

        // float[,] noiseValues = new float[vertexCountX, vertexCountZ]; 

        for(int i0=-1; i0<vertexCountX+1; i0++){
            for(int i1=-1; i1<vertexCountZ+1; i1++){

                if(i0>=0 && i1>=0 && i0<vertexCountX && i1<vertexCountZ){
                    // noiseValues[i0,i1] = GetNoiseValue(noiseMap, i0, i1, vertexCountX, vertexCountZ, lod)*heightMultiplier;
                    vertices[verticesIndex[i0+1, i1+1]] = new Vector3(i0*lod, noiseMap[(i1+1)*lod,(i0+1)*lod], i1*lod);
                    uv[verticesIndex[i0+1, i1+1]] = new Vector2(i0*lod/(float)length, i1*lod/(float)width);

                    if(i0<vertexCountX-1 && i1<vertexCountZ-1){
                        AddTriangle(verticesIndex, triangles, triangleIndex, i0, i1);
                        triangleIndex+=6;
                    }
                    else if(i0<vertexCountX && i1<vertexCountZ){
                        AddTriangle(verticesIndex, borderTriangles, borderTriangleIndex, i0, i1);
                        borderTriangleIndex+=6;
                    }
                }
                else{
                    borderVertices[-verticesIndex[i0+1, i1+1]-1] = new Vector3(i0*lod, noiseMap[(i1+1)*lod,(i0+1)*lod], i1*lod);

                    if(i0 != vertexCountX && i1 != vertexCountZ && (i0<0 || i1<0)){
                        AddTriangle(verticesIndex, borderTriangles, borderTriangleIndex, i0, i1);
                        borderTriangleIndex+=6;
                    }
                }
            }
        }

        MeshData meshData = new MeshData(vertices, uv, triangles, CalculateNormals(vertices, borderVertices, triangles, borderTriangles), lod, noiseMap, verticesIndex, borderVertices);
        return meshData;
    }

    public static Vector3[] CalculateNormals(Vector3[] vertices, Vector3[] borderVertices, int[] triangles, int[] borderTriangles){
        Vector3[] normals = new Vector3[vertices.Count()];
        
        for(int i0=0; i0<triangles.Count()/3; i0++){
            int indexA = triangles[i0*3];
            int indexB = triangles[i0*3+1];
            int indexC = triangles[i0*3+2];

            Vector3 normal = Vector3.Cross(vertices[indexB]-vertices[indexA],vertices[indexC]-vertices[indexA]).normalized;

            normals[indexA] += normal;
            normals[indexB] += normal;
            normals[indexC] += normal;
        }

        for(int i0=0; i0<borderTriangles.Count()/3; i0++){
            int indexA = borderTriangles[i0*3];
            int indexB = borderTriangles[i0*3+1];
            int indexC = borderTriangles[i0*3+2];

            Vector3 pointA = indexA < 0 ? borderVertices[-indexA-1] : vertices[indexA];
            Vector3 pointB = indexB < 0 ? borderVertices[-indexB-1] : vertices[indexB];
            Vector3 pointC = indexC < 0 ? borderVertices[-indexC-1] : vertices[indexC];
            
            Vector3 normal = Vector3.Cross(pointB-pointA,pointC-pointA).normalized;

            if(indexA>0){
                normals[indexA] += normal;
            }
            if(indexB>0){
                normals[indexB] += normal;
            }
            if(indexC>0){
                normals[indexC] += normal;
            }
        }

        for(int i0=0; i0<normals.Count(); i0++){
            normals[i0].Normalize();
        }

        return normals;
    }

    static void AddTriangle(int[,] verticesIndex, int[] triangles, int triangleIndex, int i0, int i1){
        triangles[triangleIndex++] = verticesIndex[i0+1, i1+1];
        triangles[triangleIndex++] = verticesIndex[i0+1, i1+2];
        triangles[triangleIndex++] = verticesIndex[i0+2, i1+2];

        triangles[triangleIndex++] = verticesIndex[i0+2, i1+2];
        triangles[triangleIndex++] = verticesIndex[i0+2, i1+1];
        triangles[triangleIndex++] = verticesIndex[i0+1, i1+1];        
    }

    public class MeshData{
        public int lod;
        public float[,] heightMap;
        Vector3[] vertices;
        Vector3[] borderVertices;
        Vector2[] uv;
        int[] triangles;
        Vector3[] normals;
        int[,] verticesIndices;
        Dictionary<Vector2, Vector3> normalsByPosition;
        
        public MeshData(Vector3[] vertices, Vector2[] uv, int[] triangles, Vector3[] normals, int lod, float[,] heightMap, int[,] verticesIndices, Vector3[] borderVertices){
            this.vertices = vertices;
            this.uv = uv;
            this.triangles = triangles;
            this.normals = normals;
            this.lod = lod;
            this.heightMap = heightMap;
            this.verticesIndices = verticesIndices;
            this.borderVertices = borderVertices;
            SetNormalsByPosition();
        }

        void SetNormalsByPosition(){
            normalsByPosition = new Dictionary<Vector2, Vector3>();
            for(int i0=0; i0<vertices.Count(); i0++){
                normalsByPosition.Add(new Vector2(vertices[i0].x, vertices[i0].z), normals[i0]);
            }
        }
         public Mesh CreateMesh(){
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.normals = normals;
            return mesh;
        }

        public Vector3 GetPositionAt(int i0, int i1){  
            return vertices[verticesIndices[i0+1, i1+1]];
        }

        public Vector3 GetPerpendicularFor(int i0, int i1){
            Vector3 point = verticesIndices[i0+1, i1+1] > 0 ? vertices[verticesIndices[i0+1, i1+1]] : borderVertices[-verticesIndices[i0+1, i1+1]-1];
            Vector3 Right = verticesIndices[i0+2, i1+1] > 0 ? vertices[verticesIndices[i0+2, i1+1]] : borderVertices[-verticesIndices[i0+2, i1+1]-1] - point;
            Vector3 Left = verticesIndices[i0, i1+1] > 0 ? vertices[verticesIndices[i0, i1+1]] : borderVertices[-verticesIndices[i0, i1+1]-1] - point;
            Vector3 Above = verticesIndices[i0+1, i1+2] > 0 ? vertices[verticesIndices[i0+1, i1+2]] : borderVertices[-verticesIndices[i0+1, i1+2]-1] - point;
            Vector3 Below = verticesIndices[i0+1, i1] > 0 ? vertices[verticesIndices[i0+1, i1]] : borderVertices[-verticesIndices[i0+1, i1]-1] - point;

            return (Vector3.Cross(Right, Above)+Vector3.Cross(Left, Below)).normalized;
        }

        public Vector3 GetNormalForPosition(Vector3 roughPosition){
            Vector2 position = new Vector2(Mathf.RoundToInt(roughPosition.x/lod), Mathf.RoundToInt(roughPosition.z/lod));
            return normalsByPosition[position];
        }
    }
}