using System;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard {
	public class Platform : MonoBehaviour {
		public static Platform Instance => Global.GetComponent<Platform>();
#if UNITY_EDITOR
		public UnityEvent onUnityEditorStart;
#endif
#if UNITY_EDITOR || UNITY_WEBPLAYER
		public UnityEvent onWebplayerStart;
#endif
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE
		public UnityEvent onMobileStart;
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

		[System.Serializable]
		public class PauseEvents {
			[Tooltip("do this when time is paused")] public UnityEvent onPause = new UnityEvent();
			[Tooltip("do this when time is unpaused")] public UnityEvent onUnpause = new UnityEvent();
		}

		void Start() {
#if UNITY_EDITOR
			onUnityEditorStart?.Invoke();
#endif
#if UNITY_WEBPLAYER
			onWebplayerStart?.Invoke();
#endif
#if UNITY_ANDROID || UNITY_IPHONE
			onMobileStart?.Invoke();
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
			originalTimeScale = Time.timeScale;
			Time.timeScale = 0;
		}
		public void UnfreezeTime() {
			Time.timeScale = originalTimeScale;
		}
	}
}