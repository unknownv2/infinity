
namespace NoDev.Infinity.Tools
{
    internal interface IToolImageRetriever
    {
        bool HasToolImage(string toolId);

        string RetrieveToolImageLocation(string toolId);
    }
}
