using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))] //always a mesh filter on same object as script
public class MeshGenerator : MonoBehaviour
{

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public int xSize = 20;
    public int zSize = 20;

    void Start()
    {
        mesh = new Mesh();

        CreateShape();
        UpdateMesh();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)]; //+ 1 because 1 more vertex for each grid (3 squares = 4 vertices)

        //create grid
        for (int i = 0, z = 0; z < zSize + 1; z++) // i = index of vertices (note it's local)
        {
            for (int x = 0; x < xSize + 1; x++)
            {
                float y = Mathf.PerlinNoise(x * 0.3f, z * 0.3f) * 4f; //noise to make terrain
                vertices[i++] = new Vector3(x, y, z);
            }
        }

        //NOTE: xSize & zSize NOT added by 1, because they are # of triangles, not vertices.
        triangles = new int[xSize * zSize * 6]; //6 = # of indices per quad. xSize * zSize define # of quads in the grid.

        int vert = 0; //current vertex
        int tris = 0; //current triangle
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1; //move up one row
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++; //this way no triangles connect the end of one row of triangles to the start of the next one
            //also because there's always one more vertex then the size
        }
    }

    void UpdateMesh()
    {
        mesh.Clear(); //clear mesh's previous data

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    /*
    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;

        for (int i = 0; i < vertices.Length; i ++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
    */
}
