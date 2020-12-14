
using UnityEngine;
using System.Collections.Generic;

namespace Goap.Action
{
    public abstract  class GoapAction 
    {


        private Dictionary<string, bool> preconditions;
        private Dictionary<string, bool> effects;

        private bool inRange = false;

        /* The Cost of performing the action. 
         * Figure out a weight that suits the action. 
         * Changing it will affect what actions are chosen during planning.*/
        public float Cost = 1f;
        public ACTION_TYPE type { get; internal set;}
        public virtual float GetCost()
        {
            return Cost;
        }

		/// <summary>
		/// 风险
		/// </summary>
        public float Risk = 0f;
		/// <summary>
		/// 回报
		/// </summary>
        public float Return = 1f;
		/// <summary>
		/// 权重
		/// </summary>
		/// <returns></returns>
        public virtual float GetWeight()
        {
            return (1 - Risk) * Return;
        }

        /**
         * An action often has to perform on an object. This is that object. Can be null. */
        public GameObject target;

        public GoapAction()
        {
            preconditions = new Dictionary<string, bool>();
            effects = new Dictionary<string, bool>();
        }

        public void doReset(GoapAgent agent)
        {
            inRange = false;
            target = null;
            reset();
        }


        /**
         * Reset any variables that need to be reset before planning happens again.
         */
        public abstract void reset();

        /**
         * Is the action done?
         */
        public abstract bool isDone();

		/// <summary>
		/// 检测当前agent是否符合动作条件
		/// </summary>
		/// <param name="agent"></param>
		/// <returns></returns>
        public abstract bool checkProceduralPrecondition(GoapAgent agent);

		/// <summary>
		/// 让agent执行当前action
		/// </summary>
		/// <param name="agent"></param>
		/// <returns></returns>
        public abstract IEnumerator<bool> createPerformance(GoapAgent agent);

		/// <summary>
		/// <para>添加先决条件的键值对</para>
		/// <para>仅用于planer决策时使用</para>
		/// </summary>
		/// <param name="key">键</param>
		/// <param name="value">值</param>
        public void addPrecondition(string key, bool value)
        {
            preconditions.Add(key, value);
        }

		/// <summary>
		/// 移除先决条件
		/// </summary>
		/// <param name="key">键</param>
        public void removePrecondition(string key)
        {
            if (preconditions.ContainsKey(key))
                preconditions.Remove(key);
        }


		/// <summary>
		/// <para> 添加加动作完成后造成的后果</para>
		/// <para>仅用于planer决策时使用</para>
		/// </summary>
		/// <param name="key">键</param>
		/// <param name="value">值</param>
        public void addEffect(string key, bool value)
        {
            effects.Add(key, value);
        }


		/// <summary>
		/// 移除造成后果
		/// </summary>
		/// <param name="key">键</param>
        public void removeEffect(string key)
        {
            if (effects.ContainsKey(key))
                effects.Remove(key);
        }


        public Dictionary<string, bool> Preconditions
        {
            get
            {
                return preconditions;
            }
        }

        public Dictionary<string, bool> Effects
        {
            get
            {
                return effects;
            }
        }
    }
}