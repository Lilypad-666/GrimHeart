using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserAccount : MonoBehaviour {

    public static string username;
    public static int id;
    public static int currentWeapon;
    public static GameObject weaponBullet;

	void Start () {
        username = null;
        id = -1;
        currentWeapon = -1;
        weaponBullet = null;
	}
}
