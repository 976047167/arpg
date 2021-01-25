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
		public static void Init()
		{
			if(_init) return;
			m_typeMap = new Dictionary<int, Type>();
			Assembly asm = Assembly.GetExecutingAssembly();
			Type[] types = asm.GetTypes();
			foreach (var t in types)
			{
				if (t.BaseType != typeof(GameActionBase)) continue;
				Debug.WriteLine(t.Name);
				object[] attrbutes = t.GetCustomAttributes(t, false);
				foreach (var atr in attrbutes)
				{
					if (atr is GameActionType)
					{
						GameActionType ret = (GameActionType)atr;
						int idx = (int)ret.type;
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
			int animatorIdx = -1;
			Type classType = action.GetType();
			//先在缓存中找是否可以直接读取
			cacheAnimatorIdx.TryGetValue(actionType,out animatorIdx);
			//缓存中没找到则反射读取attribute
			if (animatorIdx == -1 )
			{
				var atrArry = classType.GetCustomAttributes(typeof(AnimatorIndex), true);
				if (atrArry.Length > 0)
				{
					var attr = atrArry[0] as AnimatorIndex;
					animatorIdx = attr.idx; ;
				}
			}
			cacheAnimatorIdx[actionType] = animatorIdx;
			action.AnimatorIndex = animatorIdx;

		}
	}
}