using DarkRift;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDropHandler : MonoBehaviour, IDropHandler {
    public void OnDrop(PointerEventData eventData) {

        RectTransform invPanel = transform.GetChild(0) as RectTransform;
        
        //If it's being dropped in the inventory
        if (RectTransformUtility.RectangleContainsScreenPoint(invPanel, Input.mousePosition)) {
            GameObject[] slots = GameObject.FindGameObjectsWithTag("Inv Slot");
            moveItem(slots, eventData);
            return;
        }
        
        Transform bagTransform = transform.GetChild(1);
        RectTransform bagPanel = bagTransform as RectTransform;

        //If it's being dropped in the bag
        if (bagTransform.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(bagPanel, Input.mousePosition)) {
            GameObject[] slots = GameObject.FindGameObjectsWithTag("Bag Slot");
            moveItem(slots, eventData);

            return;
        }

        //If it's being dropped in the world
        Debug.Log("To World");
    }

    void moveItem(GameObject[] slots, PointerEventData eventData) {
        foreach (GameObject child in slots) {
            RectTransform childRect = child.transform as RectTransform;
            if (RectTransformUtility.RectangleContainsScreenPoint(childRect, Input.mousePosition)) {
                Transform from = eventData.pointerDrag.transform;
                Transform to = childRect.transform.Find("Border").GetChild(0);

                using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                    //ID
                    if (BagCollision.active != null)
                        writer.Write(BagCollision.active.bagID);
                    else
                        writer.Write(-1);

                    //From
                    writer.Write(from.GetComponentInParent<SlotID>().slotID);

                    //To
                    writer.Write(to.GetComponentInParent<SlotID>().slotID);

                    //From Item
                    writer.Write(Item.spriteToType(from.GetComponent<Image>().sprite));

                    //To Item
                    writer.Write(Item.spriteToType(to.GetComponent<Image>().sprite));

                    using (Message message = Message.Create(Tags.MoveItem, writer))
                        NetworkPlayerManager.staticClient.SendMessage(message, SendMode.Reliable);
                }

                to.GetComponent<Image>().sprite = from.GetComponent<Image>().sprite;
                to.GetComponent<Image>().enabled = true;
                from.GetComponent<Image>().sprite = null;
                from.GetComponent<Image>().enabled = false;
            }
        }
    }
}
