using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour{
    public long id;
    public int type;
    public float minDamage;
    public float maxDamage;
    public float range;
    public float fireRate;
    public int rarity;

    public Item(long id, int type, float minDamage, float maxDamage, float range, float fireRate, int rarity) {
        this.id = id;
        this.type = type;
        this.minDamage = minDamage;
        this.maxDamage = maxDamage;
        this.range = range;
        this.fireRate = fireRate;
        this.rarity = rarity;
    }
}