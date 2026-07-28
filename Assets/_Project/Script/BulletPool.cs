using System.Collections.Generic;
using UnityEngine;

namespace _2DTopDown
{
    public static class BulletPool
    {
        private static readonly Dictionary<GameObject, Queue<Bullet>> Pools = new Dictionary<GameObject, Queue<Bullet>>();
        private static Transform poolRoot;

        // 이 메소드는 BulletPool.Spawn(prefab, position, rotation)으로 총알을 재사용하는 구조
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
                return null;

            if (!Pools.TryGetValue(prefab, out Queue<Bullet> pool))
            {
                // 프리팹별로 각 총알(Queue<Bullet>)을 풀(Pool)로 관리한다.
                pool = new Queue<Bullet>();
                Pools.Add(prefab, pool);
            }

            Bullet bullet = GetBulletFromPool(pool);
            if (bullet == null)
                bullet = CreateBullet(prefab);

            if (bullet == null)
                return null;

            Transform bulletTransform = bullet.transform;
            bulletTransform.SetPositionAndRotation(position, rotation);
            bullet.gameObject.SetActive(true);

            return bullet.gameObject;
        }

        public static void Release(Bullet bullet)
        {
            if (bullet == null || bullet.IsReleased)
                return;

            GameObject prefab = bullet.SourcePrefab;
            if (prefab == null)
            {
                Object.Destroy(bullet.gameObject);
                return;
            }

            bullet.MarkReleased();
            bullet.gameObject.SetActive(false);
            bullet.transform.SetParent(GetPoolRoot(), false);
            if (!Pools.TryGetValue(prefab, out Queue<Bullet> pool))
            {
                pool = new Queue<Bullet>();
                Pools.Add(prefab, pool);
            }

            pool.Enqueue(bullet);
        }

        // 총알 생성할 때 총알 프리팹에 Bullet 스크립트가 없으면 에러 로그를 보낸다.
        private static Bullet CreateBullet(GameObject prefab)
        {
            GameObject instance = Object.Instantiate(prefab, GetPoolRoot());
            Bullet bullet = instance.GetComponent<Bullet>();
            if (bullet == null)
            {
                Object.Destroy(instance);
                Debug.LogError($"{prefab.name} 프리팹에 Bullet 스크립트가 없습니다.");
                return null;
            }

            bullet.SetSourcePrefab(prefab);
            instance.SetActive(false);
            return bullet;
        }

        private static Bullet GetBulletFromPool(Queue<Bullet> pool)
        {
            while (pool.Count > 0)
            {
                Bullet bullet = pool.Dequeue();
                if (bullet != null)
                    return bullet;
            }

            return null;
        }

        //씬 안에 [Bullet Pool] 오브젝트를 자동 생성해서 비활성화된(적이나 물체가 부딪힌) 총알 보관한다.
        private static Transform GetPoolRoot()
        {
            if (poolRoot != null)
                return poolRoot;

            Pools.Clear();
            poolRoot = new GameObject("[Bullet Pool]").transform;
            return poolRoot;
        }
    }
}
