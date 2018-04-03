using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Item {
    public int type;
    public long id;
    public int owner;
    public int rarity = 0;
    public float minDamage = -1;
    public float maxDamage = -1;
    public float range = -1;
    public float fireRate = -1;

    public Item(long id, int type) {
        this.type = type;
        this.id = id;
    }

    public static long createWeapon(int ownerID, ItemStats.Weapon weapon) {
        int[] damages = getDamages(weapon.baseDamage);
        int minDamage = damages[0];
        int maxDamage = damages[1];
        int rarity = damages[2];
        float fireRate = (float)(Math.Round(getFireRate(weapon.baseFireRate), 2));

        long newID = -1;

        lock (MySqlConnector.Connection) {
            using (MySqlCommand cmd = MySqlConnector.Connection.CreateCommand()) {
                cmd.CommandText = "INSERT INTO items (ownedaccount, mindamage, maxdamage, distance, firerate, type, rarity) VALUES (@ownedaccount, @mindamage, @maxdamage, @distance, @firerate, @type, @rarity);" +
                    "SELECT LAST_INSERT_ID();";
                cmd.Parameters.AddWithValue("@ownedaccount", ownerID);
                cmd.Parameters.AddWithValue("@mindamage", minDamage);
                cmd.Parameters.AddWithValue("@maxdamage", maxDamage);
                cmd.Parameters.AddWithValue("@distance", weapon.range);
                cmd.Parameters.AddWithValue("@firerate", fireRate);
                cmd.Parameters.AddWithValue("@rarity", rarity);
                cmd.Parameters.AddWithValue("@type", weapon.itemType);
                using (MySqlDataReader result = cmd.ExecuteReader()) {
                    if (result.HasRows) {
                        while (result.Read()) { 
                            long.TryParse(((ulong)result[0]).ToString(), out newID);
                        }
                    } else {
                        result.Close();
                    }
                }
            }
        }

        if (newID == -1) {
            Console.WriteLine("Failed to create new weapon.");
            return -1;
        }

        Item item = new Item(newID, weapon.itemType);
        item.minDamage = minDamage;
        item.maxDamage = maxDamage;
        item.rarity = rarity;
        item.range = weapon.range;
        item.fireRate = fireRate;
        item.rarity = rarity;
        item.owner = ownerID;

        GrimHeart.items[newID] = item;

        return newID;
    }

    public static float[] damageGraph() {
        float[] result = new float[2];
        float damageModifier = -1f;
        float randomPercent = (float)(GrimHeart.r.NextDouble() * 100);
        if (randomPercent < 2f) {
            result[1] = 4;
            damageModifier = (float)(-10f * randomPercent + 100f);
        } else if ((10f <= randomPercent ? false : randomPercent >= 2f)) {
            result[1] = 3;
            damageModifier = (float)(-2.5 * (double)randomPercent + 85);
        } else if ((30f <= randomPercent ? false : randomPercent >= 10f)) {
            result[1] = 2;
            damageModifier = (float)(-randomPercent + 70f);
        } else if ((85f <= randomPercent ? false : randomPercent >= 30f)) {
            result[1] = 1;
            damageModifier = (float)(-0.25 * (double)randomPercent + 47.5);
        } else if (randomPercent >= 85f) {
            result[1] = 0;
            damageModifier = (float)(-1.75 * (double)randomPercent + 175);
        }
        result[0] = damageModifier / 10f;

        return result;
    }

    public static int getDamageDifference(int damage) {
        int num = (int)(0.25 * (double)damage * GrimHeart.r.NextDouble());
        return num;
    }

    public static int[] getDamages(float baseDamage) {
        int[] damages = new int[3];
        float[] damageResult = damageGraph();
        int avgDamage = (int)(baseDamage * damageResult[0]);
        damages[2] = (int)damageResult[1];
        int difference = getDamageDifference(avgDamage);
        damages[0] = avgDamage - difference;
        damages[1] = avgDamage + difference;
        return damages;
    }

    public static float getFireRate(float rate) {
        float single = (float)(0.75 * (double)rate + GrimHeart.r.NextDouble() * (double)rate * 0.5);
        return single;
    }
}
