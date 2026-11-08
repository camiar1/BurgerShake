#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Review only the generated panels in isolated preview scenes. Never saves a scene.
[InitializeOnLoad]
public static class GeneratedUIReview
{
    private const string Folder = "Temp/GeneratedUIReview";
    static GeneratedUIReview() { EditorApplication.delayCall += RunRequestedReview; }

    private static void RunRequestedReview()
    {
        if (!File.Exists(Folder + "/request.txt")) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += RunRequestedReview; return; }
        File.Delete(Folder + "/request.txt");
        Run();
    }

    [MenuItem("Tools/Burger Shake/Review Generated UI")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Directory.CreateDirectory(Folder);
        var report = new StringBuilder();
        try
        {
            foreach (Vector2Int size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 720), new Vector2Int(1024, 768) })
            {
                Render<IngredientTooltipUI>("Tooltip", "IngredientTooltipCanvas/TooltipPanel", size, report);
                Render<RunStatusHUDController>("RunStatus", "RunStatusCanvas/RunStatusPanel", size, report);
                Render<ShopBuildSummaryUI>("BuildSummary", "ShopBuildSummaryCanvas/BuildSummaryPanel", size, report);
                Render<WinScreenController>("WinScreen", "WinScreenCanvas/WinPanel", size, report);
            }
            foreach (string guid in AssetDatabase.FindAssets("t:IngredientDefinition"))
            {
                IngredientDefinition definition = AssetDatabase.LoadAssetAtPath<IngredientDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (definition != null)
                    Render<IngredientTooltipUI>("Ingredient-" + definition.name.Replace('/', '-'), "IngredientTooltipCanvas/TooltipPanel", new Vector2Int(1920, 1080), report, definition);
            }
            report.AppendLine("COMPLETE: isolated editor previews; no gameplay scene was saved.");
        }
        catch (Exception exception) { report.AppendLine("FAILED: " + exception); Debug.LogException(exception); }
        File.WriteAllText(Folder + "/report.txt", report.ToString());
        Debug.Log("Generated UI review: " + Path.GetFullPath(Folder + "/report.txt"));
    }

    private static void Render<T>(string label, string panelPath, Vector2Int size, StringBuilder report, IngredientDefinition definition = null) where T : Component
    {
        Scene scene = EditorSceneManager.NewPreviewScene();
        RenderTexture target = null;
        Texture2D image = null;
        Texture2D cropped = null;
        RenderTexture previousTarget = RenderTexture.active;
        try
        {
            var root = new GameObject("Generated UI Review");
            SceneManager.MoveGameObjectToScene(root, scene);
            var cameraObject = new GameObject("Review Camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(67, 47, 34, 255);
            camera.cullingMask = 1 << 31;
            camera.scene = scene;
            target = new RenderTexture(size.x, size.y, 24);
            target.Create();
            camera.targetTexture = target;

            root.AddComponent<T>();
            Transform panel = root.transform.Find(panelPath);
            if (panel == null) throw new Exception(label + " did not create its panel.");
            Canvas canvas = panel.GetComponentInParent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 2f;
            // Show normal/hoverable buttons in the review; edit previews deliberately
            // disable interaction, which would otherwise show the disabled palette.
            foreach (CanvasGroup group in root.GetComponentsInChildren<CanvasGroup>(true)) group.interactable = true;
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 31;
            Canvas.ForceUpdateCanvases();
            if (definition != null) root.GetComponent<IngredientTooltipUI>()?.Show(definition);
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true)) text.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = target;
            image = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
            image.Apply();
            Vector3[] corners = new Vector3[4];
            ((RectTransform)panel).GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
            bool fits = min.x >= -1f && min.y >= -1f && max.x <= size.x + 1f && max.y <= size.y + 1f;
            int x = Mathf.Clamp(Mathf.FloorToInt(min.x) - 12, 0, size.x - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(min.y) - 12, 0, size.y - 1);
            int width = Mathf.Clamp(Mathf.CeilToInt(max.x) + 12 - x, 1, size.x - x);
            int height = Mathf.Clamp(Mathf.CeilToInt(max.y) + 12 - y, 1, size.y - y);
            cropped = new Texture2D(width, height, TextureFormat.RGB24, false);
            cropped.SetPixels(image.GetPixels(x, y, width, height));
            cropped.Apply();
            File.WriteAllBytes(Folder + "/" + label + "-" + size.x + "x" + size.y + ".png", cropped.EncodeToPNG());
            report.AppendLine(label + " " + size + ": " + (fits ? "BOUNDS OK" : "OUT OF BOUNDS"));
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                if (text.isTextOverflowing) report.AppendLine("  TEXT OVERFLOW: " + text.name + " at " + text.fontSize);
        }
        finally
        {
            RenderTexture.active = previousTarget;
            if (image != null) UnityEngine.Object.DestroyImmediate(image);
            if (cropped != null) UnityEngine.Object.DestroyImmediate(cropped);
            EditorSceneManager.ClosePreviewScene(scene);
            if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
        }
    }
}
#endif
