using Shared.UI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckVisualScript : MonoBehaviour
{
    GameObject currentCardShowing;
    [SerializeField] Transform topOfDeck;
    [SerializeField] Transform bottomOfDeck;
    [SerializeField] Transform middleOfDeck;
    [SerializeField] float deckThickness;
    private void Awake()
    {

        currentCardShowing = this.transform.GetComponentInChildren<CardFront>().gameObject;
        SetSize(new Vector3(this.transform.localScale.x, 1, this.transform.localScale.z));
    }
    internal void UpdateDeckVisuals(List<GameObject> cardsInDeck)
    {
        if (this.GetComponent<MovableObjectStateMachine>().GetCurrentFacing())
        {
            topOfDeck = cardsInDeck[cardsInDeck.Count - 1].transform;
            SetCurrentCardShowing(cardsInDeck[cardsInDeck.Count - 1]);
        }
        else
        {
            topOfDeck = cardsInDeck[0].transform;
            SetCurrentCardShowing(cardsInDeck[0]);
        }
        SetSize(new Vector3(this.transform.localScale.x, cardsInDeck.Count, this.transform.localScale.z));
    }
    public void SetSize(Vector3 localSizeSent)
    {
        this.middleOfDeck.localScale = new Vector3(this.transform.localScale.x, localSizeSent.y * deckThickness, this.transform.localScale.z); 
        this.topOfDeck.position = new Vector3(this.transform.position.x, this.transform.position.y + localSizeSent.y * deckThickness, this.transform.position.z);
        Debug.Log(this.transform.position.y + localSizeSent.y * deckThickness);
        //this.transform.localScale = localSizeSent;
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
}
