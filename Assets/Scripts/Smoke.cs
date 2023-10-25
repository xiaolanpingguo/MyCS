using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    public ParticleSystem m_particleSystem;
    private List<GameObject> enemyList=new List<GameObject>();
    private void OnEnable()
    {
        m_particleSystem.time = 0;
        m_particleSystem.Play();
        Invoke("HideSelf",25);
    }

    private void HideSelf()
    {
        m_particleSystem.Stop();
        Invoke("HideGameObject",7);
    }

    private void HideGameObject()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        CancelInvoke();
        for (int i = 0; i < enemyList.Count; i++)
        {
            if (enemyList[i])
            {
                Enemy enemy = enemyList[i].GetComponentInParent<Enemy>();
                if (enemy)
                {
                    enemy.RecoverAttackRangeValue();
                    enemy.SetSmokeCollierState(true);
                }
            }                                  
        }
        enemyList.Clear();
        StopAllCoroutines();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            if (enemyList.Contains(other.gameObject))
            {
                enemyList.Remove(other.gameObject);
            }

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag=="Enemy")
        {
            if (!enemyList.Contains(other.gameObject))
            {
                enemyList.Add(other.gameObject);
                StartCoroutine(OutSmoke(other));
            }
           
        }
    }

    IEnumerator OutSmoke(Collider other)
    {
        //进入迷失状态
        yield return new WaitForSeconds(2);
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy.enabled)
        {
            enemy.SetRange(2);
            enemy.SetSmokeCollierState(false);
        }
        
      
        //解除迷失状态
        yield return new WaitForSeconds(15);
        if (enemy.enabled)
        {
            enemy.SetSmokeCollierState(true);
        }
        
    }
}
