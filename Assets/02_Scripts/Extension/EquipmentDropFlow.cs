using System;
using System.Collections.Generic;
using UnityEngine;

public static class EquipmentDropFlow
{
    private static readonly Queue<EquipmentModel> PendingDrops =
        new Queue<EquipmentModel>();

    private static bool _isResolvingDrop;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        PendingDrops.Clear();
        _isResolvingDrop = false;
    }

    public static void ProcessMonsterKill(Action processCombatKill)
    {
        List<EquipmentModel> equipmentList =
            GameManager.Instance?.SaveServer?.GetEquipments();
        var equipmentIdsBeforeKill = new HashSet<long>();

        if (equipmentList != null)
        {
            foreach (EquipmentModel equipment in equipmentList)
            {
                if (equipment != null)
                {
                    equipmentIdsBeforeKill.Add(equipment.ItemUniqueId);
                }
            }
        }

        processCombatKill?.Invoke();

        if (equipmentList == null)
        {
            return;
        }

        foreach (EquipmentModel equipment in equipmentList)
        {
            if (equipment == null ||
                equipmentIdsBeforeKill.Contains(equipment.ItemUniqueId))
            {
                continue;
            }

            PendingDrops.Enqueue(equipment);
            Debug.Log(
                $"[EquipmentDrop] 드랍 감지 | " +
                $"ID: {equipment.ItemDataId} | Lv.{equipment.Level}"
            );
        }

        TryResolveNextDrop();
    }

    private static void TryResolveNextDrop()
    {
        if (_isResolvingDrop)
        {
            return;
        }

        while (PendingDrops.Count > 0)
        {
            EquipmentModel droppedEquipment = PendingDrops.Dequeue();
            _isResolvingDrop = true;

            if (ResolveDroppedEquipment(droppedEquipment))
            {
                return;
            }

            _isResolvingDrop = false;
        }
    }

    // 팝업 선택을 기다리는 경우 true, 즉시 처리가 끝난 경우 false를 반환한다.
    private static bool ResolveDroppedEquipment(
        EquipmentModel droppedEquipment)
    {
        GameManager gameManager = GameManager.Instance;
        EquipmentManager equipmentManager =
            gameManager?.Growth?.Equipment;
        PlayerModel playerModel =
            gameManager?.SaveServer?.GetPlayerModel();
        EquipmentItem droppedData = gameManager?.Data?.GetEquipmentData(
            droppedEquipment?.ItemDataId
        );

        if (equipmentManager == null || playerModel == null ||
            droppedEquipment == null || droppedData == null)
        {
            Debug.LogError(
                "[EquipmentDrop] 드랍 장비 처리에 필요한 데이터가 없습니다."
            );
            return false;
        }

        List<EquipmentModel> equippedItems =
            equipmentManager.GetEquippedItems(droppedData.Type);

        if (droppedData.Type == EquipmentType.Ring)
        {
            return ResolveRingDrop(
                equipmentManager,
                playerModel,
                droppedData,
                droppedEquipment,
                equippedItems
            );
        }

        if (equippedItems.Count == 0)
        {
            if (!AutoEquip(
                equipmentManager,
                droppedData,
                droppedEquipment,
                droppedData.Type.ToString()
            ))
            {
                DismantleWithLog(
                    equipmentManager,
                    droppedEquipment,
                    playerModel,
                    "빈 슬롯 자동 장착 실패"
                );
            }
            SaveResolvedDrop();
            return false;
        }

        EquipmentModel currentEquipment = equippedItems[0];
        EquipmentItem currentData =
            gameManager.Data.GetEquipmentData(currentEquipment.ItemDataId);

        EquipmentChestResultPanelUI popup = OpenDropPopup();
        if (popup == null)
        {
            Debug.LogError(
                "[EquipmentDrop] 장비 선택 팝업을 열지 못해 드랍 장비를 분해합니다."
            );
            DismantleWithLog(
                equipmentManager,
                droppedEquipment,
                playerModel,
                "팝업 생성 실패"
            );
            SaveResolvedDrop();
            return false;
        }

        popup.ShowDropSelection(
            currentData,
            currentEquipment,
            droppedData,
            droppedEquipment,
            selectedEquipment => ResolveSingleSelection(
                equipmentManager,
                playerModel,
                currentEquipment,
                droppedEquipment,
                selectedEquipment
            )
        );

        return true;
    }

    private static bool ResolveRingDrop(
        EquipmentManager equipmentManager,
        PlayerModel playerModel,
        EquipmentItem droppedData,
        EquipmentModel droppedEquipment,
        List<EquipmentModel> equippedRings)
    {
        if (equippedRings.Count < 2)
        {
            if (!AutoEquip(
                equipmentManager,
                droppedData,
                droppedEquipment,
                $"Ring{equippedRings.Count + 1}"
            ))
            {
                DismantleWithLog(
                    equipmentManager,
                    droppedEquipment,
                    playerModel,
                    "빈 반지 슬롯 자동 장착 실패"
                );
            }
            SaveResolvedDrop();
            return false;
        }

        EquipmentModel firstRing = equippedRings[0];
        EquipmentModel secondRing = equippedRings[1];
        EquipmentChestResultPanelUI popup = OpenDropPopup();

        if (popup == null)
        {
            Debug.LogError(
                "[EquipmentDrop] 반지 선택 팝업을 열지 못해 드랍 반지를 분해합니다."
            );
            DismantleWithLog(
                equipmentManager,
                droppedEquipment,
                playerModel,
                "팝업 생성 실패"
            );
            SaveResolvedDrop();
            return false;
        }

        popup.ShowRingSelection(
            firstRing,
            secondRing,
            droppedEquipment,
            selectedRings => ResolveRingSelection(
                equipmentManager,
                playerModel,
                firstRing,
                secondRing,
                droppedEquipment,
                selectedRings
            )
        );

        return true;
    }

    private static EquipmentChestResultPanelUI OpenDropPopup()
    {
        UIBase uiBase = GameManager.Instance?.UI?.OpenPopupUI(
            UIType.EquipmentChestResultPanelUI
        );
        return uiBase as EquipmentChestResultPanelUI;
    }

    private static void ResolveSingleSelection(
        EquipmentManager equipmentManager,
        PlayerModel playerModel,
        EquipmentModel currentEquipment,
        EquipmentModel droppedEquipment,
        EquipmentModel selectedEquipment)
    {
        if (ReferenceEquals(selectedEquipment, droppedEquipment))
        {
            if (equipmentManager.TryEquipDroppedItem(droppedEquipment))
            {
                LogEquipped(droppedEquipment, "드랍 장비 선택");
                DismantleWithLog(
                    equipmentManager,
                    currentEquipment,
                    playerModel,
                    "기존 장비 미선택"
                );
            }
            else
            {
                Debug.LogError(
                    "[EquipmentDrop] 드랍 장비 장착에 실패해 기존 장비를 유지합니다."
                );
                DismantleWithLog(
                    equipmentManager,
                    droppedEquipment,
                    playerModel,
                    "장착 실패"
                );
            }
        }
        else if (ReferenceEquals(selectedEquipment, currentEquipment))
        {
            LogEquipped(currentEquipment, "기존 장비 유지");
            DismantleWithLog(
                equipmentManager,
                droppedEquipment,
                playerModel,
                "드랍 장비 미선택"
            );
        }
        else
        {
            Debug.LogError("[EquipmentDrop] 선택 결과가 비교 대상과 일치하지 않습니다.");
            DismantleWithLog(
                equipmentManager,
                droppedEquipment,
                playerModel,
                "잘못된 선택 결과"
            );
        }

        FinishPopupResolution();
    }

    private static void ResolveRingSelection(
        EquipmentManager equipmentManager,
        PlayerModel playerModel,
        EquipmentModel firstRing,
        EquipmentModel secondRing,
        EquipmentModel droppedRing,
        IReadOnlyList<EquipmentModel> selectedRings)
    {
        if (!equipmentManager.TryApplyRingSelection(selectedRings))
        {
            Debug.LogError(
                "[EquipmentDrop] 반지 선택 적용에 실패해 기존 반지를 유지합니다."
            );
            firstRing.IsEquipped = true;
            secondRing.IsEquipped = true;
            droppedRing.IsEquipped = false;
            GameManager.Instance.Growth.RecalculateTotalStats();
            DismantleWithLog(
                equipmentManager,
                droppedRing,
                playerModel,
                "반지 선택 적용 실패"
            );
            FinishPopupResolution();
            return;
        }

        foreach (EquipmentModel selectedRing in selectedRings)
        {
            LogEquipped(selectedRing, "반지 선택 확정");
        }

        EquipmentModel[] candidates =
            { firstRing, secondRing, droppedRing };
        foreach (EquipmentModel candidate in candidates)
        {
            if (!ContainsReference(selectedRings, candidate))
            {
                DismantleWithLog(
                    equipmentManager,
                    candidate,
                    playerModel,
                    "반지 2개 선택에서 제외"
                );
            }
        }

        FinishPopupResolution();
    }

    private static bool AutoEquip(
        EquipmentManager equipmentManager,
        EquipmentItem droppedData,
        EquipmentModel droppedEquipment,
        string slotLabel)
    {
        if (equipmentManager.TryEquipDroppedItem(droppedEquipment))
        {
            Debug.Log(
                $"[EquipmentDrop] 빈 슬롯 자동 장착 | " +
                $"Slot: {slotLabel} | ID: {droppedData.Id} | " +
                $"Lv.{droppedEquipment.Level}"
            );
            return true;
        }

        Debug.LogError(
            $"[EquipmentDrop] 빈 슬롯 자동 장착 실패 | ID: {droppedData.Id}"
        );
        return false;
    }

    private static void LogEquipped(
        EquipmentModel equipment,
        string reason)
    {
        Debug.Log(
            $"[EquipmentDrop] 장착 완료 | {reason} | " +
            $"ID: {equipment.ItemDataId} | Lv.{equipment.Level}"
        );
    }

    private static void DismantleWithLog(
        EquipmentManager equipmentManager,
        EquipmentModel equipment,
        PlayerModel playerModel,
        string reason)
    {
        if (equipment == null)
        {
            return;
        }

        equipment.IsEquipped = false;
        List<EquipmentModel> equipmentList =
            GameManager.Instance?.SaveServer?.GetEquipments();
        long currencyBefore = playerModel.EnhanceCurrency;
        equipmentManager.DismantleItem(equipment, playerModel);
        long gainedCurrency = playerModel.EnhanceCurrency - currencyBefore;

        if (equipmentList != null && equipmentList.Contains(equipment))
        {
            Debug.LogError(
                $"[EquipmentDrop] 분해 실패 | {reason} | " +
                $"ID: {equipment.ItemDataId}"
            );
            return;
        }

        Debug.Log(
            $"[EquipmentDrop] 분해 완료 | {reason} | " +
            $"ID: {equipment.ItemDataId} | Lv.{equipment.Level} | " +
            $"EnhanceCurrency +{gainedCurrency}"
        );
    }

    private static bool ContainsReference(
        IReadOnlyList<EquipmentModel> equipmentList,
        EquipmentModel target)
    {
        if (equipmentList == null)
        {
            return false;
        }

        for (int i = 0; i < equipmentList.Count; i++)
        {
            if (ReferenceEquals(equipmentList[i], target))
            {
                return true;
            }
        }

        return false;
    }

    private static void SaveResolvedDrop()
    {
        GameManager.Instance?.SaveServer?.SaveGameData();
    }

    private static void FinishPopupResolution()
    {
        SaveResolvedDrop();
        _isResolvingDrop = false;
        TryResolveNextDrop();
    }
}
