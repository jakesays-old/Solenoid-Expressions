#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

#endregion

using NUnit.Framework;
using Solenoid.Expressions;

namespace Spring.Expressions
{
    /// <summary>
    /// Unit tests for the OpXor class.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class OpXORTests
    {
        [Test]
        public void XorsNumbers()
        {
            OpXor bxor = new OpXor(new IntLiteralNode("2"), new IntLiteralNode("3"));
            Assert.AreEqual(2 ^ 3, bxor.GetValue(null, null));
        }

        [Test]
        public void XorsBooleans()
        {
            OpXor bxor1 = new OpXor(new BooleanLiteralNode("true"), new BooleanLiteralNode("false"));
            Assert.AreEqual(true ^ false, bxor1.GetValue(null, null));

            OpXor bxor2 = new OpXor(new BooleanLiteralNode("true"), new BooleanLiteralNode("true"));
            Assert.AreEqual(true ^ true, bxor2.GetValue(null, null));
        }
    }
}