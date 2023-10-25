using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    protected Animator animator;
    protected bool isWaking;
    protected bool isDead;
    public Light pointLight;
    //public Transform targetTrans;
    private bool isAngryState;
    public bool hasSkill;

    public AnimationClip attackAnimationClip;
    public float attackSpeed;
    public GameObject keyGo;
    public float takeDamageTime;
    private float takeDamageTimer;
    public AudioClip deadClip;

    protected override void Start()
    {
        base.Start();
        animator = GetComponentInChildren<Animator>();
        animator.SetFloat("Born",-0.5f);
        currentState = ENEMYSTATE.IDLE;
        //float result = Vector3.Dot(transform.forward, targetTrans.position - transform.position);
        //if (result>0)
        //{
        //    Debug.Log("在前方");
        //}
        //else
        //{
        //    Debug.Log("在后方");
        //}
        //float result = Vector3.Cross(transform.forward,targetTrans.position).y;
        //float result = Vector3.Dot(transform.right,targetTrans.position);
        //if (result<0)
        //{
        //    Debug.Log("在左方");
        //}
        //else
        //{
        //    Debug.Log("在右方");
        //}
        currentMoveSpeed = moveSpeed;

        if (attackAnimationClip != null)
        {
            animator.SetFloat("AttackSpeed", attackSpeed);
            actRestTime = attackAnimationClip.length /attackSpeed;
        }
        else
        {
            animator.SetFloat("AttackSpeed",1);
        }
        takeDamageTimer = takeDamageTime;
    }

    protected override void Update()
    {
        if (isDead)
        {
            transform.Translate(-Vector3.up*Time.deltaTime*0.3f);
            pointLight.intensity -= Time.deltaTime * 2;
            if (transform.position.y<=-10)
            {
                Destroy(gameObject);
            }
            return;
        }
        takeDamageTimer -= Time.deltaTime;
        base.Update();
    }

    protected override void EnemyAct()
    {
        if (isWaking||isDead)
        {
            return;
        }
        base.EnemyAct();
        switch (currentState)
        {
            case ENEMYSTATE.PATROL:
                break;
            case ENEMYSTATE.CHASE:
                //animator.ResetTrigger("Hit");
                animator.ResetTrigger("Attack");
                if (hasSkill)
                {
                    animator.ResetTrigger("UseSkill");
                }
                animator.SetBool("Moving",true);
                if (isAngryState)
                {
                    animator.SetFloat("MoveSpeed",1);
                    currentMoveSpeed=moveSpeed*3;
                }
                break;
            case ENEMYSTATE.ATTACK:
                animator.SetBool("Moving",false);             
                animator.ResetTrigger("Hit");
                if (isAngryState&&hasSkill)
                {
                    animator.SetTrigger("UseSkill");
                }
                else
                {
                    animator.SetTrigger("Attack");
                }
                break;
            case ENEMYSTATE.RETURN:
                animator.SetBool("Moving", true);
                animator.ResetTrigger("Hit");
                break;
            case ENEMYSTATE.IDLE:
                animator.SetBool("Moving", false);
                break;
            case ENEMYSTATE.WARN:
                //animator.ResetTrigger("Hit");
                break;
            case ENEMYSTATE.USESKILL:
                break;
            default:
                break;
        }
    }

    protected void Warn()
    {
        if (isWaking)
        {
            return;
        }
        float wakeValue = animator.GetFloat("Born");
        if (wakeValue<0)
        {
            animator.SetFloat("Born",1);
            isWaking = true;
            animator.Play("Born",0,0);
            Invoke("FinishWaking",9);
        }
        else
        {
            animator.SetTrigger("Roar");
            transform.LookAt(new Vector3(playerTrans.position.x,transform.position.y
                ,playerTrans.position.z));
        }
    }

    private void FinishWaking()
    {
        animator.ResetTrigger("Hit");
        isWaking = false;
    }

    protected override void CheckDistance()
    {
        if (currentState == ENEMYSTATE.RETURN||isWaking||isDead)
        {
            return;
        }
        float distance = Vector3.Distance(playerTrans.position, transform.position);
        if (distance<1.5*chaseRange&&currentState!=ENEMYSTATE.WARN&&
            currentState!=ENEMYSTATE.ATTACK &&
            currentState != ENEMYSTATE.CHASE)
        {
            Warn();
            currentState = ENEMYSTATE.WARN;
        }
        if (distance <= attackRange && enemyFunction.canAttack)
        {
            currentState = ENEMYSTATE.ATTACK;
        }
        else if (distance <= chaseRange && enemyFunction.canChase)
        {
            currentState = ENEMYSTATE.CHASE;
        }
        else if (distance >= safeRange && enemyFunction.canReturn)
        {
         
            if (currentState == ENEMYSTATE.CHASE && enemyFunction.canChase)
            {
                currentState = ENEMYSTATE.RETURN;             
            }
        }

    }

    protected override void ReturnToInitPos()
    {
        if (!CanMove())
        {
            return;
        }
        currentHealth = initHealth;
        if (Vector3.Distance(transform.position,initPos)<=2)
        {
            currentState = ENEMYSTATE.IDLE;
            isAngryState = false;
            transform.eulerAngles = new Vector3(0, 180, 0);
            currentMoveSpeed = moveSpeed;
            chaseRange = initChaseRange;
            safeRange = initSafeRange;
            return;
        }
        transform.Translate(transform.forward * Time.deltaTime * currentMoveSpeed, Space.World);
        transform.LookAt(new Vector3(initPos.x,transform.position.y,initPos.z));
    }

    public void TakeDamage(float damageValue,Vector3 hitPos)
    {
        if (isWaking||isDead)
        {
            return;
        }
        currentHealth -= damageValue;
        base.RecoverAttackRangeValue();
        if (currentState==ENEMYSTATE.IDLE||currentState==ENEMYSTATE.WARN)
        {
            chaseRange = initChaseRange * 2;
            safeRange = initSafeRange * 1.5f;
        }
        if (currentHealth<=initHealth/3&&!isAngryState)
        {
            isAngryState = true;
        }
        if (takeDamageTimer<=0)
        {
            if (soundPlayer != null)
            {
                soundPlayer.PlayRandomSound();
            }
            else
            {
                Game.AudioSourceMgr.PlaySound(hurtClip);
            }
            animator.SetTrigger("Hit");
            animator.SetFloat("HitY", 0);
            animator.SetFloat("HitX", 0);
            float y = Vector3.Dot(transform.forward, hitPos - transform.position);
            float x = Vector3.Dot(transform.right, hitPos);
            if (ForwardBehindOrLeftRight(hitPos))
            {
                if (y > 0)
                {
                    //Debug.Log("在前方");
                    animator.SetFloat("HitY", 1);
                }
                else
                {
                    //Debug.Log("在后方");
                    animator.SetFloat("HitY", -1);
                }
            }
            else
            {
                if (x > 0)
                {
                    //Debug.Log("在右方");
                    animator.SetFloat("HitX", 1);
                }
                else
                {
                    //Debug.Log("在左方");
                    animator.SetFloat("HitX", -1);
                }
            }
            takeDamageTimer = takeDamageTime;
        }
        
        if (currentHealth>0)
        {
            return;
        }
        Game.AudioSourceMgr.PlaySound(deadClip);
        animator.SetTrigger("Die");
        isDead = true;
        keyGo.SetActive(true);
        keyGo.transform.position = transform.position+new Vector3(0,2,0);
        rigid.isKinematic = true;
        rigid.constraints = RigidbodyConstraints.FreezeAll;
    }
    /// <summary>
    /// 判断前后或者左右影响度（true前后优先级更高，false左右优先级更高）
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    private bool ForwardBehindOrLeftRight(Vector3 targetPos)
    {
        float ZDistance =Mathf.Abs(transform.position.z - targetPos.z);
        float XDistance = Mathf.Abs(transform.position.x-targetPos.x);
        if (ZDistance>=XDistance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected override void Attack()
    {
        if (Time.time-lastActTime<attackRange)
        {
            return;
        }
        lastActTime = Time.time;
        transform.LookAt(new Vector3(playerTrans.position.x,transform.position.y,
            playerTrans.position.z));
    }

    protected bool CanMove()
    {
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash !=
            Animator.StringToHash("Attack") &&
            animator.GetCurrentAnimatorStateInfo(0).shortNameHash !=
            Animator.StringToHash("Hit") &&
            animator.GetCurrentAnimatorStateInfo(0).shortNameHash !=
            Animator.StringToHash("Boar") &&
            animator.GetCurrentAnimatorStateInfo(0).shortNameHash !=
            Animator.StringToHash("UseSkill");
    }

    protected override void Chase()
    {
        if (!CanMove())
        {
            return;
        }
        transform.LookAt(new Vector3(playerTrans.position.x, transform.position.y,
         playerTrans.position.z));
        transform.Translate(transform.forward * Time.deltaTime * currentMoveSpeed, Space.World);
    }
}
