using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float Speed = 500f;

    private Rigidbody2D rb;

    [SyncVar]
    private NetworkIdentity owner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        //transform.position = transform.up * Speed * Time.deltaTime;
        rb.AddForce(transform.up * Speed, ForceMode2D.Impulse);
        Invoke(nameof(DestorySelf), 3f);
    }

    [Server]
    private void DestorySelf()
    {
        //gameObject.SetActive(false);
        //CommandDestorySelf();
        NetworkServer.Destroy(this.gameObject);
    }

    public void SetOwner(NetworkConnectionToClient conn)
    {
        owner = conn.identity;
    }

    //[Command]
    //private void CommandDestorySelf()
    //{
    //    //gameObject.SetActive(false);
    //    NetworkServer.Destroy(this.gameObject);
    //    RpcDestorySelf();
    //}

    //[ClientRpc]
    //private void RpcDestorySelf()
    //{NetworkServer.Destroy(this.gameObject);
    //}

    [ServerCallback]    //외부에서 트리거 발동 서버에서 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (collision.gameObject.GetComponent<NetworkConnectionToClient>().identity == owner) return;
        if (collision.CompareTag("Player"))
        {
            if(collision.GetComponent<NetworkIdentity>() == owner) return;
            collision.GetComponent<Player>().Hurt(1);
            DestorySelf();
        }
    }
}
