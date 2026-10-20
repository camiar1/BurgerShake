using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUpgradeOfferButton :
    MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button buyButton;

    private RunUpgradeDefinition upgrade;

    private Action<RunUpgradeDefinition>
        buyAction;

    private void Awake()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(
                HandlePressed
            );
        }
    }

    private void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(
                HandlePressed
            );
        }
    }

    public void Setup(
        RunUpgradeDefinition newUpgrade,
        bool canAfford,
        Action<RunUpgradeDefinition> onBuy
    )
    {
        upgrade =
            newUpgrade;

        buyAction =
            onBuy;

        if (upgrade == null)
        {
            return;
        }

        if (icon != null)
        {
            icon.sprite =
                upgrade.icon;

            icon.enabled =
                upgrade.icon != null;

            icon.preserveAspect = true;

            icon.raycastTarget = false;
        }

        if (nameText != null)
        {
            nameText.text =
                upgrade.upgradeName;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                upgrade.description;
        }

        if (costText != null)
        {
            costText.text =
                $"{upgrade.cost} Coins";
        }

        if (buyButton != null)
        {
            buyButton.interactable =
                canAfford;
        }
    }

    private void HandlePressed()
    {
        if (upgrade == null)
        {
            return;
        }

        buyAction?.Invoke(
            upgrade
        );
    }
}