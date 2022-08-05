using Client = ODBPaymentGateway.Client;
using Model = ODBPaymentGateway.Model;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Configuration;

namespace WebView2_testing
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new CardAddEditForm_Edge(CreateFormRequest(), "dh", "stripe", FormMode.Edit, GetConfiguration()));
        }

        public static string GetAppSetting(string key) => ConfigurationManager.AppSettings[key];

        public static Client.Configuration GetConfiguration()
        {
            string apiKeyName = "x-api-key";
            string basePath = GetAppSetting("DefaultUrl");

            return new Client.Configuration()
            {
                // 1/25/21: don't add ApiKeys in both ApiKey and DefaultHeader:
                //  interferes with API calls that take an ApiKey entry as a param.
                //  Classes that interact w/ PaymentGateway API w/o running requests through Swagger
                //  (e.g. CreditCardEntryForm) must collect DefaultHeaders and ApiKeys separately.
                ApiKey = new Dictionary<string, string>{
                        { apiKeyName,  GetAppSetting(apiKeyName)}
                    },
                BasePath = basePath,
                DefaultHeader = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Host", ExtractHostFromBasePath(basePath) },
                    { "Accept", "*/*" }
                }
            };
        }

        public static Model.FormRequest CreateFormRequest()
        {
            (string firstName, string lastName) = ("Test", "User");
            (string addrLine1, string addrLine2) = ("123 Seasame St", "");

            Model.Address address = new Model.Address(
                "USA",
                "New York City",
                "NY",
                "10001",
                addrLine2,
                addrLine1
            );
            Model.Customer customer = new Model.Customer(
                address,
                firstName + " " + lastName,
                "123456789"
            );
            Model.FormRequest formRequest = new Model.FormRequest(
                "MPX",
                customer
            );
            return formRequest;
        }

        public static string ExtractHostFromBasePath(string uri)
        {
            int protocolTagLength = uri.IndexOf("://") + 3;
            return uri.Substring(
                protocolTagLength,
                uri.IndexOf('/', protocolTagLength) - protocolTagLength
                );
        }
    }
}
