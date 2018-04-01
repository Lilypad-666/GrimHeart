using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using DarkRift.Server;

public class PlayerController : MonoBehaviour {

    [SerializeField]
    [Tooltip("The client to communicate with the server via.")]
    public UnityClient client;

    [SerializeField]
    [Tooltip("The distance we can move before we send a position update.")]
    float moveDistance = 0.05f;

    [SerializeField]
    [Tooltip("Prefab for the Bullet")]
    GameObject bulletPrefab;


    //movement
    Vector3 lastPosition;
    private float maxSpeed = 0.1f;
    float x = 0;
    float y = 0;

    //Rotation
    bool rotateRight = false;
    bool rotateLeft = false;
    public static float rotation;

    //Shooting
    bool shot = false;
    float nextFire = 0.0f;
    float lastFire = 0.0f;

    void Start() {
        lastPosition = transform.position;

        Camera.main.transform.parent = transform;
        //Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y - 0.4f, -10);
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y - 0.8f, -10);
        Camera.main.transform.rotation = Quaternion.Euler(-4.5f, 0, 0);
    }


    // Update is called once per frame
    void FixedUpdate() {

        if (rotateLeft) {
            GetComponentInParent<Rigidbody2D>().rotation = GetComponentInParent<Rigidbody2D>().rotation + 3;

            rotateLeft = false;
        }

        if (rotateRight) {
            GetComponentInParent<Rigidbody2D>().rotation = GetComponentInParent<Rigidbody2D>().rotation - 3;
            rotateRight = false;
        }

        GetComponent<Rigidbody2D>().MovePosition(new Vector3(transform.position.x, transform.position.y, Mathf.Sin(((rotation) % 360) * Mathf.Deg2Rad) / 1000));

        rotation = transform.rotation.eulerAngles.z;

        GameObject[] bags = GameObject.FindGameObjectsWithTag("Loot");

        foreach (GameObject bag in bags) {
            if (bag != null) {
                bag.GetComponent<Rigidbody2D>().MoveRotation(transform.rotation.eulerAngles.z);
            }
        }

        //If there is some movement
        if (x != 0 || y != 0) {
            //Set the direction of the vector in the way of the movement
            Vector3 direction = new Vector3(x, y);
            //Add the normalized force to the rigidbody
            GetComponentInParent<Rigidbody2D>().AddRelativeForce(direction.normalized * 100);
        } else {
            GetComponentInParent<Rigidbody2D>().velocity = new Vector2();
        }

        //If distance is greater than amount to send
        if (Vector3.Distance(lastPosition, transform.position) > moveDistance) {
            //Then send the new position
            sendPosition();
        }
    }

    private void Update() {
        getRotate();
        getMove();
        getShot();

        if (Input.GetKey(KeyCode.Mouse0) && Time.time > nextFire) {

            nextFire = Time.time + (1 / 2f);

            lastFire = Time.time;

            Vector3 v3Pos;
            float fAngle;
            float pAngle;

            //Convert the player to Screen coordinates
            v3Pos = Camera.main.WorldToScreenPoint(transform.position);
            v3Pos = Input.mousePosition - v3Pos;
            fAngle = Mathf.Atan2(v3Pos.y, v3Pos.x) * Mathf.Rad2Deg;
            pAngle = fAngle;
            fAngle += transform.rotation.eulerAngles.z;

            if (fAngle < 0.0f) fAngle += 360.0f;
            if (fAngle > 360.0f) fAngle -= 360.0f;

            if (pAngle < 0.0f) pAngle += 360.0f;
            if (pAngle > 360.0f) pAngle -= 360.0f;

            Quaternion bulletRotation = Quaternion.Euler(new Vector3(0f, 0f, fAngle - 90f));

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;

            GameObject bullet = Instantiate(bulletPrefab, transform.position, bulletRotation);

            BulletScript bullets = bullet.GetComponent<BulletScript>();
            bullets.setSpeed(1f);
            bullets.setDuration(1f);
            bullets.startBullet();

            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                writer.Write(fAngle);
                writer.Write(transform.position.x);
                writer.Write(transform.position.y);

                using (Message message = Message.Create(Tags.Shoot, writer))
                    client.SendMessage(message, SendMode.Unreliable);
            }

            //bullets.localID = localShotID;
        }
    }

    void sendPosition() {
        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            writer.Write(transform.position.x);
            writer.Write(transform.position.y);

            using (Message message = Message.Create(Tags.MovePlayer, writer))
                client.SendMessage(message, SendMode.Unreliable);
        }

        lastPosition = transform.position;
    }

    void getShot() {
        if (Input.GetKey(KeyCode.Mouse0) && Input.mousePosition.x > 240) {
            shot = true;
        } else {
            shot = false;
        }
    }

    void getRotate() {
        //Rotation Left
        if (Input.GetKey(KeyCode.Quote)) {
            rotateLeft = true;
        }

        //Rotation Right
        if (Input.GetKey(KeyCode.Period)) {
            rotateRight = true;
        }
    }

    void getMove() {
        //Get X and Y from joystick/keyboard
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
    }
}