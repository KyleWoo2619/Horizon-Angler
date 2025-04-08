using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InitiateMicrogames; // For FishZoneType

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways] // <-- Important! Runs in editor
public class FishingZone : MonoBehaviour
{
    public FishZoneType zoneType;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Automatically rename GameObject in hierarchy to match zone type
        gameObject.name = $"FishingZone_{zoneType}";
        EditorUtility.SetDirty(gameObject); // Marks object as dirty for saving
    }
#endif
}