namespace DotLiquid
{
    public interface ITagFactory
    {
        string TagName { get; }

        Tag Create();
    }
}
