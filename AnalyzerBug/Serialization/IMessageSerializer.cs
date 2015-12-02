
namespace AnalyzerBug.Serialization
{
    public interface IMessageSerializer
    {
        bool TrySerialize<T>(T item, out byte[] bytes);
        bool TryDeserialize<T>(byte[] source, out T item);
    }
}
