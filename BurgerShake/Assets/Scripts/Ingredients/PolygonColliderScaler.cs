using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonColliderScaler : MonoBehaviour
{
    [Header("Collider Size")]

    [Tooltip(
        "Overall collider scale. " +
        "1 = original size."
    )]
    [SerializeField]
    [Range(0.5f, 1.1f)]
    private float overallScale = 0.97f;

    [Tooltip(
        "Additional horizontal scaling. " +
        "1 = no additional horizontal change."
    )]
    [SerializeField]
    [Range(0.5f, 1.1f)]
    private float horizontalScale = 1f;

    [Tooltip(
        "Additional vertical scaling. " +
        "1 = no additional vertical change."
    )]
    [SerializeField]
    [Range(0.5f, 1.1f)]
    private float verticalScale = 1f;

    [Header("Collider Position")]

    [Tooltip(
        "Fine local-position adjustment for the collider shape."
    )]
    [SerializeField]
    private Vector2 colliderOffset =
        Vector2.zero;

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
        overallScale =
            Mathf.Clamp(
                overallScale,
                0.5f,
                1.1f
            );

        horizontalScale =
            Mathf.Clamp(
                horizontalScale,
                0.5f,
                1.1f
            );

        verticalScale =
            Mathf.Clamp(
                verticalScale,
                0.5f,
                1.1f
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
                GetComponent<
                    PolygonCollider2D
                >();
        }
    }

    [ContextMenu(
        "Capture Current Shape As Original"
    )]
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
            pathIndex <
                polygonCollider.pathCount;
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

    [ContextMenu(
        "Apply Collider Scale"
    )]
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
            pathIndex <
                originalPaths.Count;
            pathIndex++
        )
        {
            Vector2[] originalPoints =
                originalPaths[
                    pathIndex
                ].points;

            if (
                originalPoints == null ||
                originalPoints.Length == 0
            )
            {
                continue;
            }

            Vector2 center =
                CalculateBoundsCenter(
                    originalPoints
                );

            Vector2[] scaledPoints =
                new Vector2[
                    originalPoints.Length
                ];

            for (
                int pointIndex = 0;
                pointIndex <
                    originalPoints.Length;
                pointIndex++
            )
            {
                Vector2 direction =
                    originalPoints[
                        pointIndex
                    ] -
                    center;

                direction.x *=
                    overallScale *
                    horizontalScale;

                direction.y *=
                    overallScale *
                    verticalScale;

                scaledPoints[
                    pointIndex
                ] =
                    center +
                    direction +
                    colliderOffset;
            }

            polygonCollider.SetPath(
                pathIndex,
                scaledPoints
            );
        }
    }

    [ContextMenu(
        "Restore Original Collider"
    )]
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
            pathIndex <
                originalPaths.Count;
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

        overallScale =
            1f;

        horizontalScale =
            1f;

        verticalScale =
            1f;

        colliderOffset =
            Vector2.zero;
    }

    private Vector2 CalculateBoundsCenter(
        Vector2[] points
    )
    {
        float minX =
            float.PositiveInfinity;

        float maxX =
            float.NegativeInfinity;

        float minY =
            float.PositiveInfinity;

        float maxY =
            float.NegativeInfinity;

        for (
            int i = 0;
            i < points.Length;
            i++
        )
        {
            Vector2 point =
                points[i];

            minX =
                Mathf.Min(
                    minX,
                    point.x
                );

            maxX =
                Mathf.Max(
                    maxX,
                    point.x
                );

            minY =
                Mathf.Min(
                    minY,
                    point.y
                );

            maxY =
                Mathf.Max(
                    maxY,
                    point.y
                );
        }

        return new Vector2(
            (
                minX +
                maxX
            ) *
            0.5f,

            (
                minY +
                maxY
            ) *
            0.5f
        );
    }
}