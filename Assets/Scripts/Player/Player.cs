using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Org.BouncyCastle.Crypto.Macs;
using UnityEngine.UIElements;

public class Player : NetworkBehaviour
{
    [SerializeField] private float maxShotDelay = 0.2f;
    [SerializeField] private float curShotDelay;

    [SerializeField] private float Speed = 3f;

    [SerializeField] private Transform _attackPos;
    [SerializeField] private int _HP = 5;

    [SerializeField]
    [SyncVar]
    private int Power;

    private SpriteRenderer _spriteRenderer;
    private Vector2 _mousePos;
    private float _angle;

    private Rigidbody2D _rb;
    private Animator _animator;

    private Vector2 _spawnPos;

    private bool _isHurtAble = true;

    private Color HurtEffectColor = new Color(1f, 1f, 1f, 0.5f);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        _spawnPos = GameManager.Instance.RandomPoint();

        transform.position = _spawnPos;
        curShotDelay = maxShotDelay;
    }

    // Update is called once per frame
    void Update()
    {
        AttackDelay();

        if (!Application.isFocused) return;  
        if(!this.isLocalPlayer) return;

        FollowCamera();
        Move();
        MouseRotation();

        Attack();
    }

    private void AttackDelay()
    {
        curShotDelay = Mathf.Clamp(curShotDelay += Time.deltaTime, 0, maxShotDelay);
    }

    private void Attack()
    {
        if (curShotDelay < maxShotDelay) return;
        if (Input.GetMouseButton(0))
        {
            CommandAttack();
            curShotDelay = 0;
        }
    }

    [Command]
    private void CommandAttack()
    {
        switch (Power)
        {
            case 0:
                GameObject bullet = Instantiate(GameManager.Instance.Bullet[0], _attackPos.position, _attackPos.rotation);
                bullet.GetComponent<Bullet>().SetOwner(this.connectionToClient);
                NetworkServer.Spawn(bullet);
                break;
            case 1:
                GameObject bullet_L = Instantiate(GameManager.Instance.Bullet[0], _attackPos.TransformPoint(_attackPos.localPosition + new Vector3(-0.3f, 0, 0)), _attackPos.rotation);
                GameObject bullet_R = Instantiate(GameManager.Instance.Bullet[0], _attackPos.TransformPoint(_attackPos.localPosition + new Vector3(0.3f, 0, 0)), _attackPos.rotation);
                bullet_L.GetComponent<Bullet>().SetOwner(this.connectionToClient);
                bullet_R.GetComponent<Bullet>().SetOwner(this.connectionToClient);
                NetworkServer.Spawn(bullet_L);
                NetworkServer.Spawn(bullet_R);
                break;
            case 2:
                GameObject Bigbullet = Instantiate(GameManager.Instance.Bullet[1], _attackPos.position, _attackPos.rotation);
                Bigbullet.GetComponent<Bullet>().SetOwner(this.connectionToClient);
                NetworkServer.Spawn(Bigbullet);
                break;
        }
        

        RpcAttack();
    }

    [ClientRpc]
    private void RpcAttack()
    {

    }

    public void PowerUp()
    {
        Power++;
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
        if (!_isHurtAble) return;

        _HP -= (int)damage;

        _isHurtAble = false;

        if (_HP <= 0)
        {
            Dead();
        }
        else
        {
            HurtAnimation();
        }
    }

    [ClientRpc]
    private void HurtAnimation()
    {
        StartCoroutine(HurtEffect());
    }

    IEnumerator HurtEffect()
    {
        int Count = 0;
        while (Count <= 5)
        {
            Count++;
            _spriteRenderer.color = HurtEffectColor;
            yield return new WaitForSeconds(0.3f);
            _spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(0.3f);            
        }

        _isHurtAble = true;
        yield break;
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
