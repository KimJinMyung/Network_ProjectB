using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPower1 : Bullet
{
    private Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }

    protected override void SetATKPower()
    {
        ATKPower = 3;
    }
}
