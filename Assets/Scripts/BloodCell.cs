using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodCell : Enemy
{
    public int addValue;
    public AudioClip audioClip;

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if (collision.transform.tag=="Player")
        {
            Game.AudioSourceMgr.PlaySound(audioClip);
            playerTrans.GetComponent<PlayerController>().TakeDamage(-addValue);
            gameObject.SetActive(false);
        }
    }

    public override void TakeDamage(float damageValue)
    {
        base.TakeDamage(damageValue);
        if (currentHealth<=0)
        {
            Game.AudioSourceMgr.PlaySound(audioClip);
            playerTrans.GetComponent<PlayerController>().TakeDamage(-addValue);
        }
    }
}
