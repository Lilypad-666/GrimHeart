using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Bullet {

    int id;
    float fangle;
    float x;
    float y;

    public Bullet(float fangle, float x, float y) {
        id = -1;
        this.fangle = fangle;
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
