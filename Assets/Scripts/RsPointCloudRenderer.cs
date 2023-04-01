using System;
using UnityEngine;
using Intel.RealSense;
using UnityEngine.Rendering;
using UnityEngine.Assertions;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using mattatz.MeshSmoothingSystem;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RsPointCloudRenderer : MonoBehaviour
{
    public Vector2 origin;
    public Vector2Int size;
    public float depthScale = 1;
    public ushort maxDepth = 350;
    public ushort minDepth = 175;
    public int smoothingRate;
    public bool useHCFilter;
    public float HCalpha = 0.5f;
    public float HCbeta = 0.75f;

    public RsFrameProvider Source;
    private Mesh mesh;
    private Texture2D uvmap;

    [NonSerialized]
    private Vector3[] vertices;


    FrameQueue q;

    void Start()
    {
        Source.OnStart += OnStartStreaming;
        Source.OnStop += Dispose;
    }

    private void OnStartStreaming(PipelineProfile obj)
    {
        q = new FrameQueue(1);

        using (var depth = obj.Streams.FirstOrDefault(s => s.Stream == Stream.Depth && s.Format == Format.Z16).As<VideoStreamProfile>())
            ResetMesh(depth.Width, depth.Height); //TODO change mesh size

        Source.OnNewSample += OnNewSample;
    }

    private void ResetMesh(int width, int height)
    {
        Assert.IsTrue(SystemInfo.SupportsTextureFormat(TextureFormat.RGFloat));
        uvmap = new Texture2D(width, height, TextureFormat.RGFloat, false, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_UVMap", uvmap);

        if (mesh != null)
            mesh.Clear();
        else
            mesh = new Mesh()
            {
                indexFormat = IndexFormat.UInt32,
            };

        vertices = new Vector3[width * height];

        var indices = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            indices[i] = i;

        mesh.MarkDynamic();
        mesh.vertices = vertices;

        var uvs = new Vector2[width * height];
        Array.Clear(uvs, 0, uvs.Length);
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                uvs[i + j * width].x = i / (float)width;
                uvs[i + j * width].y = j / (float)height;
            }
        }

        mesh.uv = uvs;

        mesh.SetIndices(indices, MeshTopology.Points, 0, false);
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void OnDestroy()
    {
        if (q != null)
        {
            q.Dispose();
            q = null;
        }

        if (mesh != null)
            Destroy(null);
    }

    private void Dispose()
    {
        Source.OnNewSample -= OnNewSample;

        if (q != null)
        {
            q.Dispose();
            q = null;
        }
    }

    private void OnNewSample(Frame frame)
    {
        if (q == null)
            return;
        try
        {
            if (frame.IsComposite)
            {
                using (var fs = frame.As<FrameSet>())
                // using (var points = fs.FirstOrDefault<Points>(Stream.Depth, Format.Xyz32f))
                using (var depth = fs.FirstOrDefault<DepthFrame>(Stream.Depth, Format.Z16))
                {
                    if (depth != null)
                    {
                        q.Enqueue(depth);
                    }
                }
                return;
            }

            if (frame.Is(Extension.Points))
            {
                q.Enqueue(frame);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    protected void LateUpdate()
    {

        if (!Input.GetKeyDown(KeyCode.Space))
        {
            return;
        }

        if (q != null)
        {
            DepthFrame points;
            if (q.PollForFrame<DepthFrame>(out points))
                using (points)
                {
                    // if (points.Count != mesh.vertexCount)
                    // {
                    //     using (var p = points.GetProfile<VideoStreamProfile>())
                    //         ResetMesh(p.Width, p.Height);
                    // }

                    // if (points.TextureData != IntPtr.Zero)
                    // {
                    //     uvmap.LoadRawTextureData(points.TextureData, points.Count * sizeof(float) * 2);
                    //     uvmap.Apply();
                    // }


                    // var types = Enum.GetValues(typeof(Extension)).Cast<Extension>().Where(e => points.Is(e));
                    // foreach (var type in types)
                    // {
                    //     var name = Enum.GetName(typeof(Extension), type);
                    //     Debug.Log(name + ": " + points.Is(type));                        
                    // }

                    // if (points.VertexData != IntPtr.Zero)
                    if (points.Is(Extension.DepthFrame))
                    {

                        // var depth = FrameSet.FromFrame(points).DepthFrame;
                        // var depth = points.
                        var depthCount = points.Width * points.Height;
                        var depthData = new ushort[depthCount];
                        points.CopyTo(depthData);

                        // Limit values
                        depthData = depthData.Select(x => x > maxDepth ? maxDepth
                                            : x < minDepth ? minDepth
                                            : x
                                        ).ToArray();

                        // var cutValues = new List<ushort>();


                        // points.CopyVertices(vertices);
                        // var nonZero = vertices.First(p => p.z > 0);
                        // Debug.Log(nonZero);

                        // TODO skip the point cloud processing, use a height map instead
                        // Debug.Log(vertices[0..100].Select(v => v));
                        // Debug.Log(vertices[0..100].OrderBy(v => v.x).Select(v => v.x).ToArray());

                        // Debug.Log("===========");

                        List<Vector3> verts = new();
                        List<int> tris = new();
                        // var medianDepth = (float) depthData.Select(x => (int)x).Median();

                        //Bottom left section of the map, other sections are similar
                        for (int y = 0; y < 480; y++)
                        {
                            for (int x = 0; x < 640; x++)
                            {

                                // If a depth is 0 don't add vertex
                                // if (depthData[640 * y + x] == 0) //continue;
                                //     depthData[640 * y + x] = baseLevel;

                                //Add each new vertex in the plane
                                // verts.Add(new Vector3(i, hMap.GetPixel(i,j).grayscale * 100, j));
                                var depth = depthData[640 * y + x] * depthScale;
                                // var scaledDepth = depth + (depth - medianDepth) * depthScale;
                                verts.Add(new Vector3(x, depth, y));
                                // verts.Add(vertices[640 * y + x]);

                                // Correct camera position according to scaling
                                // var newCameraPosition = originalCameraPosition;
                                // newCameraPosition.y *= depthScale;
                                // CameraTransform.position = newCameraPosition;

                                //Skip if a new square on the plane hasn't been formed
                                if (x <= origin.x || y <= origin.y || x >= origin.x + size.x || y >= origin.y + size.y) continue;

                                // cutValues.Add( depthData[640 * y + x]);

                                //Adds the index of the three vertices in order to make up each of the two tris
                                tris.Add(640 * y + x); //Top right
                                tris.Add(640 * y + x - 1); //Bottom right
                                tris.Add(640 * (y - 1) + x - 1); //Bottom left - First triangle
                                tris.Add(640 * (y - 1) + x - 1); //Bottom left 
                                tris.Add(640 * (y - 1) + x); //Top left
                                tris.Add(640 * y + x); //Top right - Second triangle
                            }
                        }

                        Vector2[] uvs = new Vector2[verts.Count];
                        for (var i = 0; i < uvs.Length; i++) //Give UV coords X,Z world coords
                            uvs[i] = new Vector2(verts[i].x, verts[i].z);

                        if (useHCFilter)
                        {
                            mesh.vertices = MeshSmoothing.HCFilter(verts.ToArray(), tris.ToArray(), smoothingRate, HCalpha, HCbeta);
                        }
                        else
                        {
                            mesh.vertices = MeshSmoothing.LaplacianFilter(verts.ToArray(), tris.ToArray(), smoothingRate);
                        }
                        mesh.triangles = tris.ToArray();

                        mesh.RecalculateBounds();
                        mesh.RecalculateNormals();

                        mesh.UploadMeshData(false);


                    }
                }
        }
    }

}
