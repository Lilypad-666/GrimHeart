using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Item {
    int type;

    public Item(int type) {
        this.type = type;
    }

    public int getType() {
        return type;
    }

    public void setType(int type) {
        this.type = type;
    }

    public static int spriteToType(Sprite sprite) {
        for (int j = 0; j < NetworkPlayerManager.staticSprites.Length; j++) {
            if (sprite == NetworkPlayerManager.staticSprites[j]) {
                return j;
            }
        }

        return -1;
    }
}