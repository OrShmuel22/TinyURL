namespace TinyURL.Core.Interfaces
{
    public interface IBase62Encoding
    {
        string Encode(long num);
        long Decode(string encoded);
    }
}
