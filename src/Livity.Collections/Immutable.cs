using System;

namespace Livity.Collections
{
	public struct Immutable<T>
	{
		T _value;
		bool _hasValue;

		public T Value
		{
			get
			{
				if (!_hasValue)
					throw new InvalidOperationException("Value hasn't been set yet");
				return _value;
			}

			set
			{
				if (_hasValue)
					throw new InvalidOperationException("Value has already been set");
				_value = value;
				_hasValue = true;
			}
		}
	}
}