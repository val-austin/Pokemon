using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemonParty;
    private void Start()
    {
        foreach(var pokemon in pokemonParty)
        {
            pokemon.Init();
        }
    }
    public List<Pokemon> Pokemons
    {
        get {
            return pokemonParty;
        }
    }

    public Pokemon getHealthyPokemon()
    {
        return pokemonParty.Where(x => x.currHP > 0).FirstOrDefault();
    }
}
