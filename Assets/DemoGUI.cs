using UnityEngine;
using System.Collections;

public class DemoGUI : MonoBehaviour
{
	public TressFXRender renderInstance;
	public TressFXSimulation simulationInstance;

	public void OnGUI()
	{
		// Render settings
		GUI.Label (new Rect (10, 10, 200, 20), "Render settings: ");

		GUI.Label (new Rect (10, 30, 200, 20), "Hair Alpha (" + this.renderInstance.fiberAlpha + "): ");
		this.renderInstance.fiberAlpha = GUI.HorizontalSlider (new Rect (10, 50, 200, 10), this.renderInstance.fiberAlpha, 0, 1.0f);
		
		GUI.Label (new Rect (10, 70, 200, 20), "Fiber radius (" + this.renderInstance.fiberRadius + "): ");
		this.renderInstance.fiberRadius = GUI.HorizontalSlider (new Rect (10, 90, 200, 10), this.renderInstance.fiberRadius, 0, 1.0f);
		
		GUI.Label (new Rect (10, 110, 200, 20), "Thin tip: ");
		if (GUI.Button(new Rect (10, 130, 200, 20), this.renderInstance.thinTip ? "On" : "Off"))
		{
			this.renderInstance.thinTip = !this.renderInstance.thinTip;
		}
		
		GUI.Label (new Rect (10, 150, 200, 20), "Expand pixels: ");
		if (GUI.Button(new Rect (10, 170, 200, 20), this.renderInstance.expandPixels ? "On" : "Off"))
		{
			this.renderInstance.expandPixels = !this.renderInstance.expandPixels;
		}
		
		GUI.Label (new Rect (10, 190, 200, 20), "Cast shadows: ");
		if (GUI.Button(new Rect (10, 210, 200, 20), this.renderInstance.castShadows ? "On" : "Off"))
		{
			this.renderInstance.castShadows = !this.renderInstance.castShadows;
		}
		
		GUI.Label (new Rect (10, 230, 200, 20), "Alpha threshold (" + this.renderInstance.alphaThreshold + "): ");
		this.renderInstance.alphaThreshold = GUI.HorizontalSlider (new Rect (10, 250, 200, 10), this.renderInstance.alphaThreshold, 0, 1.0f);

		// Material settings
		int yPos = 280;
		GUI.Label (new Rect (10, yPos, 200, 20), "Kajiya-kay settings: ");
		yPos += 20;
		
		GUI.Label (new Rect (10, yPos, 200, 20), "Ka (" + this.renderInstance.g_MatKa + ": ");
		yPos += 20;
		this.renderInstance.g_MatKa = GUI.HorizontalSlider (new Rect (10, yPos, 200, 10), this.renderInstance.g_MatKa, 0, 1.0f);
		yPos += 20;
		
		GUI.Label (new Rect (10, yPos, 200, 20), "Kd (" + this.renderInstance.g_MatKd + ": ");
		yPos += 20;
		this.renderInstance.g_MatKd = GUI.HorizontalSlider (new Rect (10, yPos, 200, 10), this.renderInstance.g_MatKd, 0, 1.0f);
		yPos += 20;
		
		GUI.Label (new Rect (10, yPos, 200, 20), "Ks1 (" + this.renderInstance.g_MatKs1 + ": ");
		yPos += 20;
		this.renderInstance.g_MatKs1 = GUI.HorizontalSlider (new Rect (10, yPos, 200, 10), this.renderInstance.g_MatKs1, 0, 1.0f);
		yPos += 20;
		
		GUI.Label (new Rect (10, yPos, 200, 20), "Ks2 (" + this.renderInstance.g_MatKs2 + ": ");
		yPos += 20;
		this.renderInstance.g_MatKs2 = GUI.HorizontalSlider (new Rect (10, yPos, 200, 10), this.renderInstance.g_MatKs2, 0, 1.0f);
		yPos += 20;
		
		GUI.Label (new Rect (10, yPos, 200, 20), "Ex1 (" + this.renderInstance.g_MatEx1 + ": ");
		yPos += 20;
		this.renderInstance.g_MatEx1 = GUI.HorizontalSlider (new Rect (10, yPos, 200, 10), this.renderInstance.g_MatEx1, 0, 100.0f);
		yPos += 20;
		
		GUI.Label (new Rect (10, yPos, 200, 20), "Ex2 (" + this.renderInstance.g_MatEx2 + ": ");
		yPos += 20;
		this.renderInstance.g_MatEx2 = GUI.HorizontalSlider (new Rect (10, yPos, 200, 10), this.renderInstance.g_MatEx2, 0, 10.0f);
		yPos += 20;
		
		GUI.Label (new Rect (10, yPos, 200, 20), "Hair Color Red (" + this.renderInstance.hairColor.r + ": ");
		yPos += 20;
		this.renderInstance.hairColor.r = GUI.HorizontalSlider (new Rect (10, yPos, 200, 10), this.renderInstance.hairColor.r, 0, 1.0f);
		yPos += 20;
		
		GUI.Label (new Rect (10, yPos, 200, 20), "Hair Color Green (" + this.renderInstance.hairColor.g + ": ");
		yPos += 20;
		this.renderInstance.hairColor.g = GUI.HorizontalSlider (new Rect (10, yPos, 200, 10), this.renderInstance.hairColor.g, 0, 1.0f);
		yPos += 20;
		
		GUI.Label (new Rect (10, yPos, 200, 20), "Hair Color Blue (" + this.renderInstance.hairColor.b + ": ");
		yPos += 20;
		this.renderInstance.hairColor.b = GUI.HorizontalSlider (new Rect (10, yPos, 200, 10), this.renderInstance.hairColor.b, 0, 1.0f);
		yPos += 20;


		// Simulation settings
		GUI.Label (new Rect (Screen.width - 210.0f, 10, 200, 20), "Simulation settings: ");
		
		GUI.Label (new Rect (Screen.width - 210.0f, 30, 200, 20), "Simulate: ");
		if (GUI.Button(new Rect (Screen.width - 210.0f, 50, 200, 20), this.simulationInstance.isWarping ? "Off" : "On"))
		{
			this.simulationInstance.isWarping = !this.simulationInstance.isWarping;
		}
		
		GUI.Label (new Rect (Screen.width - 210.0f, 70, 200, 20), "Local Shape Iterations (" + this.simulationInstance.localShapeConstraintIterations + "): ");
		this.simulationInstance.localShapeConstraintIterations = (int)GUI.HorizontalSlider (new Rect (Screen.width - 210.0f, 90, 200, 10), this.simulationInstance.localShapeConstraintIterations, 0, 10.0f);
		
		GUI.Label (new Rect (Screen.width - 210.0f, 110, 200, 20), "Length Iterations (" + this.simulationInstance.lengthConstraintIterations + "): ");
		this.simulationInstance.lengthConstraintIterations = (int)GUI.HorizontalSlider (new Rect (Screen.width - 210.0f, 130, 200, 10), this.simulationInstance.lengthConstraintIterations, 0, 10.0f);
		
		GUI.Label (new Rect (Screen.width - 210.0f, 150, 200, 20), "Wind Direction X (" + this.simulationInstance.windDirection.x + "): ");
		this.simulationInstance.windDirection.x = (int)GUI.HorizontalSlider (new Rect (Screen.width - 210.0f, 170, 200, 10), this.simulationInstance.windDirection.x, 0, 1.0f);

		GUI.Label (new Rect (Screen.width - 210.0f, 190, 200, 20), "Wind Direction Y (" + this.simulationInstance.windDirection.y + "): ");
		this.simulationInstance.windDirection.y = (int)GUI.HorizontalSlider (new Rect (Screen.width - 210.0f, 210, 200, 10), this.simulationInstance.windDirection.y, 0, 1.0f);
		
		GUI.Label (new Rect (Screen.width - 210.0f, 230, 200, 20), "Wind Direction Z (" + this.simulationInstance.windDirection.z + "): ");
		this.simulationInstance.windDirection.z = (int)GUI.HorizontalSlider (new Rect (Screen.width - 210.0f, 250, 200, 10), this.simulationInstance.windDirection.z, 0, 1.0f);
		
		GUI.Label (new Rect (Screen.width - 210.0f, 270, 200, 20), "Wind Magnitude (" + this.simulationInstance.windMagnitude + "): ");
		this.simulationInstance.windMagnitude = (int)GUI.HorizontalSlider (new Rect (Screen.width - 210.0f, 290, 200, 10), this.simulationInstance.windMagnitude , -50.0f, 50.0f);
	}
}
