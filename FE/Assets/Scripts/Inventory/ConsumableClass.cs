using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Consum", menuName = "Item/Consumable")]

public class ConsumableClass : ItemClass
{
    [Header("Consumable Effects")]
    public int healthRecovery;
    public int kiRecovery;
    public override ItemClass GetItem() { return this; }
    public override ToolClass GetTool() { return null; }
    public override MiscClass GetMisc() { return null; }
    public override ConsumableClass GetConsumable() { return this; }
}
