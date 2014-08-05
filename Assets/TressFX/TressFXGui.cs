using UnityEngine;
using System.Collections;

public class TressFXGui : MonoBehaviour
{
	public Transform modelTransform;

	private TressFXSimulation simulation;
	private TressFXRender renderer;

	private Vector2 lastMousePosition;

	// Windows open?
	private bool renderGuiOpen = false;
	private bool simulationGuiOpen = false;

	// Window variables
	private Rect renderWindowRect = new Rect (100, 100, 300, 300);
	private Rect simulationWindowRect = new Rect (100, 100, 400, 300);

	public void Start()
	{
		this.simulation = this.GetComponent<TressFXSimulation>();
		this.renderer = this.GetComponent<TressFXRender>();
	}

	public void OnGUI()
	{
		// Info labels
		GUI.Label (new Rect(20, 10, 200, 20), "Simulation took: " + this.simulation.computationTime + " ms");
		GUI.Label (new Rect(20, 50, 2000, 20), "Press Mouse Button 1 (right) and move the mouse to move the model");
		GUI.Label (new Rect(20, 70, 2000, 20), "Press Mouse Button 2 (wheel) and move the mouse to rotate around the model");

		// Window openers
		if (GUI.Button (new Rect(20, 100, 200, 25), "Rendering Menu"))
		{
			this.renderGuiOpen = !this.renderGuiOpen;
		}

		if (GUI.Button (new Rect(20, 130, 200, 25), "Simulation Menu"))
		{
			this.simulationGuiOpen = !this.simulationGuiOpen;
		}

		// Windows
		if (this.renderGuiOpen)
		{
			this.renderWindowRect = GUI.Window (0, this.renderWindowRect, this.RenderWindowFunc, "Rendering Menu");
		}
		if (this.simulationGuiOpen)
		{
			this.simulationWindowRect = GUI.Window (0, this.simulationWindowRect, this.SimulationWindowFunc, "Simulation Menu");
		}

		// Test
		// this.simulation.windMagnitude = Mathf.Lerp (-20, 20, Mathf.Abs(Mathf.Sin(Time.time * 2)) );
	}

	/// <summary>
	/// Renders the rendering menu window.
	/// </summary>
	/// <param name="windowId">Window identifier.</param>
	private void RenderWindowFunc(int windowId)
	{
		// Draggable
		GUI.DragWindow (new Rect (0, 0, 300, 20));

		// Color
		Color c = this.renderer.hairColor;
		
		GUI.Label (new Rect (10, 20, 100, 20), "Hair Color Red:");
		c.r = GUI.HorizontalSlider (new Rect (110, 20, 150, 20), c.r, 0, 1);
		GUI.Label (new Rect (10, 40, 100, 20), "Hair Color Green:");
		c.g = GUI.HorizontalSlider (new Rect (110, 40, 150, 20), c.g, 0, 1);
		GUI.Label (new Rect (10, 60, 100, 20), "Hair Color Blue:");
		c.b = GUI.HorizontalSlider (new Rect (110, 60, 150, 20), c.b, 0, 1);

		this.renderer.hairColor = c;

		// Parameters
		GUI.Label (new Rect (10, 80, 100, 20), "Expand pixels: ");
		this.renderer.expandPixels = GUI.Toggle (new Rect (110, 80, 150, 20), this.renderer.expandPixels, "");
		GUI.Label (new Rect (10, 100, 100, 20), "Thin Tip (Bugged): ");
		this.renderer.thinTip = GUI.Toggle (new Rect (110, 100, 150, 20), this.renderer.thinTip, "");
		GUI.Label (new Rect (10, 120, 100, 20), "Fiber Radius: ");
		this.renderer.fiberRadius = GUI.HorizontalSlider (new Rect (110, 120, 150, 20), this.renderer.fiberRadius, 0, 1);
		/*GUI.Label (new Rect (10, 140, 100, 20), "Shininess: ");
		this.renderer.shininess = GUI.HorizontalSlider (new Rect (110, 140, 150, 20), this.renderer.shininess, 0, 1);
		GUI.Label (new Rect (10, 160, 100, 20), "Gloss: ");
		this.renderer.gloss = GUI.HorizontalSlider (new Rect (110, 160, 150, 20), this.renderer.gloss, 0, 1);*/
	}

	/// <summary>
	/// Renders the simulation menu window.
	/// </summary>
	/// <param name="windowId">Window identifier.</param>
	private void SimulationWindowFunc(int windowId)
	{
		// Draggable
		GUI.DragWindow (new Rect (0, 0, 300, 20));

		// Iterations
		// Length Constraints
		GUI.Label (new Rect (10, 20, 250, 20), "Length Constraint Iterations("+this.simulation.lengthConstraintIterations+"):");
		float lengthConstraintIterations = GUI.HorizontalSlider (new Rect (260, 20, 80, 20), this.simulation.lengthConstraintIterations, 0, 20);
		this.simulation.lengthConstraintIterations = Mathf.CeilToInt (lengthConstraintIterations);

		// Local Shape Constraints
		GUI.Label (new Rect (10, 40, 250, 20), "Local Shape Constraint Iterations ("+this.simulation.localShapeConstraintIterations+"):");
		float localShapeConstraintIterations = GUI.HorizontalSlider (new Rect (260, 40, 80, 20), this.simulation.localShapeConstraintIterations, 0, 10);
		this.simulation.localShapeConstraintIterations = Mathf.CeilToInt (localShapeConstraintIterations);

		// Wind
		GUI.Label (new Rect (10, 60, 150, 20), "Wind Magnitude:");
		this.simulation.windMagnitude = GUI.HorizontalSlider (new Rect (160, 60, 180, 20), this.simulation.windMagnitude, -40, 40);
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
