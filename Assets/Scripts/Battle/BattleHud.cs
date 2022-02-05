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
    [SerializeField] Text statusText;

    [SerializeField] Color psnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color fznColor;
    [SerializeField] Color parColor;

    Dictionary<ConditionID, Color> statusColors;
    public void SetData(Pokemon pokemon)
    {

        _pokemon = pokemon;
        nameText.text = pokemon.BaseStats.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float) pokemon.currHP / pokemon.MaxHp);
        statusColors = new Dictionary<ConditionID, Color>() {
            {ConditionID.psn, psnColor},
            {ConditionID.brn, brnColor},
            {ConditionID.frz, fznColor},
            {ConditionID.par, parColor},
            {ConditionID.slp, slpColor},
        };
        SetStatusText();
        _pokemon.OnStatusChanged += SetStatusText;
    }
    public void SetStatusText()
    {
        if(_pokemon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _pokemon.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_pokemon.Status.Id];

        }
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
