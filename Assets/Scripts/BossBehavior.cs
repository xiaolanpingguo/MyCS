using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBehavior : MonoBehaviour
{
    public GameObject bossWeaponViewGo;//boss武器的显示
    private Transform playerTrans;
    public GameObject skillWeapon;//boss技能的武器（具体与玩家交互的各种技能游戏物体）
    public Transform attackTrans;
    public AudioClip skillClip;

    // Start is called before the first frame update
    void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        if (skillWeapon!=null)
        {
            Game.PoolMgr.InitPool(skillWeapon,10);
        }
    }

    private void ShowWeapon()
    {
        bossWeaponViewGo.SetActive(true);
    }
    private void HideWeapon()
    {
        bossWeaponViewGo.SetActive(false);
    }
    private void CreateSkillBall()
    {
        bossWeaponViewGo.SetActive(false);
        GameObject go= Game.PoolMgr.GetInstance<GameObject>(skillWeapon);
        go.SetActive(true);
        go.transform.SetParent(null);
        go.transform.position = attackTrans.position;
        go.transform.LookAt(playerTrans.position);
        transform.parent.LookAt(new Vector3(playerTrans.position.x, transform.position.y
                , playerTrans.position.z));
        Game.AudioSourceMgr.PlaySound(skillClip);
    }
}
