using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float initHealth = 10;
    public float currentHealth;
    public ParticleSystem destoryEffect;
    public ENEMYSTATE currentState;
    public float moveSpeed;
    protected float lastActTime;
    public float actRestTime;//行为转换的CD
    private Quaternion targetRotation;
    protected Vector3 initPos;
    public EnemyFunction enemyFunction;
    protected Transform playerTrans;
    public float attackRange;
    public float chaseRange;
    public float safeRange;
    protected Rigidbody rigid;
    protected float currentMoveSpeed;
    protected float initChaseRange;
    protected float initSafeRange;
    protected float initAttackRange;
    public GameObject smokeCollierGo;
    public SoundPlayer soundPlayer;
    public AudioClip hurtClip;
    public AudioClip attackClip;
    private float canTurnTimer;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (destoryEffect)
        {
            Game1.PoolMgr.InitPool(destoryEffect,4);
        }
        currentHealth = initHealth;
        currentState = ENEMYSTATE.PATROL;
        initPos = transform.position;
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        rigid = GetComponent<Rigidbody>();
        initSafeRange = safeRange;
        initChaseRange = chaseRange;
        initAttackRange = attackRange;
        currentMoveSpeed = moveSpeed;
        canTurnTimer = Time.time;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        CheckDistance();
        EnemyAct();
    }

    public virtual void TakeDamage(float damageValue)
    {
        currentHealth -= damageValue;       
        RecoverAttackRangeValue();
        if (soundPlayer!=null)
        {
            soundPlayer.PlayRandomSound();
        }
        else
        {
            Game1.AudioSourceMgr.PlaySound(hurtClip);
        }
        if (currentHealth>0)
        {
            return;
        }
        if (destoryEffect)
        {
            ParticleSystem ps = Game1.PoolMgr.GetInstance<ParticleSystem>(destoryEffect);
            ps.time = 0;
            ps.Play();
            ps.transform.position = transform.position+new Vector3(0,0.5f,0);
        }
        gameObject.SetActive(false);
    }

    protected virtual void EnemyAct()
    {
        switch (currentState)
        {
            case ENEMYSTATE.PATROL:
                Move();
                if (Time.time-lastActTime>actRestTime)
                {
                    lastActTime = Time.time;
                    targetRotation = Quaternion.Euler(GetRandomEuler());
                }
                break;
            case ENEMYSTATE.CHASE:
                Chase();
                break;
            case ENEMYSTATE.ATTACK:
                Attack();
                break;
            case ENEMYSTATE.RETURN:
                ReturnToInitPos();
                break;
            default:
                break;
        }
    }

    private void Move()
    {
        transform.Translate(transform.forward*Time.deltaTime*currentMoveSpeed,Space.World);
        transform.rotation = Quaternion.Slerp(transform.rotation,targetRotation,0.1f);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (currentState==ENEMYSTATE.PATROL&&collision.gameObject.layer!=11
            &&Time.time-canTurnTimer>5)
        {
            canTurnTimer = Time.time;
            targetRotation = Quaternion.LookRotation(-transform.forward,transform.up);
            lastActTime = Time.time;
        }
        else if (currentState==ENEMYSTATE.CHASE||currentState==ENEMYSTATE.RETURN||currentState==ENEMYSTATE.ATTACK)
        {
            rigid.isKinematic = true;
            Invoke("CloseIsKinematicState",1);
        }
    }

    private void CloseIsKinematicState()
    {
        rigid.isKinematic = false;
    }

    protected virtual void ReturnToInitPos()
    {
        transform.Translate(transform.forward*Time.deltaTime*currentMoveSpeed,Space.World);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.1f);
        if (Vector3.Distance(transform.position,initPos)<=1)
        {
            currentState = ENEMYSTATE.PATROL;
        }
    }
    /// <summary>
    /// 检测与某一个对象的距离并转换敌人的状态
    /// </summary>
    protected virtual void CheckDistance()
    {
        if (currentState==ENEMYSTATE.RETURN)
        {
            return;
        }
        float distance = Vector3.Distance(playerTrans.position,transform.position);
        if (distance<=attackRange && enemyFunction.canAttack)
        {
            currentState = ENEMYSTATE.ATTACK;
        }
        else if (distance<=chaseRange && enemyFunction.canChase)
        {
            currentState = ENEMYSTATE.CHASE;
        }
        else if (distance>= safeRange && enemyFunction.canReturn)
        {
            if (currentState==ENEMYSTATE.PATROL&&Vector3.Distance(transform.position, initPos) >= 8)
            {
                currentState = ENEMYSTATE.RETURN;
                targetRotation = Quaternion.LookRotation(initPos - transform.position, transform.up);
            }
            else if (currentState==ENEMYSTATE.CHASE)
            {
                currentState = ENEMYSTATE.RETURN;
                targetRotation = Quaternion.LookRotation(initPos - transform.position, transform.up);
            }
        }
    }

    private Vector3 GetRandomEuler()
    {
        float x = 0, y = 0, z = 0;
        if (enemyFunction.canRotateX)
        {
            x = Random.Range(1, 5) * 90;
        }
        if (enemyFunction.canRotateY)
        {
            y = Random.Range(1,5)*90;
        }
        if (enemyFunction.canRotateZ)
        {
            z = Random.Range(1, 5) * 90;
        }
        return new Vector3(x,y,z);
    }

    protected virtual void Chase()
    {
        transform.LookAt(playerTrans);
        transform.Translate(transform.forward*Time.deltaTime*currentMoveSpeed,Space.World);
    }

    protected virtual void Attack()
    {
        //Debug.Log(gameObject.name+"正在攻击!");
    }

    public void SetRange(float value)
    {
        attackRange = chaseRange = value;
        currentState = ENEMYSTATE.IDLE;
    }

    public void RecoverAttackRangeValue()
    {
        attackRange = initAttackRange;
        chaseRange = initChaseRange;
        if (enemyFunction.canPatrol)
        {
            currentState = ENEMYSTATE.PATROL;
        }
    }
    /// <summary>
    /// 用于烟雾弹检测
    /// </summary>
    /// <param name="state"></param>
    public void SetSmokeCollierState(bool state)
    {
        smokeCollierGo.SetActive(state);
      
    }
}

public enum ENEMYSTATE
{ 
    PATROL,
    CHASE,
    ATTACK,
    RETURN,
    //BOSS
    IDLE,
    WARN,//警戒(播放动画，看向玩家)
    USESKILL
}
[System.Serializable]
public struct EnemyFunction
{
    public bool canPatrol;
    public bool canChase;
    public bool canAttack;
    public bool canReturn;
    public bool canRotateX;
    public bool canRotateY;
    public bool canRotateZ;
}
