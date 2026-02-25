// 새 필드무기 추가하는 방법
/*
 * 1. Update에 있는 Switch문 case WeaponTypes.ItemWeapon: 안에 추가한다.
 * 2. if (Input.GetKeyDown(KeyCode.Alpha3) && 옆에도 추가한다.
 * 3. SetWeapon에서 case WeaponTypes.ItemWeapon: 에도 새로 추가한다.
 * 4. GameManager_Project 스크립트 중에 SelectWeapon에도 새로 추가한다.
 */

using System.Collections;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static _2DTopDown.Item;

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
            ItemWeapon
        }
        WeaponTypes CurrWeapon = WeaponTypes.Null;

        public Rigidbody rb;
        public float moveSpeed = 10.0f;
        public GameObject mousePointer;

        [Header("체력")]
        public float maxHP;
        public float currentHP;

        [Header("애니메이션")]
        public Animator Anim;
        public Animator Anim_Leg;

        [Header("총알 프리팹 & 머즐 플레시")]
        public GameObject[] projectilePrefab;
        public GameObject[] MuzzleFlashs;
        public GameObject GunSmoke;

        [Header("총구 위치 & 근접공격 위치")]
        public Transform[] FireArmsPivot;
        public Transform MeleePivot;

        [Header("반동 연출을 위한 시네머신 제어")]
        public CinemachineImpulseSource CamaraRecoil;

        [Header("조준사격 연출을 위한 시네머신 제어")]
        public CinemachineCamera vCam; // 인스펙터에서 가상 카메라를 할당하세요.
        public float normalSize = 10f; // 기본 직교 크기
        public float aimSize = 15f;    // 조준 시 직교 크기 (현재 설정은 줌 아웃)
        public float zoomSpeed = 10f;

        [Header("사운드")]
        public AudioSource[] FootStep;  // 향후 여러 발소리 추가예정
        public AudioSource[] WeaponAttackSFX;
        public AudioSource WeaponItemEquipSFX;

        [Header("아이템 무기 종류를 저장할 변수")]
        public Item.ItemTypes currentItemWeaponType;

        // 발소리 간격 제어
        private float footStepTimer;
        private float footStepInterval = 0.4f;

        // 근접무기 피해량 제어
        private float MeleeDamage = 50f;

        // 탄약량 제어.
        // 무기종류는 Enum으로 분류되어 있어 public 함수로 따로 정할 수가 없는 상황이라
        // 스크립트 private 함수로 각각 무기별로 제어하기로 결정
        // 체력으로 따지면 AmmoCount => currentHP / AmmoCount_Max => maxHP
        private float AmmoCount;
        private float AmmoCount_Max;

        // 발사간격 제어. 무기별로 발사속도를 제어할 예정이므로 지역변수로 설정하고 0으로 초기화.
        private float fireRate = 0f;    // 총알 사이의 시간 간격 (낮을수록 빠름)
        private float nextFireTime = 0f;   // 다음 발사 가능한 시점 (계산용)

        public static Player instance; // 싱글톤 인스턴스 추가

        private void Awake()
        {
            // 싱글톤 초기화
            if (instance == null) instance = this;
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            currentHP = maxHP;
            GameManager_Project.instance.HealthBar.color = GameManager_Project.instance.healthColor;
            GameManager_Project.instance.HealthBar.fillAmount = currentHP / maxHP;
            GameManager_Project.instance.HealthNum.text = $"{currentHP:F0} / {maxHP:F0}";
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
                    if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
                    {
                        // 원래 fireRate는 연사총에만 넣을 예정이나 근접공격 모션이 안맞아 추가함
                        fireRate = 0.8f;
                        Attack();
                        nextFireTime = Time.time + fireRate;
                    }
                    break;
                case WeaponTypes.Pistol:
                    if (Input.GetMouseButtonDown(0))
                    {
                        Attack();
                    }
                    break;
                case WeaponTypes.ItemWeapon:
                    // 반자동 무기는 GetMouseButtonDown으로 조작하게 설정
                    if (currentItemWeaponType == Item.ItemTypes.Rifle)
                    {
                        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
                        {
                            // fireRate를 먼저 설정
                            fireRate = 0.12f;
                            Attack();
                            nextFireTime = Time.time + fireRate;
                            Anim.SetTrigger("isFiring");
                        }
                    }
                    if (currentItemWeaponType == Item.ItemTypes.Shotgun)
                    {
                        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
                        {
                            fireRate = 0.8f;
                            Attack();
                            nextFireTime = Time.time + fireRate;
                        }
                    }
                    if(currentItemWeaponType == Item.ItemTypes.SMGSD)
                    {
                        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
                        {
                            fireRate = 0.1f;
                            Attack();
                            nextFireTime = Time.time + fireRate;
                        }
                    }
                    if (currentItemWeaponType == Item.ItemTypes.DMR)
                    {
                        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
                        {
                            fireRate = 0.12f;
                            Attack();
                            nextFireTime = Time.time + fireRate;
                        }

                        // 마우스 오른쪽 누르기 : 조준 (Orthographic Size 조절)
                        if (Input.GetMouseButton(1))
                        {
                            // Lens.OrthographicSize로 접근합니다.
                            vCam.Lens.OrthographicSize = Mathf.Lerp(vCam.Lens.OrthographicSize, aimSize, Time.deltaTime * zoomSpeed);
                        }
                        else
                        {
                            // 떼면 다시 원래 크기로 복귀
                            vCam.Lens.OrthographicSize = Mathf.Lerp(vCam.Lens.OrthographicSize, normalSize, Time.deltaTime * zoomSpeed);
                        }
                    }
                    if (currentItemWeaponType == Item.ItemTypes.Null_Weapon)
                    {
                        SetWeapon(WeaponTypes.Pistol);
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

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if(currentItemWeaponType == Item.ItemTypes.Rifle ||
                currentItemWeaponType == Item.ItemTypes.Shotgun ||
                currentItemWeaponType == Item.ItemTypes.SMGSD ||
                currentItemWeaponType == Item.ItemTypes.DMR)
                {
                    SetWeapon(WeaponTypes.ItemWeapon);
                }
            }

            // (Pistol)권총사격 도중 재장전하기
            if (Input.GetKeyDown(KeyCode.R))
            {
                TryReload();
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

        // 아이템 습득 시 호출될 함수 추가
        public void EquipItem(Item.ItemTypes type)
        {
            currentItemWeaponType = type; // 어떤 아이템인지 저장
            SetWeapon(WeaponTypes.ItemWeapon); // 무기 타입을 아이템 무기로 변경
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
                        AmmoCount = 7;
                        AmmoCount_Max = 7;
                        AmmoCount = AmmoCount_Max;
                        UpdateAmmoUI();
                        Anim.SetInteger("WeaponType", 1);
                        break;
                    case WeaponTypes.ItemWeapon:
                        // 아이템 무기일 때
                        // 아이템 종류에 따라 애니메이션 파라미터나 탄약 설정 가능
                        // 예: Rifle은 권총 애니메이션(1)을 공유하거나 별도 번호 부여
                        if (currentItemWeaponType == Item.ItemTypes.Rifle)
                        {
                            AmmoCount = 17;
                            AmmoCount_Max = 17;
                            AmmoCount = AmmoCount_Max;
                            UpdateAmmoUI();
                            Anim.SetInteger("WeaponType", 2);
                        }
                        else if (currentItemWeaponType == Item.ItemTypes.Shotgun)
                        {
                            AmmoCount = 8;
                            AmmoCount_Max = 8;
                            AmmoCount = AmmoCount_Max;
                            UpdateAmmoUI();
                            Anim.SetInteger("WeaponType", 3);
                        }
                        else if (currentItemWeaponType == Item.ItemTypes.SMGSD)
                        {
                            AmmoCount = 20;
                            AmmoCount_Max = 20;
                            AmmoCount = AmmoCount_Max;
                            UpdateAmmoUI();
                            Anim.SetInteger("WeaponType", 4);
                        }
                        else if (currentItemWeaponType == Item.ItemTypes.DMR)
                        {
                            AmmoCount = 6;
                            AmmoCount_Max = 6;
                            AmmoCount = AmmoCount_Max;
                            UpdateAmmoUI();
                            Anim.SetInteger("WeaponType", 5);
                        }
                        else
                        {
                            Anim.SetInteger("WeaponType", 0);
                        }
                        break;
                }
            }
            if (GameManager_Project.instance != null)
            {
                GameManager_Project.instance.SelectWeapon(weaponType);
            }
        }

        // UI 로직 최적화
        private void UpdateAmmoUI()
        {
            if (GameManager_Project.instance != null)
            {
                GameManager_Project.instance.WeaponAmmoGuage.fillAmount = AmmoCount / AmmoCount_Max;
                GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount.ToString();
            }
        }

        bool PistolReload = false; // 권총 재장전 중인지 확인하는 변수
        public void Attack()
        {
            // 1. 재장전 중이면 공격 함수를 바로 빠져나감 (공격 불가)
            if (PistolReload) return;

            switch (CurrWeapon)
            {
                case WeaponTypes.Knife:
                    Invoke("DoHit", 0.2f);
                    WeaponAttackSFX[0].Play();
                    Anim.SetBool("Attack", true);
                    CancelInvoke("AttackOver");
                    Invoke("AttackOver", 0.8f);
                    break;
                case WeaponTypes.Pistol:
                    // 탄약소모를 확인하기 위한 UI 갱신
                    AmmoCount--;
                    GameManager_Project.instance.WeaponAmmoGuage.fillAmount = AmmoCount / AmmoCount_Max;
                    GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount.ToString();

                    // 머즐 플래시 생성
                    CreateMuzzleFlash(0);

                    // 투사체 발사
                    GameObject bullet = Instantiate(projectilePrefab[0], FireArmsPivot[0].position, FireArmsPivot[0].rotation) as GameObject;
                    bullet.transform.LookAt(mousePointer.transform);
                    bullet.transform.Rotate(0, Random.Range(-5.5f, 5.5f), 0);

                    // 시네머신 제어
                    CamaraRecoil.DefaultVelocity = new Vector3(0, -1, 0);
                    CamaraRecoil.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Recoil;
                    CamaraRecoil.GenerateImpulse();

                    // 발사음
                    WeaponAttackSFX[1].Play();
                    AlertEnemies();
                    if (AmmoCount <= 0)
                    {
                        StartCoroutine(ReloadPistol());
                    }
                    break;
                case WeaponTypes.ItemWeapon:
                    if (currentItemWeaponType == Item.ItemTypes.Rifle)
                    {
                        AmmoCount--;
                        GameManager_Project.instance.WeaponAmmoGuage.fillAmount = AmmoCount / AmmoCount_Max;
                        GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount.ToString();

                        CreateMuzzleFlash(1);

                        // 라이플 공격: 자동 연사 (Update에서 GetMouseButton 처리 중)
                        GameObject bulletAR = Instantiate(projectilePrefab[1], FireArmsPivot[1].position, FireArmsPivot[1].rotation);
                        bulletAR.transform.LookAt(mousePointer.transform);
                        bulletAR.transform.Rotate(0, Random.Range(-3f, 3f), 0); // 라이플은 권총보다 반동 적게 설정

                        // 시네머신 초기화
                        CamaraRecoil.DefaultVelocity = new Vector3(0, -1.5f, 0);
                        CamaraRecoil.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Recoil;
                        CamaraRecoil.GenerateImpulse();
                        WeaponAttackSFX[2].Play();

                        AlertEnemies();
                        if (AmmoCount <= 0)
                        {
                            DropWeapon();
                        }
                    }
                    else if (currentItemWeaponType == Item.ItemTypes.Shotgun)
                    {
                        AmmoCount--;
                        GameManager_Project.instance.WeaponAmmoGuage.fillAmount = AmmoCount / AmmoCount_Max;
                        GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount.ToString();

                        CreateMuzzleFlash(2);

                        // 샷건 공격: 한 번에 여러 발 발사(벅샷)
                        for (int i = 0; i < 3; i++)
                        {
                            GameObject birdshot = Instantiate(projectilePrefab[2], FireArmsPivot[2].position, FireArmsPivot[2].rotation);
                            birdshot.transform.LookAt(mousePointer.transform);
                            birdshot.transform.Rotate(0, Random.Range(-15f, 15f), 0); // 산탄 범위 넓게
                        }

                        CamaraRecoil.DefaultVelocity = new Vector3(0, -2.5f, 0);
                        CamaraRecoil.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Recoil;
                        CamaraRecoil.GenerateImpulse();
                        WeaponAttackSFX[3].Play();

                        AlertEnemies();
                        if (AmmoCount <= 0)
                        {
                            DropWeapon();
                        }
                    }
                    else if (currentItemWeaponType == Item.ItemTypes.SMGSD)
                    {
                        AmmoCount--;
                        GameManager_Project.instance.WeaponAmmoGuage.fillAmount = AmmoCount / AmmoCount_Max;
                        GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount.ToString();

                        CreateMuzzleFlashSD(3);

                        GameObject bulletAR = Instantiate(projectilePrefab[0], FireArmsPivot[1].position, FireArmsPivot[1].rotation);
                        bulletAR.transform.LookAt(mousePointer.transform);
                        bulletAR.transform.Rotate(0, Random.Range(-2.7f, 2.7f), 0);

                        // 시네머신 초기화
                        CamaraRecoil.DefaultVelocity = new Vector3(0, -0.3f, 0);
                        CamaraRecoil.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Recoil;
                        CamaraRecoil.GenerateImpulse();
                        WeaponAttackSFX[4].Play();

                        if (AmmoCount <= 0)
                        {
                            DropWeapon();
                        }
                    }
                    else if (currentItemWeaponType == Item.ItemTypes.DMR)
                    {
                        AmmoCount--;
                        GameManager_Project.instance.WeaponAmmoGuage.fillAmount = AmmoCount / AmmoCount_Max;
                        GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount.ToString();

                        CreateMuzzleFlash(1);

                        GameObject bulletDMR = Instantiate(projectilePrefab[3], FireArmsPivot[4].position, FireArmsPivot[4].rotation);
                        bulletDMR.transform.LookAt(mousePointer.transform);
                        bulletDMR.transform.Rotate(0, Random.Range(-1.5f, 1.5f), 0);

                        CamaraRecoil.DefaultVelocity = new Vector3(0, -3f, 0);
                        CamaraRecoil.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Recoil;
                        CamaraRecoil.GenerateImpulse();
                        WeaponAttackSFX[5].Play();

                        if (AmmoCount <= 0)
                        {
                            DropWeapon();
                        }
                    }
                    else
                    {
                        return;
                    }
                    break;
            }
        }

        // 머즐 플래시 로직
        private void CreateMuzzleFlash(int index)
        {
            // 배열 범위를 벗어나지 않는지 확인하고 프리팹이 있는지 체크
            if (MuzzleFlashs != null && MuzzleFlashs.Length > index && MuzzleFlashs[index] != null)
            {
                // FireArmsPivot 위치와 회전값으로 생성
                GameObject flash = Instantiate(MuzzleFlashs[index], FireArmsPivot[index].position, FireArmsPivot[index].rotation);
                
                // 총구 연막생성
                GameObject FireSmoke = Instantiate(GunSmoke, FireArmsPivot[index].position, FireArmsPivot[index].rotation);

                // 총구 위치를 계속 따라가게 하려면 부모를 설정 (선택 사항)
                flash.transform.SetParent(FireArmsPivot[index]);
                FireSmoke.transform.SetParent(FireArmsPivot[index]);

                // 머즐 플래시 회전값 설정
                flash.transform.Rotate(0, 270, 0);
                FireSmoke.transform.Rotate(0, 270, 0);

                // 아주 짧은 시간 뒤에 자동 삭제 (0.15초)
                Destroy(flash, 0.15f);
                Destroy(FireSmoke, 0.5f);
            }
        }
        // 머즐 플래시 로직 (소음기)
        private void CreateMuzzleFlashSD(int index)
        {
            // 배열 범위를 벗어나지 않는지 확인하고 프리팹이 있는지 체크
            if (MuzzleFlashs != null && MuzzleFlashs.Length > index && MuzzleFlashs[index] != null)
            {
                // FireArmsPivot 위치와 회전값으로 생성
                GameObject flash = Instantiate(MuzzleFlashs[index], FireArmsPivot[index].position, FireArmsPivot[index].rotation);

                // 총구 위치를 계속 따라가게 하려면 부모를 설정 (선택 사항)
                flash.transform.SetParent(FireArmsPivot[index]);

                // 머즐 플래시 회전값 설정
                flash.transform.Rotate(0, 270, 0);

                // 아주 짧은 시간 뒤에 자동 삭제 (0.5초)
                Destroy(flash, 0.5f);
            }
        }

        // 재장전 시도를 위한 별도 함수 (가독성과 안전성을 위해)
        private void TryReload()
        {
            // 1. 현재 무기가 권총(Pistol)인지 확인
            // 2. 이미 재장전 중(isReloading)인지 확인
            // 3. 이미 탄약이 꽉 차 있는지 확인 (선택 사항)
            if (CurrWeapon == WeaponTypes.Pistol && !PistolReload && AmmoCount < AmmoCount_Max)
            {
                StartCoroutine(ReloadPistol());
            }
        }

        // 재장전 처리를 위한 코루틴
        private IEnumerator ReloadPistol()
        {
            PistolReload = true;
            GameManager_Project.instance.PistolReload_ArlarmTxt.enabled = true;

            // 2초 대기
            yield return new WaitForSeconds(2.0f);

            // 탄약 채우기
            AmmoCount = AmmoCount_Max;

            // UI 갱신 (다시 꽉 찬 상태로)
            GameManager_Project.instance.WeaponAmmoGuage.fillAmount = 1f;
            GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount.ToString();

            PistolReload = false;
            GameManager_Project.instance.PistolReload_ArlarmTxt.enabled = false;
            Debug.Log("재장전 완료!");
        }

        public void DropWeapon()
        {
            // 탄약 UI 표시 초기화 (0으로 설정)
            if (GameManager_Project.instance != null)
            {
                GameManager_Project.instance.WeaponAmmoGuage.fillAmount = 0;
                GameManager_Project.instance.WeaponAmmoTxt.text = "0";
            }

            // 필드무기는 아무것도 없는 상태로 교체 & 모션과 무기는 권총으로 교체
            SetWeapon(WeaponTypes.Pistol);
            EquipItem(ItemTypes.Null_Weapon);
        }

        private void AttackOver()
        {
            Anim.SetBool("Attack", false);
        }

        private void AlertEnemies()
        {
            // 기존의 hitTestPivot에서 플레이어의 위치인 transform으로 대체
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, 25.0f, transform.up);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null && hit.collider.tag == "Enemy")
                {
                    hit.collider.GetComponent<Enemy_Info>().SetAlertPos(transform.position);
                }
                else
                {
                    return;
                }
            }
        }

        public void DoHit()
        {
            RaycastHit[] hits = Physics.SphereCastAll(MeleePivot.position, 2.2f, MeleePivot.up);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null && hit.collider.tag == "Enemy" || hit.collider.tag == "Dummy")
                {
                    RaycastHit forwarHit = new RaycastHit();
                    Physics.Raycast(MeleePivot.position, hit.transform.position - transform.position, out forwarHit);
                    if (forwarHit.collider != null && forwarHit.collider.tag == "Enemy" || forwarHit.collider.tag == "Dummy")
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
            GameManager_Project.instance.HealthBar.fillAmount = currentHP / maxHP;
            GameManager_Project.instance.HealthNum.text = $"{currentHP:F0} / {maxHP:F0}";

            // 시네머신 제어
            CamaraRecoil.DefaultVelocity = new Vector3(1, 0, 1);
            CamaraRecoil.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
            CamaraRecoil.GenerateImpulse();

            // UI Image로 피격연출
            GameManager_Project.instance.StartCoroutine(isHit());

            if (currentHP > 50)
            {
                GameManager_Project.instance.HealthBar.color = GameManager_Project.instance.healthColor;
                GameManager_Project.instance.PlayerDangerEffect.SetActive(false);

                GameManager_Project.instance.HealthBar.fillAmount = currentHP / maxHP;
                GameManager_Project.instance.HealthNum.text = $"{currentHP:F0} / {maxHP:F0}";
            }
            else if (currentHP > 20)
            {
                // 체력 상태변화를 시각적으로 확인하기 위해 노란색으로 변경
                GameManager_Project.instance.HealthBar.color = GameManager_Project.instance.healthWarningColor;
                GameManager_Project.instance.PlayerDangerEffect.SetActive(false);

                // 체력 UI 갱신
                GameManager_Project.instance.HealthBar.fillAmount = currentHP / maxHP;
                GameManager_Project.instance.HealthNum.text = $"{currentHP:F0} / {maxHP:F0}";
            }
            else
            {
                GameManager_Project.instance.HealthBar.color = GameManager_Project.instance.healthDangerColor;
                GameManager_Project.instance.PlayerDangerEffect.SetActive(true);

                GameManager_Project.instance.HealthBar.fillAmount = currentHP / maxHP;
                GameManager_Project.instance.HealthNum.text = $"{currentHP:F0} / {maxHP:F0}";
            }

            if (currentHP <= 0)
            {
                PlayerDead();
            }
        }
        public void PlayerDead()
        {
            Anim.SetBool("Dead", true);
            Anim_Leg.SetBool("Dead", true);
            Anim.transform.parent = null;
            this.enabled = false;
            rb.isKinematic = true;
            GameManager_Project.instance.gameOver = true;
            GameManager_Project.instance.GameOver.SetActive(true);
            gameObject.GetComponent<Collider>().enabled = false;
            Vector3 pos = Anim.transform.position;
            pos.y = 0.2f;
            Anim.transform.position = pos;
        }

        // 0.2초 동안 피격 화면 연출하기
        public IEnumerator isHit()
        {
            GameManager_Project.instance.PlayerHitEffect.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            GameManager_Project.instance.PlayerHitEffect.SetActive(false);
        }

        public void PlayerHeal(float HealHP)
        {
            currentHP += HealHP;
            // 체력 UI 갱신
            GameManager_Project.instance.HealthBar.fillAmount = currentHP / maxHP;
            GameManager_Project.instance.HealthNum.text = $"{currentHP:F0} / {maxHP:F0}";

            // 최대 체력을 초과하지 않도록 제한
            if (currentHP > maxHP) currentHP = maxHP;

            // 체력이 회복되었으므로 색상 및 경고 효과 상태 업데이트
            GameManager_Project.instance.HealthBar.color = GameManager_Project.instance.healthColor;
            GameManager_Project.instance.PlayerDangerEffect.SetActive(false);

            GameManager_Project.instance.HealthBar.fillAmount = currentHP / maxHP;
            GameManager_Project.instance.HealthNum.text = $"{currentHP:F0} / {maxHP:F0}";
        }
    }
}
