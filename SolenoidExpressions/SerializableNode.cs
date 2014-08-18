using System;
using System.Runtime.Serialization;
using Solenoid.Expressions.Parser.antlr;
using Solenoid.Expressions.Parser.antlr.collections;

namespace Solenoid.Expressions
{
    /// <summary>
    /// For internal purposes only. Use <see cref="BaseNode"/> for expression node implementations.
    /// </summary>
    /// <remarks>
    /// This class is only required to enable serialization of parsed Spring expressions since antlr.CommonAST
    /// unfortunately is not marked as [Serializable].<br/>
    /// <br/>
    /// <b>Note:</b>Since SerializableNode implements <see cref="ISerializable"/>, deriving classes 
    /// have to explicitely override <see cref="GetObjectData"/> if they need to persist additional
    /// data during serialization.
    /// </remarks>
    [Serializable]
    public class SerializableNode : BaseAST, ISerializable
    {
	    internal class SerializableNodeCreator : ASTNodeCreator
        {
            public override AST Create()
            {
                return new SerializableNode();
            }

            public override string ASTNodeTypeName
            {
                get { return typeof(SerializableNode).FullName; }
            }
        }

        /// <summary>
        /// The global SerializableNode node factory
        /// </summary>
        internal static readonly SerializableNodeCreator Creator = new SerializableNodeCreator();

	    private string _text;
        private int _ttype;

	    /// <summary>
        /// Create an instance
        /// </summary>
        public SerializableNode()
        {}

        /// <summary>
        /// Create an instance from a token
        /// </summary>
        public SerializableNode(IToken token)
        {
            initialize(token);
        }

        /// <summary>
        /// initialize this instance from an AST
        /// </summary>
        public override void initialize(AST t)
        {
            setText(t.getText());
            Type = t.Type;
        }

        /// <summary>
        /// initialize this instance from an IToken
        /// </summary>
        public override void initialize(IToken tok)
        {
            setText(tok.getText());
            Type = tok.Type;
        }

        /// <summary>
        /// initialize this instance from a token type number and a text
        /// </summary>
        public override void initialize(int t, string txt)
        {
            Type = t;
            setText(txt);
        }

        /// <summary>
        /// gets or sets the token type of this node
        /// </summary>
        public override int Type
        {
            get { return _ttype; }
            set { _ttype = value; }
        }

        /// <summary>
        /// gets or sets the text of this node
        /// </summary>
        public string Text
        {
            get { return getText(); }
            set { setText(value); }
        }

        /// <summary>
        /// sets the text of this node
        /// </summary>
        public override void setText(string txt)
        {
            _text = txt;
        }

        /// <summary>
        /// gets the text of this node
        /// </summary>
        public override string getText()
        {
            return _text;
        }

	    /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected SerializableNode(SerializationInfo info, StreamingContext context)
        {
            down = (BaseAST)info.GetValue("down", typeof(BaseAST));
            right = (BaseAST)info.GetValue("right", typeof(BaseAST));
            _ttype = info.GetInt32("ttype");
            _text = info.GetString("text");
        }

        /// <summary>
        /// populate SerializationInfo from this instance
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("down", down, typeof(SerializableNode));
            info.AddValue("right", right, typeof(SerializableNode));
            info.AddValue("ttype", Type, typeof(int));
            info.AddValue("text", Text, typeof(string));
        }
    }
}