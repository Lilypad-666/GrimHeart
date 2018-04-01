using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagCollision : MonoBehaviour {

    public static BagCollision active = null;
    public static GameObject BagInv;
    public int bagID = -1;
    public Item[] items;

    private void Start() {
        if (BagInv == null) {
            GameObject UI = GameObject.FindGameObjectWithTag("UI Canvas");
            BagInv = UI.transform.Find("Bag").gameObject;
        }

        if (items == null) {
            items = new Item[6];

            for (int i = 0; i < items.Length; i++) {
                items[i] = new Item(ItemTags.None);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col) {

        if (active != null) {
            return;
        }

        active = this;

        setBagSlots();

        BagInv.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D col) {

        if (active != this) {
            return;
        }

        BagInv.SetActive(false);

        saveBagSlots();

        active = null;
    }

    private void OnTriggerStay2D(Collider2D col) {
       if (active == null) {
            OnTriggerEnter2D(col);
        }
    }

    public void updateSlot(int slot, int type) {

        if (items == null) {
            items = new Item[6];

            for (int i = 0; i < items.Length; i++) {
                items[i] = new Item(ItemTags.None);
            }
        }

        items[slot].setType(type);
    }

    void setBagSlots() {
        for (int i = 0; i < items.Length; i++) {
            if (items[i].getType() != ItemTags.None) {
                Transform ItemImage = BagInv.transform.GetChild(i).GetChild(0).GetChild(0);

                ItemImage.GetComponent<Image>().sprite = NetworkPlayerManager.staticSprites[items[i].getType()];
                ItemImage.GetComponent<Image>().enabled = true;
            }
        }
    }

    public void setBagSlots(Item[] newItems) {
        items = newItems;
        for (int i = 0; i < items.Length; i++) {
            Transform ItemImage = BagInv.transform.GetChild(i).GetChild(0).GetChild(0);
            if (items[i].getType() != ItemTags.None) {
                ItemImage.GetComponent<Image>().sprite = NetworkPlayerManager.staticSprites[items[i].getType()];
                ItemImage.GetComponent<Image>().enabled = true;
            } else {
                ItemImage.GetComponent<Image>().sprite = null;
                ItemImage.GetComponent<Image>().enabled = false;
            }
        }
    }

    void saveBagSlots() {
        for (int i = 0; i < BagInv.transform.childCount; i++) {
            Transform ItemImage = BagInv.transform.GetChild(i).GetChild(0).GetChild(0);

            //set type instead of sprite here
            for (int j = 0; j < NetworkPlayerManager.staticSprites.Length; j++) {
                if (ItemImage.GetComponent<Image>().sprite == NetworkPlayerManager.staticSprites[j]) {
                    items[i].setType(j);
                    break;
                }
            }
            ItemImage.GetComponent<Image>().sprite = null;
            ItemImage.GetComponent<Image>().enabled = false;
        }
    }
}
