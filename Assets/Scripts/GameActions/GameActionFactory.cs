using System.Runtime.CompilerServices;
using System.Reflection;
using System;
using System.Diagnostics;
using System.Collections.Generic;
namespace GameAction
{
	public static class GameActionFactory
	{
		private static bool _init;
		private static Dictionary<int, Type> m_typeMap;
		private static Dictionary<int, int> cacheAnimatorIdx;
		private static Dictionary<int, int> cacheStartType;
		private static Dictionary<int, int> cacheStopType;
		public static void Init()
		{
			if(_init) return;
			m_typeMap = new Dictionary<int, Type>();
			cacheAnimatorIdx = new Dictionary<int, int>();
			Assembly asm = Assembly.GetExecutingAssembly();
			Type[] types = asm.GetTypes();
			foreach (var t in types)
			{
				if (t.BaseType != typeof(GameActionBase)) continue;
				Debug.WriteLine(t.Name);
				object[] attrbutes = t.GetCustomAttributes(typeof(GameActionType), false);
				foreach (var atr in attrbutes)
				{
					if (atr is GameActionType)
					{
						GameActionType ret = (GameActionType)atr;
						int idx = (int)ret.value;
						m_typeMap[idx] = t;
						break;
					}
				}
			}
			_init = true;
		}
		public static GameActionBase GetAction(ACTION_TYPE actionType)
		{
			if(!_init) Init();
			Type actionClass = null;
			m_typeMap.TryGetValue((int)actionType, out actionClass);
			if (actionClass == null)
			{
				Debug.WriteLine("create action failed! illgal type: " + actionType);
				return null;
			}
			GameActionBase ret =Activator.CreateInstance(actionClass) as GameActionBase;
			setActionValue(ret,(int)actionType);
			return ret;
		}
		private static void setActionValue(GameActionBase action,int actionType)
		{
			action.type =(ACTION_TYPE)actionType ;
			setAnimatorIdx(action);

		}
		private static void setAnimatorIdx(GameActionBase action)
		{
			int animatorIdx;
			int actionType = (int)action.type;
			Type classType = action.GetType();
			//先在缓存中找是否可以直接读取
			bool find = cacheAnimatorIdx.TryGetValue(actionType,out animatorIdx);
			//缓存中没找到则反射读取attribute
			if (!find )
			{
				var atrArry = classType.GetCustomAttributes(typeof(AnimatorIndex), true);
				if (atrArry.Length > 0)
				{
					var attr = atrArry[0] as AnimatorIndex;
					animatorIdx = attr.value; ;
				}
			}else{
				animatorIdx = -1;
			}
			cacheAnimatorIdx[actionType] = animatorIdx;
			action.AnimatorIndex = animatorIdx;
		}

		private static void setStartType(GameActionBase action)
		{
			int value;
			int actionType = (int)action.type;
			Type classType = action.GetType();
			//先在缓存中找是否可以直接读取
			bool find = cacheStartType.TryGetValue(actionType,out value);
			//缓存中没找到则反射读取attribute
			if (!find )
			{
				var atrArry = classType.GetCustomAttributes(typeof(StartType), true);
				if (atrArry.Length > 0)
				{
					var attr = atrArry[0] as StartType;
					value = (int)attr.value; ;
				}
			}else{
				value = 0;
			}
			cacheStartType[actionType] = value;
			action.StartType = (START_TYPE)value;
		}

		private static void setStopType(GameActionBase action)
		{
			int value;
			int actionType = (int)action.type;
			Type classType = action.GetType();
			//先在缓存中找是否可以直接读取
			bool find = cacheStopType.TryGetValue(actionType,out value);
			//缓存中没找到则反射读取attribute
			if (!find )
			{
				var atrArry = classType.GetCustomAttributes(typeof(StopType), true);
				if (atrArry.Length > 0)
				{
					var attr = atrArry[0] as StopType;
					value = (int)attr.value; ;
				}
			}else{
				value = 0;
			}
			cacheStopType[actionType] = value;
			action.StopType = (STOP_TYPE)value;
		}

	}
}