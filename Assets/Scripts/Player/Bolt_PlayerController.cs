using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bolt_PlayerController : MonoBehaviour
{
    public float speed = 3f;
    // Update is called once per frame
    void Update()
    {
        PlayerMove();
    }

    void PlayerMove()
    {
        float hor = Input.GetAxis(Axis.HORIZONTAL);
        float vert = Input.GetAxis(Axis.VERTICAL);
        Vector3 playerMovement = new Vector3(hor, 0f, vert).normalized * speed * Time.deltaTime;
        transform.Translate(playerMovement,Space.Self);

    }
}
