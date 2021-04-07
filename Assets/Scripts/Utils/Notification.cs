using System.Collections;
using System.Collections.Generic;
using System;
public static class Notification
{
    private static Dictionary<Type, Dictionary<int, HashSet<object>>> BindMap = new Dictionary<Type, Dictionary<int, HashSet<object>>>();
    public static void CreateBinding(int EventHash, Action callback)
    {
        Type t = typeof(Action);
        AddBinding(t, EventHash, callback);
    }
    public static void Emit(int EventHash)
    {
        var bindings = GetBinding<Action>(EventHash);
		if(bindings == null) return; 
		foreach (var callback in bindings)
		{
			(callback as Action)();
		}
    }
    public static void RemoveBinding(int hash, Action callback)
    {
        Type t = typeof(Action);
        RemoveBinding(t,hash,callback);
    }


    public static void CreateBinding<T>(int EventHash, Action<T> callback)
    {
        Type t = typeof(Action<T>);
        AddBinding(t, EventHash, callback);
    }
    public static void Emit<T>(int EventHash, T arg)
    {
        var bindings = GetBinding<Action<T>>(EventHash);
		if(bindings == null) return; 
		foreach (var callback in bindings)
		{
			(callback as Action<T>)(arg);
		}
    }
    public static void RemoveBinding<T>(int hash, Action<T> callback)
    {
        Type t = typeof(Action<T>);
        RemoveBinding(t,hash,callback);
    }

    public static void CreateBinding<T1, T2>(int EventHash, Action<T1, T2> callback)
    {
        Type t = typeof(Action<T1, T2>);
        AddBinding(t, EventHash, callback);
    }
    public static void Emit<T1, T2>(int EventHash, T1 arg1, T2 arg2)
    {
        var bindings = GetBinding<Action<T1, T2>>(EventHash);
		if(bindings == null) return; 
		foreach (var callback in bindings)
		{
			(callback as Action<T1,T2>)(arg1,arg2);
		}
    }
    public static void RemoveBinding<T1, T2>(int hash, Action<T1, T2> callback)
    {
        Type t = typeof(Action<T1, T2>);
        RemoveBinding(t,hash,callback);
    }




    private static void RemoveBinding(Type t,int hash,object callback)
    {
        Dictionary<int, HashSet<object>> bindings;
        bool get = BindMap.TryGetValue(t, out bindings);
        if (!get) return;
        HashSet<object> backs;
        get = bindings.TryGetValue(hash, out backs);
        if (!get) return;
        backs.Remove(callback);
		if(backs.Count == 0){
			bindings.Remove(hash);
		}
		//bindMap就不用移除了
    }
    private static void AddBinding(Type type, int hash, object callback)
    {
        Dictionary<int, HashSet<object>> bindings;
        if (BindMap.ContainsKey(type))
        {
            bindings = BindMap[type];
        }
        else
        {
            bindings = new Dictionary<int, HashSet<object>>();
			BindMap.Add(type, bindings);
        }
		HashSet<object> cbSet;
		if(bindings.ContainsKey(hash)){
			cbSet = bindings[hash];
		}else{
			cbSet = new HashSet<object>();
			bindings.Add(hash, cbSet);
		}
		cbSet.Add(callback);
    }
    private static HashSet<object> GetBinding<T>(int hash)
    {
        Type t = typeof(T);
        Dictionary<int, HashSet<object>> bindings;
        bool exit = BindMap.TryGetValue(t, out bindings);
        if (!exit) return null;
		HashSet<object> ret;
		exit = bindings.TryGetValue(hash, out ret);
        if (!exit) return null;
		return ret;
    }

}