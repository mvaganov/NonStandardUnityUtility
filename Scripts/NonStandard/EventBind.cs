// code by michael vaganov, released to the public domain via the unlicense (https://unlicense.org/)
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace NonStandard {
	public class EventBind {
		public object target;
		public string methodName;
		public object value;

		public EventBind(object target, string methodName, object value = null) {
			this.target = target; this.methodName = methodName; this.value = value;
		}
		public EventBind(object target, string methodName) {
			this.target = target; this.methodName = methodName; value = null;
		}
		public UnityAction<T> GetAction<T>(object target, string methodName) {
			System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, methodName, new Type[] { typeof(T) });
			if (targetinfo == null) {
				// does it exist without an argument?
				targetinfo = UnityEvent.GetValidMethodInfo(target, methodName, Array.Empty<Type>());
				if (targetinfo == null) {
					Debug.LogError("no \"" + methodName + "\" (" + typeof(T).Name + ") in " + target.ToString());
					return null;
				}
				Debug.LogError("found "+methodName+" without parameters. You must create "+ methodName+"("+
					typeof(T)+") and use that.");
				targetinfo = null;
			}
			return Delegate.CreateDelegate(typeof(UnityAction<T>), target, targetinfo, false) as UnityAction<T>;
		}
		public UnityAction<T0,T1> GetAction<T0,T1>(object target, string methodName) {
			System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, methodName,
				new Type[] { typeof(T0), typeof(T1) });
			if (targetinfo == null) {
				// does it exist without an argument?
				targetinfo = UnityEvent.GetValidMethodInfo(target, methodName, Array.Empty<Type>());
				if (targetinfo == null) {
					Debug.LogError("no method \"" + methodName + "\" (" + typeof(T0).Name + ", " + typeof(T1).Name +
						") in " + target.ToString());
					return null;
				}
				Debug.LogError("found " + methodName + " without parameters. You must create " + methodName + "(" +
					typeof(T0).Name + ", " + typeof(T1).Name + ") and use that.");
				targetinfo = null;
			}
			return Delegate.CreateDelegate(typeof(UnityAction<T0,T1>), target, targetinfo, false) as UnityAction<T0,T1>;
		}
		public static bool IfNotAlready<T>(UnityEvent<T> @event, UnityEngine.Object target, string methodName) {
			if (@event == null) { throw new Exception("UnityEvent never allocated."); }
			for(int i = 0; i < @event.GetPersistentEventCount(); ++i) {
				if (@event.GetPersistentTarget(i) == target && @event.GetPersistentMethodName(i) == methodName) { return false; }
			}
			On(@event, target, methodName);
			return true;
		}
		public static bool IfNotAlready(UnityEvent @event, UnityEngine.Object target, UnityAction action) {
			for (int i = 0; i < @event.GetPersistentEventCount(); ++i) {
				if (@event.GetPersistentTarget(i) == target && @event.GetPersistentMethodName(i) == action.Method.Name) { return false; }
			}
			On(@event, target, action);
			return true;
		}
		public static bool IfNotAlready<T>(UnityEvent<T> @event, UnityEngine.Object target, UnityAction<T> action) {
			for (int i = 0; i < @event.GetPersistentEventCount(); ++i) {
				if (@event.GetPersistentTarget(i) == target && @event.GetPersistentMethodName(i) == action.Method.Name) { return false; }
			}
			On(@event, target, action);
			return true;
		}
		public static void On(UnityEvent @event, object target, UnityAction action) {
#if UNITY_EDITOR
			if (target != null) {
				new EventBind(target, action.Method.Name).Bind(@event);
				return;
			}
#endif
			@event.AddListener(action.Invoke);
		}
		public static void On<T>(UnityEventBase @event, object target, UnityAction<T> action, T value) where T : UnityEngine.Object {
#if UNITY_EDITOR
			if (target != null) {
				new EventBind(target, action.Method.Name, value).Bind(@event, value);
				return;
			}
#endif
			(@event as UnityEvent<T>).AddListener((a)=>action.Invoke(value));
		}
		public static void On<T>(UnityEvent<T> @event, object target, UnityAction<T> action) {
#if UNITY_EDITOR
			if (target != null) {
				new EventBind(target, action.Method.Name).Bind(@event);
				return;
			}
#endif
			@event.AddListener(action.Invoke);
		}
		public static void On<T>(UnityEvent<T> @event, object target, string methodName) {
			new EventBind(target, methodName).Bind(@event);
		}
		public bool IsAlreadyBound(UnityEventBase @event) {
			UnityEngine.Object obj = target as UnityEngine.Object;
			return obj != null && IsAlreadyBound(@event, obj, methodName);
		}
		public static bool IsAlreadyBound(UnityEventBase @event, UnityEngine.Object target, string methodName) {
			return GetPersistentEventIndex(@event, target, methodName) >= 0;
		}
		public static int GetPersistentEventIndex(UnityEventBase @event, UnityEngine.Object target, string methodName) {
			for (int i = 0; i < @event.GetPersistentEventCount(); ++i) {
				if (@event.GetPersistentTarget(i) == target && @event.GetPersistentMethodName(i) == methodName) { return i; }
			}
			return -1;
		}
		public static bool Remove(UnityEventBase @event, UnityEngine.Object target, string methodName) {
			int index = GetPersistentEventIndex(@event, target, methodName);
			if (index >= 0) {
				UnityEventTools.RemovePersistentListener(@event, index);
				return true;
            }
			return false;
		}
		public static void Clear(UnityEventBase @event) {
			while (@event.GetPersistentEventCount() > 0) {
				UnityEventTools.RemovePersistentListener(@event, @event.GetPersistentEventCount() - 1);
			}
		}
		public static bool IfNotAlready(UnityEvent @event, UnityEngine.Object target, string methodName) {
			if (IsAlreadyBound(@event, target, methodName)) return false;
			On(@event, target, methodName);
			return true;
		}
		public static void On(UnityEvent @event, object target, string methodName) {
			new EventBind(target, methodName).Bind(@event);
		}
		public static void On<T>(UnityEvent @event, object target, string methodName, T value) where T : UnityEngine.Object {
			new EventBind(target, methodName, value).Bind(@event, value);
		}
		public void Bind<T>(UnityEvent<T> @event) {
#if UNITY_EDITOR
			UnityEventTools.AddPersistentListener(@event, GetAction<T>(target, methodName));
#else
			System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, methodName, new Type[]{typeof(T)});
			@event.AddListener((val) => targetinfo.Invoke(target, new object[] { val }));
#endif
		}
		public void Bind<T0,T1>(UnityEvent<T0,T1> @event) {
#if UNITY_EDITOR
			UnityEventTools.AddPersistentListener(@event, GetAction<T0,T1>(target, methodName));
#else
			System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, methodName, new Type[] { typeof(T0), typeof(T1) });
			@event.AddListener((t0,t1) => targetinfo.Invoke(target, new object[] { t0, t1 }));
#endif
		}
		public void Bind(UnityEvent @event) {
#if UNITY_EDITOR
			if (value == null) {
				System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, methodName, new Type[0]);
				if (targetinfo == null) { Debug.LogError("no method " + methodName + "() in " + target.ToString()); }
				UnityAction action = Delegate.CreateDelegate(typeof(UnityAction), target, targetinfo, false) as UnityAction;
				UnityEventTools.AddVoidPersistentListener(@event, action);
			} else if (value is int) {
				UnityEventTools.AddIntPersistentListener(@event, GetAction<int>(target, methodName), (int)value);
			} else if (value is float) {
				UnityEventTools.AddFloatPersistentListener(@event, GetAction<float>(target, methodName), (float)value);
			} else if (value is string) {
				UnityEventTools.AddStringPersistentListener(@event, GetAction<string>(target, methodName), (string)value);
			} else if (value is bool) {
				UnityEventTools.AddBoolPersistentListener(@event, GetAction<bool>(target, methodName), (bool)value);
			} else if (value is GameObject go) {
				Bind<GameObject>(@event, go);
			} else if (value is Transform t) {
				Bind<Transform>(@event, t);
			} else {
				Debug.LogError("unable to assign " + value.GetType());
			}
#else
				System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, methodName, new Type[0]);
				@event.AddListener(() => targetinfo.Invoke(target, new object[] { value }));
#endif
		}
		public void Bind<T>(UnityEventBase @event, T value) where T : UnityEngine.Object {
#if UNITY_EDITOR
			if (value == null || value is T) {
				UnityEventTools.AddObjectPersistentListener(@event, GetAction<T>(target, methodName), value);
			} else {
				Debug.LogError("unable to assign " + value.GetType() +", expected "+typeof(T));
			}
#else
			// TODO test in compiled build
			System.Reflection.MethodInfo targetinfo = UnityEvent.GetValidMethodInfo(target, methodName, new Type[] { typeof(T) });
			@event.AddListener(() => targetinfo.Invoke(target, new object[] { value }));
#endif
		}
		public static List<EventBind> GetList(UnityEventBase @event) {
			List<EventBind> eb = new List<EventBind>();
			for(int i = 0; i <@event.GetPersistentEventCount(); ++i) {
				eb.Add(new EventBind(@event.GetPersistentTarget(i), @event.GetPersistentMethodName(i)));
			}
			return eb;
		}
		public static string DebugPrint(UnityEventBase @event) {
			List<EventBind> eb = GetList(@event);
			return string.Join(", ", eb);
		}
		public override string ToString() {
			string t = null;
			if (target is UnityEngine.Object o) { t = o.name; }
			else if (target != null) { t = target.ToString(); }
			return t+"."+methodName+"("+value+")";
		}
	}
}