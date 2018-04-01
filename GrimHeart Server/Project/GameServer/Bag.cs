using DarkRift;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Bag {
    public float x;
    public float y;
    public Item[] items;
    public int id;

    public Bag(float x, float y, Item[] items) {
        this.x = x;
        this.y = y;
        this.items = items;
        id = -1;
    }

    public void setID(int id) {
        this.id = id;
    }

    public void addItem(int slot, Item item) {
        items[slot] = item;
    }

    public Item removeItem(int slot) {
        Item temp = items[slot];
        items[slot] = null;
        return temp;
    }
}
