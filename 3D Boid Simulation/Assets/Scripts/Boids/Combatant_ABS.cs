using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Combatant_ABS : MonoBehaviour
{
    protected enum Faction
    {
        Ally, // Blue on Blue
        // Blue on Green?
        Enemy,
        Civilian,
        Unknown
    }
    [SerializeField]
    protected Faction faction = Faction.Ally;

    protected virtual Faction GetFaction()
    {
        return faction;
    }

    protected virtual void RegisterCombatant() { BattleManager._instance.AddCombatant(this); }

    protected virtual void UnregisterCombatant() { BattleManager._instance.RemoveCombatant(this); }
}
