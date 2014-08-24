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


using System.Collections.Generic;
using System.Reflection;
using Solenoid.Expressions.Support.Util;

namespace Solenoid.Expressions.Support.Reflection.Dynamic
{
	/// <summary>
	///     Safe wrapper for the dynamic property.
	/// </summary>
	/// <remarks>
	///     <see cref="SafeProperty" /> will attempt to use dynamic
	///     property if possible, but it will fall back to standard
	///     reflection if necessary.
	/// </remarks>
	public class SafeProperty : IDynamicProperty
	{
		private readonly PropertyInfo _propertyInfo;


		private static readonly IDictionary<PropertyInfo, DynamicPropertyCacheEntry> _propertyCache =
			new Dictionary<PropertyInfo, DynamicPropertyCacheEntry>();

		/// <summary>
		///     Holds cached Getter/Setter delegates for a Property
		/// </summary>
		private class DynamicPropertyCacheEntry
		{
			public readonly PropertyGetterDelegate Getter;
			public readonly PropertySetterDelegate Setter;

			public DynamicPropertyCacheEntry(PropertyGetterDelegate getter, PropertySetterDelegate setter)
			{
				Getter = getter;
				Setter = setter;
			}
		}

		/// <summary>
		///     Obtains cached property info or creates a new entry, if none is found.
		/// </summary>
		private static DynamicPropertyCacheEntry GetOrCreateDynamicProperty(PropertyInfo property)
		{
			DynamicPropertyCacheEntry propertyInfo;
			if (!_propertyCache.TryGetValue(property, out propertyInfo))
			{
				propertyInfo = new DynamicPropertyCacheEntry(DynamicReflectionManager.CreatePropertyGetter(property),
					DynamicReflectionManager.CreatePropertySetter(property));
				lock (_propertyCache)
				{
					_propertyCache[property] = propertyInfo;
				}
			}
			return propertyInfo;
		}


		private readonly PropertyGetterDelegate _getter;
		private readonly PropertySetterDelegate _setter;

		/// <summary>
		///     Creates a new instance of the safe property wrapper.
		/// </summary>
		/// <param name="propertyInfo">Property to wrap.</param>
		public SafeProperty(PropertyInfo propertyInfo)
		{
			AssertUtils.ArgumentNotNull(propertyInfo, "You cannot create a dynamic property for a null value.");

			_propertyInfo = propertyInfo;
			var pi = GetOrCreateDynamicProperty(propertyInfo);
			_getter = pi.Getter;
			_setter = pi.Setter;
		}

		/// <summary>
		///     Gets the value of the dynamic property for the specified target object.
		/// </summary>
		/// <param name="target">
		///     Target object to get property value from.
		/// </param>
		/// <returns>
		///     A property value.
		/// </returns>
		public object GetValue(object target)
		{
			return _getter(target);
		}

		/// <summary>
		///     Gets the value of the dynamic property for the specified target object.
		/// </summary>
		/// <param name="target">
		///     Target object to get property value from.
		/// </param>
		/// <param name="index">
		///     Optional index values for indexed properties. This value should be null reference for non-indexed
		///     properties.
		/// </param>
		/// <returns>
		///     A property value.
		/// </returns>
		public object GetValue(object target, params object[] index)
		{
			return _getter(target, index);
		}

		/// <summary>
		///     Gets the value of the dynamic property for the specified target object.
		/// </summary>
		/// <param name="target">
		///     Target object to set property value on.
		/// </param>
		/// <param name="value">
		///     A new property value.
		/// </param>
		public void SetValue(object target, object value)
		{
			_setter(target, value);
		}

		/// <summary>
		///     Gets the value of the dynamic property for the specified target object.
		/// </summary>
		/// <param name="target">
		///     Target object to set property value on.
		/// </param>
		/// <param name="value">
		///     A new property value.
		/// </param>
		/// <param name="index">
		///     Optional index values for indexed properties. This value should be null reference for non-indexed
		///     properties.
		/// </param>
		public void SetValue(object target, object value, params object[] index)
		{
			_setter(target, value, index);
		}

		/// <summary>
		///     Internal PropertyInfo accessor.
		/// </summary>
		internal PropertyInfo PropertyInfo
		{
			get { return _propertyInfo; }
		}
	}
}