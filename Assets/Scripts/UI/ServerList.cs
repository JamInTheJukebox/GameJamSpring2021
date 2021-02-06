using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerList : MonoBehaviour
{
    private List<GameObject> ServerBoxes = new List<GameObject>();

    [SerializeField] GameObject ServerBox;

    [Header("Positioning")]
    [SerializeField] Transform PositionLeft;
    [SerializeField] Transform PositionRight;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 Pos = PositionLeft.position;
            var obj1 = Instantiate(ServerBox, Pos, Quaternion.identity);
            obj1.SetActive(true);
            obj1.transform.SetParent(transform.GetChild(0),false);
            obj1.transform.position = Pos;
        }
    }
}
