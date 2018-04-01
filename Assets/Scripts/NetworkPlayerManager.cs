using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkPlayerManager : MonoBehaviour {
    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    public static UnityClient staticClient;

    [SerializeField]
    [Tooltip("Prefab for the Opponent Bullet")]
    GameObject bulletPrefab;

    [SerializeField]
    [Tooltip("Prefab for a bag")]
    GameObject bagPrefab;

    [SerializeField]
    [Tooltip("Sprite for item")]
    Sprite itemSprite;

    public Sprite[] sprites;

    public static Sprite[] staticSprites;

    public GameObject[] tiles;

    public static GameObject[] staticTiles;

    Dictionary<int, PlayerObject> networkPlayers = new Dictionary<int, PlayerObject>();

    private void Start() {
        staticSprites = sprites;
        staticTiles = tiles;
        staticClient = client;

        if (client == null) {
            client = GameObject.FindGameObjectWithTag("Network").GetComponent<UnityClient>();
        }
    }

    public void Awake() {
        if (client == null) {
            client = GameObject.FindGameObjectWithTag("Network").GetComponent<UnityClient>();
        }

        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            Debug.Log("Sending loaded");
            using (Message message = Message.Create(Tags.InGame, writer))
                client.SendMessage(message, SendMode.Reliable);
        }

        client.MessageReceived += messageReceived;
    }

    void messageReceived(object sender, MessageReceivedEventArgs con) {
        using (Message message = con.GetMessage() as Message) {
            if (message.Tag == Tags.MovePlayer) {
                using (DarkRiftReader reader = message.GetReader()) {
                    int id = reader.ReadInt32();
                    Vector3 newPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0);

                    networkPlayers[id].setTransform(newPosition);
                }
            }

            if (message.Tag == Tags.SendMap) {
                using (DarkRiftReader reader = message.GetReader()) {
                    int width = reader.ReadInt32();
                    int height = reader.ReadInt32();

                    for (int i = 0; i < width; i++) {
                        for (int j = 0; j < height; j++) {
                            byte next = reader.ReadByte();
                            if (next > 0) {
                                next--;
                                switch (next) {
                                    case 32:
                                    case 160:
                                    case 162:
                                    case 163:
                                    case 164:
                                        Instantiate(tiles[next], new Vector3(j / 6.25f, i / 6.25f, (i / 6.25f) / 1000 - 0.48f), Quaternion.identity);
                                        break;
                                    default:
                                        Instantiate(tiles[next], new Vector3(j / 6.25f, i / 6.25f, (i / 6.25f) / 1000 + 0.02f), Quaternion.identity);
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            if (message.Tag == Tags.Shoot) {
                using (DarkRiftReader reader = message.GetReader()) {
                    int id = reader.ReadInt32();
                    float fAngle = reader.ReadSingle();
                    float posX = reader.ReadSingle();
                    float posY = reader.ReadSingle();

                    Quaternion bulletRotation = Quaternion.Euler(new Vector3(0f, 0f, fAngle - 90f));

                    Vector3 mousePos = Input.mousePosition;
                    mousePos.z = 10;

                    GameObject bullet = Instantiate(bulletPrefab, new Vector2(posX, posY), bulletRotation);

                    BulletScript bullets = bullet.GetComponent<BulletScript>();
                    bullets.remoteID = id;
                    bullets.setSpeed(1f);
                    bullets.setDuration(1f);
                    bullets.startBullet();
                }
            }

            if (message.Tag == Tags.SpawnBag) {
                using (DarkRiftReader reader = message.GetReader()) {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    int id = reader.ReadInt32();

                    GameObject bag = Instantiate(bagPrefab, new Vector3(x, y, 0), new Quaternion());

                    Item[] items = new Item[6];

                    for (int i = 0; i < 6; i++) {
                        items[i] = new Item(reader.ReadInt32());
                        bag.GetComponent<BagCollision>().updateSlot(i, items[i].getType());
                    }

                    bag.GetComponent<BagCollision>().bagID = id;
                }
            }

            if (message.Tag == Tags.SpawnBags) {
                using (DarkRiftReader reader = message.GetReader()) {
                    int count = reader.ReadInt32();

                    for (int i = 0; i < count; i++) {
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        int id = reader.ReadInt32();

                        GameObject bag = Instantiate(bagPrefab, new Vector3(x, y, 0), new Quaternion());

                        Item[] items = new Item[6];

                        for (int j = 0; j < 6; j++) {
                            items[j] = new Item(reader.ReadInt32());
                            bag.GetComponent<BagCollision>().updateSlot(j, items[j].getType());
                        }

                        bag.GetComponent<BagCollision>().bagID = id;
                    }

                }
            }

            if (message.Tag == Tags.UpdateBag) {
                using (DarkRiftReader reader = message.GetReader()) {
                    int id = reader.ReadInt32();

                    GameObject[] bags = GameObject.FindGameObjectsWithTag("Loot");

                    foreach (GameObject bag in bags) {
                        BagCollision bagCollision = bag.GetComponent<BagCollision>();
                        if (bagCollision.bagID == id) {
                            //If is active on screen
                            if (BagCollision.active == bagCollision) {
                                Item[] items = new Item[6];

                                for (int i = 0; i < items.Length; i++) {
                                    items[i] = new Item(reader.ReadInt32());
                                }

                                bagCollision.setBagSlots(items);  
                            } else {
                                for (int i = 0; i < 6; i++) {
                                    bagCollision.items[i].setType(reader.ReadInt32());
                                }
                            }

                            break;
                        }
                    }
                }
            }

            if (message.Tag == Tags.EmptyBag) {
                using (DarkRiftReader reader = message.GetReader()) {
                    int id = reader.ReadInt32();

                    GameObject[] loot = GameObject.FindGameObjectsWithTag("Loot");

                    foreach (GameObject bag in loot) {
                        if (bag.GetComponent<BagCollision>().bagID == id) {
                            Destroy(bag);
                        }
                    }
                }
            }

            if (message.Tag == Tags.SendInv) {
                using (DarkRiftReader reader = message.GetReader()) {
                    GameObject inventory = GameObject.FindGameObjectWithTag("Inventory");
                    for (int i = 0; i < 9; i++) {
                        int item = reader.ReadInt32();
                        Transform slot = inventory.transform.GetChild(i);
                        Image image = slot.GetChild(0).GetChild(0).GetComponent<Image>();

                        if (item == ItemTags.None) {
                            image.sprite = null;
                            image.enabled = false;
                        } else {
                            image.sprite = sprites[item];
                            image.enabled = true;
                        }

                    }
                }
            }
        }
    }

    public void Add(int id, PlayerObject player) {
        networkPlayers.Add(id, player);
    }

    public void destroyPlayer(ushort id) {
        PlayerObject o = networkPlayers[id];

        Destroy(o.gameObject);

        networkPlayers.Remove(id);
    }
}