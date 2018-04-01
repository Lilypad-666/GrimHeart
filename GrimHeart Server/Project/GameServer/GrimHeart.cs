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

    public override bool ThreadSafe => false;

    public override Version Version => new Version(1, 0, 0);

    public static GrimHeart GrimHeartReference;

    public MySqlConnector MySQL;

    public GrimHeart(PluginLoadData pluginLoadData) : base(pluginLoadData) {
        //Convert map to 2d array, and put in new world
        worlds.Add(0, new World(getMapFromFile(JObject.Parse(File.ReadAllText(".\\Maps\\Erden.json")))));

        Heartbeat.startHeartbeat();

        Console.WriteLine("Started Game Server");

        GrimHeartReference = this;
    }

    public void loadingChar(int AccountID, string AccountName, IClient Client, int[] items) {
        Random r = new Random();
        Item[] startingItems = new Item[9];

        for (int i = 0; i < startingItems.Length; i++) {
            startingItems[i] = new Item(items[i]);
        }

        Player newPlayer = new Player(
            Client,
            AccountID,
            AccountName,
            0,
            15 + (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2,
            5 + (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2,
            startingItems
        );

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

        Item[] bag = new Item[6];
        for (int i = 0; i < bag.Length; i++) {
            bag[i] = new Item(ItemTags.None);
        }
        bag[r.Next(0, 6)].setType(ItemTags.DarkPotion);
        worlds[0].addBag(new Bag(15 + ((float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2), 5 + ((float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2), bag));
        worlds[newPlayer.world].sendBags(Client);

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
                    cmd.CommandText = "UPDATE characters SET item1 = @slot1, item2 = @slot2, item3 = @slot3, item4 = @slot4, item5 = @slot5, item6 = @slot6, item7 = @slot7, item8 = @slot8, item9 = @slot9 WHERE dead = 0 AND accountid = @accountid";
                    cmd.Parameters.AddWithValue("@slot1", player.items[0].getType());
                    cmd.Parameters.AddWithValue("@slot2", player.items[1].getType());
                    cmd.Parameters.AddWithValue("@slot3", player.items[2].getType());
                    cmd.Parameters.AddWithValue("@slot4", player.items[3].getType());
                    cmd.Parameters.AddWithValue("@slot5", player.items[4].getType());
                    cmd.Parameters.AddWithValue("@slot6", player.items[5].getType());
                    cmd.Parameters.AddWithValue("@slot7", player.items[6].getType());
                    cmd.Parameters.AddWithValue("@slot8", player.items[7].getType());
                    cmd.Parameters.AddWithValue("@slot9", player.items[8].getType());
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
                    int id = reader.ReadInt32();
                    byte from = reader.ReadByte();
                    byte to = reader.ReadByte();
                    int fromItem = reader.ReadInt32();
                    int toItem = reader.ReadInt32();

                    //If from and to are a valid item
                    if (validItem(from, fromItem, id, players[con.Client.ID]) && validItem(to, toItem, id, players[con.Client.ID])) {
                        moveItem(from, fromItem, to, toItem, id, players[con.Client.ID]);
                        if (id != -1)
                            updateBag(id, players[con.Client.ID].world);
                    } else {
                        sendPlayerInventory(players[con.Client.ID]);
                        if (id != -1)
                            updateBag(id, players[con.Client.ID]);
                    }
                }
            }
        }
    }

    public bool validItem(byte slot, int item, int bagIndex, Player player) {
        //In inventory
        if (slot <= 8) {
            return (player.items[slot].getType() == item);
        }
        return (worlds[player.world].bags[bagIndex].items[slot - 9].getType() == item);
    }

    public void moveItem(byte from, int fromItem, byte to, int toItem, int bagIndex, Player player) {
        //Player inventory. move the "to" item to the "from" slot
        if (to <= 8) {
            player.items[to].setType(fromItem);
        } else {
            worlds[player.world].bags[bagIndex].items[to - 9].setType(fromItem);
        }

        //Player inventory. move the "from" item to the "to" slot
        if (from <= 8) {
            player.items[from].setType(toItem);
        } else {
            worlds[player.world].bags[bagIndex].items[from - 9].setType(toItem);
        }
    }

    public void sendPlayerInventory(Player player) {
        using (DarkRiftWriter writer = DarkRiftWriter.Create()) {
            Item[] items = player.items;
            for (int i = 0; i < 9; i++) {
                writer.Write(items[i].getType());
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
                writer.Write(bag.items[i].getType());
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
                if (bag.items[i].getType() != ItemTags.None) {
                    empty = false;
                }
                writer.Write(bag.items[i].getType());
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