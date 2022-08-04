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
        #region Constants

        private const string _jsGetCCTokenStringFromAddedNodes = "mutation.addedNodes.item(0).value";
        private const string _ccTokenParentElementId = "#payment-form";

        #endregion

        #region Constructor

        public CardAddEditForm(Model.FormRequest formRequest, string merchant, string processor, FormMode mode, Client.Configuration config)
        {
            //PaymentModelHelper.ValidateStringParams_FormProcessorMerchantFormModePost(processor, merchant, formModeString);

            Merchant = merchant;
            Processor = processor;
            Mode = mode;
            string formModeString = mode.ToString().ToLower();

            Bridge = new CardAddEditFormTokenBridge(this.SetResponseToken, this.SetResultDocAndOKClose);

            InitializeComponent();
            webView.NavigationCompleted +=
                new EventHandler<CoreWebView2NavigationCompletedEventArgs>(webView_NavigationCompleted);

            this.Resize += new System.EventHandler(this.Form_Resize);

            EventHandler<CoreWebView2InitializationCompletedEventArgs> loadFormOnReady = (sender, args) =>
            {
                string url = GetRequestUrlString(config.BasePath, merchant, processor, formModeString);
                var request = webView.CoreWebView2.Environment.CreateWebResourceRequest(
                        url, "POST", EncodePostBody(formRequest), EncodePostHeader(config));
                webView.CoreWebView2.NavigateWithWebResourceRequest(request);
            };
            webView.CoreWebView2InitializationCompleted += loadFormOnReady;
        }

        #endregion

        #region Private Methods

        private void SetEditButtonVisibility()
        {
            submitButton.Visible =
                Mode == FormMode.Edit &&
                !PageHasSubmitEditButton();
        }

        private bool PageHasSubmitEditButton()
        {
            bool found = false;
            HtmlElementCollection buttons = ResultDocument?.GetElementsByTagName("button");
            if (buttons != null)
            {
                foreach (HtmlElement button in buttons)
                {
                    if (button.InnerText.Contains("Edit Profile"))
                        found = true;
                }
            }
            return found;
        }

        private string SetResponseToken(string token)
        {
            this.ResponseToken = token.Trim();
            this.SetResultDocAndOKClose();
            return this.ResponseToken;
        }

        private async void SetResultDocAndOKClose()
        {
            ResultDocument = await GetCurrentHtmlDocument();
            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion

        #region Navigation

        public string GetRequestUrlString(string basePath, string merchant, string processor, string formMode) =>
            basePath + $"/form/{processor}/{merchant}/{formMode.ToString().ToLower()}";

        #endregion

        #region Form Capture

        private CardAddEditFormTokenBridge Bridge { get; set; }

        /// <summary>
        /// Attach MutationObserver to a DOM element matching the query string.
        /// </summary>
        /// <remarks>
        /// Credit to https://stackoverflow.com/a/62883088/5403341
        /// </remarks>
        /// <param name="parentNodeId"></param>
        /// <returns></returns>
        private async Task<string> CreateCCTokenObserver(
            string parentNodeId,
            string hostObjectName,
            string hostObjectCCTokenReceiverMethodName,
            string jsVariableToCapture,
            bool attributes = false,
            bool subtree = false,
            bool childList = false,
            bool charData = false
            )
        {
            string ScriptEl = @"
                var observer = new MutationObserver(function(mutations) {
                mutations.forEach(function(mutation) {
                        chrome.webview.hostObjects." +
                        hostObjectName + $".{hostObjectCCTokenReceiverMethodName}({jsVariableToCapture});" + @"
                    });
                });

            //everything
            var observerConfig = {
                attributes: " + attributes.ToString().ToLower() + @",
                subtree: " + subtree.ToString().ToLower() + @",
                childList: " + childList.ToString().ToLower() + @",
                characterData: " + charData.ToString().ToLower() + @"
            };

                var targetNode = document.querySelector('" + parentNodeId + @"');
                observer.observe(targetNode, observerConfig);";

            return await ExecuteScript(ScriptEl);
        }

        private async Task<string> CreateEditEventListener(
            string parentNodeId,
            string hostObjectName,
            string hostObjectEditCompletedMethodName
            )
        {
            string script = @"
                var eventParent = document.querySelector('" + parentNodeId + @"');
                eventParent.addEventListener('edit', function (event) {
                    chrome.webview.hostObjects." +
                        $"{hostObjectName}.{hostObjectEditCompletedMethodName}()" + @";
                });";
            return await ExecuteScript(script);
        }

        private async Task<string> ExecuteScript(string code)
        {
            return await webView.ExecuteScriptAsync(code);
        }

        /// <summary>
        /// Handle JS errors using a function on a given host object.
        /// </summary>
        /// <remarks>
        /// The function must accept args (string message, string url, int lineNumber)
        /// </remarks>
        /// <param name="hostObjectName"></param>
        /// <param name="hostObjectFnName"></param>
        /// <returns></returns>
        private async Task<string> AddErrorHandler(string hostObjectName, string hostObjectFnName)
        {
            return await ExecuteScript(@"window.onerror = function(message, url, lineNumber) 
                { 
                 chrome.webview.hostObjects." +
                        hostObjectName + "." + hostObjectFnName + @"(message, url, lineNumber);
                }");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>NULL or HTML string</returns>
        protected async Task<string> GetCurrentHtmlDocumentAsString()
        {
            var html = await ExecuteScript("document.body.outerHTML");
            var unescapedHtml = (string.IsNullOrEmpty(html) || html == "null") ? 
                null : 
                System.Net.WebUtility.HtmlDecode(UnescapeCodes(html));
            return unescapedHtml;
        }

        protected static HtmlDocument GetHtmlDocument(string html)
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

        #endregion

        #region Encoding / Decoding

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

        public string Merchant { get; private set; }
        public string Processor { get; private set; }
        public FormMode Mode { get; private set; }
        public string ResponseToken { get; private set; }
        public HtmlDocument ResultDocument { get; set; }
        public bool UpdateBillingAddress
        {
            get
            {
                if (this.updateBillingAddressCheckBox.Visible)
                    return this.updateBillingAddressCheckBox.Checked;
                else
                    return false;
            }
        }

        public DialogResult LaunchAndAwaitResponse() => this.ShowDialog();

        #endregion

        #region Events

        /// <summary>
        /// On navigation completed, if page is loaded, add observers to get data out of the page on submit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void webView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            _ = ResizeForOptimalFit();
            ResultDocument = await GetCurrentHtmlDocument();
            if(ResultDocument != null)
            {
                // Add JavaScript-to-C# bridge object
                webView.CoreWebView2.AddHostObjectToScript(nameof(Bridge), Bridge);
                
                // Watch for CC token being added to Create form after submit
                _ = this.CreateCCTokenObserver(_ccTokenParentElementId, nameof(Bridge), nameof(Bridge.SetCCToken),
                    _jsGetCCTokenStringFromAddedNodes, childList: true);
                
                // Watch for Edit form submit
                _ = this.CreateEditEventListener(_ccTokenParentElementId, nameof(Bridge), nameof(Bridge.NotifyEditFinished));
                
                _ = this.AddErrorHandler(nameof(Bridge), nameof(Bridge.HandleError));

                SetEditButtonVisibility();
            }
        }
        #endregion

        #region Resizing

        private void Form_Resize(object sender, EventArgs e)
        {
            webView.Size = this.ClientSize - new System.Drawing.Size(webView.Location);
            webView.Height = this.ClientSize.Height - submitButton.Height; 
        }

        private async Task<Size> GetWebViewDocumentSize()
        {
            string getDimFmtString = "var body = document.body, html = document.documentElement; " +
                "Math.max( body.scroll{0}, body.offset{0}, html.client{0}, html.scroll{0}, html.offset{0} );";
            int height = int.Parse( 
                await ExecuteScript( 
                    string.Format(getDimFmtString, "Height")));
            int width = int.Parse(
                await ExecuteScript(
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
