using System;
using Albumprinter.CorrelationTracking.Correlation.Core;

namespace WebApp1
{
    public partial class Correlation : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.CorrelationID.Text = CorrelationScope.Current.CorrelationId.ToString();
        }

        protected void Reload_OnClick(object sender, EventArgs e)
        {
            this.CorrelationID.Text = CorrelationScope.Current.CorrelationId.ToString();
        }
    }
}