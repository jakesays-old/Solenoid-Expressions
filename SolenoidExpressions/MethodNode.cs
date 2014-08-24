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
using Solenoid.Expressions.Extensions;
using Solenoid.Expressions.Support.Reflection.Dynamic;
using Solenoid.Expressions.Support.Util;

namespace Solenoid.Expressions
{
    /// <summary>
    /// Represents parsed method node in the navigation expression.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class MethodNode : NodeWithArguments
    {
        private const BindingFlags DefaultBindingFlags
            = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static
            | BindingFlags.IgnoreCase;

        private static readonly Dictionary<string, ICollectionExtension> _collectionProcessorMap = 
			new Dictionary<string, ICollectionExtension>();
        private static readonly Dictionary<string, IMethodCallExtension> _extensionMethodProcessorMap =
			new Dictionary<string, IMethodCallExtension>();

        private bool _initialized = false;
        private bool _cachedIsParamArray = false;
        private Type _paramArrayType;
        private int _argumentCount;
        private SafeMethod _cachedInstanceMethod;
        private int _cachedInstanceMethodHash;

        /// <summary>
        /// Static constructor. Initializes a map of special collection processor methods.
        /// </summary>
        static MethodNode()
        {
            _collectionProcessorMap.Add("count", new CountAggregator());
            _collectionProcessorMap.Add("sum", new SumAggregator());
            _collectionProcessorMap.Add("max", new MaxAggregator());
            _collectionProcessorMap.Add("min", new MinAggregator());
            _collectionProcessorMap.Add("average", new AverageAggregator());
            _collectionProcessorMap.Add("sort", new SortExtension());
            _collectionProcessorMap.Add("orderBy", new OrderByExtension());
            _collectionProcessorMap.Add("distinct", new DistinctExtension());
            _collectionProcessorMap.Add("nonNull", new NonNullExtension());
            _collectionProcessorMap.Add("reverse", new ReverseExtension());
            _collectionProcessorMap.Add("convert", new ConversionExtension());

            _extensionMethodProcessorMap.Add("date", new DateConversionExtension());
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        public MethodNode()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected MethodNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns node's value for the given context.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            var methodName = getText();
            var argValues = ResolveArguments(evalContext);

	        // resolve method, if necessary
            lock (this)
            {
				ICollectionExtension localCollectionExtension = null;
				// check if it is a collection and the methodname denotes a collection processor
                if ((context == null || context is ICollection))
                {
                    // predefined collection processor?
					if (!_collectionProcessorMap.TryGetValue(methodName, out localCollectionExtension) && 
						evalContext.Variables != null)
                    {
						// user-defined collection processor?
						object temp;
                        evalContext.Variables.TryGetValue(methodName, out temp);
                        localCollectionExtension = temp as ICollectionExtension;
                    }
                }

				if (localCollectionExtension != null)
				{
					return localCollectionExtension.Execute((ICollection) context, argValues);
				}

                // try extension methods

	            IMethodCallExtension methodCallExtension = null;
	            if (!_extensionMethodProcessorMap.TryGetValue(methodName, out methodCallExtension)
					&& evalContext.Variables != null)
	            {
		            // user-defined extension method processor?
		            object temp;
		            evalContext.Variables.TryGetValue(methodName, out temp);
		            methodCallExtension = temp as IMethodCallExtension;
	            }

				if (methodCallExtension != null)
				{
					return methodCallExtension.Execute(context, argValues);
				}

	            // try instance method
                if (context != null)
                {
                    // calculate checksum, if the cached method matches the current context
                    if (_initialized)
                    {
                        var calculatedHash = CalculateMethodHash(context.GetType(), argValues);
                        _initialized = (calculatedHash == _cachedInstanceMethodHash);
                    }

                    if (!_initialized)
                    {
                        Initialize(methodName, argValues, context);
                        _initialized = true;
                    }
                }
            }

	        if (_cachedInstanceMethod != null)
	        {
		        var paramValues = (_cachedIsParamArray)
			        ? ReflectionUtils.PackageParamArray(argValues, _argumentCount, _paramArrayType)
			        : argValues;
		        return _cachedInstanceMethod.Invoke(context, paramValues);
	        }

	        throw new ArgumentException(string.Format("Method '{0}' with the specified number and types of arguments does not exist.", methodName));
        }

        private int CalculateMethodHash(Type contextType, object[] argValues)
        {
            var hash = contextType.GetHashCode();
            for (var i = 0; i < argValues.Length; i++)
            {
                var arg = argValues[i];
	            if (arg != null)
	            {
		            hash += _primeNumbers[i] * arg.GetType().GetHashCode();
	            }
            }
            return hash;
        }

        private void Initialize(string methodName, object[] argValues, object context)
        {
            var contextType = (context is Type ? context as Type : context.GetType());

            // check the context type first
            var mi = GetBestMethod(contextType, methodName, DefaultBindingFlags, argValues);

            // if not found, probe the Type's type          
            if (mi == null)
            {
                mi = GetBestMethod(typeof(Type), methodName, DefaultBindingFlags, argValues);
            }

            if (mi == null)
            {
                return;
            }
	        var parameters = mi.GetParameters();
	        if (parameters.Length > 0)
	        {
		        var lastParameter = parameters[parameters.Length - 1];
		        _cachedIsParamArray = lastParameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
		        if (_cachedIsParamArray)
		        {
			        _paramArrayType = lastParameter.ParameterType.GetElementType();
			        _argumentCount = parameters.Length;
		        }
	        }

	        _cachedInstanceMethod = new SafeMethod(mi);
	        _cachedInstanceMethodHash = CalculateMethodHash(contextType, argValues);
        }

        /// <summary>
        /// Gets the best method given the name, argument values, for a given type.
        /// </summary>
        /// <param name="type">The type on which to search for the method.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="argValues">The arg values.</param>
        /// <returns>Best matching method or null if none found.</returns>
        public static MethodInfo GetBestMethod(Type type, string methodName, BindingFlags bindingFlags, object[] argValues)
        {
            MethodInfo mi = null;
            try
            {
                mi = type.GetMethod(methodName, bindingFlags | BindingFlags.FlattenHierarchy);
            }
            catch (AmbiguousMatchException)
            {

                var overloads = GetCandidateMethods(type, methodName, bindingFlags, argValues.Length);
                if (overloads.Count > 0)
                {
                    mi = ReflectionUtils.GetMethodByArgumentValues(overloads, argValues);
                }
            }
            return mi;
        }



        private static IList<MethodInfo> GetCandidateMethods(Type type, string methodName, BindingFlags bindingFlags, int argCount)
        {
            var methods = type.GetMethods(bindingFlags | BindingFlags.FlattenHierarchy);
            var matches = new List<MethodInfo>();

            foreach (var method in methods)
            {
                if (method.Name == methodName)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == argCount)
                    {
                        matches.Add(method);
                    }
                    else if (parameters.Length > 0)
                    {
                        var lastParameter = parameters[parameters.Length - 1];
                        if (lastParameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                        {
                            matches.Add(method);
                        }
                    }
                }
            }

            return matches;
        }

        // used to calculate signature hash while caring for arg positions
	    private static readonly int[] _primeNumbers =
	    {
		    17, 19, 23, 29
		    , 31, 37, 41, 43, 47, 53, 59, 61, 67, 71
		    , 73, 79, 83, 89, 97, 101, 103, 107, 109, 113
		    , 127, 131, 137, 139, 149, 151, 157, 163, 167, 173
		    , 179, 181, 191, 193, 197, 199, 211, 223, 227, 229
		    , 233, 239, 241, 251, 257, 263, 269, 271, 277, 281
		    , 283, 293, 307, 311, 313, 317, 331, 337, 347, 349
		    , 353, 359, 367, 373, 379, 383, 389, 397, 401, 409
		    , 419, 421, 431, 433, 439, 443, 449, 457, 461, 463
		    , 467, 479, 487, 491, 499, 503, 509, 521, 523, 541
		    , 547, 557, 563, 569, 571, 577, 587, 593, 599, 601
		    , 607, 613, 617, 619, 631, 641, 643, 647, 653, 659
		    , 661, 673, 677, 683, 691, 701, 709, 719, 727, 733
	    };
    }
}
