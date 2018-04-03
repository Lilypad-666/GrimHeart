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
    [Tooltip("Prefab for a bag")]
    GameObject bagPrefab;

    [SerializeField]
    [Tooltip("Prefab for an enemy")]
    GameObject enemyPrefab;

    public GameObject[] items;

    public static GameObject[] staticItems;

    public GameObject[] tiles;

    public static GameObject[] staticTiles;

    public GameObject[] defaultObjects;

    public static GameObject[] staticDefaultObjects;

    public Texture2D image;

    public GameObject[] bullets;

    public static GameObject[] staticBullets;

    public static Texture2D staticImage;

    Dictionary<int, PlayerObject> networkPlayers = new Dictionary<int, PlayerObject>();

    private void Start() {
        staticItems = items;
        staticTiles = tiles;
        staticClient = client;
        staticDefaultObjects = defaultObjects;
        staticImage = image;
        staticBullets = bullets;

        if (client == null) {
            client = GameObject.FindGameObjectWithTag("Network").GetComponent<UnityClient>();
        }
    }

    public void Awake() {
        if (client == null) {
            client = GameObject.FindGameObjectWithTag("Network").GetComponent<UnityClient>();
        }

        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
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

                    GameObject bullet = Instantiate(bullets[0], new Vector2(posX, posY), bulletRotation);

                    BulletScript bulletScript = bullet.GetComponent<BulletScript>();
                    bulletScript.remoteID = id;
                    bulletScript.setSpeed(1f);
                    bulletScript.setDuration(1f);
                    bulletScript.startBullet();
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
                        long itemid = reader.ReadInt64();
                        int type = reader.ReadInt32();
                        float minDamage = reader.ReadSingle();
                        float maxDamage = reader.ReadSingle();
                        int rarity = reader.ReadInt32();
                        float range = reader.ReadSingle();
                        float fireRate = reader.ReadSingle();
                            
                        items[i] = new Item(itemid, type, minDamage, maxDamage, range, fireRate, rarity);
                        bag.GetComponent<BagCollision>().updateSlot(i, items[i]);
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
                            long itemid = reader.ReadInt64();
                            int type = reader.ReadInt32();
                            float minDamage = reader.ReadSingle();
                            float maxDamage = reader.ReadSingle();
                            int rarity = reader.ReadInt32();
                            float range = reader.ReadSingle();
                            float fireRate = reader.ReadSingle();

                            items[j] = new Item(itemid, type, minDamage, maxDamage, range, fireRate, rarity);
                            bag.GetComponent<BagCollision>().updateSlot(j, items[j]);
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
                            long itemid = reader.ReadInt64();
                            int type = reader.ReadInt32();
                            float minDamage = reader.ReadSingle();
                            float maxDamage = reader.ReadSingle();
                            int rarity = reader.ReadInt32();
                            float range = reader.ReadSingle();
                            float fireRate = reader.ReadSingle();
                            //If is active on screen
                            if (BagCollision.active == bagCollision) {
                                Item[] items = new Item[6];

                                for (int i = 0; i < items.Length; i++) {
                                    items[i] = new Item(itemid, type, minDamage, maxDamage, range, fireRate, rarity);
                                }

                                bagCollision.setBagSlots(items);  
                            } else {
                                for (int i = 0; i < 6; i++) {
                                    bagCollision.items[i].id = itemid;
                                    bagCollision.items[i].type = type;
                                    bagCollision.items[i].minDamage = minDamage;
                                    bagCollision.items[i].maxDamage = maxDamage;
                                    bagCollision.items[i].rarity = rarity;
                                    bagCollision.items[i].range = range;
                                    bagCollision.items[i].fireRate = fireRate;
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
                        long id = reader.ReadInt64();
                        int type = reader.ReadInt32();
                        float minDamage = reader.ReadSingle();
                        float maxDamage = reader.ReadSingle();
                        int rarity = reader.ReadInt32();
                        float range = reader.ReadSingle();
                        float fireRate = reader.ReadSingle();
                        Transform slot = inventory.transform.GetChild(i);
                        Transform border = slot.GetChild(0);

                        int count = border.childCount;

                        for (int j = 0; j < count; j++) {
                            Destroy(border.GetChild(j).gameObject);
                        }

                        if (type != ItemTags.None) {
                            GameObject temp = Instantiate(items[type], border);
                            temp.transform.localPosition = new Vector3();
                            temp.transform.localScale = new Vector3(1, 1, 1);
                            Item tempItem = temp.GetComponentInChildren<Item>();
                            tempItem.id = id;
                            tempItem.type = type;
                            tempItem.minDamage = minDamage;
                            tempItem.maxDamage = maxDamage;
                            tempItem.rarity = rarity;
                            tempItem.range = range;
                            tempItem.fireRate = fireRate;
                        }

                    }

                    GameObject equips = GameObject.FindGameObjectWithTag("Equips");
                    for (int i = 0; i < 4; i++) {
                        long id = reader.ReadInt64();
                        int type = reader.ReadInt32();
                        float minDamage = reader.ReadSingle();
                        float maxDamage = reader.ReadSingle();
                        int rarity = reader.ReadInt32();
                        float range = reader.ReadSingle();
                        float fireRate = reader.ReadSingle();
                        Transform slot = equips.transform.GetChild(i);
                        Transform border = slot.GetChild(0);

                        int count = border.childCount;

                        for (int j = 0; j < count; j++) {
                            Destroy(border.GetChild(j).gameObject);
                        }

                        if (type == ItemTags.None) {
                            GameObject temp = null;
                            switch (i) {
                                case 0:
                                    temp = Instantiate(defaultObjects[0], border);
                                    break;
                                case 1:
                                    temp = Instantiate(defaultObjects[1], border);
                                    break;
                                case 2:
                                    temp = Instantiate(defaultObjects[2], border);
                                    break;
                                case 3:
                                    temp = Instantiate(defaultObjects[3], border);
                                    break;
                            }

                            if (temp != null) {
                                temp.transform.localPosition = new Vector3();
                                temp.transform.localScale = new Vector3(1, 1, 1);
                            }
                        } else {
                            GameObject temp = Instantiate(items[type], border, true);
                            temp.transform.localPosition = new Vector3();
                            temp.transform.localScale = new Vector3(1, 1, 1);
                            Item tempItem = temp.GetComponentInChildren<Item>();
                            tempItem.id = id;
                            tempItem.type = type;
                            tempItem.minDamage = minDamage;
                            tempItem.maxDamage = maxDamage;
                            tempItem.range = range;
                            tempItem.fireRate = fireRate;
                        }
                    }
                }
            }

            if (message.Tag == Tags.SpawnEnemy) {
                using (DarkRiftReader reader = message.GetReader()) {
                    int count = reader.ReadInt32();

                    for (int i = 0; i < count; i++) {
                        int id = reader.ReadInt32();
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        
                        GameObject enemy = Instantiate(enemyPrefab, new Vector3(x, y, 0), new Quaternion());

                        enemy.GetComponent<EnemyScript>().enemyID = id;
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