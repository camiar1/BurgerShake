using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private RunManager runManager;
    [SerializeField] private RunProgress progress;
    [SerializeField] private ShopManager shopManager;

    [Header("Shop Root")]
    [SerializeField] private GameObject shopPanel;

    [Header("General UI")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text messageText;

    [Header("Ingredient Crate")]
    [SerializeField] private Button openCrateButton;
    [SerializeField] private TMP_Text openCrateButtonText;

    [SerializeField]
    private GameObject ingredientOffersRoot;

    [SerializeField]
    private ShopIngredientOfferButton[]
        ingredientOfferButtons;

    [Header("Upgrades")]
    [SerializeField]
    private ShopUpgradeOfferButton[]
        upgradeOfferButtons;

    [Header("Continue")]
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }

        if (progress == null)
        {
            progress =
                FindFirstObjectByType<RunProgress>();
        }

        if (shopManager == null)
        {
            shopManager =
                FindFirstObjectByType<ShopManager>();
        }
    }

    private void OnEnable()
    {
        if (runManager != null)
        {
            runManager.StateChanged +=
                HandleRunStateChanged;
        }

        if (openCrateButton != null)
        {
            openCrateButton.onClick.AddListener(
                HandleOpenCratePressed
            );
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(
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

        if (openCrateButton != null)
        {
            openCrateButton.onClick.RemoveListener(
                HandleOpenCratePressed
            );
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(
                HandleContinuePressed
            );
        }
    }

    private void Start()
    {
        SetIngredientOffersVisible(false);

        if (
            runManager != null &&
            runManager.State == RunState.Shop
        )
        {
            ShowShop();
        }
        else
        {
            SetShopVisible(false);
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
            SetShopVisible(false);
        }
    }

    private void ShowShop()
    {
        if (shopManager == null)
        {
            return;
        }

        shopManager.BeginShop();

        SetShopVisible(true);

        SetIngredientOffersVisible(false);

        SetMessage("");

        RefreshUI();
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
        if (ingredientOffersRoot != null)
        {
            ingredientOffersRoot.SetActive(
                visible
            );
        }
    }

    private void HandleOpenCratePressed()
    {
        if (
            shopManager == null ||
            progress == null
        )
        {
            return;
        }

        if (
            shopManager.HasOpenIngredientCrate
        )
        {
            SetMessage(
                "Choose an ingredient first."
            );

            return;
        }

        if (
            progress.Coins <
            shopManager.IngredientCrateCost
        )
        {
            SetMessage(
                "Not enough coins."
            );

            return;
        }

        bool opened =
            shopManager.OpenIngredientCrate();

        if (!opened)
        {
            SetIngredientOffersVisible(false);

            SetMessage(
                "There are no new ingredients left to buy."
            );
        }
        else
        {
            SetIngredientOffersVisible(true);

            SetMessage(
                "Choose one ingredient."
            );
        }

        RefreshUI();
    }

    private void HandleIngredientSelected(
        IngredientDefinition ingredient
    )
    {
        if (shopManager == null)
        {
            return;
        }

        if (
            shopManager.ChooseIngredient(
                ingredient
            )
        )
        {
            SetIngredientOffersVisible(false);

            SetMessage(
                $"{ingredient.ingredientName} added to your ingredient pool!"
            );
        }

        RefreshUI();
    }

    private void HandleUpgradeSelected(
        RunUpgradeDefinition upgrade
    )
    {
        if (
            shopManager == null ||
            progress == null
        )
        {
            return;
        }

        if (
            progress.Coins <
            upgrade.cost
        )
        {
            SetMessage(
                "Not enough coins."
            );

            return;
        }

        if (
            shopManager.PurchaseUpgrade(
                upgrade
            )
        )
        {
            SetMessage(
                $"{upgrade.upgradeName} purchased!"
            );
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
            shopManager.HasOpenIngredientCrate
        )
        {
            SetMessage(
                "Choose an ingredient from your crate first."
            );

            return;
        }

        SetIngredientOffersVisible(false);

        runManager.ContinueAfterShop();
    }

    private void RefreshUI()
    {
        RefreshCoins();

        RefreshCrateButton();

        RefreshIngredientOffers();

        RefreshUpgradeOffers();

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

    private void RefreshCrateButton()
    {
        if (
            openCrateButtonText != null &&
            shopManager != null
        )
        {
            openCrateButtonText.text =
                $"Open Crate - {shopManager.IngredientCrateCost} Coins";
        }

        if (
            openCrateButton != null &&
            shopManager != null
        )
        {
            openCrateButton.interactable =
                shopManager.CanOpenIngredientCrate();
        }
    }

    private void RefreshIngredientOffers()
    {
        if (
            ingredientOfferButtons == null ||
            shopManager == null
        )
        {
            return;
        }

        var choices =
            shopManager.CurrentIngredientChoices;

        bool hasOffers =
            choices.Count > 0;

        SetIngredientOffersVisible(
            hasOffers
        );

        for (
            int i = 0;
            i < ingredientOfferButtons.Length;
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
                button.gameObject.SetActive(
                    true
                );

                button.Setup(
                    choices[i],
                    HandleIngredientSelected
                );
            }
            else
            {
                button.gameObject.SetActive(
                    false
                );
            }
        }
    }

    private void RefreshUpgradeOffers()
    {
        if (
            upgradeOfferButtons == null ||
            shopManager == null
        )
        {
            return;
        }

        var choices =
            shopManager.CurrentUpgradeChoices;

        for (
            int i = 0;
            i < upgradeOfferButtons.Length;
            i++
        )
        {
            ShopUpgradeOfferButton button =
                upgradeOfferButtons[i];

            if (button == null)
            {
                continue;
            }

            if (i < choices.Count)
            {
                button.gameObject.SetActive(
                    true
                );

                bool canAfford =
                    progress != null &&
                    progress.Coins >=
                        choices[i].cost;

                button.Setup(
                    choices[i],
                    canAfford,
                    HandleUpgradeSelected
                );
            }
            else
            {
                button.gameObject.SetActive(
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