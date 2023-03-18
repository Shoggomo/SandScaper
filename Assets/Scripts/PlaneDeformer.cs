using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlaneDeformer : MonoBehaviour
{
    public float scale = 1;
    private Mesh mesh;


    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetHeights(float[] heights)
    {
        if (heights.Length != 121)
        {
            throw new System.Exception("Height map must be of size 121 (11 x 11)");

        }

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        for (int i = 0; i < heights.Length; i++)
        {
            vertices[i] = heights[i] * scale * normals[i];
        }

    }
}
