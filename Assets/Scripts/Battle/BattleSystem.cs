using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    
    [SerializeField] BattleUnit enemyUnit;
    
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetUpBattle());
    }

    public IEnumerator SetUpBattle()
    {
        playerUnit.Setup(playerParty.getHealthyPokemon());
        
        enemyUnit.Setup(wildPokemon);
        
        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return (dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.BaseStats.Name} appeared!"));
        
        ChooseFirstTurn();

    }
    void ChooseFirstTurn()
    {
        if(playerUnit.Pokemon.Speed > enemyUnit.Pokemon.Speed)
        {
            ActionSelection();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }
    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action.");
        dialogBox.EnableActionSelector(true);
    }
    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
        
    }
    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }
    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }
    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;
        var move = playerUnit.Pokemon.Moves[currentMove];

        yield return RunMove(playerUnit, enemyUnit, move);

        //If battle is not over, have enemy attack
        if (state == BattleState.PerformMove)
        {
            StartCoroutine(EnemyMove());
        }
    }
    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;
        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if(state == BattleState.PerformMove)
        {
            ActionSelection();
        }
        
    }
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.BaseStats.Name} used {move.moveBase.Name}");

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        targetUnit.PlayHitAnimation();

        if (move.moveBase.MoveCategory == MoveCategory.Status)
        {
            yield return RunMoveEffects(move, sourceUnit.Pokemon, targetUnit.Pokemon);
        }
        else
        {

            var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }
        
        if (targetUnit.Pokemon.currHP <= 0)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.BaseStats.Name} fainted");
            targetUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            CheckForBattleOver(targetUnit);
        }

        //Status psn and brn checking for faint and applying damage.
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();

        if (sourceUnit.Pokemon.currHP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.BaseStats.Name} fainted");
            sourceUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            CheckForBattleOver(sourceUnit);
        }
    }
    IEnumerator RunMoveEffects(Move move, Pokemon source, Pokemon target)
    {
        var effects = move.moveBase.MoveEffects;
        //Stat Boost
        if(effects.Boosts != null)
        {
            if(move.moveBase.MoveTarget == MoveTarget.Face)
            {
                source.ApplyBoost(effects.Boosts);
            }
            else
            {
                target.ApplyBoost(effects.Boosts);
            }
            
        }
        //Status Condition
        if(effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }
    IEnumerator ShowDamageDetails(Pokemon.DamageDetails damageDetails)
    {
        if(damageDetails.Critical > 1)
        {
            yield return dialogBox.TypeDialog("A critical hit!");
        }
        if(damageDetails.Type > 1)
        {
            yield return dialogBox.TypeDialog("It's super effective!");
        }else if(damageDetails.Type < 1)
        {
            yield return dialogBox.TypeDialog("It's not very effective!");
        }
    }
    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while(pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }
    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.getHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            BattleOver(true);
        }
    }
    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if(state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if(state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentAction++;
        }else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentAction--;
        }else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);
        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.F))
        {
            if(currentAction == 0)
            {
                //Fight
                MoveSelection();
            }else if(currentAction == 1)
            {
                //Bag
            }else if(currentAction == 2)
            {
                //Pokemon
                OpenPartyScreen();
            }else if(currentAction == 3)
            {
                //Run
            }
        }
    }
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentMove++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentMove--;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count -1);
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.F))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }

    }
    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentMember++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentMember--;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);
        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.F))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if(selectedMember.currHP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted Pokemon!");
                return;
            }
            if(selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("This Pokemon is already out there!");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        bool currentPokemonFainted = true;
        if (playerUnit.Pokemon.currHP > 0)
        {
            currentPokemonFainted = false;
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.BaseStats.Name}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        yield return (dialogBox.TypeDialog($"Go {newPokemon.BaseStats.Name}!"));

        if (currentPokemonFainted)
        {
            ChooseFirstTurn();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }
}