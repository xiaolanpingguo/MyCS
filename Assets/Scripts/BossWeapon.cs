using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossWeapon : MonoBehaviour
{
    public int damageValue = 10;
    private PlayerController PlayerController;
    public float moveSpeed = 5;
    //public Vector3 targetPos;
    private float destoryTime = 10;
    private float timeVal;
    public ParticleSystem destoryEffect;
    public bool canMove;
    private bool canTakeDamage;
    private float damageTimeVal ;
    public float initDamageTime = 2;
    public bool isSmoke;
    public Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        PlayerController = GameObject.FindGameObjectWithTag("Player")
            .GetComponent<PlayerController>();
        if (destoryEffect!=null)
        {
            Game.PoolMgr.InitPool(destoryEffect,10);
        }
        damageTimeVal = initDamageTime;
        //transform.LookAt(targetPos);
    }

    // Update is called once per frame
    void Update()
    {
        //技能型武器
        if (damageTimeVal<=0)
        {
            canTakeDamage = true;
            damageTimeVal = initDamageTime;
        }
        else
        {
            damageTimeVal -= Time.deltaTime;
        }

        //近战武器
        if (!canMove)
        {
            return;
        }
        //远程武器
        transform.Translate(transform.forward*moveSpeed*Time.deltaTime,Space.World);
        if (timeVal>=destoryTime)
        {
            gameObject.SetActive(false);
            //gameObject.transform.SetParent(new Transform());
            timeVal = 0;
        }
        else
        {
            timeVal += Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag=="Player")
        {
            
            if (destoryEffect!=null)
            {
                ParticleSystem particleSystem= Game.PoolMgr.GetInstance<ParticleSystem>(destoryEffect);
                particleSystem.gameObject.SetActive(true);
                particleSystem.transform.position = transform.position+offset;
                particleSystem.time = 0;
                particleSystem.Play();
            }        
            if (canMove)
            {
                //gameObject.transform.SetParent(Game.PoolMgr.transform);
                timeVal = 0;
            }
            if (isSmoke)
            {
                if (canTakeDamage)
                {
                    PlayerController.TakeDamage(damageValue);
                    canTakeDamage = false;
                    damageTimeVal = initDamageTime;
                }
            }
            else
            {
                PlayerController.TakeDamage(damageValue);
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (canTakeDamage)
            {
                PlayerController.TakeDamage(damageValue);
                canTakeDamage = false;
                damageTimeVal = initDamageTime;
            }
        }
       
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            canTakeDamage = true;
            damageTimeVal = initDamageTime;
        }      
    }
}
