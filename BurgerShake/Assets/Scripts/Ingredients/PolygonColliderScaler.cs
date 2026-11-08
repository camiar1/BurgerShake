using System;
using System.Collections.Generic;
using UnityEngine;

// Editor authoring tool: saved PolygonCollider2D paths are the runtime authority.
// Never regenerate in OnEnable/OnValidate; that would erase manual vertex edits.
[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonColliderScaler : MonoBehaviour
{
    [Header("Outline Authoring")]
    [Tooltip("SpriteRenderer on this object, or a child. Assign explicitly if there are several.")]
    [SerializeField] private SpriteRenderer sourceRenderer;
    [Tooltip("Distance inside the outline, in collider-local units. Not a percentage.")]
    [Min(0f)] [SerializeField] private float insetDistance = 0.02f;
    [Tooltip("Removes small outline details. 0 keeps every source vertex.")]
    [Min(0f)] [SerializeField] private float simplificationTolerance = 0.005f;

    // Keep the old serialized names so existing prefab data and backups survive.
    [SerializeField, HideInInspector] private float overallScale = 0.97f;
    [SerializeField, HideInInspector] private float horizontalScale = 1f;
    [SerializeField, HideInInspector] private float verticalScale = 1f;
    [SerializeField, HideInInspector] private Vector2 colliderOffset = Vector2.zero;

    [Serializable]
    private class ColliderPath { public Vector2[] points; }

    [SerializeField, HideInInspector] private List<ColliderPath> originalPaths = new List<ColliderPath>();
    [SerializeField, HideInInspector] private bool originalShapeCaptured;
    [SerializeField, HideInInspector] private string lastResult;
    public string LastResult => lastResult;

    private void OnValidate()
    {
        insetDistance = Mathf.Max(0f, insetDistance);
        simplificationTolerance = Mathf.Max(0f, simplificationTolerance);
    }

    [ContextMenu("Generate From Sprite")]
    public void GenerateFromSprite()
    {
        if (!CanEdit()) return;
        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
        SpriteRenderer renderer = sourceRenderer != null ? sourceRenderer : GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = GetComponentInChildren<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null)
        {
            Warn("Assign a SpriteRenderer with a sprite. Collider was not changed.");
            return;
        }

        Sprite sprite = renderer.sprite;
        int count = sprite.GetPhysicsShapeCount();
        if (count == 0)
        {
            Warn("Sprite has no physics outline. In Sprite Editor > Custom Physics Shape, generate and Apply an outline first. Collider was not changed.");
            return;
        }

        var paths = new List<ColliderPath>();
        var points = new List<Vector2>();
        Matrix4x4 toCollider = collider.transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
        for (int i = 0; i < count; i++)
        {
            sprite.GetPhysicsShape(i, points);
            var transformed = new Vector2[points.Count];
            for (int j = 0; j < points.Count; j++)
            {
                Vector2 p = points[j];
                if (renderer.flipX) p.x = -p.x;
                if (renderer.flipY) p.y = -p.y;
                // Collider.offset is applied separately by Unity.
                transformed[j] = (Vector2)toCollider.MultiplyPoint3x4(p) - collider.offset;
            }
            paths.Add(new ColliderPath { points = transformed });
        }
        BuildAndApply(paths, true, "Generate ingredient collider");
    }

    [ContextMenu("Apply Inset From Captured Outline")]
    public void ApplyInset()
    {
        if (!CanEdit()) return;
        if (!originalShapeCaptured || originalPaths.Count == 0)
        {
            Warn("Generate From Sprite or Capture Manual Edits first. Collider was not changed.");
            return;
        }
        BuildAndApply(originalPaths, false, "Inset ingredient collider");
    }

    [ContextMenu("Capture Current Shape As Original")]
    public void CaptureCurrentShapeAsOriginal()
    {
        if (!CanEdit()) return;
        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
        var paths = new List<ColliderPath>();
        for (int i = 0; i < collider.pathCount; i++)
            paths.Add(new ColliderPath { points = collider.GetPath(i) });
        if (!ValidPaths(paths))
        {
            Warn("Current outline is degenerate, self-intersecting, or has overlapping paths. Capture cancelled.");
            return;
        }
        BeginEdit("Capture manual collider edits");
        originalPaths = paths;
        originalShapeCaptured = true;
        // Capturing is a snapshot, never another scale/inset operation.
        insetDistance = 0f;
        simplificationTolerance = 0f;
        overallScale = horizontalScale = verticalScale = 1f;
        colliderOffset = Vector2.zero;
        FinishEdit("Manual outline captured unchanged. Set an inset and click Apply Inset if wanted.");
    }

    private void BuildAndApply(List<ColliderPath> source, bool capture, string undoLabel)
    {
        var clean = new List<ColliderPath>();
        foreach (ColliderPath path in source)
            clean.Add(new ColliderPath { points = Clean(path.points) });
        if (!ValidPaths(clean))
        {
            Warn("Source outline is invalid, or paths overlap/nest. Edit its Physics Shape or capture a simpler manual outline. Collider was not changed.");
            return;
        }

        var simplified = new List<ColliderPath>();
        foreach (ColliderPath path in clean)
            simplified.Add(new ColliderPath { points = Simplify(path.points, simplificationTolerance) });
        // Simplifying can change topology. Prefer the intact source if that happens.
        if (!ValidPaths(simplified)) simplified = clean;

        float effectiveInset = insetDistance;
        List<ColliderPath> result = null;
        // Narrow necks can collapse under an offset. Reduce globally to preserve all paths.
        for (int attempt = 0; attempt < 12; attempt++)
        {
            var candidate = new List<ColliderPath>();
            foreach (ColliderPath path in simplified)
                candidate.Add(new ColliderPath { points = Inset(path.points, effectiveInset) });
            if (ValidPaths(candidate) && InsideSource(candidate, simplified, effectiveInset))
            {
                result = candidate;
                break;
            }
            effectiveInset *= 0.5f;
        }
        if (result == null)
        {
            Warn("Could not make a safe inset. Reduce Inset Distance or simplify the outline manually. Collider was not changed.");
            return;
        }

        BeginEdit(undoLabel);
        if (capture)
        {
            originalPaths = clean;
            originalShapeCaptured = true;
            overallScale = horizontalScale = verticalScale = 1f;
            colliderOffset = Vector2.zero;
        }
        SetPaths(result);
        string message = "Outline applied. Effective inset: " + effectiveInset.ToString("G4") +
            ". Manual vertex edits are safe; nothing regenerates on reload or spawn.";
        if (effectiveInset < insetDistance)
            message += " Requested inset was reduced to preserve narrow details.";
        FinishEdit(message);
    }

    [ContextMenu("Restore Original Collider")]
    public void RestoreOriginalCollider()
    {
        if (!CanEdit() || !originalShapeCaptured || originalPaths.Count == 0) return;
        BeginEdit("Restore captured collider outline");
        SetPaths(originalPaths);
        insetDistance = 0f;
        overallScale = horizontalScale = verticalScale = 1f;
        colliderOffset = Vector2.zero;
        FinishEdit("Captured outline restored. Undo is available.");
    }

    // Retain the old public entry point for compatibility, but only on explicit invocation.
    [ContextMenu("Legacy/Apply Saved Percentage Scale")]
    public void ApplyColliderScale()
    {
        if (!CanEdit() || !originalShapeCaptured || originalPaths.Count == 0) return;
        var paths = new List<ColliderPath>();
        foreach (ColliderPath path in originalPaths)
        {
            Vector2 min = path.points[0], max = min;
            foreach (Vector2 p in path.points) { min = Vector2.Min(min, p); max = Vector2.Max(max, p); }
            Vector2 center = (min + max) * 0.5f;
            var scaled = new Vector2[path.points.Length];
            for (int i = 0; i < scaled.Length; i++)
                scaled[i] = center + Vector2.Scale(path.points[i] - center,
                    new Vector2(overallScale * horizontalScale, overallScale * verticalScale)) + colliderOffset;
            paths.Add(new ColliderPath { points = scaled });
        }
        if (!ValidPaths(paths)) { Warn("Saved percentage scale produces invalid geometry. Collider was not changed."); return; }
        BeginEdit("Apply legacy collider scale");
        SetPaths(paths);
        FinishEdit("Saved percentage scale applied explicitly.");
    }

    private void SetPaths(List<ColliderPath> paths)
    {
        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
        collider.pathCount = paths.Count;
        for (int i = 0; i < paths.Count; i++) collider.SetPath(i, paths[i].points);
    }

    private const float Epsilon = 0.000001f;
    private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    private static float Area(Vector2[] p)
    {
        float sum = 0f;
        for (int i = 0; i < p.Length; i++) sum += Cross(p[i], p[(i + 1) % p.Length]);
        return sum * 0.5f;
    }

    private static Vector2[] Clean(Vector2[] source)
    {
        var p = new List<Vector2>();
        if (source == null) return p.ToArray();
        foreach (Vector2 v in source)
            if (p.Count == 0 || (v - p[p.Count - 1]).sqrMagnitude > Epsilon * Epsilon) p.Add(v);
        if (p.Count > 1 && (p[0] - p[p.Count - 1]).sqrMagnitude <= Epsilon * Epsilon) p.RemoveAt(p.Count - 1);
        return p.ToArray();
    }

    private static float SegmentDistance(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 d = b - a;
        float t = d.sqrMagnitude > Epsilon * Epsilon ? Mathf.Clamp01(Vector2.Dot(p - a, d) / d.sqrMagnitude) : 0f;
        return Vector2.Distance(p, a + d * t);
    }

    private static Vector2[] Simplify(Vector2[] source, float tolerance)
    {
        var p = new List<Vector2>(source);
        if (tolerance <= 0f) return p.ToArray();
        // Bound deviation from ALL original points skipped by a replacement edge.
        var indices = new List<int>();
        for (int i = 0; i < source.Length; i++) indices.Add(i);
        bool changed = true;
        while (changed && p.Count > 3)
        {
            changed = false;
            for (int i = 0; i < p.Count; i++)
            {
                int previous = (i + p.Count - 1) % p.Count, next = (i + 1) % p.Count;
                bool close = true;
                for (int j = (indices[previous] + 1) % source.Length; j != indices[next]; j = (j + 1) % source.Length)
                    if (SegmentDistance(source[j], p[previous], p[next]) > tolerance) { close = false; break; }
                if (!close) continue;
                p.RemoveAt(i);
                indices.RemoveAt(i);
                changed = true;
                break;
            }
        }
        return p.ToArray();
    }

    private static Vector2[] Inset(Vector2[] p, float distance)
    {
        if (distance == 0f) return (Vector2[])p.Clone();
        var result = new Vector2[p.Length];
        float winding = Mathf.Sign(Area(p));
        for (int i = 0; i < p.Length; i++)
        {
            Vector2 incoming = (p[i] - p[(i + p.Length - 1) % p.Length]).normalized;
            Vector2 outgoing = (p[(i + 1) % p.Length] - p[i]).normalized;
            Vector2 n1 = new Vector2(-incoming.y, incoming.x) * winding;
            Vector2 n2 = new Vector2(-outgoing.y, outgoing.x) * winding;
            Vector2 bisector = n1 + n2;
            float divisor = Vector2.Dot(bisector, n1);
            if (divisor < Epsilon) return null; // Hairpin: no finite miter.
            result[i] = p[i] + bisector * (distance / divisor);
        }
        return result;
    }

    private static bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        float abC = Cross(b - a, c - a), abD = Cross(b - a, d - a);
        float cdA = Cross(d - c, a - c), cdB = Cross(d - c, b - c);
        if (((abC > 0f && abD < 0f) || (abC < 0f && abD > 0f)) &&
            ((cdA > 0f && cdB < 0f) || (cdA < 0f && cdB > 0f))) return true;
        return SegmentDistance(a, c, d) <= Epsilon || SegmentDistance(b, c, d) <= Epsilon ||
            SegmentDistance(c, a, b) <= Epsilon || SegmentDistance(d, a, b) <= Epsilon;
    }

    private static bool Contains(Vector2[] p, Vector2 point)
    {
        bool inside = false;
        for (int i = 0, j = p.Length - 1; i < p.Length; j = i++)
        {
            if (SegmentDistance(point, p[j], p[i]) <= Epsilon) return true;
            if ((p[i].y > point.y) != (p[j].y > point.y) &&
                point.x < (p[j].x - p[i].x) * (point.y - p[i].y) / (p[j].y - p[i].y) + p[i].x)
                inside = !inside;
        }
        return inside;
    }

    private static bool ValidPaths(List<ColliderPath> paths)
    {
        if (paths.Count == 0) return false;
        for (int k = 0; k < paths.Count; k++)
        {
            Vector2[] p = paths[k].points;
            if (p == null || p.Length < 3 || Mathf.Abs(Area(p)) <= Epsilon) return false;
            for (int i = 0; i < p.Length; i++)
            {
                if (float.IsNaN(p[i].x) || float.IsNaN(p[i].y) || float.IsInfinity(p[i].x) || float.IsInfinity(p[i].y)) return false;
                int next = (i + 1) % p.Length;
                if ((p[i] - p[next]).sqrMagnitude <= Epsilon * Epsilon) return false;
                for (int j = i + 1; j < p.Length; j++)
                {
                    int nextJ = (j + 1) % p.Length;
                    if (j == next || nextJ == i) continue;
                    if (SegmentsIntersect(p[i], p[next], p[j], p[nextJ])) return false;
                }
                for (int l = 0; l < k; l++)
                {
                    Vector2[] q = paths[l].points;
                    for (int j = 0; j < q.Length; j++)
                        if (SegmentsIntersect(p[i], p[next], q[j], q[(j + 1) % q.Length])) return false;
                }
            }
            // Nested loops aren't treated as holes in this ingredient authoring tool.
            for (int l = 0; l < k; l++)
                if (Contains(paths[l].points, p[0]) || Contains(p, paths[l].points[0])) return false;
        }
        return true;
    }

    private static bool InsideSource(List<ColliderPath> result, List<ColliderPath> source, float distance)
    {
        for (int k = 0; k < result.Count; k++)
        {
            Vector2[] p = result[k].points, q = source[k].points;
            if (Mathf.Sign(Area(p)) != Mathf.Sign(Area(q)) || Mathf.Abs(Area(p)) > Mathf.Abs(Area(q)) + Epsilon) return false;
            if (distance == 0f) continue;
            for (int i = 0; i < p.Length; i++)
            {
                if (!Contains(q, p[i])) return false;
                // An overlarge inset can reverse an edge while retaining winding.
                if (Vector2.Dot(p[(i + 1) % p.Length] - p[i], q[(i + 1) % q.Length] - q[i]) <= 0f) return false;
                for (int j = 0; j < q.Length; j++)
                {
                    if (SegmentDistance(p[i], q[j], q[(j + 1) % q.Length]) < distance - Epsilon) return false;
                    if (SegmentsIntersect(p[i], p[(i + 1) % p.Length], q[j], q[(j + 1) % q.Length])) return false;
                }
            }
        }
        return true;
    }

    private bool CanEdit()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return true;
#endif
        Warn("Author ingredient colliders outside Play Mode. Runtime colliders use their saved paths.");
        return false;
    }

    private void BeginEdit(string label)
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObjects(new UnityEngine.Object[] { this, GetComponent<PolygonCollider2D>() }, label);
#endif
    }

    private void FinishEdit(string message)
    {
        lastResult = message;
#if UNITY_EDITOR
        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.EditorUtility.SetDirty(collider);
        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
        if (gameObject.scene.IsValid()) UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        Debug.Log(message, this);
    }

    private void Warn(string message) { Debug.LogWarning(message, this); }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(PolygonColliderScaler)), UnityEditor.CanEditMultipleObjects]
public class PolygonColliderScalerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        UnityEditor.EditorGUILayout.HelpBox(
            "No automatic regeneration. Generate/Apply replaces collider points; manual edits persist until you explicitly use a button. Inset is measured in collider-local units. Use solid silhouettes without holes.",
            UnityEditor.MessageType.Info);
        using (new UnityEditor.EditorGUI.DisabledScope(Application.isPlaying))
        {
            if (GUILayout.Button("Generate From Sprite + Inset"))
                foreach (UnityEngine.Object item in targets) ((PolygonColliderScaler)item).GenerateFromSprite();
            if (GUILayout.Button("Apply Inset From Captured Outline"))
                foreach (UnityEngine.Object item in targets) ((PolygonColliderScaler)item).ApplyInset();
            if (GUILayout.Button("Capture Manual Edits (Reset Inset to 0)"))
                foreach (UnityEngine.Object item in targets) ((PolygonColliderScaler)item).CaptureCurrentShapeAsOriginal();
            if (GUILayout.Button("Restore Captured Outline"))
                foreach (UnityEngine.Object item in targets) ((PolygonColliderScaler)item).RestoreOriginalCollider();
        }
        var tool = (PolygonColliderScaler)target;
        if (!string.IsNullOrEmpty(tool.LastResult))
            UnityEditor.EditorGUILayout.HelpBox(tool.LastResult, UnityEditor.MessageType.None);
    }
}
#endif
