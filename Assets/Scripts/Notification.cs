using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
public static class Notification
{
	private static Dictionary<Type,KeyValuePair<int, HashSet<int>>> BindMap = new Dictionary<Type,KeyValuePair<int,HashSet<int>>>();
	private static List<object> Callbacks = new List<object>();

	public static void CreateBinding(int EventHash, Action callback)
	{
		Type t = typeof(Action);
		AddBinding(t, EventHash, callback);
	}
	public static void Emit(int EventHash)
	{
		var bindings = GetBinding<Action>(EventHash);
		for (int i = 0; i < bindings.Length; i++)
		{
			bindings[i]();
		}
	}
	public static void RemoveBinding(int hash,Action callback)
	{
		int idx = Callbacks.IndexOf(callback);
		if(idx == -1)return;
		Type t = typeof(Action);
		RemoveBinding(t, idx);
	}


	public static void CreateBinding<T>(int EventHash, Action<T> callback)
	{
		Type t = typeof(Action<T>);
		AddBinding(t, EventHash, callback);
	}
	public static void Emit<T>(int EventHash, T arg)
	{
		var bindings = GetBinding<Action<T>>(EventHash);
		for (int i = 0; i < bindings.Length; i++)
		{
			bindings[i](arg);
		}
	}
	public static void RemoveBinding<T>(int hash,Action<T> callback)
	{
		int idx = Callbacks.IndexOf(callback);
		if(idx == -1)return;
		Type t = typeof(Action<T>);
		RemoveBinding(t, idx);
	}

	public static void CreateBinding<T1,T2>(int EventHash, Action<T1,T2> callback)
	{
		Type t = typeof(Action<T1,T2>);
		AddBinding(t, EventHash, callback);
	}
	public static void Emit<T1,T2>(int EventHash, T1 arg1,T2 arg2)
	{
		var bindings = GetBinding<Action<T1,T2>>(EventHash);
		for (int i = 0; i < bindings.Length; i++)
		{
			bindings[i](arg1,arg2);
		}
	}
	public static void RemoveBinding<T1,T2>(int hash,Action<T1,T2> callback)
	{
		int idx = Callbacks.IndexOf(callback);
		if(idx == -1)return;
		Type t = typeof(Action<T1,T2>);
		RemoveBinding(t, idx);
	}




	private static void RemoveBinding(Type t,int idx)
	{
		KeyValuePair<int,HashSet<int>> bindings;
		BindMap.TryGetValue(t, out bindings);
		if( bindings.Value== null )return;
		bindings.Value.Remove(idx);
	}
	private static void AddBinding(Type type, int hash, object callback)
	{
		//计算回调在列表中index;
		int idx = Callbacks.IndexOf(callback);
		if (idx == -1)
		{
			Callbacks.Add(callback);
			idx = Callbacks.Count-1;
		}

		KeyValuePair<int,HashSet<int>> bindings;
		if (BindMap.ContainsKey(type))
		{
			bindings = BindMap[type];
		}
		else
		{
			bindings = new KeyValuePair<int,HashSet<int>>(hash,new HashSet<int>()) ;
			BindMap.Add(type, bindings);
		}
		//将回调索引写入hashset
		bindings.Value.Add(idx);
	}
	private static T[] GetBinding<T>(int hash)
	{
		Type t = typeof(T);
		KeyValuePair<int,HashSet<int>> bindings = new KeyValuePair<int, HashSet<int>>();
		BindMap.TryGetValue(t, out bindings);
		if(bindings.Value == null)return new T[0];
		var set = bindings.Value;
		var ret = new T[set.Count];
		int i = 0;
		foreach (int idx in set)
		{
			ret[i++] = (T)Callbacks[idx];
		}
		return ret;
	}

}