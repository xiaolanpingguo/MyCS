using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public float rotateSpeed;
    public AudioClip pickupClip;
    //public Door door;

    // Start is called before the first frame update
    void Start()
    {
        rotateSpeed = 50;
    }

    void Update()
    {
        transform.eulerAngles += new Vector3(0, rotateSpeed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag=="Player")
        {
            //door.OpenDoor();
            Game1.AudioSourceMgr.PlaySound(pickupClip);
            Destroy(gameObject);
        }
    }
}
