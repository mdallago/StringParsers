namespace StringParsers
{
    public interface IStringParser<T> where T : class,new()
    {
        T Parse(string text);
        string Write(T data);
    }
}