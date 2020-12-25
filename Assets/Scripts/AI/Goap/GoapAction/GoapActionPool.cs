using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
namespace Goap.Action
{


	internal class GoapActionPool
	{

		private static GoapActionPool _instance;
		internal static GoapActionPool getInstance()
		{
			if (_instance == null) { _instance = new GoapActionPool(); }
			return _instance;
		}
		private Dictionary<int, GoapAction> actionPools;//不用enum，防止gc
		private Dictionary<int, IEnumerator<bool>[]> performancePools;//不用enum，防止gc
		private Dictionary<int, Type> typeMap;//不用enum，防止gc
		private GoapActionPool()
		{
			this.actionPools = new Dictionary<int, GoapAction>();
			this.performancePools = new Dictionary<int, IEnumerator<bool>[]>();
			Assembly asm = Assembly.GetExecutingAssembly();
			Type[] types = asm.GetTypes();
			foreach (var t in types)
			{
				if (t.BaseType != typeof(GoapAction)) continue;
				Console.WriteLine(t.Name);
				object[] attrbutes = t.GetCustomAttributes(t,false);
				foreach (var atr in attrbutes)
				{
					if(atr is GoapActionType){
						GoapActionType ret = (GoapActionType)atr;
						int idx = (int)ret.type;
						this.typeMap[idx] = t;
						break;
					}
				}

			}

		}
		public GoapAction getAction(ACTION_TYPE type)
		{

			GoapAction action;
			bool has = this.actionPools.TryGetValue((int)type, out action);
			if (!has )
			{
				action = this.createAction(type);
			}
			return action;
		}
		private GoapAction createAction(ACTION_TYPE actionType)
		{
			Type actionClass;
			this.typeMap.TryGetValue((int)actionType, out actionClass);
			if (actionClass == null)
			{
				System.Console.WriteLine("create action failed! illgal type: " + actionType);
				return null;
			}
			GoapAction ret =(GoapAction)Activator.CreateInstance(actionClass);
			ret.type = actionType;
			return ret;
		}
	}
}