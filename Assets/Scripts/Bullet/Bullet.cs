using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float maxShotDelay = 1.0f;
    [SerializeField] private float curShotDelay = 0;
    [SerializeField] private float Speed = 500f;
    [SerializeField] 
    protected int ATKPower;

    public int GetATKPowerValue {  get { return ATKPower; } }

    protected Rigidbody2D rb;

    [SyncVar]
    protected NetworkIdentity owner;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        SetATKPower();
        //transform.position = transform.up * Speed * Time.deltaTime;
        rb.AddForce(transform.up * Speed, ForceMode2D.Impulse);
        Invoke(nameof(DestorySelf), 3f);
    }

    public void SetOwner(NetworkConnectionToClient conn)
    {
        owner = conn.identity;
    }

    protected virtual void SetATKPower()
    {
        ATKPower = 1;
    }

    [Server]
    protected virtual void DestorySelf()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    [ServerCallback]    //외부에서 트리거 발동 서버에서 처리
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("총알");
        //if (collision.gameObject.GetComponent<NetworkConnectionToClient>().identity == owner) return;
        if (collision.CompareTag("Item")) return;

        if (collision.CompareTag("Player"))
        {
            if(collision.GetComponent<NetworkIdentity>() == owner) return;
            collision.GetComponent<Player>().Hurt(ATKPower);

            DestorySelf();
        }
    }
}
