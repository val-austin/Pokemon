using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    //Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMoves> learnableMoves;

    //Getters
    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }
    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }
    public Sprite BackSprite
    {
        get { return backSprite; }
    }
    public PokemonType Type1
    {
        get { return type1; }
    }
    public PokemonType Type2
    {
        get { return type2; }
    }
    public int MaxHp
    {
        get { return maxHp; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int Defense
    {
        get { return defense; }
    }
    public int SpAttack
    {
        get { return spAttack; }
    }
    public int SpDefense
    {
        get { return spDefense; }
    }
    public int Speed
    {
        get { return speed; }
    }
    public List<LearnableMoves> LearnableMoves
    {
        get { return learnableMoves; }
    }

}

[System.Serializable]
public class LearnableMoves
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }
    public int Level
    {
        get { return level; }
    }
}
public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock, 
    Ghost,
    DragonDeezNuts
}
public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed
}
public class TypeChart
{
    static float[][] chart =
    {   //                         NOR  FIR  WAT  GRA  ELE  ICE  FIG  POI  GRO  FLY  PSY  BUG  ROC  GHO  DRA  
        /*Normal*/    new float[] {1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f, .5f,  0f,  1f},
        /*Fire*/      new float[] {1f, .5f, .5f,  1f,  2f,  2f,  1f,  1f,  1f,  1f,  1f,  2f, .5f,  1f, .5f},
        /*Water*/     new float[] {1f,  2f, .5f,  1f, .5f,  1f,  1f,  1f,  2f,  1f,  1f,  1f,  2f,  1f, .5f},
        /*Electric*/  new float[] {1f,  1f,  2f, .5f, .5f,  1f,  1f,  1f,  0f,  2f,  1f,  1f,  1f,  1f, .5f},
        /*Grass*/     new float[] {1f, .5f,  2f,  1f, .5f,  1f,  1f, .5f,  2f, .5f,  1f, .5f,  2f,  1f, .5f},
        /*Ice*/       new float[] {1f, .5f, .5f,  1f,  2f,  1f,  1f,  1f,  2f,  1f,  1f,  1f,  1f,  1f,  2f},
        /*Fighting*/  new float[] {2f,  1f,  1f,  1f,  1f,  2f,  1f, .5f,  1f, .5f, .5f, .5f,  2f,  0f,  1f},
        /*Poison*/    new float[] {1f,  1f,  1f,  1f,  2f,  1f,  1f, .5f, .5f,  1f,  1f,  1f, .5f, .5f,  1f},
        /*Ground*/    new float[] {1f,  2f,  1f,  2f, .5f,  1f,  1f,  2f,  1f,  0f,  1f, .5f,  2f,  1f,  1f},
        /*Flying*/    new float[] {1f,  1f,  1f, .5f,  2f,  1f,  2f,  1f,  1f,  1f,  1f,  2f, .5f,  1f,  1f},
        /*Pyschic*/   new float[] {1f,  1f,  1f,  1f,  1f,  1f,  2f,  2f,  1f,  1f, .5f, .5f,  1f,  1f,  1f},
        /*Bug*/       new float[] {1f, .5f,  1f,  1f, .5f,  1f, .5f, .5f,  1f, .5f,  2f,  1f,  1f, .5f,  1f},
        /*Rock*/      new float[] {1f,  2f,  1f,  1f,  1f,  2f, .5f,  1f, .5f,  2f,  1f,  2f,  1f,  1f,  1f},
        /*Ghost*/     new float[] {0f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f,  1f,  2f,  1f},
        /*Dragon*/    new float[] {1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  2f},

    };
    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if(attackType == PokemonType.None || defenseType == PokemonType.None)
        {
            return 1;
        }
        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;
        return chart[row][col];
    }
}
