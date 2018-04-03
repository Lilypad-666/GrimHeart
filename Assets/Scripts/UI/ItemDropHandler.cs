using DarkRift;
using UnityEngine;
using UnityEngine.EventSystems;

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

        Transform equipTransform = transform.GetChild(4);
        RectTransform equipPanel = equipTransform as RectTransform;

        //If it's being dropped in the bag
        if (equipTransform.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(equipPanel, Input.mousePosition)) {
            GameObject[] slots = GameObject.FindGameObjectsWithTag("EquipSlot");
            moveItem(slots, eventData);

            return;
        }

        //If it's being dropped in the world
    }

    void moveItem(GameObject[] slots, PointerEventData eventData) {
        foreach (GameObject child in slots) {
            RectTransform childRect = child.transform as RectTransform;
            if (RectTransformUtility.RectangleContainsScreenPoint(childRect, Input.mousePosition)) {
                Transform fromObject = eventData.pointerDrag.transform;
                Transform fromBorder = fromObject.GetComponentInChildren<ItemDragHandler>().startParent;
                Transform toBorder = childRect.transform.Find("Border");

                using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                    //ID
                    if (BagCollision.active != null)
                        writer.Write(BagCollision.active.bagID);
                    else
                        writer.Write(-1);

                    //From
                    writer.Write(fromBorder.GetComponentInParent<SlotID>().slotID);

                    //To
                    writer.Write(toBorder.GetComponentInParent<SlotID>().slotID);

                    //From Item
                    writer.Write(fromObject.GetComponentInChildren<Item>().id);

                    //To Item
                    if (toBorder.childCount > 0) {
                        bool empty = false;
                        switch (toBorder.GetComponentInParent<SlotID>().slotID) {
                            case 15:
                                empty = true;
                                break;
                            case 16:
                                empty = true;
                                break;
                            case 17:
                                empty = true;
                                break;
                            case 18:
                                empty = true;
                                break;
                        }
                        if (empty)
                            writer.Write(ItemTags.None);
                        else
                            writer.Write(toBorder.GetChild(0).GetComponentInChildren<Item>().id);
                    } else {
                        writer.Write(ItemTags.None);
                    }

                    using (Message message = Message.Create(Tags.MoveItem, writer))
                        NetworkPlayerManager.staticClient.SendMessage(message, SendMode.Reliable);
                }

                int item = toBorder.childCount;
                
                //Set from's parent to to's parent
                fromObject.SetParent(toBorder);
                fromObject.transform.localPosition = new Vector3();
                fromObject.transform.localScale = new Vector3(1, 1, 1);
                if (item == 0) {
                    GameObject temp = null;
                    switch (fromBorder.GetComponentInParent<SlotID>().slotID) {
                        case 15:
                            temp = Instantiate(NetworkPlayerManager.staticDefaultObjects[0], fromBorder);
                            UserAccount.currentWeapon = -1;
                            UserAccount.weaponBullet = null;
                            break;
                        case 16:
                            temp = Instantiate(NetworkPlayerManager.staticDefaultObjects[1], fromBorder);
                            break;
                        case 17:
                            temp = Instantiate(NetworkPlayerManager.staticDefaultObjects[2], fromBorder);
                            break;
                        case 18:
                            temp = Instantiate(NetworkPlayerManager.staticDefaultObjects[3], fromBorder);
                            break;
                        default:
                            int count = fromBorder.childCount;

                            for (int i = 0; i < count; i++) {
                                Destroy(fromBorder.GetChild(i).gameObject);
                            }

                            break;
                    }

                    if (temp != null) {
                        temp.transform.localPosition = new Vector3();
                        temp.transform.localScale = new Vector3(1, 1, 1);
                    }

                } else {
                    switch (fromBorder.GetComponentInParent<SlotID>().slotID) {
                        case 15:
                            UserAccount.currentWeapon = fromObject.GetComponentInChildren<Item>().type;
                            UserAccount.weaponBullet = NetworkPlayerManager.staticBullets[ItemTags.getBullet(UserAccount.currentWeapon)];

                            Debug.Log(UserAccount.currentWeapon);
                            Debug.Log(UserAccount.weaponBullet.name);
                            break;
                    }
                    //Set to's parent to from's parent
                    Transform temp = toBorder.GetChild(0);
                    temp.SetParent(fromBorder);
                    temp.transform.localPosition = new Vector3();
                    temp.transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
    }
}
