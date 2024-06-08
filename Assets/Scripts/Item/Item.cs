using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum ItemType
{
    Power,
    Heal,
}

public class Item : NetworkBehaviour
{
    [SerializeField]
    protected ItemType Type;
    [SerializeField]
    protected float Speed = 1f;

    protected Rigidbody2D _rb;


    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void OnEnable()
    {
        _rb.velocity = transform.up * Speed;
        Invoke(nameof(DestorySelf), 30f);
    }

    [Server]
    protected virtual void DestorySelf()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    [ServerCallback]
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            Vector2 normal = (collision.bounds.center - transform.position).normalized;

            Vector2 incomingVelocity = _rb.velocity;
            Vector2 reflection = Vector2.Reflect(incomingVelocity, normal);

            float randomAngle = Random.Range(-30f, 30f);
            reflection = Quaternion.Euler(0,0,randomAngle) * reflection;

            ReflectItem(reflection);
        }


        //if (collision.CompareTag("Player"))
        //{
        //    collision.GetComponent<Player>().PowerUp();
        //    CancelInvoke();
        //    DestorySelf();
        //}
    }

    [ClientRpc]
    private void ReflectItem(Vector2 dir)
    {
        _rb.velocity = dir.normalized * Speed;
        Debug.Log("Æ¨±â´Â Áß...");
    }

}
