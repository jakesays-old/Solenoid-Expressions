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
	///     Factory class for dynamic constructors.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
	public class DynamicConstructor : BaseDynamicMember
	{
		/// <summary>
		///     Creates dynamic constructor instance for the specified <see cref="ConstructorInfo" />.
		/// </summary>
		/// <param name="constructorInfo">Constructor info to create dynamic constructor for.</param>
		/// <returns>Dynamic constructor for the specified <see cref="ConstructorInfo" />.</returns>
		public static IDynamicConstructor Create(ConstructorInfo constructorInfo)
		{
			AssertUtils.ArgumentNotNull(constructorInfo, "You cannot create a dynamic constructor for a null value.");

			return new SafeConstructor(constructorInfo);
		}
	}
}