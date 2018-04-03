using DarkRift;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {
    
    public int remoteID;

    public bool spinning;

    private float speed = 0;
    private float duration = 0;


    private void OnTriggerEnter2D(Collider2D collision) {
        GameObject hit = collision.gameObject;

        if (hit.tag == "Enemy") {
            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                //Bullet ID
                writer.Write(remoteID);

                //Enemy ID
                writer.Write(hit.GetComponent<EnemyScript>());

                using (Message message = Message.Create(Tags.HitEnemy, writer))
                    NetworkPlayerManager.staticClient.SendMessage(message, SendMode.Reliable);
            }
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