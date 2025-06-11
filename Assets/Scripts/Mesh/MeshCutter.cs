using System.Collections.Generic;
using UnityEngine;

namespace MeshCutter
{
    public class MeshCutter : MonoBehaviour
    {
        public static bool inCut = false;
        public static Mesh originalMesh; //modifying original mesh, creating new mesh/gameobject (during cut) - set in Cut method

        /// <summary>
        /// Cuts gameobject into two based on plane
        /// </summary>
        /// <param name="originalGameObject">Gameobject to cut - will modify it and create another when cut</param>
        /// <param name="cutPlane">Cut plane in WORLD space</param>
        /// <returns>The newly created gameobject after cut (the one on the side of the plane's normal)</returns>
        public static GameObject Cut(GameObject originalGameObject, Plane cutPlane)
        {
            if (inCut) //multithreading lock
                return null;

            inCut = true;

            //init data
            Plane plane = new Plane(originalGameObject.transform.InverseTransformDirection(cutPlane.normal),
                originalGameObject.transform.InverseTransformPoint(-cutPlane.normal * cutPlane.distance));
            //worldspace->localspace because mesh vertices are local space

            originalMesh = originalGameObject.GetComponent<MeshFilter>().mesh;

            GeneratedMesh leftMesh = new GeneratedMesh(); //left will be put back into originalMesh
            GeneratedMesh rightMesh = new GeneratedMesh();

            //cutting logic
            //data for for loop
            List<Vector3> addedVertices = new List<Vector3>();
            int[] submeshIndices;
            for (int i = 0; i < originalMesh.subMeshCount; i ++)
            {
                submeshIndices = originalMesh.GetTriangles(i);
                //submeshIndices are basically the triangles of the submesh
                for (int j = 0; j < submeshIndices.Length; j += 3) //each triangle has 3 vertices - loop by triangle
                {
                    int triangleIdx1 = submeshIndices[j];
                    int triangleIdx2 = submeshIndices[j + 1];
                    int triangleIdx3 = submeshIndices[j + 2];
                    MeshTriangle curTriangle = createTriangle(i, triangleIdx1, triangleIdx2, triangleIdx3);

                    bool isLeftSide1 = plane.GetSide(originalMesh.vertices[triangleIdx1]);
                    bool isLeftSide2 = plane.GetSide(originalMesh.vertices[triangleIdx2]);
                    bool isLeftSide3 = plane.GetSide(originalMesh.vertices[triangleIdx3]);

                    //points have to all be on the left side to be part of left mesh, vice versa
                    //if intersecting plane (if points are both not all on the right side as well as the left side) then cut triangle
                    if (isLeftSide1 && isLeftSide2 && isLeftSide3)
                    {
                        leftMesh.AddTriangle(curTriangle);
                    }
                    else if (!isLeftSide1 && !isLeftSide2 && !isLeftSide3)
                    {
                        rightMesh.AddTriangle(curTriangle);
                    }
                    else
                    {
                        cutTriangle(curTriangle, isLeftSide1, isLeftSide2, isLeftSide3, plane, leftMesh, rightMesh, addedVertices);
                    }
                }
            }

            //fill cut
            //define center postiion
            Vector3 centerPos = Vector3.zero;
            Vector2 centerUV = Vector2.zero;
            int addedVerticesCount = addedVertices.Count;
            for (int i = 0; i < addedVerticesCount; i ++)
            {
                centerPos += addedVertices[i];
            }
            centerPos /= addedVerticesCount;

            //create new triangles connecting to center pos to fill
            for (int i = 0; i < addedVertices.Count; i += 2)
            {
                MeshTriangle finalTriangle = new MeshTriangle( //left triangle
                    new Vector3[] { addedVertices[i], addedVertices[i + 1], centerPos },
                    new Vector3[] { -plane.normal, -plane.normal, -plane.normal },
                    new Vector2[] { Vector2.zero, Vector2.zero, Vector2.zero }, 0);//assign to 1st submesh of GameObject

                //UV's can be improved - add Material parameter to Cut method, then based on location of addedVertices[i]
                //relative to the plane and each other, find its UV and set it. Then set the originalGameObject's renderer's
                //material list to be itself with an added element, which is the material parameter.

                if (Vector3.Dot(Vector3.Cross(finalTriangle.vertices[1] - finalTriangle.vertices[0],
                    finalTriangle.vertices[2] - finalTriangle.vertices[0]), finalTriangle.normals[0]) < 0)
                {
                    flipTriangle(finalTriangle);
                }
                leftMesh.AddTriangle(finalTriangle);

                finalTriangle.normals = new Vector3[] { plane.normal, plane.normal, plane.normal }; //right triangle - same but flipped
                if (Vector3.Dot(Vector3.Cross(finalTriangle.vertices[1] - finalTriangle.vertices[0],
                    finalTriangle.vertices[2] - finalTriangle.vertices[0]), finalTriangle.normals[0]) < 0)
                {
                    flipTriangle(finalTriangle);
                }
                rightMesh.AddTriangle(finalTriangle);
            }

            //end
            //change originalMesh -> rightMesh
            originalMesh.Clear();
            originalMesh.vertices = rightMesh.vertices.ToArray();
            originalMesh.normals = rightMesh.normals.ToArray();
            originalMesh.uv = rightMesh.uvs.ToArray();
            originalMesh.subMeshCount = rightMesh.submeshIndices.Count;
            for (int i = 0; i < rightMesh.submeshIndices.Count; i ++)
            {
                originalMesh.SetTriangles(rightMesh.submeshIndices[i].ToArray(), i);
            }
            originalGameObject.GetComponent<MeshCollider>().sharedMesh = originalMesh;

            //instantiate new gameobject as child, setting its mesh to leftMesh
            GameObject newObject = Instantiate(originalGameObject, originalGameObject.transform.position,
                originalGameObject.transform.rotation);
            Mesh newMesh = newObject.GetComponent<MeshFilter>().mesh;
            newMesh.Clear();
            newMesh.vertices = leftMesh.vertices.ToArray();
            newMesh.normals = leftMesh.normals.ToArray();
            newMesh.uv = leftMesh.uvs.ToArray();
            newMesh.subMeshCount = leftMesh.submeshIndices.Count;
            for (int i = 0; i < leftMesh.submeshIndices.Count; i ++)
            {
                newMesh.SetTriangles(leftMesh.submeshIndices[i].ToArray(), i);
            }
            newObject.GetComponent<MeshCollider>().sharedMesh = newMesh;

            inCut = false;
            return newObject; //returns newly created gameobject after cut
        }

        private static void cutTriangle(MeshTriangle curTriangle, bool isLeftSide1, bool isLeftSide2, bool isLeftSide3, Plane plane,
            GeneratedMesh leftMesh, GeneratedMesh rightMesh, List<Vector3> addedVertices)
        {
            //logic: first, split triangle into two sides, split by plane. Keep separate lists of vertices, uvs, and normals.
            //Using plane.raycast to find the distance along a cut side (of the triangle), put in Vector3.Lerp to find partitioned
            //vertices, uvs, and normals. These are the new vertices. Finding which intersected vertex is on the "left"/"right"
            //(that are in clockwise order - check if triangle is flipped), create the new triangles, and store these in order
            //from "left" -> "right" in addedVertices array.

            //cutting triangle
            List<int> leftIndices = new List<int>(); //means indices of left vertices/uvs/normals of triangle
            List<int> rightIndices = new List<int>(); //"" for right
            if (isLeftSide1)
                leftIndices.Add(0);
            else
                rightIndices.Add(0);
            if (isLeftSide2)
                leftIndices.Add(1);
            else
                rightIndices.Add(1);
            if (isLeftSide3)
                leftIndices.Add(2);
            else
                rightIndices.Add(2);
            bool TriangleIsLeft = false; //if 2 vertices of triangle on left side of plane
            if (leftIndices.Count == 1) //duplicate element if only 1 - more simplified code
            {
                leftIndices.Add(leftIndices[0]); //TriangleIsLeft already set to false
            }
            else if (rightIndices.Count == 1)
            {
                rightIndices.Add(rightIndices[0]);
                TriangleIsLeft = true;
            }


            //find intersection points - vertices, uvs, and normals
            //left intersection: leftIndices[0] -> rightIndices[0]
            float distance;
            plane.Raycast(new Ray(curTriangle.vertices[leftIndices[0]],
                (curTriangle.vertices[rightIndices[0]] - curTriangle.vertices[leftIndices[0]]).normalized), out distance);
            float percent = distance / Vector3.Distance(curTriangle.vertices[leftIndices[0]], curTriangle.vertices[rightIndices[0]]);

            Vector3 leftIVertex = Vector3.Lerp(curTriangle.vertices[leftIndices[0]], curTriangle.vertices[rightIndices[0]], percent);
            Vector3 leftINormal = Vector3.Lerp(curTriangle.normals[leftIndices[0]], curTriangle.normals[rightIndices[0]], percent);
            Vector2 leftIUV = Vector2.Lerp(curTriangle.uvs[leftIndices[0]], curTriangle.uvs[rightIndices[0]], percent);
            addedVertices.Add(leftIVertex);

            //right intersection: leftIndices[1] -> rightIndices[1] (same thing, but indices changed from [0] to [1])
            plane.Raycast(new Ray(curTriangle.vertices[leftIndices[1]],
                (curTriangle.vertices[rightIndices[1]] - curTriangle.vertices[leftIndices[1]]).normalized), out distance);
            percent = distance / Vector3.Distance(curTriangle.vertices[leftIndices[1]], curTriangle.vertices[rightIndices[1]]);

            Vector3 rightIVertex = Vector3.Lerp(curTriangle.vertices[leftIndices[1]], curTriangle.vertices[rightIndices[1]], percent);
            Vector3 rightINormal = Vector3.Lerp(curTriangle.normals[leftIndices[1]], curTriangle.normals[rightIndices[1]], percent);
            Vector2 rightIUV = Vector2.Lerp(curTriangle.uvs[leftIndices[1]], curTriangle.uvs[rightIndices[1]], percent);
            addedVertices.Add(rightIVertex);


            //add new triangles to left/rightMesh
            //LEFT MESH
            MeshTriangle finalTriangle = new MeshTriangle(
                new Vector3[] { curTriangle.vertices[leftIndices[0]], leftIVertex, rightIVertex },
                new Vector3[] { curTriangle.normals[leftIndices[0]], leftINormal, rightINormal },
                new Vector2[] { curTriangle.uvs[leftIndices[0]], leftIUV, rightIUV }, curTriangle.submeshIdx);
            //dot product used to calculate if two vectors are more than 90 degrees apart - if so, it will be negative
            //if angle between finalTriangle's supposed normal and its actual normal is > 90 degrees, flip
            if (Vector3.Dot(Vector3.Cross(finalTriangle.vertices[1] - finalTriangle.vertices[0],
                finalTriangle.vertices[2] - finalTriangle.vertices[0]), finalTriangle.normals[0]) < 0)
            {
                flipTriangle(finalTriangle);
            }
            leftMesh.AddTriangle(finalTriangle);
            //repeat if two vertices on left side of cut plane
            if (TriangleIsLeft)
            {
                finalTriangle = new MeshTriangle(
                    new Vector3[] { curTriangle.vertices[leftIndices[0]], curTriangle.vertices[leftIndices[1]], rightIVertex },
                    new Vector3[] { curTriangle.normals[leftIndices[0]], curTriangle.normals[leftIndices[1]], rightINormal },
                    new Vector2[] { curTriangle.uvs[leftIndices[0]], curTriangle.uvs[leftIndices[1]], rightIUV }, curTriangle.submeshIdx);
                if (Vector3.Dot(Vector3.Cross(finalTriangle.vertices[1] - finalTriangle.vertices[0],
                    finalTriangle.vertices[2] - finalTriangle.vertices[0]), finalTriangle.normals[0]) < 0)
                {
                    flipTriangle(finalTriangle);
                }
                leftMesh.AddTriangle(finalTriangle);
            }

            //RIGHT MESH
            finalTriangle = new MeshTriangle(
                new Vector3[] { curTriangle.vertices[rightIndices[0]], leftIVertex, rightIVertex },
                new Vector3[] { curTriangle.normals[rightIndices[0]], leftINormal, rightINormal },
                new Vector2[] { curTriangle.uvs[rightIndices[0]], leftIUV, rightIUV }, curTriangle.submeshIdx);
            if (Vector3.Dot(Vector3.Cross(finalTriangle.vertices[1] - finalTriangle.vertices[0],
                finalTriangle.vertices[2] - finalTriangle.vertices[0]), finalTriangle.normals[0]) < 0)
            {
                flipTriangle(finalTriangle);
            }
            rightMesh.AddTriangle(finalTriangle);
            //repeat if two vertices on right side of cut plane
            if (!TriangleIsLeft)
            {
                finalTriangle = new MeshTriangle(
                    new Vector3[] { curTriangle.vertices[rightIndices[0]], curTriangle.vertices[rightIndices[1]], rightIVertex },
                    new Vector3[] { curTriangle.normals[rightIndices[0]], curTriangle.normals[rightIndices[1]], rightINormal },
                    new Vector2[] { curTriangle.uvs[rightIndices[0]], curTriangle.uvs[rightIndices[1]], rightIUV }, curTriangle.submeshIdx);
                if (Vector3.Dot(Vector3.Cross(finalTriangle.vertices[1] - finalTriangle.vertices[0],
                    finalTriangle.vertices[2] - finalTriangle.vertices[0]), finalTriangle.normals[0]) < 0)
                {
                    flipTriangle(finalTriangle);
                }
                rightMesh.AddTriangle(finalTriangle);
            }
        }

        private static void flipTriangle(MeshTriangle triangle)
        {
            //swap first and last vertices, normals, and uvs of triangle
            Vector3 tmp = triangle.vertices[0];
            triangle.vertices[0] = triangle.vertices[2];
            triangle.vertices[2] = tmp;

            tmp = triangle.normals[0];
            triangle.normals[0] = triangle.normals[2];
            triangle.normals[2] = tmp;

            Vector2 tmp2 = triangle.uvs[0];
            triangle.uvs[0] = triangle.uvs[2];
            triangle.uvs[2] = tmp2;
        }

        private static MeshTriangle createTriangle(int i, int triangleIdx1, int triangleIdx2, int triangleIdx3)
        {
            Vector3[] vertices = new Vector3[]
            {
                originalMesh.vertices[triangleIdx1],
                originalMesh.vertices[triangleIdx2],
                originalMesh.vertices[triangleIdx3]
            };
            Vector3[] normals = new Vector3[]
            {
                originalMesh.normals[triangleIdx1],
                originalMesh.normals[triangleIdx2],
                originalMesh.normals[triangleIdx3]
            };
            Vector2[] uvs = new Vector2[]
            {
                originalMesh.uv[triangleIdx1],
                originalMesh.uv[triangleIdx2],
                originalMesh.uv[triangleIdx3]
            };
            return new MeshTriangle(vertices, normals, uvs, i);
        }
    }

    internal class GeneratedMesh
    {
        internal List<Vector3> vertices = new List<Vector3>();
        internal List<Vector3> normals = new List<Vector3>();
        internal List<Vector2> uvs = new List<Vector2>();
        internal List<List<int>> submeshIndices = new List<List<int>>(); //indices for triangles, compatible with submeshes

        internal void AddTriangle(MeshTriangle triangle) //add a triangle to this mesh
        {
            int verticesLength = vertices.Count; //get count before vertices.AddRange - which changes count

            vertices.AddRange(triangle.vertices); //used to add a list to the end of this list
            normals.AddRange(triangle.normals);
            uvs.AddRange(triangle.uvs);

            //check if you need to add new submesh index lists, and if so, do it
            for (int i = submeshIndices.Count; i < triangle.submeshIdx + 1; i ++)
            {
                submeshIndices.Add(new List<int>());
            }

            //using added vertices, add a triangle to submesh indices (which are the triangle indices)
            for (int i = 0; i < 3; i ++)
            {
                submeshIndices[triangle.submeshIdx].Add(verticesLength + i); //add vertex idx's to end of vertices list
            }
        }
    }

    internal class MeshTriangle
    {
        internal Vector3[] vertices = new Vector3[3];
        internal Vector3[] normals = new Vector3[3];
        internal Vector2[] uvs = new Vector2[3];
        internal int submeshIdx = 0;

        internal MeshTriangle(Vector3[] newvertices, Vector3[] newnormals, Vector2[] newuvs, int newsubmeshIdx)
        {
            for (int i = 0; i < 3; i++)
            {
                vertices[i] = newvertices[i]; //initialized already, just add to current list
                normals[i] = newnormals[i];
                uvs[i] = newuvs[i];
            }

            submeshIdx = newsubmeshIdx;
        }
    }
}