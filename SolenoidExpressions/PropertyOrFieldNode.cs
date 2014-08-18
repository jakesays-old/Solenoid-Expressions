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
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using Solenoid.Expressions.Support;
using Solenoid.Expressions.Support.Collections;
using Solenoid.Expressions.Support.Reflection.Dynamic;
using Solenoid.Expressions.Support.TypeConversion;
using Solenoid.Expressions.Support.TypeResolution;
using Solenoid.Expressions.Support.Util;

namespace Solenoid.Expressions
{
    /// <summary>
    /// Represents node that navigates to object's property or public field.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class PropertyOrFieldNode : BaseNode
    {
        private const BindingFlags DefaultBindingFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.IgnoreCase;

        private string _memberName;
        private ValueAccessor _accessor;

        /// <summary>
        /// Create a new instance
        /// </summary>
        public PropertyOrFieldNode()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected PropertyOrFieldNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes the node.
        /// </summary>
        /// <param name="context">The parent.</param>
        private void InitializeNode(object context)
        {
            var contextType = (context == null || context is Type ? context as Type : context.GetType());

            if (_accessor == null || _accessor.RequiresRefresh(contextType))
            {
                _memberName = getText();

                // clear cached member info if context type has changed (for example, when ASP.NET page is recompiled)
                if (_accessor != null && _accessor.RequiresRefresh(contextType))
                {
                    _accessor = null;
                }

                // initialize this node if necessary
                if (contextType != null && _accessor == null)
                {
                    // try to initialize node as ExpandoObject value
                    if (contextType == typeof(ExpandoObject))
                    {
                        _accessor = new ExpandoObjectValueAccessor(_memberName);
                    }
                    else if (contextType.IsEnum)
                    {
						// try to initialize node as enum value first
						try
                        {
                            _accessor = new EnumValueAccessor(Enum.Parse(contextType, _memberName, true));
                        }
                        catch (ArgumentException)
                        {
                            // ArgumentException will be thrown if specified member name is not a valid
                            // enum value for the context type. We should just ignore it and continue processing,
                            // because the specified member could be a property of a Type class (i.e. EnumType.FullName)
                        }
                    }

                    // then try to initialize node as property or field value
                    if (_accessor == null)
                    {
                        // check the context type first
                        _accessor = GetPropertyOrFieldAccessor(contextType, _memberName, DefaultBindingFlags);

                        // if not found, probe the Type type
                        if (_accessor == null && context is Type)
                        {
                            _accessor = GetPropertyOrFieldAccessor(typeof(Type), _memberName, DefaultBindingFlags);
                        }
                    }
                }

                // if there is still no match, try to initialize node as type accessor
                if (_accessor == null)
                {
                    try
                    {
                        _accessor = new TypeValueAccessor(TypeResolutionUtils.ResolveType(_memberName));
                    }
                    catch (TypeLoadException)
                    {
	                    if (context == null)
                        {
                            throw new NullValueInNestedPathException("Cannot initialize property or field node '" +
                                                                     _memberName +
                                                                     "' because the specified context is null.");
                        }
	                    throw new InvalidPropertyException(contextType, _memberName,
		                    "'" + _memberName +
							"' node cannot be resolved for the specified context [" +
							context + "].");
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to resolve property or field.
        /// </summary>
        /// <param name="contextType">
        /// Type to search for a property or a field.
        /// </param>
        /// <param name="memberName">
        /// Property or field name.
        /// </param>
        /// <param name="bindingFlags">
        /// Binding flags to use.
        /// </param>
        /// <returns>
        /// Resolved property or field accessor, or <c>null</c> 
        /// if specified <paramref name="memberName"/> cannot be resolved.
        /// </returns>
        private static ValueAccessor GetPropertyOrFieldAccessor(Type contextType, string memberName, BindingFlags bindingFlags)
        {
            try
            {
                var pi = contextType.GetProperty(memberName, bindingFlags);
                if (pi == null)
                {
                    var fi = contextType.GetField(memberName, bindingFlags);
                    if (fi != null)
                    {
                        return new FieldValueAccessor(fi);
                    }
                }
                else
                {
                    return new PropertyValueAccessor(pi);
                }
            }
            catch (AmbiguousMatchException)
            {
                PropertyInfo pi = null;

                // search type hierarchy
                while (contextType != typeof(object) &&
					contextType != null)
                {
                    pi = contextType.GetProperty(memberName, bindingFlags | BindingFlags.DeclaredOnly);
                    if (pi == null)
                    {
                        var fi = contextType.GetField(memberName, bindingFlags | BindingFlags.DeclaredOnly);
                        if (fi != null)
                        {
                            return new FieldValueAccessor(fi);
                        }
                    }
                    else
                    {
                        return new PropertyValueAccessor(pi);
                    }
                    contextType = contextType.BaseType;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns node's value for the given context.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            lock (this)
            {
                InitializeNode(context);

                if (context == null && _accessor.RequiresContext)
                {
                    throw new NullValueInNestedPathException(
                        "Cannot retrieve the value of a field or property '" + _memberName
                        + "', because context for its resolution is null.");
                }
                if (IsProperty || IsField)
                {
                    return GetPropertyOrFieldValue(context, evalContext);
                }
	            return _accessor.Get(context);
            }
        }

        /// <summary>
        /// Sets node's value for the given context.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <param name="newValue">New value for this node.</param>
        protected override void Set(object context, EvaluationContext evalContext, object newValue)
        {
            lock (this)
            {
                InitializeNode(context);

                if (context == null && _accessor.RequiresContext)
                {
                    throw new NullValueInNestedPathException(
                        "Cannot set the value of a field or property '" + _memberName
                        + "', because context for its resolution is null.");
                }
                if (IsProperty || IsField)
                {
                    SetPropertyOrFieldValue(context, evalContext, newValue);
                }
                else
                {
                    _accessor.Set(context, newValue);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this node represents a property.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this node is a property; otherwise, <c>false</c>.
        /// </value>
        private bool IsProperty
        {
            get { return _accessor is PropertyValueAccessor; }
        }

        /// <summary>
        /// Gets a value indicating whether this node represents a field.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this node is a field; otherwise, <c>false</c>.
        /// </value>
        private bool IsField
        {
            get { return _accessor is FieldValueAccessor; }
        }

        /// <summary>
        /// Retrieves property or field value.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Property or field value.</returns>
        private object GetPropertyOrFieldValue(object context, EvaluationContext evalContext)
        {
            try
            {
                return _accessor.Get(context);
            }
            catch (InvalidOperationException)
            {
                throw new NotReadablePropertyException(evalContext.RootContextType, _memberName);
            }
            catch (TargetInvocationException e)
            {
                throw new InvalidPropertyException(evalContext.RootContextType, _memberName,
                                                   "Getter for property '" + _memberName + "' threw an exception.",
                                                   e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new InvalidPropertyException(evalContext.RootContextType, _memberName,
                                                   "Illegal attempt to get value for the property '" + _memberName +
                                                   "'.", e);
            }
        }

        /// <summary>
        /// Sets property value, doing any type conversions that are necessary along the way.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <param name="newValue">New value for this node.</param>
        private void SetPropertyOrFieldValue(object context, EvaluationContext evalContext, object newValue)
        {
            var isWriteable = _accessor.IsWriteable;
            var targetType = _accessor.TargetType;

            try
            {
                if (!isWriteable)
                {
                    if (!AddToCollections(context, evalContext, newValue))
                    {
                        throw new NotWritablePropertyException(
                            "Can't change the value of the read-only property or field '" + _memberName + "'.");
                    }
                }
                else if (targetType.IsPrimitive && (newValue == null || String.Empty.Equals(newValue)))
                {
                    throw new ArgumentException("Invalid value [" + newValue + "] for property or field '" +
                                                _memberName + "' of primitive type ["
                                                + targetType + "]");
                }
                else if (newValue == null || ObjectUtils.IsAssignable(targetType, newValue)) // targetType.IsAssignableFrom(newValue.GetType())
                {
                    SetPropertyOrFieldValueInternal(context, newValue);
                }
                else if (!RemotingServices.IsTransparentProxy(newValue) &&
                         (newValue is IList || newValue is IDictionary))
                {
                    if (!AddToCollections(context, evalContext, newValue))
                    {
                        var tmpValue =
                            TypeConversionUtils.ConvertValueIfNecessary(targetType, newValue, _memberName);
                        SetPropertyOrFieldValueInternal(context, tmpValue);
                    }
                }
                else
                {
                    var tmpValue = TypeConversionUtils.ConvertValueIfNecessary(targetType, newValue, _memberName);
                    SetPropertyOrFieldValueInternal(context, tmpValue);
                }
            }
            catch (TargetInvocationException ex)
            {
                var propertyChangeEvent =
                    new PropertyChangeEventArgs(_memberName, null, newValue);
                if (ex.GetBaseException() is InvalidCastException)
                {
                    throw new TypeMismatchException(propertyChangeEvent, targetType, ex.GetBaseException());
                }
	            throw new MethodInvocationException(ex.GetBaseException(), propertyChangeEvent);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new FatalReflectionException("Illegal attempt to set property '" + _memberName + "'", ex);
            }
            catch (NotWritablePropertyException)
            {
                throw;
            }
            catch (NotReadablePropertyException)
            {
                throw;
            }
            catch (ArgumentException ex)
            {
                var propertyChangeEvent =
                    new PropertyChangeEventArgs(_memberName, null, newValue);
                throw new TypeMismatchException(propertyChangeEvent, targetType, ex);
            }
        }

        /// <summary>
        /// Sets property or field value using either dynamic or standard reflection.
        /// </summary>
        /// <param name="context">Object to evaluate node against.</param>
        /// <param name="newValue">New value for this node, converted to appropriate type.</param>
        private void SetPropertyOrFieldValueInternal(object context, object newValue)
        {
            _accessor.Set(context, newValue);
        }

        /// <summary>
        /// In the case of read only collections or custom collections that are not assignable from
        /// IList, try to add to the collection.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <param name="newValue">New value for this node.</param>
        /// <returns>true if was able add to IList, IDictionary, or ISet</returns>
        private bool AddToCollections(object context, EvaluationContext evalContext, object newValue)
        {
            // short-circuit if accessor is not readable or if we have an array
            if (!_accessor.IsReadable || _accessor.TargetType.IsArray)
            {
                return false;
            }

            var added = false;

            // try adding values if property is a list...
            if (newValue is IList && !RemotingServices.IsTransparentProxy(newValue))
            {
                var currentValue = (IList)Get(context, evalContext);
                if (currentValue != null && !currentValue.IsFixedSize && !currentValue.IsReadOnly)
                {
                    foreach (var el in (IList)newValue)
                    {
                        currentValue.Add(el);
                    }
                    added = true;
                }
            }
            else if (newValue is IDictionary && !RemotingServices.IsTransparentProxy(newValue))
            {
				// try adding values if property is a dictionary...
				var currentValue = (IDictionary) Get(context, evalContext);
                if (currentValue != null && !currentValue.IsFixedSize && !currentValue.IsReadOnly)
                {
                    foreach (DictionaryEntry entry in (IDictionary)newValue)
                    {
                        currentValue[entry.Key] = entry.Value;
                    }
                    added = true;
                }
            }
            else if (newValue is ISet && !RemotingServices.IsTransparentProxy(newValue))
            {
				// try adding values if property is a set...
				var currentValue = (ISet) Get(context, evalContext);
                if (currentValue != null)
                {
                    currentValue.AddAll((ICollection)newValue);
                    added = true;
                }
            }
            return added;
        }

        /// <summary>
        /// Utility method that is needed by ObjectWrapper and AbstractAutowireCapableObjectFactory.
        /// We try as hard as we can, but there are instances when we won't be able to obtain PropertyInfo...
        /// </summary>
        /// <param name="context">Context to resolve property against.</param>
        /// <returns>PropertyInfo for this node.</returns>
        internal MemberInfo GetMemberInfo(object context)
        {
            lock (this)
            {
                InitializeNode(context);
            }
            return _accessor.MemberInfo;
        }

	    private abstract class ValueAccessor
        {
            public abstract object Get(object context);

            public abstract void Set(object context, object value);

            public virtual bool IsReadable
            {
                get { return true; }
            }

            public virtual bool IsWriteable
            {
                get { return false; }
            }

            public virtual bool RequiresContext
            {
                get { return false; }
            }

            public virtual Type TargetType
            {
                get { throw new NotSupportedException(); }
            }

            public virtual MemberInfo MemberInfo
            {
                get { throw new NotSupportedException(); }
            }

            public virtual bool RequiresRefresh(Type contextType)
            {
                return false;
            }
        }

	    private class PropertyValueAccessor : ValueAccessor
        {
            private readonly SafeProperty _property;
            private readonly string _name;
            private readonly bool _isReadable;
            private readonly bool _isWriteable;
            private readonly Type _targetType;
            private readonly Type _contextType;

            public PropertyValueAccessor(PropertyInfo propertyInfo)
            {
                _name = propertyInfo.Name;
                _isReadable = propertyInfo.CanRead;
                _isWriteable = propertyInfo.CanWrite;
                _targetType = propertyInfo.PropertyType;
                _contextType = propertyInfo.DeclaringType;
                _property = new SafeProperty(propertyInfo);
            }

            public override object Get(object context)
            {
                if (!_isReadable)
                {
                    throw new NotReadablePropertyException("Cannot get a non-readable property [" + _name + "]");
                }
                return _property.GetValue(context);
            }

            public override void Set(object context, object value)
            {
                if (!_isWriteable)
                {
                    throw new NotWritablePropertyException("Cannot set a read-only property [" + _name + "]");
                }
                _property.SetValue(context, value);
            }

            public override bool IsReadable
            {
                get { return _isReadable; }
            }

            public override bool IsWriteable
            {
                get { return _isWriteable; }
            }

            public override bool RequiresContext
            {
                get { return true; }
            }

            public override Type TargetType
            {
                get { return _targetType; }
            }

            public override MemberInfo MemberInfo
            {
                get { return _property.PropertyInfo; }
            }

            public override bool RequiresRefresh(Type contextType)
            {
                return _contextType != contextType;
            }
        }

	    private class FieldValueAccessor : ValueAccessor
        {
            private readonly SafeField _field;
            private readonly bool _isWriteable;
            private readonly Type _targetType;
            private readonly Type _contextType;

            public FieldValueAccessor(FieldInfo fieldInfo)
            {
                _field = new SafeField(fieldInfo);
                _isWriteable = !(fieldInfo.IsInitOnly || fieldInfo.IsLiteral);
                _targetType = fieldInfo.FieldType;
                _contextType = fieldInfo.DeclaringType;
            }

            public override object Get(object context)
            {
                return _field.GetValue(context);
            }

            public override void Set(object context, object value)
            {
                _field.SetValue(context, value);
            }

            public override bool IsWriteable
            {
                get { return _isWriteable; }
            }

            public override bool RequiresContext
            {
                get { return true; }
            }

            public override Type TargetType
            {
                get { return _targetType; }
            }

            public override MemberInfo MemberInfo
            {
                get { return _field.FieldInfo; }
            }

            public override bool RequiresRefresh(Type contextType)
            {
                return _contextType != contextType;
            }
        }

	    private class EnumValueAccessor : ValueAccessor
        {
            private readonly object _enumValue;

            public EnumValueAccessor(object enumValue)
            {
                _enumValue = enumValue;
            }

            public override object Get(object context)
            {
                return _enumValue;
            }

            public override void Set(object context, object value)
            {
                throw new NotSupportedException("Cannot set the value of an enum.");
            }
        }

	    private class ExpandoObjectValueAccessor : ValueAccessor
        {
            private readonly string _memberName;

            public ExpandoObjectValueAccessor(string memberName)
            {
                _memberName = memberName;
            }

            public override object Get(object context)
            {
                var dictionary = context as IDictionary<string, object>;

                object value;
	            if (dictionary != null &&
					dictionary.TryGetValue(_memberName, out value))
	            {
		            return value;
	            }
                throw new InvalidPropertyException(typeof(ExpandoObject), _memberName,
                                                  "'" + _memberName +
                                                  "' node cannot be resolved for the specified context [" +
                                                  context + "].");
            }

            public override void Set(object context, object value)
            {
                throw new NotSupportedException("Cannot set the value of an expando object.");
            }
        }

	    private class TypeValueAccessor : ValueAccessor
        {
            private readonly Type _type;

            public TypeValueAccessor(Type type)
            {
                _type = type;
            }

            public override object Get(object context)
            {
                return _type;
            }

            public override void Set(object context, object value)
            {
                throw new NotSupportedException("Cannot set the value of a type.");
            }
        }
    }
}