using Microsoft.Web.WebView2.Core;
using Client = ODBPaymentGateway.Client;
using Model = ODBPaymentGateway.Model;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;

namespace WebView2_testing
{
    public partial class CardAddEditForm : Form, ICardAddEditForm
    {
        public enum FormMode
        {
            Create,
            Edit
        }

        #region Constructor
        
        public CardAddEditForm(Model.FormRequest formRequest, string merchant, string processor, FormMode mode, string xApiKey)
        {
            _merchant = merchant;
            _processor = processor;
            string formMode = mode.ToString().ToLower();

            //PaymentModelHelper.ValidateStringParams_FormProcessorMerchantFormModePost(processor, merchant, formMode);

            InitializeComponent();
            webView.NavigationCompleted +=
                new EventHandler<CoreWebView2NavigationCompletedEventArgs>(webView_NavigationCompleted);
            
            submitButton.Visible = mode == FormMode.Edit;

            this.Resize += new System.EventHandler(this.Form_Resize);
            Form_Resize(this, EventArgs.Empty);
        }

        #endregion

        #region Encoding / Decoding

        protected async Task<string> GetCurrentHtmlDocumentAsString()
        {
            var html = await webView.CoreWebView2.ExecuteScriptAsync("document.body.outerHTML");
            html = System.Net.WebUtility.HtmlDecode(UnescapeCodes(html, true));
            return html;
        }

        protected HtmlDocument GetHtmlDocument(string html)
        {
            WebBrowser browser = new WebBrowser
            {
                ScriptErrorsSuppressed = true,
                DocumentText = html
            };
            browser.Document.OpenNew(true);
            browser.Document.Write(html);
            browser.Refresh();
            return browser.Document;
        }

        protected async Task<HtmlDocument> GetCurrentHtmlDocument()
        {
            return GetHtmlDocument(await GetCurrentHtmlDocumentAsString());
        }

        protected static string UnescapeCodes(string src, bool doubleUnescape = false)
        {
            if (doubleUnescape)
                src = src.Replace(@"\\", @"\");

            var rx = new Regex("\\\\([0-9A-Fa-f]+)");
            var res = new StringBuilder();
            var pos = 0;
            foreach (Match m in rx.Matches(src))
            {
                res.Append(src.Substring(pos, m.Index - pos));
                pos = m.Index + m.Length;
                res.Append((char)Convert.ToInt32(m.Groups[1].ToString(), 16));
            }
            res.Append(src.Substring(pos));
            return res.ToString();
        }

        protected string EncodePostHeader(Client.Configuration config)
        {
            StringBuilder headerBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> header in config.DefaultHeader)
            {
                headerBuilder.Append(header.Key + ": " + header.Value + "\r\n");
            }
            foreach (KeyValuePair<string, string> header in config.ApiKey)
            {
                headerBuilder.Append(header.Key + ": " + header.Value + "\r\n");
            }
            return headerBuilder.ToString();
        }

        protected byte[] EncodePostBody(Model.FormRequest formRequest)
        {
            return Encoding.UTF8.GetBytes(formRequest.ToJson());
        }

        #endregion

        #region ICardAddEditForm Members

        private readonly string _merchant;
        public string Merchant => _merchant;

        private readonly string _processor;
        public string Processor => _processor;

        private string _responseToken;
        public string ResponseToken => _responseToken;

        private HtmlDocument _resultDocument;
        public HtmlDocument ResultDocument => _resultDocument;

        #endregion

        #region Events
        protected async void webView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            _ = ResizeWindowByDocumentSize();
            _resultDocument = await GetCurrentHtmlDocument();
        }
        #endregion

        #region Resizing

        private void Form_Resize(object sender, EventArgs e)
        {
            webView.Size = this.ClientSize - new System.Drawing.Size(webView.Location);
            webView.Height = this.ClientSize.Height - submitButton.Height; 
            submitButton.Left = this.ClientSize.Width - submitButton.Width;
            submitButton.Top = this.ClientSize.Height - submitButton.Height;
        }

        private async Task<Size> GetWebViewDocumentSize()
        {
            string getDimFmtString = "var body = document.body, html = document.documentElement; " +
                "Math.max( body.scroll{0}, body.offset{0}, html.client{0}, html.scroll{0}, html.offset{0} );";
            int height = int.Parse( 
                await this.webView.CoreWebView2.ExecuteScriptAsync( 
                    string.Format(getDimFmtString, "Height")));
            int width = int.Parse(
                await this.webView.CoreWebView2.ExecuteScriptAsync(
                    string.Format(getDimFmtString, "Width")));
            return new Size(width, height);
        }

        private async Task ResizeWindowByDocumentSize()
        {
            System.Drawing.Rectangle screenRectangle = this.RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRectangle.Top - this.Top;

            this.webView.Size = await GetWebViewDocumentSize();

            this.Size = new System.Drawing.Size(
                this.webView.Size.Width +
                    this.webView.Margin.Left +
                    this.webView.Margin.Right,
                this.webView.Size.Height +
                    titleHeight +
                    this.submitButton.Height +
                    this.submitButton.Margin.Top +
                    this.submitButton.Margin.Bottom
                );
            this.PerformLayout();

            this.Top = 0;
        }
    }

    #endregion
}
