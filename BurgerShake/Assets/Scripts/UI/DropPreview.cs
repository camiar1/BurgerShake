using UnityEngine;

public class DropPreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private SpriteRenderer landingPreview;
    [SerializeField] private Collider2D ingredientCollider;

    [Header("Settings")]
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float maxDropDistance = 20f;
    [SerializeField] private float previewAlpha = 0.3f;

    private void Update()
    {
        ShowDropPreview();
    }

    private void ShowDropPreview()
    {
        Vector2 startPosition = transform.position;

        float radius = ingredientCollider.bounds.extents.x;

        RaycastHit2D hit = Physics2D.CircleCast(
            startPosition,
            radius,
            Vector2.down,
            maxDropDistance,
            collisionLayers
        );

        Vector2 endPosition;

        if (hit.collider != null)
        {
            endPosition = hit.centroid;
        }
        else
        {
            endPosition = startPosition + Vector2.down * maxDropDistance;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);

        if (landingPreview != null)
        {
            landingPreview.transform.position = endPosition;
            landingPreview.sprite =
                GetComponent<SpriteRenderer>().sprite;

            Color colour = landingPreview.color;
            colour.a = previewAlpha;
            landingPreview.color = colour;

            landingPreview.enabled = hit.collider != null;
        }
    }

    public void HidePreview()
    {
        lineRenderer.enabled = false;

        if (landingPreview != null)
            landingPreview.enabled = false;

        enabled = false;
    }
}