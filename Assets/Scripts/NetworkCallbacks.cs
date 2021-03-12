using UnityEngine;
using Bolt;
public class NetworkCallbacks : GlobalEventListener
{
    // instantiate player when they are loaded.

    public override void SceneLoadLocalDone(string scene)
    {

        var spawnPos = SpawnPositionManager.instance.LobbySpawnPosition.position;
        spawnPos += new Vector3(Random.Range(-8, 8), 1, Random.Range(-8, 8));
        var entity = BoltNetwork.Instantiate(BoltPrefabs.MasterPlayer, spawnPos, Quaternion.identity);
       // entity.TakeControl();
    }

}
