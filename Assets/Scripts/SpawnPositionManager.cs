using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPositionManager : MonoBehaviour
{
    public static SpawnPositionManager instance;
    public Transform LobbySpawnPosition;

    public Transform GameSpawnPosition;

    private void Awake()
    {
        instance = this;
    }
}
