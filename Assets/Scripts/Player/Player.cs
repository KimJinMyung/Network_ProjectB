using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SerializeField] private float Speed = 3f;

    private Vector2 _mousePos;
    private float _angle;

    private Rigidbody2D _rb;
    private Animator _animator;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!this.isLocalPlayer) return;

        Move();
        MouseRotation();
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        //Vector2 movePos = new Vector2(h, v) * Speed * Time.deltaTime;

        _rb.velocity = ((Vector2.up * v + Vector2.right * h) * Speed);

        if(Input.GetButtonDown("Horizontal") || Input.GetButtonUp("Horizontal"))
            _animator.SetInteger("Input", (int)h);
    }

    private void MouseRotation()
    {
        _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _angle = Mathf.Atan2(_mousePos.y - transform.position.y, _mousePos.x - transform.position.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(_angle - 90, Vector3.forward);
    }
}
