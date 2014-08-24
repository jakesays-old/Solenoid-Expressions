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


using System.Linq;
using System.Text.RegularExpressions;
using Solenoid.Expressions.Support.Util;

namespace Solenoid.Expressions.Support.TypeResolution
{
	/// <summary>
	///     Holder for the generic arguments when using type parameters.
	/// </summary>
	/// <remarks>
	///     <p>
	///         Type parameters can be applied to classes, interfaces,
	///         structures, methods, delegates, etc...
	///     </p>
	/// </remarks>
	public class GenericArgumentsHolder
	{
		private static readonly Regex _clrPattern = new Regex(
			"^"
			+ @"(?'name'\w[\w\d\.]+)"
			+ @"`\d+\s*\["
			+ @"(?'args'(?>[^\[\]]+|\[(?<DEPTH>)|\](?<-DEPTH>))*(?(DEPTH)(?!)))"
			+ @"\]"
			+ @"(?'remainder'.*)"
			+ @"$"
			, RegexOptions.CultureInvariant | RegexOptions.Compiled
			);

		private static readonly Regex _cSharpPattern = new Regex(
			"^"
			+ @"(?'name'\w[\w\d\.]+)"
			+ @"<"
			+ @"(?'args'.*)"
			+ @">"
			+ @"(?'remainder'.*)"
			+ @"$"
			, RegexOptions.CultureInvariant | RegexOptions.Compiled
			);

		private static Regex _genericArgumentListPattern = new Regex(
			@",("
			+ @"(\[(?>[^\[\]]+|\[(?<DEPTH>)|\](?<-DEPTH>))*(?(DEPTH)(?!))\])" // capture anything between matching brackets
			+ @"|"
			+ @"([^,\[\]]*)" // alternatively capture any string that doesn't contain brackets and commas
			+ @")+"
			);

		/// <summary>
		///     The generic arguments prefix.
		/// </summary>
		public const char GenericArgumentsQuotePrefix = '[';

		/// <summary>
		///     The generic arguments suffix.
		/// </summary>
		public const char GenericArgumentsQuoteSuffix = ']';

		/// <summary>
		///     The generic arguments prefix.
		/// </summary>
		public const char GenericArgumentsPrefix = '<';

		/// <summary>
		///     The generic arguments suffix.
		/// </summary>
		public const char GenericArgumentsSuffix = '>';

		/// <summary>
		///     The character that separates a list of generic arguments.
		/// </summary>
		public const char GenericArgumentsSeparator = ',';


		private string _unresolvedGenericTypeName;
		private string _unresolvedGenericMethodName;
		private string[] _unresolvedGenericArguments;
		private string _arrayDeclaration;


		/// <summary>
		///     Creates a new instance of the GenericArgumentsHolder class.
		/// </summary>
		/// <param name="value">
		///     The string value to parse looking for a generic definition
		///     and retrieving its generic arguments.
		/// </param>
		public GenericArgumentsHolder(string value)
		{
			ParseGenericTypeDeclaration(value);
		}


		/// <summary>
		///     The (unresolved) generic type name portion
		///     of the original value when parsing a generic type.
		/// </summary>
		public string GenericTypeName
		{
			get { return _unresolvedGenericTypeName; }
		}

		/// <summary>
		///     The (unresolved) generic method name portion
		///     of the original value when parsing a generic method.
		/// </summary>
		public string GenericMethodName
		{
			get { return _unresolvedGenericMethodName; }
		}

		/// <summary>
		///     Is the string value contains generic arguments ?
		/// </summary>
		/// <remarks>
		///     <p>
		///         A generic argument can be a type parameter or a type argument.
		///     </p>
		/// </remarks>
		public bool ContainsGenericArguments
		{
			get
			{
				return (_unresolvedGenericArguments != null &&
						_unresolvedGenericArguments.Length > 0);
			}
		}

		/// <summary>
		///     Is generic arguments only contains type parameters ?
		/// </summary>
		public bool IsGenericDefinition
		{
			get
			{
				if (_unresolvedGenericArguments == null)
				{
					return false;
				}

				var result = _unresolvedGenericArguments.All(arg => arg.Length <= 0);
				return result;
			}
		}

		/// <summary>
		///     Returns the array declaration portion of the definition, e.g. "[,]"
		/// </summary>
		/// <returns></returns>
		public string GetArrayDeclaration()
		{
			return _arrayDeclaration;
		}

		/// <summary>
		///     Is this an array type definition?
		/// </summary>
		public bool IsArrayDeclaration
		{
			get { return _arrayDeclaration != null; }
		}


		/// <summary>
		///     Returns an array of unresolved generic arguments types.
		/// </summary>
		/// <remarks>
		///     <p>
		///         A empty string represents a type parameter that
		///         did not have been substituted by a specific type.
		///     </p>
		/// </remarks>
		/// <returns>
		///     An array of strings that represents the unresolved generic
		///     arguments types or an empty array if not generic.
		/// </returns>
		public string[] GetGenericArguments()
		{
			if (_unresolvedGenericArguments == null)
			{
				return StringUtils.EmptyStrings;
			}

			return _unresolvedGenericArguments;
		}

		private void ParseGenericTypeDeclaration(string originalString)
		{
			if (originalString.IndexOf('[') == -1 && originalString.IndexOf('<') == -1)
			{
				// nothing to do
				_unresolvedGenericTypeName = originalString;
				_unresolvedGenericMethodName = originalString;
				return;
			}

			originalString = originalString.Trim();

			var isClrStyleNotation = originalString.IndexOf('`') > -1;

			var m = (isClrStyleNotation)
				? _clrPattern.Match(originalString)
				: _cSharpPattern.Match(originalString);

			if (!m.Success)
			{
				_unresolvedGenericTypeName = originalString;
				_unresolvedGenericMethodName = originalString;
				return;
			}

			var g = m.Groups["args"];
			_unresolvedGenericArguments = ParseGenericArgumentList(g.Value);

			var name = m.Groups["name"].Value;
			var remainder = m.Groups["remainder"].Value.Trim();

			// check, if we're dealing with an array type declaration
			if (remainder.Length > 0 && remainder.IndexOf('[') > -1)
			{
				var remainderParts = StringUtils.Split(remainder, ",", false, false, "[]");
				var arrayPart = remainderParts[0].Trim();
				if (arrayPart[0] == '[' && arrayPart[arrayPart.Length - 1] == ']')
				{
					_arrayDeclaration = arrayPart;
					remainder = ", " + string.Join(",", remainderParts, 1, remainderParts.Length - 1);
				}
			}

			_unresolvedGenericMethodName = name + remainder;
			_unresolvedGenericTypeName = name + "`" + _unresolvedGenericArguments.Length + remainder;
		}

		private static string[] ParseGenericArgumentList(string originalArgs)
		{
			var args = StringUtils.Split(originalArgs, ",", true, false, "[]<>");
			// remove quotes if necessary
			for (var i = 0; i < args.Length; i++)
			{
				var arg = args[i];
				if (arg.Length > 1 && arg[0] == '[')
				{
					args[i] = arg.Substring(1, arg.Length - 2);
				}
			}
			return args;
		}
	}
}