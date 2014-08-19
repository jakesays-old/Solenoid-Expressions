Solenoid Expression Language
=====================

**Authors of the original Spring.NET documentation:**
Mark Pollack , Rick Evans , Aleksandar Seovic , Bruno Baia , Erich Eichinger , Federico Spinazzi , Rob Harrop , Griffin Caprio , Ruben Bartelink , Choy Rim , Erez Mazor , Stephen Bohlen , The Spring Java Team

Copies of this document may be made for your own use and for distribution to others, provided that you do not charge any fee for such copies and further provided that each copy contains this Copyright Notice, whether distributed in print or electronically.

Introduction
============

The Solenoid.Expressions namespace provides a powerful expression language for querying and manipulating an object graph at runtime. The language supports setting and getting of property values, property assignment, method invocation, accessing the context of arrays, collections and indexers, logical and arithmetic operators, named variables, and retrieval of objects by name from Spring's IoC container. It also supports list projection and selection, as well as common list aggregators.

The functionality provided in this namespace serves as the foundation for a variety of other features in Solenoid such as enhanced property evaluation in the XML based configuration of the IoC container, a Data Validation framework, and a Data Binding framework for ASP.NET. You will likely find other cool uses for this library in your own work where run-time evaluation of criteria based on an object's state is required. For those with a Java background, the Solenoid.Expressions namespace provides functionality similar to the Java based Object Graph Navigation Language, [OGNL](http://www.ognl.org/).

This chapter covers the features of the expression language using an Inventor and Inventor's Society class as the target objects for expression evaluation. The class declarations and the data used to populate them are listed at the end of the chapter in section ?. These classes are blatantly taken from the NUnit tests for the Expressions namespace which you can refer to for additional example usage.

Evaluating Expressions
======================

The simplest, but not the most efficient way to perform expression evaluation is by using one of the static convenience methods of the `ExpressionEvaluator` class:

``` csharp
public static object GetValue(object root, string expression);

public static object GetValue(object root, string expression, IDictionary variables)

public static void SetValue(object root, string expression, object newValue)

public static void SetValue(object root, string expression, IDictionary variables, object newValue)
```

The first argument is the 'root' object that the expression string (2nd argument) will be evaluated against. The third argument is used to support variables in the expression and will be discussed later. Simple usage to get the value of an object property is shown below using the `Inventor` class. You can find the class listing in section ?.

``` csharp
Inventor tesla = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");

tesla.PlaceOfBirth.City = "Smiljan";

string evaluatedName = (string) ExpressionEvaluator.GetValue(tesla, "Name");

string evaluatedCity = (string) ExpressionEvaluator.GetValue(tesla, "PlaceOfBirth.City"));
```

The value of 'evaluatedName' is 'Nikola Tesla' and that of 'evaluatedCity' is 'Smiljan'. A period is used to navigate the nested properties of the object. Similarly to set the property of an object, say we want to rewrite history and change Tesla's city of birth, we would simply add the following line

``` csharp
ExpressionEvaluator.SetValue(tesla, "PlaceOfBirth.City", "Novi Sad");
```

A much better way to evaluate expressions is to parse them once and then evaluate as many times as you want using`Expression`class. Unlike `ExpressionEvaluator`, which parses expression every time you invoke one of its methods, `Expression` class will cache the parsed expression for increased performance. The methods of this class are listed below:

``` csharp
public static IExpression Parse(string expression)

public override object Get(object context, IDictionary variables)

public override void Set(object context, IDictionary variables, object newValue)
```

The retrieval of the Name property in the previous example using the Expression class is shown below

``` csharp
IExpression exp = Expression.Parse("Name");

string evaluatedName = (string) exp.GetValue(tesla, null);
```

The difference in performance between the two approaches, when evaluating the same expression many times, is several orders of magnitude, so you should only use convenience methods of the `ExpressionEvaluator` class when you are doing one-off expression evaluations. In all other cases you should parse the expression first and then evaluate it as many times as you need.

There are a few exception classes to be aware of when using the `ExpressionEvaluator`. These are `InvalidPropertyException`, when you refer to a property that doesn't exist, `NullValueInNestedPathException`, when a null value is encountered when traversing through the nested property list, and `ArgumentException` and `NotSupportedException` when you pass in values that are in error in some other manner.

The expression language is based on a grammar and uses [ANTLR](http://www.antlr.org/) to construct the lexer and parser. Errors relating to bad syntax of the language will be caught at this level of the language implementation. For those interested in the digging deeper into the implementation, the grammar file is named Expression.g and is located in the source directory. As a side note, the ANTLR runtime has been included in to the Solenoid source tree, removing the dependency on the antlr.runtime.dll.

Language Reference
==================

Literal expressions
-------------------

The types of literal expressions supported are strings, dates, numeric values (int, real, and hex), boolean and null. String are delimited by single quotes. To put a single quote itself in a string use the backslash character. The following listing shows simple usage of literals. Typically they would not be used in isolation like this, but as part of a more complex expression, for example using a literal on one side of a logical comparison operator.

``` csharp
string helloWorld = (string) ExpressionEvaluator.GetValue(null, "'Hello World'"); // evals to "Hello World"

string tonyPizza  = (string) ExpressionEvaluator.GetValue(null, "'Tony\\'s Pizza'"); // evals to "Tony's Pizza"

double avogadrosNumber = (double) ExpressionEvaluator.GetValue(null, "6.0221415E+23");

int maxValue = (int)  ExpressionEvaluator.GetValue(null, "0x7FFFFFFF");  // evals to 2147483647

DateTime birthday = (DateTime) ExpressionEvaluator.GetValue(null, "date('1974/08/24')");

DateTime exactBirthday =
  (DateTime) ExpressionEvaluator.GetValue(null, " date('19740824T131030', 'yyyyMMddTHHmmss')");

bool trueValue = (bool) ExpressionEvaluator.GetValue(null, "true");

object nullValue = ExpressionEvaluator.GetValue(null, "null");
```

Note that the extra backslash character in Tony's Pizza is to satisfy C\# escape syntax. Numbers support the use of the negative sign, exponential notation, and decimal points. By default real numbers are parsed using `Double.Parse` unless the format character "M" or "F" is supplied, in which case `Decimal.Parse` and `Single.Parse` would be used respectfully. As shown above, if two arguments are given to the date literal then `DateTime.ParseExact` will be used. Note that all parse methods of classes that are used internally reference the `CultureInfo.InvariantCulture`.

Properties, Arrays, Lists, Dictionaries, Indexers
-------------------------------------------------

As shown in the previous example in ?, navigating through properties is easy, just use a period to indicate a nested property value. The instances of `Inventor` class, *pupin* and *tesla*, were populated with data listed in section ?. To navigate "down" and get Tesla's year of birth and Pupin's city of birth the following expressions are used

``` csharp
int year = (int) ExpressionEvaluator.GetValue(tesla, "DOB.Year"));  // 1856

string city = (string) ExpressionEvaluator.GetValue(pupin, "PlaCeOfBirTh.CiTy");  // "Idvor"
```

For the sharp-eyed, that isn't a typo in the property name for place of birth. The expression uses mixed cases to demonstrate that the evaluation is case insensitive.

The contents of arrays and lists are obtained using square bracket notation.

``` csharp
// Inventions Array
string invention = (string) ExpressionEvaluator.GetValue(tesla, "Inventions[3]"); // "Induction motor"

// Members List
string name = (string) ExpressionEvaluator.GetValue(ieee, "Members[0].Name"); // "Nikola Tesla"

// List and Array navigation
string invention = (string) ExpressionEvaluator.GetValue(ieee, "Members[0].Inventions[6]") // "Wireless communication"
```

The contents of dictionaries are obtained by specifying the literal key value within the brackets. In this case, because keys for the *Officers* dictionary are strings, we can specify string literal.

``` csharp
// Officer's Dictionary
Inventor pupin = (Inventor) ExpressionEvaluator.GetValue(ieee, "Officers['president']";

string city = (string) ExpressionEvaluator.GetValue(ieee, "Officers['president'].PlaceOfBirth.City"); // "Idvor"

ExpressionEvaluator.SetValue(ieee, "Officers['advisors'][0].PlaceOfBirth.Country", "Croatia");
```

You may also specify non literal values in place of the quoted literal values by using another expression inside the square brackets such as variable names or static properties/methods on other types. These features are discussed in other sections.

Indexers are similarly referenced using square brackets. The following is a small example that shows the use of indexers. Multidimensional indexers are also supported.

``` csharp
public class Bar
{
    private int[] numbers = new int[] {1, 2, 3};

    public int this[int index]
    {
        get { return numbers[index];}
        set { numbers[index] = value; }
    }
}

Bar b = new Bar();

int val = (int) ExpressionEvaluator.GetValue(bar, "[1]") // evaluated to 2

ExpressionEvaluator.SetValue(bar, "[1]", 3);  // set value to 3
```

### Defining Arrays, Lists and Dictionaries Inline

In addition to accessing arrays, lists and dictionaries by navigating the graph for the context object, Solenoid Expression Language allows you to define them inline, within the expression. Inline lists are defined by simply enclosing a comma separated list of items with curly brackets:

    {1, 2, 3, 4, 5}
    {'abc', 'xyz'}

If you want to ensure that a strongly typed array is initialized instead of a weakly typed list, you can use array initializer instead:

``` csharp
new int[] {1, 2, 3, 4, 5}
new string[] {'abc', 'xyz'}
```

Dictionary definition syntax is a bit different: you need to use a \# prefix to tell expression parser to expect key/value pairs within the brackets and to specify a comma separated list of key/value pairs within the brackets:

    #{'key1' : 'Value 1', 'today' : DateTime.Today}
    #{1 : 'January', 2 : 'February', 3 : 'March', ...}

Arrays, lists and dictionaries created this way can be used anywhere where arrays, lists and dictionaries obtained from the object graph can be used, which we will see later in the examples.

Keep in mind that even though examples above use literals as array/list elements and dictionary keys and values, that's only to simplify the examples -- you can use any valid expression wherever literals are used.

Methods
-------

Methods are invoked using typical C\# programming syntax. You may also invoke methods on literals.

``` csharp
//string literal
char[] chars = (char[]) ExpressionEvaluator.GetValue(null, "'test'.ToCharArray(1, 2)"))  // 't','e'

//date literal
int year = (int) ExpressionEvaluator.GetValue(null, "date('1974/08/24').AddYears(31).Year") // 2005

// object usage, calculate age of tesla navigating from the IEEE society.

ExpressionEvaluator.GetValue(ieee, "Members[0].GetAge(date('2005-01-01')") // 149 (eww..a big anniversary is coming up ;)
```

Operators
---------

### Relational operators

The relational operators; equal, not equal, less than, less than or equal, greater than, and greater than or equal are supported using standard operator notation. These operators take into account if the object implements the `IComparable` interface. Enumerations are also supported but you will need to register the enumeration type, as described in Section ?, in order to use an enumeration value in an expression if it is not contained in the mscorlib.

``` csharp
ExpressionEvaluator.GetValue(null, "2 == 2")  // true

ExpressionEvaluator.GetValue(null, "date('1974-08-24') != DateTime.Today")  // true

ExpressionEvaluator.GetValue(null, "2 < -5.0") // false

ExpressionEvaluator.GetValue(null, "DateTime.Today <= date('1974-08-24')") // false

ExpressionEvaluator.GetValue(null, "'Test' >= 'test'") // true
```

Enumerations can be evaluated as shown below

``` csharp
FooColor fColor = new FooColor();

ExpressionEvaluator.SetValue(fColor, "Color", KnownColor.Blue);

bool trueValue = (bool) ExpressionEvaluator.GetValue(fColor, "Color == KnownColor.Blue"); //true
```

Where FooColor is the following class.

``` csharp
public class FooColor
{
    private KnownColor knownColor;

    public KnownColor Color
    {
        get { return knownColor;}
        set { knownColor = value; }
    }
}
```

In addition to standard relational operators, Solenoid Expression Language supports some additional, very useful operators that were "borrowed" from SQL, such as *in*, *like* and *between*, as well as *is* and *matches* operators, which allow you to test if object is of a specific type or if the value matches a regular expression.

``` csharp
ExpressionEvaluator.GetValue(null, "3 in {1, 2, 3, 4, 5}")  // true

ExpressionEvaluator.GetValue(null, "'Abc' like '[A-Z]b*'")  // true

ExpressionEvaluator.GetValue(null, "'Abc' like '?'")  // false

ExpressionEvaluator.GetValue(null, "1 between {1, 5}")  // true

ExpressionEvaluator.GetValue(null, "'efg' between {'abc', 'xyz'}")  // true

ExpressionEvaluator.GetValue(null, "'xyz' is int")  // false

ExpressionEvaluator.GetValue(null, "{1, 2, 3, 4, 5} is IList")  // true

ExpressionEvaluator.GetValue(null, "'5.0067' matches '^-?\\d+(\\.\\d{2})?$'"))  // false

ExpressionEvaluator.GetValue(null, @"'5.00' matches '^-?\d+(\.\d{2})?$'")  // true
```

Note that the Visual Basic and not SQL syntax is used for the *like* operator pattern string.

### Logical operators

The logical operators that are supported are *and*, *or*, and *not*. Their use is demonstrated below

``` csharp
// AND
bool falseValue = (bool) ExpressionEvaluator.GetValue(null, "true and false"); //false

string expression = @"IsMember('Nikola Tesla') and IsMember('Mihajlo Pupin')";
bool trueValue = (bool) ExpressionEvaluator.GetValue(ieee, expression);  //true

// OR
bool trueValue = (bool) ExpressionEvaluator.GetValue(null, "true or false");  //true

string expression = @"IsMember('Nikola Tesla') or IsMember('Albert Einstien')";
bool trueValue = (bool) ExpressionEvaluator.GetValue(ieee, expression); // true

// NOT
bool falseValue = (bool) ExpressionEvaluator.GetValue(null, "!true");

// AND and NOT
string expression = @"IsMember('Nikola Tesla') and !IsMember('Mihajlo Pupin')";
bool falseValue = (bool) ExpressionEvaluator.GetValue(ieee, expression);
```

### Bitwise operators

The bitwise operators that are supported are *and*, *or*, *xor* and *not*. Their use is demonstrated below. Note, that the logical and bitwise operators are the same and their interpretation depends if you pass in integral values or boolean values.

``` csharp
// AND
int result = (int) ExpressionEvaluator.GetValue(null, "1 and 3"); // 1 & 3

// OR
int result = (int) ExpressionEvaluator.GetValue(null, "1 or 3");  // 1 | 3

// XOR
int result = (int) ExpressionEvaluator.GetValue(null, "1 xor 3");  // 1 ^ 3

// NOT
int result = (int) ExpressionEvaluator.GetValue(null, "!1"); // ~1
```

### Mathematical operators

The addition operator can be used on numbers, strings and dates. Subtraction can be used on numbers and dates. Multiplication and division can be used only on numbers. Other mathematical operators supported are modulus (%) and exponential power (^). Standard operator precedence is enforced. These operators are demonstrated below

``` csharp
// Addition
int two = (int)ExpressionEvaluator.GetValue(null, "1 + 1"); // 2

String testString = (String)ExpressionEvaluator.GetValue(null, "'test' + ' ' + 'string'"); //'test string'

DateTime dt = (DateTime)ExpressionEvaluator.GetValue(null, "date('1974-08-24') + 5"); // 8/29/1974

// Subtraction

int four = (int) ExpressionEvaluator.GetValue(null, "1 - -3"); //4

Decimal dec = (Decimal) ExpressionEvaluator.GetValue(null, "1000.00m - 1e4"); // 9000.00

TimeSpan ts = (TimeSpan) ExpressionEvaluator.GetValue(null, "date('2004-08-14') - date('1974-08-24')"); //10948.00:00:00

// Multiplication

int six = (int) ExpressionEvaluator.GetValue(null, "-2 * -3"); // 6

int twentyFour = (int) ExpressionEvaluator.GetValue(null, "2.0 * 3e0 * 4"); // 24

// Division

int minusTwo = (int) ExpressionEvaluator.GetValue(null, "6 / -3"); // -2

int one = (int) ExpressionEvaluator.GetValue(null, "8.0 / 4e0 / 2"); // 1

// Modulus

int three = (int) ExpressionEvaluator.GetValue(null, "7 % 4"); // 3

int one = (int) ExpressionEvaluator.GetValue(null, "8.0 % 5e0 % 2"); // 1

// Exponent

int sixteen = (int) ExpressionEvaluator.GetValue(null, "-2 ^ 4"); // 16

// Operator precedence

int minusFortyFive = (int) ExpressionEvaluator.GetValue(null, "1+2-3*8^2/2/2"); // -45
```

Assignment
----------

Setting of a property is done by using the assignment operator. This would typically be done within a call to `GetValue` since in the simple case `SetValue` offers the same functionality. Assignment in this manner is useful when combining multiple operators in an expression list, discussed in the next section. Some examples of assignment are shown below

``` csharp
Inventor inventor = new Inventor();
String aleks = (String) ExpressionEvaluator.GetValue(inventor, "Name = 'Aleksandar Seovic'");
DateTime dt = (DateTime) ExpressionEvaluator.GetValue(inventor, "DOB = date('1974-08-24')");

//Set the vice president of the society
Inventor tesla = (Inventor) ExpressionEvaluator.GetValue(ieee, "Officers['vp'] = Members[0]");
```

Expression lists
----------------

Multiple expressions can be evaluated against the same context object by separating them with a semicolon and enclosing the entire expression within parentheses. The value returned is the value of the last expression in the list. Examples of this are shown below

``` csharp
//Perform property assignments and then return Name property.

String pupin = (String) ExpressionEvaluator.GetValue(ieee.Members,
  "( [1].PlaceOfBirth.City = 'Beograd'; [1].PlaceOfBirth.Country = 'Serbia'; [1].Name )"));

// pupin = "Mihajlo Pupin"
```

Types
-----

In many cases, you can reference types by simply specifying type name:

``` csharp
ExpressionEvaluator.GetValue(null, "1 is int")

ExpressionEvaluator.GetValue(null, "DateTime.Today")

ExpressionEvaluator.GetValue(null, "new string[] {'abc', 'efg'}")
```

This is possible for all standard types from `mscorlib`, as well as for any other type that is registered with the `TypeRegistry` as described in the next section.

For all other types, you need to use special `T(typeName)` expression:

``` csharp
Type dateType = (Type) ExpressionEvaluator.GetValue(null, "T(System.DateTime)")

Type evalType = (Type) ExpressionEvaluator.GetValue(null, "T(Solenoid.Expressions.ExpressionEvaluator, Solenoid.Core)")

bool trueValue = (bool) ExpressionEvaluator.GetValue(tesla, "T(System.DateTime) == DOB.GetType()")
```

> **Note**
>
> The implementation delegates to Spring's `ObjectUtils.ResolveType` method for the actual type resolution, which means that the types used within expressions are resolved in the exactly the same way as the types specified in Spring configuration files.

Type Registration
-----------------

To refer to a type within an expression that is not in the mscorlib you need to register it with the `TypeRegistry`. This will allow you to refer to a shorthand name of the type within your expressions. This is commonly used in expression that use the new operator or refer to a static properties of an object. Example usage is shown below.

``` csharp
TypeRegistry.RegisterType("Society", typeof(Society));

Inventor pupin = (Inventor) ExpressionEvaluator.GetValue(ieee, "Officers[Society.President]");
```

Alternatively, you can register types using `typeAliases` configuration section.

Constructors
------------

Constructors can be invoked using the new operator. For classes outside mscorlib you will need to register your types so they can be resolved. Examples of using constructors are shown below:

``` csharp
// simple ctor
DateTime dt = (DateTime) ExpressionEvaluator.GetValue(null, "new DateTime(1974, 8, 24)");

// Register Inventor type then create new inventor instance within Add method inside an expression list.
// Then return the new count of the Members collection.

TypeRegistry.RegisterType(typeof(Inventor));
int three = (int) ExpressionEvaluator.GetValue(ieee.Members, "{ Add(new Inventor('Aleksandar Seovic', date('1974-08-24'), 'Serbian')); Count}"));
```

As a convenience, Solenoid also allows you to define named constructor arguments, which are used to set object's properties after instantiation, similar to the way standard .NET attributes work. For example, you could create an instance of the `Inventor` class and set its `Inventions` property in a single statement:

``` csharp
Inventor aleks = (Inventor) ExpressionEvaluator.GetValue(null, "new Inventor('Aleksandar Seovic', date('1974-08-24'), 'Serbian', Inventions = {'SPELL'})");
```

The only rule you have to follow is that named arguments should be specified *after* standard constructor arguments, just like in the .NET attributes.

While we are on the subject, Solenoid Expression Language also provides a convenient syntax for .NET attribute instance creation. Instead of using standard constructor syntax, you can use a somewhat shorter and more familiar syntax to create an instance of a .NET attribute class:

``` csharp
WebMethodAttribute webMethod = (WebMethodAttribute) ExpressionEvaluator.GetValue(null, "@[WebMethod(true, CacheDuration = 60, Description = 'My Web Method')]");
```

As you can see, with the exception of the `@` prefix, syntax is exactly the same as in C\#.

Slightly different syntax is not the only thing that differentiates an attribute expression from a standard constructor invocation expression. In addition to that, attribute expression uses slightly different type resolution mechanism and will attempt to load both the specified type name and the specified type name with an `Attribute` suffix, just like the C\# compiler.

Variables
---------

Variables can referenced in the expression using the syntax `#`*variableName*. The variables are passed in and out of the expression using the dictionary parameter in `ExpressionEvaluator`'s `GetValue` or `SetValue` methods.

``` csharp
public static object GetValue(object root, string expression, IDictionary variables)

public static void SetValue(object root, string expression, IDictionary variables, object newValue)
```

The variable name is the key value of the dictionary. Example usage is shown below;

``` csharp
IDictionary vars = new Hashtable();
vars["newName"] = "Mike Tesla";
ExpressionEvaluator.GetValue(tesla, "Name = #newName", vars));
```

You can also use the dictionary as a place to store values of the object as they are evaluated inside the expression. For example to change Tesla's first name back again and keep the old value;

``` csharp
ExpressionEvaluator.GetValue(tesla, "{ #oldName = Name; Name = 'Nikola Tesla' }", vars);
String oldName = (String)vars["oldName"]; // Mike Tesla
```

Variable names can also be used inside indexers or maps instead of literal values. For example;

``` csharp
vars["prez"] = "president";
Inventor pupin = (Inventor) ExpressionEvaluator.GetValue(ieee, "Officers[#prez]", vars);
```

### The '\#this' and '\#root' variables

There are two special variables that are always defined and can be references within the expression: `#this` and `#root`.

The `#this` variable can be used to explicitly refer to the context for the node that is currently being evaluated:

``` csharp
// sets the name of the president and returns its instance
ExpressionEvaluator.GetValue(ieee, "Officers['president'].( #this.Name = 'Nikola Tesla'; #this )")
```

Similarly, the `#root` variable allows you to refer to the root context for the expression:

``` csharp
// removes president from the Officers dictionary and returns removed instance
ExpressionEvaluator.GetValue(ieee, "Officers['president'].( #root.Officers.Remove('president'); #this )")
```

Ternary Operator (If-Then-Else)
-------------------------------

You can use the ternary operator for performing if-then-else conditional logic inside the expression. A minimal example is;

``` csharp
String aTrueString  = (String) ExpressionEvaluator.GetValue(null, "false ? 'trueExp' : 'falseExp'") // trueExp
```

In this case, the boolean false results in returning the string value 'trueExp'. A less artificial example is shown below

``` csharp
ExpressionEvaluator.SetValue(ieee, "Name", "IEEE");
IDictionary vars = new Hashtable();
vars["queryName"] = "Nikola Tesla";

string expression = @"IsMember(#queryName)
                      ? #queryName + ' is a member of the ' + Name + ' Society'
                      : #queryName + ' is not a member of the ' + Name + ' Society'";

String queryResultString = (String) ExpressionEvaluator.GetValue(ieee, expression, vars));

// queryResultString = "Nikola Tesla is a member of the IEEE Society"
```

List Projection and Selection
-----------------------------

List projection and selection are very powerful expression language features that allow you to transform the source list into another list by either *projecting* across its "columns", or *selecting* from its "rows". In other words, projection can be thought of as a column selector in a SQL SELECT statement, while selection would be comparable to the WHERE clause.

For example, let's say that we need a list of the cities where our inventors were born. This could be easily obtained by projecting on the `PlaceOfBirth.City` property:

``` csharp
IList placesOfBirth = (IList) ExpressionEvaluator.GetValue(ieee, "Members.!{PlaceOfBirth.City}")  // { 'Smiljan', 'Idvor' }
```

Or we can get the list of officers' names:

``` csharp
IList officersNames = (IList) ExpressionEvaluator.GetValue(ieee, "Officers.Values.!{Name}")  // { 'Nikola Tesla', 'Mihajlo Pupin' }
```

As you can see from the examples, projection uses `!{`*projectionExpression*`}` syntax and will return a new list of the same length as the original list but typically with the elements of a different type.

On the other hand, selection, which uses `?{`*projectionExpression*`}` syntax, will filter the list and return a new list containing a subset of the original element list. For example, selection would allow us to easily get a list of Serbian inventors:

``` csharp
IList serbianInventors = (IList) ExpressionEvaluator.GetValue(ieee, "Members.?{Nationality == 'Serbian'}")  // { tesla, pupin }
```

Or to get a list of inventors that invented sonar:

``` csharp
IList sonarInventors = (IList) ExpressionEvaluator.GetValue(ieee, "Members.?{'Sonar' in Inventions}")  // { pupin }
```

Or we can combine selection and projection to get a list of sonar inventors' names:

``` csharp
IList sonarInventorsNames = (IList) ExpressionEvaluator.GetValue(ieee, "Members.?{'Sonar' in Inventions}.!{Name}")  // { 'Mihajlo Pupin' }
```

As a convenience, Solenoid Expression Language also supports a special syntax for selecting the first or last match. Unlike regular selection, which will return an empty list if no matches are found, first or last match selection expression will either return an instance of the matched element, or `null` if no matching elements were found. In order to return a first match you should prefix your selection expression with `^{` instead of `?{`, and to return last match you should use `${` prefix:

``` csharp
ExpressionEvaluator.GetValue(ieee, "Members.^{Nationality == 'Serbian'}.Name")  // 'Nikola Tesla'
ExpressionEvaluator.GetValue(ieee, "Members.${Nationality == 'Serbian'}.Name")  // 'Mihajlo Pupin'
```

Notice that we access the `Name` property directly on the selection result, because an actual matched instance is returned by the first and last match expression instead of a filtered list.

Collection Processors and Aggregators
-------------------------------------

In addition to list projection and selection, Solenoid Expression Language also supports several collection processors, such as `distinct`, `nonNull` and `sort`, as well as a number of commonly used aggregators, such as `max`, `min`, `count`, `sum` and `average`.

The difference between processors and aggregators is that processors return a new or transformed collection, while aggregators return a single value. Other than that, they are very similar -- both processors and aggregators are invoked on a collection node using standard method invocation expression syntax, which makes them very simple to use and allows easy chaining of multiple processors.

### Count Aggregator

The count aggregator is a safe way to obtain a number of items in a collection. It can be applied to a collection of any type, including arrays, which helps eliminate the decision on whether to use `Count` or `Length` property depending on the context. Unlike its standard .NET counterparts, count aggregator can also be invoked on the `null` context without throwing a `NullReferenceException`. It will simply return zero in this case, which makes it much safer than standard .NET properties within larger expression.

``` csharp
ExpressionEvaluator.GetValue(null, "{1, 5, -3}.count()")  // 3
ExpressionEvaluator.GetValue(null, "count()")  // 0
```

### Sum Aggregator

The sum aggregator can be used to calculate a total for the list of numeric values. If numbers within the list are not of the same type or precision, it will automatically perform necessary conversion and the result will be the highest precision type. If any of the collection elements is not a number, this aggregator will throw an `InvalidArgumentException`.

``` csharp
ExpressionEvaluator.GetValue(null, "{1, 5, -3, 10}.sum()")  // 13 (int)
ExpressionEvaluator.GetValue(null, "{5, 5.8, 12.2, 1}.sum()")  // 24.0 (double)
```

### Average Aggregator

The average aggregator will return the average for the collection of numbers. It will use the same type coercion rules, as the sum aggregator in order to be as precise as possible. Just like the sum aggregator, if any of the collection elements is not a number, it will throw an `InvalidArgumentException`.

``` csharp
ExpressionEvaluator.GetValue(null, "{1, 5, -4, 10}.average()")  // 3
ExpressionEvaluator.GetValue(null, "{1, 5, -2, 10}.average()")  // 3.5
```

### Minimum Aggregator

The minimum aggregator will return the smallest item in the list. In order to determine what "the smallest" actually means, this aggregator relies on the assumption that the collection items are of the uniform type and that they implement the `IComparable` interface. If that is not the case, this aggregator will throw an `InvalidArgumentException`.

``` csharp
ExpressionEvaluator.GetValue(null, "{1, 5, -3, 10}.min()")  // -3
ExpressionEvaluator.GetValue(null, "{'abc', 'efg', 'xyz'}.min()")  // 'abc'
```

### Maximum Aggregator

The maximum aggregator will return the largest item in the list. In order to determine what "the largest" actually means, this aggregator relies on the assumption that the collection items are of the uniform type and that they implement `IComparable` interface. If that is not the case, this aggregator will throw an `InvalidArgumentException`.

``` csharp
ExpressionEvaluator.GetValue(null, "{1, 5, -3, 10}.max()")  // 10
ExpressionEvaluator.GetValue(null, "{'abc', 'efg', 'xyz'}.max()")  // 'xyz'
```

### Non-null Processor

A non-null processor is a very simple collection processor that eliminates all `null` values from the collection.

``` csharp
ExpressionEvaluator.GetValue(null, "{ 'abc', 'xyz', null, 'abc', 'def', null}.nonNull()")  // { 'abc', 'xyz', 'abc', 'def' }
ExpressionEvaluator.GetValue(null, "{ 'abc', 'xyz', null, 'abc', 'def', null}.nonNull().distinct().sort()")  // { 'abc', 'def', 'xyz' }
```

### Distinct Processor

A distinct processor is very useful when you want to ensure that you don't have duplicate items in the collection. It can also accept an optional `Boolean` argument that will determine whether `null` values should be included in the results. The default is `false`, which means that they will not be included.

``` csharp
ExpressionEvaluator.GetValue(null, "{ 'abc', 'xyz', 'abc', 'def', null, 'def' }.distinct(true).sort()")  // { null, 'abc', 'def', 'xyz' }
ExpressionEvaluator.GetValue(null, "{ 'abc', 'xyz', 'abc', 'def', null, 'def' }.distinct(false).sort()")  // { 'abc', 'def', 'xyz' }
```

### Sort Processor

The sort processor can be used to sort uniform collections of elements that implement `IComparable`.

``` csharp
ExpressionEvaluator.GetValue(null, "{1.2, 5.5, -3.3}.sort()")  // { -3.3, 1.2, 5.5 }
ExpressionEvaluator.GetValue(null, "{ 'abc', 'xyz', 'abc', 'def', null, 'def' }.sort()")  // { null, 'abc', 'abc', 'def', 'def', 'xyz' }
```

The sort processor also accepts a boolean value as an argument to determine sort order, sort(false) will sort the collection in decending order.

### Type Conversion Processor

The convert processor can be used to convert a collection of elements to a given Type.

``` csharp
object[] arr = new object[] { "0", 1, 1.1m, "1.1", 1.1f };
decimal[] result = (decimal[]) ExpressionEvaluator.GetValue(arr, "convert(decimal)");
```

### Reverse Processor

The reverse processor returns the reverse order of elements in the list

``` csharp
object[] arr = new object[] { "0", 1, 2.1m, "3", 4.1f };
object[] result = new ArrayList( (ICollection) ExpressionEvaluator.GetValue(arr, "reverse()") ).ToArray(); // { 4.1f, "3", 2.1m, 1, "0" }            
```

### OrderBy Processor

Collections can be ordered in three ways, an expression, a SpEL lamda expreression, or a delegate.

``` csharp
// orderBy expression
IExpression exp = Expression.Parse("orderBy('ToString()')");
object[] input = new object[] { 'b', 1, 2.0, "a" };
object[] ordered = exp.GetValue(input);  // { 1, 2.0, "a", 'b' }


// SpEL lambda expressions
IExpression exp = Expression.Parse("orderBy({|a,b| $a.ToString().CompareTo($b.ToString())})");
object[] input = new object[] { 'b', 1, 2.0, "a" };
object[] ordered = exp.GetValue(input);  // { 1, 2.0, "a", 'b' }

Hashtable vars = new Hashtable();
Expression.RegisterFunction( "compare", "{|a,b| $a.ToString().CompareTo($b.ToString())}", vars);
exp = Expression.Parse("orderBy(#compare)");
ordered = exp.GetValue(input, vars);  // { 1, 2.0, "a", 'b' }

// .NET delegate
private delegate int CompareCallback(object x, object y);
private int CompareObjects(object x, object y)
{
  if (x == y) return 0;
  return x.ToString().CompareTo(""+y);
}

Hashtable vars = new Hashtable();
vars["compare"] = new CompareCallback(CompareObjects);

IExpression exp = Expression.Parse("orderBy(#compare)");
object[] input = new object[] { 'b', 1, 2.0, "a" };
object[] ordered = exp.GetValue(input);  // { 1, 2.0, "a", 'b' }
```

### User Defined Collection Processor

You can register your own collection processor for use in evaluation a collection. Here is an example of a ICollectionProcessor implementation that sums only the even numbers of an integer list

``` csharp
        public class IntEvenSumCollectionProcessor : ICollectionProcessor
        {
            public object Process(ICollection source, object[] args)
            {
                object total = 0d;
                foreach (object item in source)
                {
                    if (item != null)
                    {
                        if (NumberUtils.IsInteger(item))
                        {
                            if ((int)item % 2 == 0)
                            {
                                total = NumberUtils.Add(total, item);
                            }
                        }
                        else
                        {
                            throw new ArgumentException("Sum can only be calculated for a collection of numeric values.");
                        }
                    }
                }

                return total;
            }
        }


        public void DoWork()
        {
            Hashtable vars = new Hashtable();
            vars["EvenSum"] = new IntEvenSumCollectionProcessor();
            int result = (int)ExpressionEvaluator.GetValue(null, "{1, 2, 3, 4}.EvenSum()", vars));  // 6
        }
```

Lambda Expressions
------------------

A somewhat advanced, but a very powerful feature of Solenoid Expression Language are lambda expressions. Lambda expressions allow you to define inline functions, which can then be used within your expressions just like any other function or method. You may also use .NET delegates as described in the next section.

The syntax for defining lambda expressions is:

`#`*functionName*` =
      {|`*argList*`|
      `*functionBody*` }`

For example, you could define a `max` function and call it like this:

``` csharp
ExpressionEvaluator.GetValue(null, "(#max = {|x,y| $x > $y ? $x : $y }; #max(5,25))", new Hashtable())  // 25
```

As you can see, any arguments defined for the expression can be referenced within the function body using a *local variable* syntax, `$`*varName*. Invocation of the function defined using lambda expression is as simple as specifying the comma-separated list of function arguments in parentheses, after the function name.

Lambda expressions can be recursive, which means that you can invoke the function within its own body:

``` csharp
ExpressionEvaluator.GetValue(null, "(#fact = {|n| $n <= 1 ? 1 : $n * #fact($n-1) }; #fact(5))", new Hashtable())  // 120
```

Notice that in both examples above we had to specify a `variables` parameter for the `GetValue` method. This is because lambda expressions are actually nothing more than parameterized variables and we need variables dictionary in order to store them. If you don't specify a valid `IDictionary` instance for the `variables` parameter, you will get a runtime exception.

Also, in both examples above we used an expression list in order to define and invoke a function in a single expression. However, more likely than not, you will want to define your functions once and then use them within as many expressions as you need. Solenoid provides an easy way to pre-register your lambda expressions by exposing a static `Expression.RegisterFunction` method, which takes function name, lambda expression and variables dictionary to register function in as parameters:

``` csharp
IDictionary vars = new Hashtable();
Expression.RegisterFunction("sqrt", "{|n| Math.Sqrt($n)}", vars);
Expression.RegisterFunction("fact", "{|n| $n <= 1 ? 1 : $n * #fact($n-1)}", vars);
```

Once the function registration is done, you can simply evaluate an expression that uses these functions, making sure that the `vars` dictionary is passed as a parameter to expression evaluation engine:

``` csharp
ExpressionEvaluator.GetValue(null, "#fact(5)", vars)  // 120
ExpressionEvaluator.GetValue(null, "#sqrt(9)", vars)  // 3
```

Finally, because lambda expressions are treated as variables, they can be assigned to other variables or passed as parameters to other lambda expressions. In the following example we are defining a delegate function that accepts function `f` as the first argument and parameter `n` that will be passed to function `f` as the second. Then we invoke the functions registered in the previous example, as well as the lambda expression defined inline, through our delegate:

``` csharp
Expression.RegisterFunction("delegate", "{|f, n| $f($n) }", vars);
ExpressionEvaluator.GetValue(null, "#delegate(#sqrt, 4)", vars)  // 2
ExpressionEvaluator.GetValue(null, "#delegate(#fact, 5)", vars)  // 120
ExpressionEvaluator.GetValue(null, "#delegate({|n| $n ^ 2 }, 5)", vars)  // 25
```

While this particular example is not particularly useful, it does demonstrate that lambda expressions are indeed treated as nothing more than parameterized variables, which is important to remember.

Delegate Expressions
--------------------

Delegate expressions allow you to refer to .NET delegates which can then be used within your expressions just like any other function or method.

For example, you can define a max delegate and call it like this

``` csharp
private delegate double DoubleFunctionTwoArgs(double arg1, double arg2);

private double Max(double arg1, double arg2)
{
  return Math.Max(arg1, arg2);
}


public void DoWork()
{
  Hashtable vars = new Hashtable();
  vars["max"] = new DoubleFunctionTwoArgs(Max);
  double result = (double) ExpressionEvaluator.GetValue(null, "#max(5,25)", vars);  // 25
}
```

Null Context
------------

If you do not specify a root object, i.e. pass in null, then the expressions evaluated either have to be literal values, i.e. ExpressionEvaluator.GetValue(null, "2 + 3.14"), refer to classes that have static methods or properties, i.e. ExpressionEvaluator.GetValue(null, "DateTime.Today"), create new instances of objects, i.e. ExpressionEvaluator.GetValue(null, "new DateTime(2004, 8, 14)") or refer to other objects such as those in the variable dictionary or in the IoC container. The latter two usages will be discussed later.

Classes used in the examples
============================

The following simple classes are used to demonstrate the functionality of the expression language.

``` csharp
public class Inventor
{
    public string Name;
    public string Nationality;
    public string[] Inventions;
    private DateTime dob;
    private Place pob;

    public Inventor() : this(null, DateTime.MinValue, null)
    {}

    public Inventor(string name, DateTime dateOfBirth, string nationality)
    {
        this.Name = name;
        this.dob = dateOfBirth;
        this.Nationality = nationality;
        this.pob = new Place();
    }

    public DateTime DOB
    {
        get { return dob; }
        set { dob = value; }
    }

    public Place PlaceOfBirth
    {
        get { return pob; }
    }

    public int GetAge(DateTime on)
    {
        // not very accurate, but it will do the job ;-)
        return on.Year - dob.Year;
    }
}

public class Place
{
    public string City;
    public string Country;
}

public class Society
{
    public string Name;
    public static string Advisors = "advisors";
    public static string President = "president";

    private IList members = new ArrayList();
    private IDictionary officers = new Hashtable();

    public IList Members
    {
        get { return members; }
    }

    public IDictionary Officers
    {
        get { return officers; }
    }

    public bool IsMember(string name)
    {
        bool found = false;
        foreach (Inventor inventor in members)
        {
            if (inventor.Name == name)
            {
                found = true;
                break;
            }
        }
        return found;
    }
}
```

The code listings in this chapter use instances of the data populated with the following information.

``` csharp
Inventor tesla = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
tesla.Inventions = new string[]
    {
        "Telephone repeater", "Rotating magnetic field principle",
        "Polyphase alternating-current system", "Induction motor",
        "Alternating-current power transmission", "Tesla coil transformer",
        "Wireless communication", "Radio", "Fluorescent lights"
    };
tesla.PlaceOfBirth.City = "Smiljan";

Inventor pupin = new Inventor("Mihajlo Pupin", new DateTime(1854, 10, 9), "Serbian");
pupin.Inventions = new string[] {"Long distance telephony & telegraphy", "Secondary X-Ray radiation", "Sonar"};
pupin.PlaceOfBirth.City = "Idvor";
pupin.PlaceOfBirth.Country = "Serbia";

Society ieee = new Society();
ieee.Members.Add(tesla);
ieee.Members.Add(pupin);
ieee.Officers["president"] = pupin;
ieee.Officers["advisors"] = new Inventor[] {tesla, pupin};
```
