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
using System.Collections.Generic;
using Solenoid.Expressions.Support.Util;

namespace Solenoid.Expressions.Support.TypeResolution
{
	/// <summary>
	///     Provides access to a central registry of aliased <see cref="System.Type" />s.
	/// </summary>
	/// <remarks>
	///     <p>
	///         Simplifies configuration by allowing aliases to be used instead of
	///         fully qualified type names.
	///     </p>
	///     <p>
	///         Comes 'pre-loaded' with a number of convenience alias' for the more
	///         common types; an example would be the '<c>int</c>' (or '<c>Integer</c>'
	///         for Visual Basic.NET developers) alias for the <see cref="System.Int32" />
	///         type.
	///     </p>
	/// </remarks>
	/// <author>Aleksandar Seovic</author>
	public sealed class TypeRegistry
	{
		/// <summary>
		///     The alias around the 'int' type.
		/// </summary>
		private const string Int32Alias = "int";

		/// <summary>
		///     The alias around the 'Integer' type (Visual Basic.NET style).
		/// </summary>
		private const string Int32AliasVb = "Integer";

		/// <summary>
		///     The alias around the 'int[]' array type.
		/// </summary>
		private const string Int32ArrayAlias = "int[]";

		/// <summary>
		///     The alias around the 'Integer()' array type (Visual Basic.NET style).
		/// </summary>
		private const string Int32ArrayAliasVb = "Integer()";

		/// <summary>
		///     The alias around the 'decimal' type.
		/// </summary>
		private const string DecimalAlias = "decimal";

		/// <summary>
		///     The alias around the 'Decimal' type (Visual Basic.NET style).
		/// </summary>
		private const string DecimalAliasVb = "Decimal";

		/// <summary>
		///     The alias around the 'decimal[]' array type.
		/// </summary>
		private const string DecimalArrayAlias = "decimal[]";

		/// <summary>
		///     The alias around the 'Decimal()' array type (Visual Basic.NET style).
		/// </summary>
		private const string DecimalArrayAliasVb = "Decimal()";

		/// <summary>
		///     The alias around the 'char' type.
		/// </summary>
		private const string CharAlias = "char";

		/// <summary>
		///     The alias around the 'Char' type (Visual Basic.NET style).
		/// </summary>
		private const string CharAliasVb = "Char";

		/// <summary>
		///     The alias around the 'char[]' array type.
		/// </summary>
		private const string CharArrayAlias = "char[]";

		/// <summary>
		///     The alias around the 'Char()' array type (Visual Basic.NET style).
		/// </summary>
		private const string CharArrayAliasVb = "Char()";

		/// <summary>
		///     The alias around the 'long' type.
		/// </summary>
		private const string Int64Alias = "long";

		/// <summary>
		///     The alias around the 'Long' type (Visual Basic.NET style).
		/// </summary>
		private const string Int64AliasVb = "Long";

		/// <summary>
		///     The alias around the 'long[]' array type.
		/// </summary>
		private const string Int64ArrayAlias = "long[]";

		/// <summary>
		///     The alias around the 'Long()' array type (Visual Basic.NET style).
		/// </summary>
		private const string Int64ArrayAliasVb = "Long()";

		/// <summary>
		///     The alias around the 'short' type.
		/// </summary>
		private const string Int16Alias = "short";

		/// <summary>
		///     The alias around the 'Short' type (Visual Basic.NET style).
		/// </summary>
		private const string Int16AliasVb = "Short";

		/// <summary>
		///     The alias around the 'short[]' array type.
		/// </summary>
		private const string Int16ArrayAlias = "short[]";

		/// <summary>
		///     The alias around the 'Short()' array type (Visual Basic.NET style).
		/// </summary>
		private const string Int16ArrayAliasVb = "Short()";

		/// <summary>
		///     The alias around the 'unsigned int' type.
		/// </summary>
		private const string UInt32Alias = "uint";

		/// <summary>
		///     The alias around the 'unsigned long' type.
		/// </summary>
		private const string UInt64Alias = "ulong";

		/// <summary>
		///     The alias around the 'ulong[]' array type.
		/// </summary>
		private const string UInt64ArrayAlias = "ulong[]";

		/// <summary>
		///     The alias around the 'uint[]' array type.
		/// </summary>
		private const string UInt32ArrayAlias = "uint[]";

		/// <summary>
		///     The alias around the 'unsigned short' type.
		/// </summary>
		private const string UInt16Alias = "ushort";

		/// <summary>
		///     The alias around the 'ushort[]' array type.
		/// </summary>
		private const string UInt16ArrayAlias = "ushort[]";

		/// <summary>
		///     The alias around the 'double' type.
		/// </summary>
		private const string DoubleAlias = "double";

		/// <summary>
		///     The alias around the 'Double' type (Visual Basic.NET style).
		/// </summary>
		private const string DoubleAliasVb = "Double";

		/// <summary>
		///     The alias around the 'double[]' array type.
		/// </summary>
		private const string DoubleArrayAlias = "double[]";

		/// <summary>
		///     The alias around the 'Double()' array type (Visual Basic.NET style).
		/// </summary>
		private const string DoubleArrayAliasVb = "Double()";

		/// <summary>
		///     The alias around the 'float' type.
		/// </summary>
		private const string FloatAlias = "float";

		/// <summary>
		///     The alias around the 'Single' type (Visual Basic.NET style).
		/// </summary>
		private const string SingleAlias = "Single";

		/// <summary>
		///     The alias around the 'float[]' array type.
		/// </summary>
		private const string FloatArrayAlias = "float[]";

		/// <summary>
		///     The alias around the 'Single()' array type (Visual Basic.NET style).
		/// </summary>
		private const string SingleArrayAliasVb = "Single()";

		/// <summary>
		///     The alias around the 'DateTime' type.
		/// </summary>
		private const string DateTimeAlias = "DateTime";

		/// <summary>
		///     The alias around the 'DateTime' type (C# style).
		/// </summary>
		private const string DateAlias = "date";

		/// <summary>
		///     The alias around the 'DateTime' type (Visual Basic.NET style).
		/// </summary>
		private const string DateAliasVb = "Date";

		/// <summary>
		///     The alias around the 'DateTime[]' array type.
		/// </summary>
		private const string DateTimeArrayAlias = "DateTime[]";

		/// <summary>
		///     The alias around the 'DateTime[]' array type.
		/// </summary>
		private const string DateTimeArrayAliasCSharp = "date[]";

		/// <summary>
		///     The alias around the 'DateTime()' array type (Visual Basic.NET style).
		/// </summary>
		private const string DateTimeArrayAliasVb = "DateTime()";

		/// <summary>
		///     The alias around the 'bool' type.
		/// </summary>
		private const string BoolAlias = "bool";

		/// <summary>
		///     The alias around the 'Boolean' type (Visual Basic.NET style).
		/// </summary>
		private const string BoolAliasVb = "Boolean";

		/// <summary>
		///     The alias around the 'bool[]' array type.
		/// </summary>
		private const string BoolArrayAlias = "bool[]";

		/// <summary>
		///     The alias around the 'Boolean()' array type (Visual Basic.NET style).
		/// </summary>
		private const string BoolArrayAliasVb = "Boolean()";

		/// <summary>
		///     The alias around the 'string' type.
		/// </summary>
		private const string StringAlias = "string";

		/// <summary>
		///     The alias around the 'string' type (Visual Basic.NET style).
		/// </summary>
		private const string StringAliasVb = "String";

		/// <summary>
		///     The alias around the 'string[]' array type.
		/// </summary>
		private const string StringArrayAlias = "string[]";

		/// <summary>
		///     The alias around the 'string[]' array type (Visual Basic.NET style).
		/// </summary>
		private const string StringArrayAliasVb = "String()";

		/// <summary>
		///     The alias around the 'object' type.
		/// </summary>
		private const string ObjectAlias = "object";

		/// <summary>
		///     The alias around the 'object' type (Visual Basic.NET style).
		/// </summary>
		private const string ObjectAliasVb = "Object";

		/// <summary>
		///     The alias around the 'object[]' array type.
		/// </summary>
		private const string ObjectArrayAlias = "object[]";

		/// <summary>
		///     The alias around the 'object[]' array type (Visual Basic.NET style).
		/// </summary>
		private const string ObjectArrayAliasVb = "Object()";

		/// <summary>
		///     The alias around the 'int?' type.
		/// </summary>
		private const string NullableInt32Alias = "int?";

		/// <summary>
		///     The alias around the 'int?[]' array type.
		/// </summary>
		private const string NullableInt32ArrayAlias = "int?[]";

		/// <summary>
		///     The alias around the 'decimal?' type.
		/// </summary>
		private const string NullableDecimalAlias = "decimal?";

		/// <summary>
		///     The alias around the 'decimal?[]' array type.
		/// </summary>
		private const string NullableDecimalArrayAlias = "decimal?[]";

		/// <summary>
		///     The alias around the 'char?' type.
		/// </summary>
		private const string NullableCharAlias = "char?";

		/// <summary>
		///     The alias around the 'char?[]' array type.
		/// </summary>
		private const string NullableCharArrayAlias = "char?[]";

		/// <summary>
		///     The alias around the 'long?' type.
		/// </summary>
		private const string NullableInt64Alias = "long?";

		/// <summary>
		///     The alias around the 'long?[]' array type.
		/// </summary>
		private const string NullableInt64ArrayAlias = "long?[]";

		/// <summary>
		///     The alias around the 'short?' type.
		/// </summary>
		private const string NullableInt16Alias = "short?";

		/// <summary>
		///     The alias around the 'short?[]' array type.
		/// </summary>
		private const string NullableInt16ArrayAlias = "short?[]";

		/// <summary>
		///     The alias around the 'unsigned int?' type.
		/// </summary>
		private const string NullableUInt32Alias = "uint?";

		/// <summary>
		///     The alias around the 'unsigned long?' type.
		/// </summary>
		private const string NullableUInt64Alias = "ulong?";

		/// <summary>
		///     The alias around the 'ulong?[]' array type.
		/// </summary>
		private const string NullableUInt64ArrayAlias = "ulong?[]";

		/// <summary>
		///     The alias around the 'uint?[]' array type.
		/// </summary>
		private const string NullableUInt32ArrayAlias = "uint?[]";

		/// <summary>
		///     The alias around the 'unsigned short?' type.
		/// </summary>
		private const string NullableUInt16Alias = "ushort?";

		/// <summary>
		///     The alias around the 'ushort?[]' array type.
		/// </summary>
		private const string NullableUInt16ArrayAlias = "ushort?[]";

		/// <summary>
		///     The alias around the 'double?' type.
		/// </summary>
		private const string NullableDoubleAlias = "double?";

		/// <summary>
		///     The alias around the 'double?[]' array type.
		/// </summary>
		private const string NullableDoubleArrayAlias = "double?[]";

		/// <summary>
		///     The alias around the 'float?' type.
		/// </summary>
		private const string NullableFloatAlias = "float?";

		/// <summary>
		///     The alias around the 'float?[]' array type.
		/// </summary>
		private const string NullableFloatArrayAlias = "float?[]";

		/// <summary>
		///     The alias around the 'bool?' type.
		/// </summary>
		private const string NullableBoolAlias = "bool?";

		/// <summary>
		///     The alias around the 'bool?[]' array type.
		/// </summary>
		private const string NullableBoolArrayAlias = "bool?[]";


		private static readonly object _syncRoot = new object();
		private static readonly IDictionary<string, Type> _types = new Dictionary<string, Type>();


		/// <summary>
		///     Registers standard and user-configured type aliases.
		/// </summary>
		static TypeRegistry()
		{
			lock (_syncRoot)
			{
				_types["Int32"] = typeof (Int32);
				_types[Int32Alias] = typeof (Int32);
				_types[Int32AliasVb] = typeof (Int32);
				_types[Int32ArrayAlias] = typeof (Int32[]);
				_types[Int32ArrayAliasVb] = typeof (Int32[]);

				_types["UInt32"] = typeof (UInt32);
				_types[UInt32Alias] = typeof (UInt32);
				_types[UInt32ArrayAlias] = typeof (UInt32[]);

				_types["Int16"] = typeof (Int16);
				_types[Int16Alias] = typeof (Int16);
				_types[Int16AliasVb] = typeof (Int16);
				_types[Int16ArrayAlias] = typeof (Int16[]);
				_types[Int16ArrayAliasVb] = typeof (Int16[]);

				_types["UInt16"] = typeof (UInt16);
				_types[UInt16Alias] = typeof (UInt16);
				_types[UInt16ArrayAlias] = typeof (UInt16[]);

				_types["Int64"] = typeof (Int64);
				_types[Int64Alias] = typeof (Int64);
				_types[Int64AliasVb] = typeof (Int64);
				_types[Int64ArrayAlias] = typeof (Int64[]);
				_types[Int64ArrayAliasVb] = typeof (Int64[]);

				_types["UInt64"] = typeof (UInt64);
				_types[UInt64Alias] = typeof (UInt64);
				_types[UInt64ArrayAlias] = typeof (UInt64[]);

				_types[DoubleAlias] = typeof (double);
				_types[DoubleAliasVb] = typeof (double);
				_types[DoubleArrayAlias] = typeof (double[]);
				_types[DoubleArrayAliasVb] = typeof (double[]);

				_types[FloatAlias] = typeof (float);
				_types[SingleAlias] = typeof (float);
				_types[FloatArrayAlias] = typeof (float[]);
				_types[SingleArrayAliasVb] = typeof (float[]);

				_types[DateTimeAlias] = typeof (DateTime);
				_types[DateAlias] = typeof (DateTime);
				_types[DateAliasVb] = typeof (DateTime);
				_types[DateTimeArrayAlias] = typeof (DateTime[]);
				_types[DateTimeArrayAliasCSharp] = typeof (DateTime[]);
				_types[DateTimeArrayAliasVb] = typeof (DateTime[]);

				_types[BoolAlias] = typeof (bool);
				_types[BoolAliasVb] = typeof (bool);
				_types[BoolArrayAlias] = typeof (bool[]);
				_types[BoolArrayAliasVb] = typeof (bool[]);

				_types[DecimalAlias] = typeof (decimal);
				_types[DecimalAliasVb] = typeof (decimal);
				_types[DecimalArrayAlias] = typeof (decimal[]);
				_types[DecimalArrayAliasVb] = typeof (decimal[]);

				_types[CharAlias] = typeof (char);
				_types[CharAliasVb] = typeof (char);
				_types[CharArrayAlias] = typeof (char[]);
				_types[CharArrayAliasVb] = typeof (char[]);

				_types[StringAlias] = typeof (string);
				_types[StringAliasVb] = typeof (string);
				_types[StringArrayAlias] = typeof (string[]);
				_types[StringArrayAliasVb] = typeof (string[]);

				_types[ObjectAlias] = typeof (object);
				_types[ObjectAliasVb] = typeof (object);
				_types[ObjectArrayAlias] = typeof (object[]);
				_types[ObjectArrayAliasVb] = typeof (object[]);

				_types[NullableInt32Alias] = typeof (int?);
				_types[NullableInt32ArrayAlias] = typeof (int?[]);

				_types[NullableDecimalAlias] = typeof (decimal?);
				_types[NullableDecimalArrayAlias] = typeof (decimal?[]);

				_types[NullableCharAlias] = typeof (char?);
				_types[NullableCharArrayAlias] = typeof (char?[]);

				_types[NullableInt64Alias] = typeof (long?);
				_types[NullableInt64ArrayAlias] = typeof (long?[]);

				_types[NullableInt16Alias] = typeof (short?);
				_types[NullableInt16ArrayAlias] = typeof (short?[]);

				_types[NullableUInt32Alias] = typeof (uint?);
				_types[NullableUInt32ArrayAlias] = typeof (uint?[]);

				_types[NullableUInt64Alias] = typeof (ulong?);
				_types[NullableUInt64ArrayAlias] = typeof (ulong?[]);

				_types[NullableUInt16Alias] = typeof (ushort?);
				_types[NullableUInt16ArrayAlias] = typeof (ushort?[]);

				_types[NullableDoubleAlias] = typeof (double?);
				_types[NullableDoubleArrayAlias] = typeof (double?[]);

				_types[NullableFloatAlias] = typeof (float?);
				_types[NullableFloatArrayAlias] = typeof (float?[]);

				_types[NullableBoolAlias] = typeof (bool?);
				_types[NullableBoolArrayAlias] = typeof (bool?[]);
			}
		}


		/// <summary>
		///     Registers an alias for the specified <see cref="System.Type" />.
		/// </summary>
		/// <remarks>
		///     <p>
		///         This overload does eager resolution of the <see cref="System.Type" />
		///         referred to by the <paramref name="typeName" /> parameter. It will throw a
		///         <see cref="System.TypeLoadException" /> if the <see cref="System.Type" /> referred
		///         to by the <paramref name="typeName" /> parameter cannot be resolved.
		///     </p>
		/// </remarks>
		/// <param name="alias">
		///     A string that will be used as an alias for the specified
		///     <see cref="System.Type" />.
		/// </param>
		/// <param name="typeName">
		///     The (possibly partially assembly qualified) name of the
		///     <see cref="System.Type" /> to register the alias for.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		///     If either of the supplied parameters is <see langword="null" /> or
		///     contains only whitespace character(s).
		/// </exception>
		/// <exception cref="System.TypeLoadException">
		///     If the <see cref="System.Type" /> referred to by the supplied
		///     <paramref name="typeName" /> cannot be loaded.
		/// </exception>
		public static void RegisterType(string alias, string typeName)
		{
			AssertUtils.ArgumentHasText(alias, "alias");
			AssertUtils.ArgumentHasText(typeName, "typeName");

			var type = TypeResolutionUtils.ResolveType(typeName);

			if (type.IsGenericTypeDefinition)
				alias += ("`" + type.GetGenericArguments().Length);

			RegisterType(alias, type);
		}

		/// <summary>
		///     Registers short type name as an alias for
		///     the supplied <see cref="System.Type" />.
		/// </summary>
		/// <param name="type">
		///     The <see cref="System.Type" /> to register.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		///     If the supplied <paramref name="type" /> is <see langword="null" />.
		/// </exception>
		public static void RegisterType(Type type)
		{
			AssertUtils.ArgumentNotNull(type, "type");

			lock (_syncRoot)
			{
				_types[type.Name] = type;
			}
		}

		/// <summary>
		///     Registers an alias for the supplied <see cref="System.Type" />.
		/// </summary>
		/// <param name="alias">
		///     The alias for the supplied <see cref="System.Type" />.
		/// </param>
		/// <param name="type">
		///     The <see cref="System.Type" /> to register the supplied <paramref name="alias" /> under.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		///     If the supplied <paramref name="type" /> is <see langword="null" />; or if
		///     the supplied <paramref name="alias" /> is <see langword="null" /> or
		///     contains only whitespace character(s).
		/// </exception>
		public static void RegisterType(string alias, Type type)
		{
			AssertUtils.ArgumentHasText(alias, "alias");
			AssertUtils.ArgumentNotNull(type, "type");

			lock (_syncRoot)
			{
				_types[alias] = type;
			}
		}

		/// <summary>
		///     Resolves the supplied <paramref name="alias" /> to a <see cref="System.Type" />.
		/// </summary>
		/// <param name="alias">
		///     The alias to resolve.
		/// </param>
		/// <returns>
		///     The <see cref="System.Type" /> the supplied <paramref name="alias" /> was
		///     associated with, or <see lang="null" /> if no <see cref="System.Type" />
		///     was previously registered for the supplied <paramref name="alias" />.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		///     If the supplied <paramref name="alias" /> is <see langword="null" /> or
		///     contains only whitespace character(s).
		/// </exception>
		public static Type ResolveType(string alias)
		{
			AssertUtils.ArgumentHasText(alias, "alias");
			Type type;
			_types.TryGetValue(alias, out type);
			return type;
		}

		/// <summary>
		///     Returns a flag specifying whether <b>TypeRegistry</b> contains
		///     specified alias or not.
		/// </summary>
		/// <param name="alias">
		///     Alias to check.
		/// </param>
		/// <returns>
		///     <c>true</c> if the specified type alias is registered,
		///     <c>false</c> otherwise.
		/// </returns>
		public static bool ContainsAlias(string alias)
		{
			return _types.ContainsKey(alias);
		}
	}
}