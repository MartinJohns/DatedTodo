namespace DatedTodo
{
    internal struct TextPosition
    {
        public TextPosition(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public int Line { get; set; }

        public int Column { get; set; }
    }
}
