using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    PartyMemberUI[] partyMembers;
    List<Pokemon> pokemons;

    public void Init()
    {
        partyMembers = GetComponentsInChildren<PartyMemberUI>();
    }
    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;
        for(int i = 0; i<partyMembers.Length; i++)
        {
            if (i < pokemons.Count)
            {
                partyMembers[i].SetData(pokemons[i]);

            }
            else
            {
                partyMembers[i].gameObject.SetActive(false);
            }
            messageText.text = "Choose a Pokemon";

        }
    }
    public void UpdateMemberSelection(int selectedMember)
    {
        for(int i = 0; i< pokemons.Count; i++)
        {
            if (i == selectedMember)
            {
                partyMembers[i].SetSelected(true);
            }
            else
                partyMembers[i].SetSelected(false);
        }
    }
    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
