namespace RenderExpressConnectResponse
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Xsl;

#nullable enable

    public class ExpressLabelPdfRenderer : PdfRenderer
    {
        private static readonly IEnumerable<string> errorElementNames = new List<string>() { "brokenRules", "fault" };
        private const string BARCODEURL = "https://express.tnt.com/barbecue/barcode?type=Code128C&height=140&width=2&data=";
        private const string IMAGEURL = "https://express.tnt.com/expresswebservices-website/rendering/images";

        public ExpressLabelPdfRenderer(XslCompiledTransform stylesheet, XsltArgumentList? xsltarguments, XmlSchemaSet? schemaset) : base(stylesheet, xsltarguments, schemaset)
        {
            NamePrefix = "ExpressLabel";
            if (xsltarguments == null)
            {
                XsltArguments.AddParam("twoDbarcode_url", "", "");
                XsltArguments.AddParam("barcode_url", "", BARCODEURL);
                XsltArguments.AddParam("code128Bbarcode_url", "", "");
                XsltArguments.AddParam("int2of5barcode_url", "", "");
                XsltArguments.AddParam("images_dir", "", IMAGEURL);
            }
        }

        public override async Task<string> CreatePdf(XDocument xmlresponse, Func<XDocument, bool>? errorcheck, bool validateschema = true, bool savepdf = true) => errorcheck == null
                ? await base.CreatePdf(xmlresponse, IsErrorResponse, validateschema, savepdf)
                : await base.CreatePdf(xmlresponse, errorcheck, validateschema, savepdf);

        public override async Task<string> CreatePdf(string xmlresponseastext, Func<XDocument, bool>? errorcheck, bool validateschema = true, bool savepdf = true) => errorcheck == null
                ? await base.CreatePdf(xmlresponseastext, IsErrorResponse, validateschema, savepdf)
                : await base.CreatePdf(xmlresponseastext, errorcheck, validateschema, savepdf);

        private static bool IsErrorResponse(XDocument responseXml)
        {
            bool result = false;
            foreach (string name in errorElementNames)
            {
                if (result) break;
                foreach (var item in responseXml.Descendants(name))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
    }
}
