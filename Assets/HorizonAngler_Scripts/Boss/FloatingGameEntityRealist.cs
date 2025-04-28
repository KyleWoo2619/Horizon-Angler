using UnityEngine;
using System.Collections.Generic;
using ArchimedsLab;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter))]
public class FloatingGameEntityRealist : GameEntity
{
    [Header("Buoyancy Settings")]
    public Mesh buoyancyMesh;
    public Vector3 CenterOfMassOffset = Vector3.zero;
    public OceanAdvancedURP oceanHeightRef;

    private tri[] _triangles;
    private tri[] worldBuffer;
    private tri[] wetTris;
    private tri[] dryTris;
    private uint nbrWet, nbrDry;
    private WaterSurface.GetWaterHeight realist;
    private Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();

        // Get the Rigidbody explicitly
        rb = GetComponent<Rigidbody>();

        // Prepare mesh cache
        Mesh m = buoyancyMesh != null ? buoyancyMesh : GetComponent<MeshFilter>().sharedMesh;
        WaterCutter.CookCache(m, ref _triangles, ref worldBuffer, ref wetTris, ref dryTris);

        // Define water height function
        realist = pos =>
        {
            const float eps = 0.1f;
            return (oceanHeightRef.GetWaterHeight(pos + new Vector3(-eps, 0f, -eps)) +
                    oceanHeightRef.GetWaterHeight(pos + new Vector3(eps, 0f, -eps)) +
                    oceanHeightRef.GetWaterHeight(pos + new Vector3(0f, 0f, eps))) / 3f;
        };
    }

    protected virtual void Start()
    {
        // Explicitly apply Center of Mass in Start instead of Awake
        if (rb != null)
        {
            rb.centerOfMass = CenterOfMassOffset;
            Debug.Log($"Center of Mass set to {CenterOfMassOffset} in Start");
        }
        else
        {
            Debug.LogError("Rigidbody is null in Start!");
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (rb.IsSleeping()) return;

        // Update mesh to world space
        WaterCutter.CookMesh(transform.position, transform.rotation, ref _triangles, ref worldBuffer);

        // Split mesh based on water height
        WaterCutter.SplitMesh(worldBuffer, ref wetTris, ref dryTris, out nbrWet, out nbrDry, realist);

        // Apply buoyancy forces
        Archimeds.ComputeAllForces(wetTris, dryTris, nbrWet, nbrDry, speed, rb);
    }

    public void ResetCenterOfMass()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
            
        if (rb != null)
        {
            rb.centerOfMass = CenterOfMassOffset;
            Debug.Log($"Center of Mass reset to {CenterOfMassOffset}");
        }
        else
        {
            Debug.LogError("Cannot reset Center of Mass: Rigidbody is null!");
        }
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (!Application.isPlaying) return;

        // Wet tris visualization
        Gizmos.color = Color.blue;
        for (uint i = 0; i < nbrWet; i++)
        {
            Gizmos.DrawLine(wetTris[i].a, wetTris[i].b);
            Gizmos.DrawLine(wetTris[i].b, wetTris[i].c);
            Gizmos.DrawLine(wetTris[i].a, wetTris[i].c);
        }

        // Dry tris visualization
        Gizmos.color = Color.yellow;
        for (uint i = 0; i < nbrDry; i++)
        {
            Gizmos.DrawLine(dryTris[i].a, dryTris[i].b);
            Gizmos.DrawLine(dryTris[i].b, dryTris[i].c);
            Gizmos.DrawLine(dryTris[i].a, dryTris[i].c);
        }

        // Center of Mass visualization
        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rb.worldCenterOfMass, 0.25f);
            Gizmos.DrawLine(transform.position, rb.worldCenterOfMass);
            Handles.Label(rb.worldCenterOfMass + Vector3.up * 0.1f, "Center of Mass");
        }
    }
#endif
}
