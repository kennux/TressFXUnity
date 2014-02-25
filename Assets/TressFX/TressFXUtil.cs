using System;
using System.Collections.Generic;
using UnityEngine;

public class TressFXUtil
{
	public static Quaternion QuaternionFromAngleAxis(float angle_radian, Vector3 axis)
	{
		Quaternion ret = new Quaternion();

		float halfAng = 0.5f * angle_radian;
		float sinHalf = Mathf.Sin(halfAng);
		ret.w = Mathf.Cos(halfAng);

		ret.x = sinHalf * axis.x;
		ret.y = sinHalf * axis.y;
		ret.z = sinHalf * axis.z;

		return ret;
	}
}

/// <summary>
/// Matrix3x3 implementation.
/// Adaptive to unity's Matrix4x4.
/// </summary>
public class Matrix3x3
{
	public float[,] matrixData;
	
	public Matrix3x3()
	{
		this.matrixData = new float[3, 3];
	}
	
	public Matrix3x3(float[,] matrixData)
	{
		this.matrixData = matrixData;
	}
	
	/// <summary>
	/// Inverse this instance.
	/// </summary>
	public void Inverse()
	{
		float det = this.matrixData[0, 0] * (this.matrixData[2,2] * this.matrixData[1,1] - this.matrixData[2,1] * this.matrixData[1,2]) - 
			this.matrixData[1,0] * ( this.matrixData[2,2] * this.matrixData[0,1] - this.matrixData[2,1] * this.matrixData[0,2] ) +
				this.matrixData[2,0] * ( this.matrixData[1,2] * this.matrixData[0,1] - this.matrixData[1,1] * this.matrixData[0,2] );
		
		this.matrixData[0,0] = this.matrixData[2,2] * this.matrixData[1,1] - this.matrixData[2,1] * this.matrixData[1,2];
		this.matrixData[0,1] = -this.matrixData[2,2] * this.matrixData[0,1] - this.matrixData[2,1] * this.matrixData[0,2];
		this.matrixData[0,2] = this.matrixData[1,2] * this.matrixData[0,1] - this.matrixData[1,1] * this.matrixData[0,2];
		
		this.matrixData[1,0] = -this.matrixData[2,2] * this.matrixData[1,0] - this.matrixData[2,0] * this.matrixData[1,2];
		this.matrixData[1,1] = this.matrixData[2,2] * this.matrixData[0,0] - this.matrixData[2,0] * this.matrixData[0,2];
		this.matrixData[1,2] = -this.matrixData[1,2] * this.matrixData[0,0] - this.matrixData[1,0] * this.matrixData[0,2];
		
		this.matrixData[2,0] = this.matrixData[2,1] * this.matrixData[1,0] - this.matrixData[2,0] * this.matrixData[1,1];
		this.matrixData[2,1] = -this.matrixData[2,1] * this.matrixData[0,0] - this.matrixData[2,0] * this.matrixData[0,1];
		this.matrixData[2,2] = this.matrixData[1,1] * this.matrixData[0,0] - this.matrixData[1,0] * this.matrixData[0,1];
		
		this.Multiply(1.0f/det);
	}
	
	/// <summary>
	/// Multiply the matrix with the specified scalar.
	/// </summary>
	/// <param name="val">Value.</param>
	public void Multiply(float val)
	{
		for (int i = 0; i < 3; i++)
			for (int j = 0; j < 3; j++)
				this.matrixData[i, j] *= val;
	}
	
	/// <summary>
	/// Multiply the given vector with the matrix.
	/// New vector is returned!
	/// </summary>
	/// <param name="vec">Vec.</param>
	public Vector3 Multiply(Vector3 vec)
	{
		Vector3 ret = new Vector3();
		
		ret.x = this.matrixData[0,0] * vec.x + this.matrixData[0,1] * vec.y + this.matrixData[0,2] * vec.z;
		ret.y = this.matrixData[1,0] * vec.x + this.matrixData[1,1] * vec.y + this.matrixData[1,2] * vec.z;
		ret.z = this.matrixData[2,0] * vec.x + this.matrixData[2,1] * vec.y + this.matrixData[2,2] * vec.z;
		
		return ret;
	}
	
	/// <summary>
	/// Transpose a new matrix3x3 instance
	/// </summary>
	public Matrix3x3 Transpose()
	{
		Matrix3x3 ret = new Matrix3x3();
		
		for ( int i = 0; i < 3; i++ )
			for ( int j = 0; j < 3; j++ )
				ret.matrixData[i,j] = this.matrixData[j,i];
		
		return ret;
	}
	
	/// <summary>
	/// Calculates a quaternion with this matrix as rotation matrix
	/// </summary>
	/// <returns>The quaternion.</returns>
	public Quaternion ToQuaternion()
	{
		Quaternion quaternion = new Quaternion();
		
		float fTrace = this.matrixData[0,0]+this.matrixData[1,1]+this.matrixData[2,2];
		float fRoot;
		
		if ( fTrace > 0.0f )
		{
			// |w| > 1/2, may as well choose w > 1/2
			fRoot = Mathf.Sqrt(fTrace + 1.0f);  // 2w
			quaternion.w = 0.5f*fRoot;
			fRoot = 0.5f/fRoot;  // 1/(4w)
			quaternion.x = (this.matrixData[2,1]-this.matrixData[1,2])*fRoot;
			quaternion.y = (this.matrixData[0,2]-this.matrixData[2,0])*fRoot;
			quaternion.z = (this.matrixData[1,0]-this.matrixData[0,1])*fRoot;
		}
		else
		{
			// |w| <= 1/2
			int[] s_iNext = new int[] { 1, 2, 0 };
			int i = 0;
			if ( this.matrixData[1,1] > this.matrixData[0,0] )
				i = 1;
			if ( this.matrixData[2,2] > this.matrixData[i,i])
				i = 2;
			int j = s_iNext[i];
			int k = s_iNext[j];
			/*float[] apkQuat = new float[]{ quaternion.x, quaternion.y, quaternion.z };*/
			
			fRoot = Mathf.Sqrt(this.matrixData[i,i]-this.matrixData[j,j]-this.matrixData[k,k] + 1.0f);
			// this.QuaternionHelper(i, ref quaternion, 0.5f*fRoot);
			float val = 0.5f*fRoot;

			if (i == 0)
			{
				quaternion.x = val;
			}
			else if (i == 1)
			{
				quaternion.y = val;
			}
			else if (i == 2)
			{
				quaternion.z = val;
			}

			fRoot = 0.5f/fRoot;
			quaternion.w = (this.matrixData[k,j]-this.matrixData[j,k])*fRoot;

			val = (this.matrixData[j,i]+this.matrixData[i,j])*fRoot;
			if (j == 0)
			{
				quaternion.x = val;
			}
			else if (j == 1)
			{
				quaternion.y = val;
			}
			else if (j == 2)
			{
				quaternion.z = val;
			}

			val = (this.matrixData[k,i]+this.matrixData[i,k])*fRoot;
			if (k == 0)
			{
				quaternion.x = val;
			}
			else if (k == 1)
			{
				quaternion.y = val;
			}
			else if (k == 2)
			{
				quaternion.z = val;
			}
		}
		
		return quaternion;
	}

	private void QuaternionHelper(int index, ref Quaternion quaternion, float val)
	{
		switch (index)
		{
		case 0:
			quaternion.x = val;
			break;

		case 1:
			quaternion.y = val;
			break;

		case 2:
			quaternion.z = val;
			break;
		}
	}

	/// <summary>
	/// Multiply the specified matrix with the given scalar val.
	/// This will return a new matrix3x3 instance!
	/// </summary>
	/// <param name="matrix">Matrix.</param>
	/// <param name="val">Value.</param>
	public static Matrix3x3 Multiply (Matrix3x3 matrix, float val)
	{
		Matrix3x3 ret = new Matrix3x3();
		ret.Multiply(val);
		
		return ret;
	}
}
