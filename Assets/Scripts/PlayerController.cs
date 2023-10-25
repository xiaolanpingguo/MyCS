using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    private CharacterController m_characterController;
    private PlayerInputActions m_playerInputActions;

    private CollisionFlags m_collisionFlags;

    // Move
    public float MoveSpeed = 5f;
    public float RunningSpeedMultiple = 2f;
    public float ActualSpeed;

    // camera
    public float MouseSensitivity = 2.4f;
    private float m_angleY;
    private float m_angleX;
    private Transform m_cameraTrans;

    // jump
    public float InitJumpSpeed = 5f;
    private bool m_isGrounded = true;
    private float m_jumpSpeed = 0f;

    // crouch
    public float CrouchHeight = 1f;
    private bool m_isCrouching = false;
    private Vector3 m_defaultCrouchCenter;
    private float m_defaultCrouchHeight;

    public int currentWeaponID;
    private Dictionary<int,Weapon> weaponsDict;
    private Transform weaponPlaceTrans;
    private Dictionary<int, int> ammoInventory;//玩家的武器库（背包，当前玩家某个武器以及其剩余的子弹数量）
    public int currentHP;
    public int initHP;
    public float decreaseSpeed;
    public CameraShaker cameraShaker;
    public bool dead;
    public Transform deadPostionTrans;
    public AudioClip jumpClip;
    public AudioClip landClip;
    private bool canPlayLandClip;
    public AudioClip deadClip;
    public AudioClip hurtClip;
    public Weapon[] m_weapons;

    private void Awake()
    {
        m_playerInputActions = new PlayerInputActions();
    }

    void Start()
    {
        m_playerInputActions.Enable();
        m_playerInputActions.Gameplay.Enable();

        m_characterController = GetComponent<CharacterController>();

        m_angleY = transform.eulerAngles.y;
        m_cameraTrans = Camera.main.transform;
        m_angleX = m_cameraTrans.eulerAngles.x;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        currentWeaponID = -1;
        weaponPlaceTrans = m_cameraTrans.Find("WeaponPlace");
        m_weapons = new Weapon[]
        {
            weaponPlaceTrans.GetChild(0).GetComponent<Weapon>(),
            //weaponPlaceTrans.GetChild(1).GetComponent<Weapon>(),
            //weaponPlaceTrans.GetChild(2).GetComponent<Weapon>(),
        };
        weaponsDict = new Dictionary<int, Weapon>();
        ammoInventory = new Dictionary<int, int>();
        currentHP = initHP;
        //Game.UIMgr.ShowOrHideWeaponUIView(false);
        for (int i = 0; i < m_weapons.Length; i++)
        {
            m_weapons[i].gameObject.SetActive(true);
            m_weapons[i].PickUp(this);
        }
    }

    private void OnEnable()
    {
        m_playerInputActions.Enable();
        m_playerInputActions.Gameplay.Enable();
    }

    private void OnDisable()
    {
        m_playerInputActions.Disable();
        m_playerInputActions.Gameplay.Disable();
    }

    void Update()
    {
        if (dead)
        {
            return;
        }
        Move();
        TurnAndLook();
        Jump();
        Crouch();
        ChangeCurrentWeapon();
    }

    private void Move()
    {
        Vector2 inputMove = Vector2.ClampMagnitude(m_playerInputActions.Gameplay.Move.ReadValue<Vector2>(), 1f);
        ActualSpeed = MoveSpeed;
        bool isRunning = m_playerInputActions.Gameplay.Run.IsPressed();
        if (isRunning)
        {
            ActualSpeed *= RunningSpeedMultiple;
        }

        Vector3 move = new Vector3(inputMove.x, 0, inputMove.y);
        move.Normalize();
        move = move * Time.deltaTime * ActualSpeed;
        move = transform.TransformDirection(move);
        m_characterController.Move(move);
        if (inputMove.x <= 0.1f && inputMove.y <= 0.1f)
        {
            ActualSpeed = 0;
        }
    }

    private void TurnAndLook()
    {
        Vector2 inputLook = m_playerInputActions.Gameplay.Look.ReadValue<Vector2>() * 2f;

        m_angleY = m_angleY + inputLook.x * MouseSensitivity;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, m_angleY, transform.eulerAngles.z);

        float lookAngle = -inputLook.y * MouseSensitivity;
        m_angleX = Mathf.Clamp(m_angleX + lookAngle, -90f, 90f);
        m_cameraTrans.eulerAngles = new Vector3(m_angleX, m_cameraTrans.eulerAngles.y, m_cameraTrans.eulerAngles.z);
    }

    private void Jump()
    {
        bool isJumpPressed = m_playerInputActions.Gameplay.Jump.IsPressed();
        if (isJumpPressed && m_isGrounded)
        {
            m_isGrounded = false;
            m_jumpSpeed = InitJumpSpeed;
            Game.AudioSourceMgr.PlaySound(jumpClip, 0.8f, 1.1f);
            canPlayLandClip = true;
        }

        // in air
        if (!m_isGrounded)
        {
            m_jumpSpeed = m_jumpSpeed - 9.8f * Time.deltaTime;// 9.8: gravity
            Vector3 jump = new Vector3(0, m_jumpSpeed * Time.deltaTime,0);
            m_collisionFlags= m_characterController.Move(jump);
            if (m_collisionFlags == CollisionFlags.Below)
            {
                m_jumpSpeed = 0;
                m_isGrounded = true;
            }
        }
        if (m_isGrounded && m_collisionFlags == CollisionFlags.None)
        {
            if (canPlayLandClip)
            {
                canPlayLandClip = false;
                Game.AudioSourceMgr.PlaySound(landClip, 0.8f, 1.1f);
            }

            m_isGrounded = false;
        }
    }

    private void Crouch()
    {
        bool isCrouchPressed = m_playerInputActions.Gameplay.Crouch.IsPressed() || m_playerInputActions.Gameplay.Crouch.triggered;
        if (!isCrouchPressed)
        {
            m_isCrouching = false;
            m_characterController.height = m_defaultCrouchHeight;
            m_characterController.center = m_defaultCrouchCenter;
            return;
        }

        if (m_isCrouching)
        {
            return;
        }

        Vector3 oldCenter = m_characterController.center;
        float oldHeight = m_characterController.height;
        float centerDelta = (oldHeight - CrouchHeight) / 2f;
        m_characterController.height = CrouchHeight;
        m_characterController.center = new Vector3(oldCenter.x, oldCenter.y - centerDelta, oldCenter.z);

        m_isCrouching = true;
    }

    private void ChangeWeapon(int id)
    {
        if (weaponsDict.Count == 0)
        {
            return; 
        }
        ////处理索引的上下边界
        //if (id >= weaponsDict.Count)
        //{
        //    id = 0;
        //}
        //else if (id <= -1)
        //{
        //    id = weaponsDict.Count - 1;
        //}
        if (id>weaponsDict.Keys.Max())
        {
            id = weaponsDict.Keys.Min();
        }
        else if(id<weaponsDict.Keys.Min())
        {
            id = weaponsDict.Keys.Max();
        }
        if (id == currentWeaponID)//只有一种武器时不切换,否则会出现颠簸颤抖的情况
        {
            return;
        }
        while (!weaponsDict.ContainsKey(id))
        {
            if (id>currentWeaponID)
            {
                id++;
            }
            else
            {
                id--;
            }
        }
        //隐藏上一把武器
        if (currentWeaponID!=-1)//排除第一次没有武器的情况
        {
            weaponsDict[currentWeaponID].PutAway();
        }
        //显示当前武器
        weaponsDict[id].Selected();
        currentWeaponID = id;
    }

    public void ChangeCurrentWeapon(bool autoChange=false)
    {
        if (autoChange)
        {
            //切换到最新拿到的一把
            //ChangeWeapon(weaponsDict.Count-1);
            ChangeWeapon(weaponsDict.Keys.Last());
        }
        else
        {
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                ChangeWeapon(currentWeaponID + 1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                ChangeWeapon(currentWeaponID - 1);
            }
            for (int i = 0; i < 10; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    int num;
                    if (i == 0)
                    {
                        num = 10;
                    }
                    else
                    {
                        num = i - 1;
                    }
                    if (weaponsDict.ContainsKey(num))
                    {
                        ChangeWeapon(num);
                    }
                }
            }
        }
        
    }

    public void PickUpWeapon(int weaponID)
    {
        if (weaponsDict.ContainsKey(weaponID))
        {
            //补充弹药
            Weapon weapon = weaponsDict[weaponID];
            ammoInventory[weapon.GetID()] = weapon.GetInitAmount();
            weapon.clipContent = weapon.clipSize;
            if (currentWeaponID==weaponID)
            {
                Game.UIMgr.UpdateBulletNum(weapon.clipSize, weapon.GetInitAmount());
            }
        }
        else//当前这种名称的武器列表里没有
        {
            //GameObject weaponGo= Instantiate(Resources.Load<GameObject>("Prefabs/Weapons/"+weaponID.ToString()));
            //weaponGo.transform.SetParent(weaponPlaceTrans);
            //weaponGo.transform.localPosition = Vector3.zero;
            //weaponGo.transform.localRotation = Quaternion.identity;
            //weaponGo.gameObject.SetActive(false);
            //Weapon weapon = weaponGo.GetComponent<Weapon>();
            //weapon.PickUp(this);
            m_weapons[weaponID].clipContent = m_weapons[weaponID].clipSize;
            weaponsDict.Add(weaponID, m_weapons[weaponID]);
            ammoInventory.Add(weaponID, m_weapons[weaponID].GetInitAmount());
            ChangeWeapon(weaponID);
        }
    }

    public int GetAmmoAmount(int id)
    {
        int value = 0;
        ammoInventory.TryGetValue(id,out value);
        return value;
    }

    public void UpdateAmmoAmount(int id,int value)
    {
        if (ammoInventory.ContainsKey(id))
        {
            ammoInventory[id] += value;
        }
    }

    public void TakeDamage(int value)
    {
        if (dead)
        {
            return;
        }      
        
        if (value<0)
        {
            if (currentHP<initHP)
            {
                currentHP -= value;
                if (currentHP>= initHP)
                {
                    currentHP = initHP;
                }
            }
        }
        else
        {
            currentHP -= value;
        }
   
        if (currentHP <= 0)
        {
            dead = true;
            m_cameraTrans.localPosition = deadPostionTrans.localPosition;
            m_cameraTrans.eulerAngles = deadPostionTrans.eulerAngles;
            weaponPlaceTrans.gameObject.SetActive(false);
            currentHP = 0;
            Game.UIMgr.ShowDeadUI();
            Game.AudioSourceMgr.PlaySound(deadClip);
        }
        else
        {
            if (value>0)
            {
                Game.AudioSourceMgr.PlaySound(hurtClip);
                Game.UIMgr.ShowTakeDamageView();
                cameraShaker.SetShakeValue(0.2f, 0.5f);
            }
            
        }
        Game.UIMgr.UpdateHPValue(currentHP);
    }
}
