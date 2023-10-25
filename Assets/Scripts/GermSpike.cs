using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GermSpike : Enemy
{
    public GameObject explosionEffect;
    public int damageValue;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        Game.PoolMgr.InitPool(explosionEffect,1);
    }

    protected override void Attack()
    {
        base.Attack();
        if (Time.time - lastActTime < actRestTime)
        {
            return;
        }
        transform.LookAt(playerTrans);
        lastActTime = Time.time;
        playerTrans.GetComponent<PlayerController>().TakeDamage(damageValue);
        GameObject effect = Game.PoolMgr.GetInstance<GameObject>(explosionEffect);
        effect.transform.position = transform.position;
        effect.transform.localScale = Vector3.one * 3;
        effect.SetActive(true);
    }
}
