using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebView2_testing
{
    public interface ICardAddEditForm
    {
        string Merchant { get; }
        string Processor { get; }
        string ResponseToken { get; }
        System.Windows.Forms.HtmlDocument ResultDocument { get; }
    }
}
