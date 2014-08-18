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

namespace Solenoid.Expressions
{
    /// <summary>
    /// Represents ternary expression node.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class TernaryNode : BaseNode
    {
        private bool _initialized = false;
        private BaseNode _condition;
        private BaseNode _trueExp;
        private BaseNode _falseExp;

        /// <summary>
        /// Create a new instance
        /// </summary>
        public TernaryNode()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected TernaryNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        
        /// <summary>
        /// Returns a value for the string literal node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            if (!_initialized)
            {
                lock (this)
                {
                    if (!_initialized)
                    {
                        var node = getFirstChild();
                        _condition = (BaseNode) node;
                        node = node.getNextSibling();
                        _trueExp = (BaseNode) node;
                        node = node.getNextSibling();
                        _falseExp = (BaseNode) node;

                        _initialized = true;
                    }
                }
            }

            if (Convert.ToBoolean(GetValue(_condition, context, evalContext)))
            {
                return GetValue(_trueExp, context, evalContext);
            }
	        return GetValue(_falseExp, context, evalContext);
        }
    }
}