using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    public float rotateSpeed;
    public int itemID=-1;
    public AudioClip pickupClip;

    // Start is called before the first frame update
    void Start()
    {
        rotateSpeed = 50;
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += new Vector3(0,rotateSpeed*Time.deltaTime,0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name=="Doctor")
        {
            PlayerController PlayerController = other.GetComponent<PlayerController>();
            PlayerController.PickUpWeapon(itemID);
            //PlayerController.ChangeCurrentWeapon(true);
            Game.AudioSourceMgr.PlaySound(pickupClip);
            Destroy(gameObject);
        }
    }
}
