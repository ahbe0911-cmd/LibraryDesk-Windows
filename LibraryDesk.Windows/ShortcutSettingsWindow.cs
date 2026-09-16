using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LibraryDesk.Windows;

public sealed class ShortcutSettingsWindow : Window
{
    private readonly string _path;
    private readonly Dictionary<string, TextBox> _boxes = new();
    private readonly (string Key, string Title)[] _items =
    {
        ("home","صفحه اصلی"),("member","ثبت عضو جدید"),("documents","وضعیت مدارک"),
        ("loan","ثبت امانت"),("return","بازگشت کتاب"),("book","ثبت کتاب جدید"),("culture","برنامه‌های فرهنگی")
    };

    public ShortcutSettingsWindow(string path, IReadOnlyDictionary<string,string> links)
    {
        _path = path;
        Title = "تنظیم لینک‌های میانبر";
        Width = 720; Height = 600; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = new SolidColorBrush(Color.FromRgb(7,20,38)); FlowDirection = FlowDirection.RightToLeft;
        var root = new Grid { Margin = new Thickness(24) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.Children.Add(new TextBlock { Text = "تنظیم آدرس سرویس‌ها", Foreground = Brushes.White, FontSize = 24, FontWeight = FontWeights.Bold, Margin = new Thickness(0,0,0,18) });
        var panel = new StackPanel(); Grid.SetRow(panel,1); root.Children.Add(panel);
        foreach (var item in _items)
        {
            panel.Children.Add(new TextBlock { Text = item.Title, Foreground = new SolidColorBrush(Color.FromRgb(190,215,235)), Margin = new Thickness(0,7,0,4) });
            var box = new TextBox { Text = links.TryGetValue(item.Key, out var value) ? value : "", Height = 36, Padding = new Thickness(8), FlowDirection = FlowDirection.LeftToRight, VerticalContentAlignment = VerticalAlignment.Center };
            _boxes[item.Key] = box; panel.Children.Add(box);
        }
        var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0,18,0,0) }; Grid.SetRow(actions,2); root.Children.Add(actions);
        var save = new Button { Content = "ذخیره تنظیمات", Width = 140, Height = 40, Margin = new Thickness(6), Background = new SolidColorBrush(Color.FromRgb(15,118,110)), Foreground = Brushes.White, BorderThickness = new Thickness(0) };
        save.Click += Save_Click; actions.Children.Add(save);
        var cancel = new Button { Content = "انصراف", Width = 100, Height = 40, Margin = new Thickness(6) }; cancel.Click += (_,_) => Close(); actions.Children.Add(cancel);
        Content = root;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var data = new Dictionary<string,string>();
        foreach (var item in _items)
        {
            var value = _boxes[item.Key].Text.Trim();
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            { MessageBox.Show($"آدرس «{item.Title}» معتبر نیست.", "بررسی آدرس", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            data[item.Key] = value;
        }
        File.WriteAllText(_path, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        DialogResult = true; Close();
    }
}