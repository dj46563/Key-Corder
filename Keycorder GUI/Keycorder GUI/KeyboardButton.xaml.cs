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
using System.Windows.Threading;

namespace Keycorder_GUI
{
    /// <summary>
    /// Interaction logic for KeyboardButton.xaml
    /// </summary>
    public partial class KeyboardButton : UserControl
    {
        // dispatch timer to handle the flashing background of the key
        readonly DispatcherTimer dt = new DispatcherTimer();
        // bool to store if the key is currently counting up
        public bool InProgress = false;

        // The type of key this is, mostly for style purposes
        public enum KeyTypeEnum
        {
            OneTime, Duration, Other
        }
        public KeyTypeEnum KeyType { get; set; }

        // Background color of the key, stored in private var so that background can be flashed and we can keep the old color
        private Brush _backgroundColor;
        public Brush BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = Panel.Background = value;
        }

        public string ElapsedTime
        {
            get { return String.Concat((string)GetValue(ElapsedTimeProperty), "s"); }
            set { SetValue(ElapsedTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ElapsedTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ElapsedTimeProperty =
            DependencyProperty.Register("ElapsedTime", typeof(string), typeof(KeyboardButton), new PropertyMetadata(""));


        public string Behavior
        {
            get { return (string)GetValue(BehaviorProperty); }
            set { SetValue(BehaviorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Behavior.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BehaviorProperty =
            DependencyProperty.Register("Behavior", typeof(string), typeof(KeyboardButton), new PropertyMetadata(""));



        public Key KeyEnum { get; set; }

        public string Key
        {
            get { return (string)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Key.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(KeyboardButton), new PropertyMetadata(""));



        public Brush FlashColorBrush { get; set; }

        public KeyboardButton()
        {
            InitializeComponent();

            FlashColorBrush = Brushes.Red;

            dt.Tick += DtOnTick;
            dt.Interval = new TimeSpan(0, 0, 0, 0, 300);
        }

        private void DtOnTick(object sender, EventArgs eventArgs)
        {
            if (!InProgress)
                ElapsedTime = "";

            Panel.Background = _backgroundColor;
            dt.Stop();
        }

        // Make the background color be the flash color for 300 millilseconds before switching back
        public void FlashColor()
        {
            if (FlashColorBrush != null)
            {
                Panel.Background = FlashColorBrush;
                dt.Start();
            }
        }

        private void KeyboardButton_OnLoaded(object sender, RoutedEventArgs e)
        {
            switch (KeyType)
            {
                case KeyTypeEnum.Duration:
                    BackgroundColor = Brushes.CornflowerBlue;
                    break;
                case KeyTypeEnum.OneTime:
                    BackgroundColor = Brushes.Coral;
                    break;
                case KeyTypeEnum.Other:
                    BackgroundColor = Brushes.AliceBlue;
                    break;
            }
        }
    }
}
