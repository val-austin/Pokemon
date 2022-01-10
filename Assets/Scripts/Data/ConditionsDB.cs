using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB 
{
    
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisioned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.BaseStats.Name} ate some gluten.");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.MaxHp / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.BaseStats.Name} is cooking.");
                }
            }
        }
    };
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz
}
