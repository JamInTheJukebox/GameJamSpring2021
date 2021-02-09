using UnityEngine;

public class EventManager : Bolt.GlobalEventListener
{
    public override void OnEvent(PlayerJoinedEvent evnt)
    {
        Debug.LogWarning(evnt.Message);
    }
}
