﻿using System;
using System.Collections;
using System.Linq;
using Objects.Bullets;
using UnityEngine;
using Util;
using Util.Interfaces;

namespace Objects.Towers {
	public class Sniper : MonoBehaviour, ITower {

		public float Damage = 10;
		public float FireRate = 5;
		public float Range = 55;
		public GameObject Bullet;
		public Transform FirePoint;
		public TowerMode Mode = TowerMode.First;
		public Transform Top;
		
		private bool _canFire;
		private float _dot;
		private Coroutine _fireTimerCoroutine;
		private Transform _target;
		private Vector3 _direction;

		private const string EnemyTag = "Enemy";
		
		// Links CanFire to _canFire and OnCanFireEvent
		private bool CanFire {
			set {
				if (_canFire == value) return;

				_canFire = value;

				OnCanFireEvent?.Invoke(value);
			}
		}

		public void Start () {
			OnCanFireEvent += FiringHander;
			
			InvokeRepeating(nameof(FindTarget), 0, 0.066f);
		}

		public void Update () {
			if (_target != null) {
				_dot = Vector3.Dot((_target.position - Top.position).normalized, Top.forward);
				
				var targetPoint = _target.transform.position - Top.transform.position;
				var rotation = Quaternion.Slerp(Top.transform.rotation, Quaternion.LookRotation(targetPoint), 10 * Time.fixedDeltaTime);
				Top.transform.rotation = rotation;
				var y = Top.transform.eulerAngles.y;
				Top.transform.eulerAngles = new Vector3(0, y, 0);
			}
		}

		public void FindTarget () {
			var enemies = GameObject.FindGameObjectsWithTag(EnemyTag).ToList();

			if (enemies.Count > 0) {
				switch (Mode) {
					case TowerMode.First:
						_target = Core.EnemyArray
							.FirstOrDefault(enemy => Vector3.Distance(transform.position, enemy.transform.position) <= Range)?.transform;
						break;
					case TowerMode.Last:
						_target = SpawnPoint.EnemyArray
							.FirstOrDefault(enemy => Vector3.Distance(transform.position, enemy.transform.position) <= Range)?.transform;
						break;
					case TowerMode.Closest:
						_target = enemies
							.OrderBy(enemy => Vector3.Distance(transform.position, enemy.transform.position))
							.FirstOrDefault(enemy => Vector3.Distance(transform.position, enemy.transform.position) <= Range)?.transform;
						break;
					case TowerMode.Furthest:
						_target = enemies.OrderByDescending(enemy => Vector3.Distance(transform.position, enemy.transform.position))
							.FirstOrDefault(enemy => Vector3.Distance(transform.position, enemy.transform.position) <= Range)?.transform;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				
				CanFire = true;
			} else {
				CanFire = false;
			}
		}

		public void Fire () {
			if (_dot >= 0.7) {
				var bullet = Instantiate(Bullet, FirePoint.position, Quaternion.identity);
				bullet.GetComponent<BasicBullet>().InheritedProperties(_target, Damage);
			}
		}

		public IEnumerator FireTimer () {
			while (true) {
				yield return new WaitForSeconds(FireRate);
				
				Fire();
			}
		}

		void FiringHander (bool canFire) {
			if (canFire) {
				_fireTimerCoroutine = StartCoroutine(FireTimer());
			} else {
				StopCoroutine(_fireTimerCoroutine);
			}
		}

		// Set up OnCanFireEvent
		public delegate void OnCanFireDelegate (bool value);

		public static event OnCanFireDelegate OnCanFireEvent;

		private void OnDrawGizmos () {
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, Range);
		}
	}
}