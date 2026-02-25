using UnityEngine;

namespace _2DTopDown
{
    public class Bullet : MonoBehaviour
    {
        // 플레이어, 적 총알을 이 스크립트를 통일하고 확실한 구분을 위해 Enum으로 구분하기
        public enum Target 
        { 
            Player, 
            Enemy
        }
        public Target TriggerTarget;

        [Header("LifeTime이 지나면 총알 삭제")]
        public float LifeTime;

        [Header("투사체 속도")]
        public float speed;

        [Header("투사체 피해량")]
        public float Damage;

        // 총알 발사를 위한 bool 함수 코드만 제어를 하기 위해 지역변수로 설정함
        private bool isMoving;

        private void Start()
        {
            isMoving = true;
            Destroy(gameObject, LifeTime);  // 총알 삭제 ( 지정된 LifeTime이 지나면 )
        }

        // Update is called once per frame
        private void Update()
        {
            if(isMoving) transform.Translate(transform.forward * speed, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(TriggerTarget == Target.Enemy && other.CompareTag("Enemy") || other.CompareTag("Dummy"))
            {
                other.gameObject.GetComponent<Enemy_Info>().TakeDamage(Damage);
                Destroy(gameObject);
            }
            else if(TriggerTarget == Target.Player && other.CompareTag("Player"))
            {
                other.gameObject.GetComponent < Player>().DamagePlayer(Damage);
            }
            else if (other.CompareTag("Object"))
            {
                LifeTime = 0;
                gameObject.GetComponent<Rigidbody>().isKinematic = true;
                gameObject.GetComponent<Collider>().enabled = false;
                isMoving = false;
                Destroy(gameObject, LifeTime);
            }
        }
    }
}
