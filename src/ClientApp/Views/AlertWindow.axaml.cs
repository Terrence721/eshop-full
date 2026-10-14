using Avalonia.Controls;

namespace eShop.ClientApp.Views;

public partial class AlertWindow : Window
{
    public AlertWindow()
    {
        InitializeComponent();
    }

    public AlertWindow(string message, string title, string buttonLabel) : this()
    {
        TitleText.Text = title;
        MessageText.Text = message;
        OkButton.Content = buttonLabel;
        OkButton.Click += (_, _) => Close();
    }
}
