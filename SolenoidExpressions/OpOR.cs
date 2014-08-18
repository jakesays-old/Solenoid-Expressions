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
using System.Runtime.Serialization;
using Solenoid.Expressions.Support.Util;

namespace Solenoid.Expressions
{
    /// <summary>
    /// Represents OR operator (both, bitwise and logical).
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class OpOr : BinaryOperator
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public OpOr()
        {
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        public OpOr(BaseNode left, BaseNode right)
            :base(left, right)
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected OpOr(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        
        /// <summary>
        /// Returns a value for the logical OR operator node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            var l = GetLeftValue(context, evalContext);
            
            if (NumberUtils.IsInteger(l))
            {
                var r = GetRightValue(context, evalContext);
                if (NumberUtils.IsInteger(r))
                {
                    return NumberUtils.BitwiseOr(l, r);
                }
            }
            else if (l is Enum)
            {
                var r = GetRightValue(context, evalContext);
                if (l.GetType() == r.GetType())
                {
                    var enumType = l.GetType();
                    var integralType = Enum.GetUnderlyingType(enumType);
                    l = Convert.ChangeType(l, integralType);
                    r = Convert.ChangeType(r, integralType);
                    var result = NumberUtils.BitwiseOr(l, r);
                    return Enum.ToObject(enumType, result);
                }
            }

            return Convert.ToBoolean(l) || 
                Convert.ToBoolean(GetRightValue(context, evalContext));
        }
    }
}