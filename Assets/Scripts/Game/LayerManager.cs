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