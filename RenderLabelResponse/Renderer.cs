namespace RenderLabelResponse
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Xsl;

#nullable enable

    public class Renderer
    {
        private const string BARCODEURL = "https://express.tnt.com/barbecue/barcode?type=Code128C&height=140&width=2&data=";
        private const string IMAGEURL = "https://express.tnt.com/expresswebservices-website/rendering/images";

        private const string NamePrefix = "label";
        private const string FoNameSuffix = ".txt";
        private const string PdfNameSuffix = ".pdf";

        public static string CreatePdf(string labelResponseAsText, XslCompiledTransform? foStylesheet, string fopFrameworkPath)
        {
            try
            {
                return CreatePdfImpl(labelResponseAsText, foStylesheet, fopFrameworkPath);
            }
            catch
            {
                throw;
            }
        }

        public static string CreatePdf(string labelResponseAsText, XslCompiledTransform? foStylesheet, string fopFrameworkPath, string saveOutputPath)
        {
            try
            {
                // If no output folder is defined inform caller
                ValidateOutputDirectory(saveOutputPath);
                return CreatePdfImpl(labelResponseAsText, foStylesheet, fopFrameworkPath, saveOutputPath);
            }
            catch
            {
                throw;
            }
        }

        public static string CreatePdf(string labelResponseAsText, XslCompiledTransform? foStylesheet, string fopFrameworkPath, XmlSchemaSet schemaSet)
        {
            try
            {
                return CreatePdfImpl(labelResponseAsText, foStylesheet, fopFrameworkPath, null, schemaSet);
            }
            catch
            {
                throw;
            }
        }

        public static string CreatePdf(string labelResponseAsText, XslCompiledTransform? foStylesheet, string fopFrameworkPath, string saveOutputPath, XmlSchemaSet schemaSet)
        {
            try
            {
                // If no output folder is defined inform caller
                ValidateOutputDirectory(saveOutputPath);
                return CreatePdfImpl(labelResponseAsText, foStylesheet, fopFrameworkPath, saveOutputPath, schemaSet);
            }
            catch
            {
                throw;
            }
        }

        public static string CreatePdf(XDocument labelResponse, XslCompiledTransform? foStylesheet, string fopFrameworkPath)
        {
            try
            {
                return CreatePdfImpl(labelResponse, foStylesheet, fopFrameworkPath);
            }
            catch
            {
                throw;
            }
        }

        public static string CreatePdf(XDocument labelResponse, XslCompiledTransform? foStylesheet, string fopFrameworkPath, string saveOutputPath)
        {
            try
            {
                // If no output folder is defined inform caller
                ValidateOutputDirectory(saveOutputPath);
                return CreatePdfImpl(labelResponse, foStylesheet, fopFrameworkPath, saveOutputPath);
            }
            catch
            {
                throw;
            }
        }

        public static string CreatePdf(XDocument labelResponse, XslCompiledTransform? foStylesheet, string fopFrameworkPath, XmlSchemaSet schemaSet)
        {
            try
            {
                return CreatePdfImpl(labelResponse, foStylesheet, fopFrameworkPath, null, schemaSet);
            }
            catch
            {
                throw;
            }
        }

        public static string CreatePdf(XDocument labelResponse, XslCompiledTransform? foStylesheet, string fopFrameworkPath, string saveOutputPath, XmlSchemaSet schemaSet)
        {
            try
            {
                // If no output folder is defined inform caller
                ValidateOutputDirectory(saveOutputPath);
                return CreatePdfImpl(labelResponse, foStylesheet, fopFrameworkPath, saveOutputPath, schemaSet);
            }
            catch
            {
                throw;
            }
        }

        private static string CreatePdfImpl(string labelResponseAsText, XslCompiledTransform? foStylesheet, string fopFrameworkPath, string? saveOutputPath = null, XmlSchemaSet? schemaSet = null)
        {
            // If no input then inform caller
            if (string.IsNullOrWhiteSpace(labelResponseAsText))
                throw new ArgumentException("The label response text to render is missing. \r\nPlease input the label response to render.", nameof(labelResponseAsText));

            // If not valid XML then inform caller
            XDocument inputxml;
            try
            {
                inputxml = XDocument.Parse(labelResponseAsText, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"The label response text to render is invalid. \r\nThe error returned was : \r\n{ex.Message}\r\nPlease correct the errors and try again.", nameof(labelResponseAsText), ex);
            }

            return CreatePdfImpl(inputxml, foStylesheet, fopFrameworkPath, saveOutputPath, schemaSet);
        }

        private static string CreatePdfImpl(XDocument labelResponse, XslCompiledTransform? foStylesheet, string fopFrameworkPath, string? saveOutputPath = null, XmlSchemaSet? schemaSet = null)
        {
            // If no stylesheet is defined inform caller
            if (foStylesheet == null)
                throw new ArgumentNullException(nameof(foStylesheet), "The FO stylesheet information is missing. \r\nPlease correct this before proceeding.");

            // If the FOP framework is not installed inform user
            if (string.IsNullOrWhiteSpace(fopFrameworkPath))
                throw new ArgumentException("The FOP Framework information is missing. \r\nPlease correct this before proceeding.", nameof(fopFrameworkPath));

            if (!Directory.Exists(fopFrameworkPath))
                throw new ArgumentException("The FOP Framework directory does not exist or cannot be accessed. \r\nPlease select an existing directory before proceeding.", nameof(fopFrameworkPath));

            // Check if this is error response as this otherwise generates errors in FOP framework
            int errorcount = 0;
            foreach (var item in labelResponse.Descendants("brokenRules"))
            {
                errorcount++;
                break;
            }

            foreach (var item in labelResponse.Descendants("fault"))
            {
                errorcount++;
                break;
            }

            if (errorcount > 0)
                throw new ArgumentException($"\r\nResponse seems to be an error response, which cannot be rendered:\r\n{labelResponse}", nameof(labelResponse));

            // If schema not valid inform user
            if (schemaSet != null && TryValidateSchema(labelResponse, schemaSet, out string message))
                throw new ArgumentException($"The label response text to render is not confirming to the ExpressLabel schema. \r\nThe error returned was : \r\n{message}\r\nPlease ensure you provide a valid ExpressLabel response.");

            // -------------------------------
            try
            {
                // Start Rendering by creating the label.fo
                XDocument transform = TransformInputXml(labelResponse, foStylesheet);

                // Now save the label.fo to file
                string tempPath = Path.GetTempPath();
                string uniquename = GenerateName();
                string fo_filename = NamePrefix + uniquename + FoNameSuffix;
                SaveFoFile(transform, @$"{tempPath}{fo_filename}");

                // The great moment: call the FOP environment to create a PDF file
                string pdf_filename = NamePrefix + uniquename + PdfNameSuffix;
                CreatePdfFile(fopFrameworkPath, tempPath, fo_filename, pdf_filename);

                // Get the Base64 copy of the PDF
                byte[] bytes = File.ReadAllBytes(@$"{tempPath}{pdf_filename}");
                string pdf = Convert.ToBase64String(bytes);

                // Copy the file if specified to save a copy
                if (!string.IsNullOrEmpty(saveOutputPath))
                    File.Copy(@$"{tempPath}{pdf_filename}", @$"{saveOutputPath}\{pdf_filename}");

                // Finally, let's clean up
                File.Delete(@$"{tempPath}{fo_filename}");
                File.Delete(@$"{tempPath}{pdf_filename}");

                return pdf;
            }
            catch (Exception ex)
            {
                throw new Exception($"The creation of a PDF document containing the label did not succeed. \r\nThe error returned was : \r\n{ex.Message}\r\n{ex.InnerException}\r\nPlease report to developer to correct.", ex);
            }
        }

        private static void CreatePdfFile(string fop_directory, string outputPath, string fo_filename, string pdf_filename)
        {
            Process process = new();
            ProcessStartInfo startInfo = new();
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.FileName = $"{fop_directory}fop.bat";
            startInfo.Arguments = @$"-c ""{fop_directory}conf\fop.xconf"" -fo ""{outputPath}{fo_filename}"" -pdf ""{outputPath}{pdf_filename}""";
            startInfo.WorkingDirectory = fop_directory;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            _ = process.Start();
            process.WaitForExit();
        }

        private static string GenerateName() => DateTime.Now.Ticks.ToString();

        private static void SaveFoFile(XDocument transform, string filename) => transform.Save(filename);

        private static XDocument TransformInputXml(XDocument inputxml, XslCompiledTransform stylesheet)
        {
            XDocument? newDocument = new();

            XsltArgumentList args = new();
            args.AddParam("barcode_url", "", BARCODEURL);
            args.AddParam("images_dir", "", IMAGEURL);
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
            string tempmsg = string.Empty;
            inputxml.Validate(schemas, (o, e) =>
            {
                tempmsg = "The following messages came from validating against the schema: \r\n";
                switch (e.Severity)
                {
                    case XmlSeverityType.Error:
                        tempmsg += $"\r\nERROR {e.Message}";
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

        private static void ValidateOutputDirectory(string saveOutputPath)
        {
            if (string.IsNullOrWhiteSpace(saveOutputPath))
                throw new ArgumentException("The output directory is missing. \r\nPlease select this before proceeding.", nameof(saveOutputPath));

            if (!Directory.Exists(saveOutputPath))
                throw new ArgumentException("The output directory does not exist or cannot be accessed. \r\nPlease select an existing directory before proceeding.", nameof(saveOutputPath));
        }
    }
}
