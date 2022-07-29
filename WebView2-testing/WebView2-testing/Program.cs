using Client = ODBPaymentGateway.Client;
using Model = ODBPaymentGateway.Model;
using System;
using System.Windows.Forms;
using System.Collections.Generic;

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

            Application.Run(new CardAddEditForm(CreateFormRequest(), "odb", "stripe", CardAddEditForm.FormMode.Create, GetConfiguration()));
        }

        public static Client.Configuration GetConfiguration()
        {
            return new Client.Configuration()
            {
                // 1/25/21: don't add ApiKeys in both ApiKey and DefaultHeader:
                //  interferes with API calls that take an ApiKey entry as a param.
                //  Classes that interact w/ PaymentGateway API w/o running requests through Swagger
                //  (e.g. CreditCardEntryForm) must collect DefaultHeaders and ApiKeys separately.
                ApiKey = new Dictionary<string, string>{
                        { "x-api-key",  xApiKey}
                    },
                BasePath = basePath,
                DefaultHeader = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Host", PaymentModelHelper.ExtractHostFromBasePath(basePath) },
                    { "Accept", "*/*" }
                }
            };
        }

        public static Model.FormRequest CreateFormRequest()
        {

            (string addrLine1, string addrLine2) =
                PaymentModelHelper.ConvertThreeLineAddrToTwo(
                    ccRequest.Address1, ccRequest.Address2, ccRequest.Address3);

            Model.Address address = new Model.Address(
                PaymentModelHelper.MpxCountryCodeToISOCode(ccRequest.Country),
                ccRequest.City,
                ccRequest.State,
                ccRequest.PostalCode,
                addrLine2,
                addrLine1
            );
            Model.Customer customer = new Model.Customer(
                address,
                ccRequest.FirstName + " " + ccRequest.LastName,
                ccRequest.EntityId.ToString()
            );
            Model.FormRequest formRequest = new Model.FormRequest(
                Configuration.ApiPaymentSource,
                customer
            );
            return formRequest;
        }
    }
}
