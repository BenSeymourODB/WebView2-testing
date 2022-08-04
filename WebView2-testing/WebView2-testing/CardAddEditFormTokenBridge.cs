using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WebView2_testing
{
    /// <summary>
    /// Derivative of <see cref="MPXWebScriptingConnector"/> with <see cref="ClassInterfaceAttribute"/> = 
    /// <see cref="ClassInterfaceType.AutoDual"/>
    /// </summary>
    /// <remarks>
    /// Based on examples from 
    /// <a href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.addhostobjecttoscript"/>
    /// </remarks>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class CardAddEditFormTokenBridge: MPXWebScriptingConnector
    {
        public CardAddEditFormTokenBridge(): base() { }

        public CardAddEditFormTokenBridge(Func<string, string> setCCTokenCallback, Action editCompletedCallback): base(setCCTokenCallback) 
        {
            SetEditEventCallback(editCompletedCallback);
        }

        private Action editEventCallback;

        public void SetEditEventCallback(Action editEventCallback) => this.editEventCallback = editEventCallback;

        public void NotifyEditFinished() => this.editEventCallback?.Invoke();
    }
}
