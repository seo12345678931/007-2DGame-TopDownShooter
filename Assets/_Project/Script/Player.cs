// 새 필드무기 추가하는 방법
/*
 * 1. Update에 있는 Switch문 case WeaponTypes.ItemWeapon: 안에 추가한다.
 * 2. if (Input.GetKeyDown(KeyCode.Alpha3) && 옆에도 추가한다.
 * 3. SetWeapon에서 case WeaponTypes.ItemWeapon: 에도 새로 추가한다.
 * 4. GameManager_Project 스크립트 중에 SelectWeapon에도 새로 추가한다.
 */

using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
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

        [Header("조준사격 연출 및 마우스 포인터를 위한 시네머신 제어")]
        public CinemachineCamera virtualCamera;
        public float normalSize = 10f; // 기본 직교 크기
        public float aimSize = 15f;    // 조준 시 직교 크기 (현재 설정은 줌 아웃)
        public float zoomSpeed = 10f;

        [Header("사운드")]
        public AudioSource[] FootStep;  // 향후 여러 발소리 추가예정
        public AudioSource[] WeaponAttackSFX;   // 무기발사음
        public AudioSource WeaponItemEquipSFX;  // 아이템 획득
        public AudioSource HPItem_SFX;  // 플레이어 체력회복
        public AudioSource PlayerDangerSFX; // 체력 20% 이하 시 특정소리를 반복출력
        public AudioSource PistolReloadSFX; // 권총장전

        // 피격, 사망 사운드는 랜덤함수로 출력
        public AudioSource[] PlayerHitSFX;
        public AudioSource[] PlayerWeapon_HitSFX;
        public AudioSource[] PlayerDeadSFX;

        [Header("아이템 무기 종류를 저장할 변수")]
        public Item.ItemTypes currentItemWeaponType;

        // 근접무기 피해량 제어
        private float MeleeDamage = 50f;

        // 탄약량 제어. (pistolAmmo_Current / ItemWeaponAmmo_Current 각각 담아낼 변수)
        // 스크립트를 통해 각각 무기별로 제어할 예정이라 인스펙터를 숨김
        // 체력으로 따지면 AmmoCount => currentHP / AmmoCount_Max => maxHP
        [HideInInspector]
        public float pistolAmmo_Current;
        public float ItemWeaponAmmo_Current;
        public float AmmoCount;
        public float AmmoCount_Max;
        public float AmmoCount_Item;
        public float AmmoCount_Max_Item;

        // 발사간격 제어. 무기별로 발사속도를 제어할 예정이므로 지역변수로 설정하고 0으로 초기화.
        private float fireRate = 0f;    // 총알 사이의 시간 간격 (낮을수록 빠름)
        private float nextFireTime = 0f;   // 다음 발사 가능한 시점 (계산용)

        // 마우스 포인터를 위한 시네머신 제어
        private CinemachinePositionComposer composer;

        public static Player instance; // 싱글톤 인스턴스 추가
        private void Awake()
        {
            // 싱글톤 초기화
            if (instance == null) instance = this;

            // 컴포저 컴포넌트를 미리 가져오기
            if (virtualCamera != null)
            {
                composer = virtualCamera.GetComponent<CinemachinePositionComposer>();
            }
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            currentHP = maxHP;
            GameManager_Project.instance.HealthBar.color = GameManager_Project.instance.healthColor;
            GameManager_Project.instance.HealthBar.fillAmount = currentHP / maxHP;
            GameManager_Project.instance.HealthNum.text = $"{currentHP:F0} / {maxHP:F0}";
            SetWeapon(WeaponTypes.Knife);
            pistolAmmo_Current = 7;
            ItemWeaponAmmo_Current = 0;
        }

        private void Update()
        {
            if (!CanProcessGameplayInput())
            {
                rb.linearVelocity = Vector3.zero;
                Anim_Leg.SetBool("isWalk", false);
                return;
            }

            //  이동조작 및 움직임
            float H = Input.GetAxis("Horizontal");
            float V = Input.GetAxis("Vertical");
            Vector3 moveVec = new Vector3(H * moveSpeed, 0.0f, V * moveSpeed);
            rb.linearVelocity = moveVec;
            Anim_Leg.SetBool("isWalk", moveVec.magnitude > 0.1);
            Anim_Leg.SetFloat("xDir", H);
            Anim_Leg.SetFloat("yDir", V);

            bool isMoving = moveVec.magnitude > 0.1f;

            // 마우스 포인터 갱신
            UpdateAim();

            switch (CurrWeapon)
            {
                case WeaponTypes.Knife:
                    if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
                    {
                        // 원래 fireRate는 연사총에만 넣을 예정이나 근접공격 모션이 안맞아 추가함
                        fireRate = 0.5f;
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
                            virtualCamera.Lens.OrthographicSize = Mathf.Lerp(virtualCamera.Lens.OrthographicSize, aimSize, Time.deltaTime * zoomSpeed);
                            GameManager_Project.instance.Scope.gameObject.SetActive(true);
                        }
                        else
                        {
                            // 떼면 다시 원래 크기로 복귀
                            virtualCamera.Lens.OrthographicSize = Mathf.Lerp(virtualCamera.Lens.OrthographicSize, normalSize, Time.deltaTime * zoomSpeed);
                            GameManager_Project.instance.Scope.gameObject.SetActive(false);
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

        private bool CanProcessGameplayInput()
        {
            if (Time.timeScale <= 0f)
                return false;

            if (GameManager_Project.instance == null)
                return true;

            return !GameManager_Project.instance.gameOver && !GameManager_Project.instance.gameClear;
        }

        [SerializeField] private float cameraShiftAmount = 0.1f;
        [SerializeField] private float cameraShiftSmoothTime = 0.3f;
        [SerializeField] private float cameraShiftMaxSpeed = 0.35f;
        [SerializeField] private float cameraShiftDeadZone = 0.5f;
        [SerializeField] private float cameraShiftFullDistance = 6f;
        private Vector2 cameraShiftVelocity;
        // 마우스 포인터
        public void UpdateAim()
        {
            // 1. 기존 마우스 포인터 및 회전 로직
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.y = transform.position.y;
            mousePointer.transform.position = mousePos;

            float deltaZ = mousePos.z - transform.position.z;
            float deltaX = mousePos.x - transform.position.x;

            // 각도 계산: $angle = \arctan2(\Delta Z, \Delta X) \times \frac{180}{\pi}$
            float angleInDegrees = Mathf.Atan2(deltaZ, deltaX) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, -angleInDegrees, 0);

            // 2. 시네머신 Screen Position 제어 로직
            if (composer != null)
            {
                Vector2 targetPos = GetAimCameraScreenPosition(deltaX, deltaZ);
                Vector2 currentPos = composer.Composition.ScreenPosition;
                currentPos = Vector2.SmoothDamp(
                    currentPos,
                    targetPos,
                    ref cameraShiftVelocity,
                    cameraShiftSmoothTime,
                    cameraShiftMaxSpeed,
                    Time.deltaTime);

                composer.Composition.ScreenPosition = currentPos;
            }
        }

        // 현재 조준 방향을 기준으로 부드러운 카메라 화면 오프셋을 계산.
        private Vector2 GetAimCameraScreenPosition(float deltaX, float deltaZ)
        {
            Vector2 aimDelta = new Vector2(deltaX, deltaZ);
            float aimDistance = aimDelta.magnitude;

            if (aimDistance <= cameraShiftDeadZone)
                return Vector2.zero;

            float fullDistance = Mathf.Max(cameraShiftDeadZone + 0.01f, cameraShiftFullDistance);
            float shiftStrength = Mathf.InverseLerp(cameraShiftDeadZone, fullDistance, aimDistance);
            Vector2 aimDirection = aimDelta / aimDistance;

            return new Vector2(
                -aimDirection.x * cameraShiftAmount * shiftStrength,
                aimDirection.y * cameraShiftAmount * shiftStrength);
        }

        // 아이템 습득 시 호출될 함수 추가
        public void EquipItem(Item.ItemTypes type)
        {
            // 1. 동일한 무기를 또 먹은 경우 (탄약 보급)
            if (currentItemWeaponType == type)
            {
                UpdateItemWeaponMaxStats(); // 현재 타입의 Max값을 먼저 가져옴
                ItemWeaponAmmo_Current = AmmoCount_Max_Item; // 보관함 가득 채움
                AmmoCount_Item = ItemWeaponAmmo_Current; // 현재 탄약 적용
                UpdateAmmoUI();
                return;
            }

            // 2. 새로운 무기를 먹은 경우
            currentItemWeaponType = type;

            // 무기 종류에 맞는 Max치를 미리 계산해서 보관함에 먼저 넣기
            UpdateItemWeaponMaxStats();
            ItemWeaponAmmo_Current = AmmoCount_Max_Item;

            // 실제 발사에 쓰이는 AmmoCount를 새 무기의 탄약수로 덮어씌움
            AmmoCount_Item = ItemWeaponAmmo_Current;

            // 3. 이제 무기 외형을 바꿉니다. (SetWeapon 내에서 보관함 값을 로드함)
            SetWeapon(WeaponTypes.ItemWeapon);

            UpdateAmmoUI();
        }

        // 무기세팅
        public void SetWeapon(WeaponTypes weaponType)
        {
            if (weaponType != CurrWeapon)
            {
                SaveCurrentAmmo(); // 1. 현재 무기 탄수 저장
                CurrWeapon = weaponType; // 2. 무기 교체

                CurrWeapon = weaponType;
                Anim.SetTrigger("WeaponChange");
                switch (weaponType)
                {
                    case WeaponTypes.Knife:
                        Anim.SetInteger("WeaponType", 0);
                        break;
                    case WeaponTypes.Pistol:
                        Anim.SetInteger("WeaponType", 1);
                        AmmoCount_Max = 7;
                        AmmoCount = pistolAmmo_Current;
                        break;
                    case WeaponTypes.ItemWeapon:
                        UpdateItemWeaponMaxStats();
                        AmmoCount_Item = ItemWeaponAmmo_Current;
                        break;
                }
                UpdateAmmoUI();
            }
            if (GameManager_Project.instance != null)
            {
                GameManager_Project.instance.SelectWeapon(weaponType);
            }
        }
        // 각 필드 무기별 최대 탄약수만 결정하는 보조 함수
        private void UpdateItemWeaponMaxStats()
        {
            switch (currentItemWeaponType)
            {
                case Item.ItemTypes.Rifle:
                    AmmoCount_Max_Item = 20;
                    Anim.SetInteger("WeaponType", 2);
                    break;
                case Item.ItemTypes.Shotgun:
                    AmmoCount_Max_Item = 8;
                    Anim.SetInteger("WeaponType", 3);
                    break;
                case Item.ItemTypes.SMGSD:
                    AmmoCount_Max_Item = 20;
                    Anim.SetInteger("WeaponType", 4);
                    break;
                case Item.ItemTypes.DMR:
                    AmmoCount_Max_Item = 9;
                    Anim.SetInteger("WeaponType", 5);
                    break;
                default:
                    AmmoCount_Max_Item = 0;
                    Anim.SetInteger("WeaponType", 0);
                    break;
            }
        }

        // 탄약을 소모할 때마다 호출
        public void SaveCurrentAmmo()
        {
            if (CurrWeapon == WeaponTypes.Pistol)
            {
                pistolAmmo_Current = AmmoCount;
            }
            else if (CurrWeapon == WeaponTypes.ItemWeapon)
            {
                ItemWeaponAmmo_Current = AmmoCount_Item;
            }
        }

        // UI 로직 최적화
        private void UpdateAmmoUI()
        {
            if (GameManager_Project.instance != null && CurrWeapon == WeaponTypes.Pistol)
            {
                GameManager_Project.instance.WeaponAmmoGuage.fillAmount = AmmoCount / AmmoCount_Max;
                GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount.ToString();
            }
            else if(GameManager_Project.instance != null && CurrWeapon == WeaponTypes.ItemWeapon)
            {
                GameManager_Project.instance.WeaponAmmoGuage.fillAmount = AmmoCount_Item / AmmoCount_Max_Item;
                GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount_Item.ToString();
            }
        }

        bool PistolReload = false; // 권총 재장전 중인지 확인하는 변수
        public void Attack()
        {
            if (!CanProcessGameplayInput())
                return;

            // 1. 재장전 중이면 공격 함수를 바로 빠져나감 (공격 불가)
            if (PistolReload) return;

            switch (CurrWeapon)
            {
                case WeaponTypes.Knife:
                    Invoke("DoHit", 0.2f);
                    WeaponAttackSFX[0].Play();
                    Anim.SetBool("Attack", true);
                    CancelInvoke("AttackOver");
                    Invoke("AttackOver", 0.5f);
                    break;
                case WeaponTypes.Pistol:
                    // 탄약소모를 확인하기 위한 UI 갱신
                    AmmoCount--;
                    // 게임 튕김이나 예기치 못한 상황일 때 탄약량을 실시간으로 안전하게
                    // 지키기 위해 한 번 더 생성.
                    SaveCurrentAmmo();
                    GameManager_Project.instance.WeaponAmmoGuage.fillAmount = AmmoCount / AmmoCount_Max;
                    GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount.ToString();

                    // 머즐 플래시 생성
                    CreateMuzzleFlash(0);

                    // 투사체 발사
                    GameObject bullet = BulletPool.Spawn(projectilePrefab[0], FireArmsPivot[0].position, FireArmsPivot[0].rotation);
                    bullet.transform.LookAt(mousePointer.transform);
                    bullet.transform.Rotate(0, Random.Range(-5.5f, 5.5f), 0);

                    // 시네머신 제어
                    CamaraRecoil.DefaultVelocity = new Vector3(0, -1, 0);
                    CamaraRecoil.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Recoil;
                    CamaraRecoil.GenerateImpulse();

                    // 발사음
                    WeaponAttackSFX[1].Play();
                    AlertEnemies(0);
                    if (AmmoCount <= 0)
                    {
                        StartCoroutine(ReloadPistol());
                    }
                    break;
                case WeaponTypes.ItemWeapon:
                    if (currentItemWeaponType == Item.ItemTypes.Rifle)
                    {
                        AmmoCount_Item--;
                        SaveCurrentAmmo();
                        GameManager_Project.instance.WeaponAmmoGuage.fillAmount = AmmoCount_Item / AmmoCount_Max_Item;
                        GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount_Item.ToString();

                        CreateMuzzleFlash(1);

                        // 라이플 공격: 자동 연사 (Update에서 GetMouseButton 처리 중)
                        GameObject bulletAR = BulletPool.Spawn(projectilePrefab[1], FireArmsPivot[1].position, FireArmsPivot[1].rotation);
                        bulletAR.transform.LookAt(mousePointer.transform);
                        bulletAR.transform.Rotate(0, Random.Range(-3f, 3f), 0); // 라이플은 권총보다 반동 적게 설정

                        // 시네머신 초기화
                        CamaraRecoil.DefaultVelocity = new Vector3(0, -1.5f, 0);
                        CamaraRecoil.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Recoil;
                        CamaraRecoil.GenerateImpulse();
                        WeaponAttackSFX[2].Play();

                        AlertEnemies(1);
                        if (AmmoCount_Item <= 0)
                        {
                            DropWeapon();
                        }
                    }
                    else if (currentItemWeaponType == Item.ItemTypes.Shotgun)
                    {
                        AmmoCount_Item--;
                        SaveCurrentAmmo();
                        GameManager_Project.instance.WeaponAmmoGuage.fillAmount = AmmoCount_Item / AmmoCount_Max_Item;
                        GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount_Item.ToString();

                        CreateMuzzleFlash(2);

                        // 샷건 공격: 한 번에 여러 발 발사(벅샷)
                        for (int i = 0; i < 3; i++)
                        {
                            GameObject birdshot = BulletPool.Spawn(projectilePrefab[2], FireArmsPivot[2].position, FireArmsPivot[2].rotation);
                            birdshot.transform.LookAt(mousePointer.transform);
                            birdshot.transform.Rotate(0, Random.Range(-10f, 10f), 0); // 산탄 범위 넓게
                        }

                        CamaraRecoil.DefaultVelocity = new Vector3(0, -2.5f, 0);
                        CamaraRecoil.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Recoil;
                        CamaraRecoil.GenerateImpulse();
                        WeaponAttackSFX[3].Play();

                        AlertEnemies(2);
                        if (AmmoCount_Item <= 0)
                        {
                            DropWeapon();
                        }
                    }
                    else if (currentItemWeaponType == Item.ItemTypes.SMGSD)
                    {
                        AmmoCount_Item--;
                        SaveCurrentAmmo();
                        GameManager_Project.instance.WeaponAmmoGuage.fillAmount = AmmoCount_Item / AmmoCount_Max_Item;
                        GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount_Item.ToString();

                        CreateMuzzleFlashSD(3);

                        GameObject bulletAR = BulletPool.Spawn(projectilePrefab[0], FireArmsPivot[1].position, FireArmsPivot[1].rotation);
                        bulletAR.transform.LookAt(mousePointer.transform);
                        bulletAR.transform.Rotate(0, Random.Range(-2.7f, 2.7f), 0);

                        // 시네머신 초기화
                        CamaraRecoil.DefaultVelocity = new Vector3(0, -0.3f, 0);
                        CamaraRecoil.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Recoil;
                        CamaraRecoil.GenerateImpulse();
                        WeaponAttackSFX[4].Play();

                        AlertEnemiesSD(3);
                        if (AmmoCount_Item <= 0)
                        {
                            DropWeapon();
                        }
                    }
                    else if (currentItemWeaponType == Item.ItemTypes.DMR)
                    {
                        AmmoCount_Item--;
                        SaveCurrentAmmo();
                        GameManager_Project.instance.WeaponAmmoGuage.fillAmount = AmmoCount_Item / AmmoCount_Max_Item;
                        GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount_Item.ToString();

                        CreateMuzzleFlash(1);

                        GameObject bulletDMR = BulletPool.Spawn(projectilePrefab[3], FireArmsPivot[4].position, FireArmsPivot[4].rotation);
                        bulletDMR.transform.LookAt(mousePointer.transform);
                        bulletDMR.transform.Rotate(0, Random.Range(-1.5f, 1.5f), 0);

                        CamaraRecoil.DefaultVelocity = new Vector3(0, -3f, 0);
                        CamaraRecoil.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Recoil;
                        CamaraRecoil.GenerateImpulse();
                        WeaponAttackSFX[5].Play();

                        AlertEnemies(4);
                        if (AmmoCount_Item <= 0)
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
            PistolReloadSFX.Play();

            // 1초 대기
            yield return new WaitForSeconds(1.0f);

            // 탄약 채우기
            AmmoCount = AmmoCount_Max;

            // UI 갱신 (다시 꽉 찬 상태로)
            GameManager_Project.instance.WeaponAmmoGuage.fillAmount = 1f;
            GameManager_Project.instance.WeaponAmmoTxt.text = AmmoCount.ToString();

            PistolReload = false;
            GameManager_Project.instance.PistolReload_ArlarmTxt.enabled = false;
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

            // DMR 조준사격 도중 모든 탄약이 소진되면 렌즈값을 다시 원래대로 복귀하기
            virtualCamera.Lens.OrthographicSize = normalSize;
        }

        private void AttackOver()
        {
            Anim.SetBool("Attack", false);
        }

        private void AlertEnemies(int index)
        {
            RaycastHit[] hits = Physics.SphereCastAll(FireArmsPivot[index].position, 10.0f, FireArmsPivot[index].up);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null && hit.collider.tag == "Enemy")
                {
                    hit.collider.GetComponent<Enemy_Info>().SetAlertPos(transform.position);
                }
            }
        }

        // 소음기용
        private void AlertEnemiesSD(int index)
        {
            RaycastHit[] hits = Physics.SphereCastAll(FireArmsPivot[index].position, 5.0f, FireArmsPivot[index].up);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null && hit.collider.tag == "Enemy")
                {
                    hit.collider.GetComponent<Enemy_Info>().SetAlertPos(transform.position);
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
                    if (forwarHit.collider.tag == "Dummy")
                    {
                        forwarHit.collider.GetComponent<Enemy_Info>().TakeDamage(MeleeDamage);
                    }
                    if(forwarHit.collider != null && forwarHit.collider.tag == "Enemy")
                    {
                        // 근접 피해량 뻥튀기 (50 * 2 = 100)
                        forwarHit.collider.GetComponent<Enemy_Info>().TakeDamage(MeleeDamage * 2);
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


            // 피격 사운드 랜덤 재생
            int randomIndex = Random.Range(0, PlayerHitSFX.Length);
            PlayerHitSFX[randomIndex].Play();
            int randomIndex_Impact = Random.Range(0, PlayerWeapon_HitSFX.Length);
            PlayerWeapon_HitSFX[randomIndex_Impact].Play();

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

                // 신음소리 출력
                PlayerDangerSFX.Play();
            }

            if (currentHP <= 0)
            {
                PlayerDead();
            }
        }
        public void PlayerDead()
        {
            // 애니메이션 발동
            Anim.SetBool("Dead", true);
            Anim_Leg.SetBool("Dead", true);

            // 오브젝트 비활성화 관련 코드
            Anim.transform.parent = null;
            this.enabled = false;
            rb.isKinematic = true;

            // 게임 매니저를 통해서 게임오버 시키기
            GameManager_Project.instance.gameOver = true;
            GameManager_Project.instance.GameOver.SetActive(true);

            // 콜라이더, 트렌스폼 비활성화
            gameObject.GetComponent<Collider>().enabled = false;
            Vector3 pos = Anim.transform.position;
            pos.y = 0.2f;
            Anim.transform.position = pos;

            // 사망 사운드 랜덤 출력
            int randomIndex = Random.Range(0, PlayerDeadSFX.Length);
            PlayerDeadSFX[randomIndex].Play();

            // 신음소리 출력 중지
            PlayerDangerSFX.Stop();

            // -값 안 보이게 고정시키기
            GameManager_Project.instance.HealthNum.text = $"0 / 100";
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

            PlayerDangerSFX.Stop();
        }
    }
}
