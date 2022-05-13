using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryTracker : MonoBehaviour
{
    public static HistoryTracker singleton;
    public Dictionary<int, List<HistoryObject>> historyObjects = new Dictionary<int, List<HistoryObject>>();
    public Dictionary<int, List<GameObject>> historyObjectsToRemove = new Dictionary<int, List<GameObject>>();

    public Dictionary<int, GameObject> gameObjectsToInstantiate = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> gameObjectsToDestroy = new Dictionary<int, GameObject>();

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
        GameObject newGO = Instantiate(historyObject.prefabToInstantiate);
        newGO.SetActive(false);
        
        gameObjectsToInstantiate.Add(currentTurn, newGO);

        int nameExists = 0;
        for (int i = 0; i < gameObjectsToInstantiate.Count; i++)
        {
            if (gameObjectsToInstantiate[i].name == newGO.name)
            {
                nameExists++;
                if (nameExists > 1)
                {
                    if (gameObjectsToDestroy.Count >=  i)
                    {
                        gameObjectsToDestroy.Remove(i);
                    }
                   
                    gameObjectsToDestroy.Add(i, gameObjectsToInstantiate[i]);
                }
            }
        }


        currentTurn++;
        if (gameObjectsToDestroy.Count >= currentTurn)
        {
            gameObjectsToDestroy.Remove(currentTurn);
        }
        gameObjectsToDestroy.Add(currentTurn, historyObject.prefabToInstantiate);
        /*if (!historyObjects.ContainsKey(currentTurn))
        {
            List<HistoryObject> emptyHistoryObjectsList = new List<HistoryObject>();
            historyObjects.Add(currentTurn, emptyHistoryObjectsList);
        }
        
        historyObjects[currentTurn].Add(historyObject);*/
    } 
    public void FingerReleased(HistoryObject historyObject)
    {
        //Debug.Log("Adding " + historyObject.prefabToInstantiate + " to gameobjects to destroy.  Current Turn = " + currentTurn);
        /*if (!historyObjectsToRemove.ContainsKey(currentTurn))
        {
            List<GameObject> emptyHistoryObjectsList = new List<GameObject>();
            historyObjectsToRemove.Add(currentTurn, emptyHistoryObjectsList);
        }
        if (historyObjectsToRemove.ContainsKey(currentTurn))
        {
            historyObjectsToRemove.Remove(currentTurn);
            List<GameObject> emptyHistoryObjectsList = new List<GameObject>();
            historyObjectsToRemove.Add(currentTurn, emptyHistoryObjectsList);
        }
        historyObjectsToRemove[currentTurn].Add(historyObject.prefabToInstantiate);*/

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
        if (gameObjectsToInstantiate.Count <= turnToGoTo)
        {
            return;
        }
        if (currentTurn <= 0)
        {
            return;
        }
        GameObject activeObject = gameObjectsToInstantiate[turnToGoTo];
        activeObject.SetActive(true);
        activeObject.GetComponent<MovableObjectStateMachine>().UnsubscribeToDelegates();

        gameObjectsToInstantiate.Remove(turnToGoTo);

        Destroy(gameObjectsToDestroy[currentTurn]); 

        /*  for (int i = 0; i < historyObjects[turnToGoTo].Count; i++)
          {
              HistoryObject newHistoryObject = historyObjects[turnToGoTo][i];
              Debug.Log(newHistoryObject + " new history object");

              GameObject newGO = Instantiate(newHistoryObject.prefabToInstantiate, newHistoryObject.positionToInstantiate, newHistoryObject.currentRotation);
          }
          for (int i = 0; i < historyObjectsToRemove[currentTurn].Count; i++)
          {
              Debug.Log("Current object to destroy = " + historyObjectsToRemove[currentTurn][i]);
              Destroy(historyObjectsToRemove[currentTurn][i].gameObject);
          }*/
        currentTurn = turnToGoTo; 
        if (gameObjectsToInstantiate.ContainsValue(activeObject))
        {
            Debug.Log("Gameobjects to instantiate contains value of "  + activeObject + " ");
            gameObjectsToDestroy.Add(currentTurn, gameObjectsToInstantiate[turnToGoTo]); 
            Debug.Log("gameObjectsToDestroy at current turn " + gameObjectsToDestroy[currentTurn] + "  current turn = "  + currentTurn);
        }
    }
}
