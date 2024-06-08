using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Power : Item
{
    protected override void Awake()
    {
        base.Awake();
        Type = ItemType.Power;
    }

    [ServerCallback]
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().PowerUp();
            CancelInvoke();
            DestorySelf();
        }
    }
}
