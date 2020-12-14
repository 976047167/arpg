using System;
using System.Collections.Generic;
using UnityEngine;
using Goap.Action;
namespace Goap
{


	public interface IAgent
	{
		void AddAction(ACTION_TYPE a);

		void RemoveAction(ACTION_TYPE action);
		void AbortFsm();
	}
	public sealed class GoapAgent : IAgent
	{
		private HashSet<GoapAction> availableActions;
		private Queue<GoapAction> workingActions;

		public IGoap dataProvider { private set; get; }
		// this is the implementing class that provides our world data and listens to feedback on planning

		private IEnumerator<bool> actionPerformance;
		private FSM.FSMState idleState; // finds something to do
										// private FSM.FSMState moveToState; // moves to a target
		private FSM.FSMState performActionState; // performs an action
		private FSM stateMachine;

		public GoapAgent(IGoap dataProvider)
		{
			stateMachine = new FSM();
			availableActions = new HashSet<GoapAction>();
			workingActions = new Queue<GoapAction>();
			this.dataProvider = dataProvider;
			createIdleState();
			createMoveToState();
			createPerformActionState();
			stateMachine.pushState(idleState);
		}
		public void AbortFsm()
		{
			stateMachine.ClearState();
			stateMachine.pushState(idleState);
		}

		public void AddAction(ACTION_TYPE type)
		{
			GoapAction action = GoapActionPool.getInstance().getAction(type);
			availableActions.Add(action);
		}
		public HashSet<GoapAction> GetActions()
		{
			return availableActions;
		}

		public void RemoveAction(ACTION_TYPE type)
		{
			GoapAction action = GoapActionPool.getInstance().getAction(type);
			availableActions.Remove(action);
		}


		// private void Update()
		// {
		//     stateMachine.Update(gameObject);
		// }

		private bool hasActionPlan()
		{
			return workingActions.Count > 0;
		}

		/// <summary>
		/// <para>idle状态</para>
		/// <para>每帧调用，让处于空闲状态的ai计划下一步动作并改变状态 </para> 
		/// </summary>
		private void createIdleState()
		{
			this.idleState = (fsm, gameObj) =>
			{
				//获得一个根据优先级排序的目标队列
				var goals = dataProvider.createGoalState();
				Queue<GoapAction> plan = null;
				KeyValuePair<string, bool> lastGoal = new KeyValuePair<string, bool>();
				//遍历所有目标，找到第一个可以执行的目标和方案
				foreach (var goal in goals)
				{
					lastGoal = goal;
					plan = GoapPlanner.plan(this, goal);
					if (plan != null)
						break;
				}
				if (plan != null)
				{
					//更新当前agent的状态
					workingActions = plan;
					dataProvider.planFound(lastGoal, plan);

					fsm.popState(); // move to PerformAction state
					fsm.pushState(performActionState);
				}

			};
		}

		private void createMoveToState()
		{
			// moveToState = (fsm, gameObj) =>
			// {
			//     GoapAction action = workingActions.Peek();
			//     if (dataProvider.moveAgent(action))
			//     {
			//         fsm.popState();
			//     }

			// };
		}

		private void createPerformActionState()
		{
			performActionState = (fsm, gameObj) =>
			{
				// perform the action
				var action = this.actionPerformance;
				if (!action.MoveNext())
				{
					var success = action.Current;
					if (!success)
					{
						// action failed, we need to plan again
						fsm.popState();
						fsm.pushState(idleState);
						// dataProvider.planAborted(action);
						return;
					}
					// the action is done. Remove it so we can perform the next one
					workingActions.Dequeue();
					if (!hasActionPlan())
					{
						// no actions to perform
						Debug.Log("<color=red>Done actions</color>");
						fsm.popState();
						fsm.pushState(idleState);
						dataProvider.actionsFinished();
						return;
					}
				}
				else
				{
					// perform the next action
					this.actionPerformance = workingActions.Peek().createPerformance(this);
					this.actionPerformance.Reset();
				}
			};
		}

		public static string prettyPrint(HashSet<KeyValuePair<string, object>> state)
		{
			var s = "";
			foreach (var kvp in state)
			{
				s += kvp.Key + ":" + kvp.Value;
				s += ", ";
			}
			return s;
		}

		public static string prettyPrint(Queue<GoapAction> actions)
		{
			var s = "";
			foreach (var a in actions)
			{
				s += a.GetType().Name;
				s += "-> ";
			}
			s += "GOAL";
			return s;
		}
		public static string prettyPrint(Dictionary<string, bool> goals)
		{
			var s = "";
			foreach (var a in goals)
			{
				s += a.Key;
				s += "-> ";
			}
			s += "GOAL";
			return s;
		}

		public static string prettyPrint(GoapAction[] actions)
		{
			var s = "";
			foreach (var a in actions)
			{
				s += a.GetType().Name;
				s += ", ";
			}
			return s;
		}

		public static string prettyPrint(GoapAction action)
		{
			var s = "" + action.GetType().Name;
			return s;
		}
	}
}