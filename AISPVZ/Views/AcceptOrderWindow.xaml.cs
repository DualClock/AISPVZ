using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using AISPVZ.ViewModels;

namespace AISPVZ.Views;

public partial class AcceptOrderWindow : Window
{
    private readonly AcceptOrderViewModel _viewModel;
    private readonly DispatcherTimer _clockTimer;
    private readonly DispatcherTimer _toastTimer;
    private bool _isToastSuccess;

    public AcceptOrderWindow()
    {
        InitializeComponent();

        _viewModel = new AcceptOrderViewModel();
        DataContext = _viewModel;

        // Clock timer
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (s, e) => UpdateClock();
        _clockTimer.Start();

        // Toast timer
        _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _toastTimer.Tick += (s, e) => HideToast();

        Loaded += AcceptOrderWindow_Loaded;
        Closing += (s, e) => _clockTimer.Stop();

        _viewModel.OperationCompleted += OnOperationCompleted;
        _viewModel.ShowToastNotification += (title, msg, success) =>
        {
            Dispatcher.Invoke(() => ShowToast(title, msg, success));
        };
    }

    private async void AcceptOrderWindow_Loaded(object s, RoutedEventArgs e)
    {
        UpdateClock();
        UpdateOperatorName();

        try
        {
            await _viewModel.LoadDataAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ShowToast("Ошибка", ex.Message, false);
        }

        // Autofocus barcode
        _ = Dispatcher.BeginInvoke(new Action(() =>
        {
            BarcodeInput.Focus();
            BarcodeInput.SelectAll();
        }), System.Windows.Threading.DispatcherPriority.Input);

        // Set default planned date to +3 days
        _viewModel.PlannedIssueDate = DateTime.Now.AddDays(3);
    }

    private void UpdateClock()
    {
        CurrentTimeText.Text = DateTime.Now.ToString("HH:mm:ss");
    }

    private void UpdateOperatorName()
    {
        if (App.CurrentOperator != null)
            OperatorNameText.Text = $"Оператор: {App.CurrentOperator.FullName}";
    }

    private void OnOperationCompleted(bool success)
    {
        Dispatcher.Invoke(() =>
        {
            if (success)
            {
                ShowToast("✓ Успешно", "Заказ успешно принят!", true);
                PlaySuccessSound();
                PulseAcceptButton();
            }
        });
    }

    private void ShowToast(string title, string message, bool isSuccess)
    {
        _isToastSuccess = isSuccess;
        ToastTitle.Text = title;
        ToastMessage.Text = message;

        ToastBorder.Background = isSuccess
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129))  // #10B981
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(239, 68, 68)); // #EF4444

        ToastBorder.Visibility = Visibility.Visible;
        ToastBorder.Opacity = 0;

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
        ToastBorder.BeginAnimation(OpacityProperty, fadeIn);

        _toastTimer.Stop();
        _toastTimer.Start();
    }

    private void HideToast()
    {
        _toastTimer.Stop();
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
        fadeOut.Completed += (s, e) => ToastBorder.Visibility = Visibility.Collapsed;
        ToastBorder.BeginAnimation(OpacityProperty, fadeOut);
    }

    private void PulseAcceptButton()
    {
        var storyboard = (Storyboard)FindResource("PulseAnimation");
        storyboard?.Begin();
    }

    private void PlayBeep()
    {
        try { SystemSounds.Beep.Play(); } catch { }
    }

    private void PlaySuccessSound()
    {
        try
        {
            SystemSounds.Asterisk.Play();
        }
        catch { }
    }

    // Barcode scanner event - beep on scan
    private void BarcodeInput_GotFocus(object sender, RoutedEventArgs e)
    {
        BarcodeHint.Visibility = Visibility.Collapsed;
    }

    // Comment expand/collapse
    private void CommentToggle_Click(object sender, RoutedEventArgs e)
    {
        if (CommentToggle.IsChecked == true)
        {
            CommentBox.Visibility = Visibility.Visible;
            CommentToggle.Content = "📝 КОММЕНТАРИЙ ▼";
        }
        else
        {
            CommentBox.Visibility = Visibility.Collapsed;
            CommentToggle.Content = "📝 КОММЕНТАРИЙ ►";
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        switch (e.Key)
        {
            case Key.F2:
                _viewModel.AddItemCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.F3:
                _viewModel.AssignAutoCellCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Escape:
                if (CommentToggle.IsChecked == true)
                {
                    CommentToggle.IsChecked = false;
                    CommentBox.Visibility = Visibility.Collapsed;
                    CommentToggle.Content = "📝 КОММЕНТАРИЙ ►";
                }
                else
                {
                    Close();
                }
                e.Handled = true;
                break;
            case Key.Enter:
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    _viewModel.SaveOrderCommand.Execute(null);
                    e.Handled = true;
                }
                break;
        }
    }
}