namespace CreateLabel
{
    using RenderExpressConnectResponse;
    using SendLabelRequest;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Xsl;

    public partial class Form1 : Form
    {
        const string NamePrefix = "label";
        const string PdfNameSuffix = ".pdf";
        const string FopDirectory = @"C:\Fop\";

        public Form1()
        {
            InitializeComponent();
            Stylesheet = "";
            StylesheetPath = "";
            Schema = "";
            SchemaPath = "";
            OutputPath = "";
            InputSchema = "";
            InputSchemaPath = "";
            InputMode = InputModeEnum.Request;
        }

        internal string Stylesheet { get; set; }
        internal string StylesheetPath { get; set; }
        internal string Schema { get; set; }
        internal string SchemaPath { get; set; }
        internal string InputSchema { get; set; }
        internal string InputSchemaPath { get; set; }
        internal bool ValidateSchema { get; set; }
        internal bool ValidateInputSchema { get; set; }
        internal string OutputPath { get; set; }
        internal InputModeEnum InputMode { get; set; }
        internal XslCompiledTransform? CompiledStylesheet { get; set; }
        internal XmlSchemaSet? SchemaSet { get; set; }
        internal XmlSchemaSet? InputSchemaSet { get; set; }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Check if a previous used stylesheet is saved
            string stylesheet = Properties.Settings.Default.StyleSheet;
            if (!string.IsNullOrWhiteSpace(stylesheet))
            {
                if (File.Exists(stylesheet))
                {
                    Stylesheet = stylesheet;
                    // Load the stylesheet
                    try
                    {
                        CompiledStylesheet = LoadStylesheet(stylesheet);
                    }
                    catch
                    {
                        Stylesheet = string.Empty;
                    }
                }
            }

            textBox2.Text = Stylesheet;

            // Check if a previous used stylesheet path is saved
            string stylesheetpath = Properties.Settings.Default.StyleSheetDirectory;
            if (!string.IsNullOrWhiteSpace(stylesheetpath))
            {
                if (Directory.Exists(stylesheetpath)) StylesheetPath = stylesheetpath;
            }

            // Check if a previous used schema is saved
            string schema = Properties.Settings.Default.Schema;
            if (!string.IsNullOrWhiteSpace(schema))
            {
                if (File.Exists(schema)) Schema = schema;
                // Load the schema
                try
                {
                    SchemaSet = LoadSchema(schema);
                }
                catch
                {
                    Schema = string.Empty;
                }
            }

            textBox3.Text = Schema;

            // Check if a previous used schema path is saved
            string schemapath = Properties.Settings.Default.SchemaDirectory;
            if (!string.IsNullOrWhiteSpace(schemapath))
            {
                if (Directory.Exists(schemapath)) SchemaPath = schemapath;
            }

            ValidateSchema = textBox3.Visible = button3.Visible = checkBox1.Checked = Properties.Settings.Default.ValidateSchema && !string.IsNullOrEmpty(Schema);

            // Check if a previous used output path is saved
            string outputpath = Properties.Settings.Default.OutputDirectory;
            if (!string.IsNullOrWhiteSpace(outputpath))
            {
                if (Directory.Exists(outputpath)) OutputPath = outputpath;
            }

            textBox4.Text = OutputPath;

            // Check if a previous used input schema is saved
            string inputschema = Properties.Settings.Default.InputSchema;
            if (!string.IsNullOrWhiteSpace(inputschema))
            {
                if (File.Exists(inputschema)) InputSchema = inputschema;
            }

            textBox5.Text = InputSchema;

            // Check if a previous used schema path is saved
            string inputschemapath = Properties.Settings.Default.InputSchemaPath;
            if (!string.IsNullOrWhiteSpace(inputschemapath))
            {
                if (Directory.Exists(inputschemapath))
                {
                    InputSchemaPath = inputschemapath;
                    // Load the schema
                    try
                    {
                        InputSchemaSet = LoadSchema(schema);
                    }
                    catch
                    {
                        InputSchema = string.Empty;
                    }
                }
            }

            ValidateInputSchema = textBox5.Visible = button5.Visible = checkBox2.Checked = Properties.Settings.Default.ValidateInputSchema && !string.IsNullOrEmpty(InputSchema);
        }

        private void BrowseStylesheet(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(StylesheetPath))
            {
                OpenFileDialog.InitialDirectory = StylesheetPath;
            }

            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stylesheet = textBox2.Text = OpenFileDialog.FileName;
                SaveStylesheetInfo(Stylesheet);
            }
        }

        private void SaveStylesheetInfo(string stylesheet)
        {
            try
            {
                CompiledStylesheet = LoadStylesheet(stylesheet);
                string? stylesheetpath = Path.GetDirectoryName(stylesheet);
                Properties.Settings.Default.StyleSheetDirectory = stylesheetpath;
                Properties.Settings.Default.StyleSheet = stylesheet;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Loading the specified stylesheet did not succeed. \r\nThe error returned was : \r\n{ex.Message}\r\n{ex.InnerException}\r\n");
            }
        }

        private async void CreatePdf(object sender, EventArgs e)
        {
            if (CompiledStylesheet == null)
            {
                _ = MessageBox.Show($"To create a PDF document containing the label, you must provide a stylesheet designed for the purpose. \r\nPlease input the needed information and try again.");
                return;
            }

            try
            {
                string response = string.Empty;
                string result = string.Empty;

                //TODO Update design to support xslt params by user input 
                //TODO Update design to support user input
                PdfRenderer pdf = new ExpressLabelPdfRenderer(CompiledStylesheet, null, ValidateSchema ? SchemaSet : null)
                {
                    SavePdfFileName = $@"{ NamePrefix}{ GenerateName()}{ PdfNameSuffix}",
                    SavePdfPath = OutputPath,
                    FopPath = FopDirectory
                }; 

            switch (InputMode)
                {
                    case InputModeEnum.Request:
                        ExpressLabelRequest request = new();
                        request.CustomerId = textBox6.Text;
                        request.Password = textBox7.Text;
                        XDocument answer = await request.SubmitRequestAsync(textBox1.Text);
                        if (request.IsErrorResponse(answer))
                            throw new Exception($"The web service returned an error response. \r\n{answer}");
                        result = await pdf.CreatePdf(answer, null, ValidateSchema, !string.IsNullOrWhiteSpace(OutputPath));
                        break;
                    case InputModeEnum.Response:
                        response = textBox1.Text;
                        result = await pdf.CreatePdf(response, null, ValidateSchema, !string.IsNullOrWhiteSpace(OutputPath));
                        break;
                    default:
                        break;
                }

                if (result != string.Empty)
                {
                    string pdf_filename = $@"{Path.GetTempPath()}\{NamePrefix}{GenerateName()}{PdfNameSuffix}";
                    byte[] bytes = Convert.FromBase64String(result);
                    File.WriteAllBytes(pdf_filename, bytes);
                    ShowPdfFileInReader(pdf_filename);
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"The creation of a PDF document containing the label did not succeed. \r\nThe error returned was : \r\n{ex.Message}\r\n{ex.InnerException}\r\n");
            }
        }

        private static void ShowPdfFileInReader(string fullFilePath)
        {
            Process reader = new();
            reader.StartInfo.UseShellExecute = true;
            reader.StartInfo.FileName = fullFilePath;
            _ = reader.Start();
        }

        private static string GenerateName() => DateTime.Now.Ticks.ToString();

        private static XslCompiledTransform LoadStylesheet(string stylesheet)
        {
            XmlReaderSettings readerSettings = new() { XmlResolver = new XmlUrlResolver() };
            XsltSettings xsltSettings = new() { EnableDocumentFunction = true, EnableScript = true };
            try
            {
                using XmlReader xsltReader = XmlReader.Create(stylesheet, readerSettings);
                XslCompiledTransform transformer = new();
                transformer.Load(xsltReader, xsltSettings, new XmlUrlResolver());
                return transformer;
            }
            catch
            {
                throw;
            }
        }

        private static XmlSchemaSet LoadSchema(string fullPathOfSchemaFile)
        {
            string tempmsg = string.Empty;

            XmlSchema? myschema = XmlSchema.Read(XmlReader.Create(fullPathOfSchemaFile), (o, e) =>
            {
                tempmsg = "The following messages came from reading the schema: \r\n";
                if (e.Severity == XmlSeverityType.Warning)
                    tempmsg = tempmsg + "\r\n" + "WARNING: " + e.Message;
                else if (e.Severity == XmlSeverityType.Error)
                    tempmsg = tempmsg + "\r\n" + "ERROR: " + e.Message;
            });

            if (myschema == null)
                throw new XmlException($"The XML schema could not be read. The following errors were encountered:\r\n{ tempmsg }");

            try
            {
                XmlSchemaSet schemas = new();
                schemas.XmlResolver = new XmlUrlResolver();
                _ = schemas.Add(myschema);
                return schemas;
            }
            catch
            {
                throw;
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            ValidateSchema = Properties.Settings.Default.ValidateSchema = textBox3.Visible = button3.Visible = checkBox1.Checked;
            Properties.Settings.Default.Save();
        }

        private void BrowseSchema(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SchemaPath))
            {
                OpenFileDialog.InitialDirectory = SchemaPath;
            }

            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                Schema = textBox3.Text = OpenFileDialog.FileName;
                SaveSchemaInfo(Schema);
            }
        }

        private void SaveSchemaInfo(string schema)
        {
            try
            {
                SchemaSet = LoadSchema(schema);
                string? schemapath = Path.GetDirectoryName(schema);
                Properties.Settings.Default.SchemaDirectory = schemapath;
                Properties.Settings.Default.Schema = schema;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Loading the specified schema did not succeed. \r\nThe error returned was : \r\n{ex.Message}\r\n{ex.InnerException}\r\n");
            }
        }

        private void BrowseOutputFolder(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(OutputPath))
            {
                FolderBrowserDialog.SelectedPath = SchemaPath;
            }

            if (FolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                OutputPath = textBox4.Text = FolderBrowserDialog.SelectedPath;
                SaveFolderInfo(OutputPath);
            }
        }

        private static void SaveFolderInfo(string folder)
        {
            Properties.Settings.Default.OutputDirectory = folder;
            Properties.Settings.Default.Save();
        }

        private void InputRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton? choice = groupBox1.Controls.OfType<RadioButton>().FirstOrDefault(x => x.Checked);
            InputMode = Enum.TryParse(choice == null ? RequestRadioButton.Tag.ToString() : choice.Tag.ToString(), true, out InputModeEnum result) ? result : InputModeEnum.Request;
            switch (InputMode)
            {
                case InputModeEnum.Request:
                    textBox7.Visible = textBox6.Visible = label4.Visible = label5.Visible = true;
                    break;
                case InputModeEnum.Response:
                    textBox7.Visible = textBox6.Visible = label4.Visible = label5.Visible = false;
                    break;
                default:
                    break;
            }
        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            ValidateInputSchema = Properties.Settings.Default.ValidateInputSchema = textBox5.Visible = button5.Visible = checkBox2.Checked;
            Properties.Settings.Default.Save();
        }

        private void BrowseInputSchema(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputSchemaPath))
            {
                OpenFileDialog.InitialDirectory = InputSchemaPath;
            }

            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                InputSchema = textBox5.Text = OpenFileDialog.FileName;
                SaveInputSchemaInfo(InputSchema);
            }
        }

        private void SaveInputSchemaInfo(string schema)
        {
            try
            {
                InputSchemaSet = LoadSchema(schema);
                string? schemapath = Path.GetDirectoryName(schema);
            Properties.Settings.Default.InputSchemaPath = schemapath;
            Properties.Settings.Default.InputSchema = schema;
            Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Loading the specified schema did not succeed. \r\nThe error returned was : \r\n{ex.Message}\r\n{ex.InnerException}\r\n");
            }
        }
    }

    internal enum InputModeEnum { Request, Response }
}
