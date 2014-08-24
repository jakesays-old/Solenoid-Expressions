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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Solenoid.Expressions.Support.Reflection.Dynamic;
using Solenoid.Expressions.Support.TypeResolution;
using Solenoid.Expressions.Support.Util;

namespace Solenoid.Expressions
{
	/// <summary>
	///     Represents parsed method node in the navigation expression.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
	[Serializable]
	public class ConstructorNode : NodeWithArguments
	{
		private SafeConstructor _constructor;
		private IDictionary _namedArgs;
		private bool _isParamArray = false;
		private Type _paramArrayType;
		private int _argumentCount;

		/// <summary>
		///     Create a new instance
		/// </summary>
		public ConstructorNode()
		{
		}

		/// <summary>
		///     Create a new instance
		/// </summary>
		public ConstructorNode(Type type)
			: base(type.FullName)
		{
		}

		/// <summary>
		///     Create a new instance from SerializationInfo
		/// </summary>
		protected ConstructorNode(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///     Creates new instance of the type defined by this node.
		/// </summary>
		/// <param name="context">Context to evaluate expressions against.</param>
		/// <param name="evalContext">Current expression evaluation context.</param>
		/// <returns>Node's value.</returns>
		protected override object Get(object context, EvaluationContext evalContext)
		{
			var argValues = ResolveArguments(evalContext);
			var namedArgValues = ResolveNamedArguments(evalContext);

			if (_constructor == null)
			{
				lock (this)
				{
					if (_constructor == null)
					{
						_constructor = InitializeNode(argValues, namedArgValues);
					}
				}
			}

			var paramValues = (_isParamArray
				? ReflectionUtils.PackageParamArray(argValues, _argumentCount, _paramArrayType)
				: argValues);
			var instance = _constructor.Invoke(paramValues);
			if (namedArgValues != null)
			{
				SetNamedArguments(instance, namedArgValues);
			}

			return instance;
		}

		/// <summary>
		///     Determines the type of object that should be instantiated.
		/// </summary>
		/// <param name="typeName">
		///     The type name to resolve.
		/// </param>
		/// <returns>
		///     The type of object that should be instantiated.
		/// </returns>
		/// <exception cref="TypeLoadException">
		///     If the type cannot be resolved.
		/// </exception>
		protected virtual Type GetObjectType(string typeName)
		{
			return TypeResolutionUtils.ResolveType(typeName);
		}

		/// <summary>
		///     Initializes this node by caching necessary constructor and property info.
		/// </summary>
		/// <param name="argValues"></param>
		/// <param name="namedArgValues"></param>
		private SafeConstructor InitializeNode(object[] argValues, IDictionary namedArgValues)
		{
			SafeConstructor ctor = null;
			var objectType = GetObjectType(getText().Trim());

			// cache constructor info
			var ci = GetBestConstructor(objectType, argValues);
			if (ci == null)
			{
				throw new ArgumentException(
					String.Format("Constructor for the type [{0}] with a specified " +
								"number and types of arguments does not exist.",
						objectType.FullName));
			}
			var parameters = ci.GetParameters();
			if (parameters.Length > 0)
			{
				var lastParameter = parameters[parameters.Length - 1];
				_isParamArray = lastParameter.GetCustomAttributes(typeof (ParamArrayAttribute), false).Length > 0;
				if (_isParamArray)
				{
					_paramArrayType = lastParameter.ParameterType.GetElementType();
					_argumentCount = parameters.Length;
				}
			}
			ctor = new SafeConstructor(ci);

			// cache named args info
			if (namedArgValues != null)
			{
				_namedArgs = new Hashtable(namedArgValues.Count);
				foreach (string name in namedArgValues.Keys)
				{
					_namedArgs[name] = Expression.ParseProperty(name);
				}
			}

			return ctor;
		}

		/// <summary>
		///     Sets the named arguments (properties).
		/// </summary>
		/// <param name="instance">Instance to set property values on.</param>
		/// <param name="namedArgValues">Argument (property) name to value mappings.</param>
		private void SetNamedArguments(object instance, IDictionary namedArgValues)
		{
			foreach (string name in namedArgValues.Keys)
			{
				var property = (IExpression) _namedArgs[name];
				property.SetValue(instance, namedArgValues[name]);
			}
		}

		private static ConstructorInfo GetBestConstructor(Type type, object[] argValues)
		{
			var candidates = GetCandidateConstructors(type, argValues.Length);
			if (candidates.Count > 0)
			{
				return ReflectionUtils.GetConstructorByArgumentValues(candidates, argValues);
			}
			return null;
		}

		private static IList<ConstructorInfo> GetCandidateConstructors(Type type, int argCount)
		{
			var ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var matches = new List<ConstructorInfo>();

			foreach (var ctor in ctors)
			{
				var parameters = ctor.GetParameters();
				if (parameters.Length == argCount)
				{
					matches.Add(ctor);
				}
				else if (parameters.Length > 0)
				{
					var lastParameter = parameters[parameters.Length - 1];
					if (lastParameter.GetCustomAttributes(typeof (ParamArrayAttribute), false).Length > 0)
					{
						matches.Add(ctor);
					}
				}
			}

			return matches;
		}
	}
}