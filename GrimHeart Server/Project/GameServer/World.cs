using DarkRift;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class World {

    public Dictionary<IClient, Player> players;
    public Dictionary<int, Bullet> bullets;
    public Dictionary<int, Bag> bags;
    public Dictionary<int, Enemy> enemies;
    int currentBagID = 0;
    int currentBulletID = 0;
    int currentEnemyID = 0;
    public byte[,] map;

    public World(byte[,] map) {
        enemies = new Dictionary<int, Enemy>();
        players = new Dictionary<IClient, Player>();
        bullets = new Dictionary<int, Bullet>();
        bags = new Dictionary<int, Bag>();
        this.map = map;
    }

    public void tick() {

    }

    public int addBag(Bag bag) {
        int ID = currentBagID;

        bag.id = ID;

        bags.Add(ID, bag);

        currentBagID++;
        
        using (DarkRiftWriter bagSpawnWriter = DarkRiftWriter.Create()) {
            bagSpawnWriter.Write(bag.x);
            bagSpawnWriter.Write(bag.y);
            bagSpawnWriter.Write(bag.id);

            for (int i = 0; i < 6; i++) {
                bagSpawnWriter.Write(GrimHeart.items[bag.items[i]].type);
                bagSpawnWriter.Write(GrimHeart.items[bag.items[i]].minDamage);
                bagSpawnWriter.Write(GrimHeart.items[bag.items[i]].maxDamage);
                bagSpawnWriter.Write(GrimHeart.items[bag.items[i]].rarity);
                bagSpawnWriter.Write(GrimHeart.items[bag.items[i]].range);
                bagSpawnWriter.Write(GrimHeart.items[bag.items[i]].fireRate);
            }

            using (Message bagSpawnMessage = Message.Create(Tags.SpawnBag, bagSpawnWriter)) {
                foreach (Player player in GrimHeart.players.Values) {
                    player.client.SendMessage(bagSpawnMessage, SendMode.Reliable);
                }
            }
        }

        return ID;
    }

    public void sendBags(IClient client) {
        using (DarkRiftWriter bagSpawnWriter = DarkRiftWriter.Create()) {
            bagSpawnWriter.Write(bags.Count);

            foreach (Bag bag in bags.Values) {
                bagSpawnWriter.Write(bag.x);
                bagSpawnWriter.Write(bag.y);
                bagSpawnWriter.Write(bag.id);

                for (int j = 0; j < 6; j++) {
                    bagSpawnWriter.Write(GrimHeart.items[bag.items[j]].type);
                    bagSpawnWriter.Write(GrimHeart.items[bag.items[j]].minDamage);
                    bagSpawnWriter.Write(GrimHeart.items[bag.items[j]].maxDamage);
                    bagSpawnWriter.Write(GrimHeart.items[bag.items[j]].rarity);
                    bagSpawnWriter.Write(GrimHeart.items[bag.items[j]].range);
                    bagSpawnWriter.Write(GrimHeart.items[bag.items[j]].fireRate);
                }
            }

            using (Message bagSpawnMessage = Message.Create(Tags.SpawnBags, bagSpawnWriter)) {
                client.SendMessage(bagSpawnMessage, SendMode.Reliable);
            }
        }
    }

    public void sendEnemies(IClient client) {
        using (DarkRiftWriter enemySpawnWriter = DarkRiftWriter.Create()) {
            enemySpawnWriter.Write(enemies.Count);

            foreach (Enemy enemy in enemies.Values) {
                enemySpawnWriter.Write(enemy.id);
                enemySpawnWriter.Write(enemy.x);
                enemySpawnWriter.Write(enemy.y);
            }

            using (Message enemySpawnMessage = Message.Create(Tags.SpawnEnemy, enemySpawnWriter)) {
                client.SendMessage(enemySpawnMessage, SendMode.Reliable);
            }
        }
    }

    public int addEnemy(Enemy enemy) {
        int ID = currentEnemyID++;
        enemies.Add(ID, enemy);
        return ID;
    }

    public void updateBag(int id, long[] items) {
        bags[id].items = items;
    }

    public void removeBag(int ID) {
        bags.Remove(ID);
    }

    public void addPlayer(IClient client, Player player) {
        players.Add(client, player);
    }

    public void removePlayer(IClient client) {
        players.Remove(client);
    }

    public int addBullet(Bullet bullet) {
        int ID = currentBulletID++;
        bullets.Add(ID, bullet);
        return ID;
    }

    public void removeBullet(int id) {
        bullets.Remove(id);
    }
}