using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField]
    private RunManager runManager;

    [SerializeField]
    private RunProgress progress;

    [SerializeField]
    private ShopManager shopManager;

    [Header("Shop Root")]
    [SerializeField]
    private GameObject shopPanel;

    [Header("General UI")]
    [SerializeField]
    private TMP_Text coinsText;

    [SerializeField]
    private TMP_Text messageText;

    [Header("Four Shop Slots")]
    [SerializeField]
    private ShopOfferButton[]
        shopOfferButtons =
            new ShopOfferButton[4];

    [Header("Opened Crate")]
    [SerializeField]
    private GameObject ingredientOffersRoot;

    [SerializeField]
    private ShopIngredientOfferButton[]
        ingredientOfferButtons =
            new ShopIngredientOfferButton[3];

    [Header("Continue")]
    [SerializeField]
    private Button continueButton;

    private void Awake()
    {
        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<
                    RunManager
                >();
        }

        if (progress == null)
        {
            progress =
                FindFirstObjectByType<
                    RunProgress
                >();
        }

        if (shopManager == null)
        {
            shopManager =
                FindFirstObjectByType<
                    ShopManager
                >();
        }
    }

    private void OnEnable()
    {
        if (runManager != null)
        {
            runManager.StateChanged +=
                HandleRunStateChanged;
        }

        if (continueButton != null)
        {
            continueButton
                .onClick
                .AddListener(
                    HandleContinuePressed
                );
        }
    }

    private void OnDisable()
    {
        if (runManager != null)
        {
            runManager.StateChanged -=
                HandleRunStateChanged;
        }

        if (continueButton != null)
        {
            continueButton
                .onClick
                .RemoveListener(
                    HandleContinuePressed
                );
        }
    }

    private void Start()
    {
        SetIngredientOffersVisible(
            false
        );

        if (
            runManager != null &&
            runManager.State ==
                RunState.Shop
        )
        {
            ShowShop();
        }
        else
        {
            SetShopVisible(
                false
            );
        }
    }

    private void HandleRunStateChanged(
        RunState state
    )
    {
        if (state == RunState.Shop)
        {
            ShowShop();
        }
        else
        {
            SetShopVisible(
                false
            );
        }
    }

    private void ShowShop()
    {
        if (shopManager == null)
        {
            return;
        }

        shopManager.BeginShop();

        SetShopVisible(
            true
        );

        SetIngredientOffersVisible(
            false
        );

        SetMessage(
            ""
        );

        RefreshUI();
    }

    private void HandleShopOfferPressed(
        ShopOffer offer
    )
    {
        if (
            offer == null ||
            shopManager == null ||
            progress == null
        )
        {
            return;
        }

        if (
            shopManager
                .HasOpenIngredientCrate
        )
        {
            SetMessage(
                "Choose an ingredient from your open crate first."
            );

            return;
        }

        if (
            progress.Coins <
            offer.Cost
        )
        {
            SetMessage(
                "Not enough coins."
            );

            return;
        }

        if (
            offer.OfferType ==
            ShopOfferType.Crate
        )
        {
            bool opened =
                shopManager
                    .OpenCrateOffer(
                        offer
                    );

            if (opened)
            {
                SetIngredientOffersVisible(
                    true
                );

                SetMessage(
                    $"{offer.DisplayName}: choose one ingredient."
                );
            }
            else
            {
                SetMessage(
                    $"Could not open {offer.DisplayName}."
                );
            }
        }
        else
        {
            bool purchased =
                shopManager
                    .PurchaseHelperOffer(
                        offer
                    );

            if (purchased)
            {
                SetMessage(
                    $"{offer.DisplayName} purchased!"
                );
            }
            else
            {
                SetMessage(
                    $"Could not purchase {offer.DisplayName}."
                );
            }
        }

        RefreshUI();
    }

    private void HandleIngredientSelected(
        IngredientDefinition ingredient
    )
    {
        if (
            shopManager == null ||
            progress == null ||
            ingredient == null
        )
        {
            return;
        }

        int copiesBefore =
            progress
                .GetIngredientCopies(
                    ingredient
                );

        if (
            shopManager
                .ChooseIngredient(
                    ingredient
                )
        )
        {
            int copiesAfter =
                progress
                    .GetIngredientCopies(
                        ingredient
                    );

            SetIngredientOffersVisible(
                false
            );

            if (copiesBefore > 0)
            {
                SetMessage(
                    $"{ingredient.ingredientName}: ×{copiesBefore} → ×{copiesAfter}"
                );
            }
            else
            {
                SetMessage(
                    $"{ingredient.ingredientName} added to your pantry at ×{copiesAfter}."
                );
            }
        }

        RefreshUI();
    }

    private void HandleContinuePressed()
    {
        if (
            runManager == null ||
            shopManager == null
        )
        {
            return;
        }

        if (
            shopManager
                .HasOpenIngredientCrate
        )
        {
            SetMessage(
                "Choose an ingredient from your open crate first."
            );

            return;
        }

        SetIngredientOffersVisible(
            false
        );

        runManager
            .ContinueAfterShop();
    }

    private void RefreshUI()
    {
        RefreshCoins();

        RefreshShopOffers();

        RefreshIngredientOffers();

        RefreshContinueButton();
    }

    private void RefreshCoins()
    {
        if (
            coinsText != null &&
            progress != null
        )
        {
            coinsText.text =
                $"Coins: {progress.Coins}";
        }
    }

    private void RefreshShopOffers()
    {
        if (
            shopOfferButtons == null ||
            shopManager == null
        )
        {
            return;
        }

        var offers =
            shopManager
                .CurrentOffers;

        for (
            int i = 0;
            i < shopOfferButtons.Length;
            i++
        )
        {
            ShopOfferButton button =
                shopOfferButtons[i];

            if (button == null)
            {
                continue;
            }

            if (i < offers.Count)
            {
                ShopOffer offer =
                    offers[i];

                bool interactable =
                    shopManager
                        .CanPurchaseOffer(
                            offer
                        );

                button.Setup(
                    offer,
                    interactable,
                    HandleShopOfferPressed
                );
            }
            else
            {
                button
                    .gameObject
                    .SetActive(
                        false
                    );
            }
        }
    }

    private void RefreshIngredientOffers()
    {
        if (
            ingredientOfferButtons ==
                null ||
            shopManager == null
        )
        {
            return;
        }

        var choices =
            shopManager
                .CurrentIngredientChoices;

        bool hasChoices =
            choices.Count > 0;

        SetIngredientOffersVisible(
            hasChoices
        );

        for (
            int i = 0;
            i <
                ingredientOfferButtons
                    .Length;
            i++
        )
        {
            ShopIngredientOfferButton button =
                ingredientOfferButtons[i];

            if (button == null)
            {
                continue;
            }

            if (i < choices.Count)
            {
                IngredientDefinition ingredient =
                    choices[i];

                int ownedCopies =
                    progress != null
                        ? progress
                            .GetIngredientCopies(
                                ingredient
                            )
                        : 0;

                int copiesGranted =
                    shopManager
                        .GetCopiesGrantedByActiveCrate(
                            ingredient
                        );

                button
                    .gameObject
                    .SetActive(
                        true
                    );

                button.Setup(
                    ingredient,
                    ownedCopies,
                    copiesGranted,
                    HandleIngredientSelected
                );
            }
            else
            {
                button
                    .gameObject
                    .SetActive(
                        false
                    );
            }
        }
    }

    private void RefreshContinueButton()
    {
        if (
            continueButton != null &&
            shopManager != null
        )
        {
            continueButton.interactable =
                !shopManager
                    .HasOpenIngredientCrate;
        }
    }

    private void SetShopVisible(
        bool visible
    )
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(
                visible
            );
        }
    }

    private void SetIngredientOffersVisible(
        bool visible
    )
    {
        if (
            ingredientOffersRoot !=
            null
        )
        {
            ingredientOffersRoot
                .SetActive(
                    visible
                );
        }
    }

    private void SetMessage(
        string message
    )
    {
        if (messageText != null)
        {
            messageText.text =
                message;
        }
    }
}