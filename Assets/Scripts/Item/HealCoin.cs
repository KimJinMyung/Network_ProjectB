using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealCoin : Item
{
    protected override void Awake()
    {
        base.Awake();
        Type = ItemType.Heal;
    }

    [ServerCallback]
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().Healing();
            CancelInvoke();
            DestorySelf();
        }
    }
}
