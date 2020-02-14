﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace E.Updater
{
    /// <summary>
    /// AppInfo.xaml 的交互逻辑
    /// </summary>
    public partial class AppInfoItem : UserControl
    {
        public AppInfoItem()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty AppNameProperty = DependencyProperty.Register("AppName", typeof(string), typeof(AppInfoItem), new PropertyMetadata("App Name", new PropertyChangedCallback(OnAppNameChanged)));
        public static readonly DependencyProperty AppDescriptionProperty = DependencyProperty.Register("AppDescription", typeof(string), typeof(AppInfoItem), new PropertyMetadata("App Description", new PropertyChangedCallback(OnAppDescriptionChanged)));
        public static readonly DependencyProperty AppVersionProperty = DependencyProperty.Register("AppVersion", typeof(string), typeof(AppInfoItem), new PropertyMetadata("App Version", new PropertyChangedCallback(OnAppVersionChanged)));
        public static readonly DependencyProperty AppStateProperty = DependencyProperty.Register("AppState", typeof(string), typeof(AppInfoItem), new PropertyMetadata("App State", new PropertyChangedCallback(OnAppStateChanged)));
        public static readonly DependencyProperty AppIconProperty = DependencyProperty.Register("AppIcon", typeof(string), typeof(AppInfoItem), new PropertyMetadata("App Icon", new PropertyChangedCallback(OnAppIconChanged)));
       
        public string AppName
        {
            get { return (string)GetValue(AppNameProperty); }
            set { SetValue(AppNameProperty, value); }
        }
        public string AppDescription
        {
            get { return (string)GetValue(AppDescriptionProperty); }
            set { SetValue(AppDescriptionProperty, value); }
        }
        public string AppVersion
        {
            get { return (string)GetValue(AppVersionProperty); }
            set { SetValue(AppVersionProperty, value); }
        }
        public string AppState
        {
            get { return (string)GetValue(AppStateProperty); }
            set { SetValue(AppStateProperty, value); }
        }
        public string AppIcon
        {
            get { return (string)GetValue(AppIconProperty); }
            set { SetValue(AppIconProperty, value); }
        }

        private static void OnAppNameChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            AppInfoItem source = (AppInfoItem)sender;
            source.LblName.Content = (string)args.NewValue;
        }
        private static void OnAppDescriptionChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            AppInfoItem source = (AppInfoItem)sender;
            source.LblDescription.Content = (string)args.NewValue;
        }
        private static void OnAppVersionChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            AppInfoItem source = (AppInfoItem)sender;
            source.LblVersion.Content = (string)args.NewValue;
        }
        private static void OnAppStateChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            AppInfoItem source = (AppInfoItem)sender;
            source.LblState.Content = (string)args.NewValue;
        }
        private static void OnAppIconChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            AppInfoItem source = (AppInfoItem)sender;
            source.ImgIcon.Source = new BitmapImage(new Uri((string)args.NewValue, UriKind.Relative));
        }

        private void PanBtns_Loaded(object sender, RoutedEventArgs e)
        {
            PanBtns.Visibility = Visibility.Collapsed;
        }
        private void PanAppInfo_MouseEnter(object sender, MouseEventArgs e)
        {
            PanAppInfo.Background = (Brush)FindResource("三级背景颜色");
            PanBtns.Visibility = Visibility.Visible;
        }
        private void PanAppInfo_MouseLeave(object sender, MouseEventArgs e)
        {
            PanAppInfo.Background = (Brush)FindResource("二级背景颜色");
            PanBtns.Visibility = Visibility.Collapsed;
        }

        private void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Install(AppName);
        }
        private void BtnUnInstall_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).UnInstall(AppName);
        }
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Update(AppName);
        }
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Browse(AppName);
        }
        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Run(AppName);
        }
        private void BtnKill_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).Kill(AppName);
        }
    }
}