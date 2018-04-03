using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ItemUI : MonoBehaviour {

    GUIStyle guistyle;
    bool showGUI = false;
    Item item = null;

    private void Start() {
        guistyle = new GUIStyle();
        guistyle.normal.textColor = Color.white;
        guistyle.fontSize = 12;
    }

    public void mouseEnter() {
        if (gameObject.transform.childCount > 0) {
            item = gameObject.transform.GetChild(0).GetComponentInChildren<Item>();
            showGUI = true;
        } else {
            item = null;
            showGUI = false;
        }
        
    }


    public void mouseExit() {
        showGUI = false;
        item = null;
    }


    public void OnGUI() {
        if (showGUI && gameObject.transform.childCount > 0 && !Input.GetMouseButton(0)) {
            GUI.DrawTexture(new Rect(Event.current.mousePosition.x + 5, Event.current.mousePosition.y - 105, 100, 100), NetworkPlayerManager.staticImage);
            GUI.Label(new Rect(Event.current.mousePosition.x + 15, Event.current.mousePosition.y - 95, 100, 100), "Rarity: " + item.rarity, guistyle);
            if (item.minDamage != -1 && item.maxDamage != -1)
                GUI.Label(new Rect(Event.current.mousePosition.x + 15, Event.current.mousePosition.y - 85, 100, 100), "Damage: " + item.minDamage + "-" + item.maxDamage, guistyle);
            if (item.range != -1)
                GUI.Label(new Rect(Event.current.mousePosition.x + 15, Event.current.mousePosition.y - 75, 100, 100), "Range: " + item.range, guistyle);
        } else {
            showGUI = false;
            item = null;
        }
    }
}
