using System.IO;
using System.Text;

namespace JiraCli
{
    public class CompositTextWriter : TextWriter
    {
        private readonly TextWriter[] _textWriters;

        public CompositTextWriter(params TextWriter[] textWriters)
        {
            _textWriters = textWriters;
        }

        public override Encoding Encoding => Encoding.Unicode;

        protected override void Dispose(bool disposing)
        {
            foreach (var textWriter in _textWriters)
            {
                textWriter.Dispose();
            }
        }

        public override void Close()
        {
            Dispose(true);
        }

        public override void Flush()
        {
            foreach (var textWriter in _textWriters)
            {
                textWriter.Flush();
            }
        }

        public override void Write(char value)
        {
            foreach (var textWriter in _textWriters)
            {
                textWriter.Write(value);
            }
        }
    }
}