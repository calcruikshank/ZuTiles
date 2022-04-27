using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZuTiileButtonSelector : MonoBehaviour
{
    [SerializeField] GameObject deckToInstantiate;
    ZuTilePlayer currentZuTilePlayer;
    void HaveChildSubscribeToDelegates()
    {
        Debug.Log("Subscribe to delegates"); TouchScript.touchMoved += FingerMoved;
        
        TouchScript.fingerReleased += FingerReleased;
        if (currentZuTilePlayer != null)
        {
            currentZuTilePlayer.RemoveDeckSelection();
            currentZuTilePlayer = null;
        }
    }


    private void FingerReleased(Vector3 position, int index)
    {
        CheckToSeeIfPlayerContainerHit();
    }

    private void CheckToSeeIfPlayerContainerHit()
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, Vector3.down, 100.0F);

        //this for loop is to check for any player containers hit
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.GetComponentInChildren<PlayerContainer>() != null)
            {
                PlayerContainer playerToAddCardTo = hits[i].transform.GetComponentInChildren<PlayerContainer>();
                Debug.Log(deckToInstantiate);
                Crutilities.singleton.GetFinalParent(playerToAddCardTo.transform).transform.GetComponentInChildren<ZuTilePlayer>().SetDeckToDeckToInstantiate(deckToInstantiate, this.gameObject);
            }
        }
        UnsubFromDelegates();
    }

    private void UnsubFromDelegates()
    {
        TouchScript.fingerReleased -= FingerReleased;
    }

    private void FingerMoved(Vector3 position, int index)
    {
    }
}
