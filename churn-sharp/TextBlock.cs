using System.Collections.Generic;

namespace churn_sharp
{
    /// <summary>
    ///   Text block.
    /// </summary>
    public class TextBlock
    {
        /// <summary>
        ///   Text to be applied after the block.
        /// </summary>
        private string _postText;

        /// <summary>
        ///   Textual content.
        /// </summary>
        private string _text;

        /// <summary>
        ///   Type of block.
        /// </summary>
        private BlockType _type;

        /// <summary>
        ///   Inner blocks.
        /// </summary>
        private ICollection<TextBlock> _innerBlocks;

        /// <summary>
        ///   Gets the inner blocks.
        /// </summary>
        public ICollection<TextBlock> InnerBlocks
        {
            get { return this._innerBlocks; }
        }

        /// <summary>
        ///   Gets or sets the post text.
        /// </summary>
        /// <value>
        ///   The post text.
        /// </value>
        public string PostText
        {
            get { return this._postText; }
            set { this._postText = value; }
        }

        /// <summary>
        ///   Gets or sets the text.
        /// </summary>
        /// <value>
        ///   The text.
        /// </value>
        public string Text
        {
            get { return this._text; }
            set { this._text = value; }
        }

        /// <summary>
        ///   Gets or sets the type.
        /// </summary>
        /// <value>
        ///   The type.
        /// </value>
        public BlockType Type
        {
            get { return this._type; }
            set { this._type = value; }
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="TextBlock"/> class.
        /// </summary>
        /// <param name="Text">The text.</param>
        /// <param name="Type">The type.</param>
        /// <param name="PostText">The post text.</param>
        public TextBlock(string Text, BlockType Type = BlockType.Plain, string PostText = "")
        {
            this._innerBlocks = new List<TextBlock>();
            this._text = Text;
            this._type = Type;
            this._postText = PostText;
        }
    }
}
