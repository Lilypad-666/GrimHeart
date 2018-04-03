using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Player {
    public IClient client { get; set; }
    public int ID { get; set; }
    public String name { get; set; }
    public int world { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public long[] items;
    public long[] equips;

    public Player(IClient client, int ID, String name, int world, float x, float y) {
        this.client = client;
        this.ID = ID;
        this.name = name;
        this.world = world;
        this.x = x;
        this.y = y;
        items = new long[9];

        for (int i = 0; i < items.Length; i++) {
            items[i] = -1;
        }

        equips = new long[4];

        for (int i = 0; i < equips.Length; i++) {
            equips[i] = -1;
        }
    }

    public Player(IClient client, int ID, String name, int world, float x, float y, long[] items, long[] equips) {
        this.client = client;
        this.ID = ID;
        this.name = name;
        this.world = world;
        this.x = x;
        this.y = y;
        this.items = items;
        this.equips = equips;
    }
}