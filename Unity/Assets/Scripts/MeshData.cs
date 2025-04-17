using UnityEngine;
public class MeshData {
    Vector3[] vertices;
    Vector2[] uv;
    int[] triangles;

    public MeshData(Vector3[] vertices, Vector2[] uv, int[] triangles){
        this.vertices = vertices;
        this.uv = uv;
        this.triangles = triangles;
    }

    public Mesh CreateMesh(){
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}