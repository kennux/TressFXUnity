using UnityEngine;
using System.Collections;

public class TressFXGui : MonoBehaviour
{
	public Transform modelTransform;

	private TressFXSimulation simulation;
	private TressFXRender renderer;

	private Vector2 lastMousePosition;

	public void Start()
	{
		this.simulation = this.GetComponent<TressFXSimulation>();
		this.renderer = this.GetComponent<TressFXRender>();
	}

	public void OnGUI()
	{
		GUI.Label (new Rect(20, 10, 200, 20), "Simulation took: " + this.simulation.computationTime + " ms");
		GUI.Label (new Rect(20, 30, 200, 20), "Rendering took: " + this.renderer.renderTime + " ms");
		GUI.Label (new Rect(20, 50, 2000, 20), "Press Mouse Button 1 (right) and move the mouse to move the model");
		GUI.Label (new Rect(20, 70, 2000, 20), "Press Mouse Button 2 (wheel) and move the mouse to rotate around the model");
	}

	public void Update()
	{
		if (Input.GetMouseButton(1))
		{
			Vector2 mousePos = new Vector2(Input.mousePosition.x, -Input.mousePosition.y);
			if (this.lastMousePosition != Vector2.zero)
			{
				Vector2 difference = mousePos - this.lastMousePosition;
				difference = difference * -0.1f;

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
