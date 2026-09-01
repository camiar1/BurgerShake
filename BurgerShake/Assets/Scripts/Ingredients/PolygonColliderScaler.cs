using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonColliderScaler : MonoBehaviour
{
    [Header("Collider Size")]

    [Tooltip(
        "1 = original collider size. " +
        "Values below 1 shrink the collider toward its center."
    )]
    [Range(0.5f, 1f)]
    [SerializeField]
    private float colliderScale = 0.97f;

    [Serializable]
    private class ColliderPath
    {
        public Vector2[] points;
    }

    [SerializeField, HideInInspector]
    private List<ColliderPath> originalPaths =
        new List<ColliderPath>();

    [SerializeField, HideInInspector]
    private bool originalShapeCaptured;

    private PolygonCollider2D polygonCollider;

    private void OnEnable()
    {
        GetCollider();

        if (polygonCollider == null)
        {
            return;
        }

        if (!originalShapeCaptured)
        {
            CaptureCurrentShapeAsOriginal();
        }

        ApplyColliderScale();
    }

    private void OnValidate()
    {
        colliderScale =
            Mathf.Clamp(
                colliderScale,
                0.5f,
                1f
            );

        GetCollider();

        if (polygonCollider == null)
        {
            return;
        }

        if (!originalShapeCaptured)
        {
            CaptureCurrentShapeAsOriginal();
        }

        ApplyColliderScale();
    }

    private void GetCollider()
    {
        if (polygonCollider == null)
        {
            polygonCollider =
                GetComponent<PolygonCollider2D>();
        }
    }

    [ContextMenu("Capture Current Shape As Original")]
    public void CaptureCurrentShapeAsOriginal()
    {
        GetCollider();

        if (polygonCollider == null)
        {
            return;
        }

        originalPaths.Clear();

        for (
            int pathIndex = 0;
            pathIndex < polygonCollider.pathCount;
            pathIndex++
        )
        {
            Vector2[] currentPoints =
                polygonCollider.GetPath(
                    pathIndex
                );

            ColliderPath newPath =
                new ColliderPath();

            newPath.points =
                new Vector2[
                    currentPoints.Length
                ];

            Array.Copy(
                currentPoints,
                newPath.points,
                currentPoints.Length
            );

            originalPaths.Add(
                newPath
            );
        }

        originalShapeCaptured =
            true;

        ApplyColliderScale();
    }

    [ContextMenu("Apply Collider Scale")]
    public void ApplyColliderScale()
    {
        GetCollider();

        if (
            polygonCollider == null ||
            !originalShapeCaptured ||
            originalPaths.Count == 0
        )
        {
            return;
        }

        polygonCollider.pathCount =
            originalPaths.Count;

        for (
            int pathIndex = 0;
            pathIndex < originalPaths.Count;
            pathIndex++
        )
        {
            Vector2[] originalPoints =
                originalPaths[pathIndex].points;

            if (
                originalPoints == null ||
                originalPoints.Length == 0
            )
            {
                continue;
            }

            Vector2 center =
                CalculateCenter(
                    originalPoints
                );

            Vector2[] scaledPoints =
                new Vector2[
                    originalPoints.Length
                ];

            for (
                int pointIndex = 0;
                pointIndex < originalPoints.Length;
                pointIndex++
            )
            {
                Vector2 direction =
                    originalPoints[pointIndex] -
                    center;

                scaledPoints[pointIndex] =
                    center +
                    direction * colliderScale;
            }

            polygonCollider.SetPath(
                pathIndex,
                scaledPoints
            );
        }
    }

    [ContextMenu("Restore Original Collider")]
    public void RestoreOriginalCollider()
    {
        GetCollider();

        if (
            polygonCollider == null ||
            !originalShapeCaptured
        )
        {
            return;
        }

        polygonCollider.pathCount =
            originalPaths.Count;

        for (
            int pathIndex = 0;
            pathIndex < originalPaths.Count;
            pathIndex++
        )
        {
            polygonCollider.SetPath(
                pathIndex,
                originalPaths[
                    pathIndex
                ].points
            );
        }

        colliderScale =
            1f;
    }

    private Vector2 CalculateCenter(
        Vector2[] points
    )
    {
        Vector2 center =
            Vector2.zero;

        for (
            int i = 0;
            i < points.Length;
            i++
        )
        {
            center +=
                points[i];
        }

        center /=
            points.Length;

        return center;
    }
}