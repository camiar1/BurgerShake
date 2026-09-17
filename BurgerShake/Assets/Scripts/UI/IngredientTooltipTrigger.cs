using UnityEngine;
using UnityEngine.EventSystems;

public class IngredientTooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private IngredientDefinition ingredient;
    private IngredientTooltipUI tooltipUI;

    public void Setup(IngredientDefinition definition)
    {
        ingredient = definition;
        ResolveTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ingredient == null)
            return;

        ResolveTooltip();
        tooltipUI?.Show(ingredient);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipUI?.Hide();
    }

    private void OnDisable()
    {
        tooltipUI?.Hide();
    }

    private void ResolveTooltip()
    {
        if (tooltipUI == null)
        {
            tooltipUI = FindFirstObjectByType<IngredientTooltipUI>();
        }
    }
}
