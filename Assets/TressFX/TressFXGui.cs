using UnityEngine;
using System.Collections;

public class TressFXGui : MonoBehaviour
{
	public Transform modelTransform;

	private TressFXSimulation simulation;
	private TressFXRender renderer;

	private Vector2 lastMousePosition;
	
	private float globalStiffness;
	private float globalStiffnessRange;
	private float damping;

	public void Start()
	{
		this.simulation = this.GetComponent<TressFXSimulation>();
		this.renderer = this.GetComponent<TressFXRender>();
		
		this.damping = this.simulation.damping;
		this.globalStiffnessRange = this.simulation.globalShapeMatchingEffectiveRange;
		this.globalStiffness = this.simulation.stiffnessForGlobalShapeMatching;
	}

	public void OnGUI()
	{
		GUI.Label (new Rect(20, 10, 200, 20), "Simulation took: " + this.simulation.computationTime + " ms");
		GUI.Label (new Rect(20, 30, 200, 20), "Rendering took: " + this.renderer.renderTime + " ms");
		GUI.Label (new Rect(20, 50, 2000, 20), "Press Mouse Button 1 (right) and move the mouse to move the model");
		GUI.Label (new Rect(20, 70, 2000, 20), "Press Mouse Button 2 (wheel) and move the mouse to rotate around the model");
		
		// Options
		GUI.Label (new Rect(20, 90, 2000, 20), "Global Stiffness");
		this.globalStiffness = GUI.HorizontalSlider(new Rect(20, 110, 200, 20), this.globalStiffness, 0, 1);
		GUI.Label (new Rect(20, 130, 2000, 20), "Global Stiffness Matching Effective Range");
		this.globalStiffnessRange = GUI.HorizontalSlider(new Rect(20, 150, 200, 20), this.globalStiffnessRange, 0, 1);
		GUI.Label (new Rect(20, 170, 2000, 20), "Damping");
		this.damping = GUI.HorizontalSlider(new Rect(20, 190, 200, 20), this.damping, 0, 1);

		// Update options
		
		this.simulation.damping = this.damping;
		this.simulation.globalShapeMatchingEffectiveRange = this.globalStiffnessRange;
		this.simulation.stiffnessForGlobalShapeMatching = this.globalStiffness;
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
