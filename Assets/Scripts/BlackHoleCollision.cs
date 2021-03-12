using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.name);
        if(other.tag == Tags.GROUND_TAG)
        {
            // play effect here.
            Destroy(other.transform.parent.gameObject);         // destroy the parent
        }
        
        else if(other.tag == Tags.PLAYER_TAG)
        {
            other.GetComponent<Health>().LoseGame();
        }
    }
}
