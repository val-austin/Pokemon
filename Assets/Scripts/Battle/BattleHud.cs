using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    Pokemon _pokemon;

    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
   

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = pokemon.BaseStats.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float) pokemon.currHP / pokemon.MaxHp);
    }

     
    public IEnumerator UpdateHP()
    {
        if (_pokemon.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)_pokemon.currHP / _pokemon.MaxHp);
            _pokemon.HpChanged = false;
        }
    }
}
