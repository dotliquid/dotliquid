namespace DotLiquid.FileSystems
{
	/// <summary>
	/// A Liquid file system is way to let your templates retrieve other templates for use with the include tag.
	/// 
	/// You can implement subclasses that retrieve templates from the database, from the file system using a different
	/// path structure, you can provide them as hard-coded inline strings, or any manner that you see fit.
	/// 
	/// You can add additional instance variables, arguments, or methods as needed.
	/// 
	/// Example:
	/// 
	/// Liquid::Template.file_system = Liquid::LocalFileSystem.new(template_path)
	/// liquid = Liquid::Template.parse(template)
	/// 
	/// This will parse the template with a LocalFileSystem implementation rooted at 'template_path'.
	/// </summary>
	public interface IFileSystem
	{
		/// <summary>
		/// Called by Liquid to retrieve a template or template file.
		/// </summary>
        /// <param name="templateName">The name of the requested template.</param>
		/// <returns>Either a string or a Template. If something else is returned, ToString() will be
		/// used and the result will be parsed as a new template.</returns>
		object ReadTemplateFile(Context context, string templateName);
	}
}