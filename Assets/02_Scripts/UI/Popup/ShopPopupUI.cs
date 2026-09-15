using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPopupUI : UIBase
{
    private const long EquipmentLowChestPrice = 100;
    private const long EquipmentHighChestPrice = 10000;

    private const int RelicLowChestPrice = 50;
    private const int RelicHighChestPrice = 300;

    private const int UniqueIdRetryCount = 8;

    private enum ChestTier
    {
        Low,
        High
    }

    [Header("Buttons")]
    [SerializeField] private UIButton Button_Close;
    [SerializeField] private UIButton Button_EquipmentLowChest;
    [SerializeField] private UIButton Button_EquipmentHighChest;
    [SerializeField] private UIButton Button_RelicLowChest;
    [SerializeField] private UIButton Button_RelicHighChest;

    private EquipmentChestResultPanelUI _equipmentResultPanel;
    private RelicChestResultPanelUI _relicResultPanel;

    private GameObject _backgroundOverlay;

    private void Awake()
    {
        EnsureEquipmentResultPanel();
        CreateBackgroundOverlay();
    }

    private void OnEnable()
    {
        SetBackgroundOverlayActive(true);

        Button_Close?.BindOnClickButtonEvent(OnClickClose);
        Button_EquipmentLowChest?.BindOnClickButtonEvent(() => OnClickEquipmentChest(ChestTier.Low));
        Button_EquipmentHighChest?.BindOnClickButtonEvent(() => OnClickEquipmentChest(ChestTier.High));
        Button_RelicLowChest?.BindOnClickButtonEvent(() => OnClickRelicChest(ChestTier.Low));
        Button_RelicHighChest?.BindOnClickButtonEvent(() => OnClickRelicChest(ChestTier.High));
    }

    private void OnDisable()
    {
        SetBackgroundOverlayActive(false);

        Button_Close?.UnBindAllOnClickButtonEvent();
        Button_EquipmentLowChest?.UnBindAllOnClickButtonEvent();
        Button_EquipmentHighChest?.UnBindAllOnClickButtonEvent();
        Button_RelicLowChest?.UnBindAllOnClickButtonEvent();
        Button_RelicHighChest?.UnBindAllOnClickButtonEvent();

        _equipmentResultPanel?.Hide();
        _relicResultPanel?.Hide();
    }

    private void OnDestroy()
    {
        if (_backgroundOverlay != null)
        {
            Destroy(_backgroundOverlay);
        }
    }

    private void CreateBackgroundOverlay()
    {
        if (_backgroundOverlay != null || transform.parent == null)
        {
            return;
        }

        GameObject overlay = new GameObject(
            "ShopPopupBackdrop",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );

        overlay.layer = gameObject.layer;

        RectTransform rectTransform = overlay.GetComponent<RectTransform>();

        rectTransform.SetParent(transform.parent, false);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = overlay.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.4f);

        Button button = overlay.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = image;
        button.onClick.AddListener(OnClickClose);

        overlay.transform.SetSiblingIndex(transform.GetSiblingIndex());

        overlay.SetActive(false);
        _backgroundOverlay = overlay;
    }

    private void SetBackgroundOverlayActive(bool isActive)
    {
        if (_backgroundOverlay == null)
        {
            CreateBackgroundOverlay();
        }

        if (_backgroundOverlay != null)
        {
            _backgroundOverlay.SetActive(isActive);
        }
    }

    private bool IsShopResultVisible()
    {
        return (_equipmentResultPanel != null && _equipmentResultPanel.IsVisible) || (_relicResultPanel != null && _relicResultPanel.IsVisible);
    }

    private void OnClickClose()
    {
        if (EquipmentDropFlow.IsBusy || IsShopResultVisible())
        {
            return;
        }

        GameManager.Instance?.UI?.ClosePopupUI(UIType.ShopPopupUI);
    }

    private void OnClickEquipmentChest(ChestTier tier)
    {
        if (EquipmentDropFlow.IsBusy || IsShopResultVisible())
        {
            return;
        }

        EnsureEquipmentResultPanel();

        if (_equipmentResultPanel == null)
        {
            Debug.LogError("[ShopPopupUI] 결과창을 생성하지 못했습니다.");
            return;
        }

        GameManager gameManager = GameManager.Instance;
        GrowthManager growth = gameManager?.Growth;
        SaveServerManager saveServer = gameManager?.SaveServer;
        PlayerModel playerModel = saveServer?.GetPlayerModel();

        if (gameManager == null ||
            gameManager.Data == null ||
            growth == null ||
            !growth.IsInitialized ||
            growth.Equipment == null ||
            saveServer == null ||
            playerModel == null ||
            saveServer.GetEquipments() == null)
        {
            ShowPurchaseFailure("상점 데이터를 불러오지 못했습니다.");
            return;
        }

        long chestPrice = tier == ChestTier.Low ? EquipmentLowChestPrice : EquipmentHighChestPrice;

        if (playerModel.Gold < chestPrice)
        {
            ShowPurchaseFailure($"골드가 부족합니다.\n" + $"필요 골드: {chestPrice:N0}\n" + $"보유 골드: {playerModel.Gold:N0}");
            return;
        }

        List<EquipmentItem> equipmentList = GetAllEquipmentItems();

        List<EquipmentItem> candidates = GetEquipmentItemsByTier(equipmentList, tier);

        EquipmentItem selectedEquipment = GetRandomEquipment(candidates);

        if (selectedEquipment == null)
        {
            ShowPurchaseFailure("추첨 가능한 장비가 없습니다.");
            return;
        }

        EquipmentModel newEquipment = TryCreateAndAddEquipment(growth.Equipment, selectedEquipment);

        if (newEquipment == null)
        {
            ShowPurchaseFailure("획득 장비를 등록하지 못했습니다.\n" + "골드는 차감되지 않았습니다.");
            return;
        }

        playerModel.Gold -= chestPrice;

        saveServer.SaveGameData();

        Debug.Log(
            $"[ShopPopupUI] 장비 상자 구매 완료 | " +
            $"비용: {chestPrice:N0} Gold | " +
            $"ID: {newEquipment.ItemDataId} | " +
            $"Lv.{newEquipment.Level}"
        );

        EquipmentDropFlow.ProcessPurchasedEquipment(newEquipment);
    }

    private EquipmentModel TryCreateAndAddEquipment(EquipmentManager equipmentManager, EquipmentItem equipmentData)
    {
        if (equipmentManager == null || equipmentData == null)
        {
            return null;
        }

        for (int attempt = 0; attempt < UniqueIdRetryCount; attempt++)
        {
            EquipmentModel equipmentModel = new EquipmentModel
            {
                ItemUniqueId = CreateEquipmentUniqueId(),
                ItemDataId = equipmentData.Id,
                Level = 1,
                IsEquipped = false
            };

            if (equipmentManager.TryAddEquipment(equipmentModel))
            {
                return equipmentModel;
            }
        }

        return null;
    }

    private long CreateEquipmentUniqueId()
    {
        long uniqueId = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0) & long.MaxValue;

        return uniqueId == 0 ? 1 : uniqueId;
    }

    private void EnsureEquipmentResultPanel()
    {
        if (_equipmentResultPanel != null)
        {
            return;
        }

        GameObject resultPanelPrefab = Resources.Load<GameObject>("PopupUI/EquipmentChestResultPanelUI");

        if (resultPanelPrefab == null)
        {
            Debug.LogError("[ShopPopupUI] 결과창 프리팹을 찾지 못했습니다.");
            return;
        }

        GameObject resultPanelObject = Instantiate(resultPanelPrefab, transform, false);

        _equipmentResultPanel = resultPanelObject.GetComponent<EquipmentChestResultPanelUI>();

        if (_equipmentResultPanel == null)
        {
            Debug.LogError(
                "[ShopPopupUI] 결과창 프리팹에 " +
                "EquipmentChestResultPanelUI가 없습니다."
            );

            Destroy(resultPanelObject);
        }
    }

    private void EnsureRelicResultPanel()
    {
        if (_relicResultPanel != null)
        {
            return;
        }

        GameObject resultPanelPrefab = Resources.Load<GameObject>("PopupUI/RelicChestResultPanelUI");

        if (resultPanelPrefab == null)
        {
            Debug.LogError("[ShopPopupUI] 유물 결과창 프리팹을 찾지 못했습니다.");
            return;
        }

        GameObject resultPanelObject = Instantiate(resultPanelPrefab, transform, false);

        _relicResultPanel = resultPanelObject.GetComponent<RelicChestResultPanelUI>();

        if (_relicResultPanel == null)
        {
            Debug.LogError(
                "[ShopPopupUI] 유물 결과창 프리팹에 " +
                "RelicChestResultPanelUI가 없습니다."
            );

            Destroy(resultPanelObject);
        }
    }

    private void ShowPurchaseFailure(string message)
    {
        ShowNotice("구매 실패", message);
    }

    private void ShowNotice(string title, string message)
    {
        EnsureEquipmentResultPanel();

        if (_equipmentResultPanel != null)
        {
            _equipmentResultPanel.ShowNotice(title, message);
        }
        else
        {
            Debug.LogWarning(
                $"[ShopPopupUI] {title}: {message}"
            );
        }
    }

    private List<EquipmentItem> GetAllEquipmentItems()
    {
        GameManager gameManager = GameManager.Instance;

        if (gameManager == null || gameManager.Data == null)
        {
            Debug.LogWarning("[ShopPopupUI] 장비 데이터 관리자가 없습니다.");

            return new List<EquipmentItem>();
        }

        List<EquipmentItem> equipmentList = gameManager.Data.GetAllEquipmentDataList();

        if (equipmentList == null)
        {
            Debug.LogWarning("[ShopPopupUI] 장비 목록을 가져오지 못했습니다.");

            return new List<EquipmentItem>();
        }

        return equipmentList;
    }

    private List<EquipmentItem> GetEquipmentItemsByTier(List<EquipmentItem> equipmentList, ChestTier tier)
    {
        List<EquipmentItem> result = new List<EquipmentItem>();

        if (equipmentList == null)
        {
            return result;
        }

        foreach (EquipmentItem equipment in equipmentList)
        {
            if (equipment == null)
            {
                continue;
            }

            bool isTargetGrade;

            if (tier == ChestTier.Low)
            {
                isTargetGrade =
                    equipment.Grade == EquipmentGrade.Common ||
                    equipment.Grade == EquipmentGrade.Rare;
            }
            else
            {
                isTargetGrade =
                    equipment.Grade == EquipmentGrade.Epic ||
                    equipment.Grade == EquipmentGrade.Legendary ||
                    equipment.Grade == EquipmentGrade.Mythic;
            }

            if (isTargetGrade)
            {
                result.Add(equipment);
            }
        }

        return result;
    }

    private EquipmentItem GetRandomEquipment(List<EquipmentItem> candidates)
    {
        if (candidates == null || candidates.Count == 0)
        {
            Debug.LogWarning("[ShopPopupUI] 추첨 가능한 장비가 없습니다.");
            return null;
        }

        int totalWeight = 0;

        foreach (EquipmentItem equipment in candidates)
        {
            if (equipment == null)
            {
                continue;
            }

            totalWeight += Mathf.Max(0, equipment.DropWeight);
        }

        if (totalWeight <= 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, candidates.Count);

            return candidates[randomIndex];
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        int accumulatedWeight = 0;

        foreach (EquipmentItem equipment in candidates)
        {
            if (equipment == null)
            {
                continue;
            }

            accumulatedWeight += Mathf.Max(0, equipment.DropWeight);

            if (randomValue < accumulatedWeight)
            {
                return equipment;
            }
        }

        return candidates[candidates.Count - 1];
    }

    private void OnClickRelicChest(ChestTier tier)
    {
        if (EquipmentDropFlow.IsBusy || IsShopResultVisible())
        {
            return;
        }

        EnsureRelicResultPanel();

        if (_relicResultPanel == null)
        {
            ShowPurchaseFailure(
                "유물 결과창 프리팹을 찾지 못했습니다."
            );
            return;
        }

        GameManager gameManager = GameManager.Instance;
        GrowthManager growth = gameManager?.Growth;
        SaveServerManager saveServer = gameManager?.SaveServer;
        PlayerModel playerModel = saveServer?.GetPlayerModel();

        if (gameManager == null ||
            growth == null ||
            !growth.IsInitialized ||
            saveServer == null ||
            playerModel == null)
        {
            ShowPurchaseFailure("유물 데이터를 불러오지 못했습니다.");
            return;
        }

        RelicManager relicManager = FindFirstObjectByType<RelicManager>();

        if (relicManager == null)
        {
            ShowPurchaseFailure("유물 관리자를 찾지 못했습니다.");
            return;
        }

        int chestPrice = tier == ChestTier.Low ? RelicLowChestPrice : RelicHighChestPrice;

        if (playerModel.RebirthPoints < chestPrice)
        {
            ShowPurchaseFailure(
                $"환생 포인트가 부족합니다.\n" +
                $"필요 포인트: {chestPrice:N0}\n" +
                $"보유 포인트: {playerModel.RebirthPoints:N0}"
            );
            return;
        }

        EquipmentGrade[] availableGrades =
            tier == ChestTier.Low
                ? new[]
                {
                    EquipmentGrade.Common,
                    EquipmentGrade.Rare
                }
                : new[]
                {
                    EquipmentGrade.Epic,
                    EquipmentGrade.Legendary
                };

        if (!relicManager.TryDrawRelic(
                playerModel,
                chestPrice,
                availableGrades,
                out RelicDrawResult result))
        {
            ShowPurchaseFailure("추첨 가능한 유물이 없습니다.");
            return;
        }

        saveServer.SaveGameData();

        _relicResultPanel.Show(result);
    }
}