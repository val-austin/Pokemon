using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB 
{
    public static void Init()
    {
        foreach(var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }
    
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
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyze",
                StartMessage = "has been paralzyed",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1,5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.BaseStats.Name} is paralyzed. It can't move.");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1,5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.BaseStats.Name} thawed out..");
                        return true;
                    }
                    pokemon.StatusChanges.Enqueue($"{pokemon.BaseStats.Name} is frozen.");
                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep.",
                OnStart = (Pokemon pokemon) =>
                {
                    //sleep for 1-3 turns
                    pokemon.StatusTime = Random.Range(1,4);
                    Debug.Log($"Will be asleep for {pokemon.StatusTime} turns");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(pokemon.StatusTime == 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.BaseStats.Name} has woken up.");
                        return true;
                    }
                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.BaseStats.Name} is sleeping.");
                    return false;
                }
            }
        },
        //Volatile Status
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused.",
                OnStart = (Pokemon pokemon) =>
                {
                    //sleep for 1-3 turns
                    pokemon.VolatileStatusTime = Random.Range(1,5);
                    Debug.Log($"Will be confused for {pokemon.VolatileStatusTime} turns");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(pokemon.VolatileStatusTime == 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.BaseStats.Name} snapped out of confusion!");
                        return true;
                    }
                    pokemon.VolatileStatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.BaseStats.Name} is confused.");
                    if(Random.Range(1,3) == 1)
                    {
                        return true;
                    }
                    //Hurt in confusion
                    pokemon.UpdateHp(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.BaseStats.Name} hurt itself in confusion.");
                    return false;
                }
            }
        }
    };
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz,
    confusion
}
