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
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: AssemblyTitle("Solenoid.Expressions")]
[assembly: AssemblyDescription("Expression evaluation engine derived from Spring.Net SPEL")]
[assembly: AssemblyCompany("https://github.com/jakesays/Solenoid-Expressions")]
[assembly: AssemblyProduct("Solenoid.Expressions 1.0.0")]
[assembly: AssemblyCopyright("Portions Copyright Spring.NET Framework Team. (http://www.springframework.net)")]
[assembly: AssemblyTrademark("Apache License, Version 2.0")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyConfiguration("net-" + AssyInfo.FrameworkVersion + "; " + AssyInfo.BuildType)]

internal static class AssyInfo
{
#if NET_4_5
	internal const string FrameworkVersion = "4.5";
#elif NET_4_0
	internal const string FrameworkVersion = "4.0";
#endif
#if DEBUG
	internal const string BuildType = "Debug";
#else
	internal const string BuildType = "Release";
#endif
}




//#if !NET_4_0
//[assembly: AllowPartiallyTrustedCallers]

//[assembly: SecurityCritical]

//#endif
