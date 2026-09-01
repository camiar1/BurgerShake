using System.Collections.Generic;
using UnityEngine;

public class IngredientHighlight : MonoBehaviour
{
    private readonly List<LineRenderer>
        highlightLines =
            new List<LineRenderer>();

    private bool built;

    private static Material sharedLineMaterial;

    public void Show(
        Color color,
        float width
    )
    {
        if (!built)
        {
            BuildHighlight();
        }

        foreach (
            LineRenderer line
            in highlightLines
        )
        {
            if (line == null)
            {
                continue;
            }

            line.startColor =
                color;

            line.endColor =
                color;

            line.widthMultiplier =
                width;

            line.enabled =
                true;
        }
    }

    public void Hide()
    {
        foreach (
            LineRenderer line
            in highlightLines
        )
        {
            if (line != null)
            {
                line.enabled =
                    false;
            }
        }
    }

    private void BuildHighlight()
    {
        built = true;

        Collider2D[] colliders =
            GetComponentsInChildren<Collider2D>(
                true
            );

        foreach (
            Collider2D collider
            in colliders
        )
        {
            if (
                collider is PolygonCollider2D polygon
            )
            {
                BuildPolygon(
                    polygon
                );
            }
            else if (
                collider is CircleCollider2D circle
            )
            {
                BuildCircle(
                    circle
                );
            }
            else if (
                collider is BoxCollider2D box
            )
            {
                BuildBox(
                    box
                );
            }
            else if (
                collider is CapsuleCollider2D capsule
            )
            {
                BuildCapsule(
                    capsule
                );
            }
        }
    }

    private void BuildPolygon(
        PolygonCollider2D collider
    )
    {
        for (
            int pathIndex = 0;
            pathIndex < collider.pathCount;
            pathIndex++
        )
        {
            Vector2[] path =
                collider.GetPath(
                    pathIndex
                );

            LineRenderer line =
                CreateLine(
                    collider.transform
                );

            line.positionCount =
                path.Length;

            line.loop =
                true;

            for (
                int i = 0;
                i < path.Length;
                i++
            )
            {
                line.SetPosition(
                    i,
                    path[i] +
                    collider.offset
                );
            }
        }
    }

    private void BuildCircle(
        CircleCollider2D collider
    )
    {
        const int segments =
            48;

        LineRenderer line =
            CreateLine(
                collider.transform
            );

        line.positionCount =
            segments;

        line.loop =
            true;

        for (
            int i = 0;
            i < segments;
            i++
        )
        {
            float angle =
                (float)i /
                segments *
                Mathf.PI *
                2f;

            Vector2 point =
                collider.offset +
                new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle)
                ) *
                collider.radius;

            line.SetPosition(
                i,
                point
            );
        }
    }

    private void BuildBox(
        BoxCollider2D collider
    )
    {
        Vector2 halfSize =
            collider.size *
            0.5f;

        Vector2 center =
            collider.offset;

        Vector3[] points =
        {
            center +
            new Vector2(
                -halfSize.x,
                -halfSize.y
            ),

            center +
            new Vector2(
                -halfSize.x,
                halfSize.y
            ),

            center +
            new Vector2(
                halfSize.x,
                halfSize.y
            ),

            center +
            new Vector2(
                halfSize.x,
                -halfSize.y
            )
        };

        LineRenderer line =
            CreateLine(
                collider.transform
            );

        line.positionCount =
            points.Length;

        line.loop =
            true;

        line.SetPositions(
            points
        );
    }

    private void BuildCapsule(
        CapsuleCollider2D collider
    )
    {
        const int segments =
            40;

        LineRenderer line =
            CreateLine(
                collider.transform
            );

        List<Vector3> points =
            new List<Vector3>();

        Vector2 size =
            collider.size;

        Vector2 center =
            collider.offset;

        if (
            collider.direction ==
            CapsuleDirection2D.Vertical
        )
        {
            float radius =
                size.x * 0.5f;

            float straight =
                Mathf.Max(
                    0f,
                    size.y -
                    radius * 2f
                );

            Vector2 topCenter =
                center +
                Vector2.up *
                straight *
                0.5f;

            Vector2 bottomCenter =
                center +
                Vector2.down *
                straight *
                0.5f;

            for (
                int i = 0;
                i <= segments / 2;
                i++
            )
            {
                float angle =
                    Mathf.Lerp(
                        0f,
                        Mathf.PI,
                        (float)i /
                        (segments / 2)
                    );

                points.Add(
                    topCenter +
                    new Vector2(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle)
                    ) *
                    radius
                );
            }

            for (
                int i = 0;
                i <= segments / 2;
                i++
            )
            {
                float angle =
                    Mathf.Lerp(
                        Mathf.PI,
                        Mathf.PI * 2f,
                        (float)i /
                        (segments / 2)
                    );

                points.Add(
                    bottomCenter +
                    new Vector2(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle)
                    ) *
                    radius
                );
            }
        }
        else
        {
            float radius =
                size.y * 0.5f;

            float straight =
                Mathf.Max(
                    0f,
                    size.x -
                    radius * 2f
                );

            Vector2 rightCenter =
                center +
                Vector2.right *
                straight *
                0.5f;

            Vector2 leftCenter =
                center +
                Vector2.left *
                straight *
                0.5f;

            for (
                int i = 0;
                i <= segments / 2;
                i++
            )
            {
                float angle =
                    Mathf.Lerp(
                        -Mathf.PI * 0.5f,
                        Mathf.PI * 0.5f,
                        (float)i /
                        (segments / 2)
                    );

                points.Add(
                    rightCenter +
                    new Vector2(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle)
                    ) *
                    radius
                );
            }

            for (
                int i = 0;
                i <= segments / 2;
                i++
            )
            {
                float angle =
                    Mathf.Lerp(
                        Mathf.PI * 0.5f,
                        Mathf.PI * 1.5f,
                        (float)i /
                        (segments / 2)
                    );

                points.Add(
                    leftCenter +
                    new Vector2(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle)
                    ) *
                    radius
                );
            }
        }

        line.positionCount =
            points.Count;

        line.loop =
            true;

        line.SetPositions(
            points.ToArray()
        );
    }

    private LineRenderer CreateLine(
        Transform parent
    )
    {
        GameObject lineObject =
            new GameObject(
                "IngredientHighlightLine"
            );

        lineObject.transform.SetParent(
            parent,
            false
        );

        LineRenderer line =
            lineObject.AddComponent<LineRenderer>();

        line.useWorldSpace =
            false;

        line.enabled =
            false;

        line.alignment =
            LineAlignment.TransformZ;

        line.numCornerVertices =
            4;

        line.numCapVertices =
            4;

        line.textureMode =
            LineTextureMode.Stretch;

        line.material =
            GetSharedMaterial();

        SpriteRenderer spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            line.sortingLayerID =
                spriteRenderer.sortingLayerID;

            line.sortingOrder =
                spriteRenderer.sortingOrder +
                20;
        }

        highlightLines.Add(
            line
        );

        return line;
    }

    private static Material GetSharedMaterial()
    {
        if (sharedLineMaterial != null)
        {
            return sharedLineMaterial;
        }

        Shader shader =
            Shader.Find(
                "Sprites/Default"
            );

        if (shader == null)
        {
            shader =
                Shader.Find(
                    "Universal Render Pipeline/Unlit"
                );
        }

        if (shader == null)
        {
            shader =
                Shader.Find(
                    "Unlit/Color"
                );
        }

        if (shader != null)
        {
            sharedLineMaterial =
                new Material(
                    shader
                );
        }

        return sharedLineMaterial;
    }
}