using System;
using System.Collections.Generic;
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
		private IEnumerator<bool> movePerformance;
		private FSM.FSMState idleState; // finds something to do
		private FSM.FSMState moveToState; // finds something to do
		private FSM.FSMState performActionState; // performs an action
		private FSM.FSMState performte; // performs an action
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
			this.idleState = (fsm) =>
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


		private void createPerformActionState()
		{
			performActionState = (fsm) =>
			{
				var performance = this.actionPerformance;
				var action = this.workingActions.Peek();
				if (performance == null)
				{
					this.actionPerformance = action.createPerformance(this);
					this.actionPerformance.Reset();
				}
				if (!action.CheckInRange(this))
				{
					fsm.pushState(idleState);
					return;
				}
				var working = performance.MoveNext();//进行一步动作
				var success = performance.Current;//现在的状态
				if (!working)
				{
					//不能进行下一步，说明动作成功做完
					this.workingActions.Dequeue();
					if (!hasActionPlan())
					{
						// no actions to perform
						System.Console.WriteLine("<color=red>Done actions</color>");
						this.actionPerformance = null;
						fsm.popState();
						fsm.pushState(idleState);
						dataProvider.actionsFinished();
						return;
					}
					else
					{
						// perform the next action
						this.actionPerformance = this.workingActions.Peek().createPerformance(this);
						this.actionPerformance.Reset();
						return;
					}
				}
				if (!success)//进行失败,重新计算
				{
					fsm.popState();
					fsm.pushState(idleState);
					dataProvider.planAborted(action);
					return;
				}
			};
		}

    private void createMoveToState()
    {
        moveToState = (fsm) =>
        {

			var action = workingActions.Peek();
			if (this.movePerformance == null)
			{
				this.movePerformance =dataProvider.MoveMent(action);
			}
			this.movePerformance.MoveNext();
			if (action.CheckInRange(this))
			{
				this.movePerformance = null;
				fsm.popState();
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