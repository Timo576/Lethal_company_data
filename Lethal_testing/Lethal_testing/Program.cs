using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lethal_testing
{
    class Program
    {
        public int randomMapSeed;
        public int numberSpawned;

        public void Main()
        {
            randomMapSeed = 012345678;
            numberSpawned = 4;
            System.Random randomSeed = new System.Random(randomMapSeed + 1314 + numberSpawned);
            Vector3 inBoxPredictable = GetRandomNavMeshPositionInBoxPredictable(this.transform.position, navHit: Instance.navHit, randomSeed: randomSeed, layerMask: -5);
            int hiveScrapValue = (double)Vector3.Distance(inBoxPredictable, elevatorTransform.transform.position) >= 40.0 ? randomSeed.Next(50, 150) : randomSeed.Next(40, 100);
        }
    }
}
