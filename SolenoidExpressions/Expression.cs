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
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Solenoid.Expressions.Parser;
using Solenoid.Expressions.Parser.antlr;
using Solenoid.Expressions.Parser.antlr.collections;
using Solenoid.Expressions.Support;
using Solenoid.Expressions.Support.Reflection.Dynamic;
using Solenoid.Expressions.Support.Util;
using StringUtils = Solenoid.Expressions.Support.Util.StringUtils;

namespace Solenoid.Expressions
{
	/// <summary>
	///     Container object for the parsed expression.
	/// </summary>
	/// <remarks>
	///     <p>
	///         Preparing this object once and reusing it many times for expression
	///         evaluation can result in significant performance improvements, as
	///         expression parsing and reflection lookups are only performed once.
	///     </p>
	/// </remarks>
	/// <author>Aleksandar Seovic</author>
	[Serializable]
	public class Expression : BaseNode
	{
		/// <summary>
		///     Contains a list of reserved variable names.
		///     You must not use any variable names with the reserved prefix!
		/// </summary>
		public static class ReservedVariableNames
		{
			/// <summary>
			///     Variable Names using this prefix are reserved for internal framework use
			/// </summary>
			public const string ReservedPrefix = "____spring_";

			/// <summary>
			///     variable name of the currently processed object factory, if any
			/// </summary>
			internal static readonly string CurrentObjectFactory = ReservedPrefix + "CurrentObjectFactory";
		}

		private class AstNodeCreator : ASTNodeCreator
		{
			private readonly SafeConstructor _ctor;
			private readonly string _name;

			public AstNodeCreator(ConstructorInfo ctor)
			{
				_ctor = new SafeConstructor(ctor);
				_name = ctor.DeclaringType.FullName;
			}

			public override AST Create()
			{
				return (AST) _ctor.Invoke(new object[0]);
			}

			public override string ASTNodeTypeName
			{
				get { return _name; }
			}
		}

		private class SpringAstFactory : ASTFactory
		{
			private static readonly Type _basenodeType;
			private static readonly Hashtable _typename2Creator;

			static SpringAstFactory()
			{
				_basenodeType = typeof (SerializableNode);

				_typename2Creator = new Hashtable();
				foreach (var type in typeof (SpringAstFactory).Assembly.GetTypes())
				{
					if (_basenodeType.IsAssignableFrom(type))
					{
						var ctor = type.GetConstructor(new Type[0]);
						if (ctor != null)
						{
							var creator = new AstNodeCreator(ctor);
							_typename2Creator[creator.ASTNodeTypeName] = creator;
						}
					}
				}
				_typename2Creator[_basenodeType.FullName] = Creator;
			}

			public SpringAstFactory() : base(_basenodeType)
			{
				defaultASTNodeTypeObject_ = _basenodeType;
				typename2creator_ = _typename2Creator;
			}
		}

		private class SolenoidExpressionParser : ExpressionParser
		{
			public SolenoidExpressionParser(TokenStream lexer)
				: base(lexer)
			{
				astFactory = new SpringAstFactory();
				initialize();
			}
		}

		static Expression()
		{
			// Ensure antlr is loaded (fixes GAC issues)!
			var antlrAss = typeof (LLkParser).Assembly;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Expression" /> class
		///     by parsing specified expression string.
		/// </summary>
		/// <param name="expression">Expression to parse.</param>
		public static IExpression Parse(string expression)
		{
			if (StringUtils.HasText(expression))
			{
				var lexer = new ExpressionLexer(new StringReader(expression));
				ExpressionParser parser = new SolenoidExpressionParser(lexer);

				try
				{
					parser.expr();
				}
				catch (TokenStreamRecognitionException ex)
				{
					throw new SyntaxErrorException(ex.recog.Message, ex.recog.Line, ex.recog.Column, expression);
				}
				return (IExpression) parser.getAST();
			}
			return new Expression();
		}

		/// <summary>
		///     Registers lambda expression under the specified <paramref name="functionName" />.
		/// </summary>
		/// <param name="functionName">Function name to register expression as.</param>
		/// <param name="lambdaExpression">Lambda expression to register.</param>
		/// <param name="variables">Variables dictionary that the function will be registered in.</param>
		public static void RegisterFunction(string functionName, string lambdaExpression, IDictionary variables)
		{
			AssertUtils.ArgumentHasText(functionName, "functionName");
			AssertUtils.ArgumentHasText(lambdaExpression, "lambdaExpression");

			var lexer = new ExpressionLexer(new StringReader(lambdaExpression));
			ExpressionParser parser = new SolenoidExpressionParser(lexer);

			try
			{
				parser.lambda();
			}
			catch (TokenStreamRecognitionException ex)
			{
				throw new SyntaxErrorException(ex.recog.Message, ex.recog.Line, ex.recog.Column, lambdaExpression);
			}
			variables[functionName] = parser.getAST();
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Expression" /> class
		///     by parsing specified primary expression string.
		/// </summary>
		/// <param name="expression">Primary expression to parse.</param>
		internal static IExpression ParsePrimary(string expression)
		{
			if (StringUtils.HasText(expression))
			{
				var lexer = new ExpressionLexer(new StringReader(expression));
				ExpressionParser parser = new SolenoidExpressionParser(lexer);

				try
				{
					parser.primaryExpression();
				}
				catch (TokenStreamRecognitionException ex)
				{
					throw new SyntaxErrorException(ex.recog.Message, ex.recog.Line,
						ex.recog.Column, expression);
				}
				return (IExpression) parser.getAST();
			}
			return new Expression();
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Expression" /> class
		///     by parsing specified property expression string.
		/// </summary>
		/// <param name="expression">Property expression to parse.</param>
		internal static IExpression ParseProperty(string expression)
		{
			if (StringUtils.HasText(expression))
			{
				var lexer = new ExpressionLexer(new StringReader(expression));
				ExpressionParser parser = new SolenoidExpressionParser(lexer);

				try
				{
					parser.property();
				}
				catch (TokenStreamRecognitionException ex)
				{
					throw new SyntaxErrorException(ex.recog.Message, ex.recog.Line, ex.recog.Column, expression);
				}
				return (IExpression) parser.getAST();
			}
			return new Expression();
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Expression" /> class.
		/// </summary>
		public Expression()
		{
		}

		/// <summary>
		///     Create a new instance from SerializationInfo
		/// </summary>
		protected Expression(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		/// <summary>
		///     Evaluates this expression for the specified root object and returns
		///     value of the last node.
		/// </summary>
		/// <param name="context">Context to evaluate expressions against.</param>
		/// <param name="evalContext">Current expression evaluation context.</param>
		/// <returns>Value of the last node.</returns>
		protected override object Get(object context, EvaluationContext evalContext)
		{
			var result = context;

			if (getNumberOfChildren() > 0)
			{
				var node = getFirstChild();
				while (node != null)
				{
					result = GetValue(((BaseNode) node), result, evalContext);

					node = node.getNextSibling();
				}
			}

			return result;
		}

		/// <summary>
		///     Evaluates this expression for the specified root object and sets
		///     value of the last node.
		/// </summary>
		/// <param name="context">Context to evaluate expressions against.</param>
		/// <param name="evalContext">Current expression evaluation context.</param>
		/// <param name="newValue">Value to set last node to.</param>
		/// <exception cref="NotSupportedException">If navigation expression is empty.</exception>
		protected override void Set(object context, EvaluationContext evalContext, object newValue)
		{
			var target = context;

			if (getNumberOfChildren() > 0)
			{
				var node = getFirstChild();

				for (var i = 0; i < getNumberOfChildren() - 1; i++)
				{
					try
					{
						target = GetValue(((BaseNode) node), target, evalContext);
						node = node.getNextSibling();
					}
					catch (NotReadablePropertyException e)
					{
						throw new NotWritablePropertyException(
							"Cannot read the value of '" + node.getText() + "' property in the expression.", e);
					}
				}
				SetValue(((BaseNode) node), target, evalContext, newValue);
			}
			else
			{
				throw new NotSupportedException("You cannot set the value for an empty expression.");
			}
		}

		/// <summary>
		///     Evaluates this expression for the specified root object and returns
		///     <see cref="PropertyInfo" /> of the last node, if possible.
		/// </summary>
		/// <param name="context">Context to evaluate expression against.</param>
		/// <param name="variables">Expression variables map.</param>
		/// <returns>Value of the last node.</returns>
		internal PropertyInfo GetPropertyInfo(object context, IDictionary<string, object> variables)
		{
			if (getNumberOfChildren() > 0)
			{
				var target = context;
				var node = getFirstChild();

				for (var i = 0; i < getNumberOfChildren() - 1; i++)
				{
					target = ((IExpression) node).GetValue(target, variables);
					node = node.getNextSibling();
				}

				var fieldNode = node as PropertyOrFieldNode;
				if (fieldNode != null)
				{
					return (PropertyInfo) fieldNode.GetMemberInfo(target);
				}
				var indexerNode = node as IndexerNode;
				if (indexerNode != null)
				{
					return indexerNode.GetPropertyInfo(target, variables);
				}
				throw new FatalReflectionException(
					"Cannot obtain PropertyInfo from an expression that does not resolve to a property or an indexer.");
			}

			throw new FatalReflectionException("Cannot obtain PropertyInfo for empty property name.");
		}
	}
}