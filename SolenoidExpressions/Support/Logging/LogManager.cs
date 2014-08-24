
/*
 * Copyright © 2002-2009 the original author or authors.
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
// ReSharper disable UnusedParameter.Global

using System;

namespace Solenoid.Expressions.Support.Logging
{
    /// <summary>
    /// Use the LogManager's <see cref="GetLogger(string)"/> or <see cref="GetLogger(System.Type)"/> 
    /// methods to obtain <see cref="ILog"/> instances for logging.
    /// </summary>
    /// <remarks>
    /// For configuring the underlying log system using application configuration, see the example 
    /// at <c>System.Configuration.ConfigurationManager</c>
    /// For configuring programmatically, see the example section below.
    /// </remarks>
    /// <example>
    /// The example below shows the typical use of LogManager to obtain a reference to a logger
    /// and log an exception:
    /// <code>
    /// 
    /// ILog log = LogManager.GetLogger(this.GetType());
    /// ...
    /// try 
    /// { 
    ///   /* .... */ 
    /// }
    /// catch(Exception ex)
    /// {
    ///   log.ErrorFormat("Hi {0}", ex, "dude");
    /// }
    /// 
    /// </code>
    /// The example below shows programmatic configuration of the underlying log system:
    /// <code>
    /// 
    /// // create properties
    /// NameValueCollection properties = new NameValueCollection();
    /// properties[&quot;showDateTime&quot;] = &quot;true&quot;;
    /// 
    /// // set Adapter
    /// Common.Logging.LogManager.Adapter = new 
    /// Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter(properties);
    /// 
    /// </code>
    /// </example>
    /// <seealso cref="ILog"/>
    /// <seealso cref="Adapter"/>
    /// <seealso cref="ILoggerFactoryAdapter"/>
    /// <author>Gilles Bayon</author>
    public static class LogManager
    {
        private static ILoggerFactoryAdapter _adapter;
        private static readonly object _loadLock = new object();

	    /// <summary>
        /// Gets or sets the adapter.
        /// </summary>
        /// <value>The adapter.</value>
        public static ILoggerFactoryAdapter Adapter
        {
            get
            {
                if (_adapter == null)
                {
                    lock (_loadLock)
                    {
                        if (_adapter == null)
                        {
                            _adapter = new NoOpLoggerFactoryAdapter();
                        }
                    }
                }
                return _adapter;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                lock (_loadLock)
                {
                    _adapter = value;
                }
            }
        }

	    /// <summary>
        /// Gets the logger by calling <see cref="ILoggerFactoryAdapter.GetLogger(Type)"/>
        /// on the currently configured <see cref="Adapter"/> using the specified type.
        /// </summary>
        /// <returns>the logger instance obtained from the current <see cref="Adapter"/></returns>
        public static ILog GetLogger<T>()
        {
            return Adapter.GetLogger(typeof(T));
        }

        /// <summary>
        /// Gets the logger by calling <see cref="ILoggerFactoryAdapter.GetLogger(Type)"/>
        /// on the currently configured <see cref="Adapter"/> using the specified type.
        /// </summary>
        /// <param key="type">The type.</param>
        /// <returns>the logger instance obtained from the current <see cref="Adapter"/></returns>
        public static ILog GetLogger(Type type)
        {
            return Adapter.GetLogger(type);
        }


        /// <summary>
        /// Gets the logger by calling <see cref="ILoggerFactoryAdapter.GetLogger(string)"/>
        /// on the currently configured <see cref="Adapter"/> using the specified key.
        /// </summary>
        /// <param key="key">The key.</param>
        /// <returns>the logger instance obtained from the current <see cref="Adapter"/></returns>
        public static ILog GetLogger(string name)
        {
            return Adapter.GetLogger(name);
        }
    }
}