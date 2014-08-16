using UnityEngine;
using System.Collections;

public class TressFXDemo : MonoBehaviour
{
	public Transform modelTransform;
	private Vector2 lastMousePosition;
	public float movementSpeed = 1;

	public void Update()
	{
		if (Input.GetMouseButton(1))
		{
			Vector2 mousePos = new Vector2(Input.mousePosition.x, -Input.mousePosition.y);
			if (this.lastMousePosition != Vector2.zero)
			{
				Vector2 difference = mousePos - this.lastMousePosition;
				difference = difference * -0.1f * this.movementSpeed;
				
				// Move model
				this.modelTransform.position = new Vector3(this.modelTransform.position.x + difference.x,
				                                           this.modelTransform.position.y + difference.y,
				                                           this.modelTransform.position.z);
			}
			
			this.lastMousePosition = mousePos;
		}
		else
		{
			this.lastMousePosition = Vector2.zero;
		}
	}
}
