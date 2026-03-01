using UnityEngine;

namespace _2DTopDown
{
    public class Anim_FootEvent : MonoBehaviour
    {
        [Header("레이캐스트 설정")]
        public float rayDistance = 1.5f; // 바닥까지의 거리
        public LayerMask groundLayer;    // 바닥으로 인식할 레이어 (선택 사항)

        public void FootStepEvent()
        {
            // 기본값은 0 (Street/기본 발소리)
            int stepIdx = 0;

            // 1. 레이캐스트 발사: 현재 위치에서 약간 위에서 아래 방향으로 쏩니다.
            // QueryTriggerInteraction.Collide를 넣어 트리거로 설정된 콜라이더도 감지하게 합니다.
            RaycastHit hit;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDistance, Physics.AllLayers, QueryTriggerInteraction.Collide))
            {
                // 2. 닿은 바닥의 태그 확인
                if (hit.collider.CompareTag("Grass"))
                {
                    stepIdx = 1; // 풀밭 소리
                }
                else if (hit.collider.CompareTag("Wood"))
                {
                    stepIdx = 2;
                }
                else if (hit.collider.CompareTag("Sand"))
                {
                    stepIdx = 3;
                }
                // 나중에 다른 바닥이 생기면 여기에 else if (hit.collider.CompareTag("Sand")) 등으로 확장 가능
            }

            // 3. 사운드 재생
            if (Player.instance != null && Player.instance.FootStep.Length > stepIdx)
            {
                // 기존 소리가 아직 재생 중이면 끊고 새로 재생 (선택 사항)
                if (Player.instance.FootStep[stepIdx].isPlaying)
                    Player.instance.FootStep[stepIdx].Stop();

                Player.instance.FootStep[stepIdx].Play();
            }
        }

        // 에디터 뷰에서 레이저가 잘 나가는지 확인하기 위한 시각화 코드
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, Vector3.down * rayDistance);
        }
    }
}
