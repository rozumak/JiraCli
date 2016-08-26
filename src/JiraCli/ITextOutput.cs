namespace JiraCli
{
    public interface ITextOutput
    {
        void Write(string text);

        void WriteLine();

        void WriteLine(string text);
    }
}