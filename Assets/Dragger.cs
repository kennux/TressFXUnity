using UnityEngine;
using System.Collections;

public class Dragger : MonoBehaviour
{
	public Transform tressfxModel;
	public float speed = 100;

	public void FixedUpdate()
	{
		if (!Input.GetMouseButton (1))
			return;

		float mouseX = Input.GetAxis ("Mouse X");
		float mouseY = Input.GetAxis ("Mouse Y");

		this.tressfxModel.position = new Vector3
		(
			this.tressfxModel.position.x + (-mouseX * this.speed * Time.deltaTime),
			this.tressfxModel.position.y + (mouseY * this.speed * Time.deltaTime),
			this.tressfxModel.position.z
		);
	}
}
