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
    [SerializeField] float PointSpacing = 5f, Amplitude = 2f, GridPointY = -10f;
    [SerializeField] int PointDensity = 2;
    [SerializeField] bool DiscreteValues = false, GenerationDelay = false;

    Coroutine Generation;

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
            StopCoroutine(Generation);

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
            Vector3 right = Vector3.right, up = Vector3.up;

            for(int z = 0; z < GridPoints.GetLength(1); z++)
            {
                for(int x = 0; x < GridPoints.GetLength(0); x++)
                {
                    Vector3 position = new Vector3(x * PointSpacing, GridPointY, z * PointSpacing);
                    GameObject gridPoint = Instantiate(GridPointPrefab, position, Quaternion.identity);
                    gridPoint.GetComponent<GridPointInfo>().Gradient = new Vector2(Random.Range(-1, 1f), Random.Range(-1, 1f)).normalized;
                    gridPoint.transform.parent = PointParent;
                    GridPoints[x,z] = gridPoint;
                }
            }

            Generation = StartCoroutine(VisibleGeneration());

            Updated = true;
        }

        
    }

    IEnumerator VisibleGeneration()
    {
        List<Vector3> positions = new List<Vector3>();

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
                    for (int zProgress = 0; zProgress < PointDensity; zProgress++)
                    {
                        if(GenerationDelay)
                            yield return new WaitForSeconds(0.0001f);

                        for (int xProgress = 0; xProgress < PointDensity; xProgress++)
                        {
                            Vector2 scaledPosition = new Vector2(GridPoints[x, z].transform.position.x + xFraction * xProgress, GridPoints[x, z].transform.position.z + zFraction * zProgress);
                            float rawNoise = PerlinNoise(new Vector2(scaledPosition.x, scaledPosition.y)) * Amplitude;
                            float yNoise = DiscreteValues ? Mathf.Round(rawNoise) : rawNoise;

                            Vector3 position = new Vector3(GridPoints[x, z].transform.position.x + xFraction * xProgress, yNoise, GridPoints[x, z].transform.position.z + zFraction * zProgress);
                            GameObject point = Instantiate(PointPrefab, position, Quaternion.identity);
                            point.transform.parent = PointParent;
                        }
                    }
                }
            }

            Transform First = GridPoints[0, 0].transform;
            Transform Last = GridPoints[GridPoints.GetLength(0) - 1, GridPoints.GetLength(1) - 1].transform;
            CenterPos = (Last.position - First.position) / 2f;
        }
        yield return null;
    }
    float PerlinNoise(Vector2 flatPos)
    {
        //starting indicies
        int maxIndexX = PX, maxIndexZ = PZ;
        Vector2Int blCorner = new Vector2Int(
        Mathf.Clamp((int)(flatPos.x / PointSpacing), 0, maxIndexX), 
        Mathf.Clamp((int)(flatPos.y / PointSpacing), 0, maxIndexZ)
        );
       
        GameObject[] corners = {
        GridPoints[blCorner.x, blCorner.y],
        GridPoints[blCorner.x + 1, blCorner.y],
        GridPoints[blCorner.x, blCorner.y + 1],
        GridPoints[blCorner.x + 1, blCorner.y + 1]};

        float[] influences = new float[corners.Length];
        for(int i = 0; i < corners.Length; i++)
        {
            Vector2 flatCornPos = new Vector2(corners[i].transform.position.x, corners[i].transform.position.z);
            Vector2 offset = (flatPos - flatCornPos) / PointSpacing;
            float influence = Vector2.Dot(corners[i].GetComponent<GridPointInfo>().Gradient, offset);
            influences[i] = influence;
        }

        float tx = TSmooth((flatPos.x - GridPoints[blCorner.x, blCorner.y].transform.position.x) / PointSpacing);
        float ty = TSmooth((flatPos.y - GridPoints[blCorner.x, blCorner.y].transform.position.z) / PointSpacing);

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
