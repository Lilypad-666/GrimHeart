
public abstract class ItemStats {
    public int itemType = 0;

    public abstract class Consumable : ItemStats {
        public class DarkPotion : Consumable {

            public DarkPotion() {
                itemType = 1;
            }
        }
    }

    public abstract class Weapon : ItemStats {
        public float baseDamage;
        public float baseFireRate;
        public float range;

        public class TLeg : Weapon {

            public TLeg () {
                itemType = 2;
                baseDamage = 10f;
                baseFireRate = 2f;
                range = 1f;
            }
        }
    }
}