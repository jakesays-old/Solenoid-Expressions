
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
    /// Factory class for dynamic methods.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class DynamicMethod : BaseDynamicMember
    {
        /// <summary>
        /// Creates dynamic method instance for the specified <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="method">Method info to create dynamic method for.</param>
        /// <returns>Dynamic method for the specified <see cref="MethodInfo"/>.</returns>
        public static IDynamicMethod Create(MethodInfo method)
        {
            AssertUtils.ArgumentNotNull(method, "You cannot create a dynamic method for a null value.");

            IDynamicMethod dynamicMethod = new SafeMethod(method);
            return dynamicMethod;
        }
    }
}
