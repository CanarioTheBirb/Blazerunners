using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("----- NODE -----")]
    [SerializeField] Node[] derivedNodes;
    [SerializeField] Color color;
    [SerializeField] int pathWays = 0;
    [SerializeField] bool isDriftNode = false;
    [SerializeField] float driftingSecs = 0;
    [SerializeField] float nodeSpeedRatio = 1;
    [SerializeField] float nodeDecelerateDistance = 50;
    [SerializeField] float nodeActivateDist = 10;
    [SerializeField] bool hasMysteryBox = false;

    public float distance = 0;
    public float avgSpeedRatio = 0;

    public Transform pathPos;
    public int pathPoints = 0;

    private void Start()
    {
        CalculateDistance();
        CalculateAverageSpeedRatio();
        CalculateBasePoints();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        pathPos = transform;
        Gizmos.DrawWireSphere(pathPos.transform.position, nodeActivateDist);
        DrawPaths();
    }

    private void DrawPaths()
    {
        if (pathWays > 1 && derivedNodes != null)
        {
            System.Collections.Generic.List<Vector3> lineVertices = new System.Collections.Generic.List<Vector3>(); //what is needed to drawlinelist
            for (int i = 0; i < derivedNodes.Length; i++)
            {
                Gizmos.DrawLine(transform.position, derivedNodes[i].transform.position);
                Node[] derivedDerivedNodes = derivedNodes[i].GetDerivedNodes();
                for (int j = 0; j < derivedDerivedNodes.Length; j++)
                {
                    if (j > 0)
                    {
                        lineVertices.Add(derivedDerivedNodes[j - 1].pathPos.position);
                        lineVertices.Add(derivedDerivedNodes[j].pathPos.position);
                    }
                }
            }
            Gizmos.DrawLineList(lineVertices.ToArray());
        }
    }

    private void CalculateDistance()
    {
        if (pathWays > 1 && derivedNodes != null)
        {
            for (int i = 0; i < derivedNodes.Length; i++)
            {
                Node[] derivedDerivedNodes = derivedNodes[i].GetDerivedNodes();
                for (int j = 1; j < derivedDerivedNodes.Length; j++)
                {
                    derivedNodes[i].distance += Vector3.Distance(derivedDerivedNodes[j - 1].pathPos.position, derivedDerivedNodes[j].pathPos.position);
                }
            }
        }
    }

    private void CalculateAverageSpeedRatio()
    {
        if (pathWays > 1 && derivedNodes != null)
        {
            for (int i = 0; i < derivedNodes.Length; i++)
            {
                Node[] derivedDerivedNodes = derivedNodes[i].GetDerivedNodes();
                for (int j = 1; j < derivedDerivedNodes.Length; j++)
                {
                    derivedNodes[i].avgSpeedRatio += derivedDerivedNodes[j].GetNodeSpeed();
                }
                derivedNodes[i].avgSpeedRatio /= derivedDerivedNodes.Length - 1;
            }
        }
    }

    private void CalculateBasePoints()
    {
        if (pathWays > 1 && derivedNodes != null)
        {
            for (int i = 0; i < derivedNodes.Length; i++)
            {
                // Adding points based on path's average speed
                derivedNodes[i].pathPoints += Mathf.RoundToInt(1000.0f * derivedNodes[i].avgSpeedRatio);

                // Adding points based on if the pathway has a Mystery-Box
                if (derivedNodes[i].hasMysteryBox) 
                {
                    derivedNodes[i].pathPoints += 200;
                }

                // Adding points based on the distance
                // (Lesser the distance, Greater the points)
                float maxDist = 0;
                float minDist = Mathf.Infinity;
                for(int j = 0;j < derivedNodes.Length; j++)
                {
                    minDist = Mathf.Min(minDist, derivedNodes[j].distance);
                    maxDist = Mathf.Min(maxDist, derivedNodes[j].distance);
                }
                float distancepoints = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(minDist, maxDist, derivedNodes[i].distance));
                derivedNodes[i].pathPoints += Mathf.RoundToInt(1000 * distancepoints);
            }
        }
    }

    public int GetPathWays()
    {
        return pathWays;
    }

    public Node[] GetDerivedNodes()
    {
        return derivedNodes;
    }

    public float GetDecelerateNodeDist()
    {
        return nodeDecelerateDistance;
    }

    public float GetNodeSpeed()
    {
        return nodeSpeedRatio;
    }

    public float GetActivateNodeDist()
    {
        return nodeActivateDist;
    }

    public bool GetDrift()
    {
        return isDriftNode;
    }

    public float GetDriftTime()
    {
        return driftingSecs;
    }

    public bool HasMysteryBox()
    {
        return hasMysteryBox;
    }
}
