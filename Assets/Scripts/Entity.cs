using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Entity : MonoBehaviour{

    GameObject player;
    
    private void FixedUpdate() {
        if (player == null) {
            player = GameObject.FindGameObjectWithTag("Player");
        } else {
            GetComponent<Rigidbody2D>().MoveRotation(player.transform.rotation.eulerAngles.z);
        }
    }
}