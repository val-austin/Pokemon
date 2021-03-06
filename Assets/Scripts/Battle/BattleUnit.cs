using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;
    public bool IsPlayerUnit
    {
        get { return isPlayerUnit; }
    }
    public BattleHud Hud
    {
        get { return hud; }
    }

    public Pokemon Pokemon { get; set; }
    Image image;
    Color originalColor;
    Vector3 originalPos;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }
    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if (isPlayerUnit)
        {
            image.sprite = Pokemon.BaseStats.BackSprite;
        }
        else
        {
            image.sprite = Pokemon.BaseStats.FrontSprite;
        }
        hud.SetData(pokemon);
        image.color = originalColor;
        PlayEnterAnimation();
    }
    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
        {
            image.transform.localPosition = new Vector3(-500f, originalPos.y);
        }else
            image.transform.localPosition = new Vector3(500f, originalPos.y);
        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }
    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, .25f));
        }else
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, .25f));
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, .25f));
    }
    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, .1f));
        sequence.Append(image.DOColor(originalColor, .1f));
    }
    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, .5f));
        sequence.Join(image.DOFade(0f, .5f));
    }
}
