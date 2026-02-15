using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _2DTopDown
{
    public class Player : MonoBehaviour
    {
        // 무기 타입
        public enum WeaponTypes
        {
            Knife = 0,
            Pistol = 1,
            Null = 2,
            Rifle = 3,
            ShotGun = 4
        }
        WeaponTypes CurrWeapon = WeaponTypes.Null;

        Rigidbody rb;
        public float moveSpeed = 10.0f;
        public GameObject mousePointer;

        [Header("체력")]
        public float maxHP;
        public float currentHP;
        public Image HealthBar;
        public TextMeshProUGUI HealthNum;

        [Header("애니메이션")]
        public Animator Anim;
        public Animator Anim_Leg;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            currentHP = maxHP;
            HealthBar.fillAmount = currentHP / maxHP;
            HealthNum.text = $"{currentHP:F0} / {maxHP:F0}";
            SetWeapon(WeaponTypes.Knife);
        }

        private void Update()
        {
            //  이동조작 및 움직임
            float H = Input.GetAxis("Horizontal");
            float V = Input.GetAxis("Vertical");
            Vector3 moveVec = new Vector3(H * moveSpeed, 0.0f, V * moveSpeed);
            rb.linearVelocity = moveVec;
            Anim_Leg.SetBool("isWalk", moveVec.magnitude > 0.1);
            Anim_Leg.SetFloat("xDir", H);
            Anim_Leg.SetFloat("yDir", V);

            // 마우스 포인터 갱신
            UpdateAim();

            switch (CurrWeapon)
            {
                case WeaponTypes.Knife:
                    if (Input.GetMouseButton(0))
                    {

                    }
                    break;
                case WeaponTypes.Pistol:
                    if (Input.GetMouseButtonDown(0))
                    {

                    }
                    break;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetWeapon(WeaponTypes.Knife);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetWeapon(WeaponTypes.Pistol);
            }
        }

        // 마우스 포인터
        public void UpdateAim()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.y = transform.position.y;
            mousePointer.transform.position = mousePos;
            float deltaY = mousePos.z - transform.position.z;
            float deltaX = mousePos.x - transform.position.x;
            float angleInDegrees = Mathf.Atan2(deltaY, deltaX) * 180 / Mathf.PI;
            transform.eulerAngles = new Vector3(0, -angleInDegrees, 0);
        }

        // 무기세팅
        public void SetWeapon(WeaponTypes weaponType)
        {
            if (weaponType != CurrWeapon)
            {
                CurrWeapon = weaponType;
                Anim.SetTrigger("WeaponChange");
                switch (weaponType)
                {
                    case WeaponTypes.Knife:
                        Anim.SetInteger("WeaponType", 0);
                        break;
                    case WeaponTypes.Pistol:
                        Anim.SetInteger("WeaponType", 1);
                        break;
                }
            }
            if (GameManager_Project.instance != null)
            {
                GameManager_Project.instance.SelectWeapon(weaponType);
            }
        }
    }
}
