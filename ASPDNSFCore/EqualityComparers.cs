// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace AspDotNetStorefrontCore
{
	public class KeyEqualityComparer<TKey, TValue> : EqualityComparer<KeyValuePair<TKey, TValue>>
	{
		readonly IEqualityComparer<TKey> Comparer;

		public KeyEqualityComparer(IEqualityComparer<TKey> comparer = null)
		{
			Comparer = comparer ?? EqualityComparer<TKey>.Default;
		}

		public override bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
		{
			return Comparer.Equals(x.Key, y.Key);
		}

		public override int GetHashCode(KeyValuePair<TKey, TValue> obj)
		{
			return Comparer.GetHashCode(obj.Key);
		}
	}

	public class MemberEqualityComparer<TObject, TCompare> : EqualityComparer<TObject>
	{
		readonly Func<TObject, TCompare> Selector;
		readonly IEqualityComparer<TCompare> Comparer;

		public MemberEqualityComparer(Func<TObject, TCompare> selector, IEqualityComparer<TCompare> comparer = null)
		{
			Selector = selector;
			Comparer = comparer ?? EqualityComparer<TCompare>.Default;
		}

		public override bool Equals(TObject x, TObject y)
		{
			return Comparer.Equals(Selector(x), Selector(y));
		}

		public override int GetHashCode(TObject obj)
		{
			return Comparer.GetHashCode(Selector(obj));
		}
	}
}
