using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(SpringJoint2D))]
public class SwordController : MonoBehaviour
{
	[SerializeField, Required] private SpringJoint2D _springJoint;
	[SerializeField, Required] private Sword _sword;

	public void Activate()
	{
		_springJoint.enabled = true;
		_sword.ActiveFollow();
	}

	private void Update()
	{
		_sword.UpdateLook(transform);
	}

	public void Deactivate()
	{
		_sword.DeactiveFollow();
		_springJoint.enabled = false;
	}
}