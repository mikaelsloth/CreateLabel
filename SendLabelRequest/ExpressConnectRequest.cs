namespace SendLabelRequest
{
    using RestSharp;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using System.Xml.Schema;

    /// <summary>
    /// Abstract base class for request to the various ExpressConnect endpoints, like labeling, pricing and shipping.
    /// </summary>
    public abstract class ExpressConnectRequest
    {
        protected RestClient GetHttpClient { get; } = new();

        /// <summary>
        /// Get or set the login name to be used for the request
        /// </summary>
        public virtual string CustomerId
        {
            get;
            set;
        }

        /// <summary>
        /// Set the password to be used for the request
        /// </summary>
        public virtual string Password
        {
            protected get;
            set;
        }

        /// <summary>
        /// Get or set the URL of the web service endpoint
        /// </summary>
        public abstract string URL
        {
            get;
            internal set;
        }

        /// <summary>
        /// Indicate if the implementation is using Authentication method of the class or no authentication
        /// </summary>
        public abstract bool UseAuthentication
        {
            get;
        }

        /// <summary>
        /// Get or set the header content type
        /// </summary>
        public abstract string ContentType
        {
            get;
        }

        /// <summary>
        /// Create an asyncronous request to the TNT ExpressConnect service
        /// </summary>
        /// <param name="requestXml">A <see cref="XDocument"/> object containing the XML message to be submitted to the web service</param>
        /// <returns>A <see cref="Task"/> object where the result is the XML response from the service</returns>
        public abstract Task<XDocument> SubmitRequestAsync(XDocument requestXml);

        /// <summary>
        /// Create an asyncronous request to the TNT ExpressConnect service
        /// </summary>
        /// <param name="requestXmlAsString">A <see cref="string"/> object containing the XML message to be submitted to the web service</param>
        /// <returns>A <see cref="Task"/> object where the result is the XML response from the service</returns>
        public abstract Task<XDocument> SubmitRequestAsync(string requestXmlAsString);

        /// <summary>
        /// Create a request to the TNT ExpressConnect service 
        /// </summary>
        /// <param name="requestXml">A <see cref="XDocument"/> object containing the XML message to be submitted to the web service</param>
        /// <returns>An <see cref="XDocument"/> object containing the XML response from the service</returns>
        public abstract XDocument SubmitRequest(XDocument requestXml);

        /// <summary>
        /// Create a request to the TNT ExpressConnect service 
        /// </summary>
        /// <param name="requestXmlAsString">A <see cref="string"/> object containing the XML message to be submitted to the web service</param>
        /// <returns>An <see cref="XDocument"/> object containing the XML response from the service</returns>
        public abstract XDocument SubmitRequest(string requestXmlAsString);

        /// <summary>
        /// Create an asyncronous request to the TNT ExpressConnect service after validating the input against a schema
        /// </summary>
        /// <param name="requestXml">A <see cref="XDocument"/> object containing the XML message to be submitted to the web service</param>
        /// <param name="schemaSet">A <see cref="XmlSchemaSet"/> object with the validation schema</param>
        /// <returns>A <see cref="Task"/> object where the result is the XML response from the service</returns>
        public abstract Task<XDocument> SubmitRequestAsync(XDocument requestXml, XmlSchemaSet schemaSet);

        /// <summary>
        /// Create an asyncronous request to the TNT ExpressConnect service after validating the input against a schema
        /// </summary>
        /// <param name="requestXmlAsString">A <see cref="string"/> object containing the XML message to be submitted to the web service</param>
        /// <param name="schemaSet">A <see cref="XmlSchemaSet"/> object with the validation schema</param>
        /// <returns>A <see cref="Task"/> object where the result is the XML response from the service</returns>
        public abstract Task<XDocument> SubmitRequestAsync(string requestXmlAsString, XmlSchemaSet schemaSet);

        /// <summary>
        /// Create a request to the TNT ExpressConnect service after validating the input against a schema
        /// </summary>
        /// <param name="requestXml">A <see cref="XDocument"/> object containing the XML message to be submitted to the web service</param>
        /// <param name="schemaSet">A <see cref="XmlSchemaSet"/> object with the validation schema</param>
        /// <returns>A <see cref="Task"/> object where the result is the XML response from the service</returns>
        public abstract XDocument SubmitRequest(XDocument requestXml, XmlSchemaSet schemaSet);

        /// <summary>
        /// Create a request to the TNT ExpressConnect service after validating the input against a schema
        /// </summary>
        /// <param name="requestXmlAsString">A <see cref="string"/> object containing the XML message to be submitted to the web service</param>
        /// <param name="schemaSet">A <see cref="XmlSchemaSet"/> object with the validation schema</param>
        /// <returns>A <see cref="Task"/> object where the result is the XML response from the service</returns>
        public abstract XDocument SubmitRequest(string requestXmlAsString, XmlSchemaSet schemaSet);

        /// <summary>
        /// Validate if the response contains known error elements
        /// </summary>
        /// <param name="responseXml">A <see cref="XDocument"/> object containing the XML response from the web service</param>
        /// <returns><see langword="true"/> if error elements are found in the response, otherwise <see langword="false"/> </returns>
        public abstract bool IsErrorResponse(XDocument responseXml);

        protected static bool TryValidateSchema(XDocument inputxml, XmlSchemaSet schemas, out string message)
        {
            bool errors = false;
            string tempmsg = string.Empty;

            inputxml.Validate(schemas, (o, e) =>
            {
                errors = true;
                tempmsg = "The following messages came from validating against the schema: \r\n";
                switch (e.Severity)
                {
                    case XmlSeverityType.Error:
                        tempmsg = tempmsg + "\r\n" + "ERROR: " + e.Message;
                        break;
                    case XmlSeverityType.Warning:
                        tempmsg = tempmsg + "\r\n" + "WARNING: " + e.Message;
                        break;
                }
            });

            message = tempmsg;
            return errors;
        }

        protected static bool IsErrorResponse(XDocument responseXml, IEnumerable<string> errorElementNames)
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
