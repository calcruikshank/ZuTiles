using Gameboard.Tools;
using Shared.UI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Deck : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> cardsInDeck = new List<GameObject>();
    MovableObjectStateMachine movableObject;
    GameObject currentCardShowing;
    public CardDefinition CardCompanionDefiniiton;
    Shader startingShader;
    void Start()
    {
        InitializeDeck();
        UpdateDeckInfo();
    }

    public void SetSelected(int id, Vector3 offset)
    {
        TouchScript.shuffleInitiated += ShuffleDeck;
    }
    public void SetUnselected(int id, Vector3 offset)
    {
        TouchScript.shuffleInitiated -= ShuffleDeck;
    }
    public void InitializeDeck()
    {
        currentCardShowing = this.transform.GetComponentInChildren<CardFront>().gameObject;
        movableObject = this.transform.GetComponent<MovableObjectStateMachine>();
        startingShader = this.GetComponentInChildren<Renderer>().material.shader;
    }

    public void UpdateDeckInfo()
    {
        SetSize(new Vector3(this.transform.localScale.x, cardsInDeck.Count, this.transform.localScale.z));
        SetTopCard(cardsInDeck[0]);
        if (this.GetComponent<MovableObjectStateMachine>().GetCurrentFacing())
        {
            SetCurrentCardShowing(cardsInDeck[cardsInDeck.Count - 1]);
        }
    }

    public void AddToDeck(List<GameObject> cardsSents)
    {
        foreach (GameObject card in cardsSents)
        {
            cardsInDeck.Add(card);
        }
        UpdateDeckInfo();
    }
    public void AddToFrontOfList(List<GameObject> cardsSents)
    {
        for (int i = 0; i < cardsSents.Count; i++)
        {
            cardsInDeck.Add(cardsInDeck[i]);
            cardsInDeck[i] = cardsSents[i];
        }
        UpdateDeckInfo();
    }

    public void CheckToSeeIfDeckShouldBeAdded()
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, Vector3.down, 100.0F);

        //this for loop is to check for any player containers hit
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.GetComponentInChildren<PlayerContainer>() != null)
            {
                PlayerContainer playerToAddCardTo = hits[i].transform.GetComponentInChildren<PlayerContainer>();
                playerToAddCardTo.AddCardToHand(this.gameObject);
                return;
            }
        }



        //this for loop is to check for any decks hit to add to
        for (int j = 0; j < hits.Length; j++)
        {
            Transform targetHit = hits[j].transform;

            while (targetHit.parent != null)
            {
                targetHit = targetHit.transform.parent;
            }
            if (targetHit.GetComponentInChildren<Deck>() != null)
            {
                if (targetHit.GetComponent<Deck>() != this)
                {
                    if (targetHit.GetComponent<MovableObjectStateMachine>().GetCurrentFacing())
                    {
                        Debug.Log("Hit detected " + targetHit.name);
                        Deck deckToAddTo = targetHit.GetComponent<Deck>();
                        deckToAddTo.AddToDeck(this.cardsInDeck);
                        Destroy(this.gameObject);
                    }
                    if (!targetHit.GetComponent<MovableObjectStateMachine>().GetCurrentFacing())
                    {
                        Debug.Log("Adding to front of list " + targetHit.name);
                        Deck deckToAddTo = targetHit.GetComponent<Deck>();
                        deckToAddTo.AddToFrontOfList(this.cardsInDeck);
                        Destroy(this.gameObject);
                    }
                }
                targetHit = null;
                return;
            }
        }
        for (int i = 0; i < hits.Length; i++)
        {
            var placementObj = Crutilities.singleton.GetFinalParent(hits[i].transform).GetComponentInChildren<PlacementObject>();
            if (placementObj != null && placementObj.ListContainsString(this.name))
            {
                Debug.Log("The name of this component is " + hits[i].transform.name);
                this.transform.position = new Vector3(hits[i].transform.position.x, placementObj.transform.position.y + .1f, hits[i].transform.position.z);
                return;
            }
        }

        #region commented
        /*Collider colliderHit = transform.GetComponentInChildren<Collider>();
        
        RaycastHit playerContainerHit;
        bool playerContainerHitBool = Physics.BoxCast(colliderHit.bounds.center, new Vector3(colliderHit.bounds.extents.x, colliderHit.bounds.extents.y, colliderHit.bounds.extents.z), Vector3.down, out playerContainerHit, Quaternion.identity, playerContainerLayerMask);
        RaycastHit hit;
        bool hitDetected = Physics.BoxCast(colliderHit.bounds.center, new Vector3(colliderHit.bounds.extents.x, colliderHit.bounds.extents.y, colliderHit.bounds.extents.z), Vector3.down, out hit, Quaternion.identity, Mathf.Infinity);
        
        
        if (playerContainerHitBool)
        {
            Transform targetToMove = hit.transform;
            if (targetToMove.GetComponentInChildren<PlayerContainer>() != null)
            {
                PlayerContainer playerToAddCardTo = targetToMove.GetComponentInChildren<PlayerContainer>();
                playerToAddCardTo.AddCardToHand(this.gameObject);
                return;
            }
        }
        if (hitDetected)
        {
            Transform targetToMove = hit.transform;
            while (targetToMove.parent != null)
            {
                targetToMove = targetToMove.transform.parent;
            }

            if (targetToMove.GetComponent<Deck>() != null)
            {
                if (targetToMove.GetComponent<Deck>() != this)
                {
                    if (targetToMove.GetComponent<MovableObjectStateMachine>().faceUp)
                    {
                        Debug.Log("Hit detected " + targetToMove.name);
                        Deck deckToAddTo = targetToMove.GetComponent<Deck>();
                        deckToAddTo.AddToDeck(this.cardsInDeck);
                        Destroy(this.gameObject);
                    }
                    if (!targetToMove.GetComponent<MovableObjectStateMachine>().faceUp)
                    {
                        Debug.Log("Adding to front of list " + targetToMove.name);
                        Deck deckToAddTo = targetToMove.GetComponent<Deck>();
                        deckToAddTo.AddToFrontOfList(this.cardsInDeck);
                        Destroy(this.gameObject);
                    }
                }
            }
            targetToMove = null;
        }*/
        #endregion
    }



    internal void SetToStartingShader()
    {

        this.GetComponentInChildren<Renderer>().material.shader = startingShader;
    }

    public void SetSize(Vector3 localSizeSent)
    {
        this.transform.localScale = localSizeSent;
    }

    public void SetTopCard(GameObject cardSent)
    {
    }

    public void SetCurrentCardShowing(GameObject cardSent)
    {
        
        GetComponentInChildren<CardFront>().ChangeCardFront(cardSent.GetComponentInChildren<CardFront>().gameObject);
        currentCardShowing = GetComponentInChildren<CardFront>().gameObject;
        if (currentCardShowing.GetComponent<CardTilter>() != null)
        {
            currentCardShowing.GetComponent<CardTilter>().enabled = false;
        }
    }
    public void ShuffleDeck(Vector3 offset, int id)
    {
        Shuffle(cardsInDeck);
        UpdateDeckInfo();
    }

    public void Shuffle<GameObject>(List<GameObject> listToShuffle)
    {
        for (int i = 0; i < listToShuffle.Count - 1; i++)
        {
            GameObject temp = listToShuffle[i];
            int rand = UnityEngine.Random.Range(i, listToShuffle.Count);
            listToShuffle[i] = listToShuffle[rand];
            listToShuffle[rand] = temp;
        }
    }

    internal List<GameObject> MakeANewListOfInstantiatedCards(int numOfCardsToStartWith)
    {
        List<GameObject> newCardsInstantiated = new List<GameObject>();
        for (int i = 0; i < numOfCardsToStartWith; i++)
        {
            GameObject newCard;
            if (cardsInDeck.Count == 1)
            {
                return null;
            }
            newCard = InstantiateCardsFromBottom();
            newCard.GetComponent<Deck>().UpdateDeckInfo();
            /*if (this.GetComponent<MovableObjectStateMachine>().faceUp)
            {
                newCard = InstantiateCardsFromBottom();
                newCardsInstantiated.Add(newCard);
            }
            if (!this.GetComponent<MovableObjectStateMachine>().faceUp)
            {
                newCard = InstantiateCardsFromTop();
                newCardsInstantiated.Add(newCard);
            }*/
            newCardsInstantiated.Add(newCard);
        }
        return newCardsInstantiated;
    }
    GameObject InstantiateCardsFromBottom()
    {
        GameObject newDeck;
        newDeck = Instantiate(this.gameObject, Vector3.zero, Quaternion.identity);
        Deck deck = newDeck.GetComponent<Deck>();
        UpdateDeckInfo();
        deck.cardsInDeck.Clear();
        deck.cardsInDeck.Add(cardsInDeck[cardsInDeck.Count - 1]);
        cardsInDeck.RemoveAt(cardsInDeck.Count - 1);
        
        Debug.Log(" cards in new deck at 0 so update game god " + deck.cardsInDeck[0]  + " update game info " + newDeck.GetComponentInChildren<CardFront>());
        deck.UpdateDeckInfo();
        UpdateDeckInfo(); 
        
        return newDeck;
    }

    public void PickUpCards(int numOfCardsToPickUp)
    {
        if (cardsInDeck.Count == 1) //TODO change thisd to say if cardsindeck.count is greater than number of cards to pick up
        {
            return;
        }
        if (movableObject.GetCurrentFacing())
        {
            PickUpCardsFromBottom(numOfCardsToPickUp);
        }
        if (!movableObject.GetCurrentFacing())
        {
            PickUpCardsFromTop(numOfCardsToPickUp);
        }
    }


    public void PickUpCardsFromBottom(int numOfCardsToPickUp)
    {
        GameObject newDeck;
        newDeck = Instantiate(this.gameObject, transform.position, Quaternion.identity);
        int iniatedI = cardsInDeck.Count;
        cardsInDeck.Clear();
        for (int i = iniatedI - numOfCardsToPickUp; i <= iniatedI - 1; i++)
        {
            GameObject cardToAddThenRemove = newDeck.GetComponent<Deck>().cardsInDeck[i];
            cardsInDeck.Add(cardToAddThenRemove);
            newDeck.GetComponent<Deck>().cardsInDeck.RemoveAt(i);
        }

        UpdateDeckInfo();

    }

    public void PickUpCardsFromTop(int numOfCardsToPickUp)
    {
        GameObject newDeck;
        newDeck = Instantiate(this.gameObject, transform.position, Quaternion.identity);
        
        int iniatedI = cardsInDeck.Count;
        cardsInDeck.Clear();
        for (int i = 0; i <= numOfCardsToPickUp - 1; i++)
        {
            Debug.Log(i);
            GameObject cardToAddThenRemove = newDeck.GetComponent<Deck>().cardsInDeck[i];
            cardsInDeck.Add(cardToAddThenRemove);
            newDeck.GetComponent<Deck>().cardsInDeck.RemoveAt(i);
        }

        UpdateDeckInfo();
    }
}
