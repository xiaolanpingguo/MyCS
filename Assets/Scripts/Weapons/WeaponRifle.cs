//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class WeaponRifle : Weapon
//{
//    public Transform ShootPoint;
//    public Transform BulletShootPoint;
//    public Transform CasingBulletSpawnPoint;

//    public float Range = 300f;
//    public float FireRate;

//    public float OriginRate;
//    public float SpreadFactor;
//    public float FireTimer;
//    public float BulletForce = 100f;

//    void Start()
//    {
        
//    }

//    void Update()
//    {
//        Fire();
//    }

//    public override void Fire()
//    {
//        // view center coo: 0.5 0.5
//        Ray ray = Camera.main.ViewportPointToRay(Vector3.one * 0.5f);
//        RaycastHit hit;
//        if (!Physics.Raycast(ray, out hit, Range, ~(1 << 8), QueryTriggerInteraction.Ignore))
//        {
//            return;
//        }

//        Debug.Log("adwadwa");
//    }

//    public override void Reload()
//    {

//    }

//    public override void AimIn()
//    {

//    }

//    public override void AimOut()
//    {

//    }

//    public override void ExpaningCrossUpdate()
//    {

//    }

//    public override void DoReloadAnimation()
//    {

//    }
//}
