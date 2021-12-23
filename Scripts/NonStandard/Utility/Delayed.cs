using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard.Utility {
	public class Delayed : MonoBehaviour {
		public UnityEvent afterStart;
		private static Delayed _instance;
		public static Delayed Instance {
			get {
				if (_instance) return _instance;
				_instance = FindObjectOfType<Delayed>();
				if (_instance == null) {
					GameObject go = new GameObject(nameof(Delayed));
					_instance = go.AddComponent<Delayed>();
                }
				return _instance;
            }
        }
		void Start() {
			if (afterStart != null && afterStart.GetPersistentEventCount() > 0) {
				DelayFrames(1, () => afterStart.Invoke());
			}
		}
		public void DelayFrames(int frameCount, Action action) {
			StartCoroutine(DelayedEvent()); IEnumerator DelayedEvent() {
				for (int i = 0; i < frameCount; ++i) {
					yield return new WaitForEndOfFrame();
				}
				action.Invoke();
			}
		}
		public void DelaySeconds(float seconds, Action action) {
			StartCoroutine(DelayedEvent()); IEnumerator DelayedEvent() {
				yield return new WaitForSeconds(seconds);
				action.Invoke();
			}
        }
		public static void Frames(int frameCount, Action action) {
			Instance.DelayFrames(frameCount, action);
        }
		public static void Seconds(float seconds, Action action) {
			Instance.DelaySeconds(seconds, action);
		}
	}
}