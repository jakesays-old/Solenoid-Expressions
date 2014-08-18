options
{
	language = "CSharp";
	namespace = "Solenoid.Expressions.Parser";
}

class ExpressionParser extends Parser;

options {
	codeGenMakeSwitchThreshold = 3;
	codeGenBitsetTestThreshold = 4;
	classHeaderPrefix = "internal"; 
	buildAST=true;
	ASTLabelType = "Solenoid.Expressions.SerializableNode";
	k = 2;
}

tokens {
	EXPR;
	OPERAND;
	FALSE = "false";
	TRUE = "true";
	AND = "and";
	OR = "or";
	XOR = "xor";
	IN = "in";
	IS = "is";
	BETWEEN = "between";
	LIKE = "like";
	MATCHES = "matches";
	NULL_LITERAL = "null";
}

{
    // CLOVER:OFF
    
    public override void reportError(RecognitionException ex)
    {
		//base.reportError(ex);
        throw new antlr.TokenStreamRecognitionException(ex);
    }

    public override void reportError(string error)
    {
		//base.reportError(error);
        throw new RecognitionException(error);
    }
    
    private string GetRelationalOperatorNodeType(string op)
    {
        switch (op)
        {
            case "==" : return "Solenoid.Expressions.OpEqual";
            case "!=" : return "Solenoid.Expressions.OpNotEqual";
            case "<" : return "Solenoid.Expressions.OpLess";
            case "<=" : return "Solenoid.Expressions.OpLessOrEqual";
            case ">" : return "Solenoid.Expressions.OpGreater";
            case ">=" : return "Solenoid.Expressions.OpGreaterOrEqual";
            case "in" : return "Solenoid.Expressions.OpIn";
            case "is" : return "Solenoid.Expressions.OpIs";
            case "between" : return "Solenoid.Expressions.OpBetween";
            case "like" : return "Solenoid.Expressions.OpLike";
            case "matches" : return "Solenoid.Expressions.OpMatches";
            default : 
                throw new ArgumentException("Node type for operator '" + op + "' is not defined.");
        }
    }
}

expr : expression EOF!;

exprList 
    : LPAREN! expression (SEMI! expression)+ RPAREN!
        { #exprList = #([EXPR,"expressionList","Solenoid.Expressions.ExpressionListNode"], #exprList); }
    ;

expression	:	logicalOrExpression 
				(
					(ASSIGN^ <AST = Solenoid.Expressions.AssignNode> logicalOrExpression) 
				|   (DEFAULT^ <AST = Solenoid.Expressions.DefaultNode> logicalOrExpression) 
				|	(QMARK^ <AST = Solenoid.Expressions.TernaryNode> expression COLON! expression)
				)?
			;
			
parenExpr
    : LPAREN! expression RPAREN!;
    
logicalOrExpression : logicalXorExpression (OR^ <AST = Solenoid.Expressions.OpOr> logicalXorExpression)* ;

logicalXorExpression : logicalAndExpression (XOR^ <AST = Solenoid.Expressions.OpXor> logicalAndExpression)* ;
                        
logicalAndExpression : relationalExpression (AND^ <AST = Solenoid.Expressions.OpAnd> relationalExpression)* ;                        

relationalExpression
    :   e1:sumExpr 
          (op:relationalOperator! e2:sumExpr
            {#relationalExpression = #(#[EXPR, op_AST.getText(), GetRelationalOperatorNodeType(op_AST.getText())], #relationalExpression);} 
          )?
    ;

sumExpr  : prodExpr (
                        (PLUS^ <AST = Solenoid.Expressions.OpAdd> 
                        | MINUS^ <AST = Solenoid.Expressions.OpSubtract>) prodExpr)* ; 

prodExpr : powExpr (
                        (STAR^ <AST = Solenoid.Expressions.OpMultiply> 
                        | DIV^ <AST = Solenoid.Expressions.OpDivide> 
                        | MOD^ <AST = Solenoid.Expressions.OpModulous>) powExpr)* ;

powExpr  : unaryExpression (POWER^ <AST = Solenoid.Expressions.OpPower> unaryExpression)? ;

unaryExpression 
	:	(PLUS^ <AST = Solenoid.Expressions.OpUnaryPlus> 
	    | MINUS^ <AST = Solenoid.Expressions.OpUnaryMinus> 
	    | BANG^ <AST = Solenoid.Expressions.OpNot>) unaryExpression	
	|	primaryExpression
	;
	
unaryOperator
	: PLUS | MINUS | BANG
    ;
    
primaryExpression : startNode (node)?
			{ #primaryExpression = #([EXPR,"expression","Solenoid.Expressions.Expression"], #primaryExpression); };

startNode 
    : 
    (   (LPAREN expression SEMI) => exprList 
    |   parenExpr
    |   methodOrProperty 
    |   functionOrVar 
    |   localFunctionOrVar
    |   reference
    |   indexer 
    |   literal 
    |   type 
    |   constructor
    |   projection 
    |   selection 
    |   firstSelection 
    |   lastSelection 
    |   listInitializer
    |   mapInitializer
    |   lambda
    |   attribute
    )
    ;

node : 
    (   methodOrProperty 
    |   indexer 
    |   projection 
    |   selection 
    |   firstSelection 
    |   lastSelection 
    |   exprList
    |   DOT! 
    )+
    ;

functionOrVar 
    : (POUND ID LPAREN) => function
    | var
    ;

function : POUND! ID^ <AST = Solenoid.Expressions.FunctionNode> methodArgs
    ;
    
var : POUND! ID^ <AST = Solenoid.Expressions.VariableNode>;

localFunctionOrVar 
    : (DOLLAR ID LPAREN) => localFunction
    | localVar
    ;

localFunction 
    : DOLLAR! ID^ <AST = Solenoid.Expressions.LocalFunctionNode> methodArgs
    ;

localVar 
    : DOLLAR! ID^ <AST = Solenoid.Expressions.LocalVariableNode>
    ;

methodOrProperty
	: (ID LPAREN)=> ID^ <AST = Solenoid.Expressions.MethodNode> methodArgs
	| property
	;

methodArgs
	:  LPAREN! (argument (COMMA! argument)*)? RPAREN!
	;

property
    :  ID <AST = Solenoid.Expressions.PropertyOrFieldNode>
    ;

reference
	:  (AT! LPAREN! quotableName COLON) =>
		AT! LPAREN! cn:quotableName! COLON! id:quotableName! RPAREN!
		{ #reference = #([EXPR, "ref", "Spring.Context.Support.ReferenceNode"], #cn, #id); }

	|  AT! LPAREN! localid:quotableName! RPAREN!
       { #reference = #([EXPR, "ref", "Spring.Context.Support.ReferenceNode"], null, #localid); }
	;

indexer
	:  LBRACKET^ <AST = Solenoid.Expressions.IndexerNode> argument (COMMA! argument)* RBRACKET!
	;

projection
	:	
		PROJECT^ <AST = Solenoid.Expressions.ProjectionNode> expression RCURLY!
	;

selection
	:	
		SELECT^ <AST = Solenoid.Expressions.SelectionNode> expression (COMMA! expression)* RCURLY!
	;

firstSelection
	:	
		SELECT_FIRST^ <AST = Solenoid.Expressions.SelectionFirstNode> expression RCURLY!
	;

lastSelection
	:	
		SELECT_LAST^ <AST = Solenoid.Expressions.SelectionLastNode> expression RCURLY!
	;

type
    :   TYPE! tn:name! RPAREN!
		{ #type = #([EXPR, tn_AST.getText(), "Solenoid.Expressions.TypeNode"], #type); } 
    ;
     
name
	:	ID^ <AST = Solenoid.Expressions.QualifiedIdentifier> (~(RPAREN|COLON|QUOTE))*
	;
	
quotableName
    :	STRING_LITERAL^ <AST = Solenoid.Expressions.QualifiedIdentifier>
    |	name
    ;
    
attribute
	:	AT! LBRACKET! tn:qualifiedId! (ctorArgs)? RBRACKET!
		   { #attribute = #([EXPR, tn_AST.getText(), "Solenoid.Expressions.AttributeNode"], #attribute); }
	;

lambda
    :   LAMBDA! (argList)? PIPE! expression RCURLY!
		   { #lambda = #([EXPR, "lambda", "Solenoid.Expressions.LambdaExpressionNode"], #lambda); }
	;

argList : (ID (COMMA! ID)*)
		   { #argList = #([EXPR, "args"], #argList); }
	;

constructor
	:	("new" qualifiedId LPAREN) => "new"! type:qualifiedId! ctorArgs
		   { #constructor = #([EXPR, type_AST.getText(), "Solenoid.Expressions.ConstructorNode"], #constructor); }
	|   arrayConstructor
	;

arrayConstructor
	:	 "new"! type:qualifiedId! arrayRank (listInitializer)?
	       { #arrayConstructor = #([EXPR, type_AST.getText(), "Solenoid.Expressions.ArrayConstructorNode"], #arrayConstructor); }
	;
    
arrayRank
    :   LBRACKET^ (expression (COMMA! expression)*)? RBRACKET!
    ;

listInitializer
    :   LCURLY^ <AST = Solenoid.Expressions.ListInitializerNode> expression (COMMA! expression)* RCURLY!
    ;

mapInitializer
    :   POUND! LCURLY^ <AST = Solenoid.Expressions.MapInitializerNode> mapEntry (COMMA! mapEntry)* RCURLY!
    ;
      
mapEntry
    :   expression COLON! expression
          { #mapEntry = #([EXPR, "entry", "Solenoid.Expressions.MapEntryNode"], #mapEntry); }
    ;
     
ctorArgs : LPAREN! (namedArgument (COMMA! namedArgument)*)? RPAREN!;
            
argument : expression;

namedArgument 
    :   (ID ASSIGN) => ID^ <AST = Solenoid.Expressions.NamedArgumentNode> ASSIGN! expression 
    |   argument 
    ;

qualifiedId 
	: ID^ <AST = Solenoid.Expressions.QualifiedIdentifier> (DOT ID)*
    ;
    
literal
	:	NULL_LITERAL <AST = Solenoid.Expressions.NullLiteralNode>
	|   INTEGER_LITERAL <AST = Solenoid.Expressions.IntLiteralNode>
	|   HEXADECIMAL_INTEGER_LITERAL <AST = Solenoid.Expressions.HexLiteralNode>
	|   REAL_LITERAL <AST = Solenoid.Expressions.RealLiteralNode>
	|	STRING_LITERAL <AST = Solenoid.Expressions.StringLiteralNode>
	|   boolLiteral
	;

boolLiteral
    :   TRUE <AST = Solenoid.Expressions.BooleanLiteralNode>
    |   FALSE <AST = Solenoid.Expressions.BooleanLiteralNode>
    ;
    
relationalOperator
    :   EQUAL 
    |   NOT_EQUAL
    |   LESS_THAN
    |   LESS_THAN_OR_EQUAL      
    |   GREATER_THAN            
    |   GREATER_THAN_OR_EQUAL 
    |   IN   
    |   IS   
    |   BETWEEN   
    |   LIKE   
    |   MATCHES   
    ; 
    


class ExpressionLexer extends Lexer;

options {
    charVocabulary = '\u0000' .. '\uFFFE'; 
	classHeaderPrefix = "internal"; 
	k = 2;
	testLiterals = false;
}

{
    // CLOVER:OFF
}

WS	:	(' '
	|	'\t'
	|	'\n'
	|	'\r')
		{ _ttype = Token.SKIP; }
	;

AT: '@'
  ;

BACKTICK: '`'
  ;
  
BACKSLASH: '\\'
  ;
    
PIPE: '|'
  ;

BANG: '!'
  ;

QMARK: '?'
  ;

DOLLAR: '$'
  ;

POUND: '#'
  ;
    
LPAREN:	'('
	;

RPAREN:	')'
	;

LBRACKET:	'['
	;

RBRACKET:	']'
	;

LCURLY:	'{'
	;

RCURLY:	'}'
	;

COMMA : ','
   ;

SEMI: ';'
  ;

COLON: ':'
  ;

ASSIGN: '='
  ;

DEFAULT: "??"
  ;
  
PLUS: '+'
  ;

MINUS: '-'
  ;
   
DIV: '/'
  ;

STAR: '*'
  ;

MOD: '%'
  ;

POWER: '^'
  ;
  
EQUAL: "=="
  ;

NOT_EQUAL: "!="
  ;

LESS_THAN: '<'
  ;

LESS_THAN_OR_EQUAL: "<="
  ;

GREATER_THAN: '>'
  ;

GREATER_THAN_OR_EQUAL: ">="
  ;

PROJECT: "!{"
  ;
  
SELECT: "?{"
  ;

SELECT_FIRST: "^{"
  ;
  
SELECT_LAST: "${"
  ;

TYPE: "T("
  ;

LAMBDA: "{|"
  ;

DOT_ESCAPED: "\\."
  ;
  
QUOTE: '\''
  ;
  
STRING_LITERAL
	:	QUOTE! (APOS|~'\'')* QUOTE!
	;

protected
APOS : QUOTE! QUOTE
    ;
  
ID
options {
	testLiterals = true;
}
	: ('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'_'|'0'..'9')*
	;

NUMERIC_LITERAL

	// real
	:	('.' DECIMAL_DIGIT) =>
		 '.' (DECIMAL_DIGIT)+ (EXPONENT_PART)? (REAL_TYPE_SUFFIX)?
		{$setType(REAL_LITERAL);}
			
	|	((DECIMAL_DIGIT)+ '.' DECIMAL_DIGIT) =>
		 (DECIMAL_DIGIT)+ '.' (DECIMAL_DIGIT)+ (EXPONENT_PART)? (REAL_TYPE_SUFFIX)?
		{$setType(REAL_LITERAL);}
		
	|	((DECIMAL_DIGIT)+ (EXPONENT_PART)) =>
		 (DECIMAL_DIGIT)+ (EXPONENT_PART) (REAL_TYPE_SUFFIX)?
		{$setType(REAL_LITERAL);}
		
	|	((DECIMAL_DIGIT)+ (REAL_TYPE_SUFFIX)) =>
		 (DECIMAL_DIGIT)+ (REAL_TYPE_SUFFIX)		
		{$setType(REAL_LITERAL);}
		 
	// integer
	|	 (DECIMAL_DIGIT)+ (INTEGER_TYPE_SUFFIX)?	
		{$setType(INTEGER_LITERAL);}
	
	// just a dot
	| '.'{$setType(DOT);}
	;

	
HEXADECIMAL_INTEGER_LITERAL
	:	"0x"   (HEX_DIGIT)+   (INTEGER_TYPE_SUFFIX)?
	;

protected
DECIMAL_DIGIT
	: 	'0'..'9'
	;
	
protected	
INTEGER_TYPE_SUFFIX
	:
	(	options {generateAmbigWarnings=false;}
		:	"UL"	| "LU" 	| "ul"	| "lu"
		|	"UL"	| "LU" 	| "uL"	| "lU"
		|	"U"		| "L"	| "u"	| "l"
	)
	;
		
protected
HEX_DIGIT
	:	'0' | '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9' | 
		'A' | 'B' | 'C' | 'D' | 'E' | 'F'  |
		'a' | 'b' | 'c' | 'd' | 'e' | 'f'
	;	
	
protected	
EXPONENT_PART
	:	"e"  (SIGN)*  (DECIMAL_DIGIT)+
	|	"E"  (SIGN)*  (DECIMAL_DIGIT)+
	;	
	
protected
SIGN
	:	'+' | '-'
	;
	
protected
REAL_TYPE_SUFFIX
	: 'F' | 'f' | 'D' | 'd' | 'M' | 'm'
	;
