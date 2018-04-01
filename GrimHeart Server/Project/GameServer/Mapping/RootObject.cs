using System.Collections.Generic;

public class RootObject {
    public int height {
        get;
        set;
    }

    public List<Layer> layers {
        get;
        set;
    }

    public int nextobjectid {
        get;
        set;
    }

    public string orientation {
        get;
        set;
    }

    public string renderorder {
        get;
        set;
    }

    public int tileheight {
        get;
        set;
    }

    public List<Tileset> tilesets {
        get;
        set;
    }

    public int tilewidth {
        get;
        set;
    }

    public int version {
        get;
        set;
    }

    public int width {
        get;
        set;
    }

    public RootObject() {
    }
}