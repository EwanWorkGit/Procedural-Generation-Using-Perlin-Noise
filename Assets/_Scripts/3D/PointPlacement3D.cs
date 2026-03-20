using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPlacement3D : MonoBehaviour
{
    public Vector3 CenterPos; //for camera
    public int PX = 10, PZ; //Points per axis, for camera

    [SerializeField] GameObject[,] GridPoints;
    [SerializeField] GameObject GridPointPrefab, PointPrefab;
    [SerializeField] Transform PointParent;
    [SerializeField] float PointSpacing = 5f, Amplitude = 2f, Frequency = 1f, SeaLevel = -10f;
    [SerializeField] int PointDensity = 2;
    [SerializeField] bool DiscreteValues = false, VisibleGrid = false;

    bool Updated = false;

    private void Start()
    {
        GridPoints = new GameObject[PX, PZ];
    }

    //define x and z directions, place grid points along them at axisinterval
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            GameObject[] points = GameObject.FindGameObjectsWithTag("Point");
            foreach(GameObject p in points)
            {
                Destroy(p);
            }
            
            GridPoints = new GameObject[PX, PZ];

            Updated = false;
        }

        if(!Updated)
        {
            for(int z = 0; z < GridPoints.GetLength(1); z++)
            {
                for(int x = 0; x < GridPoints.GetLength(0); x++)
                {
                    Vector3 position = new Vector3(x * PointSpacing, SeaLevel, z * PointSpacing);
                    GameObject gridPoint = Instantiate(GridPointPrefab, position, Quaternion.identity);
                    gridPoint.SetActive(VisibleGrid);
                    gridPoint.GetComponent<GridPointInfo>().Gradient = new Vector2(UnityEngine.Random.Range(-1, 1f), UnityEngine.Random.Range(-1, 1f)).normalized;
                    gridPoint.transform.parent = PointParent;
                    GridPoints[x,z] = gridPoint;
                }
            }

            GenerateMesh();

            Updated = true;
        }
    }

    void GenerateMesh()
    {
        //for safety
        Frequency = Mathf.Clamp(Frequency, 0, Mathf.Infinity);

        GameObject meshObject = Instantiate(PointPrefab);
        MeshFilter meshFilter = meshObject.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        int totalVertsX = ((GridPoints.GetLength(0) - 1) * PointDensity) + 1;
        int totalVertsZ = ((GridPoints.GetLength(1) - 1) * PointDensity) + 1;
        Vector3[] verticies = new Vector3[totalVertsX*totalVertsZ];
        //since verts are +1 we need to remove 1 from each axis
        int[] triangles = new int[(totalVertsX - 1)*(totalVertsZ - 1) * 6];
        int t = 0;

        for (int z = 0; z < GridPoints.GetLength(1); z++)
        { 
            for (int x = 0; x < GridPoints.GetLength(0); x++)
            {
                if (z + 1 < GridPoints.GetLength(1) && x + 1 < GridPoints.GetLength(0))
                {
                    float xDist = GridPoints[x + 1, z].transform.position.x - GridPoints[x, z].transform.position.x;
                    float zDist = GridPoints[x, z + 1].transform.position.z - GridPoints[x, z].transform.position.z;
                    float xFraction = xDist / PointDensity;
                    float zFraction = zDist / PointDensity;
                    for (int zProgress = 0; zProgress <= PointDensity; zProgress++)
                    {
                        for (int xProgress = 0; xProgress <= PointDensity; xProgress++)
                        {
                            Vector2 scaledPosition = new Vector2(GridPoints[x, z].transform.position.x + xFraction * xProgress, GridPoints[x, z].transform.position.z + zFraction * zProgress);
                            float rawNoise = PerlinNoise(scaledPosition * Frequency) * Amplitude + PerlinNoise(scaledPosition * (Frequency * 2f)) * (Amplitude / 2f);
                            float yNoise = DiscreteValues ? Mathf.Round(rawNoise) : rawNoise;

                            int vertIndexX = x * PointDensity + xProgress;
                            int vertIndexZ = z * PointDensity + zProgress;

                            //we already have world position for verticie here
                            Vector3 position = new Vector3(GridPoints[x, z].transform.position.x + xFraction * xProgress, yNoise, GridPoints[x, z].transform.position.z + zFraction * zProgress);
                            int index = vertIndexX + vertIndexZ * totalVertsX;
                            verticies[index] = position;
                        }
                    }
                }
            }
            Transform First = GridPoints[0, 0].transform;
            Transform Last = GridPoints[GridPoints.GetLength(0) - 1, GridPoints.GetLength(1) - 1].transform;
            CenterPos = (Last.position - First.position) / 2f;
            meshObject.transform.position = CenterPos - Last.position / 2f;
        }

        for(int z = 0; z < totalVertsZ - 1; z++)
        {
            for(int x = 0; x < totalVertsX - 1; x++)
            {
                int index = x + z * totalVertsX;

                if((x == totalVertsX && z == totalVertsZ) || (x == totalVertsX && z < totalVertsZ) || (x < totalVertsX && z == totalVertsZ))
                {
                    continue;
                }
                
                triangles[t++] = index;
                triangles[t++] = index + totalVertsX + 1;
                triangles[t++] = index + 1;

                triangles[t++] = index;
                triangles[t++] = index + totalVertsX;
                triangles[t++] = index + totalVertsX + 1;
            }
        }

        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        Debug.Log($"Trigs: {triangles.Length} || Verts: {verticies.Length}");
    }

    GameObject[] GetCorners(Vector2 flatPos)
    {
        //starting indicies
        int maxIndexX = PX - 2, maxIndexZ = PZ - 2;
        //Vector2 scaledPos = Frequency * flatPos;
        //Calculate first corner
        Vector2Int firstCornIndex = new Vector2Int(
        (int)(flatPos.x / PointSpacing),
        (int)(flatPos.y / PointSpacing)
        );

        //wrapped indicies used for the rest of the corners
        int[] indicies = {
        firstCornIndex.x % PX,
        (firstCornIndex.x + 1) % PX,
        firstCornIndex.y % PZ,
        (firstCornIndex.y + 1) % PZ
        };

        GameObject[] corners = {
        GridPoints[indicies[0],indicies[2]],
        GridPoints[indicies[1],indicies[2]],
        GridPoints[indicies[0],indicies[3]],
        GridPoints[indicies[1], indicies[3]]};

        return corners;
    }

    float PerlinNoise(Vector2 flatPos)
    {
        GameObject[] corners = GetCorners(flatPos);

        float tx = TSmooth((flatPos.x % PointSpacing) / PointSpacing);
        float ty = TSmooth((flatPos.y % PointSpacing) / PointSpacing);

        Vector2[] offsets =
        {
            new Vector2(tx, ty),
            new Vector2(tx - 1, ty),
            new Vector2(tx, ty - 1),
            new Vector2(tx - 1, ty - 1)
        };

        float[] influences = new float[corners.Length];

        for(int i = 0; i < corners.Length; i++)
        {
            float influence = Vector2.Dot(corners[i].GetComponent<GridPointInfo>().Gradient, offsets[i]);
            influences[i] = influence;
        }

        float bottom = Mathf.Lerp(influences[0], influences[1], tx);
        float top = Mathf.Lerp(influences[2], influences[3], tx);
        float noise = Mathf.Lerp(bottom, top, ty);

        return noise;
        
         //starts at bottom left, then br, tl, tr

        
        //find offsets between x and corners
        //find gradients on corners
        //dot gradients and offsets
        //interpolate bot r and l, then top r and l, interolate them together
    }

    float TSmooth(float t)
    {
        return 6 * Mathf.Pow(t, 5) - 15 * Mathf.Pow(t, 4) + 10 * Mathf.Pow(t, 3);
    }
}
