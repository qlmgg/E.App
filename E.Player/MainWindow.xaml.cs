﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms.Integration;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;
using System.Reflection;
using System.Drawing.Imaging;
using Tags.ID3;

using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.MessageBox;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using DragEventArgs = System.Windows.DragEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Path = System.IO.Path;

using Settings = E.Player.Properties.Settings;
using User = E.Player.Properties.User;
using E.Utility;

namespace E.Player
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
        private AppInfo AppInfo { get; set; }

        /// <summary>
        /// 语言列表
        /// </summary>
        private List<ItemInfo> LanguageItems { get; set; } = new List<ItemInfo>();
        /// <summary>
        /// 主题集合
        /// </summary>
        private List<TextBlock> ThemeItems { get; set; } = new List<TextBlock>();

        /// <summary>
        /// 播放中的媒体
        /// </summary>
        private FileOrFolderInfo PlayingMedia { get; set; }

        /// <summary>
        /// 是否正在播放
        /// </summary>
        private bool IsPlaying { get; set; } = false;
        /// <summary>
        /// 是否全屏
        /// </summary>
        private bool IsFullScreen { get; set; } = false;
        /// <summary>
        /// 是否有循环开始点
        /// </summary>
        private bool HasStartPosition { get; set; } = false;
        /// <summary>
        /// 是否有循环结束点
        /// </summary>
        private bool HasEndPosition { get; set; } = false;
        /// <summary>
        /// 循环的开始
        /// </summary>
        private TimeSpan StartPosition { get; set; } = TimeSpan.Zero;
        /// <summary>
        /// 循环的结束
        /// </summary>
        private TimeSpan EndPosition { get; set; } = TimeSpan.Zero;
        /// <summary>
        /// 全屏前坐标尺寸
        /// </summary>
        private double[] LastRect = new double[4];

        //计时器
        private DispatcherTimer timer1;
        private DispatcherTimer timer2;
        #endregion

        //构造
        /// <summary>
        /// 默认构造器
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        //载入
        /// <summary>
        /// 载入应用信息
        /// </summary>
        private void LoadAppInfo()
        {
            AssemblyProductAttribute product = (AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute));
            AssemblyDescriptionAttribute description = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute));
            AssemblyCompanyAttribute company = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCompanyAttribute));
            AssemblyCopyrightAttribute copyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute));
            Stream src = System.Windows.Application.GetResourceStream(new Uri("/文档/更新日志.txt", UriKind.Relative)).Stream;
            string updateNote = new StreamReader(src, Encoding.UTF8).ReadToEnd();
            string homePage = "http://estar.zone";
            string infoPage = "http://estar.zone/introduction/e-player/";
            string downloadPage = "http://estar.zone/introduction/e-player/";
            string gitHubPage = "https://github.com/HelloEStar/E.App";
            string qqGroupLink = "http://jq.qq.com/?_wv=1027&k=5TQxcvR";
            string qqGroupNumber = "279807070";
            string bitCoinAddress = "19LHHVQzWJo8DemsanJhSZ4VNRtknyzR1q";
            AppInfo = new AppInfo(product.Product, description.Description, company.Company, copyright.Copyright, new Version(Application.ProductVersion), updateNote,
                                  homePage, infoPage, downloadPage, gitHubPage, qqGroupLink, qqGroupNumber, bitCoinAddress);
        }
        /// <summary>
        /// 载入偏好设置
        /// </summary>
        private void LoadSettings()
        {
            //是否在切换视频时保持变换
            CkbKeepTrans.IsChecked = User.Default.isKeepTrans;
            //是否在退出时保留播放列表
            CkbSavePlayList.IsChecked = User.Default.isSavePlaylist;
            //快进快退时间
            JumpTime.Text = User.Default.jumpTime.ToString();

            //刷新选中项
            SelectLanguageItem(User.Default.language);
            SelectThemeItem(User.Default.ThemePath);
            SelectPlayModeItem(User.Default.playMode);
        }
        /// <summary>
        /// 创建语言选项
        /// </summary>
        private void LoadLanguageItems()
        {
            LanguageItems.Clear();
            ItemInfo zh_CN = new ItemInfo("中文（默认）", "zh_CN");
            ItemInfo en_US = new ItemInfo("English", "en_US");
            LanguageItems.Add(zh_CN);
            LanguageItems.Add(en_US);

            //绑定数据，真正的赋值
            CbbLanguages.ItemsSource = LanguageItems;
            CbbLanguages.DisplayMemberPath = "Name";
            CbbLanguages.SelectedValuePath = "Value";
        }
        /// <summary>
        /// 载入所有可用主题
        /// </summary>
        private void LoadThemeItems()
        {
            //创建皮肤文件夹
            if (!Directory.Exists(User.Default.ThemesDir))
            { Directory.CreateDirectory(User.Default.ThemesDir); }

            string[] _mySkins = Directory.GetFiles(User.Default.ThemesDir);
            ThemeItems.Clear();
            foreach (string s in _mySkins)
            {
                string tmp = Path.GetExtension(s);
                if (tmp == ".ini" || tmp == ".INI")
                {
                    string tmp2 = INIOperator.ReadIniKeys("文件", "类型", s);
                    //若是主题配置文件
                    if (tmp2 == "主题")
                    {
                        string tmp3 = INIOperator.ReadIniKeys("文件", "版本", s);
                        if (tmp3 == AppInfo.Version.ToString())
                        {
                            TextBlock theme = new TextBlock
                            {
                                Text = Path.GetFileNameWithoutExtension(s),
                                ToolTip = s
                            };
                            ThemeItems.Add(theme);
                        }
                    }
                }
            }

            CbbThemes.Items.Clear();
            foreach (TextBlock item in ThemeItems)
            {
                TextBlock theme = new TextBlock()
                {
                    Text = item.Text,
                    ToolTip = item.ToolTip
                };
                CbbThemes.Items.Add(theme);
            }
        }
        /// <summary>
        /// 载入媒体记录
        /// </summary>
        private void LoadVideos()
        {
            //读取一个字符串，并加入播放列表
            if (!string.IsNullOrEmpty(User.Default._medias))
            {
                string[] _myB = Regex.Split(User.Default._medias, "///");
                foreach (var b in _myB)
                {
                    AddPlayListItem(new FileOrFolderInfo(b), false);
                }
            }
        }

        ///打开

        ///关闭

        //保存
        /// <summary>
        /// 保存播放列表
        /// </summary>
        private void SavePlaylist()
        {
            User.Default._medias = "";
            if (LtbMedia.Items.Count > 0)
            {
                List<string> medias = new List<string>();
                foreach (ListBoxItem item in LtbMedia.Items)
                {
                    medias.Add(item.Tag.ToString());
                }
                User.Default._medias = string.Join("///", medias);
            }
        }

        //创建
        /// <summary>
        /// 创建计时器
        /// </summary>
        private void CreateTimer()
        {
            Loaded += new RoutedEventHandler(Timer1_Tick);
            Loaded += new RoutedEventHandler(Timer2_Tick);
            //设置计时器1,每秒触发一次
            timer1 = new DispatcherTimer
            { Interval = TimeSpan.FromSeconds(1) };
            timer1.Tick += new EventHandler(Timer1_Tick);
            timer1.Start();
            //设置计时器2,每0.01秒触发一次
            timer2 = new DispatcherTimer
            { Interval = TimeSpan.FromSeconds(0.02) };
            timer2.Tick += new EventHandler(Timer2_Tick);
            timer2.Start();
        }
        /// <summary>
        /// 创建颜色
        /// </summary>
        /// <param name="text">ARGB色值，以点号分隔，0-255</param>
        /// <returns></returns>
        private static Color CreateColor(string text)
        {
            //MessageBox.Show(text);
            try
            {
                string[] colors = text.Split('.');
                byte red = byte.Parse(colors[0]);
                byte green = byte.Parse(colors[1]);
                byte blue = byte.Parse(colors[2]);
                byte alpha = byte.Parse(colors[3]);
                Color color = Color.FromArgb(alpha, red, green, blue);
                return color;
            }
            catch (Exception)
            {
                Color color = Color.FromArgb(255, 125, 125, 125);
                return color;
            }
        }

        //添加
        /// <summary>
        /// 创建播放列表媒体元素
        /// </summary>
        /// <param name="media">媒体路径</param>
        /// <param name="isPlay">是否直接播放</param>
        private void AddPlayListItem(FileOrFolderInfo media, bool isPlay)
        {
            if (media != null)
            {
                ListBoxItem PlayListBoxItem = new ListBoxItem
                {
                    Name = "VideoItem",
                    Tag = media.Path,
                    ToolTip = media.Path,
                    Content = media.Name,
                    Style = (Style)FindResource("列表子项样式")
                };
                //添加鼠标事件
                PlayListBoxItem.AddHandler(MouseDownEvent, new MouseButtonEventHandler(Item_Play_MouseDown), true);

                //添加Item时检测是否有重复视频
                bool isThere = false;
                if (LtbMedia.Items.Count == 0)
                {
                    //增加第一个Item进PlayListBox
                    LtbMedia.Items.Add(PlayListBoxItem);
                    LtbMedia.ToolTip = FindResource("媒体总数") + "：" + LtbMedia.Items.Count;
                    LtbMedia.SelectedIndex = 0;
                    if (isPlay == true)
                    {
                        PlayNewVideo(media);
                    }
                }
                else
                {
                    ListBoxItem tempItem = new ListBoxItem();
                    for (int i = 0; i < LtbMedia.Items.Count; i++)
                    {
                        tempItem = LtbMedia.Items.GetItemAt(i) as ListBoxItem;
                        if (media.Path == tempItem.Tag.ToString())
                        {
                            isThere = true;
                            break;
                        }
                        else
                        {
                            isThere = false;
                        }
                    }

                    if (isThere == false)
                    {
                        //增加第一个Item进PlayListBox
                        LtbMedia.Items.Add(PlayListBoxItem);
                        LtbMedia.ToolTip = FindResource("媒体总数") + "：" + LtbMedia.Items.Count;
                        LtbMedia.SelectedItem = PlayListBoxItem;
                        if (isPlay == true)
                        {
                            PlayNewVideo(media);
                        }
                    }
                    else
                    {
                        ShowMessage(FindResource("视频重复").ToString(), true);
                    }
                }
            }
        }

        //移除
        /// <summary>
        /// 删除媒体
        /// </summary>
        private void RemoveMedia()
        {
            //获取当前选择的Item在PlayListBox的索引
            ListBoxItem tempItem;
            tempItem = LtbMedia.SelectedItem as ListBoxItem;
            if (tempItem != null)
            {
                if (PlayingMedia != null)
                {
                    if (PlayingMedia.Path == tempItem.Tag.ToString())
                    {
                        Media.Stop();
                        Media.Source = null;
                        Cover.Source = null;
                        Cover.IsEnabled = false;
                        IsPlaying = false;
                        PlayingMedia = null;

                        TimeAll.Content = "00:00:00";
                        BtnPlay.Content = FindResource("播放");

                        RefreshBtnsState();
                        RefreshTitle();
                        ShowMessage(FindResource("删除视频").ToString());
                    }
                }
                int s = LtbMedia.SelectedIndex;
                LtbMedia.Items.Remove(LtbMedia.SelectedItem);
                LtbMedia.SelectedIndex = s;
                if (s == LtbMedia.Items.Count - 1)
                {
                    LtbMedia.SelectedIndex = s - 1;
                }
                LtbMedia.ToolTip = FindResource("媒体总数") + "：" + LtbMedia.Items.Count;
            }
            else
            {
                ShowMessage(FindResource("未选择视频").ToString());
            }
        }
        /// <summary>
        /// 清除循环段
        /// </summary>
        private void RemoveLoop()
        {
            if (HasEndPosition == true || HasStartPosition == true)
            {
                StartPosition = TimeSpan.Zero;
                EndPosition = TimeSpan.Zero;
                HasStartPosition = false;
                HasEndPosition = false;

                //滑块高亮区域
                SldTime.SelectionStart = 0;
                SldTime.SelectionEnd = 0;
                //显示消息
                ShowMessage(FindResource("已清除循环").ToString());
            }
        }

        //清空
        /// <summary>
        /// 清空媒体
        /// </summary>
        private void ClearMedia()
        {
            Media.Stop();
            Media.Source = null;
            PlayingMedia = null;
            Cover.Source = null;
            Cover.IsEnabled = false;
            IsPlaying = false;

            TimeAll.Content = "00:00:00";
            BtnPlay.Content = FindResource("播放");

            RefreshBtnsState();
            RefreshTitle();

            LtbMedia.Items.Clear();

            ShowMessage(FindResource("已清空媒体列表").ToString());
        }

        //获取
        /// <summary>
        /// 获取媒体名字
        /// </summary>
        /// <param name="path">媒体路径</param>
        /// <returns>媒体名称</returns>
        private string GetVideoName(string path)
        {
            string _videoName = Path.GetFileName(path);
            return _videoName;
        }
        /// <summary>
        /// 浏览文件获取路径
        /// </summary>
        /// <returns>媒体路径</returns>
        private string GetFilePath()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Video File(*.avi;*.mp4;*.mkv;*.wmv;*.wma;*.wav;*.mp3;*.aac;*.flac)|" +
                                    "*.avi;*.mp4;*.mkv;*.wmv;*.wma;*.wav;*.mp3;*.aac;*.flac|" +
                         "All File(*.*)|" +
                                  "*.*"
            };
            dialog.ShowDialog();
            return dialog.FileName;
        }
        /// <summary>
        /// 由地址返回图片，返回类型为System.Drawing.Image
        /// </summary>
        /// <param name="MP3path">MP3全地址</param>
        /// <returns></returns>
        private List<BitmapImage> GetMP3Cover(string MP3path)
        {
            try
            {
                List<BitmapImage> list_bitmapimg = new List<BitmapImage>();
                ID3Info File = new ID3Info(MP3path, true);

                foreach (Tags.ID3.ID3v2Frames.BinaryFrames.AttachedPictureFrame F in File.ID3v2Info.AttachedPictureFrames)
                {
                    System.Drawing.Image imgWinForms = F.Picture; // 此段代码用于将获得的System.Drawing.Image类型转化为
                    BitmapImage bi = new BitmapImage();                     //WPF中Image控件可以接受的内容
                    float scale = imgWinForms.Height / imgWinForms.Width;
                    bi.BeginInit();
                    MemoryStream ms = new MemoryStream();
                    imgWinForms.Save(ms, ImageFormat.Bmp);
                    ms.Seek(0, SeekOrigin.Begin);
                    bi.StreamSource = ms;
                    bi.EndInit();
                    list_bitmapimg.Add(bi);
                }
                return list_bitmapimg;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        //设置
        /// <summary>
        /// 载入语言
        /// </summary>
        /// <param name="language">语言简拼</param>
        private void SetLanguage(string language)
        {
            try
            {
                ResourceDictionary langRd = null;
                langRd = System.Windows.Application.LoadComponent(new Uri(@"语言\" + language + ".xaml", UriKind.Relative)) as ResourceDictionary;
                if (langRd != null)
                {
                    //主窗口更改语言
                    if (Resources.MergedDictionaries.Count > 0)
                    {
                        Resources.MergedDictionaries.Clear();
                    }
                    Resources.MergedDictionaries.Add(langRd);
                    User.Default.language = language;
                    User.Default.Save();
                }
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message);
            }
        }
        /// <summary>
        /// 切换主题显示
        /// </summary>
        private void SetTheme(string themePath)
        {
            foreach (TextBlock theme in ThemeItems)
            {
                if (theme.ToolTip.ToString() == themePath)
                {
                    if (File.Exists(themePath))
                    {
                        SetSkin(themePath);
                        User.Default.ThemePath = themePath;
                    }
                    else
                    {
                        ThemeItems.Remove(theme);
                        //设为默认主题
                        User.Default.ThemePath = User.Default.ThemePath;
                        SetSkin(User.Default.ThemePath);
                        ShowMessage(FindResource("偏好主题的不存在").ToString());
                    }
                    User.Default.Save();
                    break;
                }
            }
        }
        /// <summary>
        /// 设置播放模式
        /// </summary>
        /// <param name="mode">模式</param>
        /// <param name="showMessage">是否显示消息</param>
        private void SetPlayMode(int mode)
        {
            User.Default.playMode = mode;
            User.Default.Save();
        }
        /// <summary>
        /// 切换下个主题显示
        /// </summary>
        private void SetNextTheme()
        {
            foreach (TextBlock theme in ThemeItems)
            {
                if (theme.ToolTip.ToString() == User.Default.ThemePath)
                {
                    int themeOrder = ThemeItems.IndexOf(theme);
                    int themeCounts = ThemeItems.Count;
                    if (themeOrder + 1 < themeCounts)
                    { themeOrder += 1; }
                    else
                    { themeOrder = 0; }
                    if (File.Exists(ThemeItems[themeOrder].ToolTip.ToString()))
                    {
                        //设为此主题
                        User.Default.ThemePath = ThemeItems[themeOrder].ToolTip.ToString();
                        User.Default.Save();
                        SetSkin(User.Default.ThemePath);
                    }
                    else
                    {
                        ShowMessage(FindResource("下一个主题的配置文件不存在").ToString());
                        ThemeItems.Remove(ThemeItems[themeOrder]);
                    }
                    break;
                }
            }

        }
        /// <summary>
        /// 重置主题颜色
        /// </summary>
        /// <param name="themePath">主题文件路径</param>
        private void SetSkin(string themePath)
        {
            SetColor("一级字体颜色", CreateColor(INIOperator.ReadIniKeys("字体", "一级字体", themePath)));
            SetColor("二级字体颜色", CreateColor(INIOperator.ReadIniKeys("字体", "二级字体", themePath)));
            SetColor("三级字体颜色", CreateColor(INIOperator.ReadIniKeys("字体", "三级字体", themePath)));

            SetColor("一级背景颜色", CreateColor(INIOperator.ReadIniKeys("背景", "一级背景", themePath)));
            SetColor("二级背景颜色", CreateColor(INIOperator.ReadIniKeys("背景", "二级背景", themePath)));
            SetColor("三级背景颜色", CreateColor(INIOperator.ReadIniKeys("背景", "三级背景", themePath)));

            SetColor("一级边框颜色", CreateColor(INIOperator.ReadIniKeys("边框", "一级边框", themePath)));

            SetColor("有焦点选中颜色", CreateColor(INIOperator.ReadIniKeys("高亮", "有焦点选中", themePath)));
            SetColor("无焦点选中颜色", CreateColor(INIOperator.ReadIniKeys("高亮", "无焦点选中", themePath)));
        }
        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="colorName"></param>
        /// <param name="c"></param>
        public void SetColor(string colorName, Color c)
        {
            Resources.Remove(colorName);
            Resources.Add(colorName, new SolidColorBrush(c));
        }
        /// <summary>
        /// 设置循环段开始
        /// </summary>
        private void SetLoopStart()
        {
            //获取当前位置
            TimeSpan tempPosition = Media.Position;
            if (tempPosition < EndPosition || HasEndPosition == false)
            {
                StartPosition = tempPosition;
                string str = StartPosition.ToString();
                //滑块高亮区域
                SldTime.SelectionStart = StartPosition.TotalSeconds;
                HasStartPosition = true;

                //显示消息
                ShowMessage(FindResource("循环始于").ToString() + "：" + str.Substring(0, 8));
            }
            else
            {
                //显示消息
                ShowMessage(FindResource("开始点应靠前").ToString());
            }
        }
        /// <summary>
        /// 设置循环段结束
        /// </summary>
        private void SetLoopEnd()
        {
            //获取当前位置
            TimeSpan tempPosition = Media.Position;
            if (StartPosition < tempPosition)
            {
                EndPosition = tempPosition;
                string str = EndPosition.ToString();
                //滑块高亮区域
                SldTime.SelectionEnd = EndPosition.TotalSeconds;
                HasEndPosition = true;

                //显示消息
                ShowMessage(FindResource("循环终于").ToString() + "：" + str.Substring(0, 8));
            }
            else
            {
                //显示消息
                ShowMessage(FindResource("结束点应靠后").ToString());
            }
        }

        //重置
        /// <summary>
        /// 重置应用设置
        /// </summary>
        private void ResetAppSettings()
        {
            Settings.Default.Reset();
        }
        /// <summary>
        /// 重置用户设置
        /// </summary>
        private void ResetUserSettings()
        {
            User.Default.Reset();
        }
        /// <summary>
        /// 翻转归位
        /// </summary>
        private void ResetFlip()
        {
            User.Default.isLRFlip = false;
            User.Default.isUDFlip = false;
            User.Default.Save();
            ScaleTransform scaleTransform = new ScaleTransform
            {
                ScaleX = 1,
                ScaleY = 1
            };
            Media.LayoutTransform = scaleTransform;
        }
        /// <summary>
        /// 旋转归位
        /// </summary>
        private void ResetRotation()
        {
            User.Default.rotateTo = 0;
            User.Default.Save();
            Media.LayoutTransform = new RotateTransform(0);
        }

        //选择
        /// <summary>
        /// 设置语言选项
        /// </summary>
        /// <param name="language">语言简拼</param>
        private void SelectLanguageItem(string language)
        {
            foreach (ItemInfo ci in LanguageItems)
            {
                if (ci.Value == language)
                {
                    CbbLanguages.SelectedItem = ci;
                    break;
                }
            }
        }
        /// <summary>
        /// 设置主题选项
        /// </summary>
        /// <param name="themePath">主题路径</param>
        private void SelectThemeItem(string themePath)
        {
            foreach (TextBlock item in CbbThemes.Items)
            {
                if (item.ToolTip.ToString() == themePath)
                {
                    CbbThemes.SelectedItem = item;
                    break;
                }
            }
        }
        /// <summary>
        /// 设置字体选项
        /// </summary>
        private void SelectPlayModeItem(int playMode)
        {
            CbbPlayMode.SelectedIndex = playMode;
        }

        //检查
        /// <summary>
        /// 检测路径是否正确
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>路径是否正确</returns>
        private bool IsPathRight(string path)
        {
            //检测路径是否存在
            if (File.Exists(path))
            {
                //获取文件扩展名
                string _videoExtension = System.IO.Path.GetExtension(path);
                //检测文件扩展名是否正确
                if (_videoExtension == ".avi" || _videoExtension == ".mp4" || _videoExtension == ".rmvb" || _videoExtension == ".mkv" || _videoExtension == ".wmv" || _videoExtension == ".wma" ||
                    _videoExtension == ".AVI" || _videoExtension == ".MP4" || _videoExtension == ".RMVB" || _videoExtension == ".MKV" || _videoExtension == ".WMV" || _videoExtension == ".WMA" ||
                    _videoExtension == ".wav" || _videoExtension == ".mp3" || _videoExtension == ".aac" || _videoExtension == ".flac" ||
                    _videoExtension == ".WAV" || _videoExtension == ".MP3" || _videoExtension == ".AAC" || _videoExtension == ".FLAC")
                {
                    return true;
                }
                else
                {
                    ShowMessage(FindResource("无法播放").ToString(), true);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //刷新
        /// <summary>
        /// 更改主窗口右键菜单和窗口控件状态
        /// </summary>
        /// <param name="state"></param>
        private void RefreshBtnsState()
        {
            if (PlayingMedia == null)
            {
                BtnLast.IsEnabled = false;
                BtnNext.IsEnabled = false;
                BtnStop.IsEnabled = false;
                BtnPlay.IsEnabled = false;
                BtnReplay.IsEnabled = false;
                BtnForward.IsEnabled = false;
                BtnBack.IsEnabled = false;

                BtnLRFlip.IsEnabled = false;
                BtnUDFlip.IsEnabled = false;
                BtnClockwise.IsEnabled = false;
                BtnCClockwise.IsEnabled = false;
                BtnSetLoopStart.IsEnabled = false;
                BtnSetLoopEnd.IsEnabled = false;
                BtnClearLoop.IsEnabled = false;

                SldTime.IsEnabled = false;
            }
            else
            {
                BtnLast.IsEnabled = true;
                BtnNext.IsEnabled = true;
                BtnStop.IsEnabled = true;
                BtnPlay.IsEnabled = true;
                BtnReplay.IsEnabled = true;
                BtnForward.IsEnabled = true;
                BtnBack.IsEnabled = true;

                BtnLRFlip.IsEnabled = true;
                BtnUDFlip.IsEnabled = true;
                BtnClockwise.IsEnabled = true;
                BtnCClockwise.IsEnabled = true;
                BtnSetLoopStart.IsEnabled = true;
                BtnSetLoopEnd.IsEnabled = true;
                BtnClearLoop.IsEnabled = true;

                SldTime.IsEnabled = true;
            }
        }
        /// <summary>
        /// 刷新主窗口标题
        /// </summary>
        public void RefreshTitle()
        {
            Main.Title = AppInfo.Name + " " + AppInfo.Version;
            if (PlayingMedia != null)
            {
                Main.Title += " " + PlayingMedia.Name;
            }
        }

        //显示
        /// <summary>
        /// 显示软件信息
        /// </summary>
        private void ShowAppInfo()
        {
            ThisName.Text = AppInfo.Name;
            Description.Text = AppInfo.Description;
            Developer.Text = AppInfo.Company;
            Version.Text = AppInfo.Version.ToString();
            BitCoinAddress.Text = AppInfo.BitCoinAddress;
            UpdateNote.Text = AppInfo.UpdateNote;
        }
        /// <summary>
        /// 创建mp3封面
        /// </summary>
        private void ShowMP3Cover(string path)
        {
            if (Path.GetExtension(path) == ".mp3")
            {
                Cover.Source = null;
                Cover.IsEnabled = true;
                List<BitmapImage> images = GetMP3Cover(path);
                if (images != null)
                {
                    if (images.Count > 0)
                    {
                        Cover.Source = images[0];
                    }
                }
            }
            else
            {
                Cover.Source = null;
                Cover.IsEnabled = false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void ShowPanMediaList()
        {
            PanPanels.Visibility = Visibility.Visible;

            PanMediaList.Visibility = Visibility.Visible;
            PanSetting.Visibility = Visibility.Collapsed;
            PanAbout.Visibility = Visibility.Collapsed;

            BtnsMedia.Visibility = Visibility.Visible;
            BtnsControl.Visibility = Visibility.Collapsed;
            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Collapsed;

            BtnMedia.BorderThickness = new Thickness(4, 0, 0, 0);
            BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        private void ShowPanControl()
        {
            PanControl.Visibility = Visibility.Visible;

            //PanMediaList.Visibility = Visibility.Collapsed;
            //PanSetting.Visibility = Visibility.Visible;
            //PanAbout.Visibility = Visibility.Collapsed;

            BtnsMedia.Visibility = Visibility.Collapsed;
            BtnsControl.Visibility = Visibility.Visible;
            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Collapsed;

            //BtnMedia.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnControl.BorderThickness = new Thickness(4, 0, 0, 0);
            //BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
            //BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        private void ShowPanSetting()
        {
            PanPanels.Visibility = Visibility.Visible;

            PanMediaList.Visibility = Visibility.Collapsed;
            PanSetting.Visibility = Visibility.Visible;
            PanAbout.Visibility = Visibility.Collapsed;

            BtnsMedia.Visibility = Visibility.Collapsed;
            BtnsControl.Visibility = Visibility.Collapsed;
            BtnsSetting.Visibility = Visibility.Visible;
            BtnsAbout.Visibility = Visibility.Collapsed;

            BtnMedia.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnSetting.BorderThickness = new Thickness(4, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(0, 0, 0, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        private void ShowPanAbout()
        {
            PanPanels.Visibility = Visibility.Visible;

            PanMediaList.Visibility = Visibility.Collapsed;
            PanSetting.Visibility = Visibility.Collapsed;
            PanAbout.Visibility = Visibility.Visible;

            BtnsMedia.Visibility = Visibility.Collapsed;
            BtnsControl.Visibility = Visibility.Collapsed;
            BtnsSetting.Visibility = Visibility.Collapsed;
            BtnsAbout.Visibility = Visibility.Visible;

            BtnMedia.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnSetting.BorderThickness = new Thickness(0, 0, 0, 0);
            BtnAbout.BorderThickness = new Thickness(4, 0, 0, 0);
        }
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="tip">资源名</param>
        /// <param name="newBox">是否弹出对话框</param>
        private void ShowMessage(string tip, bool newBox = false)
        {
            if (newBox)
            {
                MessageBox.Show(tip.ToString());
            }
            else
            {
                Message.Opacity = 1;
                Message.Content = tip;
            }
        }

        //切换
        /// <summary>
        /// 显示/隐藏播放列表
        /// </summary>
        private void SwichMenu()
        {
            User.Default.isShowMenu = !User.Default.isShowMenu;
            User.Default.Save();
            if (User.Default.isShowMenu)
            {
                PanMenu.Visibility = Visibility.Visible;
            }
            else
            {
                PanMenu.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void SwichPanels()
        {
            if (PanPanels.Visibility == Visibility.Visible)
            {
                PanPanels.Visibility = Visibility.Collapsed;
                BtnFold.BorderThickness = new Thickness(0, 0, 0, 0);
            }
            else
            {
                PanPanels.Visibility = Visibility.Visible;
                BtnFold.BorderThickness = new Thickness(4, 0, 0, 0);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void SwichPanControl()
        {
            if (PanControl.Visibility == Visibility.Visible)
            {
                PanControl.Visibility = Visibility.Collapsed;
                BtnControl.BorderThickness = new Thickness(0, 0, 0, 0);
                Trigger.Visibility = Visibility.Visible;
            }
            else
            {
                ShowPanControl();
                Trigger.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// 切换全屏
        /// </summary>
        private void SwichScreenState()
        {
            if (IsFullScreen == false)
            {
                LastRect[0] = Left;
                LastRect[1] = Top;
                LastRect[2] = Width;
                LastRect[3] = Height;

                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Topmost = true;

                Left = 0;
                Top = 0;
                Width = SystemParameters.PrimaryScreenWidth;
                Height = SystemParameters.PrimaryScreenHeight;
            }
            else
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;
                Topmost = false;

                Left = LastRect[0];
                Top = LastRect[1];
                Width = LastRect[2];
                Height = LastRect[3];
            }

            IsFullScreen = !IsFullScreen;
        }

        //其它
        /// <summary>
        /// 播放新媒体
        /// </summary>
        /// <param name="path">媒体路径</param>
        private void PlayNewVideo(FileOrFolderInfo media)
        {
            PlayingMedia = media;

            //若在切换视频时不保留镜像状态
            if (User.Default.isKeepTrans == false)
            {
                ResetFlip();
                ResetRotation();
            }
            //清空媒体源
            Media.Source = null;
            //若播放音频，显示封面
            ShowMP3Cover(media.Path);
            //获取路径开始播放
            Media.Source = new Uri(media.Path);
            Media.Play();
            Media.ScrubbingEnabled = true;
            IsPlaying = true;
            BtnPlay.Content = FindResource("暂停").ToString();

            RefreshBtnsState();
            RefreshTitle();
            ShowMessage(FindResource("开始播放").ToString() + "：" + GetVideoName(media.Path));
        }
        /// <summary>
        /// 播放与暂停操作
        /// </summary>
        private void Play()
        {
            if (IsPlaying == false)
            {
                Media.Play();
                IsPlaying = true;
                BtnPlay.Content = FindResource("暂停");
                ShowMessage(FindResource("开始播放").ToString());
            }
            else if (IsPlaying == true)
            {
                Media.Pause();
                IsPlaying = false;
                BtnPlay.Content = FindResource("播放");
                ShowMessage(FindResource("暂停播放").ToString());
            }
        }
        /// <summary>
        /// 重播
        /// </summary>
        private void Replay()
        {
            Media.Position = TimeSpan.Zero;
            Media.Play();
            IsPlaying = true;
            BtnPlay.Content = FindResource("暂停");
            //显示消息
            ShowMessage(FindResource("开始播放").ToString());
        }
        /// <summary>
        /// 停止
        /// </summary>
        private void Stop()
        {
            Media.Stop();
            IsPlaying = false;
            BtnPlay.Content = FindResource("播放");
            ShowMessage(FindResource("停止播放").ToString());
        }
        /// <summary>
        /// 快进
        /// </summary>
        private void Forward()
        {
            if (PlayingMedia == null)
            {
                return;
            }
            if (File.Exists(PlayingMedia.Path))
            {
                Media.Position += TimeSpan.FromSeconds(User.Default.jumpTime);
                if (Media.Position > Media.NaturalDuration.TimeSpan)
                {
                    Media.Position = Media.NaturalDuration.TimeSpan;
                }
                string str = Media.Position.ToString();
                //显示消息
                ShowMessage(FindResource("当前进度").ToString() + "：" + str.Substring(0, 8));
            }
        }
        /// <summary>
        /// 快退
        /// </summary>
        private void Back()
        {
            if (PlayingMedia == null)
            {
                return;
            }
            if (File.Exists(PlayingMedia.Path))
            {
                Media.Position -= TimeSpan.FromSeconds(User.Default.jumpTime);
                if (Media.Position <= TimeSpan.Zero)
                {
                    Media.Position = TimeSpan.Zero;
                }
                string str = Media.Position.ToString();
                //显示消息
                ShowMessage(FindResource("当前进度").ToString() + "：" + str.Substring(0, 8));
            }
        }
        /// <summary>
        /// 上一个
        /// </summary>
        private void Last()
        {
            if (PlayingMedia == null)
            {
                return;
            }

            int n = 0;
            string[] path = new string[LtbMedia.Items.Count];
            ListBoxItem tempItem;
            for (int i = 0; i < LtbMedia.Items.Count; i++)
            {
                tempItem = LtbMedia.Items.GetItemAt(i) as ListBoxItem;
                path[i] = tempItem.Tag.ToString();

                if (PlayingMedia.Path == path[i])
                {
                    n = LtbMedia.Items.IndexOf(tempItem);
                    break;
                }
            }

            if (n == 0)
            {
                ShowMessage(FindResource("已经是第一个媒体").ToString());
            }
            else
            {
                ListBoxItem nextItem = LtbMedia.Items.GetItemAt(n - 1) as ListBoxItem;
                FileOrFolderInfo media = new FileOrFolderInfo(nextItem.Tag.ToString());
                LtbMedia.SelectedIndex = n - 1;
                PlayNewVideo(media);
                IsPlaying = true;
            }
        }
        /// <summary>
        /// 下一个
        /// </summary>
        private void Next()
        {
            if (PlayingMedia == null)
            {
                return;
            }

            int n = 0;
            string[] path = new string[LtbMedia.Items.Count];
            ListBoxItem tempItem;
            for (int i = 0; i < LtbMedia.Items.Count; i++)
            {
                tempItem = LtbMedia.Items.GetItemAt(i) as ListBoxItem;
                path[i] = tempItem.Tag.ToString();

                if (PlayingMedia.Path == path[i])
                {
                    n = LtbMedia.Items.IndexOf(tempItem);
                    break;
                }
            }

            if (n == LtbMedia.Items.Count - 1)
            {
                Stop();
                ShowMessage(FindResource("全部播放完毕").ToString());
            }
            else
            {
                ListBoxItem nextItem = LtbMedia.Items.GetItemAt(n + 1) as ListBoxItem;
                FileOrFolderInfo media = new FileOrFolderInfo(nextItem.Tag.ToString());
                LtbMedia.SelectedIndex = n + 1;
                PlayNewVideo(media);
                IsPlaying = true;
            }
        }

        /// <summary>
        /// 音量升高
        /// </summary>
        private void VolumeUp()
        {
            Media.Volume = Media.Volume + 0.1;
            double temp = Math.Round(Media.Volume, 2);
            SldVolume.Value = temp;
            SldVolume.ToolTip = temp;
            //显示消息
            ShowMessage(FindResource("当前音量").ToString() + "：" + temp);
        }
        /// <summary>
        /// 音量降低
        /// </summary>
        private void VolumeDown()
        {
            Media.Volume = Media.Volume - 0.1;
            double temp = Math.Round(Media.Volume, 2);
            SldVolume.Value = temp;
            SldVolume.ToolTip = temp;
            //显示消息
            ShowMessage(FindResource("当前音量").ToString() + "：" + temp);
        }
        /// <summary>
        /// 速度增加
        /// </summary>
        private void SpeedUp()
        {
            Media.SpeedRatio = Media.SpeedRatio + 0.1;
            double temp = Math.Round(Media.SpeedRatio, 2);
            SldSpeed.Value = temp;
            SldSpeed.ToolTip = temp;
            //显示消息
            ShowMessage(FindResource("播放速度").ToString() + "：" + temp);
        }
        /// <summary>
        /// 速度减少
        /// </summary>
        private void SpeedDown()
        {
            Media.SpeedRatio = Media.SpeedRatio - 0.1;
            double temp = Math.Round(Media.SpeedRatio, 2);
            SldSpeed.Value = temp;
            SldSpeed.ToolTip = temp;
            //显示消息
            ShowMessage(FindResource("播放速度").ToString() + "：" + temp);
        }
        /// <summary>
        /// 左右翻转
        /// </summary>
        private void LRFlip()
        {
            if (User.Default.isLRFlip == false)
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleX = -1;
                if (User.Default.isUDFlip == true)
                {
                    scaleTransform.ScaleY = -1;
                }
                else
                {
                    scaleTransform.ScaleY = 1;
                }
                Media.LayoutTransform = scaleTransform;
                User.Default.isLRFlip = true;
                //显示消息
                ShowMessage(FindResource("已左右翻转").ToString());
            }
            else if (User.Default.isLRFlip == true)
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleX = 1;
                if (User.Default.isUDFlip == true)
                {
                    scaleTransform.ScaleY = -1;
                }
                else
                {
                    scaleTransform.ScaleY = 1;
                }
                Media.LayoutTransform = scaleTransform;
                User.Default.isLRFlip = false;
                //显示消息
                ShowMessage(FindResource("已取消左右翻转").ToString());
            }
        }
        /// <summary>
        /// 上下翻转
        /// </summary>
        private void UDFlip()
        {
            if (User.Default.isUDFlip == false)
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleY = -1;
                if (User.Default.isLRFlip == true)
                {
                    scaleTransform.ScaleX = -1;
                }
                else
                {
                    scaleTransform.ScaleX = 1;
                }
                Media.LayoutTransform = scaleTransform;
                User.Default.isUDFlip = true;
                //显示消息
                ShowMessage(FindResource("已上下翻转").ToString());
            }
            else if (User.Default.isUDFlip == true)
            {
                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleY = 1;
                if (User.Default.isLRFlip == true)
                {
                    scaleTransform.ScaleX = -1;
                }
                else
                {
                    scaleTransform.ScaleX = 1;
                }
                Media.LayoutTransform = scaleTransform;
                User.Default.isUDFlip = false;
                //显示消息
                ShowMessage(FindResource("已取消上下翻转").ToString());
            }
        }
        /// <summary>
        /// 顺时针旋转90度
        /// </summary>
        private void ClockwiseRotation()
        {
            User.Default.rotateTo += 1;
            if (User.Default.rotateTo > 3)
            {
                User.Default.rotateTo = 0;
            }
            User.Default.Save();
            Media.LayoutTransform = new RotateTransform(90 * User.Default.rotateTo);
            //显示消息
            ShowMessage(FindResource("已顺时针旋转90度").ToString());
        }
        /// <summary>
        /// 逆时针旋转90度
        /// </summary>
        private void CounterClockwiseRotation()
        {
            User.Default.rotateTo -= 1;
            if (User.Default.rotateTo < 0)
            {
                User.Default.rotateTo = 3;
            }
            User.Default.Save();
            Media.LayoutTransform = new RotateTransform(90 * User.Default.rotateTo);
            //显示消息
            ShowMessage(FindResource("已逆时针旋转90度").ToString());
        }
        /// <summary>
        /// 到达循环结束点时的动作
        /// </summary>
        private void Loop()
        {
            if (HasEndPosition == true && StartPosition < EndPosition)
            {
                if (Media.Position > EndPosition)
                {
                    Media.Position = StartPosition;
                }
                if (Media.Position < StartPosition)
                {
                    Media.Position = StartPosition;
                }
            }
        }

        //窗口
        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            CreateTimer();

            //载入并显示应用信息
            LoadAppInfo();
            ShowAppInfo();

            //载入下拉菜单项
            LoadLanguageItems();
            LoadThemeItems();

            //载入设置
            LoadSettings();

            //初始化
            SetLanguage(User.Default.language);
            SetTheme(User.Default.ThemePath);
            SetPlayMode(User.Default.playMode);

            LoadVideos();

            //刷新
            Message.Opacity = 0;
            RefreshBtnsState();
            RefreshTitle();
        }
        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            User.Default._medias = "";
            if (User.Default.isSavePlaylist)
            {
                SavePlaylist();
            }
            User.Default.Save();

            //重置
            ResetFlip();
            ResetRotation();
        }
        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            //Ctrl+1 
            if (e.Key == Key.D1 && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                SetPlayMode(0);
                SelectPlayModeItem(User.Default.playMode);
                ShowMessage(FindResource("播放模式切换").ToString() + "：" + FindResource("单次").ToString());
            }
            //Ctrl+2
            else if (e.Key == Key.D2 && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                SetPlayMode(1);
                SelectPlayModeItem(User.Default.playMode);
                ShowMessage(FindResource("播放模式切换").ToString() + "：" + FindResource("循环").ToString());
            }
            //Ctrl+3 
            else if (e.Key == Key.D3 && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                SetPlayMode(2);
                SelectPlayModeItem(User.Default.playMode);
                ShowMessage(FindResource("播放模式切换").ToString() + "：" + FindResource("顺序").ToString());
            }
            //Ctrl+4
            else if (e.Key == Key.D4 && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                SetPlayMode(3);
                SelectPlayModeItem(User.Default.playMode);
                ShowMessage(FindResource("播放模式切换").ToString() + "：" + FindResource("随机").ToString());
            }

            if (e.Key == Key.F)
            {
                SwichPanels();
            }
            else if (e.Key == Key.S)
            {
                ShowPanSetting();
            }
            else if (e.Key == Key.H)
            {
                ShowPanAbout();
            }
            //O键打开媒体文件
            else if (e.Key == Key.O)
            {
                AddPlayListItem(new FileOrFolderInfo(GetFilePath()), true);
            }
            //A键添加媒体文件
            else if (e.Key == Key.A)
            {
                AddPlayListItem(new FileOrFolderInfo(GetFilePath()), false);
            }
            //按Esc键退出全屏
            else if (e.Key == Key.Escape)
            {
                SwichScreenState();
            }
            //K键隐藏控制区
            else if (e.Key == Key.J)
            {
                SwichMenu();
            }
            //K键隐藏控制区
            else if (e.Key == Key.K)
            {
                SwichPanControl();
            }
            //L键显示/隐藏播放列表
            else if (e.Key == Key.L)
            {
                if (PanMediaList.Visibility == Visibility.Visible)
                {
                    SwichPanels();
                }
                else
                {
                    ShowPanMediaList();
                }
            }
            //Delete键删除视频
            else if (e.Key == Key.Delete)
            {
                RemoveMedia();
            }

            if (PlayingMedia != null)
            {
                //按空格键播放和暂停
                if (e.Key == Key.Space)
                {
                    Play();
                }
                //R键重新播放
                else if (e.Key == Key.R)
                {
                    Replay();
                }
                //上一个
                else if (e.Key == Key.PageUp)
                {
                    Last();
                }
                //下一个
                else if (e.Key == Key.PageDown)
                {
                    Next();
                }
                //按←→键改变进度
                else if (e.Key == Key.Right && File.Exists(PlayingMedia.Path))
                {
                    Forward();
                }
                else if (e.Key == Key.Left && File.Exists(PlayingMedia.Path))
                {
                    Back();
                }
                //按[]键设置循环段开始与结束
                else if (e.Key == Key.OemOpenBrackets && File.Exists(PlayingMedia.Path))
                {
                    SetLoopStart();
                }
                else if (e.Key == Key.OemCloseBrackets && File.Exists(PlayingMedia.Path))
                {
                    SetLoopEnd();
                }
                //按\键清除循环段
                else if (e.Key == Key.OemPipe)
                {
                    RemoveLoop();
                }
                //M键左右翻转
                else if (e.Key == Key.M)
                {
                    LRFlip();
                }
                //N键上下翻转
                else if (e.Key == Key.N)
                {
                    UDFlip();
                }
                //Q键逆时针
                else if (e.Key == Key.Q)
                {
                    CounterClockwiseRotation();
                }
                //E键顺时针
                else if (e.Key == Key.E)
                {
                    ClockwiseRotation();
                }
                //按+-键改变音量
                else if (e.Key == Key.OemPlus || e.Key == Key.Add && Media.Volume < 0.9)
                {
                    VolumeUp();
                }
                else if (e.Key == Key.OemMinus || e.Key == Key.Subtract && Media.Volume > 0.1)
                {
                    VolumeDown();
                }
                //按，。键改变播放速度
                else if (e.Key == Key.OemPeriod && Media.SpeedRatio < 2)
                {
                    SpeedUp();
                }
                else if (e.Key == Key.OemComma && Media.SpeedRatio > 0.15)
                {
                    SpeedDown();
                }
            }

            //Ctrl+T 切换下个主题
            if (e.Key == Key.T && (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            { SetNextTheme(); }

            //关于菜单
            if (e.Key == Key.F1)
            { Process.Start("explorer.exe", AppInfo.HomePage); }
            else if (e.Key == Key.F2)
            { Process.Start("explorer.exe", AppInfo.InfoPage); }
            else if (e.Key == Key.F3)
            { Process.Start("explorer.exe", AppInfo.DownloadPage); }
            else if (e.Key == Key.F4)
            { Process.Start("explorer.exe", AppInfo.FeedbackPage); }
            else if (e.Key == Key.F8)
            { Process.Start("explorer.exe", AppInfo.GitHubPage); }
            else if (e.Key == Key.F6)
            { Process.Start("explorer.exe", AppInfo.QQGroupLink); }

        }
        private void Main_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            SwichMenu();
        }
        private void PanContent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                SwichScreenState();
            }
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            SldTime.Maximum = Media.NaturalDuration.TimeSpan.TotalSeconds;
            TimeAll.Content = Media.NaturalDuration;
        }
        private void Media_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Play();
        }
        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            //单次
            if (User.Default.playMode == 0)
            {
                Stop();
                ShowMessage(FindResource("播放结束").ToString());
            }
            //循环
            else if (User.Default.playMode == 1)
            {
                Media.Position = TimeSpan.Zero;
            }
            //顺序
            else if (User.Default.playMode == 2)
            {
                Next();
            }
            //随机
            else if (User.Default.playMode == 3)
            {
                Random ran = new Random();
                int n = ran.Next(0, LtbMedia.Items.Count);
                //获取Item
                ListBoxItem nextItem = LtbMedia.ItemContainerGenerator.ContainerFromIndex(n) as ListBoxItem;
                FileOrFolderInfo media = new FileOrFolderInfo(nextItem.Tag.ToString());
                LtbMedia.SelectedIndex = n;
                PlayNewVideo(media);
                IsPlaying = true;
            }
            RefreshTitle();
        }

        private void LtbMedia_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ListBoxItem tempItem;
                tempItem = LtbMedia.SelectedItem as ListBoxItem;
                if (tempItem != null)
                {
                    FileOrFolderInfo media = new FileOrFolderInfo(tempItem.Tag.ToString());
                    PlayNewVideo(media);
                }
            }
        }
        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            SwichPanControl();
        }
        private void PanControl_MouseLeave(object sender, MouseEventArgs e)
        {
            SwichPanControl();
        }

        private void JumpTime_KeyUp(object sender, KeyEventArgs e)
        {
            if (JumpTime.Text != "")
            {
                try
                {
                    int t = int.Parse(JumpTime.Text);
                    if (t > 0 && t < 1000)
                    {
                        TimeSpan ts = TimeSpan.FromMinutes(t);
                        User.Default.jumpTime = t;
                        User.Default.Save();
                        //显示消息
                        // ShowMessage("已更改");
                    }
                    else
                    {
                        JumpTime.Text = User.Default.jumpTime.ToString();
                        // ShowMessage("输入1~999整数");
                    }
                }
                catch (Exception)
                {
                    JumpTime.Text = User.Default.jumpTime.ToString();
                    // ShowMessage("输入整数");
                }
            }
        }

        //按钮
        private void BtnFold_Click(object sender, RoutedEventArgs e)
        {
            SwichPanels();
        }
        private void BtnList_Click(object sender, RoutedEventArgs e)
        {
            ShowPanMediaList();
        }
        private void BtnControl_Click(object sender, RoutedEventArgs e)
        {
            SwichPanControl();
        }
        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            ShowPanSetting();
        }
        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            ShowPanAbout();
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            AddPlayListItem(new FileOrFolderInfo(GetFilePath()), true);
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddPlayListItem(new FileOrFolderInfo(GetFilePath()), false);
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            RemoveMedia();
        }
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearMedia();
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }
        private void BtnReplay_Click(object sender, RoutedEventArgs e)
        {
            Replay();
        }
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }
        private void BtnLast_Click(object sender, RoutedEventArgs e)
        {
            Last();
        }
        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }
        private void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            Forward();
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }
        private void BtnVolumeUp_Click(object sender, RoutedEventArgs e)
        {
            VolumeUp();
        }
        private void BtnVolumeDown_Click(object sender, RoutedEventArgs e)
        {
            VolumeDown();
        }
        private void BtnSpeedUp_Click(object sender, RoutedEventArgs e)
        {
            SpeedUp();
        }
        private void BtnSpeedDown_Click(object sender, RoutedEventArgs e)
        {
            SpeedDown();
        }
        private void BtnLoopStart_Click(object sender, RoutedEventArgs e)
        {
            SetLoopStart();
        }
        private void BtnLoopEnd_Click(object sender, RoutedEventArgs e)
        {
            SetLoopEnd();
        }
        private void BtnClearLoop_Click(object sender, RoutedEventArgs e)
        {
            RemoveLoop();
        }
        private void BtnLRFlip_Click(object sender, RoutedEventArgs e)
        {
            LRFlip();
        }
        private void BtnUDFlip_Click(object sender, RoutedEventArgs e)
        {
            UDFlip();
        }
        private void BtnClockwise_Click(object sender, RoutedEventArgs e)
        {
            ClockwiseRotation();
        }
        private void BtnCClockwise_Click(object sender, RoutedEventArgs e)
        {
            CounterClockwiseRotation();
        }
        private void BtnFullScreen_Click(object sender, RoutedEventArgs e)
        {
            SwichScreenState();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetUserSettings();
            LoadSettings();
            ShowMessage(FindResource("已重置").ToString());
        }
        private void BtnClearRunInfo_Click(object sender, RoutedEventArgs e)
        {
            ResetAppSettings();
        }

        private void BitCoinAddress_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Clipboard.SetDataObject(BitCoinAddress.Text, true);
            //ShowMessage("已复制");
        }
        private void BtnHomePage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.HomePage);
        }
        private void BtnInfoPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.InfoPage);
        }
        private void BtnDownloadPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.DownloadPage);
        }
        private void BtnFeedbackPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.FeedbackPage);
        }
        private void BtnGitHubPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.GitHubPage);
        }
        private void BtnQQGroup_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", AppInfo.QQGroupLink);
        }

        //设置更改事件
        private void CbbLanguages_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (CbbLanguages.SelectedValue != null)
            {
                string langName = CbbLanguages.SelectedValue.ToString();
                SetLanguage(langName);
            }
        }
        private void CbbThemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbbThemes.SelectedItem != null)
            {
                TextBlock item = CbbThemes.SelectedItem as TextBlock;
                string tmp = item.ToolTip.ToString();
                if (File.Exists(tmp))
                {
                    SetTheme(tmp);
                    ShowMessage(FindResource("已更改").ToString());
                }
                else
                {
                    CbbThemes.Items.Remove(CbbThemes.SelectedItem);
                    ShowMessage(FindResource("该主题的配置文件不存在").ToString());
                }
            }
        }
        private void CbbPlayMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbbPlayMode.SelectedIndex > -1)
            {
                SetPlayMode(CbbPlayMode.SelectedIndex);
            }
        }

        private void SldTime_Loaded(object sender, RoutedEventArgs e)
        {
            SldTime.AddHandler(MouseLeftButtonUpEvent,
                new MouseButtonEventHandler(SldTime_MouseLeftButtonUp), true);
            SldVolume.AddHandler(MouseLeftButtonUpEvent,
                new MouseButtonEventHandler(SldVolume_MouseLeftButtonUp), true);
        }
        private void SldTime_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Media.Position = TimeSpan.FromSeconds(SldTime.Value);
            string str = Media.Position.ToString();
            //显示消息
            Message.Opacity = 1;
            Message.Content = "当前进度：" + str.Substring(0, 8);
        }
        private void SldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
        private void SldVolume_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            double temp = Math.Round(SldVolume.Value, 2);
            Media.Volume = temp;
            SldVolume.ToolTip = temp;
            ShowMessage(FindResource("当前音量").ToString() + "：" + temp);

        }
        private void SldSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
        private void SldSpeed_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            double temp = Math.Round(SldSpeed.Value, 2);
            Media.SpeedRatio = temp;
            SldSpeed.ToolTip = temp;
            ShowMessage(FindResource("播放速度").ToString() + "：" + temp);
        }

        private void Item_Play_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                //获取当前选择的Item在PlayListBox的索引
                ListBoxItem tempItem;
                tempItem = LtbMedia.SelectedItem as ListBoxItem;
                PlayNewVideo(new FileOrFolderInfo(tempItem.Tag.ToString()));
            }
        }

        private void PanMediaList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void PanMediaList_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Pressed)
            //{
            //    var pos = e.GetPosition(PlayListBox);
            //    HitTestResult result = VisualTreeHelper.HitTest(PlayListBox, pos);
            //    if (result == null)
            //    {
            //        return;
            //    }
            //    var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
            //    if (listBoxItem == null || listBoxItem.Content != PlayListBox.SelectedItem)
            //    {
            //        return;
            //    }
            //    System.Windows.DataObject dataObj = new System.Windows.DataObject(listBoxItem.Content as TextBlock);
            //    DragDrop.DoDragDrop(PlayListBox, dataObj, System.Windows.DragDropEffects.Move);
            //}
        }
        private void PanMediaList_PreviewDrop(object sender, DragEventArgs e)
        {
            ////拖拽排序
            //var pos = e.GetPosition(PlayListBox);
            //var result = VisualTreeHelper.HitTest(PlayListBox, pos);
            //if (result == null)
            //{
            //    return;
            //}
            ////查找元数据  
            //var sourcePerson = e.Data.GetData(typeof(TextBlock)) as TextBlock;
            //if (sourcePerson == null)
            //{
            //    return;
            //}
            ////查找目标数据  
            //var listBoxItem = Utils.FindVisualParent<ListBoxItem>(result.VisualHit);
            //if (listBoxItem == null)
            //{
            //    return;
            //}
            //var targetPerson = listBoxItem.Content as TextBlock;

            //if (ReferenceEquals(targetPerson, sourcePerson))
            //{
            //    return;
            //}
            //PlayListBox.Items.Remove(sourcePerson);
            //PlayListBox.Items.Insert(PlayListBox.Items.IndexOf(targetPerson), sourcePerson);
        }
        private void PanMediaList_Drop(object sender, DragEventArgs e)
        {
            //如果有文件拖拽进入
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                //获取该文件路径
                FileOrFolderInfo media = new FileOrFolderInfo(((Array)e.Data.GetData(System.Windows.DataFormats.FileDrop)).GetValue(0).ToString());
                //检测路径是否正确
                if (IsPathRight(media.Path))
                {
                    //在播放列表添加此媒体，不播放
                    AddPlayListItem(media, false);
                }
            }
        }
        private void PanContent_Drop(object sender, DragEventArgs e)
        {
            //如果有文件拖拽进入
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                //获取该文件路径
                FileOrFolderInfo media = new FileOrFolderInfo(((Array)e.Data.GetData(System.Windows.DataFormats.FileDrop)).GetValue(0).ToString());
                //检测路径是否正确
                if (IsPathRight(media.Path))
                {
                    //在播放列表添加此媒体，播放
                    AddPlayListItem(media, true);
                }
            }
        }

        private void KeepTrans_Checked(object sender, RoutedEventArgs e)
        {
            User.Default.isKeepTrans = true;
            User.Default.Save();
            //显示消息
            //ShowMessage("已更改");
        }
        private void KeepTrans_Unchecked(object sender, RoutedEventArgs e)
        {
            User.Default.isKeepTrans = false;
            User.Default.Save();
            //显示消息
            //ShowMessage("已更改");
        }
        private void SavePlaylist_Checked(object sender, RoutedEventArgs e)
        {
            User.Default.isSavePlaylist = true;
            User.Default.Save();
            //显示消息
            //ShowMessage("已更改");
        }
        private void SavePlaylist_Unchecked(object sender, RoutedEventArgs e)
        {
            User.Default.isSavePlaylist = false;
            User.Default.Save();
            //显示消息
            //ShowMessage("已更改");
        }


        //计时器事件
        private void Timer1_Tick(object sender, EventArgs e)
        {
            TimePosition.Content = Media.Position;
            SldTime.Value = Media.Position.TotalSeconds;
        }
        private void Timer2_Tick(object sender, EventArgs e)
        {
            Message.Opacity -= 0.01;
            Loop();
        }
    }
}
