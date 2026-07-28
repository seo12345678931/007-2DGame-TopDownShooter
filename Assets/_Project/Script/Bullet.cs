using UnityEngine;

namespace _2DTopDown
{
    public class Bullet : MonoBehaviour
    {
        public enum Target
        {
            Player,
            Enemy
        }

        public Target TriggerTarget;

        [Header("LifeTime이 지나면 총알 반환")]
        public float LifeTime;

        [Header("투사체 속도")]
        public float speed;

        [Header("투사체 피해량")]
        public float Damage;

        private bool isMoving;
        private Rigidbody bulletRigidbody;
        private Collider bulletCollider;

        public GameObject SourcePrefab { get; private set; }
        public bool IsReleased { get; private set; }

        public void SetSourcePrefab(GameObject sourcePrefab)
        {
            SourcePrefab = sourcePrefab;
        }

        public void MarkReleased()
        {
            IsReleased = true;
            CancelInvoke(nameof(ReturnToPool));
        }

        private void Awake()
        {
            bulletRigidbody = GetComponent<Rigidbody>();
            bulletCollider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            IsReleased = false;
            isMoving = true;

            if (bulletRigidbody != null)
            {
                bulletRigidbody.isKinematic = false;
                bulletRigidbody.linearVelocity = Vector3.zero;
                bulletRigidbody.angularVelocity = Vector3.zero;
            }

            if (bulletCollider != null)
                bulletCollider.enabled = true;

            if (LifeTime > 0f)
                Invoke(nameof(ReturnToPool), LifeTime);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(ReturnToPool));
        }

        private void Update()
        {
            if (isMoving)
                transform.Translate(transform.forward * speed, Space.World);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (TriggerTarget == Target.Enemy && (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Dummy")))
            {
                Enemy_Info enemy = other.gameObject.GetComponent<Enemy_Info>();
                if (enemy != null)
                    enemy.TakeDamage(Damage);

                ReturnToPool();
            }
            else if (TriggerTarget == Target.Player && other.gameObject.CompareTag("Player"))
            {
                Player player = other.gameObject.GetComponent<Player>();
                if (player != null)
                    player.DamagePlayer(Damage);

                ReturnToPool();
            }
            else if (other.gameObject.CompareTag("Object"))
            {
                isMoving = false;

                if (bulletRigidbody != null)
                    bulletRigidbody.isKinematic = true;

                if (bulletCollider != null)
                    bulletCollider.enabled = false;

                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            BulletPool.Release(this);
        }
    }
}
