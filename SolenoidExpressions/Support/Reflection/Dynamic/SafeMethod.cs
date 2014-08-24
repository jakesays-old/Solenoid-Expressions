/*
 * Copyright © 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using System;
using System.Collections;
using System.Reflection;
using Solenoid.Expressions.Support.Util;

namespace Solenoid.Expressions.Support.Reflection.Dynamic
{
	/// <summary>
	///     Safe wrapper for the dynamic method.
	/// </summary>
	/// <remarks>
	///     <see cref="SafeMethod" /> will attempt to use dynamic
	///     method if possible, but it will fall back to standard
	///     reflection if necessary.
	/// </remarks>
	public class SafeMethod : IDynamicMethod
	{
		private readonly MethodInfo _methodInfo;


		private class SafeMethodState
		{
			public readonly FunctionDelegate Method;
			public readonly object[] NullArguments;

			public SafeMethodState(FunctionDelegate method, object[] nullArguments)
			{
				Method = method;
				NullArguments = nullArguments;
			}
		}

		private class IdentityTable : Hashtable
		{
			protected override int GetHash(object key)
			{
				return key.GetHashCode();
			}

			protected override bool KeyEquals(object item, object key)
			{
				return ReferenceEquals(item, key);
			}
		}

		private static readonly Hashtable _stateCache = new IdentityTable();


		private readonly SafeMethodState _state;

		/// <summary>
		///     Creates a new instance of the safe method wrapper.
		/// </summary>
		/// <param name="methodInfo">Method to wrap.</param>
		public SafeMethod(MethodInfo methodInfo)
		{
			AssertUtils.ArgumentNotNull(methodInfo, "You cannot create a dynamic method for a null value.");

			_state = (SafeMethodState) _stateCache[methodInfo];
			if (_state == null)
			{
				var newState = new SafeMethodState(DynamicReflectionManager.CreateMethod(methodInfo),
					new object[methodInfo.GetParameters().Length]);

				lock (_stateCache.SyncRoot)
				{
					_state = (SafeMethodState) _stateCache[methodInfo];
					if (_state == null)
					{
						_state = newState;
						_stateCache[methodInfo] = _state;
					}
				}
			}

			_methodInfo = methodInfo;
		}

		/// <summary>
		///     Invokes dynamic method.
		/// </summary>
		/// <param name="target">
		///     Target object to invoke method on.
		/// </param>
		/// <param name="arguments">
		///     Method arguments.
		/// </param>
		/// <returns>
		///     A method return value.
		/// </returns>
		public object Invoke(object target, params object[] arguments)
		{
			// special case - when calling Invoke(null,null) it is undecidible if the second null is an argument or the argument array
			var nullArguments = _state.NullArguments;
			if (arguments == null && nullArguments.Length == 1)
			{
				arguments = nullArguments;
			}
			var arglen = (arguments == null ? 0 : arguments.Length);
			
			if (nullArguments.Length != arglen)
			{
				throw new ArgumentException(
					string.Format("Invalid number of arguments passed into method {0} - expected {1}, but was {2}", _methodInfo.Name,
						nullArguments.Length, arglen));
			}

			return _state.Method(target, arguments);
		}
	}
}