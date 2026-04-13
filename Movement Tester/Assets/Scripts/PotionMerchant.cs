using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PotionMerchant : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float interactionRange = 1.25f;
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptLabel;
    [SerializeField] private string interactionMessage = "Press E to buy potion";

    [Header("Potion Sale")]
    [SerializeField] private int goldCost = 5;
    [SerializeField] private int potionsPerPurchase = 1;

    private Collider2D merchantCollider;
    private PlayerResources playerResources;
    private Collider2D playerCollider;
    private PlayerResources currentBuyer;

    private void Awake()
    {
        merchantCollider = GetComponent<Collider2D>();

        if (promptRoot == null)
        {
            promptRoot = FindPromptRoot();
        }

        if (promptLabel == null && promptRoot != null)
        {
            promptLabel = promptRoot.GetComponentInChildren<TMP_Text>(true);
        }

        HidePrompt();
    }

    private void OnEnable()
    {
        HidePrompt();
    }

    private void Start()
    {
        playerResources = FindFirstObjectByType<PlayerResources>();
        if (playerResources != null)
        {
            playerCollider = playerResources.GetComponent<Collider2D>();
            if (playerCollider == null)
            {
                playerCollider = playerResources.GetComponentInChildren<Collider2D>();
            }
        }

        if (merchantCollider == null || playerResources == null || playerCollider == null)
        {
            return;
        }
    }

    private void Update()
    {
        bool isPlayerInRange = Vector2.Distance(transform.position, playerResources.transform.position) <= interactionRange;
        if (isPlayerInRange)
        {
            currentBuyer = playerResources;
            ShowPrompt();
        }
        else
        {
            currentBuyer = null;
            HidePrompt();
        }

        if (currentBuyer != null && Input.GetKeyDown(interactionKey))
        {
            TrySellPotion(currentBuyer);
        }
    }

    public bool TrySellPotion(PlayerResources buyer)
    {
        if (buyer == null || goldCost < 0 || potionsPerPurchase <= 0)
        {
            return false;
        }

        if (!buyer.SpendGold(goldCost))
        {
            return false;
        }

        buyer.AddPotions(potionsPerPurchase);
        return true;
    }

    private void ShowPrompt()
    {
        if (promptLabel != null)
        {
            promptLabel.text = interactionMessage + " (" + goldCost + " gold)";
        }

        if (promptRoot != null)
        {
            promptRoot.SetActive(true);
        }
    }

    private void HidePrompt()
    {
        if (promptRoot != null)
        {
            promptRoot.SetActive(false);
        }
    }

    private GameObject FindPromptRoot()
    {
        Transform promptTransform = transform.Find("InteractionPromptAnchor");
        return promptTransform != null ? promptTransform.gameObject : null;
    }
}
