using System.IO;

namespace JiraCli
{
    public class PlainTextOutput : ITextOutput
    {
        private readonly TextWriter _writer;

        public PlainTextOutput(TextWriter writer)
        {
            _writer = writer;
        }

        public void Write(string text)
        {
            _writer.Write(text);
        }

        public void WriteLine()
        {
            _writer.WriteLine();
        }

        public void WriteLine(string text)
        {
            _writer.WriteLine(text);
        }
    }
}