using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LayerManager : MonoBehaviour
{
	private static LayerManager s_Instance;
	private static bool s_Initialized;

	// Built-in Unity layers.
	private const int DefaultLayer = 0;
	private const int TransparentFXLayer = 1;
	private const int IgnoreRaycastLayer = 2;
	private const int WaterLayer = 4;
	private const int UILayer = 5;

	public static int Default { get { return DefaultLayer; } }
	public static int TransparentFX { get { return TransparentFXLayer; } }
	public static int IgnoreRaycast { get { return IgnoreRaycastLayer; } }
	public static int Water { get { return WaterLayer; } }
	public static int UI { get { return UILayer; } }

	private const int EnemyLayer = 26;
	private const int MovingPlatformLayer = 27;
	private const int VisualEffectLayer = 28;
	private const int OverlayLayer = 29;
	private const int SubCharacterLayer = 30;
	private const int CharacterLayer = 31;

	public static int Enemy { get { return EnemyLayer; } }
	public static int MovingPlatform { get { return MovingPlatformLayer; } }
	public static int VisualEffect { get { return VisualEffectLayer; } }
	public static int Overlay { get { return OverlayLayer; } }
	public static int SubCharacter { get { return SubCharacterLayer; } }
	public static int Character { get { return CharacterLayer; } }

	private static Dictionary<Collider, List<Collider>> s_IgnoreCollisionMap;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void Initialize()
	{
		if (!s_Initialized)
		{
			s_Instance = new GameObject("Layer Manager").AddComponent<LayerManager>();
			s_Initialized = true;
		}
	}

	private void OnEnable()
	{
		if (s_Instance == null)
		{
			s_Instance = this;
			s_Initialized = true;
			SceneManager.sceneUnloaded -= SceneUnloaded;
		}
	}

	private void Awake()
	{
		Physics.IgnoreLayerCollision(IgnoreRaycast, VisualEffect);
		Physics.IgnoreLayerCollision(SubCharacter, Default);
		Physics.IgnoreLayerCollision(SubCharacter, VisualEffect);
		Physics.IgnoreLayerCollision(VisualEffect, VisualEffect);
		Physics.IgnoreLayerCollision(Overlay, Default);
		Physics.IgnoreLayerCollision(Overlay, VisualEffect);
		Physics.IgnoreLayerCollision(Overlay, Enemy);
		Physics.IgnoreLayerCollision(Overlay, SubCharacter);
		Physics.IgnoreLayerCollision(Overlay, Character);
	}
	/// <summary>
	///	在两个碰撞器之间相互添加碰撞忽略
	/// </summary>
	/// <param name="mainCollider"></param>
	/// <param name="otherCollider"></param>
	public static void IgnoreCollision(Collider mainCollider, Collider otherCollider)
	{
		if (s_IgnoreCollisionMap == null)
		{
			s_IgnoreCollisionMap = new Dictionary<Collider, List<Collider>>();
		}

		List<Collider> colliderList;
		if (!s_IgnoreCollisionMap.TryGetValue(mainCollider, out colliderList))
		{
			colliderList = new List<Collider>();
			s_IgnoreCollisionMap.Add(mainCollider, colliderList);
		}
		colliderList.Add(otherCollider);

		if (!s_IgnoreCollisionMap.TryGetValue(otherCollider, out colliderList))
		{
			colliderList = new List<Collider>();
			s_IgnoreCollisionMap.Add(otherCollider, colliderList);
		}
		colliderList.Add(mainCollider);

		Physics.IgnoreCollision(mainCollider, otherCollider);
	}

	/// <summary>
	/// 消除碰撞器身上所有在缓存中储存的忽略，并且被忽略者也消除
	/// </summary>
	/// <param name="mainCollider"></param>
	public static void RevertCollision(Collider mainCollider)
	{
		if(s_IgnoreCollisionMap == null) return;
		List<Collider> colliderList;
		List<Collider> otherColliderList;

		if (s_IgnoreCollisionMap.TryGetValue(mainCollider, out colliderList))
		{
			for (int i = 0; i < colliderList.Count; ++i)
			{
				if (!mainCollider.enabled || !mainCollider.gameObject.activeInHierarchy || !colliderList[i].enabled || !colliderList[i].gameObject.activeInHierarchy)
				{
					continue;
				}

				Physics.IgnoreCollision(mainCollider, colliderList[i], false);

				if (s_IgnoreCollisionMap.TryGetValue(colliderList[i], out otherColliderList))
				{
					for (int j = 0; j < otherColliderList.Count; ++j)
					{
						if (otherColliderList[j].Equals(mainCollider))
						{
							otherColliderList.RemoveAt(j);
							break;
						}
					}
				}
			}
			colliderList.Clear();
		}
	}

	private void SceneUnloaded(Scene scene)
	{
		s_Initialized = false;
		s_Instance = null;
		SceneManager.sceneUnloaded -= SceneUnloaded;
	}
	private void OnDisable()
	{
		SceneManager.sceneUnloaded += SceneUnloaded;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void DomainReset()
	{
		s_Initialized = false;
		s_Instance = null;
	}
}