using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerResources : MonoBehaviour
{
    [Header("Starting Resources")]
    [SerializeField] private int startingGold;
    [SerializeField] private int startingPotionCount;
    [SerializeField] private bool resetResourcesOnEnable = false;

    public event Action<int> GoldChanged;
    public event Action<int> PotionCountChanged;

    public int Gold { get; private set; }
    public int PotionCount { get; private set; }

    private void Awake()
    {
        Gold = Mathf.Max(0, startingGold);
        PotionCount = Mathf.Max(0, startingPotionCount);
    }

    private void OnEnable()
    {
        if (PlayerRuntimeState.Instance.HasCurrentState)
        {
            NotifyResourceChanges();
            return;
        }

        if (resetResourcesOnEnable)
        {
            ResetResources();
        }
        else
        {
            NotifyResourceChanges();
        }
    }

    public void ResetResources()
    {
        Gold = Mathf.Max(0, startingGold);
        PotionCount = Mathf.Max(0, startingPotionCount);
        NotifyResourceChanges();
        PlayerRuntimeState.Instance.CaptureResourceState(this);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Gold += amount;
        GoldChanged?.Invoke(Gold);
        PlayerRuntimeState.Instance.CaptureResourceState(this);
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (Gold < amount)
        {
            return false;
        }

        Gold -= amount;
        GoldChanged?.Invoke(Gold);
        PlayerRuntimeState.Instance.CaptureResourceState(this);
        return true;
    }

    public void AddPotions(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        PotionCount += amount;
        PotionCountChanged?.Invoke(PotionCount);
        PlayerRuntimeState.Instance.CaptureResourceState(this);
    }

    public bool ConsumePotion(int amount = 1)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (PotionCount < amount)
        {
            return false;
        }

        PotionCount -= amount;
        PotionCountChanged?.Invoke(PotionCount);
        PlayerRuntimeState.Instance.CaptureResourceState(this);
        return true;
    }

    public void ApplyState(int gold, int potionCount)
    {
        Gold = Mathf.Max(0, gold);
        PotionCount = Mathf.Max(0, potionCount);
        NotifyResourceChanges();
        PlayerRuntimeState.Instance.CaptureResourceState(this);
    }

    private void NotifyResourceChanges()
    {
        GoldChanged?.Invoke(Gold);
        PotionCountChanged?.Invoke(PotionCount);
    }
}
