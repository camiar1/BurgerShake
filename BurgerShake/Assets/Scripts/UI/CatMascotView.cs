using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CatMascotView : MonoBehaviour
{
    [SerializeField]
    private Image targetImage;

    [SerializeField]
    private Sprite idleSprite;

    [SerializeField]
    private Sprite throwSprite;

    [SerializeField]
    private float throwPoseDuration = 0.18f;

    private Coroutine throwRoutine;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        SetIdle();
    }

    public void SetIdle()
    {
        if (targetImage != null && idleSprite != null)
        {
            targetImage.sprite = idleSprite;
        }
    }

    public void PlayThrowPose()
    {
        if (throwRoutine != null)
        {
            StopCoroutine(throwRoutine);
        }

        throwRoutine = StartCoroutine(ThrowPoseRoutine());
    }

    private IEnumerator ThrowPoseRoutine()
    {
        if (targetImage != null && throwSprite != null)
        {
            targetImage.sprite = throwSprite;
        }

        yield return new WaitForSecondsRealtime(throwPoseDuration);

        SetIdle();

        throwRoutine = null;
    }
}