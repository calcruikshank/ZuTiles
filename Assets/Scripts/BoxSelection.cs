using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoxSelection : MonoBehaviour
{
    public State state; public List<int> idList = new List<int>();
    [SerializeField] private RectTransform selectionBox;
    private List<MovableObjectStateMachine> selectedMovableObjects = new List<MovableObjectStateMachine>();
    Vector3 startPosition, currentPosition;

    public enum State
    {
        Idle,
        Indeterminate,
        Selected,
        Moving,
        Rotating,
        BoxSelected,
        BoxRotation
    }

    [SerializeField] LayerMask tableMask;
    private Camera cam;
    int id = -1;
    float width;
    float height;

    Vector3 fingerMovePosition;
    Vector3 offset;
    Vector3 raycastStartPos;

    bool alreadySubscribed = false;

    float distanceFromStartToCurrent;

    [SerializeField] GameObject closeButton;
    bool moving = false;
    public static BoxSelection singleton;
    GameObject newCloseButton, newSelectionWheel;

    [SerializeField] GameObject selectionWheelBox;
    private void Awake()
    {
        singleton = this;
        cam = Camera.main;
        this.state = State.Idle;
    }

    private void Update()
    {
        /*switch (state)
        {
            case State.Idle:
                HandleIdle();
                HandleLowering();
                break;
            case State.Indeterminate:
                CheckForInputCommands();
                CheckToSeeIfShouldBeginRotating();
                break;
            case State.Selected:
                HandleSelected();
                HandleLowering();
                break;
            case State.Moving:
                Move();
                HandleRaising();
                CheckToSeeIfShouldBeginRotating();
                CheckForShuffle();
                break;
            case State.Rotating:
                Move();
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
        }*/
    }

    public void BeginDraggingGrid(int indexSent, Vector3 positionSent)
    {
        if (id != -1)
        {
            return;
        }
        if (selectedMovableObjects.Count >= 1)
        {
            return;
        }
        id = indexSent;
        currentPosition = positionSent;
        startPosition = positionSent;
        Ray ray = Camera.main.ScreenPointToRay(startPosition);
        startingWorldPosition = ray.GetPoint(Camera.main.transform.position.y);
        
        SubscribeToDelegates();
        Debug.Log("Begin draggin Grid");
        raycastStartPos = startPosition;
        UpdateSelectionBox();
        moving = false;
    }
    void SubscribeToDelegates()
    {
        TouchScript.touchMoved += FingerMoved;
        TouchScript.fingerReleased += FingerReleased;
        if (!alreadySubscribed)
        {
            TouchScript.rotateRight += RotateAllMovableObjectsSelected;
            TouchScript.rotateLeft += RotateAllMovableObjectsLeft;
            alreadySubscribed = true;
        }
    }


    void UnsubscribeToDelegates()
    {
        TouchScript.touchMoved -= FingerMoved;
        TouchScript.fingerReleased -= FingerReleased;
    }

    private void FingerReleased(Vector3 position, int index)
    {
        if (id != index)
        {
            return;
        }
        id = -1;
        UnsubscribeToDelegates();
        if (selectedMovableObjects.Count == 0)
        {
            TouchScript.rotateRight -= RotateAllMovableObjectsSelected;
            TouchScript.rotateLeft -= RotateAllMovableObjectsLeft;
            alreadySubscribed = false;
        }
        ChangeRectSizeToFitAllMovableObjects();
        
    }

    public void ChangeRectSizeToFitAllMovableObjects()
    {

        List<Collider> movableObjectColliders = new List<Collider>();
        for (int i = 0; i < selectedMovableObjects.Count; i++)
        {
            movableObjectColliders.Add(selectedMovableObjects[i].transform.GetComponentInChildren<Collider>());
        }

        float mostNegativeX = 0, mostPositiveX = 0, mostNegativeY = 0, mostPositiveY = 0;
        for (int i = 0; i < movableObjectColliders.Count; i++)
        {
            //position on the x - halfwidth
            float leftBounds = movableObjectColliders[i].transform.position.x - (movableObjectColliders[i].bounds.size.x / 2);
            float rightBound = movableObjectColliders[i].transform.position.x + (movableObjectColliders[i].bounds.size.x / 2);
            float topBounds = movableObjectColliders[i].transform.position.z + (movableObjectColliders[i].bounds.size.z / 2);
            float bottomBounds = movableObjectColliders[i].transform.position.z - (movableObjectColliders[i].bounds.size.z / 2);

            if (leftBounds < mostNegativeX || mostNegativeX == 0)
            {
                mostNegativeX = leftBounds;
            }
            if (rightBound > mostPositiveX || mostPositiveX == 0)
            {
                mostPositiveX = rightBound;
            }
            if (topBounds > mostPositiveY || mostPositiveY == 0)
            {
                mostPositiveY = topBounds;
            }
            if (bottomBounds < mostNegativeY || mostNegativeY == 0)
            {
                mostNegativeY = bottomBounds;
            }
        }
        width = mostPositiveX - mostNegativeX;
        height = mostPositiveY - mostNegativeY;

        selectionBox.localScale = new Vector3(MathF.Abs(width), MathF.Abs(height), 1);
        selectionBox.anchoredPosition3D = new Vector3(mostNegativeX + width / 2, selectionBox.transform.position.y, mostNegativeY + height / 2);
        for (int i = 0; i < selectedMovableObjects.Count; i++)
        {
            Debug.Log(selectedMovableObjects[i]);
            selectedMovableObjects[i].SetGridOffset(this.selectionBox.position);
        }
        if (newCloseButton == null)
        {
            SpawnInCloseButton(width, height);
        }
    }

    public void SpawnInCloseButton(float width, float height)
    {
        //if start position.x < currentposition.x && startposition.z < currentPosition.z
        if (startPosition.x < currentPosition.x && startPosition.y > currentPosition.y)
        {
            Vector3 offset = new Vector3(selectionBox.transform.position.x + width / 2, selectedMovableObjects[0].transform.position.y + .2f, selectionBox.transform.position.z + height / 2);
            //call this method when finger is released and selected movable objects.count > 0 
            newCloseButton = Instantiate(closeButton, offset, Quaternion.identity);
            closeButtonOffset = offset;
            //if seleceted movable object count is == 0 then call closeBox
            //newCloseButton.transform.parent = selectionBox;
        }
        else if (startPosition.x < currentPosition.x && startPosition.y < currentPosition.y)
        {
            Vector3 offset = new Vector3(selectionBox.transform.position.x + width / 2, selectedMovableObjects[0].transform.position.y + .2f, selectionBox.transform.position.z + height / 2);
            //call this method when finger is released and selected movable objects.count > 0 
            newCloseButton = Instantiate(closeButton, offset, Quaternion.identity);
            closeButtonOffset = offset;
            //  newCloseButton.transform.parent = selectionBox;
        }
        else if (startPosition.x > currentPosition.x && startPosition.y < currentPosition.y)
        {
            Vector3 offset = new Vector3(selectionBox.transform.position.x + width / 2, selectedMovableObjects[0].transform.position.y + .2f, selectionBox.transform.position.z + height / 2);
            //call this method when finger is released and selected movable objects.count > 0 
            newCloseButton = Instantiate(closeButton, offset, Quaternion.identity);
            closeButtonOffset = offset;
            //newCloseButton.transform.parent = selectionBox;
        }
        else if (startPosition.x > currentPosition.x && startPosition.y > currentPosition.y)
        {
            Vector3 offset = new Vector3(selectionBox.transform.position.x + width / 2, selectedMovableObjects[0].transform.position.y + .2f, selectionBox.transform.position.z + height / 2);
            //call this method when finger is released and selected movable objects.count > 0 
            newCloseButton = Instantiate(closeButton, offset, Quaternion.identity);
            closeButtonOffset = offset;
            // newCloseButton.transform.parent = selectionBox;
        }
        else
        {
            //call this method when finger is released and selected movable objects.count > 0 
            Vector3 offset = new Vector3(selectionBox.transform.position.x + width / 2, selectedMovableObjects[0].transform.position.y + .2f, selectionBox.transform.position.z + height / 2);
            //call this method when finger is released and selected movable objects.count > 0 
            newCloseButton = Instantiate(closeButton, offset, Quaternion.identity);
            closeButtonOffset = offset;
            //if seleceted movable object count is == 0 then call closeBox
            // newCloseButton.transform.parent = selectionBox;
        }
        if (newSelectionWheel == null)
        {
           newSelectionWheel = Instantiate(selectionWheelBox, new Vector3(selectionBox.transform.position.x, 7, selectionBox.transform.position.z), Quaternion.identity);
            newSelectionWheel.transform.parent = selectionBox;
            ButtonSelector[] buttonSelectors = newSelectionWheel.GetComponentsInChildren<ButtonSelector>();
            for (int i = 0; i < buttonSelectors.Length; i++)
            {
                buttonSelectors[i].SetTargetTransform(selectionBox);
            }
        }

        if (newCloseButton != null)
        {
            newCloseButton.GetComponentInChildren<ButtonSelector>().SetTargetTransform(this.transform);
        }

    }

    public void FlipObject()
    {
        for (int i = 0; i < selectedMovableObjects.Count; i++)
        {
            selectedMovableObjects[i].FlipObject();
        }
    }
    
    public void Close()
    {
        if (selectionBox.gameObject.activeInHierarchy)
        {
            selectionBox.gameObject.SetActive(false);
        }
        for (int i = 0; i < selectedMovableObjects.Count; i++)
        {
            selectedMovableObjects[i].SetBoxDeselected();
        }
        selectedMovableObjects.Clear();
        Destroy(newCloseButton);
        Destroy(newSelectionWheel);
    }
    private void FingerMoved(Vector3 position, int index)
    {
        if (id != index)
        {
            return;
        }
        if (!moving)
        {
            UpdateSelectionBox();
            Ray ray = Camera.main.ScreenPointToRay(position);
            currentPosition = ray.GetPoint(Camera.main.transform.position.y);
            
        }
        if (moving)
        {
            Ray ray = Camera.main.ScreenPointToRay(position);
            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                fingerMovePosition = raycastHit.point;
            }
            Move();
        }
    }

    Vector3 startingWorldPosition;
    void UpdateSelectionBox()
    {
        if (!selectionBox.gameObject.activeInHierarchy)
        {
            selectionBox.gameObject.SetActive(true);
        }
        Vector3 currentWorldPosition = currentPosition;
        
        width = currentWorldPosition.x - startingWorldPosition.x;
        height = currentWorldPosition.z - startingWorldPosition.z;
        selectionBox.localScale = new Vector3(MathF.Abs(width), MathF.Abs(height), 1);

        selectionBox.anchoredPosition3D = new Vector3(startingWorldPosition.x + width / 2, startingWorldPosition.y, startingWorldPosition.z + height / 2);

        distanceFromStartToCurrent = Vector3.Distance(raycastStartPos, currentPosition);
        if (distanceFromStartToCurrent > .01f)
        {
            ShootRayCastToCheckForMovableObjects();
            raycastStartPos = currentPosition;
        }
    }
    void ShootRayCastToCheckForMovableObjects()
    {
        RaycastHit[] objectsHit = Physics.BoxCastAll(selectionBox.position, new Vector3(selectionBox.localScale.x / 2, selectionBox.localScale.z / 2, selectionBox.localScale.y / 2), Vector3.down, Quaternion.identity);
        Debug.Log(objectsHit.Length);
        List<MovableObjectStateMachine> newListOfMovablesToSelect = new List<MovableObjectStateMachine>();
        for (int i = 0; i < objectsHit.Length; i++)
        {
            if (objectsHit[i].transform.root.GetComponent<MovableObjectStateMachine>() != null)
            {
                newListOfMovablesToSelect.Add(objectsHit[i].transform.root.GetComponent<MovableObjectStateMachine>());
            }
        }

        List<MovableObjectStateMachine> result = selectedMovableObjects.Except(newListOfMovablesToSelect).ToList<MovableObjectStateMachine>();

        for (int i = 0; i < result.Count(); i++)
        {
            result[i].SetBoxDeselected();
        }
        List<MovableObjectStateMachine> resultToAdd = newListOfMovablesToSelect.Except(selectedMovableObjects).ToList<MovableObjectStateMachine>();

        for (int j = 0; j < resultToAdd.Count(); j++)
        {
            resultToAdd[j].SetBoxSelected();
        }
        selectedMovableObjects = newListOfMovablesToSelect;
    }

    bool ArrayContains(RaycastHit[] arraySent, RaycastHit rayToCheck)
    {
        foreach (RaycastHit ray in arraySent)
        {
            if (rayToCheck.transform == ray.transform)
            {
                return true;
            }
        }
        return false;
    }
    Vector3 closeButtonOffset;
    void Move()
    {
        Vector3 targetPosition = new Vector3(fingerMovePosition.x, this.selectionBox.transform.position.y, fingerMovePosition.z);
        targetPosition = targetPosition + offset;
        Vector3 targetCloseButtonPosition = targetPosition + closeButtonOffset;
        selectionBox.transform.position = targetPosition;
        MoveAllObjectsWithinSelection(targetPosition);

        if (newCloseButton != null)
        {
            newCloseButton.transform.position = targetCloseButtonPosition;
        }
    }

    public void MoveAllObjectsWithinSelection(Vector3 targetPosition)
    {
        for (int i = 0; i < selectedMovableObjects.Count; i++)
        {
            selectedMovableObjects[i].transform.root.GetComponentInChildren<MovableObjectStateMachine>().GridMove(targetPosition);
        }
    }
    public void SetBoxSelected(int index, Vector3 positionSent)
    {
        this.id = index;
        SubscribeToDelegates();
        offset = new Vector3(this.selectionBox.position.x - positionSent.x, 0, this.selectionBox.position.z - positionSent.z);
        moving = true;
    }
    private void RotateAllMovableObjectsSelected(Vector3 position, int index)
    {
        for (int i = 0; i < selectedMovableObjects.Count; i++)
        {
            selectedMovableObjects[i].transform.root.GetComponentInChildren<MovableObjectStateMachine>().BoxRotateRight();
        }
    }
    private void RotateAllMovableObjectsLeft(Vector3 position, int index)
    {
        for (int i = 0; i < selectedMovableObjects.Count; i++)
        {
            selectedMovableObjects[i].transform.root.GetComponentInChildren<MovableObjectStateMachine>().BoxRotateLeft();
        }
    }
   
    public void Shuffle()
    {
        for (int i = 0; i < selectedMovableObjects.Count; i++)
        {
            selectedMovableObjects[i].transform.root.GetComponentInChildren<MovableObjectStateMachine>().ShuffleDeck();
        }
    }

    public void GroupAllSimilarObjects()
    {
        //choose a master object to add all other objects to i[0]
        bool hasChosenMasterObject = false;
        Deck deckToAddTo = new Deck();
        MovableObjectStateMachine deckToAddToMovable = new MovableObjectStateMachine();
        for (int i = 0; i < selectedMovableObjects.Count; i++)
        {
            if (selectedMovableObjects[i].transform.GetComponentInChildren<Deck>() != null)
            {
                if (hasChosenMasterObject)
                {
                    if (selectedMovableObjects[i].GetCurrentFacing())
                    {
                        if (deckToAddTo != null)
                        {
                            deckToAddTo.transform.position = selectionBox.transform.position;
                            deckToAddTo.AddToDeck(selectedMovableObjects[i].GetComponentInChildren<Deck>().cardsInDeck);
                            selectedMovableObjects[i].GetComponentInChildren<Deck>().UpdateDeckInfo();
                            Destroy(selectedMovableObjects[i].gameObject);
                        }
                    }
                }
                if (!hasChosenMasterObject)
                {
                    hasChosenMasterObject = true;
                    deckToAddTo = selectedMovableObjects[i].GetComponentInChildren<Deck>(); deckToAddToMovable = selectedMovableObjects[i];
                }
                
                
            }
        }

        Close();
    }


    public void RotateRightFromButton()
    {
        for (int i = 0; i < selectedMovableObjects.Count; i++)
        {
            selectedMovableObjects[i].transform.root.GetComponentInChildren<MovableObjectStateMachine>().RotateRight(selectedMovableObjects[i].transform.position, 0);
        }
    }
}


