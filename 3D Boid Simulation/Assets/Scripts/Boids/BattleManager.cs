using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : Singleton
{

    public static new BattleManager _instance;

    [SerializeField]
    private List<Combatant_ABS> combatants = new List<Combatant_ABS>();

    void Awake()
    {
        //Singelton
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple singleton instances; deleting script on " + this.gameObject);
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

    public void AddCombatant(Combatant_ABS comb)
    {
        combatants.Add(comb);
    }

    public void RemoveCombatant(Combatant_ABS comb)
    {
        combatants.Remove(comb);
    }

    public List<Combatant_ABS> GetCombatants ()
    {
        return combatants;
    }

}
