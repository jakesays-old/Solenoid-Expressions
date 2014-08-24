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

// ReSharper disable UnusedMember.Global

using System.Reflection;
using Solenoid.Expressions.Support.Util;


namespace Solenoid.Expressions.Support.Reflection.Dynamic
{
	/// <summary>
	///     Factory class for dynamic properties.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
	public class DynamicProperty : BaseDynamicMember
	{
		/// <summary>
		///     Creates safe dynamic property instance for the specified <see cref="PropertyInfo" />.
		/// </summary>
		/// <remarks>
		///     <p>This factory method will create a dynamic property with a "safe" wrapper.</p>
		///     <p>
		///         Safe wrapper will attempt to use generated dynamic property if possible,
		///         but it will fall back to standard reflection if necessary.
		///     </p>
		/// </remarks>
		/// <param name="property">Property info to create dynamic property for.</param>
		/// <returns>Safe dynamic property for the specified <see cref="PropertyInfo" />.</returns>
		/// <seealso cref="SafeProperty" />
		public static IDynamicProperty CreateSafe(PropertyInfo property)
		{
			return new SafeProperty(property);
		}

		/// <summary>
		///     Creates dynamic property instance for the specified <see cref="PropertyInfo" />.
		/// </summary>
		/// <param name="property">Property info to create dynamic property for.</param>
		/// <returns>Dynamic property for the specified <see cref="PropertyInfo" />.</returns>
		public static IDynamicProperty Create(PropertyInfo property)
		{
			AssertUtils.ArgumentNotNull(property, "You cannot create a dynamic property for a null value.");

			IDynamicProperty dynamicProperty = new SafeProperty(property);
			return dynamicProperty;
		}
	}
}