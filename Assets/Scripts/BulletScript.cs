using DarkRift;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {

    public int localID;
    public int remoteID;

    public bool spinning;

    private float speed = 0;
    private float duration = 0;


    private void OnTriggerEnter2D(Collider2D collision) {
        GameObject hit = collision.gameObject;

        if (hit.tag == "Player" || hit.tag == "NetworkPlayer" || hit.tag == "Bullets" || hit.tag == "EnemyBullets") {
            return;
        }

        Debug.Log(hit.name);

        if (hit.tag == "Enemies") {
            //Send to server
        }
    }

    void FixedUpdate() {
        if (spinning) {
            GetComponentInParent<Rigidbody2D>().MoveRotation(transform.rotation.eulerAngles.z + 5);
        }
    }

    public void setSpeed(float speed) {
        this.speed = speed;
    }

    public void setDuration(float duration) {
        this.duration = duration;
    }

    public void startBullet() {
        GetComponentInParent<Rigidbody2D>().velocity = transform.up * speed;

        Destroy(gameObject, duration);
    }
}