using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Enemy {

    public int id;
    public float x;
    public float y;

    public Enemy(float x, float y) {
        this.x = x;
        this.y = y;
    }

    public void setID(int id) {
        this.id = id;
    }

    public int getID() {
        return id;
    }
}
