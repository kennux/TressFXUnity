using UnityEngine;
using System.Collections;

public class Matrix3x3
{
	public float[][] e;

	public Matrix3x3()
	{
		this.Init();
	}
	
	public Matrix3x3(float[][] data)
	{
		this.e = data;
	}
	
	public Matrix3x3(float[,] data)
	{
		this.Init();

		for (int i = 0; i < 3; i++)
			for (int j = 0; j < 3; j++)
				this.e[i][j] = data[i,j];
	}

	private void Init()
	{
		this.e = new float[3][];
		for (int i = 0; i < 3; i++)
			this.e[i] = new float[3];
	}
	
	public Matrix3x3 Inverse()
	{
		float det = e[0][0] * ( e[2][2] * e[1][1] - e[2][1] * e[1][2] ) - 
			e[1][0] * ( e[2][2] * e[0][1] - e[2][1] * e[0][2] ) +
				e[2][0] * ( e[1][2] * e[0][1] - e[1][1] * e[0][2] );
		
		e[0][0] = e[2][2] * e[1][1] - e[2][1] * e[1][2];
		e[0][1] = -e[2][2] * e[0][1] - e[2][1] * e[0][2];
		e[0][2] = e[1][2] * e[0][1] - e[1][1] * e[0][2];
		
		e[1][0] = -e[2][2] * e[1][0] - e[2][0] * e[1][2];
		e[1][1] = e[2][2] * e[0][0] - e[2][0] * e[0][2];
		e[1][2] = -e[1][2] * e[0][0] - e[1][0] * e[0][2];
		
		e[2][0] = e[2][1] * e[1][0] - e[2][0] * e[1][1];
		e[2][1] = -e[2][1] * e[0][0] - e[2][0] * e[0][1];
		e[2][2] = e[1][1] * e[0][0] - e[1][0] * e[0][1];

		return this.Multiply(1.0f/det);
	}

	public Matrix3x3 Multiply(float val)
	{
		Matrix3x3 ret = new Matrix3x3();
		
		for ( int i = 0; i < 3; i++ )
			for ( int j = 0; j < 3; j++ )
				ret.e[i][j] = e[i][j] * val;
		
		return ret;
	}

	public Vector3 Multiply(Vector3 vec)
	{
		Vector3 ret = new Vector3();

		ret.x = e[0][0] * vec.x + e[0][1] * vec.y + e[0][2] * vec.z;
		ret.y = e[1][0] * vec.x + e[1][1] * vec.y + e[1][2] * vec.z;
		ret.y = e[2][0] * vec.x + e[2][1] * vec.y + e[2][2] * vec.z;

		return ret;
	}

	public Matrix3x3 Transpose()
	{
		return this.Inverse();
	}
}
