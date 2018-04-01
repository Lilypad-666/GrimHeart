using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

class TabBetweenInputs : MonoBehaviour {
    EventSystem system;

    private void Start() {
        system = EventSystem.current;
    }

    private void Update() {

        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (system.currentSelectedGameObject != null) {
                Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

                Debug.Log(system.currentSelectedGameObject.name);

                if (system.currentSelectedGameObject.name == "Username" || system.currentSelectedGameObject.name == "Password" || system.currentSelectedGameObject.name == "RepeatPass") {
                    if (next != null) {

                        InputField inputfield = next.GetComponent<InputField>();
                        if (inputfield != null)
                            inputfield.OnPointerClick(new PointerEventData(system));

                        system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
                    }
                }
            }
        }
    }
}
