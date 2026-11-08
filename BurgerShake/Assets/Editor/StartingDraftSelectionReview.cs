#if UNITY_EDITOR
// Uses preview scenes and restores Random.state; never starts the user's live run.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class StartingDraftSelectionReview
{
    private const string Folder = "Temp/StartingDraftReview";
    static StartingDraftSelectionReview() { EditorApplication.delayCall += Requested; }
    private static void Requested()
    {
        if (!File.Exists(Folder + "/request.txt")) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Requested; return; }
        File.Delete(Folder + "/request.txt");
        Run();
    }

    [MenuItem("Tools/Burger Shake/Review Starting Draft")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Directory.CreateDirectory(Folder);
        Scene scene = EditorSceneManager.NewPreviewScene();
        UnityEngine.Random.State random = UnityEngine.Random.state;
        var report = new StringBuilder();
        try
        {
            UnityEngine.Random.InitState(4831);
            GameObject controllerObject = Create("Isolated Draft Controller", scene);
            var controller = controllerObject.AddComponent<StartingIngredientDraftController>();
            Set(controller, "runDefinition", AssetDatabase.LoadAssetAtPath<RunDefinition>("Assets/ScriptableObjects/Run/RegularRun.asset"));
            Set(controller, "runManager", null); // NEVER start/modify the live run.

            GameObject legacyCanvas = Create("Isolated Legacy Canvas", scene, typeof(RectTransform), typeof(Canvas));
            GameObject uiObject = new GameObject("Isolated Draft UI", typeof(RectTransform));
            uiObject.transform.SetParent(legacyCanvas.transform, false);
            GameObject legacyContent = new GameObject("Legacy Content", typeof(RectTransform));
            legacyContent.transform.SetParent(uiObject.transform, false);
            StartingIngredientDraftUI ui = uiObject.AddComponent<StartingIngredientDraftUI>();
            Set(ui, "draftController", controller);
            Set(ui, "contentRoot", legacyContent);
            Invoke(ui, "Awake");
            Invoke(ui, "OnEnable");
            int completed = 0;
            controller.DraftCompleted += _ => completed++;
            controller.BeginDraft();
            Require(controller.DraftActive && controller.CurrentChoices.Count == 3, "three initial choices");
            StartingDraftSelectionView view = (StartingDraftSelectionView)Get(ui, "generatedView");
            Require(view != null && view.gameObject.activeSelf, "generated screen opens");
            Require(view.transform.parent == null, "canvas does not inherit legacy rect");
            Require(!legacyContent.activeSelf, "legacy content hidden");
            Require(view.GetComponentsInChildren<Image>().Count(i => i.name == "LargeIngredientSprite" && i.enabled && i.sprite != null) == 3, "three large sprites assigned");
            Render(view, scene, "Pick1", new Vector2Int(1920, 1080), report);

            int rerolls = controller.RerollsRemaining;
            IngredientDefinition oldChoice = controller.CurrentChoices[1];
            view.transform.Find("DraftLayout/RerollCard1").GetComponent<Button>().onClick.Invoke();
            Require(controller.RerollsRemaining == rerolls - 1 && controller.CurrentChoices[1] != oldChoice, "reroll replaces the requested card and spends one reroll");
            Require(view.GetComponentsInChildren<Button>().Where(b => b.name.StartsWith("RerollCard")).All(b => !b.interactable), "shared reroll budget disables all rerolls");
            view.transform.Find("DraftLayout/IngredientCard0").GetComponent<Button>().onClick.Invoke();
            Require(controller.CurrentRound == 2 && controller.SelectedIngredients.Count == 1, "first click advances to pick two");
            Require(view.transform.Find("DraftLayout/PickedIngredients/Pick0/PickLabel").GetComponent<TMP_Text>().text == controller.SelectedIngredients[0].ingredientName.ToUpperInvariant(), "first selection shown in pantry strip");
            Invoke(ui, "OnDisable");
            Invoke(ui, "OnEnable");
            Require(view.gameObject.activeSelf, "disable/re-enable restores the active draft");
            Render(view, scene, "Pick2", new Vector2Int(1920, 1080), report);
            Render(view, scene, "Pick2", new Vector2Int(1280, 720), report);
            Render(view, scene, "Pick2", new Vector2Int(1024, 768), report);
            Render(view, scene, "Pick2", new Vector2Int(2560, 1080), report);
            view.transform.Find("DraftLayout/IngredientCard0").GetComponent<Button>().onClick.Invoke();
            Require(!controller.DraftActive && controller.SelectedIngredients.Count == 2 && controller.SelectedIngredients.Distinct().Count() == 2 && completed == 1, "second click completes exactly one draft with two unique ingredients");
            Require(!view.gameObject.activeSelf, "screen closes after the second pick");
            Invoke(ui, "OnDisable");
            report.AppendLine("PASS: open, reroll, pick 1, selection strip, re-enable, pick 2 and close; live run untouched.");

            List<IngredientDefinition> definitions = AssetDatabase.FindAssets("t:IngredientDefinition")
                .Select(g => AssetDatabase.LoadAssetAtPath<IngredientDefinition>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(d => d != null).ToList();
            foreach (IngredientDefinition definition in definitions)
            {
                Require(StartingDraftSelectionView.DisplaySprite(definition) != null, "display sprite for " + definition.name);
            }
            for (int i = 0; i < definitions.Count; i += 3)
            {
                var preview = Create("Draft Art Preview", scene, typeof(RectTransform)).AddComponent<StartingDraftSelectionView>();
                preview.ShowPreview(definitions.Skip(i).Take(3).ToList());
                Render(preview, scene, "Roster" + i, new Vector2Int(1920, 1080), report);
                UnityEngine.Object.DestroyImmediate(preview.gameObject);
            }
            report.AppendLine("PASS: all " + definitions.Count + " ingredient sprites resolved and card copy rendered.");
        }
        catch (Exception exception) { report.AppendLine("FAILED: " + exception); Debug.LogException(exception); }
        finally { UnityEngine.Random.state = random; EditorSceneManager.ClosePreviewScene(scene); }
        File.WriteAllText(Folder + "/report.txt", report.ToString());
        Debug.Log("Starting draft review: " + Path.GetFullPath(Folder + "/report.txt"));
    }

    private static void Render(StartingDraftSelectionView view, Scene scene, string name, Vector2Int size, StringBuilder report)
    {
        GameObject cameraObject = Create("Draft Review Camera", scene, typeof(Camera));
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(56, 37, 23, 255);
        camera.cullingMask = 1 << 31;
        camera.scene = scene;
        var target = new RenderTexture(size.x, size.y, 24);
        Texture2D image = null;
        RenderTexture previous = RenderTexture.active;
        Canvas canvas = view.GetComponent<Canvas>();
        try
        {
            target.Create();
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 2f;
            foreach (Transform transform in view.GetComponentsInChildren<Transform>(true)) transform.gameObject.layer = 31;
            Canvas.ForceUpdateCanvases();
            view.RefreshLayout();
            // These captures run synchronously without a player frame. Explicitly
            // rebuild static graphics too, not only labels whose text just changed.
            foreach (Graphic graphic in view.GetComponentsInChildren<Graphic>())
            {
                graphic.SetAllDirty();
                graphic.Rebuild(CanvasUpdate.PreRender);
            }
            foreach (TMP_Text text in view.GetComponentsInChildren<TMP_Text>(true)) text.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = target;
            image = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
            image.Apply();
            File.WriteAllBytes(Folder + "/" + name + "-" + size.x + "x" + size.y + ".png", image.EncodeToPNG());
            report.AppendLine(name + " " + size + ": RENDERED");
            foreach (TMP_Text text in view.GetComponentsInChildren<TMP_Text>())
                if (text.isTextOverflowing) report.AppendLine("  TEXT OVERFLOW: " + text.transform.parent.name + "/" + text.name);
            foreach (Button button in view.GetComponentsInChildren<Button>())
            {
                Vector3[] corners = new Vector3[4];
                button.GetComponent<RectTransform>().GetWorldCorners(corners);
                foreach (Vector3 corner in corners)
                {
                    Vector2 p = RectTransformUtility.WorldToScreenPoint(camera, corner);
                    if (p.x < -1 || p.x > size.x + 1 || p.y < -1 || p.y > size.y + 1)
                        report.AppendLine("  BUTTON OUT OF BOUNDS: " + button.name);
                }
            }
        }
        finally
        {
            RenderTexture.active = previous;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            if (image != null) UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(cameraObject);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
        }
    }

    private static GameObject Create(string name, Scene scene, params Type[] components)
    { var obj = new GameObject(name, components); SceneManager.MoveGameObjectToScene(obj, scene); return obj; }
    private static void Require(bool condition, string label) { if (!condition) throw new Exception(label); }
    private static FieldInfo Field(object obj, string field) => obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
    private static void Set(object obj, string field, object value) => Field(obj, field).SetValue(obj, value);
    private static object Get(object obj, string field) => Field(obj, field).GetValue(obj);
    private static void Invoke(object obj, string method) => obj.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(obj, null);
}
#endif
