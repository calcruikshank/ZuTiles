using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryTracker : MonoBehaviour
{
    public static HistoryTracker singleton;
    public Dictionary<int, List<HistoryObject>> historyObjects = new Dictionary<int, List<HistoryObject>>();
    public Dictionary<int, List<HistoryObject>> historyObjectsToRemove = new Dictionary<int, List<HistoryObject>>();


    public int currentTurn;
    //What variables do I need to keep track of? 
        //Position
        //Cards in deck
        //Order of cards in deck
        //rotation

    //Create a struct object that stores all of this information. 
    
    //When finger is touched down 

    //When finger is released Update all info for all objects effected

    // Start is called before the first frame update
    void Awake()
    {
        if (singleton != null)
        {
            Destroy(this);
        }
        singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddToList(HistoryObject historyObject)
    {
        if (!historyObjects.ContainsKey(currentTurn))
        {
            List<HistoryObject> emptyHistoryObjectsList = new List<HistoryObject>();
            historyObjects.Add(currentTurn, emptyHistoryObjectsList);
        }
        if (historyObjects.ContainsKey(currentTurn))
        {
            historyObjects.Remove(currentTurn);
            List<HistoryObject> emptyHistoryObjectsList = new List<HistoryObject>();
            historyObjects.Add(currentTurn, emptyHistoryObjectsList);
        }
        historyObjects[currentTurn].Add(historyObject);
    }

    public void FingerReleased(HistoryObject historyObject)
    {
        currentTurn++; 
        if (!historyObjectsToRemove.ContainsKey(currentTurn))
        {
            List<HistoryObject> emptyHistoryObjectsList = new List<HistoryObject>();
            historyObjectsToRemove.Add(currentTurn, emptyHistoryObjectsList);
        }
        if (historyObjectsToRemove.ContainsKey(currentTurn))
        {
            historyObjectsToRemove.Remove(currentTurn);
            List<HistoryObject> emptyHistoryObjectsList = new List<HistoryObject>();
            historyObjectsToRemove.Add(currentTurn, emptyHistoryObjectsList);
        }
        historyObjectsToRemove[currentTurn].Add(historyObject);

    }

    internal void SetTouched()
    {

    }

    void GoToPreviousTurn()
    {
        if (currentTurn == 0)
        {
            return;
        }
        int newTurnToGoTo = currentTurn - 1;
        GoToTurn(newTurnToGoTo);
    }
    public void GoToTurn(int turnToGoTo)
    {
        for (int i = 0; i < historyObjects[turnToGoTo].Count; i++)
        {
            Debug.Log(historyObjects[turnToGoTo].Count + " history object count");
            HistoryObject newHistoryObject = historyObjects[turnToGoTo][i];
            GameObject newGO = Instantiate(newHistoryObject.prefabToInstantiate, newHistoryObject.positionToInstantiate, newHistoryObject.currentRotation);
        }
        for (int i = 0; i < historyObjectsToRemove[currentTurn].Count; i++)
        {
            Debug.Log("Current object to destroy = " + historyObjectsToRemove[currentTurn][i]);
            Destroy(historyObjectsToRemove[currentTurn][i].gameObject);
        }
        currentTurn = turnToGoTo;
    }
}
