using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;

namespace LibraryDesk.Windows;

public partial class MainWindow : Window
{
    private const string DefaultHomeUrl = "https://www.iranpl.ir/";
    private readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "shortcuts.json");
    private readonly DispatcherTimer _clock = new() { Interval = TimeSpan.FromSeconds(1) };
    private Dictionary<string, string> _links = new();

    public MainWindow()
    {
        InitializeComponent();
        LoadLinks();
        UpdateDateTime();
        _clock.Tick += (_, _) => UpdateDateTime();
        _clock.Start();
        Loaded += async (_, _) =>
        {
            try
            {
                var userData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LibraryDesk", "WebView2");
                var env = await CoreWebView2Environment.CreateAsync(null, userData);
                await Browser.EnsureCoreWebView2Async(env);
                Browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                Browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
                Browser.CoreWebView2.Settings.IsPasswordAutosaveEnabled = true;
                Browser.CoreWebView2.NewWindowRequested += (_, e) => { e.Handled = true; Browser.CoreWebView2.Navigate(e.Uri); };
                Navigate(GetLink("home"));
            }
            catch (Exception ex) { StatusText.Text = "خطا در راه‌اندازی مرورگر داخلی: " + ex.Message; }
        };
    }

    private void LoadLinks()
    {
        _links = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["home"] = DefaultHomeUrl, ["member"] = DefaultHomeUrl, ["documents"] = DefaultHomeUrl,
            ["loan"] = DefaultHomeUrl, ["return"] = DefaultHomeUrl, ["book"] = DefaultHomeUrl, ["culture"] = DefaultHomeUrl
        };
        try
        {
            if (File.Exists(_configPath))
            {
                var saved = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(_configPath));
                if (saved != null) foreach (var item in saved) _links[item.Key] = item.Value;
            }
            else File.WriteAllText(_configPath, JsonSerializer.Serialize(_links, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { StatusText.Text = "فایل میانبرها قابل خواندن نیست؛ مقادیر پیش‌فرض استفاده شد."; }
    }

    private string GetLink(string key) => _links.TryGetValue(key, out var url) ? url : DefaultHomeUrl;

    private void UpdateDateTime()
    {
        var now = DateTime.Now;
        ClockText.Text = now.ToString("HH:mm:ss");
        var pc = new PersianCalendar();
        string[] days = { "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه" };
        string[] months = { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
        PersianDateText.Text = $"{days[(int)now.DayOfWeek]} {pc.GetDayOfMonth(now)} {months[pc.GetMonth(now)-1]} {pc.GetYear(now)}";
    }

    private void Navigate(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) url = "https://" + url;
        try { Browser.Source = new Uri(url); } catch { StatusText.Text = "آدرس واردشده معتبر نیست."; }
    }

    private void Shortcut_Click(object sender, RoutedEventArgs e) => Navigate(GetLink((sender as Button)?.Tag?.ToString() ?? "home"));
    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("notepad.exe", _configPath) { UseShellExecute = true }); StatusText.Text = "لینک‌ها را در shortcuts.json ویرایش کنید و برنامه را دوباره اجرا کنید."; }
        catch { StatusText.Text = "باز کردن تنظیمات میانبرها ناموفق بود."; }
    }
    private void Home_Click(object sender, RoutedEventArgs e) => Navigate(GetLink("home"));
    private void Refresh_Click(object sender, RoutedEventArgs e) => Browser.Reload();
    private void Back_Click(object sender, RoutedEventArgs e) { if (Browser.CanGoBack) Browser.GoBack(); }
    private void Forward_Click(object sender, RoutedEventArgs e) { if (Browser.CanGoForward) Browser.GoForward(); }
    private void Go_Click(object sender, RoutedEventArgs e) => Navigate(AddressBox.Text);
    private void AddressBox_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) Navigate(AddressBox.Text); }
    private void Browser_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e) { AddressBox.Text = e.Uri; StatusText.Text = "در حال بارگذاری…"; }
    private void Browser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e) { StatusText.Text = e.IsSuccess ? "صفحه آماده است" : $"خطا در بارگذاری: {e.WebErrorStatus}"; AddressBox.Text = Browser.Source?.ToString() ?? ""; }
}