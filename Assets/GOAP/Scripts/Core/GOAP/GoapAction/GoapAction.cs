
using UnityEngine;
using System.Collections.Generic;

namespace Goap.Action
{
    public abstract  class GoapAction 
    {
		/// <summary>
		/// 先决条件
		/// </summary>
		/// <value></value>
        public Dictionary<string, bool> Preconditions
        { get;private set; }

		/// <summary>
		/// 造成后果
		/// </summary>
		/// <value></value>
        public Dictionary<string, bool> Effects
        { get;private set; }

        public ACTION_TYPE type { get; internal set;}
		/// <summary>
		/// 行为的消耗,用于与权重进行计算
		/// </summary>
		/// <value></value>
        public virtual float Cost
        {
			get; protected set;
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
        public GoapAction()
        {
            Preconditions = new Dictionary<string, bool>();
            Effects = new Dictionary<string, bool>();
        }

		/// <summary>
		/// 重置agent所需变量
		/// </summary>
		/// <param name="agent"></param>
        public abstract void Reset(GoapAgent agent);

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
            Preconditions.Add(key, value);
        }

		/// <summary>
		/// 移除先决条件
		/// </summary>
		/// <param name="key">键</param>
        public void removePrecondition(string key)
        {
            if (Preconditions.ContainsKey(key))
                Preconditions.Remove(key);
        }


		/// <summary>
		/// <para> 添加加动作完成后造成的后果</para>
		/// <para>仅用于planer决策时使用</para>
		/// </summary>
		/// <param name="key">键</param>
		/// <param name="value">值</param>
        public void addEffect(string key, bool value)
        {
            Effects.Add(key, value);
        }


		/// <summary>
		/// 移除造成后果
		/// </summary>
		/// <param name="key">键</param>
        public void removeEffect(string key)
        {
            if (Effects.ContainsKey(key))
                Effects.Remove(key);
        }
    }
}