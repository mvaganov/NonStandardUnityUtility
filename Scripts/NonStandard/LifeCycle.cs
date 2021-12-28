using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace NonStandard {
	public class LifeCycle : MonoBehaviour {
		public static LifeCycle Instance => Global.GetComponent<LifeCycle>();
#if UNITY_EDITOR
		public LifeCycleEvents lifeCycleEditor = new LifeCycleEvents();
#endif
#if UNITY_EDITOR || UNITY_WEBPLAYER
		public LifeCycleEvents lifeCycleWebPlayer = new LifeCycleEvents();
#endif
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE
		public LifeCycleEvents lifeCycleMobile = new LifeCycleEvents();
#endif
		public PauseEvents pauseEvents = new PauseEvents();
		private bool _isPaused;
		private float originalTimeScale = 1;
		public bool isPaused {
			get => _isPaused;
			set {
				if (value == isPaused) return;
				if (value) { Pause(); } else { Unpause(); }
            }
        }
		[System.Serializable] public class LifeCycleEvents {
			public UnityEvent onStart;
			public UnityEvent onDestroy;
		}
		[System.Serializable] public class PauseEvents {
			[Tooltip("do this when time is paused")] public UnityEvent onPause = new UnityEvent();
			[Tooltip("do this when time is unpaused")] public UnityEvent onUnpause = new UnityEvent();
		}

		void Start() {
#if UNITY_EDITOR
				lifeCycleEditor.onStart.Invoke();
#endif
#if UNITY_WEBPLAYER
				lifeCycleWebPlayer.onStart.Invoke();
#endif
#if UNITY_ANDROID || UNITY_IPHONE
				lifeCycleMobile.onStart.Invoke();
#endif
		}
		private void OnDestroy() {
#if UNITY_EDITOR
				lifeCycleEditor.onDestroy.Invoke();
#endif
#if UNITY_WEBPLAYER
				lifeCycleWebPlayer.onDestroy.Invoke();
#endif
#if UNITY_ANDROID || UNITY_IPHONE
				lifeCycleMobile.onDestroy.Invoke();
#endif
		}
		private void Reset() {
#if UNITY_EDITOR
			// bind the callbacks the long way, since NonStandard.EventBind is not a required library
			System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(this, nameof(FreezeTime), Type.EmptyTypes);
			UnityAction action = Delegate.CreateDelegate(typeof(UnityAction), this, targetinfo, false) as UnityAction;
			UnityEventTools.AddVoidPersistentListener(pauseEvents.onPause, action);
			targetinfo = UnityEvent.GetValidMethodInfo(this, nameof(UnfreezeTime), Type.EmptyTypes);
			action = Delegate.CreateDelegate(typeof(UnityAction), this, targetinfo, false) as UnityAction;
			UnityEventTools.AddVoidPersistentListener(pauseEvents.onUnpause, action);
#else
			pauseEvents.onPause.AddListener(FreezeTime);
			pauseEvents.onUnpause.AddListener(UnfreezeTime);
#endif
		}
		public void Quit() { Exit(); }

		public static void Exit() {
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
			Application.OpenURL(webplayerQuitURL);
#else
			Application.Quit();
#endif
		}
		public void Pause() {
			_isPaused = true;
			if (pauseEvents.onPause != null) { pauseEvents.onPause.Invoke(); }
		}
		public void Unpause() {
			_isPaused = false;
			if (pauseEvents.onUnpause != null) { pauseEvents.onUnpause.Invoke(); }
		}
		public void FreezeTime() {
			if (Time.timeScale == 0) { return; }
			originalTimeScale = Time.timeScale;
			Time.timeScale = 0;
		}
		public void UnfreezeTime() {
			Time.timeScale = originalTimeScale;
		}
	}
}