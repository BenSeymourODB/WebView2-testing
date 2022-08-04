using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WebView2_testing
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Based on https://docs.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.addhostobjecttoscript?view=webview2-dotnet-1.0.1264.42#microsoft-web-webview2-core-corewebview2-addhostobjecttoscript(system-string-system-object)
    /// </remarks>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class CardAddEditFormTokenBridge
    {
        public string CCAddToken { get; set; }

        public void HandleError(string message, string url, string lineNumber)
        {
            MessageBox.Show(
                $"The page at {url} encountered an error on line {lineNumber}:\n" +
                message, "Error");
        }
    }
}
