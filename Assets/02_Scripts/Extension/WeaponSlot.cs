using System;
using UnityEngine;

public enum WeaponType
{
    Sword = 0,
    Axe = 1,
    Spear = 2
}

[Serializable]
public struct WeaponSlot
{
    public WeaponType WeaponType;
    public GameObject WeaponObject;
    public WeaponHitbox WeaponHitbox;
}