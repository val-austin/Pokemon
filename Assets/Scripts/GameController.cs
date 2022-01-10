using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum GameState { FreeRoam, Battle}
public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    GameState gameState;

    private void Start()
    {
        playerController.onEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }
    void StartBattle()
    {
        gameState = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();
        battleSystem.StartBattle(playerParty, wildPokemon);
        worldCamera.gameObject.SetActive(false);
    }
    void EndBattle(bool won)
    {
        gameState = GameState.FreeRoam;
        worldCamera.gameObject.SetActive(true);
        battleSystem.gameObject.SetActive(false);
    }
    public void Update()
    {
        if(gameState == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
            
        }
        else if (gameState == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
    }

}
