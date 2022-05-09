using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryTracker : MonoBehaviour
{
    public static HistoryTracker singleton;
    public Dictionary<int, List<HistoryObject>> historyObjects = new Dictionary<int, List<HistoryObject>>();

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
        historyObjects[currentTurn].Add(historyObject);
        Debug.Log(currentTurn + " " + historyObject + " History object");
    }

    public void FingerReleased(HistoryObject historyObject)
    {
        currentTurn++;
    }

    internal void SetTouched()
    {
    }

    void GoToPreviousTurn()
    {
        GoToTurn(currentTurn--);
    }
    public void GoToTurn(int turnToGoTo)
    {
        for (int i = 0; i < historyObjects[currentTurn].Count; i++)
        {
            HistoryObject newHistoryObject = historyObjects[currentTurn][i];
            Instantiate(newHistoryObject.prefabToInstantiate, newHistoryObject.positionToInstantiate, newHistoryObject.currentRotation);
        }
        for (int i = 0; i < historyObjects[currentTurn].Count; i++)
        {
            HistoryObject newHistoryObject = historyObjects[currentTurn][i];
            Instantiate(newHistoryObject.prefabToInstantiate, newHistoryObject.positionToInstantiate, newHistoryObject.currentRotation);
        }
    }
}
