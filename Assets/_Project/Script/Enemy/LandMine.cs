using UnityEngine;

namespace _2DTopDown
{
    public class LandMine : MonoBehaviour
    {
        [Header("폭파 이펙트를 위한 애니메이션")]
        public Animator Anim;

        [Header("피해량")]
        public float Damage;

        [Header("사운드")]
        public AudioSource Explosive_SFX;

        // 폭발 후 피격여부를 체크하는 변수(컴포넌트에서 확인하기 위해 전역변수로 설정함)
        public bool isExploded = false;

        private void OnTriggerEnter(Collider other)
        {
            // 이미 폭발 중이라면 더 이상 아래 코드를 실행하지 않음
            if (isExploded) return;

            // 트리거에 닿으면 적 & 플레이어 가리지 않고 데미지를 입혀서 삭제
            if (other.CompareTag("Player"))
            {
                isExploded = true; // 중복 실행 방지를 위해 즉시 true로 변경

                Anim.SetTrigger("isBoom");

                // 자식 객체는 남기기
                Anim.transform.parent = null;
                Vector3 pos = Anim.transform.position;
                pos.y = 0.5f;
                Anim.transform.position = pos;

                Explosive_SFX.Play();
                AlertEnemies();

                other.gameObject.GetComponent <Player>().DamagePlayer(Damage);
                Destroy(gameObject, 0.8f);
            }
            else if (other.CompareTag("Enemy"))
            {
                isExploded = true; // 중복 실행 방지를 위해 즉시 true로 변경

                Anim.SetTrigger("isBoom");

                Anim.transform.parent = null;
                Vector3 pos = Anim.transform.position;
                pos.y = 0.5f;
                Anim.transform.position = pos;

                Explosive_SFX.Play();

                other.gameObject.GetComponent<Enemy_Info>().TakeDamage(Damage * 2);
                GameManager_Project.instance.AddScore(50);

                Destroy(gameObject, 0.8f);
            }
            // 에러방지를 위해 체력이 담겨 있는 않은 물체(총알)가 트리거에 닿으면 바로 파괴
            else if (other.CompareTag("P_Bullet"))
            {
                isExploded = true;

                Anim.SetTrigger("isBoom");

                Anim.transform.parent = null;
                Vector3 pos = Anim.transform.position;
                pos.y = 0.5f;
                Anim.transform.position = pos;

                Explosive_SFX.Play();

                GameManager_Project.instance.AddScore(50);
                GameManager_Project.instance.KillCount++;

                AlertEnemies();
                Destroy(gameObject, 0.8f);
            }
        }

        // 지뢰가 폭발하면 폭발지점에서 이동시키게 하기
        private void AlertEnemies()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, 15.0f, transform.up);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null && hit.collider.tag == "Enemy")
                {
                    hit.collider.GetComponent<Enemy_Info>().SetAlertPos(transform.position);
                }
            }
        }
    }
}
