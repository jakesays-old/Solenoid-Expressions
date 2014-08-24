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

using System.Collections;
using Solenoid.Expressions.Support.Util;

namespace Solenoid.Expressions.Extensions
{
    /// <summary>
    /// Implementation of the maximum aggregator.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class MaxAggregator : ICollectionExtension
    {
        /// <summary>
        /// Returns the largest item in the source collection.
        /// </summary>
        /// <param name="source">
        /// The source collection to process.
        /// </param>
        /// <param name="args">
        /// Ignored.
        /// </param>
        /// <returns>
        /// The largest item in the source collection.
        /// </returns>
        public object Execute(ICollection source, object[] args)
        {
            object maxItem = null;
            foreach (var item in source)
            {
                if (CompareUtils.Compare(maxItem, item) < 0)
                {
                    maxItem = item;
                }
            }
            return maxItem;
        }
    }
}
