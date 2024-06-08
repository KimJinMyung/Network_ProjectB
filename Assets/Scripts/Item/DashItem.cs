using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashItem : Item
{
    protected override void Awake()
    {
        base.Awake();
        Type = ItemType.Dash;
    }

    [ServerCallback]
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().DashRecharge();
            CancelInvoke();
            DestorySelf();
        }
    }
}
