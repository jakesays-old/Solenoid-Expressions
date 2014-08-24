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
	///     Safe wrapper for the dynamic field.
	/// </summary>
	/// <remarks>
	///     <see cref="SafeField" /> will attempt to use dynamic
	///     field if possible, but it will fall back to standard
	///     reflection if necessary.
	/// </remarks>
	public class SafeField : IDynamicField
	{
		private readonly FieldInfo _fieldInfo;


		private static readonly IDictionary<FieldInfo, DynamicFieldCacheEntry> _fieldCache =
			new Dictionary<FieldInfo, DynamicFieldCacheEntry>();

		/// <summary>
		///     Holds cached Getter/Setter delegates for a Field
		/// </summary>
		private class DynamicFieldCacheEntry
		{
			public readonly FieldGetterDelegate Getter;
			public readonly FieldSetterDelegate Setter;

			public DynamicFieldCacheEntry(FieldGetterDelegate getter, FieldSetterDelegate setter)
			{
				Getter = getter;
				Setter = setter;
			}
		}

		/// <summary>
		///     Obtains cached fieldInfo or creates a new entry, if none is found.
		/// </summary>
		private static DynamicFieldCacheEntry GetOrCreateDynamicField(FieldInfo field)
		{
			DynamicFieldCacheEntry fieldInfo;
			if (!_fieldCache.TryGetValue(field, out fieldInfo))
			{
				fieldInfo = new DynamicFieldCacheEntry(DynamicReflectionManager.CreateFieldGetter(field),
					DynamicReflectionManager.CreateFieldSetter(field));
				lock (_fieldCache)
				{
					_fieldCache[field] = fieldInfo;
				}
			}
			return fieldInfo;
		}


		private readonly FieldGetterDelegate _getter;
		private readonly FieldSetterDelegate _setter;

		/// <summary>
		///     Creates a new instance of the safe field wrapper.
		/// </summary>
		/// <param name="field">Field to wrap.</param>
		public SafeField(FieldInfo field)
		{
			AssertUtils.ArgumentNotNull(field, "You cannot create a dynamic field for a null value.");

			_fieldInfo = field;
			var fi = GetOrCreateDynamicField(field);
			_getter = fi.Getter;
			_setter = fi.Setter;
		}

		/// <summary>
		///     Gets the value of the dynamic field for the specified target object.
		/// </summary>
		/// <param name="target">
		///     Target object to get field value from.
		/// </param>
		/// <returns>
		///     A field value.
		/// </returns>
		public object GetValue(object target)
		{
			return _getter(target);
		}

		/// <summary>
		///     Gets the value of the dynamic field for the specified target object.
		/// </summary>
		/// <param name="target">
		///     Target object to set field value on.
		/// </param>
		/// <param name="value">
		///     A new field value.
		/// </param>
		public void SetValue(object target, object value)
		{
			_setter(target, value);
		}

		internal FieldInfo FieldInfo
		{
			get { return _fieldInfo; }
		}
	}
}