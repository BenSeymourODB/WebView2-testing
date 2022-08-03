using System.Windows.Forms;

namespace WebView2_testing
{
    public interface ICardAddEditForm
    {
        bool UpdateBillingAddress { get; }
        string Merchant { get; }
        string Processor { get; }
        FormMode Mode { get; }
        string ResponseToken { get; }
        System.Windows.Forms.HtmlDocument ResultDocument { get; }

        DialogResult LaunchAndAwaitResponse();
    }
}
