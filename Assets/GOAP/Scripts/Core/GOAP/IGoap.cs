using System;
using UnityEngine;
using System.Collections;

/**
 * Collect the world data for this Agent that will be
 * used for GOAP planning.
 */
using System.Collections.Generic;

namespace Goap
{
using Action;

	/// <summary>
	/// <para>游戏对象想要使用GOAP必须实现的接口</para>
	/// <para>包括提供世界数据，对象数据</para>
	/// <para>接收GOAP回调</para>
	/// </summary>
    public interface IGoap
    {
        /**
         * The starting state of the Agent and the world.
         * Supply what states are needed for actions to run.
         */
        Dictionary<string, bool> getWorldState();

		/// <summary>
		/// 获取一个目标（goal）
		/// </summary>
		/// <returns>各种状态需要达到的值</returns>
        Dictionary<string, bool> createGoalState();


        /**
         * Get blackboard for environment
         */
        DataBag GetDataBag();

        /**
         * No sequence of actions could be found for the supplied goal.
         * You will need to try another goal
         */
        void planFailed(Dictionary<string, bool> failedGoal);

        /**
         * A plan was found for the supplied goal.
         * These are the actions the Agent will perform, in order.
         */
        void planFound(KeyValuePair<string, bool> goal, Queue<GoapAction> actions);

        /**
         * All actions are complete and the goal was reached. Hooray!
         */
        void actionsFinished();

        /**
         * One of the actions caused the plan to abort.
         * That action is returned.
         */
        void planAborted(GoapAction aborter);

        void Init();
        void Tick();
        void Release();

        /// <summary>
        /// save agent instance
        /// </summary>
        IAgent Agent { get; set; }
		GameObject gameObject{ get; set; }
	}
}