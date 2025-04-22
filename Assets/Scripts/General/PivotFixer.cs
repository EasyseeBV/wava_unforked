using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PivotFixer
{
    // Minimum offset (in world units) before we consider the pivot "off"
    const float k_Threshold = 0.001f;

    /// <summary>
    /// Computes the combined mesh bounds of 'modelRoot' and, if its pivot
    /// is more than k_Threshold away from the bounds center, shifts the
    /// entire GameObject so that its pivot sits at that center.
    /// </summary>
    public static void FixPivotByPosition(GameObject modelRoot)
    {
        // 1) collect all renderers
        var renderers = modelRoot.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;

        // 2) compute the world‐space bounds of the whole hierarchy
        var worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            worldBounds.Encapsulate(renderers[i].bounds);

        // 3) find the mesh‐center in world space
        Vector3 meshCenterWS = worldBounds.center;

        // 4) desired pivot in world space is the parent's origin
        Transform parent = modelRoot.transform.parent;
        if (parent == null)
            return; // nothing to do if there's no parent
        Vector3 desiredPivotWS = parent.position;

        // 5) compute how much we need to move in world space
        Vector3 worldDelta = desiredPivotWS - meshCenterWS;
        if (worldDelta.sqrMagnitude < k_Threshold * k_Threshold)
            return;

        // 6) convert that into the parent's local‐space offset
        Vector3 localDelta = parent.InverseTransformVector(worldDelta);

        // 7) apply to the child's localPosition
        modelRoot.transform.localPosition += localDelta;
    }
}
