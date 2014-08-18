using System;

namespace Solenoid.Expressions.Parser.antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/license.html
	*
	* $Id:$
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	[Serializable]
	public class SemanticException : RecognitionException
	{
		public SemanticException(string message) : base(message)
		{
		}
		
		[Obsolete("Replaced by SemanticException(string, string, int, int) since version 2.7.2.6", false)]
		public SemanticException(String message, String fileName, int line) :
					this(message, fileName, line, -1)
		{
			
		}

		public SemanticException(string message, string fileName, int line, int column) :
					base(message, fileName, line, column)
		{
		}
	}
}