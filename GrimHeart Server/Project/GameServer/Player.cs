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
    public Item[] items;

    public Player(IClient client, int ID, String name, int world, float x, float y) {
        this.client = client;
        this.ID = ID;
        this.name = name;
        this.world = world;
        this.x = x;
        this.y = y;
        items = new Item[9];

        for (int i = 0; i < items.Length; i++) {
            items[i] = new Item(ItemTags.None);
        }
    }

    public Player(IClient client, int ID, String name, int world, float x, float y, Item[] items) {
        this.client = client;
        this.ID = ID;
        this.name = name;
        this.world = world;
        this.x = x;
        this.y = y;
        this.items = items;
    }
}