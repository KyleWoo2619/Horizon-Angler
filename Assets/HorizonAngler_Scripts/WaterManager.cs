using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class WaterManager : MonoBehaviour
{
    private MeshFilter meshfilter;
    private void Awake()
    {
        meshfilter = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        Vector3[] vertices = meshfilter.mesh.vertices;
        for (int i =0; i < vertices.Length; i++)
        {
            vertices[i].y = WaveManager.instance.GetWaveHeight(transform.position.x + vertices[i].x);
        }

        meshfilter.mesh.vertices = vertices;
        meshfilter.mesh.RecalculateNormals();
    }
}
