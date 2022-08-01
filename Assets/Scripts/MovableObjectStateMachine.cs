using Shared.UI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovableObjectStateMachine : MonoBehaviour
{
    public State state;
    public List<int> idList = new List<int>();
    Vector3 startingTouchPosition;
    Vector3 fingerMovePosition;
    Vector3 offset;
    float heldDownTimer = 0f;
    float heldDownThreshold = .25f;
    float doubleTapTimer = 0f;
    float doubleTapThreshold = .25f;
    float distanceThreshold = .25f;
    Card deck;
    public bool faceUp;
    bool lowering;
    bool snappingToThreeOnY;
    int numOfFingersOnCard = 0;
    Vector3 rayPosition;

    Vector3 startingTouchPositionFinger2;
    Vector3 fingerMovePosition2;
    public PlayerContainer playerOwningCard;
    Vector3 targetRotation;
    float startingXRotation;

    Vector3 startingSize, startingTargetLerp;
    Vector3 currentLocalEulerAngles;

    bool showSelectedWheel = false;
    GameObject selectedWheelGO;

    Crutilities crutilitiesSingleton;
    float shakeTimer = 0f;
    float shakeTimerThreshold = 0f;

    public bool boxSelected = false;

    bool fullyZoomed = false;
    float targetPositionOnY = 1f;
    Vector3 previousInitialMoveDirection;

    HistoryObject historyObject;
    public enum State
    {
        Idle,
        Indeterminate,
        Selected,
        Moving,
        Rotating,
        BoxSelected,
        BoxRotation,
        Zoom,
        Shrink
    }
    private void Awake()
    {
        startingXRotation = this.transform.GetChild(0).localEulerAngles.x;
        faceUp = true;
        lowering = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        InitializeMovableObject();
    }

    private void InitializeMovableObject()
    {
        crutilitiesSingleton = Crutilities.singleton;
        playerOwningCard = null;
        this.state = State.Idle;
        this.idList.Clear();
        if (this.GetComponentInChildren<Card>() != null)
        {
            deck = this.GetComponentInChildren<Card>();
        }
        doubleTapTimer = doubleTapThreshold;
        if (this.transform.childCount != 0)
        {
            targetRotation = this.transform.GetChild(0).transform.localEulerAngles;
            currentLocalEulerAngles = this.transform.GetChild(0).localEulerAngles;
        }
        if (this.GetComponent<HistoryObject>() == null)
        {
            historyObject = this.gameObject.AddComponent<HistoryObject>();
        }
        else
        {
            historyObject = this.GetComponent<HistoryObject>();
        }
        historyObject.prefabToInstantiate = this.gameObject;
        historyObject.positionToInstantiate = this.transform.position;
        historyObject.currentRotation = this.transform.rotation;
        //HistoryTracker.singleton.AddToList(historyObject);
        startingSize = this.transform.localScale;
        startingTargetLerp = this.transform.localScale * 2;
    }



    protected virtual void Update()
    {
        switch (state)
        {
            case State.Idle:
                HandleIdle();
                CheckForZoom();
                HandleLowering();
                break;
            case State.Indeterminate:
                CheckForZoom();
                CheckForInputCommands();
                break;
            case State.Selected:
                HandleSelected();
                HandleLowering();
                break;
            case State.Moving:
                Move();
                CheckForZoom();
                HandleRaising();
                CheckForShuffle();
                HighlightPotentialCompatibleObjects();
                if (deck != null)
                {
                    HighlightPotentialCompatibleObjects();
                }
                break;
            case State.Rotating:
                //Move();
                HandleRaising();
                HandleRotating();
                break;
            case State.BoxSelected:
                HandleRaising();
                HandleLowering();
                break;
            case State.BoxRotation:
                HandleRotating();
                break;
            case State.Zoom:
                HandleZoom();
                Move();
                break;
            case State.Shrink:
                HandleShrink();
                Move();
                break;
        }

        HandleFlipCard();
    }



    private void HandleRotating()
    {
        if (state == State.BoxRotation)
        {
            BoxSelection.singleton.ChangeRectSizeToFitAllMovableObjects();
        }
        if (MathF.Abs(currentLocalEulerAngles.y - targetRotation.y) < 25f)
        {
            currentLocalEulerAngles.y = targetRotation.y;
            transform.GetChild(0).localEulerAngles = targetRotation;

            return;
        }
        if (currentLocalEulerAngles.y > targetRotation.y)
        {
            currentLocalEulerAngles.y -= 1500 * Time.deltaTime;
        }
        if (currentLocalEulerAngles.y < targetRotation.y)
        {
            currentLocalEulerAngles.y += 1500 * Time.deltaTime;
        }

        //transform.GetChild(0).localEulerAngles = new Vector3(transform.GetChild(0).localEulerAngles.x, currentLocalEulerAngles.y, transform.GetChild(0).localEulerAngles.z);
        transform.GetChild(0).localEulerAngles = targetRotation;

        state = State.Idle;
    }

    void HandleFlipCard()
    {
        if (doubleTapTimer > doubleTapThreshold)
        {
            return;
        }
        doubleTapTimer += Time.deltaTime;
    }
    public void HandleIdle()
    {

    }
    void CheckForShuffle()
    {

        Vector3 initialMoveDirection = fingerMovePosition - startingTouchPosition;

        if (initialMoveDirection.x > previousInitialMoveDirection.x)
        {
            shakeTimer += Time.deltaTime;
        }

        previousInitialMoveDirection = initialMoveDirection;

    }
    void HandleSelected()
    {
        Vector3 targetScale = Vector3.one;
        if (showSelectedWheel)
        {

        }
    }

    Vector3 startingTouchPosition1, startingTouchPosition2;
    public void SetTouched(int id, Vector3 positionSent)
    {
        Debug.Log("Set Touched this " + this.transform);
        if (!this.idList.Contains(id))
        {
            this.idList.Add(id);
        }
        if (state == State.Selected)
        {
            SetUnselected();
        }
        if (doubleTapTimer < doubleTapThreshold && idList.Count == 1)
        {
            SetSelected();
        }


        if (state != State.Idle)
            return;

        doubleTapTimer = 0f;
        if (playerOwningCard != null)
        {
            playerOwningCard.RemoveCardFromHand(this.gameObject);
        }

        if (this.transform.GetComponentInChildren<CardTilter>() != null)
        {
            this.transform.GetComponentInChildren<CardTilter>().SetRotationToNotZero();
        }
        if (idList.Count <= 1)
        {
            if (transform.localScale != startingSize)
            {
                frames = 0;
                state = State.Shrink;
            }
        }
        
        SubscribeToDelegates();
        historyObject.prefabToInstantiate = this.gameObject;
        historyObject.positionToInstantiate = this.transform.position;
        historyObject.currentRotation = this.transform.rotation;
        if (HistoryTracker.singleton != null)
        {
            HistoryTracker.singleton.AddToList(historyObject);
            HistoryTracker.singleton.SetTouched();
        }
        
        startingTouchPosition = positionSent;

        if (idList.Count >= 2)
        {
            if (idList[0] == id)
            {
                startingTouchPosition1 = positionSent;
            }
            if (idList[1] == id)
            {
                startingTouchPosition2 = positionSent;
            }
            startingDistanceBetweenEachFingerOnCard = Mathf.Abs((startingTouchPosition - startingTouchPosition2).magnitude);
        }

        raycastStartPos = startingTouchPosition;
        offset = new Vector3(this.transform.position.x - positionSent.x, 0, this.transform.position.z - positionSent.z);
        heldDownTimer = 0f;
        lowering = false;
        fingerMovePosition = positionSent;
        targetPositionOnY = this.transform.position.y + 1f + transform.GetComponentInChildren<Collider>().bounds.extents.y;
        snappingToThreeOnY = true;
        if (this.GetComponentInChildren<MoveTowardsWithLerp>() != null)
        {
            this.GetComponentInChildren<MoveTowardsWithLerp>().SetToIdle();
        }
        state = State.Indeterminate;
        if (idList.Count == 1)
        {
            if (transform.localScale != startingSize)
            {
                state = State.Shrink;
            }
        }
    }

    public void FlipObject()
    {
        float tempSXRotation = startingXRotation;
        Debug.Log(GetCurrentFacing() + " Flipping object");
        if (GetCurrentFacing())
        {
            transform.GetChild(0).localEulerAngles = new Vector3(tempSXRotation + 180, transform.GetChild(0).localEulerAngles.y, transform.GetChild(0).localEulerAngles.z);
            faceUp = false;
            return;
        }
        if (!GetCurrentFacing())
        {
            transform.GetChild(0).localEulerAngles = new Vector3(tempSXRotation + 180, transform.GetChild(0).localEulerAngles.y, transform.GetChild(0).localEulerAngles.z);
            faceUp = true;
            return;
        }
    }

    float distanceBetweenEachFingerOnCard = 1;
    float startingDistanceBetweenEachFingerOnCard = 1;
    void CheckForZoom()
    {
        if (state == State.Shrink)
        {
            return;
        }
        if (idList.Count >= 2)
        {
            distanceBetweenEachFingerOnCard = Mathf.Abs((fingerMovePosition2 - fingerMovePosition).magnitude);
            
            if (Mathf.Abs(startingDistanceBetweenEachFingerOnCard - distanceBetweenEachFingerOnCard) > .4f && startingDistanceBetweenEachFingerOnCard < distanceBetweenEachFingerOnCard)
            {
                frames = 0;
                state = State.Zoom;
            }
            if (Mathf.Abs(startingDistanceBetweenEachFingerOnCard - distanceBetweenEachFingerOnCard) > .4f && startingDistanceBetweenEachFingerOnCard > distanceBetweenEachFingerOnCard)
            {
                frames = 0;
                state = State.Zoom;
            }
        }
    }

    float frames = 0;
    void HandleZoom()
    {
        float multiplier = Time.deltaTime * 15;
        if (transform.localScale.x >= 2)
        {
            startingDistanceBetweenEachFingerOnCard = Mathf.Abs((fingerMovePosition2 - fingerMovePosition).magnitude);
            if (idList.Count > 1)
            {
                state = State.Moving;
            }
            if (idList.Count == 0)
            {
                state = State.Idle;
            }
            return;
        }
        transform.localScale = Vector3.Lerp(startingSize, startingTargetLerp, frames/5f);
        frames++;
        //transform.localScale += new Vector3(multiplier, multiplier, multiplier);

    }
    void HandleShrink()
    {
        float multiplier = Time.deltaTime * 15;
        if (transform.localScale.magnitude <= startingSize.magnitude + .2f)
        {
            transform.localScale = startingSize;
            
            if (idList.Count > 0)
            {
                state = State.Moving;
            }
            else
            {
                state = State.Idle;
            }
            return;
        }
        transform.localScale = Vector3.Lerp(startingTargetLerp, startingSize, frames/5f);
        frames++;
        //transform.localScale -= new Vector3(multiplier, multiplier, multiplier);
    }
    public void CheckForInputCommands()
    {
        if (fingerMovePosition == Vector3.zero)
        {
            return;
        }
        HideSelectedWheel();
        heldDownTimer += Time.deltaTime;
        Vector3 differenceBetweenStartingPositionAndMovePosition = startingTouchPosition - fingerMovePosition;
        //if held down timer is greater than helddowntimerthreshold then start moving entire entity
        if (heldDownTimer >= heldDownThreshold)
        {
            LongPress();
        }
        //if differencebetweenstartingpositionandmoveposition is greater than distancethreshold then move top card only
        if (differenceBetweenStartingPositionAndMovePosition.magnitude > distanceThreshold)
        {
            QuickDrag();
        }
        //if release before either are triggered than bring up context menu and select 
    }
    void HandleLowering()
    {
        if (lowering)
        {
            SnapToLowestPointHit();
        }
    }
    void HandleRaising()
    {
        if (snappingToThreeOnY)
        {
            SnapPositionToThreeOnY();
        }
    }
    void Move()
    {
        Vector3 targetPosition = new Vector3(fingerMovePosition.x, this.transform.position.y, fingerMovePosition.z);
        targetPosition = targetPosition + offset;
        this.transform.position = targetPosition;
    }


    public void SnapPositionToThreeOnY()
    {
        Transform targetToMove = this.transform;
        if (targetToMove.transform.position.y == targetPositionOnY)
        {
            snappingToThreeOnY = false;
        }
        targetToMove.position = Vector3.MoveTowards(targetToMove.position, new Vector3(targetToMove.position.x, targetPositionOnY, targetToMove.position.z), .05f * 200 * Time.deltaTime);
    }
    public void SnapToLowestPointHit()
    {
        Transform targetToMove = this.transform;

        targetToMove.eulerAngles = new Vector3(0, targetToMove.eulerAngles.y, 0);
        if (targetToMove.GetComponentInChildren<CardTilter>() != null)
        {
            targetToMove.GetComponentInChildren<CardTilter>().SetRotationToZero();
        }
        float lowestPointHit = FindLowestPoint();
        targetToMove.position = Vector3.MoveTowards(targetToMove.position, new Vector3(targetToMove.position.x, lowestPointHit, targetToMove.position.z), .05f * 200 * Time.deltaTime);
        if (targetToMove.transform.position.y == lowestPointHit)
        {
            lowering = false;
        }
    }
    public float FindLowestPoint()
    {
        Collider colliderHit = transform.GetComponentInChildren<Collider>();
        RaycastHit hit;
        bool hitDetected = Physics.BoxCast(colliderHit.bounds.center, new Vector3(colliderHit.bounds.extents.x, colliderHit.bounds.extents.y, colliderHit.bounds.extents.z), Vector3.down, out hit, Quaternion.identity, Mathf.Infinity);
        if (hitDetected)
        {
            if (hit.transform.GetComponent<Collider>() != null && this.transform.GetComponentInChildren<Collider>() != null)
            {
                return hit.transform.GetComponent<Collider>().bounds.extents.y + hit.transform.position.y + this.transform.GetComponentInChildren<Collider>().bounds.extents.y;
            }
        }
        return -.9f;
    }

    public void LongPress()
    {
        state = State.Moving;
    }

    public void QuickDrag()
    {
        //Draw top card 
        if (deck != null)
        {
            deck.PickUpCards(1);
            state = State.Moving;
        }
        if (deck == null)
        {
            state = State.Moving;
        }
    }
    void SetSelected()
    {
        ShowSelectedWheel();
        state = State.Selected;
    }
    void SetUnselected()
    {
        HideSelectedWheel();
        state = State.Idle;
    }
    void QuickRelease()
    {
        state = State.Idle;
    }
    void ShowSelectedWheel()
    {
        showSelectedWheel = true;
        SpawnInNewSelectedWheelTransform(this.transform.GetChild(0));
    }
    void HideSelectedWheel()
    {
        if (showSelectedWheel)
        {
            DestroySpecificSelectedWheel(this.transform);
            showSelectedWheel = false;
            if (state != State.Idle)
            {
                state = State.Idle;
            }
        }
    }

    #region delegates
    private void SubscribeToDelegates()
    {
        TouchScript.touchMoved += FingerMoved;
        for (int i = 0; i < this.GetComponentsInChildren<MonoBehaviour>().Length; i++)
        {
            this.GetComponentsInChildren<MonoBehaviour>()[i].Invoke("HaveChildSubscribeToDelegates", 0f);
        }
        TouchScript.fingerReleased += FingerReleased;
        TouchScript.rotateRight += RotateRight;
        TouchScript.rotateLeft += RotateLeft;
    }

    public void RotateRight(Vector3 position, int index)
    {
        targetRotation = new Vector3(transform.GetChild(0).localEulerAngles.x, (targetRotation.y + 90), transform.GetChild(0).localEulerAngles.z);
        state = State.Rotating;
    }
    public void RotateRightFromButton()
    {
        targetRotation = new Vector3(transform.GetChild(0).localEulerAngles.x, (targetRotation.y + 90), transform.GetChild(0).localEulerAngles.z);
        state = State.Rotating;
    }
    private void RotateLeft(Vector3 position, int index)
    {
        targetRotation = new Vector3(transform.GetChild(0).localEulerAngles.x, (targetRotation.y - 90), transform.GetChild(0).localEulerAngles.z);
        state = State.Rotating;
    }
    public void UnsubscribeToDelegates()
    {
        TouchScript.touchMoved -= FingerMoved;
        TouchScript.fingerReleased -= FingerReleased;
        TouchScript.rotateRight -= RotateRight;
        TouchScript.rotateLeft -= RotateLeft;
    }
    private void FingerReleased(Vector3 position, int index)
    {
        if (!idList.Contains(index)) return;
        if (idList.Count == 1)
        {
            if (HistoryTracker.singleton != null)
            {
                HistoryTracker.singleton.FingerReleased(historyObject);
            }

            //HistoryTracker.singleton.AddToList(historyObject);
            UnsubscribeToDelegates();
            if (state == State.Indeterminate)
            {
                QuickRelease();
            }
            if (state == State.Moving || state == State.Rotating)
            {
                lowering = true;
                snappingToThreeOnY = false;
                state = State.Idle;
               
            }
            if (deck != null)
            {
                deck.CheckToSeeIfDeckShouldBeAdded();
            }
        }
        idList.Remove(index);
    }

    public bool GetCurrentFacing()
    {
        return faceUp;
    }

    private void FingerMoved(Vector3 position, int index)
    {


        if (idList.Count >= 2)
        {
            if (index == idList[1])
            {
                Vector3 rayPosition2 = position;
                Ray ray2 = Camera.main.ScreenPointToRay(position);
                if (Physics.Raycast(ray2, out RaycastHit raycastHit2))
                {
                    fingerMovePosition2 = raycastHit2.point;
                }
            }
        }
        if (idList[0] != index) return;
        rayPosition = position;
        Ray ray = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            fingerMovePosition = raycastHit.point;
        }
    }
    #endregion

    public void RemovePlayerOwnership(PlayerContainer playerContainer)
    {
        playerOwningCard = null;
    }

    internal void GivePlayerOwnership(PlayerContainer playerContainer)
    {
        playerOwningCard = playerContainer;
    }
    public void SpawnInNewSelectedWheelTransform(Transform transformToSpawnOn)
    {
        selectedWheelGO = Instantiate(Crutilities.singleton.SelectedWheelTransform, new Vector3(this.transform.position.x, this.transform.position.y + 1f + (this.GetComponentInChildren<Collider>().bounds.size.y / 2), this.transform.position.z), this.transform.rotation);
        selectedWheelGO.transform.parent = this.transform;
    }
    public void DestroySpecificSelectedWheel(Transform transformToSpawnOn)
    {
        Destroy(selectedWheelGO);
    }

    public void Highlight()
    {
    }
    public void UnHighlight()
    {
    }

    public void SetBoxSelected()
    {
        if (this.transform.GetComponentInChildren<CardTilter>() != null)
        {
            this.transform.GetComponentInChildren<CardTilter>().SetRotationToNotZero();
        }
        Highlight();
        HideSelectedWheel();
        targetPositionOnY = this.transform.position.y + 1f + transform.GetComponentInChildren<Collider>().bounds.extents.y;
        snappingToThreeOnY = true;
        lowering = false;
        boxSelected = true;
        state = State.BoxSelected;
    }
    public void SetBoxDeselected()
    {
        state = State.Idle;
        lowering = true;
        snappingToThreeOnY = false;
        boxSelected = false;
        UnHighlight();
        SnapToLowestPointHit();
    }

    public void SetGridOffset(Vector3 positionOfBox)
    {
        offset = new Vector3(this.transform.position.x - positionOfBox.x, 0, this.transform.position.z - positionOfBox.z);
    }

    public void GridMove(Vector3 targetPositionSent)
    {
        Vector3 targetPosition = new Vector3(targetPositionSent.x, this.transform.position.y, targetPositionSent.z);
        targetPosition = targetPosition + offset;
        this.transform.position = targetPosition;
    }

    public void BoxRotateRight()
    {
        targetRotation = new Vector3(transform.GetChild(0).localEulerAngles.x, (targetRotation.y + 90), transform.GetChild(0).localEulerAngles.z);
        state = State.BoxRotation;
    }
    public void BoxRotateLeft()
    {
        targetRotation = new Vector3(transform.GetChild(0).localEulerAngles.x, (targetRotation.y - 90), transform.GetChild(0).localEulerAngles.z);
        state = State.BoxRotation;
    }

    public void ShuffleDeck()
    {
        if (deck != null)
        {
            deck.ShuffleDeck(this.offset, -1);
        }
    }

    public void PickUpTopCardOfDeckFromButton()
    {
        deck.PickUpCards(1);
    }


    public void MoveWholeDeckButton()
    {
        SubscribeToDelegates();
        HideSelectedWheel();
        state = State.Moving;
    }


    float distanceFromStartToCurrent;
    Vector3 raycastStartPos;
    Vector3 currentPosition;
    void HighlightPotentialCompatibleObjects()
    {
        distanceFromStartToCurrent = Vector3.Distance(raycastStartPos, fingerMovePosition);
        if (distanceFromStartToCurrent > .001f)
        {
            //HighlightAllCompatibleObjects();
            raycastStartPos = fingerMovePosition;
        }
    }

    GameObject previousGameObjectHit;
    void HighlightAllCompatibleObjects()
    {
        /* RaycastHit[] hits;
         hits = Physics.RaycastAll(transform.position, Vector3.down, 100.0F);

         List<Outline> newListOfMovablesToSelect = new List<Outline>();
         int numOfObjectsHighlightedPerObject = 0;
         //this for loop is to check for any player containers hit
         for (int i = 0; i < hits.Length; i++)
         {
             if (Crutilities.singleton.GetFinalParent(hits[i].transform) != this.transform)
             {
                 if (numOfObjectsHighlightedPerObject >= 1)
                 {
                     continue;
                 }
                 numOfObjectsHighlightedPerObject++;
                 if (Crutilities.singleton.GetFinalParent(hits[i].transform).GetComponent<MovableObjectStateMachine>() != null)
                 {
                     Crutilities.singleton.HighlightGameObject(hits[i].transform.gameObject);
                     newListOfMovablesToSelect.Add(hits[i].transform.gameObject.GetComponent<Outline>());
                 }
                 if (Crutilities.singleton.GetFinalParent(hits[i].transform).GetComponent<PlacementObject>() != null)
                 {
                     Crutilities.singleton.HighlightGameObject(hits[i].transform.gameObject);
                     newListOfMovablesToSelect.Add(hits[i].transform.gameObject.GetComponent<Outline>());
                 }

             }

         }



         {
             resultToAdd[j].SetBoxSelected();
         }
         selectedMovableObjects = newListOfMovablesToSelect;*/
        RaycastHit hit;
        GameObject newGameObjectHit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100.0F))
        {
            if (hit.transform.root != this.transform)
            {
                if (hit.transform.root.GetComponent<MovableObjectStateMachine>() != null)
                {
                    //crutilitiesSingleton.HighlightGameObject(hit.transform.gameObject);
                    newGameObjectHit = hit.transform.gameObject;
                    if (previousGameObjectHit != newGameObjectHit && previousGameObjectHit != null)
                    {
                        crutilitiesSingleton.RemoveHighlight(previousGameObjectHit);
                    }
                    previousGameObjectHit = newGameObjectHit;
                }
                if (hit.transform.root.GetComponent<PlacementObject>() != null)
                {
                    crutilitiesSingleton.HighlightGameObject(hit.transform.gameObject);
                    newGameObjectHit = hit.transform.gameObject;
                    if (previousGameObjectHit != newGameObjectHit && previousGameObjectHit != null)
                    {
                        crutilitiesSingleton.RemoveHighlight(previousGameObjectHit);
                    }
                    previousGameObjectHit = newGameObjectHit;
                }
            }
        }



    }

}
