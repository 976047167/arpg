
using System.Collections.Generic;
using UnityEngine;
public static class RaycastUtils
{

	/// <summary>
	/// Allows for comparison between RaycastHit objects.
	/// </summary>
	public class RaycastHitComparer : IComparer<RaycastHit>
	{
		/// <summary>
		/// Compare RaycastHit x to RaycastHit y. If x has a smaller distance value compared to y then a negative value will be returned.
		/// If the distance values are equal then 0 will be returned, and if y has a smaller distance value compared to x then a positive value will be returned.
		/// </summary>
		/// <param name="x">The first RaycastHit to compare.</param>
		/// <param name="y">The second RaycastHit to compare.</param>
		/// <returns>The resulting difference between RaycastHit x and y.</returns>
		public int Compare(RaycastHit x, RaycastHit y)
		{
			if (x.transform == null)
			{
				return int.MaxValue;
			}
			if (y.transform == null)
			{
				return int.MinValue;
			}
			return x.distance.CompareTo(y.distance);
		}
	}

	/// <summary>
	/// Allows for equity comparison checks between RaycastHit objects.
	/// </summary>
	public struct RaycastHitEqualityComparer : IEqualityComparer<RaycastHit>
	{
		/// <summary>
		/// Determines if RaycastHit x is equal to RaycastHit y.
		/// </summary>
		/// <param name="x">The first RaycastHit to compare.</param>
		/// <param name="y">The second RaycastHit to compare.</param>
		/// <returns>True if the raycasts are equal.</returns>
		public bool Equals(RaycastHit x, RaycastHit y)
		{
			if (x.distance != y.distance)
			{
				return false;
			}
			if (x.point != y.point)
			{
				return false;
			}
			if (x.normal != y.normal)
			{
				return false;
			}
			if (x.transform != y.transform)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Returns a hash code for the RaycastHit.
		/// </summary>
		/// <param name="hit">The RaycastHit to get the hash code of.</param>
		/// <returns>The hash code for the RaycastHit.</returns>
		public int GetHashCode(RaycastHit hit)
		{
			// Don't use hit.GetHashCode because that has boxing. This hash function won't always prevent duplicates but it's fine for what it's used for.
			return ((int)(hit.distance * 10000)) ^ ((int)(hit.point.x * 10000)) ^ ((int)(hit.point.y * 10000)) ^ ((int)(hit.point.z * 10000)) ^
					((int)(hit.normal.x * 10000)) ^ ((int)(hit.normal.y * 10000)) ^ ((int)(hit.normal.z * 10000));
		}
	}
}