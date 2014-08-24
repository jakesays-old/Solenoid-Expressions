
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
using NUnit.Framework;
using Solenoid.Expressions.Support.TypeConversion;
using Solenoid.Expressions.Support.Util;

namespace Solenoid.Expressions.Tests.Core.TypeConversion
{
	/// <summary>
	///     Unit tests for the TimeSpanConverter class.
	/// </summary>
	/// <author>Bruno Baia</author>
	[TestFixture]
	public sealed class TimeSpanConverterTests
	{
		[Test]
		public void ConvertFromNullReference()
		{
			try
			{
				var tsc = new TimeSpanConverter();
				tsc.ConvertFrom(null);
			}
			catch (NotSupportedException)
			{
				//.net throws NotSupportedException
				if (SystemUtils.MonoRuntime)
				{
					Assert.Fail("NotSupportedException not expected on mono");
				}
			}
			catch (NullReferenceException)
			{
				//mono throws NullReferenceException
				if (!SystemUtils.MonoRuntime)
				{
					Assert.Fail("NRE not expected on .net");
				}
			}
		}

		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void ConvertFromNonSupportedOptionBails()
		{
			var tsc = new TimeSpanConverter();
			tsc.ConvertFrom(12);
		}

		[Test]
		[ExpectedException(typeof (FormatException))]
		public void ConvertFromStringMalformed()
		{
			var tsc = new TimeSpanConverter();
			tsc.ConvertFrom("15a");
		}

		[Test]
		public void BaseConvertFrom()
		{
			var tsc = new TimeSpanConverter();
			var timeSpan = tsc.ConvertFrom("00:00:10");
			Assert.IsNotNull(timeSpan);
			Assert.IsTrue(timeSpan is TimeSpan);
			Assert.AreEqual(TimeSpan.FromSeconds(10), (TimeSpan) timeSpan);
		}

		[Test]
		public void ConvertFrom()
		{
			var tsc = new TimeSpanConverter();
			var timeSpan = tsc.ConvertFrom("1000");
			Assert.IsNotNull(timeSpan);
			Assert.IsTrue(timeSpan is TimeSpan);
			Assert.AreEqual(TimeSpan.Parse("1000"), (TimeSpan) timeSpan);
		}

		[Test]
		public void ConvertFromStringWithMilliSecondSpecifier()
		{
			var tsc = new TimeSpanConverter();
			var timeSpan = tsc.ConvertFrom("100ms");
			Assert.IsNotNull(timeSpan);
			Assert.IsTrue(timeSpan is TimeSpan);
			Assert.AreEqual(TimeSpan.FromMilliseconds(100), (TimeSpan) timeSpan);
		}

		[Test]
		public void ConvertFromStringWithSecondSpecifier()
		{
			var tsc = new TimeSpanConverter();
			var timeSpan = tsc.ConvertFrom("10s");
			Assert.IsNotNull(timeSpan);
			Assert.IsTrue(timeSpan is TimeSpan);
			Assert.AreEqual(TimeSpan.FromSeconds(10), (TimeSpan) timeSpan);
		}

		[Test]
		public void ConvertFromStringWithMinuteSpecifier()
		{
			var tsc = new TimeSpanConverter();
			var timeSpan = tsc.ConvertFrom("2m");
			Assert.IsNotNull(timeSpan);
			Assert.IsTrue(timeSpan is TimeSpan);
			Assert.AreEqual(TimeSpan.FromMinutes(2), (TimeSpan) timeSpan);
		}

		[Test]
		public void ConvertFromStringWithHourSpecifier()
		{
			var tsc = new TimeSpanConverter();

			var timeSpan = tsc.ConvertFrom("1H");
			Assert.IsNotNull(timeSpan);
			Assert.IsTrue(timeSpan is TimeSpan);
			Assert.AreEqual(TimeSpan.FromHours(1), (TimeSpan) timeSpan);

			tsc.ConvertFrom("1h");
			Assert.IsNotNull(timeSpan);
			Assert.IsTrue(timeSpan is TimeSpan);
			Assert.AreEqual(TimeSpan.FromHours(1), (TimeSpan) timeSpan);
		}

		[Test]
		public void ConvertFromStringWithDaySpecifier()
		{
			var tsc = new TimeSpanConverter();
			var timeSpan = tsc.ConvertFrom("1d");
			Assert.IsNotNull(timeSpan);
			Assert.IsTrue(timeSpan is TimeSpan);
			Assert.AreEqual(TimeSpan.FromDays(1), (TimeSpan) timeSpan);
		}
	}
}