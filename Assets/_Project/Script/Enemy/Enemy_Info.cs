using System.Collections;
using UnityEngine;

namespace _2DTopDown
{
    public enum Enemy_EnemyState
    {
        IDLE_STATIC,
        IDLE_ROAMER,
        IDLE_PATROL,
        INSPECT,
        ATTACK,
        FIND_WEAPON,
        KNOCKED_OUT,
        DEAD,
        NONE
    }
    public enum Enemy_WeaponType
    {
        None,
        Melee,
        SMG,
        Shotgun,
        LMG
    }
    public class Enemy_Info : MonoBehaviour
    {
        [Header("콜라이더")]
        [Tooltip("적이 죽을 시 처치된 모습으로 남아 콜라이더를 비활성화 시키기")]
        public Collider EnemyCollider;

        [Header("적 체력")]
        public float MaxHp;
        public float CurrentHp;

        [Header("스프라이트 피격 연출")]
        public SpriteRenderer SprRand;

        [Header("사운드")]
        public AudioSource HitSound;
        public AudioSource[] DownSound; // 적 & 보스 사운드를 나누기 위해 배열로 변경
        public AudioSource[] FootStep;  // 적도 플레이어에 맞게 여러 발소리 추가예정
        public AudioSource[] WeaponFiringSFX;

        [Header("인공지능 관련(수색시간 & 내비메쉬 에이전트)")]
        public float inspectTimeout;
        public UnityEngine.AI.NavMeshAgent navMeshAgent;

        [Header("애니메이션")]
        public Animator NPC_Anim;

        [Header("적 총알 투사체")]
        public GameObject[] proyectilePrefab;

        [Header("파티클 (머즐 플래시 & 총구연기)")]
        public GameObject[] MuzzleFlashs;
        public GameObject GunSmoke;

        delegate void InitState();
        delegate void UpdateState();
        delegate void EndState();
        InitState _initState;
        InitState _updateState;
        InitState _endState;

        [Header("적 유형")]
        public Enemy_WeaponType weaponType = Enemy_WeaponType.Melee;
        public Enemy_EnemyState idleState = Enemy_EnemyState.IDLE_ROAMER;
        Enemy_EnemyState currentState = Enemy_EnemyState.NONE;

        Vector3 targetPos, startingPos;
        public LayerMask hitTestLayer;
        private float weaponRange;  // 무기 발사거리

        [Header("적 총구 위치")]
        public Transform[] weaponPivot;

        private float weaponActionTime, weaponTime; // 발사간격, 쿨타임?
        int hashSpeed;

        [Header("Nav를 통한 적 순찰경로")]
        public Enemy_PatrolNode patrolNode;

        private void Start()
        {
            startingPos = transform.position;
            hashSpeed = Animator.StringToHash("Speed");
            SetWeapon(weaponType);
            SetState(idleState);
            CurrentHp = MaxHp;
            EnemyCollider.enabled = true;
        }
        public void SetWeapon(Enemy_WeaponType newWeapon)
        {
            NPC_Anim.SetTrigger("WeaponChange");
            NPC_Anim.SetInteger("WeaponType", (int)weaponType);
            switch (weaponType)
            {
                case Enemy_WeaponType.None:
                    weaponRange = 0;
                    weaponActionTime = 0;
                    weaponTime = 0;
                    break;
                case Enemy_WeaponType.Melee:
                    weaponRange = 1.0f;
                    weaponActionTime = 0.2f;
                    weaponTime = 0.4f;
                    break;
                case Enemy_WeaponType.SMG:
                    weaponRange = 20.0f;
                    weaponActionTime = 0.025f;
                    weaponTime = 0.05f;
                    break;
                case Enemy_WeaponType.Shotgun:
                    weaponRange = 10.0f;
                    weaponActionTime = 0.35f;
                    weaponTime = 0.75f;
                    break;
                case Enemy_WeaponType.LMG:
                    weaponRange = 30.0f;
                    weaponActionTime = 0.020f;
                    weaponTime = 0.05f;
                    break;
            }
        }

        public void Update()
        {
            if (currentState == Enemy_EnemyState.DEAD || currentState == Enemy_EnemyState.NONE) return;

            _updateState();

            NPC_Anim.SetFloat(hashSpeed, navMeshAgent.velocity.magnitude);
        }

        public void SetState(Enemy_EnemyState newState)
        {
            if (currentState != newState)
            {
                if (_endState != null)
                    _endState();
                switch (newState)
                {
                    case Enemy_EnemyState.IDLE_STATIC:
                        _initState = StateInit_IdleStatic;
                        _updateState = StateUpdate_IdleStatic;
                        _endState = StateEnd_IdleStatic;
                        break;
                    case Enemy_EnemyState.IDLE_ROAMER:
                        _initState = StateInit_IdleRoamer;
                        _updateState = StateUpdate_IdleRoamer;
                        _endState = StateEnd_IdleRoamer;
                        break;
                    case Enemy_EnemyState.IDLE_PATROL:
                        _initState = StateInit_IdlePatrol;
                        _updateState = StateUpdate_IdlePatrol;
                        _endState = StateEnd_IdlePatrol;
                        break;
                    case Enemy_EnemyState.INSPECT:
                        _initState = StateInit_Inspect;
                        _updateState = StateUpdate_Inspect;
                        _endState = StateEnd_Inspect;
                        break;
                    case Enemy_EnemyState.ATTACK:
                        _initState = StateInit_Attack;
                        _updateState = StateUpdate_Attack;
                        _endState = StateEnd_Attack;
                        break;
                    case Enemy_EnemyState.NONE:
                        _initState = StateInit_None;
                        _updateState = StateUpdate_None;
                        _endState = StateEnd_None;
                        break;
                }
                _initState();
                currentState = newState;
            }
        }

        private void UpdateSensors()
        {

        }

        private void StateInit_None()
        {
            navMeshAgent.isStopped = true;
        }
        private void StateUpdate_None()
        {

        }
        private void StateEnd_None()
        {

        }

        ///////////////////////////////////////////////////////// STATE: IDLE STATIC
        private void StateInit_IdleStatic()
        {
            navMeshAgent.SetDestination(startingPos);
            navMeshAgent.isStopped = false;
        }
        private void StateUpdate_IdleStatic()
        {

        }
        private void StateEnd_IdleStatic()
        {

        }

        ///////////////////////////////////////////////////////// STATE: IDLE PATROL
        private void StateInit_IdlePatrol()
        {
            navMeshAgent.speed = 6.0f;
            navMeshAgent.SetDestination(patrolNode.GetMovePosition());
        }
        private void StateUpdate_IdlePatrol()
        {
            if (HasReachedMyDestination())
            {
                patrolNode = patrolNode.nextNode;
                navMeshAgent.SetDestination(patrolNode.GetMovePosition());
            }

        }
        private void StateEnd_IdlePatrol()
        {

        }

        ///////////////////////////////////////////////////////// STATE: IDLE ROAMER
        Misc_Timer idleTimer = new Misc_Timer();
        Misc_Timer idleRotateTimer = new Misc_Timer();
        bool idleWaiting, idleMoving;
        private void StateInit_IdleRoamer()
        {
            navMeshAgent.speed = 7.0f;

            idleTimer.StartTimer(Random.Range(2.0f, 4.0f));
            RandomRotate();
            AdvanceIdle();
            idleWaiting = false;
            idleMoving = true;
        }
        private void StateUpdate_IdleRoamer()
        {

            idleTimer.UpdateTimer();

            if (idleMoving)
            {
                if (HasReachedMyDestination())
                {
                    AdvanceIdle();

                }
            }
            else if (idleWaiting)
            {
                idleRotateTimer.UpdateTimer();
                if (idleRotateTimer.IsFinished())
                {
                    RandomRotate();
                    idleRotateTimer.StartTimer(Random.Range(1.5f, 3.25f));
                }

            }
            if (idleTimer.IsFinished())
            {
                if (idleMoving)
                {
                    navMeshAgent.isStopped = true;
                    float waitTime = Random.Range(2.5f, 6.5f);
                    float randomTurnTime = waitTime / 2.0f;
                    idleRotateTimer.StartTimer(randomTurnTime);
                    idleTimer.StartTimer(waitTime);


                }
                else if (idleWaiting)
                {
                    idleTimer.StartTimer(Random.Range(2.0f, 4.0f));

                    AdvanceIdle();
                }
                idleMoving = !idleMoving;
                idleWaiting = !idleMoving;
            }

        }
        private void StateEnd_IdleRoamer()
        {

        }


        void RayDebug()
        {
            RaycastHit hit = new RaycastHit();
            Physics.Raycast(transform.position, transform.forward * 5.0f, out hit, 50.0f, hitTestLayer);

            Debug.DrawLine(transform.position, hit.point, Color.red);
            Vector3 dir = hit.point - transform.position;
            Vector3 reflectedVector = Vector3.Reflect(dir, hit.normal);
            Debug.DrawRay(hit.point, reflectedVector * 5.0f, Color.green);
        }

        private void AdvanceIdle()
        {
            RaycastHit hit = new RaycastHit();
            Physics.Raycast(transform.position, transform.forward * 5.0f, out hit, 50.0f, hitTestLayer);
            //Debug.DrawRay (transform.position, transform.forward, Color.red);

            if (hit.distance < 3.0f)
            {
                Vector3 dir = hit.point - transform.position;
                Vector3 reflectedVector = Vector3.Reflect(dir, hit.normal);
                Physics.Raycast(transform.position, reflectedVector, out hit, 50.0f, hitTestLayer);
            }
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(hit.point);
        }

        ///////////////////////////////////////////////////////// STATE: INSPECT
        Misc_Timer inspectTimer = new Misc_Timer();
        Misc_Timer inspectTurnTimer = new Misc_Timer();
        bool inspectWait;
        private void StateInit_Inspect()
        {
            navMeshAgent.speed = 16.0f;
            navMeshAgent.isStopped = false;
            inspectTimer.StopTimer();
            inspectWait = false;
        }
        private void StateUpdate_Inspect()
        {
            if (HasReachedMyDestination() && !inspectWait)
            {
                inspectWait = true;
                inspectTimer.StartTimer(2.0f);
                inspectTurnTimer.StartTimer(1.0f);
            }
            navMeshAgent.SetDestination(targetPos);
            RaycastHit hit = new RaycastHit();
            Physics.Raycast(transform.position, transform.forward, out hit, weaponRange, hitTestLayer);

            if (hit.collider != null && hit.collider.tag == "Player")
            {
                SetState(Enemy_EnemyState.ATTACK);
            }
            if (inspectWait)
            {
                inspectTimer.UpdateTimer();
                inspectTurnTimer.UpdateTimer();
                if (inspectTurnTimer.IsFinished())
                {
                    RandomRotate();
                    inspectTurnTimer.StartTimer(Random.Range(0.5f, 1.25f));
                }
                if (inspectTimer.IsFinished())
                    SetState(idleState);
            }
        }
        private void StateEnd_Inspect()
        {

        }

        ///////////////////////////////////////////////////////// STATE: ATTACK
        Misc_Timer attackActionTimer = new Misc_Timer();
        bool actionDone;
        private void StateInit_Attack()
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
            NPC_Anim.SetBool("Attack", true);
            CancelInvoke("AttackAction");
            Invoke("AttackAction", weaponActionTime);
            attackActionTimer.StartTimer(weaponTime);

            actionDone = false;
        }
        private void StateUpdate_Attack()
        {
            attackActionTimer.UpdateTimer();
            if (!actionDone && attackActionTimer.IsFinished())
            {
                EndAttack();

                actionDone = true;
            }
        }
        private void StateEnd_Attack()
        {
            NPC_Anim.SetBool("Attack", false);
        }
        private void EndAttack()
        {
            SetState(Enemy_EnemyState.INSPECT);
        }
        private void AttackAction()
        {
            switch (weaponType)
            {
                case Enemy_WeaponType.Melee:
                    RaycastHit[] hits = Physics.SphereCastAll(weaponPivot[0].position, 2.0f, weaponPivot[0].forward);
                    WeaponFiringSFX[0].Play();
                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.collider != null && hit.collider.tag == "Player")
                        {
                            hit.collider.GetComponent<Player>().DamagePlayer(45);
                        }
                    }
                    break;
                case Enemy_WeaponType.SMG:
                    CreateMuzzleFlash(0);
                    GameObject bullet = Instantiate(proyectilePrefab[0], weaponPivot[1].position, weaponPivot[1].rotation);
                    bullet.transform.Rotate(0, Random.Range(-7.5f, 7.5f), 0);
                    WeaponFiringSFX[1].Play();
                    break;
                case Enemy_WeaponType.Shotgun:
                    CreateMuzzleFlash(1);
                    for (int i = 0; i < 5; i++)
                    {
                        GameObject birdshot = Instantiate(proyectilePrefab[1], weaponPivot[0].position, weaponPivot[0].rotation);
                        birdshot.transform.Rotate(0, Random.Range(-15, 15), 0);
                    }
                    WeaponFiringSFX[2].Play();
                    break;
                case Enemy_WeaponType.LMG:
                    CreateMuzzleFlash(0);
                    GameObject bullet_LMG = Instantiate(proyectilePrefab[2], weaponPivot[2].position, weaponPivot[2].rotation);
                    bullet_LMG.transform.Rotate(0, Random.Range(-3.5f, 3.5f), 0);
                    WeaponFiringSFX[3].Play();
                    break;
            }
        }

        private void CreateMuzzleFlash(int index)
        {
            // 배열 범위를 벗어나지 않는지 확인하고 프리팹이 있는지 체크
            if (MuzzleFlashs != null && MuzzleFlashs.Length > index && MuzzleFlashs[index] != null)
            {
                // FireArmsPivot 위치와 회전값으로 생성
                GameObject flash = Instantiate(MuzzleFlashs[index], weaponPivot[index].position, weaponPivot[index].rotation);

                // 총구 연막생성
                GameObject FireSmoke = Instantiate(GunSmoke, weaponPivot[index].position, weaponPivot[index].rotation);

                // 총구 위치를 계속 따라가게 하려면 부모를 설정 (선택 사항)
                flash.transform.SetParent(weaponPivot[index]);
                FireSmoke.transform.SetParent(weaponPivot[index]);

                // 머즐 플래시 회전값 설정
                flash.transform.Rotate(0, 180, 0);
                FireSmoke.transform.Rotate(0, 180, 0);

                // 아주 짧은 시간 뒤에 자동 삭제 (0.15초)
                Destroy(flash, 0.15f);
                Destroy(FireSmoke, 0.5f);
            }
        }

        ////////////////////////// MISC FUNCTIONS //////////////////////////
        private void RandomRotate()
        {
            float randomAngle = Random.Range(45, 180);
            float randomSign = Random.Range(0, 2);
            if (randomSign == 0)
                randomAngle *= -1;

            transform.Rotate(0, randomAngle, 0);
        }

        public bool HasReachedMyDestination()
        {
            float dist = Vector3.Distance(transform.position, navMeshAgent.destination);
            if (dist <= 1.5f)
            {
                return true;
            }

            return false;
        }
        ////////////////////////// PUBLIC FUNCTIONS //////////////////////////
        public void SetAlertPos(Vector3 newPos)
        {
            if (idleState != Enemy_EnemyState.NONE)
            {
                SetTargetPos(newPos);
            }
        }
        public void SetTargetPos(Vector3 newPos)
        {
            targetPos = newPos;
            if (currentState != Enemy_EnemyState.ATTACK)
            {
                SetState(Enemy_EnemyState.INSPECT);
            }
        }

        public void TakeDamage(float DMG)
        {
            CurrentHp -= DMG;
            StartCoroutine(HitEffect());
            HitSound.Play();

            if (CurrentHp <= 0)
            {
                Died();
            }
        }
        public IEnumerator HitEffect()
        {
            SprRand.material.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            SprRand.material.color = Color.white;
        }

        public void Died()
        {
            // 네비메쉬 중지
            navMeshAgent.velocity = Vector3.zero;

            if(weaponType == Enemy_WeaponType.Melee || weaponType == Enemy_WeaponType.None)
            {
                // 애니메이션 작동
                NPC_Anim.SetBool("Dead", true);
                DownSound[0].Play();
                // 점수 & 처치 수 증가
                GameManager_Project.instance.AddScore(100);
            }
            // 산탄총이나 기관단총이 든 적이라면 110점 추가
            else if(weaponType == Enemy_WeaponType.SMG || weaponType == Enemy_WeaponType.Shotgun)
            {
                NPC_Anim.SetBool("Dead", true);
                DownSound[0].Play();
                GameManager_Project.instance.AddScore(110);
            }
            else if(weaponType == Enemy_WeaponType.LMG)
            {
                // 애니메이션 작동
                NPC_Anim.SetBool("Dead_Boss", true);
                DownSound[1].Play();
                GameManager_Project.instance.AddScore(200);
            }
            GameManager_Project.instance.KillCount++;

            // "Randerer"라는 자식객체만 남고 그 자리에서 Dead 애니메이션 작동
            NPC_Anim.transform.parent = null;
            Vector3 pos = NPC_Anim.transform.position;
            pos.y = 0.5f;
            NPC_Anim.transform.position = pos;

            // 위에 transform.parent를 설정한 것 때문인지 자식 객체인 Randerer를 제외한 모두 삭제했음.
            Destroy(gameObject);
        }
    }
}
