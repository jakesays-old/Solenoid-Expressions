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
using System.Runtime.Serialization;
using Solenoid.Expressions.Support.Collections;
using Solenoid.Expressions.Support.Util;

namespace Solenoid.Expressions
{
	/// <summary>
	///     Represents arithmetic subtraction operator.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
	[Serializable]
	public class OpSubtract : BinaryOperator
	{
		/// <summary>
		///     Create a new instance
		/// </summary>
		public OpSubtract()
		{
		}

		/// <summary>
		///     Create a new instance from SerializationInfo
		/// </summary>
		protected OpSubtract(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///     Returns a value for the arithmetic subtraction operator node.
		/// </summary>
		/// <param name="context">Context to evaluate expressions against.</param>
		/// <param name="evalContext">Current expression evaluation context.</param>
		/// <returns>Node's value.</returns>
		protected override object Get(object context, EvaluationContext evalContext)
		{
			var lhs = GetLeftValue(context, evalContext);
			var rhs = GetRightValue(context, evalContext);

			if (NumberUtils.IsNumber(lhs) && NumberUtils.IsNumber(rhs))
			{
				return NumberUtils.Subtract(lhs, rhs);
			}
			if (lhs is DateTime &&
				(rhs is TimeSpan ||
				rhs is string ||
				NumberUtils.IsNumber(rhs)))
			{
				if (NumberUtils.IsNumber(rhs))
				{
					rhs = TimeSpan.FromDays(Convert.ToDouble(rhs));
				}
				else if (rhs is string)
				{
					rhs = TimeSpan.Parse((string) rhs);
				}
				return (DateTime) lhs - (TimeSpan) rhs;
			}
			if (lhs is DateTime && rhs is DateTime)
			{
				return (DateTime) lhs - (DateTime) rhs;
			}
			if (lhs is IList ||
				lhs is ISet)
			{
				ISet leftset = new HybridSet(lhs as ICollection);
				ISet rightset;
				if (rhs is IList || rhs is ISet)
				{
					rightset = new HybridSet(rhs as ICollection);
				}
				else if (rhs is IDictionary)
				{
					rightset = new HybridSet(((IDictionary) rhs).Keys);
				}
				else
				{
					throw new ArgumentException("Cannot subtract instances of '"
												+ lhs.GetType().FullName
												+ "' and '"
												+ rhs.GetType().FullName
												+ "'.");
				}
				return leftset.Minus(rightset);
			}
			if (lhs is IDictionary)
			{
				ISet leftset = new HybridSet(((IDictionary) lhs).Keys);
				ISet rightset;
				if (rhs is IList || rhs is ISet)
				{
					rightset = new HybridSet(rhs as ICollection);
				}
				else if (rhs is IDictionary)
				{
					rightset = new HybridSet(((IDictionary) rhs).Keys);
				}
				else
				{
					throw new ArgumentException("Cannot subtract instances of '"
												+ lhs.GetType().FullName
												+ "' and '"
												+ rhs.GetType().FullName
												+ "'.");
				}
				IDictionary result = new Hashtable(rightset.Count);
				foreach (var key in leftset.Minus(rightset))
				{
					result.Add(key, ((IDictionary) lhs)[key]);
				}
				return result;
			}
			throw new ArgumentException("Cannot subtract instances of '"
										+ lhs.GetType().FullName
										+ "' and '"
										+ rhs.GetType().FullName
										+ "'.");
		}
	}
}