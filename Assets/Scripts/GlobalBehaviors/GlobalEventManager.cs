using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class GlobalEventManager : Bolt.GlobalEventListener
{
    List<Material> PlayerMaterials = new List<Material>();


}
