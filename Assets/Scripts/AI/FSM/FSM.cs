﻿using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/**
 * Stack-based Finite State Machine.
 * Push and pop states to the FSM.
 * 
 * States should push other states onto the stack 
 * and pop themselves off.
 */


public class FSM {

	private Stack<FSMState> stateStack = new Stack<FSMState> ();

	public delegate void FSMState (FSM fsm);
	

	public void Update () {
		if (stateStack.Peek() != null)
			stateStack.Peek().Invoke (this);
	}

	public void pushState(FSMState state) {
		stateStack.Push (state);
	}

	public void popState() {
		stateStack.Pop ();
	}

    public void ClearState()
    {
        stateStack.Clear();
    }
}