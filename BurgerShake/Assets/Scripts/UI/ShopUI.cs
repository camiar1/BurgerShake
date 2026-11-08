using System.Collections;
using TMPro;
using UnityEngine.EventSystems;
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

    private GameObject revealOverlay;
    private Image revealCrate;
    private TMP_Text revealTitle;
    private bool revealing;
    private Coroutine revealRoutine;

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

        if (shopManager != null)
        {
            shopManager.OffersChanged +=
                HandleOffersChanged;
        }
        if (revealOverlay != null && runManager != null && runManager.State == RunState.Shop)
            RefreshUI();
    }

    private void OnDisable()
    {
        CancelReveal();
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

        if (shopManager != null)
        {
            shopManager.OffersChanged -=
                HandleOffersChanged;
        }
    }

    private void Start()
    {
        BuildCardPresentation();
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

    private void HandleOffersChanged()
    {
        RefreshUI();
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
            CancelReveal();
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
                revealRoutine = StartCoroutine(RevealCrate(offer));

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

        if (revealing) return;

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

            SetIngredientOffersVisible(false);
            if (revealOverlay != null) revealOverlay.SetActive(false);
            if (EventSystem.current != null && continueButton != null)
                EventSystem.current.SetSelectedGameObject(continueButton.gameObject);

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

        if (revealOverlay != null && hasChoices && !revealing)
        {
            revealOverlay.SetActive(true);
            revealCrate.gameObject.SetActive(false);
            revealTitle.text = "Choose one ingredient for your pantry";
            CanvasGroup choicesGroup = ingredientOffersRoot.GetComponent<CanvasGroup>();
            if (choicesGroup != null) { choicesGroup.alpha = 1; choicesGroup.interactable = true; }
        }

        SetIngredientOffersVisible(
            hasChoices && !revealing
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

    private void BuildCardPresentation()
    {
        if (shopPanel == null) return;
        ShopCardLayout.Place((RectTransform)shopPanel.transform, 0, 0, 1, 1);
        Image shopBackground = shopPanel.GetComponent<Image>();
        if (shopBackground != null) shopBackground.color = new Color(.055f, .035f, .025f, .98f);
        if (coinsText != null)
        {
            coinsText.transform.SetParent(shopPanel.transform, false);
            ShopCardLayout.Place(coinsText.rectTransform, .04f, .86f, .30f, .94f);
            coinsText.color = BurgerShakeGeneratedUITheme.Paper;
        }
        if (messageText != null)
        {
            messageText.transform.SetParent(shopPanel.transform, false);
            ShopCardLayout.Place(messageText.rectTransform, .04f, .03f, .67f, .11f);
            messageText.color = BurgerShakeGeneratedUITheme.Paper;
        }
        if (continueButton != null)
        {
            continueButton.transform.SetParent(shopPanel.transform, false);
            ShopCardLayout.Place((RectTransform)continueButton.transform, .73f, .05f, .96f, .13f);
            BurgerShakeGeneratedUITheme.Button(continueButton, continueButton.GetComponentInChildren<TMP_Text>(), true);
        }
        Transform title = shopPanel.transform.Find("TitleText");
        if (title != null)
        {
            ShopCardLayout.Place((RectTransform)title, .30f, .86f, .68f, .96f);
            TMP_Text label = title.GetComponent<TMP_Text>();
            BurgerShakeGeneratedUITheme.Text(label, 36, true);
            if (label != null) { label.text = "THE SHOP"; label.color = BurgerShakeGeneratedUITheme.Paper; }
        }
        RectTransform grid = ShopCardLayout.Root("Shop card grid", shopPanel.transform);
        ShopCardLayout.Place(grid, .04f, .16f, .68f, .83f);
        for (int i = 0; i < shopOfferButtons.Length; i++)
        {
            if (shopOfferButtons[i] == null) continue;
            RectTransform card = (RectTransform)shopOfferButtons[i].transform;
            card.SetParent(grid, false);
            int column = i % 2;
            int row = i / 2;
            ShopCardLayout.Place(card, column * .51f, .51f - row * .51f,
                column * .51f + .49f, 1f - row * .51f);
        }

        RectTransform overlay = ShopCardLayout.Root("Crate reveal overlay", shopPanel.transform);
        ShopCardLayout.Place(overlay, 0, 0, 1, 1);
        revealOverlay = overlay.gameObject;
        Image shade = revealOverlay.AddComponent<Image>();
        shade.color = new Color(.015f, .02f, .035f, .96f);
        shade.raycastTarget = true;
        // A separate canvas keeps this modal above the shop's existing nested canvases.
        Canvas modalCanvas = revealOverlay.AddComponent<Canvas>();
        modalCanvas.overrideSorting = true;
        modalCanvas.sortingOrder = 1800;
        revealOverlay.AddComponent<GraphicRaycaster>();
        revealTitle = ShopCardLayout.Label("Reveal title", overlay, 32);
        ShopCardLayout.Place(revealTitle.rectTransform, .1f, .82f, .9f, .94f);
        RectTransform crate = ShopCardLayout.Root("Animated crate", overlay);
        ShopCardLayout.Place(crate, .39f, .36f, .61f, .70f);
        revealCrate = crate.gameObject.AddComponent<Image>();
        revealCrate.preserveAspect = true;
        revealCrate.raycastTarget = false;
        if (ingredientOffersRoot != null)
        {
            ingredientOffersRoot.transform.SetParent(overlay, false);
            ShopCardLayout.Place((RectTransform)ingredientOffersRoot.transform, .09f, .16f, .91f, .76f);
            Image oldBackground = ingredientOffersRoot.GetComponent<Image>();
            if (oldBackground != null) oldBackground.enabled = false;
            for (int i = 0; i < ingredientOfferButtons.Length; i++)
            {
                if (ingredientOfferButtons[i] == null) continue;
                RectTransform card = (RectTransform)ingredientOfferButtons[i].transform;
                card.SetParent(ingredientOffersRoot.transform, false);
                ShopCardLayout.Place(card, i / 3f + .012f, 0, (i + 1) / 3f - .012f, 1);
            }
        }
        revealOverlay.SetActive(false);
    }

    private IEnumerator RevealCrate(ShopOffer offer)
    {
        revealing = true;
        revealOverlay.SetActive(true);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        revealOverlay.transform.SetAsLastSibling();
        SetIngredientOffersVisible(false);
        revealTitle.text = offer.DisplayName;
        revealCrate.gameObject.SetActive(true);
        ShopCardLayout.Place(revealCrate.rectTransform, .39f, .36f, .61f, .70f);
        revealCrate.sprite = offer.Icon;
        revealCrate.color = Color.white;
        RectTransform crate = revealCrate.rectTransform;
        Vector2 origin = crate.anchoredPosition;
        for (float elapsed = 0; elapsed < .85f; elapsed += Time.unscaledDeltaTime)
        {
            float intensity = elapsed / .85f;
            crate.anchoredPosition = origin + new Vector2(Mathf.Sin(elapsed * 65) * 12 * intensity, 0);
            crate.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(elapsed * 48) * 9 * intensity);
            yield return null;
        }
        crate.anchoredPosition = origin;
        crate.localRotation = Quaternion.identity;
        revealCrate.sprite = ShopCardLayout.OpenCrateSprite;
        revealTitle.text = "Crate opened!";
        for (float elapsed = 0; elapsed < .4f; elapsed += Time.unscaledDeltaTime)
        {
            crate.localScale = Vector3.one * (1 + Mathf.Sin(elapsed / .4f * Mathf.PI) * .16f);
            yield return null;
        }
        crate.localScale = Vector3.one;
        revealCrate.gameObject.SetActive(false);
        revealing = false;
        RefreshIngredientOffers();
        revealTitle.text = offer.DisplayName + " — choose one ingredient";
        revealing = true;
        // Keep choosing locked until the cards have finished appearing.
        CanvasGroup choicesGroup = ingredientOffersRoot.GetComponent<CanvasGroup>();
        if (choicesGroup == null) choicesGroup = ingredientOffersRoot.AddComponent<CanvasGroup>();
        choicesGroup.interactable = false;
        for (float elapsed = 0; elapsed < .25f; elapsed += Time.unscaledDeltaTime)
        {
            choicesGroup.alpha = Mathf.Clamp01(elapsed / .25f);
            yield return null;
        }
        choicesGroup.alpha = 1;
        choicesGroup.interactable = true;
        revealing = false;
        if (EventSystem.current != null && ingredientOfferButtons.Length > 0 && ingredientOfferButtons[0] != null)
            EventSystem.current.SetSelectedGameObject(ingredientOfferButtons[0].gameObject);
        revealRoutine = null;
    }

    private void CancelReveal()
    {
        if (revealRoutine != null) StopCoroutine(revealRoutine);
        revealRoutine = null;
        revealing = false;
        if (revealCrate != null)
        {
            revealCrate.rectTransform.localScale = Vector3.one;
            revealCrate.rectTransform.localRotation = Quaternion.identity;
        }
        if (revealOverlay != null) revealOverlay.SetActive(false);
    }
}
