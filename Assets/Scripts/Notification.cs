using System.Collections.Generic;
using System;
using UnityEngine;
public static class Notification
{
    private static List<Signal> signals = new List<Signal>();

    public static void createBinding<T>(int EventHash, Action<T> Listener)
    {
		var signal = GetSignal(EventHash);
		signal.Reset(EventHash);

    }
    private static Signal GetSignal(int EventHash)
    {
        var ret = signals.Find((s => s.Active && s.Event == EventHash));
        if (ret == null){
            ret = signals.Find((s => !s.Active ));
		}
        if (ret == null){
            ret = new Signal();
			signals.Add(ret);
		}
        return ret;
    }

}
public class Signal
{
	public bool Active = false;
    public int Event = 0;
	public void Reset(int Event){
		this.Active = true;
		this.Event = Event;
	}

}