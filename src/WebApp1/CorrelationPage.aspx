<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CorrelationPage.aspx.cs" Inherits="WebApp1.Correlation" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="CorrelationForm" runat="server">
        <div>
            <asp:TextBox runat="server" ID="CorrelationID" ReadOnly="True"></asp:TextBox>
            <asp:Button runat="server" ID="Reload" Text="Reload" OnClick="Reload_OnClick" />
        </div>
    </form>
</body>
</html>
