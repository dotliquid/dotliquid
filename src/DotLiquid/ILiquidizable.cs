namespace DotLiquid
{
    /// <summary>
    /// See here for motivation: <see href="https://github.com/Shopify/liquid/wiki/Using-Liquid-without-Rails/cadb5d13cb171d36b4aa9239af1b8b70bc699ad1"/>.
    /// This allows for extra security by only giving the template access to the specific
    /// variables you want it to have access to.
    /// </summary>
    public interface ILiquidizable
    {
        object ToLiquid();
    }
}
