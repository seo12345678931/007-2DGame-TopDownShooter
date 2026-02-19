using TMPro;
using Unity.Cinemachine;
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

        [Header("총알 프리팹")]
        public GameObject proyectilePrefab;

        [Header("총구 위치 & 근접공격 위치")]
        public Transform FireArmsPivot;
        public Transform MeleePivot;

        [Header("반동 연출을 위한 시네머신 제어")]
        public CinemachineImpulseSource CamaraRecoil;

        [Header("사운드")]
        public AudioSource[] FootStep;  // 향후 여러 발소리 추가예정
        public AudioSource[] WeaponAttackSFX;

        // 발소리 간격 제어
        private float footStepTimer;
        private float footStepInterval = 0.4f;

        // 근접무기 피해량 제어
        private float MeleeDamage = 50f;

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

            bool isMoving = moveVec.magnitude > 0.1f;
            // --- 발자국 소리 로직 시작 ---
            if (isMoving)
            {
                footStepTimer += Time.deltaTime; // 움직일 때만 타이머 증가

                if (footStepTimer >= footStepInterval)
                {
                    FootStep[0].Play();
                    footStepTimer = 0f; // 재생 후 타이머 초기화
                }
            }
            else
            {
                // 멈췄다 다시 움직일 때 즉시 첫 소리가 나도록 설정
                footStepTimer = footStepInterval; 
            }

            // 마우스 포인터 갱신
            UpdateAim();

            switch (CurrWeapon)
            {
                case WeaponTypes.Knife:
                    if (Input.GetMouseButtonDown(0))
                    {
                        Attack();
                    }
                    break;
                case WeaponTypes.Pistol:
                    if (Input.GetMouseButtonDown(0))
                    {
                        Attack();
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

        public void Attack()
        {
            switch (CurrWeapon)
            {
                case WeaponTypes.Knife:
                    Invoke("DoHit", 0.2f);
                    WeaponAttackSFX[0].Play();
                    CancelInvoke("AttackOver");
                    Invoke("AttackOver", 0.4f);
                    break;
                case WeaponTypes.Pistol:
                    GameObject bullet = GameObject.Instantiate(proyectilePrefab, FireArmsPivot.position, FireArmsPivot.rotation) as GameObject;
                    CamaraRecoil.GenerateImpulse();
                    bullet.transform.LookAt(mousePointer.transform);
                    bullet.transform.Rotate(0, Random.Range(-5.5f, 5.5f), 0);
                    WeaponAttackSFX[1].Play();
                    //AlertEnemies();
                    break;
            }
            Anim.SetBool("Attack", true);
        }

        private void AttackOver()
        {
            Anim.SetBool("Attack", false);
        }

        public void DoHit()
        {
            RaycastHit[] hits = Physics.SphereCastAll(MeleePivot.position, 2.0f, MeleePivot.up);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null && hit.collider.tag == "Enemy")
                {
                    RaycastHit forwarHit = new RaycastHit();
                    Physics.Raycast(MeleePivot.position, hit.transform.position - transform.position, out forwarHit);
                    if (forwarHit.collider != null && forwarHit.collider.tag == "Enemy")
                    {
                        forwarHit.collider.GetComponent<Enemy_Info>().TakeDamage(MeleeDamage);
                    }
                }
            }
        }

        public void DamagePlayer(float DMG)
        {
            currentHP -= DMG;
            // 체력 UI 갱신
            HealthBar.fillAmount = currentHP / maxHP;
            HealthNum.text = $"{currentHP:F0} / {maxHP:F0}";
            if (currentHP <= 0)
            {
                PlayerDead();
            }
        }
        public void PlayerDead()
        {
            Anim.SetBool("Dead", true);
            Anim.transform.parent = null;
            this.enabled = false;
            rb.isKinematic = true;
            GameManager.RegisterPlayerDeath();
            gameObject.GetComponent<Collider>().enabled = false;
            GameCamera.ToggleShake(0.3f);
            Vector3 pos = Anim.transform.position;
            pos.y = 0.2f;
            Anim.transform.position = pos;
        }
    }
}
