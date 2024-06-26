using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Org.BouncyCastle.Crypto.Macs;

public class Player : NetworkBehaviour
{
    [SerializeField] private float Speed = 3f;

    [SerializeField] private Transform _attackPos;
    [SerializeField] private GameObject _Bullet;
    [SerializeField] private int _HP = 5;
    private Vector2 _mousePos;
    private float _angle;

    private Rigidbody2D _rb;
    private Animator _animator;

    private Vector2 _spawnPos;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _spawnPos = GameManager.Instance.RandomPoint();

        transform.position = _spawnPos;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Application.isFocused) return;  
        if(!this.isLocalPlayer) return;

        FollowCamera();
        Move();
        MouseRotation();

        Attack();
    }

    private void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CommandAttack();
        }
    }

    [Command]
    private void CommandAttack()
    {
        //오브젝트 풀링 예정
        GameObject bullet = Instantiate(_Bullet, _attackPos.position, _attackPos.rotation);
        bullet.GetComponent<Bullet>().SetOwner(this.connectionToClient);
        NetworkServer.Spawn(bullet);

        RpcAttack();
    }

    [ClientRpc]
    private void RpcAttack()
    {

    }


    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        //Vector2 movePos = new Vector2(h, v) * Speed * Time.deltaTime;

        _rb.velocity = ((transform.up * v + transform.right * h) * Speed);

        if(Input.GetButtonDown("Horizontal") || Input.GetButtonUp("Horizontal"))
            _animator.SetInteger("Input", (int)h);
    }

    private void MouseRotation()
    {
        _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _angle = Mathf.Atan2(_mousePos.y - transform.position.y, _mousePos.x - transform.position.x) * Mathf.Rad2Deg;

        if (!this.isLocalPlayer) return;
        transform.rotation = Quaternion.AngleAxis(_angle - 90, Vector3.forward);
    }

    private void FollowCamera()
    {
        Camera.main.transform.position = transform.position + new Vector3(0,0, -15f);
    }

    [ServerCallback]
    public void Hurt(float damage)
    {
        _HP -= (int)damage;
        if(_HP <= 0)
        {
            Dead();
        }
    }

    public void Dead()
    {
        CommandDead();
    }

    [Command]
    private void CommandDead()
    {
        NetworkServer.Destroy(this.gameObject);
        RpcDead();
    }

    [ClientRpc]
    private void RpcDead()
    {
        _animator.SetTrigger("Die");
    }
}
