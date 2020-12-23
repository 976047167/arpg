using System;
using System.Collections.Generic;
using UnityEngine;


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

            // build up the tree and record the leaf nodes that provide a solution to the goal.
            var leaves = new List<GoapNode>();

            // build graph
            var start = NodeManager.GetFreeNode(null, 0, 0, worldState, null);
            var success = buildGraph(start, leaves, usableActions, goal);

            if (!success)
            {
                // oh no, we didn't get a plan
                //            Debug.Log("NO PLAN");
                return null;
            }

            // get the cheapest leaf
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

            // get its node and work back through the parents
            var result = new List<GoapAction>();
            var n = cheapest;
            while (n != null)
            {
                if (n.action != null)
                {
                    result.Insert(0, n.action); // insert the action in the front
                }
                n = n.parent;
            }

            NodeManager.Release();
            // we now have this action list in correct order

            var queue = new Queue<GoapAction>();
            foreach (var a in result)
            {
                queue.Enqueue(a);
            }
            // hooray we have a plan!
            return queue;
        }

        /**
         * Returns true if at least one solution was found.
         * The possible paths are stored in the leaves list. Each leaf has a
         * 'runningCost' value where the lowest Cost will be the best action
         * sequence.
         */

        private static bool buildGraph(GoapNode parent, List<GoapNode> leaves
            , HashSet<GoapAction> usableActions, KeyValuePair<string, bool> goal)
        {
            var foundOne = false;

            // go through each action available at this node and see if we can use it here
            foreach (var action in usableActions)
            {
                // if the parent state has the conditions for this action's preconditions, we can use it here
                if (inState(action.Preconditions, parent.state))
                {
                    // apply the action's effects to the parent state
                    var currentState = populateState(parent.state, action.Effects);
                    //Debug.Log(GoapAgent.prettyPrint(currentState));
                    var node = NodeManager.GetFreeNode(parent, parent.runningCost + action.Cost, parent.weight + action.GetWeight(),
                        currentState, action);

                    //force child.precondition in parent.effects or child.precondition is empty.
                    if (action.Preconditions.Count == 0 && parent.action != null ||
                        parent.action != null && !CondRelation(action.Preconditions, parent.action.Effects))
                        continue;

                    if (FillGoal(goal, currentState))
                    {
                        // we found a solution!
                        leaves.Add(node);
                        foundOne = true;
                    }
                    else
                    {
                        // not at a solution yet, so test all the remaining actions and branch out the tree
                        var subset = actionSubset(usableActions, action);
                        var found = buildGraph(node, leaves, subset, goal);
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

        /**
         * Check that all items in 'test' are in 'state'. If just one does not match or is not there
         * then this returns false.
         */

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
        private static bool FillGoal(KeyValuePair<string, bool> goal, Dictionary<string, bool> state)
        {
            var match = state.ContainsKey(goal.Key) && state[goal.Key] == goal.Value;
            return match;
        }

        /**
         * Apply the stateChange to the currentState
         */

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