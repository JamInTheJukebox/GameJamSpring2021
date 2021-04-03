using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleCollision : MonoBehaviour
{
    public bool LobbyCollider;      // If this is a lobby collider, just teleport the player to the lobby. Otherwise, make them lose!
    private void OnTriggerEnter(Collider other)
    {

        if(other.tag == Tags.GROUND_TAG && !LobbyCollider)
        {
            // play effect here.
            Destroy(other.transform.parent.gameObject);         // destroy the parent
        }
        
        else if(other.tag == Tags.PLAYER_TAG)
        {
            if(!LobbyCollider)
                other.GetComponent<Health>().LoseGame();
            else
            {
                other.GetComponent<Bolt_PlayerController>().Teleport(SpawnPositionManager.instance.LobbySpawnPosition.position);
            }
        }
    }
}
