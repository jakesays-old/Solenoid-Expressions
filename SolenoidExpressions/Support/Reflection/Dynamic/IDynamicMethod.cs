
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
	/// Defines methods that dynamic method class has to implement.
	/// </summary>
	public interface IDynamicMethod
	{
		/// <summary>
		/// Invokes dynamic method on the specified target object.
		/// </summary>
		/// <param name="target">
		/// Target object to invoke method on.
		/// </param>
		/// <param name="arguments">
		/// Method arguments.
		/// </param>
		/// <returns>
		/// A method return value.
		/// </returns>
		object Invoke(object target, params object[] arguments);
	}
}