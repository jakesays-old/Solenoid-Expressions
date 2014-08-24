
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

// ReSharper disable UnusedMemberInSuper.Global

namespace Solenoid.Expressions.Support.Reflection.Dynamic
{
	/// <summary>
	/// Defines constructors that dynamic constructor class has to implement.
	/// </summary>
	public interface IDynamicConstructor
	{
		/// <summary>
		/// Invokes dynamic constructor.
		/// </summary>
		/// <param name="arguments">
		/// Constructor arguments.
		/// </param>
		/// <returns>
		/// A constructor value.
		/// </returns>
		object Invoke(object[] arguments);
	}
}