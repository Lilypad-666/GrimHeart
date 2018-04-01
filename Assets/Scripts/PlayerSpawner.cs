using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;

namespace Assets.Scripts {
    class PlayerSpawner : MonoBehaviour {
        UnityClient client;

        [SerializeField]
        [Tooltip("The controllable player prefab.")]
        GameObject controllablePrefab;

        [SerializeField]
        [Tooltip("The network controllable player prefab.")]
        GameObject networkPrefab;

        [SerializeField]
        [Tooltip("The network player manager.")]
        NetworkPlayerManager networkPlayerManager;

        void Awake() {
            if (client == null) {
                client = GameObject.FindGameObjectWithTag("Network").GetComponent<UnityClient>();

                if (client == null) {
                    Debug.LogError("Client unassigned in PlayerSpawner.");
                    Application.Quit();
                }
            }

            if (controllablePrefab == null) {
                Debug.LogError("Controllable Prefab unassigned in PlayerSpawner.");
                Application.Quit();
            }

            if (networkPrefab == null) {
                Debug.LogError("Network Prefab unassigned in PlayerSpawner.");
                Application.Quit();
            }

            client.MessageReceived += messageRecieved;
        }

        void messageRecieved(object sender, MessageReceivedEventArgs con) {
            using (Message message = con.GetMessage() as Message) {
                if (message.Tag == Tags.SpawnPlayer)
                    spawnPlayer(sender, con);
                else if (message.Tag == Tags.DespawnPlayer)
                    despawnPlayer(sender, con);
                else if (message.Tag == Tags.ControllableSpawnPlayer)
                    spawnControllablePlayer(sender, con);
            }
        }

        void despawnPlayer(object sender, MessageReceivedEventArgs con) {
            using (Message message = con.GetMessage())
            using (DarkRiftReader reader = message.GetReader())
                networkPlayerManager.destroyPlayer(reader.ReadUInt16());
        }

        void spawnPlayer(object sender, MessageReceivedEventArgs con) {
            using (Message message = con.GetMessage())
            using (DarkRiftReader reader = message.GetReader()) {
                while (reader.Position < reader.Length) {
                    int id = reader.ReadInt32();
                    Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle());

                    GameObject obj;
                    obj = Instantiate(networkPrefab, position, Quaternion.identity) as GameObject;

                    PlayerObject playerObj = obj.GetComponent<PlayerObject>();

                    networkPlayerManager.Add(id, playerObj);
                }

            }
        }

        void spawnControllablePlayer(object sender, MessageReceivedEventArgs con) {
            using (Message message = con.GetMessage())
            using (DarkRiftReader reader = message.GetReader()) {
                while (reader.Position < reader.Length) {
                    int id = reader.ReadInt32();
                    Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle());

                    GameObject obj;
                    obj = Instantiate(controllablePrefab, position, Quaternion.identity) as GameObject;

                    PlayerController playerController = obj.GetComponent<PlayerController>();
                    playerController.client = client;

                    PlayerObject playerObj = obj.GetComponent<PlayerObject>();

                    networkPlayerManager.Add(id, playerObj);
                }

            }
        }
    }
}
