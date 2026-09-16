using UnityEngine;

public class ViewHUDController : MonoBehaviour
{
    [SerializeField] private ViewController viewController;

    [Header("HUD Groups")]
    [SerializeField] private CanvasGroup assemblyHUD;
    [SerializeField] private CanvasGroup windowHUD;

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (viewController == null)
        {
            return;
        }

        bool assemblyActive =
            !viewController.IsSliding &&
            viewController.CurrentView ==
            ViewController.FoodTruckView.Assembly;

        SetGroup(assemblyHUD, assemblyActive);

        bool windowActive =
            !viewController.IsSliding &&
            viewController.CurrentView ==
            ViewController.FoodTruckView.CustomerWindow;

        SetGroup(windowHUD, windowActive);
    }

    private void SetGroup(
        CanvasGroup group,
        bool active
    )
    {
        if (group == null)
        {
            return;
        }

        group.alpha = active ? 1f : 0f;
        group.interactable = active;
        group.blocksRaycasts = active;
    }
}