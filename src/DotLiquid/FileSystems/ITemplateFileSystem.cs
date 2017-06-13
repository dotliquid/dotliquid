namespace DotLiquid.FileSystems
{
    /// <summary>
    /// This interface allow you return a Template instance,
    /// it can reduce the template parsing time in some cases.
    /// Please also provide the implementation of ReadTemplateFile for fallback purpose.
    /// </summary>
    public interface ITemplateFileSystem : IFileSystem
    {
        /// <summary>
        /// Called by Liquid to retrieve a template instance
        /// </summary>
        /// <param name="templatePath"></param>
        /// <returns></returns>
        Template GetTemplate(Context context, string templateName);
    }
}
