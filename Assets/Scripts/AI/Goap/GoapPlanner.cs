using System;
using System.Collections.Generic;


namespace Goap
{
    using Action;
    /**
     * Plans what actions can be completed in order to fulfill a goal state.
     */
    internal class GoapPlanner
    {
        /// <summary>
        /// 计算可行方案
        /// </summary>
        /// <param name="agent"> 执行计算方案的代理实例</param> 
        /// <param name="goal">目标</param> 
        /// <returns> 返回可以执行的动作队列，无方案的情况下返回null</returns>
        public static Queue<GoapAction> plan(GoapAgent agent, KeyValuePair<string, bool> goal)
        {
            IGoap dataProvider = agent.dataProvider;
            Dictionary<string, bool> worldState = dataProvider.getWorldState();
            HashSet<GoapAction> availableActions  = NodeManager.GetFreeActionSet();
            // 重置所有action状态
            foreach (var action in agent.GetActions())
            {
                action.Reset(agent);
                availableActions.Add(action);
            }

            // check what actions can run using their checkProceduralPrecondition
            var usableActions = NodeManager.GetFreeActionSet();
            foreach (var a in availableActions)
            {
                if (a.checkProceduralPrecondition(agent))
                    usableActions.Add(a);
            }

            // we now have all actions that can run, stored in usableActions

            // 记录递归结果的集合.
            var leaves = new List<GoapNode>();

            // 创建根节点
            var root = NodeManager.GetFreeNode(null, 0, 0, worldState, null);
            var success = buildGraph(root,ref leaves, usableActions, goal);

            if (!success)
            {
                // oh no, we didn't get a plan
                //            Debug.Log("NO PLAN");
                return null;
            }

            //找出几个叶子节点中权重消耗比最大的那个。
            GoapNode cheapest = null;
            foreach (var leaf in leaves)
            {
                if (cheapest == null)
                    cheapest = leaf;
                else
                {
                    if (leaf.BetterThen(cheapest))
                        cheapest = leaf;
                }
            }

            //输出结果队列
            var result = new Stack<GoapAction>();
            var n = cheapest;
            while (n != null)
            {
                if (n.action != null)
                {
                    result.Push(n.action); 
                }
                n = n.parent;
            }
            NodeManager.Release();
            var queue = new Queue<GoapAction>();
			while (result.Count>0)
			{
				queue.Enqueue(result.Pop());
			}
			return queue;
        }

        /**
         * Returns true if at least one solution was found.
         * The possible paths are stored in the leaves list. Each leaf has a
         * 'runningCost' value where the lowest Cost will be the best action
         * sequence.
         */

        private static bool buildGraph(GoapNode parent,ref List<GoapNode> leaves
            , HashSet<GoapAction> usableActions, KeyValuePair<string, bool> goal)
        {
            var foundOne = false;

            // go through each action available at this node and see if we can use it here
            foreach (var action in usableActions)
            {
                //检测当前动作前提是否符合状态
                if (inState(action.Preconditions, parent.state))
                {
					//剪枝，如果动作没有先决条件，那么他的父节点必然为根节点，否则直接跳过
                    if (action.Preconditions.Count == 0 && parent.action != null)continue;
					//如果父节点不是根节点（即父节点有动作），那么动作的先决条件至少要和父节动作点造成的影响有关
					if( parent.action != null && !CondRelation(action.Preconditions, parent.action.Effects)) continue;

					
                    //应用行动后的状态为当前状态
                    var currentState = populateState(parent.state, action.Effects);
                    //Debug.Log(GoapAgent.prettyPrint(currentState));
					//生成新的节点，累积父节点的权重和消耗
                    var node = NodeManager.GetFreeNode(parent, parent.runningCost + action.Cost, parent.weight + action.GetWeight(),
                        currentState, action);
                    if (FillGoal(goal, currentState))
                    {
                        // we found a solution!
                        leaves.Add(node);
                        foundOne = true;
                    }
                    else
                    {
						//如果没找到，那么以这个动作为父节点，继续遍历
                        var subset = actionSubset(usableActions, action);
                        var found = buildGraph(node,ref leaves, subset, goal);
                        if (found)
                            foundOne = true;
                    }
                }
            }

            return foundOne;
        }


		/// <summary>
		/// 是否有至少一项条件关联
		/// </summary>
		/// <param name="preconditions"></param>
		/// <param name="effects"></param>
		/// <returns></returns>
        private static bool CondRelation(Dictionary<string, bool> preconditions
                                , Dictionary<string, bool> effects)
        {
            foreach (var t in preconditions)
            {
                var match = effects.ContainsKey(t.Key) && effects[t.Key] == t.Value;
                if (match)
                    return true;
            }
            return false;
        }

		/// <summary>
		/// 创建一个新的子SET，内容为父SET减去第二个参数
		/// </summary>
		/// <param name="actions">父SET</param>
		/// <param name="removeMe">减除的value</param>
		/// <returns></returns>
        private static HashSet<GoapAction> actionSubset(HashSet<GoapAction> actions, GoapAction removeMe)
        {
            var subset = NodeManager.GetFreeActionSet();
            foreach (var a in actions)
            {
                if (!a.Equals(removeMe))
                    subset.Add(a);
            }
            return subset;
		}
		/// <summary>
		/// 目标状态集合是否全在该目标集合内  
		/// </summary>
		/// <param name="test"></param>
		/// <param name="state"></param>
		/// <returns></returns>
        private static bool inState(Dictionary<string, bool> test, Dictionary<string, bool> state)
        {
            var allMatch = true;
            foreach (var t in test)
            {
                var match = state.ContainsKey(t.Key) && state[t.Key] == t.Value;
                if (!match)
                {
                    allMatch = false;
                    break;
                }
            }
            return allMatch;
		}
		/// <summary>
		/// state 是否已经满足gaol
		/// </summary>
		/// <param name="goal"></param>
		/// <param name="state"></param>
		/// <returns></returns>
        private static bool FillGoal(KeyValuePair<string, bool> goal, Dictionary<string, bool> state)
        {
            var match = state.ContainsKey(goal.Key) && state[goal.Key] == goal.Value;
            return match;
        }

  
		/// <summary>
		/// 将改变的state完全应用到current中
		/// </summary>
		/// <param name="currentState"></param>
		/// <param name="stateChange"></param>
		/// <returns></returns>
        private static Dictionary<string, bool> populateState(Dictionary<string, bool> currentState,
            Dictionary<string, bool> stateChange)
        {
            Dictionary<string, bool> state = NodeManager.GetFreeState();
            state.Clear();
            foreach (var s in currentState)
            {
                state.Add(s.Key, s.Value);
            }

            foreach (var change in stateChange)
            {
                // if the key exists in the current state, update the Value
                if (state.ContainsKey(change.Key))
                {
                    state[change.Key] = change.Value;
                }
                else
                {
                    state.Add(change.Key, change.Value);
                }
            }
            return state;
        }

    }
}