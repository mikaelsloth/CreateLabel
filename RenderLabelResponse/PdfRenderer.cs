namespace RenderExpressConnectResponse
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Xsl;

#nullable enable

    public abstract class PdfRenderer : Renderer
    {
        private static readonly HashSet<string> fopPathsChecked = new();
        private static readonly string tempPath = Path.GetTempPath();

        private string savePath = "";

        //private const string BARCODEURL = "https://express.tnt.com/barbecue/barcode?type=Code128C&height=140&width=2&data=";
        //private const string IMAGEURL = "https://express.tnt.com/expresswebservices-website/rendering/images";

        private string fopPath = "";

        public string FopPath
        {
            get => fopPath;
            set
            {
                try
                {
                    IoExtensions.NormalizeDirectory(ref value);
                    if (ValidateFopDirectory(value)) fopPath = value;
                }
                catch
                {
                    throw;
                }
            }
        }

        public string SavePdfPath
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

        public string SavePdfFileName { get; set; } = "";

        private const string FoNameSuffix = ".txt";
        private const string PdfNameSuffix = ".pdf";

        protected PdfRenderer(XslCompiledTransform stylesheet, XsltArgumentList? xsltarguments, XmlSchemaSet? schemaset) : base(stylesheet, xsltarguments, schemaset)
        {
        }

        private static async Task CreatePdfFile(string fop_directory, string outputpath, string fo_filename, string pdf_filename)
        {
            // see https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.standarderror?view=net-6.0
            try
            {
                string result = await ProcessWrapper.GetCommandLineProcessResultAsync($"{fop_directory}fop.bat", fop_directory, @$"-c ""{fop_directory}conf\fop.xconf"" -fo ""{outputpath}{fo_filename}"" -pdf ""{outputpath}{pdf_filename}""");
                return;
            }
            catch
            {
                throw;
            }
        }

        public virtual async Task<string> CreatePdf(string xmlresponseastext, Func<XDocument, bool>? errorcheck, bool validateschema = true, bool savepdf = true)
        {
            // If no input then inform caller
            if (string.IsNullOrWhiteSpace(xmlresponseastext))
                throw new ArgumentException("The xml response text to render is missing. \r\nPlease input the label response to render.", nameof(xmlresponseastext));

            // If not valid XML then inform caller
            XDocument inputxml;
            try
            {
                inputxml = XDocument.Parse(xmlresponseastext, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"The xml response text to render is invalid. \r\nThe error returned was : \r\n{ex.Message}\r\nPlease correct the errors and try again.", nameof(xmlresponseastext), ex);
            }

            return await CreatePdf(inputxml, errorcheck, validateschema, savepdf);
        }

        public virtual async Task<string> CreatePdf(XDocument xmlresponse, Func<XDocument, bool>? errorcheck, bool validateschema = true, bool savepdf = true)
        {
            // If the FOP framework is not installed inform user
            if (string.IsNullOrWhiteSpace(FopPath))
                throw new InvalidOperationException("The FOP Framework information is missing. \r\nPlease correct this before proceeding.");

            // If output paths for fo result file does not exists use hardcoded ones
            bool cleanup = false;
            string backedupfilename = "";
            string uniquename = GenerateFileName();
            string fo_filename = NamePrefix + uniquename + FoNameSuffix;

            if (string.IsNullOrWhiteSpace(SaveOutputPath))
            {
                backedupfilename = SaveOutputFileName;
                SaveOutputPath = tempPath;
                SaveOutputFileName = fo_filename;
                cleanup = true;
            }

            // Check this is not an error documenterrors
            if (errorcheck != null && errorcheck(xmlresponse))
                throw new ArgumentException($"\r\nResponse seems to be an error response, which cannot be rendered:\r\n{xmlresponse}", nameof(xmlresponse));

            // -------------------------------
            try
            {
                // Start Rendering by creating the label.fo
                XDocument transform = await Transform(xmlresponse, validateschema, true);

                // The great moment: call the FOP environment to create a PDF file
                string pdf_filename = NamePrefix + uniquename + PdfNameSuffix;
                await CreatePdfFile(FopPath, SaveOutputPath, fo_filename, pdf_filename);

                // Get the Base64 copy of the PDF
                byte[] bytes = await File.ReadAllBytesAsync(@$"{tempPath}{pdf_filename}");
                string pdf = Convert.ToBase64String(bytes);

                // Copy the file if specified to save a copy
                if (savepdf && !string.IsNullOrEmpty(SavePdfPath))
                    await File.WriteAllBytesAsync(@$"{SavePdfPath}{(string.IsNullOrEmpty(SavePdfFileName) ? pdf_filename : SavePdfFileName)}", bytes);

                // Finally, let's clean up
                File.Delete(@$"{SaveOutputPath}{fo_filename}");
                if (cleanup)
                {
                    SaveOutputFileName = backedupfilename;
                    SaveOutputPath = "";
                }

                File.Delete(@$"{tempPath}{pdf_filename}");

                return pdf;
            }
            catch (Exception ex)
            {
                throw new Exception($"The creation of a PDF file containing the rendered document did not succeed. \r\nThe error returned was : \r\n{ex.Message}\r\n{ex.InnerException}\r\nPlease report to developer to correct.", ex);
            }
        }

        protected virtual bool ValidateFopDirectory(string path)
        {
            if (!path.EndsWith('\\')) path += '\\';
            if (fopPathsChecked.Contains(path)) return true;

            try
            {
                if (!path.ValidateDirectory(false))
                    throw new ArgumentException("The path is not a valid path", nameof(path));
                // Check Java is installed
                string javaresult = ProcessWrapper.GetCommandLineProcessResult("java", "", "-version");
                // Get FOP version
                string fopresult = ProcessWrapper.GetCommandLineProcessResult($"{path}fop.bat", path, "-version");
            }
            catch
            {
                throw;
            }

            _ = fopPathsChecked.Add(path);
            return true;
        }
    }
}
