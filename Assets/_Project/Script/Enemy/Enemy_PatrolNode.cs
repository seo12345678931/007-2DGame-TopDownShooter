using UnityEngine;
using System.Collections;

// 지정된 경로를 1바퀴 돌아 이동하고 싶을 때 사용할 스크립트
namespace _2DTopDown
{
		public class Enemy_PatrolNode : MonoBehaviour
		{
				[Header("다음 경로를 이동하기 위한 노드")]
				public Enemy_PatrolNode nextNode;

				public Vector3 NextPosition()
				{
						return nextNode.GetMovePosition();
				}

				void OnDrawGizmos()
				{
						Gizmos.color = Color.white;
						Gizmos.DrawSphere(transform.position, 0.25f);
						if (nextNode != null)
						{
								Gizmos.color = Color.cyan;
								Gizmos.DrawLine(GetMovePosition(), NextPosition());
						}
				}
				public Vector3 GetMovePosition()
				{
						return transform.position;
				}
		}
}
