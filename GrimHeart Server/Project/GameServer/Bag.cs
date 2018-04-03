using DarkRift;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Bag {
    public float x;
    public float y;
    public long[] items;
    public int id;

    public Bag(float x, float y, long[] items) {
        this.x = x;
        this.y = y;
        this.items = items;
        id = -1;
    }

    public void addItem(int slot, int itemid) {
        items[slot] = itemid;
    }

    public long removeItem(int slot) {
        long temp = items[slot];
        items[slot] = 0;
        return temp;
    }
}
