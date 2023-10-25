using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody rigid;
    public ParticleSystem explosionEffect;
    public float destoryTime;
    private float destoryTimer;
    public bool destroyedOnHit;
    public float explosionRadius;//爆炸半径
    private Collider[] sphereCastPool;//检测到的爆炸范围内的敌人
    public float damageValue;
    public ParticleSystem bulletTrailEffect;
    public ParticleSystem bulletSizeParticle;//如果子弹是粒子且需要变化大小
    public AudioClip hitWallClip;
    public AudioClip explosionClip;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        destoryTime = 3;
        Game.PoolMgr.InitPool(explosionEffect,4);
        sphereCastPool = new Collider[10];
        if (bulletTrailEffect!=null)
        {
            bulletTrailEffect.time = 0;
            bulletTrailEffect.Play();
        }
    
    }

    private void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        destoryTimer += Time.deltaTime;
        if (destoryTimer>=destoryTime)
        {
            if (destroyedOnHit)
            {
                DestoryProjectile();
            }
            else
            {
                DestoryExplosionProjectile();
            }
        }
    }
    /// <summary>
    /// 子弹发射
    /// </summary>
    /// <param name="launcher">发射器</param>
    /// <param name="direction">发射方向</param>
    /// <param name="force">发射力的大小</param>
    public void Launch(Weapon launcher,Vector3 direction,float force)
    {
        transform.position = launcher.GetShootPoint().position;
        transform.forward = launcher.GetShootPoint().forward;
        rigid.AddForce(direction*force);
    }

    private void DestoryExplosionProjectile()
    {
        if (explosionEffect!=null)
        {
            ParticleSystem effect = Game.PoolMgr.GetInstance<ParticleSystem>(explosionEffect);
            effect.transform.position = transform.position;
            effect.gameObject.SetActive(true);
            effect.time = 0;
            effect.Play();
        }      
        gameObject.SetActive(false);
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        destoryTimer = 0;
        if (damageValue>0)
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, sphereCastPool, 1 << 9);
            for (int i = 0; i < count; i++)
            {
                sphereCastPool[i].GetComponent<Enemy>().TakeDamage(damageValue);
            }
            int bossCount = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, sphereCastPool, 1 << 10);
            if (bossCount > 0)
            {
                sphereCastPool[0].GetComponentInParent<Boss>().TakeDamage(damageValue, transform.position);
            }
        }
        
        Game.AudioSourceMgr.PlaySound(explosionClip);
    }

    private void DestoryProjectile(GameObject enemyGo=null)
    {
        if (explosionEffect != null)
        {
            ParticleSystem effect = Game.PoolMgr.GetInstance<ParticleSystem>(explosionEffect);
            effect.transform.position = transform.position;
            effect.gameObject.SetActive(true);
            effect.time = 0;
            effect.Play();
        }
        gameObject.SetActive(false);
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        destoryTimer = 0;
        TakeDamage(enemyGo);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (destroyedOnHit)
        {
            DestoryProjectile(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (destroyedOnHit)
        {
            DestoryProjectile(other.gameObject);
        }
    }
    /// <summary>
    /// 设置聚能枪子弹大小
    /// </summary>
    public void SetBulletSize(float size)
    {
        var main = bulletSizeParticle.main;
        main.startSize = size;
    }
    public void SetBulletDamageValue(float value)
    {
        damageValue = value;
    }

    private void TakeDamage(GameObject enemyGo=null)
    {
        if (enemyGo!=null)
        {
            if (enemyGo.layer==9)
            {
                enemyGo.GetComponent<Enemy>().TakeDamage(damageValue);
            }
            else if (enemyGo.layer==10)
            {
                enemyGo.GetComponentInParent<Boss>().TakeDamage(damageValue,transform.position);
            }
            else
            {
                Game.AudioSourceMgr.PlaySound(hitWallClip);
            }
        }
    }
}
