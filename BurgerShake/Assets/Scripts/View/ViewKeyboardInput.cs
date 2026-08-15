using UnityEngine;
using UnityEngine.InputSystem;

public class ViewKeyboardInput : MonoBehaviour
{
    [SerializeField] private ViewController viewController;

    private void Awake()
    {
        if (viewController == null)
        {
            viewController = GetComponent<ViewController>();
        }
    }

    private void Update()
    {
        if (viewController == null || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            viewController.TurnLeft();
        }

        if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            viewController.TurnRight();
        }
    }
}
