using DarkRift;
using DarkRift.Server;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

public class GrimHeart : Plugin {

    const float MAP_WIDTH = 2;

    public static Dictionary<int, World> worlds = new Dictionary<int, World>();
    public static Dictionary<int, Player> players = new Dictionary<int, Player>();
    public static Dictionary<long, Item> items = new Dictionary<long, Item>();

    public override bool ThreadSafe => false;

    public override Version Version => new Version(1, 0, 0);

    public static GrimHeart GrimHeartReference;

    public MySqlConnector MySQL;

    public static Random r = new Random();

    public GrimHeart(PluginLoadData pluginLoadData) : base(pluginLoadData) {
        //Convert map to 2d array, and put in new world
        worlds.Add(0, new World(getMapFromFile(JObject.Parse(File.ReadAllText(".\\Maps\\Erden.json")))));

        Heartbeat.startHeartbeat();


        Console.WriteLine("Started Game Server");

        GrimHeartReference = this;
    }

    public void loadingChar(int AccountID, string AccountName, IClient Client, long[] items, long[] equips) {
        Random r = new Random();
        Player newPlayer = new Player(
            Client,
            AccountID,
            AccountName,
            0,
            15 + (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2,
            5 + (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2,
            items,
            equips
        );

        for (int i = 0; i < items.Length; i++) {
            Console.WriteLine(items[i]);
        }

        players.Add(Client.ID, newPlayer);
        worlds[0].addPlayer(Client, newPlayer);
    }

    public void newChar(IClient Client) {
        Random r = new Random();
        Player newPlayer = players[Client.ID];

        //Send the new player to everyone else
        using (DarkRiftWriter playerWriter = DarkRiftWriter.Create()) {
            playerWriter.Write(newPlayer.ID);
            playerWriter.Write(newPlayer.x);
            playerWriter.Write(newPlayer.y);

            using (Message playerMessage = Message.Create(Tags.SpawnPlayer, playerWriter)) {
                foreach (Player player in players.Values) {
                    IClient client = player.client;

                    if (player != players[Client.ID]) {
                        client.SendMessage(playerMessage, SendMode.Reliable);
                    }

                }
            }
        }

        //Send the controllable char to new player
        using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create()) {
            newPlayerWriter.Write(newPlayer.ID);
            newPlayerWriter.Write(newPlayer.x);
            newPlayerWriter.Write(newPlayer.y);


            using (Message newPlayerMessage = Message.Create(Tags.ControllableSpawnPlayer, newPlayerWriter))
                Client.SendMessage(newPlayerMessage, SendMode.Reliable);
        }

        //Send everyone else to new player
        using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create()) {
            foreach (Player player in players.Values) {
                if (player != players[Client.ID]) {
                    newPlayerWriter.Write(player.ID);
                    newPlayerWriter.Write(player.x);
                    newPlayerWriter.Write(player.y);
                }
            }

            using (Message newPlayerMessage = Message.Create(Tags.SpawnPlayer, newPlayerWriter))
                Client.SendMessage(newPlayerMessage, SendMode.Reliable);
        }

        using (DarkRiftWriter mapWriter = DarkRiftWriter.Create()) {

            byte[,] map = worlds[newPlayer.world].map;

            mapWriter.Write(map.GetLength(0));
            mapWriter.Write(map.GetLength(1));

            for (int i = 0; i < map.GetLength(0); i++) {
                for (int j = 0; j < map.GetLength(1); j++) {
                    mapWriter.Write(map[i, j]);
                }
            }

            using (Message mapMessage = Message.Create(Tags.SendMap, mapWriter))
                Client.SendMessage(mapMessage, SendMode.Reliable);
        }

        Enemy newEnemy = new Enemy(15 + ((float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2), 5 + ((float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2));
        worlds[0].addEnemy(newEnemy);
        worlds[newPlayer.world].sendEnemies(Client);

        /*Item[] bag = new Item[6];
        for (int i = 0; i < bag.Length; i++) {
            bag[i] = new Item(ItemTags.None, 0);
        }

        worlds[0].addBag(new Bag(15 + ((float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2), 5 + ((float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2), bag));
        worlds[newPlayer.world].sendBags(Client);*/

        sendPlayerInventory(newPlayer);

        Client.MessageReceived += messageReceived;
    }

    public void removeChar(IClient Client) {
        if (players.ContainsKey(Client.ID)) {

            Player player = players[Client.ID];
            worlds[player.world].removePlayer(Client);
            players.Remove(Client.ID);

            using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                writer.Write(Client.ID);

                using (Message message = Message.Create(Tags.DespawnPlayer, writer)) {
                    foreach (Player player2 in players.Values) {
                        IClient client = player2.client;
                        client.SendMessage(message, SendMode.Reliable);
                    }
                }
            }

            lock (MySqlConnector.Connection) {
                using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                    cmd.CommandText = "UPDATE characters SET item1 = @slot1, item2 = @slot2, item3 = @slot3, item4 = @slot4, item5 = @slot5, item6 = @slot6," +
                        "item7 = @slot7, item8 = @slot8, item9 = @slot9, equip1 = @equip1, equip2 = @equip2, equip3 = @equip3," +
                        "equip4 = @equip4 WHERE dead = 0 AND accountid = @accountid";
                    cmd.Parameters.AddWithValue("@slot1", player.items[0]);
                    cmd.Parameters.AddWithValue("@slot2", player.items[1]);
                    cmd.Parameters.AddWithValue("@slot3", player.items[2]);
                    cmd.Parameters.AddWithValue("@slot4", player.items[3]);
                    cmd.Parameters.AddWithValue("@slot5", player.items[4]);
                    cmd.Parameters.AddWithValue("@slot6", player.items[5]);
                    cmd.Parameters.AddWithValue("@slot7", player.items[6]);
                    cmd.Parameters.AddWithValue("@slot8", player.items[7]);
                    cmd.Parameters.AddWithValue("@slot9", player.items[8]);
                    cmd.Parameters.AddWithValue("@equip1", player.equips[0]);
                    cmd.Parameters.AddWithValue("@equip2", player.equips[1]);
                    cmd.Parameters.AddWithValue("@equip3", player.equips[2]);
                    cmd.Parameters.AddWithValue("@equip4", player.equips[3]);
                    cmd.Parameters.AddWithValue("@accountid", player.ID);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    void messageReceived(object sender, MessageReceivedEventArgs con) {
        using (Message message = con.GetMessage() as Message) {
            //If it's a movement tag
            if (message.Tag == Tags.MovePlayer) {

                using (DarkRiftReader reader = message.GetReader()) {
                    //Get the X and Y
                    float newX = reader.ReadSingle();
                    float newY = reader.ReadSingle();

                    Player player = players[con.Client.ID];

                    player.x = newX;
                    player.y = newY;

                    using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                        writer.Write(player.ID);
                        writer.Write(player.x);
                        writer.Write(player.y);
                        message.Serialize(writer);
                    }

                    foreach (Player player2 in worlds[players[con.Client.ID].world].players.Values) {
                        IClient client = player2.client;

                        if (player2 != players[con.Client.ID]) {
                            client.SendMessage(message, SendMode.Unreliable);
                        }
                    }
                }
            }

            if (message.Tag == Tags.Shoot) {
                using (DarkRiftReader reader = message.GetReader()) {
                    float fAngle = reader.ReadSingle();
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();

                    World world = worlds[players[con.Client.ID].world];

                    int id = world.addBullet(new Bullet(fAngle, x, y));

                    using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
                        writer.Write(id);
                        writer.Write(fAngle);
                        writer.Write(x);
                        writer.Write(y);
                        message.Serialize(writer);
                    }

                    foreach (Player player2 in world.players.Values) {
                        IClient client = player2.client;

                        if (player2 != players[con.Client.ID])
                            client.SendMessage(message, con.SendMode);
                    }
                }
            }

            if (message.Tag == Tags.MoveItem) {
                using (DarkRiftReader reader = message.GetReader()) {
                    int bagid = reader.ReadInt32();
                    byte from = reader.ReadByte();
                    byte to = reader.ReadByte();
                    long fromid = reader.ReadInt64();
                    int toid = reader.ReadInt32();

                    //If from and to are a valid item
                    if (validItem(from, fromid, bagid, players[con.Client.ID]) && validItem(to, toid, bagid, players[con.Client.ID])) {
                        moveItem(from, fromid, to, toid, bagid, players[con.Client.ID]);
                        if (bagid != -1)
                            updateBag(bagid, players[con.Client.ID].world);
                    } else {
                        sendPlayerInventory(players[con.Client.ID]);
                        if (bagid != -1)
                            updateBag(bagid, players[con.Client.ID]);
                    }
                }
            }
        }
    }

    public bool validItem(byte slot, long itemid, int bagid, Player player) {
        //In inventory
        if (slot <= 8) {
            Console.WriteLine(player.items[slot] + ", " + itemid);
            return (player.items[slot] == itemid);
        } else if (slot <= 14) {
            return (worlds[player.world].bags[bagid].items[slot - 9] == itemid);
        } else {
            return (player.equips[slot - 15] == itemid);
        }
    }

    public void moveItem(byte fromSlot, long fromid, byte toSlot, long toid, int bagid, Player player) {
        //Player inventory. move the "from" item to the "to" slot
        if (toSlot <= 8) {
            Console.WriteLine(player.items[toSlot] + " vs " + fromid);
            player.items[toSlot] = fromid;
        } else if (toSlot <= 14) {
            worlds[player.world].bags[bagid].items[toSlot - 9] = fromid;
        } else {
            player.equips[toSlot - 15] = fromid;
        }

        //Player inventory. move the "from" item to the "to" slot
        if (fromSlot <= 8) {
            player.items[fromSlot] = toid;
        } else if (fromSlot <= 14) {
            worlds[player.world].bags[bagid].items[fromSlot - 9] = toid;
        } else {
            player.equips[fromSlot - 15] = toid;
        }
    }

    public void sendPlayerInventory(Player player) {
        Console.WriteLine("Sending Player Inv");
        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            for (int i = 0; i < 9; i++) {
                if (player.items[i] == -1 || player.items[i] == 0) {
                    writer.Write(-1L);
                    writer.Write(-1);
                    writer.Write(-1);
                    writer.Write(-1);
                    writer.Write(-1);
                    writer.Write(-1);
                    writer.Write(-1);
                } else {
                    writer.Write(items[player.items[i]].id);
                    writer.Write(items[player.items[i]].type);
                    writer.Write(items[player.items[i]].minDamage);
                    writer.Write(items[player.items[i]].maxDamage);
                    writer.Write(items[player.items[i]].rarity);
                    writer.Write(items[player.items[i]].range);
                    writer.Write(items[player.items[i]].fireRate);
                }
            }

            for (int i = 0; i < 4; i++) {
                if (player.equips[i] == -1 || player.equips[i] == 0) {
                    writer.Write(-1L);
                    writer.Write(-1);
                    writer.Write(-1);
                    writer.Write(-1);
                    writer.Write(-1);
                    writer.Write(-1);
                    writer.Write(-1);
                } else {
                    writer.Write(items[player.equips[i]].id);
                    writer.Write(items[player.equips[i]].type);
                    writer.Write(items[player.equips[i]].minDamage);
                    writer.Write(items[player.equips[i]].maxDamage);
                    writer.Write(items[player.equips[i]].rarity);
                    writer.Write(items[player.equips[i]].range);
                    writer.Write(items[player.equips[i]].fireRate);
                }
            }

            using (Message message = Message.Create(Tags.SendInv, writer)) {
                IClient client = player.client;
                client.SendMessage(message, SendMode.Reliable);

            }
        }
    }

    public void updateBag(int bagID, Player player) {
        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            writer.Write(bagID);
            Bag bag = worlds[player.world].bags[bagID];
            for (int i = 0; i < 6; i++) {
                writer.Write(bag.items[i]);
                writer.Write(items[bag.items[i]].type);
                writer.Write(items[bag.items[i]].minDamage);
                writer.Write(items[bag.items[i]].maxDamage);
                writer.Write(items[bag.items[i]].rarity);
                writer.Write(items[bag.items[i]].range);
                writer.Write(items[bag.items[i]].fireRate);
            }

            using (Message message = Message.Create(Tags.UpdateBag, writer)) {
                player.client.SendMessage(message, SendMode.Reliable);
            }
        }
    }

    public void updateBag(int bagID, int world) {
        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            writer.Write(bagID);
            Bag bag = worlds[world].bags[bagID];
            bool empty = true;
            for (int i = 0; i < 6; i++) {
                if (items[bag.items[i]].type != ItemTags.None) {
                    empty = false;
                }
                writer.Write(items[bag.items[i]].type);
                writer.Write(items[bag.items[i]].minDamage);
                writer.Write(items[bag.items[i]].maxDamage);
                writer.Write(items[bag.items[i]].rarity);
                writer.Write(items[bag.items[i]].range);
                writer.Write(items[bag.items[i]].fireRate);
            }

            if (empty) {

                worlds[world].bags.Remove(bagID);

                using (DarkRiftWriter emptyBag = DarkRiftWriter.Create()) {
                    writer.Write(bagID);

                    using (Message message = Message.Create(Tags.EmptyBag, writer)) {
                        foreach (Player player2 in worlds[world].players.Values) {
                            IClient client = player2.client;

                            client.SendMessage(message, SendMode.Reliable);
                        }
                    }
                }
            } else {
                using (Message message = Message.Create(Tags.UpdateBag, writer)) {
                    foreach (Player player2 in worlds[world].players.Values) {
                        IClient client = player2.client;

                        client.SendMessage(message, SendMode.Reliable);
                    }
                }
            }
        }
    }

    public static byte[,] getMapFromFile(JObject obj) {
        //Deseralize
        RootObject rootObj = JsonConvert.DeserializeObject<RootObject>(obj.ToString());
        //Make a blank array
        byte[,] objMap = new byte[rootObj.height, rootObj.width];
        //Get the data out of the JObject
        List<byte> data = rootObj.layers.ElementAt<Layer>(0).data;
        int count = 0;
        for (int height = rootObj.height - 1; height >= 0; height--) {
            for (int width = 0; width < rootObj.width; width++) {
                objMap[height, width] = data[count];
                count++;
            }
        }
        return objMap;
    }
}