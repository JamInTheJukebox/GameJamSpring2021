using UnityEngine;
using Bolt;
public class NetworkCallbacks : GlobalEventListener
{
    // instantiate player when they are loaded.

    public override void SceneLoadLocalDone(string scene)
    {
        var spawnPos = new Vector3(Random.Range(-8, 8), 0, Random.Range(-8, 8));
        BoltNetwork.Instantiate(BoltPrefabs.CustomPlayer, spawnPos, Quaternion.identity);
    }

}
