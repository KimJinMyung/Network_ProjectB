using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class Player : NetworkBehaviour
{
    [SerializeField] private float maxShotDelay = 0.2f;
    [SerializeField] private float curShotDelay;

    [SerializeField]
    //[SyncVar]
    private float Speed = 2.5f;
    //[SyncVar]
    private float defaultSpeed;

    [SerializeField] private Transform _attackPos;

    [SerializeField]
    [SyncVar]
    private int _HP = 5;

    [SerializeField]
    [SyncVar]
    private int Power;

    [ShowInInspector]
    [SyncVar]
    private int _DashCount;

    private SpriteRenderer _spriteRenderer;
    private Vector2 _mousePos;
    private float _angle;

    private Rigidbody2D _rb;
    private Animator _animator;

    private Vector2 _spawnPos;

    [SyncVar]
    private bool _isHurtAble = true;
    [SyncVar]
    private bool _isDead;

    private bool _isDashing;



    private Color HurtEffectColor = new Color(1f, 1f, 1f, 0.5f);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        defaultSpeed = Speed;
    }

    private void OnEnable()
    {
        _spawnPos = GameManager.Instance.RandomPoint();

        transform.position = _spawnPos;
        curShotDelay = maxShotDelay;

        _isDead = false;

        _DashCount = 4;
        GameManager.Instance.GetUI.Changed_DashCount(_DashCount);

        Speed = defaultSpeed;

        _HP = 5;
        GameManager.Instance.GetUI.Changed_PlayerHP(_HP);
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

        Dash();
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

    //[Command]
    private void Dash()
    {
        if (_isDashing) return;
        if(_DashCount <=  0) return;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _isDashing = true;
            _DashCount--;
            GameManager.Instance.GetUI.Changed_DashCount(_DashCount);

            CommandDash();
        }
    }

    [Command]
    private void CommandDash()
    {
        defaultSpeed = Speed;
        RpcDash();
    }

    [ClientRpc]
    private void RpcDash()
    {
        //대쉬 애니메이션

        //애니메이션이 없는 관계로 Invoke로 임시
        _isHurtAble = false;
        Speed = 10f;
        Invoke(nameof(DashEnd), 0.5f);
    }

    public void DashEnd()
    {
        Speed = defaultSpeed;
        _isHurtAble = true;
        _isDashing = false;
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

    public void Healing()
    {
        _HP++;
        GameManager.Instance.GetUI.Changed_PlayerHP(_HP);
    }

    public void DashRecharge()
    {
        _DashCount++;
        GameManager.Instance.GetUI.Changed_DashCount(_DashCount);
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

    [Server]
    public void Hurt(float damage)
    {
        if (!_isHurtAble) return;        
        if(_isDead) return;

        _HP -= (int)damage;
        Debug.LogWarning(_HP);

        //_isHurtAble = false;

        if (_HP <= 0)
        {
            Dead();
        }
        else
        {
            HurtAnimation(_HP);
        }
    }

    [Command]
    private void CommandHurt(float damage)
    {
        Hurt(damage);
    }

    [ClientRpc]
    private void HurtAnimation(int curHP)
    {
        //if(isLocalPlayer)
        //GameManager.Instance.GetUI.Changed_PlayerHP(curHP);

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

        //_isHurtAble = true;
        yield break;
    }

    public void Dead()
    {
        _isDead = true;
        CommandDead();      
    }

    [Command]
    private void CommandDead()
    {
        RpcDead();
    }

    [ClientRpc]
    private void RpcDead()
    {
        if (isLocalPlayer)
            GameManager.Instance.GetUI.Changed_PlayerHP(_HP);

        //터지는 애니메이션 실행
        _animator.SetTrigger("Die");
    }

    public void EndDieAnimation()
    {
        if (isServer)
        {
            ServerEndDieAnimation();
        }
        else
        {
            CmdEndDieAnimation();
        }
    }

    [Command]
    private void CmdEndDieAnimation()
    {
        ServerEndDieAnimation();
    }

    [Server]
    private void ServerEndDieAnimation()
    {
        NetworkServer.Destroy(this.gameObject);

        if (isLocalPlayer)
        {
             connectionToClient.Disconnect();
        }
    }
}
