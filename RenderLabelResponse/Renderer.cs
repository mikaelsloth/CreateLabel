namespace RenderExpressConnectResponse
{
    using System;
    using System.IO;
    using System.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Xsl;

#nullable enable

    public abstract class Renderer
    {
        private string savePath = "";

        protected string NamePrefix { get; set; } = "";

        public XsltArgumentList XsltArguments { get; init; }

        public XslCompiledTransform Stylesheet { get; init; }

        public XmlSchemaSet? SchemaSet { get; init; }

        public string SaveOutputPath
        {
            get => savePath;
            set
            {
                try
                {

                    if (value == "")
                    {
                        savePath = value;
                        return;
                    }

                    IoExtensions.NormalizeDirectory(ref value);
                    if (value.ValidateDirectory()) savePath = value;
                }
                catch
                {
                    throw;
                }
            }
        }

        public string SaveOutputFileName { get; set; } = "";

        protected virtual async Task<XDocument> Transform(XDocument xmlresponse, bool validateschema = true, bool saveoutput = true)
        {
            // If schema not valid inform user
            if (SchemaSet != null && validateschema && TryValidateSchema(xmlresponse, SchemaSet, out string message))
                throw new ArgumentException($"The response document to render does not confirm to the selected schema. \r\nThe error returned was : \r\n{message}\r\nPlease ensure you provide a valid response document, or the correct schema to validate against.");

            try
            {
                // Start Rendering by creating the transformation
                XDocument transform = TransformInputXml(xmlresponse, Stylesheet, XsltArguments);

                // Copy the file if specified to save a copy
                if (saveoutput && !string.IsNullOrEmpty(SaveOutputPath) && !string.IsNullOrEmpty(SaveOutputFileName))
                    await SaveOutputToFile(transform, @$"{SaveOutputPath}{SaveOutputFileName}");

                return transform;
            }
            catch (Exception ex)
            {
                throw new Exception($"The creation of a PDF document containing the label did not succeed. \r\nThe error returned was : \r\n{ex.Message}\r\n{ex.InnerException}\r\nPlease report to developer to correct.", ex);
            }
        }

        protected virtual async Task<XDocument> Transform(string responseasstring, bool validateschema = true, bool saveoutput = true)
        {
            // If no input then inform caller
            if (string.IsNullOrWhiteSpace(responseasstring))
                throw new ArgumentException("The response text to render is missing. \r\nPlease input the label response to render.", nameof(responseasstring));

            // If not valid XML then inform caller
            XDocument inputxml;
            try
            {
                inputxml = XDocument.Parse(responseasstring, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"The label response text to render is invalid. \r\nThe error returned was : \r\n{ex.Message}\r\nPlease correct the errors and try again.", nameof(responseasstring), ex);
            }

            return await Transform(inputxml, validateschema, saveoutput);
        }

        protected Renderer(XslCompiledTransform stylesheet, XsltArgumentList? xsltarguments, XmlSchemaSet? schemaset)
        {
            Stylesheet = stylesheet;
            XsltArguments = xsltarguments ?? (new());
            SchemaSet = schemaset;            
        }

        protected static string GenerateFileName() => DateTime.Now.Ticks.ToString();

        private async static Task SaveOutputToFile(XDocument transform, string filename)
        {
            await using FileStream fileStream = File.Open(filename, FileMode.Create);
            await transform.SaveAsync(fileStream, SaveOptions.None, CancellationToken.None);
        }

        private static XDocument TransformInputXml(XDocument inputxml, XslCompiledTransform stylesheet, XsltArgumentList? args)
        {
            XDocument newDocument = new();

            try
            {
                using XmlReader oldDocumentReader = inputxml.CreateReader();
                using XmlWriter newDocumentWriter = newDocument.CreateWriter();
                stylesheet.Transform(oldDocumentReader, args, newDocumentWriter);
            }
            catch (Exception)
            {
                throw;
            }

            return newDocument;
        }

        private static bool TryValidateSchema(XDocument inputxml, XmlSchemaSet schemas, out string message)
        {
            bool errors = false;
            string tempmsg = "The following messages came from validating against the schema: \r\n";

            inputxml.Validate(schemas, (o, e) =>
            {
                switch (e.Severity)
                {
                    case XmlSeverityType.Error:
                        tempmsg += $"\r\nERROR {e.Message ?? ""}";
                        errors = true;
                        break;
                    case XmlSeverityType.Warning:
                        tempmsg += $"\r\nWARNING: {e.Message}";
                        break;
                }
            });

            message = tempmsg;
            return errors;
        }
    }
}
