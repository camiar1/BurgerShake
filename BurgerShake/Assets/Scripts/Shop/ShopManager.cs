using System.Collections.Generic;
using UnityEngine;

public enum ShopOfferType
{
    Crate,
    Helper
}

public class ShopOffer
{
    public ShopOfferType OfferType
    {
        get;
        private set;
    }

    public IngredientCrateDefinition Crate
    {
        get;
        private set;
    }

    public RunUpgradeDefinition Helper
    {
        get;
        private set;
    }

    public bool Purchased
    {
        get;
        private set;
    }

    public int Cost
    {
        get
        {
            if (
                OfferType ==
                    ShopOfferType.Crate
            )
            {
                return Crate != null
                    ? Crate.cost
                    : 0;
            }

            return Helper != null
                ? Helper.cost
                : 0;
        }
    }

    public string DisplayName
    {
        get
        {
            if (
                OfferType ==
                    ShopOfferType.Crate
            )
            {
                return Crate != null
                    ? Crate.crateName
                    : "Crate";
            }

            return Helper != null
                ? Helper.upgradeName
                : "Helper";
        }
    }

    public string Description
    {
        get
        {
            if (
                OfferType ==
                    ShopOfferType.Crate
            )
            {
                return Crate != null
                    ? Crate.description
                    : "";
            }

            return Helper != null
                ? Helper.description
                : "";
        }
    }

    public Sprite Icon
    {
        get
        {
            if (
                OfferType ==
                    ShopOfferType.Crate
            )
            {
                return Crate != null
                    ? Crate.icon
                    : null;
            }

            return Helper != null
                ? Helper.icon
                : null;
        }
    }

    private ShopOffer()
    {
    }

    public static ShopOffer CreateCrate(
        IngredientCrateDefinition crate
    )
    {
        ShopOffer offer =
            new ShopOffer();

        offer.OfferType =
            ShopOfferType.Crate;

        offer.Crate =
            crate;

        return offer;
    }

    public static ShopOffer CreateHelper(
        RunUpgradeDefinition helper
    )
    {
        ShopOffer offer =
            new ShopOffer();

        offer.OfferType =
            ShopOfferType.Helper;

        offer.Helper =
            helper;

        return offer;
    }

    public void MarkPurchased()
    {
        Purchased =
            true;
    }
}

public class ShopManager : MonoBehaviour
{
    [Header("Run")]
    [SerializeField]
    private RunProgress progress;

    [Header("Ingredients")]
    [SerializeField]
    private List<IngredientDefinition>
        allIngredients =
            new List<IngredientDefinition>();

    [Header("Crates")]
    [SerializeField]
    private List<IngredientCrateDefinition>
        availableCrates =
            new List<
                IngredientCrateDefinition
            >();

    [SerializeField]
    [Min(0)]
    private int crateOffersPerShop =
        2;

    [Header("Helpers")]
    [SerializeField]
    private List<RunUpgradeDefinition>
        availableHelpers =
            new List<
                RunUpgradeDefinition
            >();

    [SerializeField]
    [Min(0)]
    private int helperOffersPerShop =
        2;

    private readonly List<ShopOffer>
        currentOffers =
            new List<ShopOffer>();

    private readonly List<
        IngredientDefinition
    > currentIngredientChoices =
        new List<
            IngredientDefinition
        >();

    private ShopOffer activeCrateOffer;

    public IReadOnlyList<ShopOffer>
        CurrentOffers =>
            currentOffers;

    public IReadOnlyList<
        IngredientDefinition
    > CurrentIngredientChoices =>
        currentIngredientChoices;

    public bool HasOpenIngredientCrate =>
        activeCrateOffer != null;

    public ShopOffer ActiveCrateOffer =>
        activeCrateOffer;

    private void Awake()
    {
        if (progress == null)
        {
            progress =
                FindFirstObjectByType<
                    RunProgress
                >();
        }
    }

    public void BeginShop()
    {
        currentOffers.Clear();

        currentIngredientChoices.Clear();

        activeCrateOffer =
            null;

        GenerateCrateOffers();

        GenerateHelperOffers();

        ShuffleOffers();
    }

    public bool CanPurchaseOffer(
        ShopOffer offer
    )
    {
        if (
            progress == null ||
            offer == null ||
            offer.Purchased ||
            !currentOffers.Contains(
                offer
            ) ||
            HasOpenIngredientCrate
        )
        {
            return false;
        }

        return
            progress.Coins >=
            offer.Cost;
    }

    public bool OpenCrateOffer(
        ShopOffer offer
    )
    {
        if (
            offer == null ||
            offer.OfferType !=
                ShopOfferType.Crate ||
            offer.Crate == null ||
            !CanPurchaseOffer(
                offer
            )
        )
        {
            return false;
        }

        List<IngredientDefinition>
            candidates =
                GetIngredientCandidates(
                    offer.Crate
                );

        if (candidates.Count == 0)
        {
            return false;
        }

        if (
            !progress.TrySpendCoins(
                offer.Cost
            )
        )
        {
            return false;
        }

        offer.MarkPurchased();

        activeCrateOffer =
            offer;

        currentIngredientChoices.Clear();

        int choiceCount =
            Mathf.Min(
                offer.Crate.choices,
                candidates.Count
            );

        for (
            int i = 0;
            i < choiceCount;
            i++
        )
        {
            int randomIndex =
                Random.Range(
                    0,
                    candidates.Count
                );

            IngredientDefinition chosen =
                candidates[
                    randomIndex
                ];

            currentIngredientChoices.Add(
                chosen
            );

            candidates.RemoveAt(
                randomIndex
            );
        }

        return
            currentIngredientChoices
                .Count > 0;
    }

    public bool ChooseIngredient(
        IngredientDefinition ingredient
    )
    {
        if (
            progress == null ||
            activeCrateOffer == null ||
            activeCrateOffer.Crate == null ||
            ingredient == null ||
            !currentIngredientChoices
                .Contains(
                    ingredient
                )
        )
        {
            return false;
        }

        int copiesCurrentlyOwned =
            progress
                .GetIngredientCopies(
                    ingredient
                );

        int copiesToAdd;

        if (copiesCurrentlyOwned > 0)
        {
            copiesToAdd =
                activeCrateOffer
                    .Crate
                    .existingIngredientCopies;
        }
        else
        {
            copiesToAdd =
                activeCrateOffer
                    .Crate
                    .newIngredientCopies;
        }

        progress.AddIngredientCopies(
            ingredient,
            copiesToAdd
        );

        currentIngredientChoices.Clear();

        activeCrateOffer =
            null;

        return true;
    }

    public int GetCopiesGrantedByActiveCrate(
        IngredientDefinition ingredient
    )
    {
        if (
            progress == null ||
            ingredient == null ||
            activeCrateOffer == null ||
            activeCrateOffer.Crate == null
        )
        {
            return 0;
        }

        int owned =
            progress
                .GetIngredientCopies(
                    ingredient
                );

        return owned > 0
            ? activeCrateOffer
                .Crate
                .existingIngredientCopies
            : activeCrateOffer
                .Crate
                .newIngredientCopies;
    }

    public bool PurchaseHelperOffer(
        ShopOffer offer
    )
    {
        if (
            offer == null ||
            offer.OfferType !=
                ShopOfferType.Helper ||
            offer.Helper == null ||
            !CanPurchaseOffer(
                offer
            )
        )
        {
            return false;
        }

        if (
            progress.HasUpgrade(
                offer.Helper
            )
        )
        {
            return false;
        }

        if (
            !progress.TrySpendCoins(
                offer.Cost
            )
        )
        {
            return false;
        }

        progress.AddUpgrade(
            offer.Helper
        );

        offer.MarkPurchased();

        return true;
    }

    private void GenerateCrateOffers()
    {
        List<IngredientCrateDefinition>
            candidates =
                new List<
                    IngredientCrateDefinition
                >();

        foreach (
            IngredientCrateDefinition crate
            in availableCrates
        )
        {
            if (
                crate == null ||
                candidates.Contains(
                    crate
                )
            )
            {
                continue;
            }

            candidates.Add(
                crate
            );
        }

        int count =
            Mathf.Min(
                crateOffersPerShop,
                candidates.Count
            );

        for (
            int i = 0;
            i < count;
            i++
        )
        {
            int index =
                Random.Range(
                    0,
                    candidates.Count
                );

            IngredientCrateDefinition crate =
                candidates[index];

            currentOffers.Add(
                ShopOffer.CreateCrate(
                    crate
                )
            );

            candidates.RemoveAt(
                index
            );
        }
    }

    private void GenerateHelperOffers()
    {
        List<RunUpgradeDefinition>
            candidates =
                new List<
                    RunUpgradeDefinition
                >();

        foreach (
            RunUpgradeDefinition helper
            in availableHelpers
        )
        {
            if (
                helper == null ||
                candidates.Contains(
                    helper
                )
            )
            {
                continue;
            }

            if (
                progress != null &&
                progress.HasUpgrade(
                    helper
                )
            )
            {
                continue;
            }

            candidates.Add(
                helper
            );
        }

        int count =
            Mathf.Min(
                helperOffersPerShop,
                candidates.Count
            );

        for (
            int i = 0;
            i < count;
            i++
        )
        {
            int index =
                Random.Range(
                    0,
                    candidates.Count
                );

            RunUpgradeDefinition helper =
                candidates[index];

            currentOffers.Add(
                ShopOffer.CreateHelper(
                    helper
                )
            );

            candidates.RemoveAt(
                index
            );
        }
    }

    private List<IngredientDefinition>
        GetIngredientCandidates(
            IngredientCrateDefinition crate
        )
    {
        List<IngredientDefinition>
            candidates =
                new List<
                    IngredientDefinition
                >();

        if (crate == null)
        {
            return candidates;
        }

        foreach (
            IngredientDefinition ingredient
            in allIngredients
        )
        {
            if (
                ingredient == null ||
                candidates.Contains(
                    ingredient
                )
            )
            {
                continue;
            }

            if (
                !ingredient.HasTag(
                    crate.requiredTag
                )
            )
            {
                continue;
            }

            candidates.Add(
                ingredient
            );
        }

        return candidates;
    }

    private void ShuffleOffers()
    {
        for (
            int i =
                currentOffers.Count - 1;
            i > 0;
            i--
        )
        {
            int randomIndex =
                Random.Range(
                    0,
                    i + 1
                );

            ShopOffer temp =
                currentOffers[i];

            currentOffers[i] =
                currentOffers[
                    randomIndex
                ];

            currentOffers[
                randomIndex
            ] =
                temp;
        }
    }
}