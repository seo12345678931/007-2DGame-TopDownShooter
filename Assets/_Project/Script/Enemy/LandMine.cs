using UnityEngine;

namespace _2DTopDown
{
    public class LandMine : MonoBehaviour
    {
        [Header("폭파 이펙트")]
        public GameObject ExplosiveEffect;

        public float Damage;

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                print("지뢰격발!");
                other.gameObject.GetComponent <Player>().DamagePlayer(Damage);
            }
        }
    }
}
