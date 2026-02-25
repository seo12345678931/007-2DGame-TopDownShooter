using UnityEngine;

namespace _2DTopDown
{
    public class LandMine : MonoBehaviour
    {
        [Header("폭파 이펙트를 위한 애니메이션")]
        public Animator Anim;

        public float Damage;

        private void OnTriggerEnter(Collider other)
        {
            // 플레이어가 트리거에 닿으면 데미지를 입히고 삭제
            if(other.CompareTag("Player"))
            {
                Anim.SetTrigger("isBoom");
                other.gameObject.GetComponent <Player>().DamagePlayer(Damage);
                Destroy(gameObject, 0.3f);  // 0.3초 뒤에 삭제
            }

            // 플레이어 총알이 트리거에 닿으면 바로 파괴
            if(other.CompareTag("P_Bullet"))
            {
                Anim.SetTrigger("isBoom");
                Destroy(gameObject, 0.3f);
            }
        }
    }
}
