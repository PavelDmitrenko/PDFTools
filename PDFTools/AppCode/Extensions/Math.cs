using System;

namespace PDFTools.AppCode.Extensions
{
	public static class MathExtensions
	{
		public static bool ApproximatelyEquals(this double value1, double value2, double acceptableDifference)
		{
			return Math.Abs(value1 - value2) <= acceptableDifference;
		}

		public static bool ApproximatelyEquals(this float value1, float value2, float acceptableDifference)
		{
			return ApproximatelyEquals(value1, value2, (double) acceptableDifference);
		}
	}
}
