using System;
using System.Collections.Generic;

namespace RngSpec
{
	public static class LinqExtensions
	{

		public static T Max<T>(this IEnumerable<T> source, Comparison<T> comparison)
		{
			var first = true;
			var max = default(T);
			foreach (var o in source)
			{
				if (first || comparison(o, max) > 0)
					max = o;
				first = false;
			}
			return max;
		}
	}
}
