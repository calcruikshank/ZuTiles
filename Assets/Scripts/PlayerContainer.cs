using Gameboard.Examples;
using Gameboard.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContainer : MonoBehaviour
{
    int numberOfCardsInHand = 0;
    float movableObjectPadding = 1f;
    public List<GameObject> cardsInHand = new List<GameObject>();
    PlayerPresenceDrawer inPlayer;
    string cardHandID;
    SetStencilReference
     setStencilReference;

    Vector3 currentOffset = Vector3.zero;
    Vector3 newOffset = Vector3.zero;
    Vector3 thisRotation;
    Vector3 fingerMovePosition;
    Vector3 fingerDownPosition;

    public Transform targetPlayCardTransform;
    public Transform deckSpawnLocation;

    Dictionary<CardDefinition, string> cardDefinitions = new Dictionary<CardDefinition, string>();
    private void Awake()
    {
        inPlayer = this.transform.root.GetComponentInChildren<PlayerPresenceDrawer>();
        setStencilReference = FindObjectOfType<SetStencilReference>();
        currentOffset = Vector3.zero;

    }
    public void AddCardToHand(GameObject cardToAdd)
    {
        thisRotation = this.transform.root.GetComponentInChildren<PlayerPresenceDrawer>().GetRotation();
        //position = (this.transform.bounds.x - this.transform.bounds.x + padding)
        //cardToAdd.transform.position = new Vector3(((this.transform.position.x + (this.transform.GetComponent<Collider>().bounds.size.x / 2) - (this.transform.GetComponent<Collider>().bounds.size.x / 2)) + (cardToAdd.transform.GetComponentInChildren<Collider>().bounds.size.x * cardsInHand.Count) + movableObjectPadding * cardsInHand.Count), cardToAdd.transform.position.y, this.transform.position.z);
        cardToAdd.transform.rotation = this.transform.rotation;
        cardsInHand.Add(cardToAdd);
        UpdateCardPositions();
        cardToAdd.GetComponent<MovableObjectStateMachine>().GivePlayerOwnership(this);
        cardToAdd.GetComponent<MovableObjectStateMachine>().state = MovableObjectStateMachine.State.Idle;
        //cardToAdd.SetActive(false);
        //cardToAdd.GetComponent<MovableObjectStateMachine>().HideObject();
        cardToAdd.GetComponent<MovableObjectStateMachine>().UnsubscribeToDelegates();
        Debug.Log("Currently face up " + cardToAdd.GetComponent<MovableObjectStateMachine>().GetCurrentFacing());
        if (cardToAdd.GetComponent<MovableObjectStateMachine>().GetCurrentFacing())
        {
            cardToAdd.GetComponent<MovableObjectStateMachine>().FlipObject();
        }
        AddToCompanion(cardToAdd);
    }

    void SetStencil(GameObject cardToAdd)
    {
        Transform[] objectsInCardToAdd = cardToAdd.GetComponentsInChildren<Transform>();
        foreach (Transform objectInCard in objectsInCardToAdd)
        {
            setStencilReference.objectsToHide.Add(objectInCard.gameObject);
            objectInCard.GetComponentInChildren<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        setStencilReference.Hide();
    }

    private void AddToCompanion(GameObject cardToAdd)
    {
        Texture2D cardImageTexture = (Texture2D)cardToAdd.GetComponentInChildren<Renderer>().material.mainTexture;
        byte[] textureArray = DeCompress(cardImageTexture).EncodeToPNG();
        CardDefinition newCardDef = new CardDefinition(cardImageTexture.name, textureArray, "", null, cardImageTexture.width / 2, cardImageTexture.height / 2);
        //cardDefinitions.Add(newCardDef, CardsTool.singleton.GetCurrentLocationOfCardByGUID(newCardDef.cardGuid));
        cardToAdd.GetComponent<Deck>().CardCompanionDefiniiton = newCardDef;
        CardsTool.singleton.AddCardToLibrary(newCardDef);
        AddCardToHandDisplay(newCardDef); 
    }

    private void AddCardToHandDisplay(CardDefinition newCardDef)
    {
        cardHandID = CardsTool.singleton.GetCardHandDisplayedForPlayer(inPlayer.userId);
        CardsTool.singleton.PlaceCardInPlayerHand(inPlayer.userId, cardHandID, newCardDef);
    }
    public void RemoveCardFromHand(GameObject cardToRemove)
    {
        cardsInHand.Remove(cardToRemove);
        cardToRemove.GetComponent<MovableObjectStateMachine>().RemovePlayerOwnership(this);

        cardToRemove.GetComponentInChildren<Deck>().SetToStartingShader();
        Transform[] objectsInCardToAdd = cardToRemove.GetComponentsInChildren<Transform>();
        foreach (Transform objectInCard in objectsInCardToAdd)
        {
            setStencilReference.objectsToHide.Remove(objectInCard.gameObject);
            Debug.Log(objectInCard + " Object in card");
            objectInCard.GetComponentInChildren<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        UpdateCardPositions();
    }


    void UpdateCardPositions()
    {
        /*if (thisRotation.y == 0)
        {
            for (int i = 0; i < cardsInHand.Count; i++)
            {
                cardsInHand[i].transform.position = new Vector3(((this.transform.position.x + (cardsInHand[i].transform.GetComponentInChildren<Collider>().bounds.size.x * i) + movableObjectPadding * i)) + currentOffset.x, this.transform.position.y + .1f, this.transform.position.z);
            }
        }
        else
        {
            for (int i = 0; i < cardsInHand.Count; i++)
            {
                cardsInHand[i].transform.position = this.transform.position;
            }
        }*/
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].transform.position = this.transform.position;
        }
    }

    internal void FindCardToRemove(CardDefinition selectedCard)
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            Debug.Log("Card companion definition = " + cardsInHand[i].GetComponent<Deck>().CardCompanionDefiniiton);
            if (selectedCard == cardsInHand[i].GetComponent<Deck>().CardCompanionDefiniiton)
            {
               // cardsInHand[i].SetActive(true);
                cardsInHand[i].GetComponent<MoveTowardsWithLerp>().objects.Add(targetPlayCardTransform);
                cardsInHand[i].GetComponent<MoveTowardsWithLerp>().ChangeStateToLerp();
                RemoveCardFromHand(cardsInHand[i]);
            }
        }
    }

    public Texture2D DeCompress(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
    internal void SelectContainer(int index, Vector3 startingPosition)
    {
        Debug.Log("Starting Position " + startingPosition);
        fingerDownPosition = startingPosition - currentOffset;
        SubscribeToDelegates();
    }

    public void SubscribeToDelegates()
    {
        TouchScript.touchMoved += Scroll;
        TouchScript.fingerReleased += FingerReleased;
        thisRotation = this.transform.root.GetComponentInChildren<PlayerPresenceDrawer>().GetRotation();

    }
    public void UnsubToDelegates()
    {
        TouchScript.fingerReleased -= FingerReleased;
        TouchScript.touchMoved -= Scroll;
    }

    private void Scroll(Vector3 position, int index)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            fingerMovePosition = raycastHit.point;
        }
        newOffset = fingerMovePosition - fingerDownPosition;
        currentOffset = newOffset;
        UpdateCardPositions();
    }
    private void FingerReleased(Vector3 position, int index)
    {
        UnsubToDelegates();
    }

}
