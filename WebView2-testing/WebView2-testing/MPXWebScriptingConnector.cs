using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebView2_testing
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IMPXWebScriptingConnector
    {
        void SetCCToken(string token); //should call callback set by SetSetCCTokenCallback
        void SetSetCCTokenCallback(Func<string, string> callback); //callback function takes token as input and returns it
        void ElementToString(dynamic jsReturn);
        void HandleError(string message, string url, string lineNumber);
    }

    [ComVisible(true)]
    public class MPXWebScriptingConnector : IMPXWebScriptingConnector
    {
        public MPXWebScriptingConnector() { }
        public MPXWebScriptingConnector(Func<string, string> setCCTokenCallback)
        {
            this.setCCTokenCallback = setCCTokenCallback;
        }

        public void ElementToString(dynamic jsReturn)
        {
            MessageBox.Show(jsReturn.ToString());
        }

        public void HandleError(string message, string url, string lineNumber)
        {
            MessageBox.Show(
                $"The page at {url} encountered an error on line {lineNumber}:\n" +
                message, "Error");
        }

        private string ccToken = "";
        private Func<string, string> setCCTokenCallback;
        public void SetCCToken(string token)
        {
            this.ccToken = token;
            this.setCCTokenCallback(token);
        }
        public void SetSetCCTokenCallback(Func<string, string> callback) => this.setCCTokenCallback = callback;
    }
}
