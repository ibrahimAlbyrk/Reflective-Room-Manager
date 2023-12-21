using UnityEngine;
using UnityEngine.UIElements;

namespace Examples.SpaceShooter.Utilities
{
    public class DrawUtilities
    {
        //Draws just the box at where it is currently hitting.
	public static void DrawBoxCastOnHit(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float hitInfoDistance, Color color)
	{
		origin = CastCenterOnCollision(origin, direction, hitInfoDistance);
		DrawBox(origin, halfExtents, orientation, color);
	}
	
	//Draws the full box from start of cast to its end distance. Can also pass in hitInfoDistance instead of full distance
	public static void DrawBoxCastBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float distance, Color color)
	{
		direction.Normalize();
		Box bottomBox = new Box(origin, halfExtents, orientation);
		Box topBox = new Box(origin + (direction * distance), halfExtents, orientation);
			
		Debug.DrawLine(bottomBox.backBottomLeft, topBox.backBottomLeft,	color);
		Debug.DrawLine(bottomBox.backBottomRight, topBox.backBottomRight, color);
		Debug.DrawLine(bottomBox.backTopLeft, topBox.backTopLeft, color);
		Debug.DrawLine(bottomBox.backTopRight, topBox.backTopRight,	color);
		Debug.DrawLine(bottomBox.frontTopLeft, topBox.frontTopLeft,	color);
		Debug.DrawLine(bottomBox.frontTopRight, topBox.frontTopRight, color);
		Debug.DrawLine(bottomBox.frontBottomLeft, topBox.frontBottomLeft, color);
		Debug.DrawLine(bottomBox.frontBottomRight, topBox.frontBottomRight,	color);
	
		DrawBox(bottomBox, color);
		DrawBox(topBox, color);
	}
	
	public static void DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color)
	{
		DrawBox(new Box(origin, halfExtents, orientation), color);
	}
	public static void DrawBox(Box box, Color color)
	{
		Debug.DrawLine(box.frontTopLeft,	 box.frontTopRight,	color);
		Debug.DrawLine(box.frontTopRight,	 box.frontBottomRight, color);
		Debug.DrawLine(box.frontBottomRight, box.frontBottomLeft, color);
		Debug.DrawLine(box.frontBottomLeft,	 box.frontTopLeft, color);
												 
		Debug.DrawLine(box.backTopLeft,		 box.backTopRight, color);
		Debug.DrawLine(box.backTopRight,	 box.backBottomRight, color);
		Debug.DrawLine(box.backBottomRight,	 box.backBottomLeft, color);
		Debug.DrawLine(box.backBottomLeft,	 box.backTopLeft, color);
												 
		Debug.DrawLine(box.frontTopLeft,	 box.backTopLeft, color);
		Debug.DrawLine(box.frontTopRight,	 box.backTopRight, color);
		Debug.DrawLine(box.frontBottomRight, box.backBottomRight, color);
		Debug.DrawLine(box.frontBottomLeft,	 box.backBottomLeft, color);
	}
	
	public struct Box
	{
		public Vector3 localFrontTopLeft     {get; private set;}
		public Vector3 localFrontTopRight    {get; private set;}
		public Vector3 localFrontBottomLeft  {get; private set;}
		public Vector3 localFrontBottomRight {get; private set;}
		public Vector3 localBackTopLeft => -localFrontBottomRight;
		public Vector3 localBackTopRight => -localFrontBottomLeft;
		public Vector3 localBackBottomLeft => -localFrontTopRight;
		public Vector3 localBackBottomRight => -localFrontTopLeft;

		public Vector3 frontTopLeft => localFrontTopLeft + origin;
		public Vector3 frontTopRight => localFrontTopRight + origin;
		public Vector3 frontBottomLeft => localFrontBottomLeft + origin;
		public Vector3 frontBottomRight => localFrontBottomRight + origin;
		public Vector3 backTopLeft => localBackTopLeft + origin;
		public Vector3 backTopRight => localBackTopRight + origin;
		public Vector3 backBottomLeft => localBackBottomLeft + origin;
		public Vector3 backBottomRight => localBackBottomRight + origin;

		public Vector3 origin {get; private set;}

		public Box(Vector3 origin, Vector3 halfExtents, Quaternion orientation) : this(origin, halfExtents)
		{
			Rotate(orientation);
		}
		public Box(Vector3 origin, Vector3 halfExtents)
		{
			localFrontTopLeft     = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
			localFrontTopRight    = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
			localFrontBottomLeft  = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
			localFrontBottomRight = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);

			this.origin = origin;
		}


		public void Rotate(Quaternion orientation)
		{
			localFrontTopLeft     = RotatePointAroundPivot(localFrontTopLeft    , Vector3.zero, orientation);
			localFrontTopRight    = RotatePointAroundPivot(localFrontTopRight   , Vector3.zero, orientation);
			localFrontBottomLeft  = RotatePointAroundPivot(localFrontBottomLeft , Vector3.zero, orientation);
			localFrontBottomRight = RotatePointAroundPivot(localFrontBottomRight, Vector3.zero, orientation);
		}
	}

	 //This should work for all cast types
	static Vector3 CastCenterOnCollision(Vector3 origin, Vector3 direction, float hitInfoDistance)
	{
		return origin + (direction.normalized * hitInfoDistance);
	}
	
	static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
	{
		Vector3 direction = point - pivot;
		return pivot + rotation * direction;
	}
    }
}