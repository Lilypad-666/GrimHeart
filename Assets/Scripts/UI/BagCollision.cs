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
                items[i] = new Item(-1, ItemTags.None, -1, -1, -1, -1, -1);
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

    public void updateSlot(int slot, Item item) {

        if (items == null) {
            items = new Item[6];

            for (int i = 0; i < items.Length; i++) {
                items[i] = new Item(-1, ItemTags.None, -1, -1, -1, -1, -1);
            }
        }

        items[slot].id = item.id;
        items[slot].type = item.type;
        items[slot].minDamage = item.minDamage;
        items[slot].maxDamage = item.maxDamage;
        items[slot].rarity = item.rarity;
        items[slot].range = item.range;
        items[slot].fireRate = item.fireRate;
    }

    void setBagSlots() {
        for (int i = 0; i < items.Length; i++) {
            if (items[i].type != ItemTags.None) {
                Transform border = BagInv.transform.GetChild(i).GetChild(0);

                GameObject temp = Instantiate(NetworkPlayerManager.staticItems[items[i].type], border);
                temp.transform.localPosition = new Vector3();
                temp.transform.localScale = new Vector3(1, 1, 1);
                Item tempItem = temp.GetComponentInChildren<Item>();
                tempItem.id = items[i].id;
                tempItem.type = items[i].type;
                tempItem.minDamage = items[i].minDamage;
                tempItem.maxDamage = items[i].maxDamage;
                tempItem.rarity = items[i].rarity;
                tempItem.range = items[i].range;
                tempItem.fireRate = items[i].fireRate;
            }
        }
    }

    public void setBagSlots(Item[] newItems) {
        items = newItems;
        for (int i = 0; i < items.Length; i++) {
            Transform border = BagInv.transform.GetChild(i).GetChild(0);

            if (items[i].type != ItemTags.None) {
                GameObject temp = Instantiate(NetworkPlayerManager.staticItems[items[i].type], border);
                temp.transform.localPosition = new Vector3();
                temp.transform.localScale = new Vector3(1, 1, 1);
                Item tempItem = temp.GetComponentInChildren<Item>();
                tempItem.id = items[i].id;
                tempItem.type = items[i].type;
                tempItem.minDamage = items[i].minDamage;
                tempItem.maxDamage = items[i].maxDamage;
                tempItem.rarity = items[i].rarity;
                tempItem.range = items[i].range;
                tempItem.fireRate = items[i].fireRate;
            } else {
                int count = border.childCount;

                for (int j = 0; j < count; j++) {
                    Destroy(border.GetChild(j).gameObject);
                }
            }
        }
    }

    void saveBagSlots() {
        for (int i = 0; i < BagInv.transform.childCount; i++) {
            Transform border = BagInv.transform.GetChild(i).GetChild(0);
            
            if (border.childCount == 0) {
                items[i].type = ItemTags.None;
                items[i].id = -1;
                items[i].minDamage = -1;
                items[i].maxDamage = -1;
                items[i].rarity = -1;
                items[i].fireRate = -1;
                items[i].range = -1;
            } else {
                items[i] = border.GetChild(0).GetChild(0).GetComponent<Item>();
            }

            int count = border.childCount;

            for (int j = 0; j < count; j++) {
                Destroy(border.GetChild(j).gameObject);
            }
        }
    }
}
