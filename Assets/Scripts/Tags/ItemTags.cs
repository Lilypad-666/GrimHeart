class ItemTags {
    public static readonly int None = -1;
    public static readonly int DarkPotion = 1;
    public static readonly int TLeg = 2;

    public static int getBullet(int weaponID) {
        switch (weaponID) {
            case 2:
                return 0;
        }

        return -1;
    }
}