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
using System.IO;
using System.Globalization;

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
        
        public CardAddEditForm(Model.FormRequest formRequest, string merchant, string processor, FormMode mode, Client.Configuration config)
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

            EventHandler<CoreWebView2InitializationCompletedEventArgs> loadFormOnReady = (sender, args) =>
            {
                string url = GetRequestUrlString(config, merchant, processor, mode);
                var request = webView.CoreWebView2.Environment.CreateWebResourceRequest(
                        url, "POST", EncodePostBody(formRequest), EncodePostHeader(config));
                webView.CoreWebView2.NavigateWithWebResourceRequest(request);
            };
            webView.CoreWebView2InitializationCompleted += loadFormOnReady;
        }

        #endregion



        public string GetRequestUrlString(Client.Configuration config, string merchant, string processor, FormMode mode) =>
            config.BasePath + $"/form/{processor}/{merchant}/{mode.ToString().ToLower()}";

        #region Encoding / Decoding

        /// <summary>
        /// 
        /// </summary>
        /// <returns>NULL or HTML string</returns>
        protected async Task<string> GetCurrentHtmlDocumentAsString()
        {
            var html = await webView.CoreWebView2.ExecuteScriptAsync("document.body.outerHTML");
            var unescapedHtml = (string.IsNullOrEmpty(html) || html == "null") ? 
                null : 
                System.Net.WebUtility.HtmlDecode(UnescapeCodes(html));
            return unescapedHtml;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns>NULL or <see cref="HtmlDocument"/></returns>
        protected async Task<HtmlDocument> GetCurrentHtmlDocument()
        {
            var htmlString = await GetCurrentHtmlDocumentAsString();
            return string.IsNullOrEmpty(htmlString) ? null : GetHtmlDocument(htmlString);
        }

        protected static string UnescapeCodes(string src)
        {
            var result = UnescapeUnicodes(src);
            result = result.Replace("\\\\n", "\n");
            result = result.Replace("\\\n", "\n");
            result = result.Replace("\\n", "\n");
            result = result.Replace("\\t", "\t");
            result = result.Replace("\\\"", "\"");
            result = result.Replace("\\", "");
            return result;
        }

        protected static string UnescapeUnicodes(string src)
        {
            return Regex.Replace(
                src,
                @"\\[Uu]([0-9A-Fa-f]{4})",
                m => char.ToString(
                    (char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));
        }

        protected string EncodePostHeader(Client.Configuration config)
        {
            StringBuilder headerBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> header in config.DefaultHeader)
            {
                // WebView2 doesn't like when you try to set some headers manually
                if(header.Key != "Host")
                    headerBuilder.Append(header.Key + ": " + header.Value + "\r\n");
            }
            foreach (KeyValuePair<string, string> header in config.ApiKey)
            {
                headerBuilder.Append(header.Key + ": " + header.Value + "\r\n");
            }
            return headerBuilder.ToString();
        }

        protected MemoryStream EncodePostBody(Model.FormRequest formRequest)
        {
            string postDataString = formRequest.ToJson();
            byte[] postData = Encoding.UTF8.GetBytes(postDataString);
            MemoryStream postDataStream = new MemoryStream(postDataString.Length);
            postDataStream.Write(postData, 0, postData.Length);
            postDataStream.Seek(0, SeekOrigin.Begin);
            return postDataStream;
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
            _ = ResizeForOptimalFit();
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

        private async Task ResizeForOptimalFit()
        {
            await ResizeWindowByDocumentSize();
            ResizeYToExcludeTaskbar();
        }

        private void ResizeYToExcludeTaskbar()
        {
            int bottomOfUsableArea = GetUsableMonitorArea().Bottom;
            if (this.Bottom > bottomOfUsableArea)
            {
                this.Size = new Size(this.Width, this.Height - (this.Bottom - bottomOfUsableArea));
            }
        }

        private Rectangle GetUsableMonitorArea()
        {
            return Screen.FromHandle(this.Handle).WorkingArea;
        }
    }

    #endregion
}
