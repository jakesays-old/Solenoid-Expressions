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
	public class RecognitionException : ANTLRException
	{
		public string FileName { get; set; }
		public int Line { get; set; }
		public int Column { get; set; }

		public RecognitionException() : base("parsing error")
		{
			FileName = null;
			Line = - 1;
			Column = - 1;
		}
		
		/*
		* RecognitionException constructor comment.
		* @param s java.lang.String
		*/
		public RecognitionException(string message) : base(message)
		{
			FileName = null;
			Line = - 1;
			Column = - 1;
		}
		
		public RecognitionException(string message, string fileName, int line, int column) : base(message)
		{
			FileName = fileName;
			Line = line;
			Column = column;
		}

		override public string ToString()
		{
			return FileLineFormatter.getFormatter().getFormatString(FileName, Line, Column) + Message;
		}
	}
}