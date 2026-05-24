using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AISPVZ.Helpers;

public static class PhoneMaskBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(PhoneMaskBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox) return;

        if ((bool)e.NewValue)
        {
            textBox.PreviewTextInput += OnPreviewTextInput;
            textBox.PreviewKeyDown += OnPreviewKeyDown;
            DataObject.AddPastingHandler(textBox, OnPaste);
            if (string.IsNullOrEmpty(textBox.Text))
                textBox.Text = "+7";
        }
        else
        {
            textBox.PreviewTextInput -= OnPreviewTextInput;
            textBox.PreviewKeyDown -= OnPreviewKeyDown;
            DataObject.RemovePastingHandler(textBox, OnPaste);
        }
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        var fullText = GetTextWithInsertion(textBox, e.Text);
        var digits = new string(fullText.Where(char.IsDigit).ToArray());

        if (digits.Length > 11)
        {
            e.Handled = true;
            return;
        }

        e.Handled = true;
        textBox.Text = FormatPhone(digits);
        textBox.CaretIndex = textBox.Text.Length;
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        if (e.Key == Key.Back || e.Key == Key.Delete)
        {
            e.Handled = true;
            var digits = new string(textBox.Text.Where(char.IsDigit).ToArray());
            if (digits.Length > 1)
            {
                digits = digits[..^1];
                textBox.Text = FormatPhone(digits);
            }
            else
            {
                textBox.Text = "+7";
            }
            textBox.CaretIndex = textBox.Text.Length;
        }
    }

    private static void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        if (!e.SourceDataObject.GetDataPresent(DataFormats.Text)) return;

        var pasteText = e.SourceDataObject.GetData(DataFormats.Text) as string ?? "";
        var digits = new string(pasteText.Where(char.IsDigit).ToArray());
        if (digits.Length > 11) digits = digits[..11];

        e.CancelCommand();
        textBox.Text = FormatPhone(digits);
        textBox.CaretIndex = textBox.Text.Length;
    }

    private static string GetTextWithInsertion(TextBox textBox, string insertion)
    {
        var text = textBox.Text;
        var caret = textBox.CaretIndex;
        return text.Insert(Math.Min(caret, text.Length), insertion);
    }

    public static string FormatPhone(string digits)
    {
        if (string.IsNullOrEmpty(digits)) return "+7";
        if (digits.StartsWith("8") && digits.Length > 1)
            digits = "7" + digits[1..];
        if (!digits.StartsWith("7"))
            digits = "7" + digits;

        var result = "+7";
        if (digits.Length > 1)
        {
            var body = digits[1..];
            if (body.Length >= 1)
                result += " (" + body[..Math.Min(3, body.Length)];
            if (body.Length >= 3)
                result += ")";
            if (body.Length > 3)
                result += " " + body[3..Math.Min(6, body.Length)];
            if (body.Length > 6)
                result += "-" + body[6..Math.Min(8, body.Length)];
            if (body.Length > 8)
                result += "-" + body[8..Math.Min(10, body.Length)];
        }
        return result;
    }

    public static string ExtractDigits(string phone)
    {
        return new string(phone.Where(char.IsDigit).ToArray());
    }
}
