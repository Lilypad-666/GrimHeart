using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public static GameObject itemBeingDragged;
    Vector3 startPosition;
    public Transform startParent;
    Transform canvas;


    public void OnBeginDrag(PointerEventData eventData) {
        itemBeingDragged = gameObject;
        startPosition = transform.position;
        startParent = transform.parent;
        canvas = GameObject.FindGameObjectWithTag("UI Canvas").transform;
        transform.SetParent(canvas);
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        itemBeingDragged = null;
        if (transform.parent == canvas) {
            transform.position = startPosition;
            transform.SetParent(startParent);
        }
    }
}
