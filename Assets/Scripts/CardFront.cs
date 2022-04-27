using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFront : MonoBehaviour
{
    public void ChangeCardFront(GameObject cardFrontSent)
    {
        
        GameObject newCard = Instantiate(cardFrontSent, this.transform.parent);
        newCard.transform.SetAsFirstSibling();
        Destroy(this.gameObject);
    }
}
