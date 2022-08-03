using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
    }
}
