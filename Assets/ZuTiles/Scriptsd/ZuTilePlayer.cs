using Gameboard;
using Gameboard.Examples;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZuTilePlayer : MonoBehaviour
{

    public GameObject ChosenDeck;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void ShowPlayerDeckToChooseFrom(GameObject[] decksToChooseFrom)
    {
        for (int i = 0; i < decksToChooseFrom.Length; i++)
        {
            //GameObject buttonSelectionDeck = Instantiate(decksToChooseFrom[i], transform.position, transform.rotation);
            Debug.Log("Showing Player Deck");
        }
    }

    internal void SetDeckToDeckToInstantiate(GameObject deckToInstantiate, GameObject deckSelected)
    {
        Debug.Log("Instantiating deck  " + deckToInstantiate);
        ChosenDeck = deckToInstantiate;
        
        //SetupZuTilePlayer(deckToInstantiate);
        deckSelected.transform.position = new Vector3(this.transform.GetComponentInChildren<PlayerContainer>().transform.position.x, this.transform.GetComponentInChildren<PlayerContainer>().transform.position.y + 1f, this.transform.GetComponentInChildren<PlayerContainer>().transform.position.z);
        deckSelected.transform.rotation = this.transform.rotation;

        UserPresenceTest.singleton.AddButtonsToPlayer(this.transform.GetComponentInChildren<PlayerPresenceDrawer>(), ChosenDeck);
        ZuTilesSetup.singleton.CheckToSeeIfShouldStartGame();
        //lock the choice in here but instantiate the deck on game start instead
    }

    public void RemoveDeckSelection()
    {
        ChosenDeck = null;
    }

    private void SetupZuTilePlayer(GameObject deckToInstantiate)
    {
        
    }
}
