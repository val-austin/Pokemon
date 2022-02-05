using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver}
public enum BattleAction { Move, SwitchPokemon, UseItem, Run}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    
    [SerializeField] BattleUnit enemyUnit;
    
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
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

        ActionSelection();

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
    
    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;
        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.moveBase.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.moveBase.Priority;
            bool playerGoesFirst = true;
            if(enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if(enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            } 

            //check who goes first

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            //First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondPokemon.currHP > 0)
            {
                //Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if(playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Pokemons[currentMember];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            //Enemy's turn
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;

        }

        if(state != BattleState.BattleOver)
        {
            ActionSelection();
        }

    }
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.BaseStats.Name} used {move.moveBase.Name}");
        if (CheckIfMoveHit(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            targetUnit.PlayHitAnimation();

            if (move.moveBase.MoveCategory == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.moveBase.MoveEffects, sourceUnit.Pokemon, targetUnit.Pokemon, move.moveBase.MoveTarget);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if(move.moveBase.SecondaryEffects != null && move.moveBase.SecondaryEffects.Count > 0 && targetUnit.Pokemon.currHP > 0)
            {
                foreach(var secondary in move.moveBase.SecondaryEffects)
                {
                    var random = UnityEngine.Random.Range(1, 101);
                    if(random <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                    }
                }
            }

            if (targetUnit.Pokemon.currHP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.BaseStats.Name} fainted");
                targetUnit.PlayFaintAnimation();

                yield return new WaitForSeconds(2f);
                CheckForBattleOver(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.BaseStats.Name}'s attack missed");

        }
    }
    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
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
    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        
        //Stat Boost
        if(effects.Boosts != null)
        {
            if(moveTarget == MoveTarget.Face)
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
        //Volatile Status Condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
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
    bool CheckIfMoveHit(Move move, Pokemon source, Pokemon target)
    {
        if (move.moveBase.AlwaysHit)
            return true;

        float moveAccuarcy = move.moveBase.Accuracy;
        int accuracy = source.StatBoosts[Stat.Accuracy];
        Debug.Log($"{accuracy} is {source.BaseStats.Name} accuracy stat");
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValue = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };
        if (accuracy > 0)
        {
            moveAccuarcy *= boostValue[accuracy];
            //Debug.Log($"{moveAccuarcy} is {source.BaseStats.Name} move accuracy stat");
            //Debug.Log($"{boostValue[accuracy]} is boost value");

        }
        else
        {
            moveAccuarcy /= boostValue[-accuracy];
            //Debug.Log($"{moveAccuarcy} is {source.BaseStats.Name} move accuracy stat");
            //Debug.Log($"{boostValue[-accuracy]} is boost value");
        }

        if (evasion > 0)
            moveAccuarcy /= boostValue[evasion];
        else
            moveAccuarcy *= boostValue[-evasion];
        //Debug.Log($"{move.moveBase.Name} accuracy is at {moveAccuarcy}");
         
        if (UnityEngine.Random.Range(1,101) <= moveAccuarcy)
        {
            return true;
        }
        return false;
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
                prevState = state;
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
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0) return;
            
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
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

            if (prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.Pokemon.currHP > 0)
        {
            
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.BaseStats.Name}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        yield return (dialogBox.TypeDialog($"Go {newPokemon.BaseStats.Name}!"));

        state = BattleState.RunningTurn;
    }
}