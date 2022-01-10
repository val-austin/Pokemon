using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    public PokemonBase BaseStats { 
        get {
            return _base;
        }
    }
    public int Level { 
        get {
            return level;
        } 
    }
    public int currHP { get; set; }
    public List<Move> Moves { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; }
    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
    public bool HpChanged = false;

    public void Init()
    {
        
        Moves = new List<Move>();

        //Create Moves Based On Level
        foreach (var move in BaseStats.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));
            if(Moves.Count >= 4)
            {
                break;
            }
        }
        CalculateStats();
        currHP = MaxHp;
        ResetStatBoost();

    }
    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt(((BaseStats.Attack * Level) / 100f) + 5));
        Stats.Add(Stat.Defense, Mathf.FloorToInt(((BaseStats.Defense * Level) / 100f) + 5));
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt(((BaseStats.SpAttack * Level) / 100f) + 5));
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt(((BaseStats.SpDefense * Level) / 100f) + 5));
        Stats.Add(Stat.Speed, Mathf.FloorToInt(((BaseStats.Speed * Level) / 100f) + 5));
        MaxHp = Mathf.FloorToInt(((BaseStats.MaxHp * Level) / 100f) + 10);
    }
    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];
        int boost = StatBoosts[stat];
        var boostValue = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt( statVal * boostValue[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValue[-boost]);

        return statVal;
    }
    public void ApplyBoost(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);
            Debug.Log($"{stat} has been boosted to {StatBoosts[stat]}");
            if(boost > 0)
            {
                StatusChanges.Enqueue($"{BaseStats.Name}'s {stat} rose!");
            }else if(boost <= 0)
            {
                StatusChanges.Enqueue($"{BaseStats.Name}'s {stat} fell!");
            }
        }
    }
    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0 },
            {Stat.Defense, 0 },
            {Stat.SpAttack, 0 },
            {Stat.SpDefense, 0 },
            {Stat.Speed, 0 }
        };
    }
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }
    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }
    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }
    public int MaxHp
    {
        get; private set;
    }
    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float criticalHit = 1f;
        float type = TypeChart.GetEffectiveness(move.moveBase.Type, this.BaseStats.Type1) * TypeChart.GetEffectiveness(move.moveBase.Type, this.BaseStats.Type2);
        if(Random.value * 100f <= 6.25)
        {
            criticalHit = 2f;
        }

        var damageDetails = new DamageDetails()
        {
            Type = type,
            Critical = criticalHit,
            Fainted = false
        };

        float attack = (move.moveBase.MoveCategory == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.moveBase.MoveCategory == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(.85f, 1f) * type * criticalHit;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.moveBase.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHp(damage);
        return damageDetails;
    }
    public void SetStatus(ConditionID conditionID)
    {
        Status = ConditionsDB.Conditions[conditionID];
        StatusChanges.Enqueue($"{ BaseStats.Name} { Status.StartMessage}");
    }
    public void UpdateHp(int damage)
    {
        HpChanged = true;
        currHP = Mathf.Clamp(currHP - damage, 0, MaxHp);
    }
    public Move GetRandomMove()
    {
        int random = Random.Range(0, Moves.Count);
        return Moves[random];
    }
    public void OnAfterTurn(){
        Status?.OnAfterTurn ?.Invoke(this);
    }
    public void OnBattleOver()
    {
        ResetStatBoost();
    }
    public class DamageDetails
    {
        public bool Fainted { get; set; }
        public float Critical { get; set; }
        public float Type { get; set; }
    }
}
