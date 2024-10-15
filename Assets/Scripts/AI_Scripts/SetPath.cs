using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SetPath : MonoBehaviour
{
    public Node[] path;
    [SerializeField] Color rayColor;

    //Draws a line from node to node
    private void OnDrawGizmos()
    {
        Gizmos.color = rayColor;

        for (int i = 0; i < path.Length; ++i)
        {
            if (path[i].pathPos != null)
            {
                Vector3 pos = path[i].pathPos.position;
                if (i > 0)
                {
                    if (path[i - 1].pathPos != null || path[i - 1].GetPathWays() < 2)
                    {
                        var prev = path[i - 1].pathPos.position;
                        Gizmos.DrawLine(prev, path[i].pathPos.position);
                    }
                }
            }
        }
    }
}
