using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopOfferButton :
    MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private Image icon;

    [SerializeField]
    private TMP_Text typeText;

    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private TMP_Text costText;

    [SerializeField]
    private TMP_Text buttonText;

    [SerializeField]
    private Button buyButton;

    private ShopOffer offer;

    private Action<ShopOffer>
        pressedAction;

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
        ShopOffer newOffer,
        bool interactable,
        Action<ShopOffer> onPressed
    )
    {
        offer =
            newOffer;

        pressedAction =
            onPressed;

        if (offer == null)
        {
            gameObject.SetActive(
                false
            );

            return;
        }

        gameObject.SetActive(
            true
        );

        if (icon != null)
        {
            icon.sprite =
                offer.Icon;

            icon.enabled =
                offer.Icon != null;

            icon.preserveAspect =
                true;

            icon.raycastTarget =
                false;
        }

        if (typeText != null)
        {
            typeText.text =
                offer.OfferType ==
                    ShopOfferType.Crate
                    ? "CRATE"
                    : "HELPER";
        }

        if (nameText != null)
        {
            nameText.text =
                offer.DisplayName;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                offer.Description;
        }

        if (costText != null)
        {
            costText.text =
                offer.Purchased
                    ? "SOLD"
                    : $"{offer.Cost} Coins";
        }

        if (buttonText != null)
        {
            if (offer.Purchased)
            {
                buttonText.text =
                    "SOLD";
            }
            else if (
                offer.OfferType ==
                ShopOfferType.Crate
            )
            {
                buttonText.text =
                    "OPEN";
            }
            else
            {
                buttonText.text =
                    "BUY";
            }
        }

        if (buyButton != null)
        {
            buyButton.interactable =
                interactable &&
                !offer.Purchased;
        }
    }

    private void HandlePressed()
    {
        if (
            offer == null ||
            offer.Purchased
        )
        {
            return;
        }

        pressedAction?.Invoke(
            offer
        );
    }
}