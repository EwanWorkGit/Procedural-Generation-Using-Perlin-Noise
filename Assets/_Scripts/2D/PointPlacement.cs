using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//IF DISCRETE: SAMPLE-DENSITY = X-INCREMENT*(1/SCALE)
//IF CONTINOUS: SAMPLE-DENSITY CAN BE DIFFERENT TO X-INCREMENT


public class PointPlacement : MonoBehaviour
{
    
    [SerializeField] List<GameObject> GridPoints = new List<GameObject>();
    [SerializeField] GameObject GridPointPrefab, SqrPointPrefab, CirPointPrefab;
    [SerializeField] List<GameObject> SamplePoints = new List<GameObject>();
    [SerializeField] Transform InterPoint;
    [SerializeField] int GridPointCount = 10;
    [SerializeField] float XIncrement = 2f, SampleDensity = 2f, M = 5f, Amplitude = 5f;
    [SerializeField] bool IsDiscrete = false;

    bool Updated = false;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            foreach(GameObject gp in GridPoints)
            {
                Destroy(gp);
            }
            foreach(GameObject sp in SamplePoints)
            {
                Destroy(sp);
            }

            GridPoints.Clear();
            SamplePoints.Clear();

            Updated = false;
        }

        if(!Updated)
        {
            //gridpoints
            for (int i = 0; i < GridPointCount; i++)
            {
                Vector2 pos = new Vector2(i*XIncrement, 0);
                GameObject point = Instantiate(GridPointPrefab, pos, Quaternion.identity);
                point.GetComponent<PointInfo>().Gradient = UnityEngine.Random.Range(-1f, 1f);
                point.transform.parent = transform;
                GridPoints.Add(point);
            }

            //sample points
            for(int i = 0; i < GridPointCount; i++)
            {
                if(i+1 < GridPointCount)
                {
                    float dist = GridPoints[i+1].transform.position.x - GridPoints[i].transform.position.x;
                    float distFraction = dist / SampleDensity;
                    for (int progress = 0; progress <= SampleDensity; progress++)
                    {
                        float x = GridPoints[i].transform.position.x + distFraction * progress;
                        float rawNoise = (PerlinNoise(x) + PerlinNoise(x * 2 + 50) * 0.5f + PerlinNoise(x * 4) * 0.25f) * Amplitude;
                        float finalNoise = IsDiscrete ? Mathf.Round(rawNoise) : rawNoise;
                        float y = finalNoise + M;
                        Vector2 pos = new Vector2(x, y);
                        GameObject point = Instantiate(IsDiscrete ? SqrPointPrefab : CirPointPrefab, pos, Quaternion.identity);
                        point.transform.parent = transform;
                        SamplePoints.Add(point);
                    }
                }
            }
            
            Updated = true;
        }
    }

    float PerlinNoise(float x)
    {
        int lIdx = (int)(x / XIncrement);
        if (lIdx < GridPoints.Count - 1)
        {
            float progress =
            (x - GridPoints[lIdx].transform.position.x) /
            (GridPoints[lIdx + 1].transform.position.x - GridPoints[lIdx].transform.position.x);
            float smoothProg = TSmooth(progress);

            GameObject lPoint = GridPoints[lIdx];
            GameObject rPoint = GridPoints[lIdx + 1];
            float lOffset = x - lPoint.transform.position.x;
            float rOffset = x - rPoint.transform.position.x;
            float lGrad = lPoint.GetComponent<PointInfo>().Gradient;
            float rGrad = rPoint.GetComponent<PointInfo>().Gradient;

            float lInf = lOffset * lGrad;
            float rInf = rOffset * rGrad;

            float noise = Mathf.Lerp(lInf, rInf, smoothProg);
            return noise;
        }
        else
            return 0;
    }
    float TSmooth(float t)
    {
        return 6 * Mathf.Pow(t, 5) - 15*Mathf.Pow(t, 4) + 10*Mathf.Pow(t, 3);
    }
}
