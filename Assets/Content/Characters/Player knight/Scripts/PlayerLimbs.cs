using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerLimbs : MonoBehaviour
{
	[SerializeField, Required] private InventoryController _inventoryController;
	[SerializeField, Required] private PlayerLimbsVisual _limbsVisual;

	private Dictionary<LimbType, LimbInfo> _limbs;

	public event System.Action Dead;
	public event System.Action BodyLosted; // When body lost, regeneration is impossible
	public event System.Action ExtremitiesLosted;
	public event System.Action LegsLosted;
	public event System.Action LegsRestored;

	public InventoryController InventoryController => _inventoryController;

	public Dictionary<LimbType, bool> LimbStates => GetLimbStates();

	private void Awake()
	{
		Initialize();
	}

	private void OnEnable()
	{
		_inventoryController.InventorySoul.SoulPlaced += Regenerate;
	}

	private void OnDisable()
	{
		_inventoryController.InventorySoul.SoulPlaced -= Regenerate;
	}

	private void Initialize()
	{
		_limbs = new Dictionary<LimbType, LimbInfo>();

		foreach (LimbType limbType in System.Enum.GetValues(typeof(LimbType)))
		{
			_limbs[limbType] = new LimbInfo(true, SoulType.None);
		}
	}

	[ContextMenu(nameof(ActivateInventory))]
	public void ActivateInventory()
	{
		_inventoryController.Activate(LimbStates);
	}

	[ContextMenu(nameof(DeactivateInventory))]
	public void DeactivateInventory()
	{
		_inventoryController.Deactivate();
	}

	public void TakeDamage()
	{
		var availableExtremities = GetAvailableExtremities();

		if (availableExtremities.Count > 0)
		{
			var randomLimb = availableExtremities[Random.Range(0, availableExtremities.Count)];
			LoseLimb(randomLimb);

			if (availableExtremities.Count == 1)
			{
				ExtremitiesLosted?.Invoke();
			}

			return;
		}

		if (_limbs[LimbType.Body].IsPresent)
		{
			LoseLimb(LimbType.Body);
			BodyLosted?.Invoke();

			return;
		}

		if (_limbs[LimbType.Head].IsPresent)
		{
			LoseLimb(LimbType.Head);
			Dead?.Invoke();
		}

	}

	public void ResetToDefault()
	{
		foreach (var limb in _limbs.Keys)
		{
			_limbs[limb] = new LimbInfo(true, SoulType.None);
		}
	}

	public bool HasLegs()
	{
		return _limbs[LimbType.LeftLeg].IsPresent || _limbs[LimbType.RightLeg].IsPresent;
	}

	private void Regenerate(LimbType limbType, SoulType soulType)
	{
		if (_limbs[LimbType.Body].IsPresent == false)
		{
			Debug.LogWarning("Cannot regenerate limb: body lost!");
			return;
		}

		bool wasLegless = HasLegs() == false;

		_limbs[limbType] = new LimbInfo(true, soulType);
		//Debug.Log($"Restore {limbType} with soul {soulType}");

		_limbsVisual.PlayRestore(limbType);

		if (wasLegless && HasLegs() && (limbType == LimbType.LeftLeg || limbType == LimbType.RightLeg))
		{
			LegsRestored?.Invoke();
		}

		DeactivateInventory();
	}

	private void LoseLimb(LimbType limbType)
	{
		_limbs[limbType] = new LimbInfo(false, SoulType.None);
		//Debug.Log($"Lose {limbType}");

		_limbsVisual.PlayLose(limbType);

		if (HasLegs() == false)
		{
			LegsLosted?.Invoke();
		}
	}

	private List<LimbType> GetAvailableExtremities()
	{
		var extremities = new List<LimbType>();

		var extremityTypes = new[]
		{
			LimbType.LeftArm,
			LimbType.RightArm,
			LimbType.LeftLeg,
			LimbType.RightLeg
		};

		foreach (var limb in extremityTypes)
		{
			if (_limbs[limb].IsPresent)
			{
				extremities.Add(limb);
			}
		}

		return extremities;
	}

	private Dictionary<LimbType, bool> GetLimbStates()
	{
		return _limbs.ToDictionary(pair => pair.Key, pair => pair.Value.IsPresent);
	}
}
