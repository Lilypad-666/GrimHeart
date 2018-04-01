using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Heartbeat {
    public static void startHeartbeat() {
        new Timer((object e) => {

            foreach (World world in GrimHeart.worlds.Values) {
                world.tick();
            }

            //20 times per second
        }, null, 0, 50);
    }
}