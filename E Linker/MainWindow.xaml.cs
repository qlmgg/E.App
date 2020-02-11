﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SharedProject;
using MessageBox = System.Windows.MessageBox;
using Settings = E.Linker.Properties.Settings;

namespace E.Linker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 属性
        /// <summary>
        /// 应用信息
        /// </summary>
        private AppInfo AppInfo { get; } = new AppInfo();

        /// <summary>
        /// 当前菜单
        /// </summary>
        private MenuTool CurrentMenuTool { get; set; } = MenuTool.文件;

        #endregion 


        #region 方法
        //构造
        /// <summary>
        /// 构造器
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Move.MouseLeftButtonDown += Node_MouseLeftButtonDown;
            Move.MouseLeftButtonUp += Node_MouseLeftButtonUp;
            Move.MouseMove += Node_MouseMove;
        }

        //载入
        /// <summary>
        /// 读取记录
        /// </summary>
        private void LoadRecordItems()
        {
            if (!string.IsNullOrEmpty(Settings.Default.Record))
            {
                string[] _myB = Regex.Split(Settings.Default.Record, "///");
                foreach (string b in _myB)
                {
                    AddRecordItem(b);
                }
            }
        }

        ///打开

        ///关闭

        //保存
        /// <summary>
        /// 保存应用设置
        /// </summary>
        private void SaveSettings()
        {
            Settings.Default.Save();
            ShowMessage(FindResource("已保存").ToString());
        }
        /// <summary>
        /// 保存记录
        /// </summary>
        private void SaveRecords()
        {
            Settings.Default.Record = "";
            List<string> strs = new List<string>();
            foreach (ListBoxItem item in LtbRecord.Items)
            {
                strs.Add(item.Content.ToString());
            }
            Settings.Default.Record = string.Join("///", strs);
        }

        //创建

        //添加
        private void AddRecordItem(string content)
        {
            ListBoxItem item = new ListBoxItem
            {
                FontSize = 20,
                Content = content,
                Style = (Style)FindResource("列表子项样式")
            };
            LtbRecord.Items.Add(item);
        }

        ///移除

        ///清空

        ///删除

        ///获取


        //设置
        /// <summary>
        /// 设置菜单
        /// </summary>
        /// <param name="menu"></param>
        private void SetMenuTool(MenuTool menu)
        {
            switch (menu)
            {
                case MenuTool.无:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.文件:
                    PanFile.Visibility = Visibility.Visible;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(4, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.编辑:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Visible;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(4, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.设置:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Visible;
                    PanAbout.Visibility = Visibility.Collapsed;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(4, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
                    break;
                case MenuTool.关于:
                    PanFile.Visibility = Visibility.Collapsed;
                    //PanEdit.Visibility = Visibility.Collapsed;
                    PanSetting.Visibility = Visibility.Collapsed;
                    PanAbout.Visibility = Visibility.Visible;
                    BtnFile.BorderThickness = new Thickness(0, 0, 0, 0);
                    //BtnEdit.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
                    BtnAbout.BorderThickness = new Thickness(4, 0, 0, 0);
                    break;
                default:
                    break;
            }
            CurrentMenuTool = menu;
        }
        /// <summary>
        /// 设置语言选项
        /// </summary>
        /// <param name="language">语言简拼</param>
        private void SetLanguage(int index)
        {
            Settings.Default.Language = index;
        }
        /// <summary>
        /// 设置主题选项
        /// </summary>
        /// <param name="themePath">主题路径</param>
        private void SetTheme(int index)
        {
            Settings.Default.Theme = index;
        }
        /// <summary>
        /// 切换下个主题显示
        /// </summary>
        private void SetNextTheme()
        {
            int index = Settings.Default.Theme;
            index++;
            if (index > CbbThemes.Items.Count - 1)
            {
                index = 0;
            }
            SetTheme(index);
        }

        //重置
        /// <summary>
        /// 重置应用设置
        /// </summary>
        private void ResetSettings()
        {
            Settings.Default.Reset();
            ShowMessage(FindResource("已重置").ToString());
        }

        ///选择

        //检查
        /// <summary>
        /// 用户是否同意
        /// </summary>
        /// <returns></returns>
        private bool IsUserAgree()
        {
            string str = AppInfo.UserAgreement + "\n\n你需要同意此协议才能使用本软件，是否同意？";
            MessageBoxResult result = MessageBox.Show(str, FindResource("用户协议").ToString(), MessageBoxButton.YesNo);
            return (result == MessageBoxResult.Yes);
        }
        /// <summary>
        /// 检查用户协议
        /// </summary>
        private void CheckUserAgreement()
        {
            Settings.Default.RunCount += 1;
            if (Settings.Default.RunCount == 1)
            {
                if (!IsUserAgree())
                {
                    Settings.Default.RunCount = 0;
                    Close();
                }
            }
        }
        /// <summary>
        /// 检测名字是否合法字符
        /// </summary>
        /// <param name="name">名字</param>
        /// <returns>是否合法字符</returns>

        //刷新
        /// <summary>
        /// 刷新软件信息
        /// </summary>
        private void RefreshAppInfo()
        {
            TxtHomePage.Text = AppInfo.HomePage;
            TxtHomePage.ToolTip = AppInfo.HomePage;
            TxtGitHubPage.Text = AppInfo.GitHubPage;
            TxtGitHubPage.ToolTip = AppInfo.GitHubPage;
            TxtQQGroup.Text = AppInfo.QQGroupNumber;
            TxtQQGroup.ToolTip = AppInfo.QQGroupLink;
            TxtBitCoinAddress.Text = AppInfo.BitCoinAddress;
            TxtBitCoinAddress.ToolTip = AppInfo.BitCoinAddress;

            TxtThisName.Text = AppInfo.Name;
            TxtDescription.Text = AppInfo.Description;
            TxtDeveloper.Text = AppInfo.Company;
            TxtVersion.Text = AppInfo.Version.ToString();
            TxtUpdateNote.Text = AppInfo.UpdateNote;
        }
        /// <summary>
        /// 刷新主窗口标题
        /// </summary>
        public void RefreshTitle()
        {
            string str = AppInfo.Name + " " + AppInfo.VersionShort;
            Main.Title = str;
        }

        //显示
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="resourceName">资源名</param>
        /// <param name="newBox">是否弹出对话框</param>
        private void ShowMessage(string message, bool newBox = false)
        {
            MessageHelper.ShowMessage(LblMessage, message, newBox);
        }

        //切换
        /// <summary>
        /// 切换工具面板
        /// </summary>
        private void SwitchMenuToolFile()
        {
            switch (CurrentMenuTool)
            {
                case MenuTool.文件:
                    SetMenuTool(MenuTool.无);
                    break;
                default:
                    SetMenuTool(MenuTool.文件);
                    break;
            }
        }
        /// <summary>
        /// 切换编辑面板
        /// </summary>
        private void SwitchMenuToolEdit()
        {
            switch (CurrentMenuTool)
            {
                case MenuTool.编辑:
                    SetMenuTool(MenuTool.无);
                    break;
                default:
                    SetMenuTool(MenuTool.编辑);
                    break;
            }
        }
        /// <summary>
        /// 切换设置面板
        /// </summary>
        private void SwitchMenuToolSetting()
        {
            switch (CurrentMenuTool)
            {
                case MenuTool.设置:
                    SetMenuTool(MenuTool.无);
                    break;
                default:
                    SetMenuTool(MenuTool.设置);
                    break;
            }
        }
        /// <summary>
        /// 切换关于面板
        /// </summary>
        private void SwitchMenuToolAbout()
        {
            switch (CurrentMenuTool)
            {
                case MenuTool.关于:
                    SetMenuTool(MenuTool.无);
                    break;
                default:
                    SetMenuTool(MenuTool.关于);
                    break;
            }
        }
        #endregion


        #region 事件
        //主窗口
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            //载入
            LanguageHelper.LoadLanguageItems(CbbLanguages);
            ThemeHelper.LoadThemeItems(CbbThemes);
            LoadRecordItems();

            //刷新
            RefreshAppInfo();
            RefreshTitle();

            //检查用户协议
            CheckUserAgreement();
        }
        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveRecords();
            SaveSettings();
        }
        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            //Ctrl+T 切换下个主题
            if (e.Key == Key.T && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { SetNextTheme(); }

            //关于菜单
            if (e.Key == Key.F1)
            { Process.Start("explorer.exe", AppInfo.HomePage); }
            else if (e.Key == Key.F2)
            { Process.Start("explorer.exe", AppInfo.GitHubPage); }
            else if (e.Key == Key.F3)
            { Process.Start("explorer.exe", AppInfo.QQGroupLink); }
        }

        //菜单栏
        private void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolFile();
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolEdit();
        }
        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolSetting();
        }
        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            SwitchMenuToolAbout();
        }

        //工具栏
        /// 文件
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            LtbRecord.Items.Clear();
        }
        ///编辑
        ///设置
        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
        private void BtnResetSettings_Click(object sender, RoutedEventArgs e)
        {
            ResetSettings();
        }
        private void CbbLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbbLanguages.SelectedItem != null)
            {
                ComboBoxItem cbi = CbbLanguages.SelectedItem as ComboBoxItem;
                if (cbi.Tag is ResourceDictionary rd)
                {
                    //主窗口更改语言
                    if (Resources.MergedDictionaries.Count > 0)
                    {
                        Resources.MergedDictionaries.Clear();
                    }
                    Resources.MergedDictionaries.Add(rd);
                }
                else
                {
                    CbbLanguages.Items.Remove(cbi);
                    //设为默认主题
                    SetLanguage(0);
                }
            }
        }
        private void CbbThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbbThemes.SelectedItem != null)
            {
                ComboBoxItem cbi = CbbThemes.SelectedItem as ComboBoxItem;
                string themePath = cbi.ToolTip.ToString();
                if (File.Exists(themePath))
                {
                    ColorHelper.SetColors(Resources, themePath);
                }
                else
                {
                    CbbThemes.Items.Remove(cbi);
                    //设为默认主题
                    SetTheme(0);
                }
            }
        }
        ///关于
        private void BtnBitCoinAddress_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetDataObject(TxtBitCoinAddress.Text, true);
            ShowMessage(FindResource("已复制").ToString());
        }
        private void BtnHomePage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.HomePage);
        }
        private void BtnGitHubPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.GitHubPage);
        }
        private void BtnQQGroup_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.QQGroupLink);
        }

        #endregion




        Point point = new Point();
        private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Move.ReleaseMouseCapture();
        }

        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            point = e.GetPosition(null);
            Move.CaptureMouse();
            Move.Cursor = Cursors.Hand;
        }

        private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double dx = e.GetPosition(null).X - point.X + Node.Margin.Left;
                double dy = e.GetPosition(null).Y - point.Y + Node.Margin.Top;
                Node.Margin = new Thickness(dx, dy, 0, 0);
                point = e.GetPosition(null);
            }
        }
    }
}
