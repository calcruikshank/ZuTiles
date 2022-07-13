using Gameboard;
using Gameboard.Examples;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZuTilesSetup : MonoBehaviour
{
    [SerializeField] GameObject[] decksToChooseFrom;

    public static ZuTilesSetup singleton;

    [SerializeField] int numOfCardsToStartWith = 7;

    // Start is called before the first frame update
    void Start()
    {
        if (singleton != null)
        {
            Destroy(this);
        }
        singleton = this;

        //display decks to choose from to the players
        //shuffle and load the decks for each player
    }

    // Update is called once per frame
    void Update()
    {

    }

    void AddZuTileToEachPlayer(PlayerPresenceDrawer ppd)
    {
        for (int i = 0; i < UserPresenceTest.singleton.playerList.Count; i++)
        {
            ZuTilePlayer ztp = UserPresenceTest.singleton.playerList[i].gameObject.AddComponent<ZuTilePlayer>();
        }
    }
    public void AddZuTilePlayer(PlayerPresenceDrawer ppd)
    {
        ZuTilePlayer ztp = ppd.gameObject.AddComponent<ZuTilePlayer>();

        //ztp.ShowPlayerDeckToChooseFrom(decksToChooseFrom);
    }

    public void CheckToSeeIfShouldStartGame()
    {
        bool decksAreNotNull = true;
        foreach (PlayerPresenceDrawer player in UserPresenceTest.singleton.playerList)
        {
            GameObject deckToSpawn = player.GetComponentInChildren<ZuTilePlayer>().ChosenDeck;
            if (deckToSpawn == null)
            {
                decksAreNotNull = false;
                return;
            }
            
        }
        if (decksAreNotNull)
        {
            SetupGame();
        }
    }

    private void SetupGame()
    {
        foreach (PlayerPresenceDrawer player in UserPresenceTest.singleton.playerList)
        {
            
            GameObject deckToSpawn = player.GetComponentInChildren<ZuTilePlayer>().ChosenDeck;
            GameObject instantiatedDeck = Instantiate(deckToSpawn, player.GetComponentInChildren<PlayerContainer>().deckSpawnLocation.position, player.GetComponentInChildren<PlayerContainer>().targetPlayCardTransform.rotation);
            instantiatedDeck.GetComponent<Deck>().ShuffleDeck(Vector3.zero, 0);
            //instantiatedDeck.GetComponent<MovableObjectStateMachine>().HideObject();
            if (instantiatedDeck.GetComponent<MovableObjectStateMachine>().GetCurrentFacing())
            {
                
                //instantiatedDeck.GetComponent<MovableObjectStateMachine>().FlipObject();
            }

            List<GameObject> cardsToAdd = (instantiatedDeck.GetComponent<Deck>().MakeANewListOfInstantiatedCards(numOfCardsToStartWith));
            for (int i = 0; i < cardsToAdd.Count; i++)
            {
                player.gameObject.GetComponentInChildren<PlayerContainer>().AddCardToHand(cardsToAdd[i]);
            }
            deckToSpawn.gameObject.SetActive(false);
            ZuTiileButtonSelector[] deckSelectors = FindObjectsOfType<ZuTiileButtonSelector>();
            for (int i = 0; i < deckSelectors.Length; i++)
            {
                deckSelectors[i].gameObject.SetActive(false);
            }
            if (instantiatedDeck.GetComponent<MovableObjectStateMachine>().GetCurrentFacing())
            {
                //instantiatedDeck.GetComponent<MovableObjectStateMachine>().FlipObject();
            }
        }
    }

    void DrawACardForPlayer(PlayerPresenceDrawer playerSent)
    {

    }
}
