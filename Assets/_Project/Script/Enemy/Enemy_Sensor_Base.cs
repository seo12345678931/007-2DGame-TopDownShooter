using UnityEngine;
using System.Collections.Generic;

namespace _2DTopDown
{
		public class NPCSensor_Base : MonoBehaviour
		{
				public Enemy_Info EnemyBase;
				protected List<GameObject> sensedObjects = new List<GameObject>();

				void Start()
				{
						if (EnemyBase == null)
								EnemyBase = gameObject.GetComponent<Enemy_Info>();
						StartSensor();
				}

				void Update()
				{
						UpdateSensor();
				}
				protected virtual void StartSensor() { }
				protected virtual void UpdateSensor() { }

				protected List<GameObject> GetSensedObjects()
				{
						return sensedObjects;
				}
		}
}

