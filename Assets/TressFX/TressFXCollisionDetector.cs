using UnityEngine;
using System.Collections;

public class TressFXCollisionDetector : MonoBehaviour
{
	public TressFXSimulation tressFXSimulation;

	public void OnTriggerEnter(Collider collider)
	{
		this.tressFXSimulation.AddCollisionTarget(collider);
	}

	public void OnTriggerExit(Collider collider)
	{
		this.tressFXSimulation.RemoveCollisionTarget(collider);
	}
}
