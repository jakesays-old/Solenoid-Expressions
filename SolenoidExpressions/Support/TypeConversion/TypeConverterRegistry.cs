
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

// ReSharper disable UnusedMember.Global


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using Solenoid.Expressions.Support.TypeResolution;
using Solenoid.Expressions.Support.Util;


namespace Solenoid.Expressions.Support.TypeConversion
{
    /// <summary>
    /// Registry class that allows users to register and retrieve type converters.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public static class TypeConverterRegistry
    {
        private static readonly object _syncRoot = new object();
        private static readonly IDictionary<Type, TypeConverter> _converters = 
			new Dictionary<Type, TypeConverter>();
        
        /// <summary>
        /// Registers standard and configured type converters.
        /// </summary>
        static TypeConverterRegistry()
        {
            lock (_syncRoot)
            {
                _converters[typeof(string[])] = new StringArrayConverter();
                _converters[typeof(Type)] = new RuntimeTypeConverter();
                _converters[typeof(Uri)] = new UriConverter();
                _converters[typeof(FileInfo)] = new FileInfoConverter();
                _converters[typeof(NameValueCollection)] = new NameValueConverter();
                _converters[typeof(Regex)] = new RegexConverter();
                _converters[typeof(TimeSpan)] = new TimeSpanConverter();
            }
        }
        
        /// <summary>
        /// Returns <see cref="TypeConverter"/> for the specified type.
        /// </summary>
        /// <param name="type">Type to get the converter for.</param>
        /// <returns>a type converter for the specified type.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <c>null</c>.</exception>
        public static TypeConverter GetConverter(Type type)
        {
            AssertUtils.ArgumentNotNull(type, "type");

            TypeConverter converter;
            if (!_converters.TryGetValue(type, out converter))
            {
                if (type.IsEnum)
                {
                    converter = new EnumConverter(type);
                }
                else
                {
                    converter = TypeDescriptor.GetConverter(type);
                }
            }
            
            return converter;
        }
        
        /// <summary>
        /// Registers <see cref="TypeConverter"/> for the specified type.
        /// </summary>
        /// <param name="type">Type to register the converter for.</param>
        /// <param name="converter">Type converter to register.</param>
        /// <exception cref="ArgumentNullException">If either of arguments is <c>null</c>.</exception>
        public static void RegisterConverter(Type type, TypeConverter converter)
        {
            AssertUtils.ArgumentNotNull(type, "type");
            AssertUtils.ArgumentNotNull(converter, "converter");

            lock (_syncRoot)
            {
                _converters[type] = converter;
            }
        }

        /// <summary>
        /// Registers <see cref="TypeConverter"/> for the specified type.
        /// </summary>
        /// <remarks>
        /// This is a convinience method that accepts the names of both
        /// type to register converter for and the converter itself,
        /// resolves them using <see cref="TypeRegistry"/>, creates an
        /// instance of type converter and calls overloaded
        /// <see cref="RegisterConverter(Type,TypeConverter)"/> method.
        /// </remarks>
        /// <param name="typeName">Type name of the type to register the converter for (can be a type alias).</param>
        /// <param name="converterTypeName">Type name of the type converter to register (can be a type alias).</param>
        /// <exception cref="ArgumentNullException">If either of arguments is <c>null</c> or empty string.</exception>
        /// <exception cref="TypeLoadException">
        /// If either of arguments fails to resolve to a valid <see cref="Type"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If type converter does not derive from <see cref="TypeConverter"/> or if it cannot be instantiated.
        /// </exception>
        public static void RegisterConverter(string typeName, string converterTypeName)
        {
            AssertUtils.ArgumentHasText(typeName, "typeName");
            AssertUtils.ArgumentHasText(converterTypeName, "converterTypeName");
            
            try
            {
                var type = TypeResolutionUtils.ResolveType(typeName);
                var converterType = TypeResolutionUtils.ResolveType(converterTypeName);
                if (!typeof(TypeConverter).IsAssignableFrom(converterType))
                {
                    throw new ArgumentException(
                            "Type specified as a 'converterTypeName' does not inherit from System.ComponentModel.TypeConverter");
                }
                RegisterConverter(type,(TypeConverter) ObjectUtils.InstantiateType(converterType));
            }
            catch (FatalReflectionException e)
            {
                throw new ArgumentException("Failed to create an instance of the specified type converter.", e);
            }
        }
        
    }
}