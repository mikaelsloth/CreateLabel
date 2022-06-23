namespace SendLabelRequest
{
    using RestSharp;
    using RestSharp.Authenticators;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using System.Xml.Schema;

    public class ExpressLabelRequest : ExpressConnectRequest
    {
        private string url = "https://express.tnt.com/expresslabel/documentation/getlabel";
        private readonly IEnumerable<string> ErrorElementNames = new List<string>() { "brokenRules", "fault"};
        private readonly bool useAuthentication = true;
        private readonly string contenttype = "text/xml";

        /// <summary>
        /// Get or set the endpoint URL for the API
        /// </summary>
        public override string URL
        {
            get => url;
            internal set => url = value;
        }

        /// <summary>
        /// Get a bool indicating whether Basic Authentication is required for the endpoint
        /// </summary>
        public override bool UseAuthentication => useAuthentication;

        /// <summary>
        /// Get a string containing the Content type for the header
        /// </summary>
        public override string ContentType => contenttype;

        /// <summary>
        /// Represents an asyncronous request to the ExpressLabel endpoint
        /// </summary>
        /// <param name="requestXml">A <see cref="XDocument"/> containing the request to submit</param>
        /// <returns>A <see cref="Task"/> object providing a <see cref="XDocument"/> response from the endpoint</returns>
        /// 
        /// <exception cref="ArgumentNullException">Authentication information is null or the endpoint <see cref="Uri"/> is null</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout</exception>
        /// <exception cref="XmlException">There is a load or parse error in the response XML received from the server</exception>
        /// <exception cref="Exception">Please examine the error details and report to developer</exception>
        public async override Task<XDocument> SubmitRequestAsync(XDocument requestXml) => await SubmitRequestAsyncImpl(requestXml.ToString());

        /// <summary>
        /// Represents a request to the ExpressLabel endpoint
        /// </summary>
        /// <param name="requestXml">A <see cref="XDocument"/> containing the request to submit</param>
        /// <returns>A <see cref="Task"/> object providing a <see cref="XDocument"/> response from the endpoint</returns>
        /// 
        /// <exception cref="ArgumentNullException">Authentication information is null or the endpoint <see cref="Uri"/> is null</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout</exception>
        /// <exception cref="XmlException">There is a load or parse error in the response XML received from the server</exception>
        /// <exception cref="Exception">Please examine the error details and report to developer</exception>
        public override XDocument SubmitRequest(XDocument requestXml) => SubmitRequestImpl(requestXml.ToString());

        /// <summary>
        /// Represents an asyncronous request to the ExpressLabel endpoint
        /// </summary>
        /// <param name="requestXmlAsString">A <see cref="string"/> object containing the XML message to be submitted to the web service</param>
        /// <returns>An <see cref="XDocument"/> object containing the XML response from the service</returns>
        /// 
        /// <exception cref="ArgumentNullException">Authentication information is null or the endpoint <see cref="Uri"/> is null</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout</exception>
        /// <exception cref="XmlException">There is a load or parse error in the response XML received from the server</exception>
        /// <exception cref="Exception">Please examine the error details and report to developer</exception>
        public async override Task<XDocument> SubmitRequestAsync(string requestXmlAsString) => await SubmitRequestAsyncImpl(requestXmlAsString);

        /// <summary>
        /// Represents a request to the ExpressLabel endpoint
        /// </summary>
        /// <param name="requestXmlAsString">A <see cref="string"/> object containing the XML message to be submitted to the web service</param>
        /// <returns>An <see cref="XDocument"/> object containing the XML response from the service</returns>
        /// 
        /// <exception cref="ArgumentNullException">Authentication information is null or the endpoint <see cref="Uri"/> is null</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout</exception>
        /// <exception cref="XmlException">There is a load or parse error in the response XML received from the server</exception>
        /// <exception cref="Exception">Please examine the error details and report to developer</exception>
        public override XDocument SubmitRequest(string requestXmlAsString) => SubmitRequestImpl(requestXmlAsString);

        private async Task<XDocument> SubmitRequestAsyncImpl(string requestXmlAsString)
        {
            try
            {
                RestClient client = GetHttpClient;
                RestRequest request = SetupConnectionParameters(requestXmlAsString, client);

                RestResponse response = await client.PostAsync(request);
                return ParseToXDoc(response);
            }
            catch
            {
                throw;
            }
        }

        private static XDocument ParseToXDoc(RestResponse response)
        {
            string value = response.Content;
            try
            {
                XDocument doc = XDocument.Parse(value, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
                return doc;
            }
            catch (Exception ex)
            {
                throw new Exception($"The label response text to render is invalid. \r\nThe error returned was : \r\n{ex.Message}\r\nPlease correct the errors and try again.", ex);
            }
        }

        private RestRequest SetupConnectionParameters(string requestXmlAsString, RestClient client)
        {
            RestRequest request = new() { Resource = URL };
            _ = request.AddHeader("Content-Type", ContentType);
            _ = request.AddHeader("Accept", "*/*");
            _ = request.AddParameter(ContentType, requestXmlAsString, ParameterType.RequestBody);
            if (UseAuthentication)
            {
                client.Authenticator = new HttpBasicAuthenticator(CustomerId, Password);
            }

            return request;
        }

        private XDocument SubmitRequestImpl(string requestXmlAsString)
        {
            try
            {
                RestClient client = GetHttpClient;
                RestRequest request = SetupConnectionParameters(requestXmlAsString, client);

                RestResponse response = client.Post(request);
                return ParseToXDoc(response);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Represents an asyncronous request to the ExpressLabel endpoint. This overload validates the input against a given XSD schema
        /// </summary>
        /// <param name="requestXml">A <see cref="XDocument"/> containing the request to submit</param>
        /// <param name="schemaSet">A <see cref="XmlSchemaSet"/> object with the validation schema</param>
        /// <returns>A <see cref="Task"/> object providing a <see cref="XDocument"/> response from the endpoint</returns>
        /// 
        /// <exception cref="ArgumentNullException">Authentication information is null or the endpoint <see cref="Uri"/> is null</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout</exception>
        /// <exception cref="XmlException">There is a load or parse error in the response XML received from the server</exception>
        /// <exception cref="Exception">Please examine the error details and report to developer</exception>
        public async override Task<XDocument> SubmitRequestAsync(XDocument requestXml, XmlSchemaSet schemaSet) =>
            // If schema not valid inform user
            !TryValidateSchema(requestXml, schemaSet, out string message)
                ? throw new ArgumentException($"The label request text to render is not confirming to the ExpressLabel schema. \r\nThe error returned was : \r\n{message}\r\nPlease ensure you copy a valid ExpressLabel request.")
                : await SubmitRequestAsyncImpl(requestXml.ToString());

        /// <summary>
        /// Represents an asyncronous request to the ExpressLabel endpoint. This overload validates the input against a given XSD schema
        /// </summary>
        /// <param name="requestXmlAsString">A <see cref="string"/> object containing the XML message to be submitted to the web service</param>
        /// <param name="schemaSet">A <see cref="XmlSchemaSet"/> object with the validation schema</param>
        /// <returns>A <see cref="Task"/> object providing a <see cref="XDocument"/> response from the endpoint</returns>
        /// 
        /// <exception cref="ArgumentNullException">Authentication information is null or the endpoint <see cref="Uri"/> is null</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout</exception>
        /// <exception cref="XmlException">There is a load or parse error in the response XML received from the server</exception>
        /// <exception cref="Exception">Please examine the error details and report to developer</exception>
        public async override Task<XDocument> SubmitRequestAsync(string requestXmlAsString, XmlSchemaSet schemaSet)
        {
            // If not valid XML then inform caller
            XDocument inputxml;
            try
            {
                inputxml = XDocument.Parse(requestXmlAsString, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
                return !TryValidateSchema(inputxml, schemaSet, out string message)
                    ? throw new ArgumentException($"The label request text to render is not confirming to the ExpressLabel schema. \r\nThe error returned was : \r\n{message}\r\nPlease ensure you copy a valid ExpressLabel request.")
                    : await SubmitRequestAsyncImpl(requestXmlAsString);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"The label request text to render is invalid. \r\nThe error returned was : \r\n{ex.Message}\r\nPlease correct the errors and try again.", nameof(requestXmlAsString), ex);
            }
        }

        /// <summary>
        /// Represents a request to the ExpressLabel endpoint. This overload validates the input against a given XSD schema
        /// </summary>
        /// <param name="requestXml">A <see cref="XDocument"/> containing the request to submit</param>
        /// <param name="schemaSet">A <see cref="XmlSchemaSet"/> object with the validation schema</param>
        /// <returns>A <see cref="Task"/> object providing a <see cref="XDocument"/> response from the endpoint</returns>
        /// 
        /// <exception cref="ArgumentNullException">Authentication information is null or the endpoint <see cref="Uri"/> is null</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout</exception>
        /// <exception cref="XmlException">There is a load or parse error in the response XML received from the server</exception>
        /// <exception cref="Exception">Please examine the error details and report to developer</exception>
        public override XDocument SubmitRequest(XDocument requestXml, XmlSchemaSet schemaSet) =>             
            // If schema not valid inform user
            !TryValidateSchema(requestXml, schemaSet, out string message)
                ? throw new ArgumentException($"The label request text to render is not confirming to the ExpressLabel schema. \r\nThe error returned was : \r\n{message}\r\nPlease ensure you copy a valid ExpressLabel request.")
                : SubmitRequestImpl(requestXml.ToString());

        /// <summary>
        /// Represents a request to the ExpressLabel endpoint. This overload validates the input against a given XSD schema
        /// </summary>
        /// <param name="requestXmlAsString">A <see cref="string"/> object containing the XML message to be submitted to the web service</param>
        /// <param name="schemaSet">A <see cref="XmlSchemaSet"/> object with the validation schema</param>
        /// <returns>A <see cref="Task"/> object providing a <see cref="XDocument"/> response from the endpoint</returns>
        /// 
        /// <exception cref="ArgumentNullException">Authentication information is null or the endpoint <see cref="Uri"/> is null</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout</exception>
        /// <exception cref="XmlException">There is a load or parse error in the response XML received from the server</exception>
        /// <exception cref="Exception">Please examine the error details and report to developer</exception>
        public override XDocument SubmitRequest(string requestXmlAsString, XmlSchemaSet schemaSet)
        {
            // If not valid XML then inform caller
            XDocument inputxml;
            try
            {
                inputxml = XDocument.Parse(requestXmlAsString, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
                return !TryValidateSchema(inputxml, schemaSet, out string message)
                    ? throw new ArgumentException($"The label request text to render is not confirming to the ExpressLabel schema. \r\nThe error returned was : \r\n{message}\r\nPlease ensure you copy a valid ExpressLabel request.")
                    : SubmitRequestImpl(requestXmlAsString);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"The label request text to render is invalid. \r\nThe error returned was : \r\n{ex.Message}\r\nPlease correct the errors and try again.", nameof(requestXmlAsString), ex);
            }
        }

        public override bool IsErrorResponse(XDocument responseXml) => IsErrorResponse(responseXml, ErrorElementNames);
    }
}
