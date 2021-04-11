﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
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

namespace FingersApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Fingers fingers;
        Thread FingersThread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            FingersThread = new Thread(() =>
            {
                fingers = new Fingers(this);
            });
            FingersThread.IsBackground = true;
            FingersThread.Start();
        }

        public void SetLeapStatus(String status)
        {
            LeapStatus.Text = status;
        }

        private void SetRingStatus(TextBlock ring, RingStatus status, ulong addr)
        {
            if (status == RingStatus.CONNECTED)
                ring.Text = addr.ToString("X").Substring(8);
            else
                ring.Text = status == RingStatus.CONNECTING ? "CONNECTING" : "SEARCHING";
        }
        public void SetLeftRingStatus(RingStatus status, ulong addr)
        {
            SetRingStatus(LeftRingStatus, status, addr);
        }
        public void SetRightRingStatus(RingStatus status, ulong addr)
        {
            SetRingStatus(RightRingStatus, status, addr);
        }

        public void SetButtonStatus(LoopButton b, Boolean pressed, Boolean rightHand)
        {
            ImageSource img = (ImageSource)(pressed ? FindResource("IndicatorOn") : FindResource("IndicatorOff"));

            if (rightHand)
            {
                if (b == LoopButton.CENTER) RightRingBtnCenter.Source = img;
                else if (b == LoopButton.FWD) RightRingBtnFwd.Source = img;
                else if (b == LoopButton.BACK) RightRingBtnBack.Source = img;
                else if (b == LoopButton.UP) RightRingBtnUp.Source = img;
                else if (b == LoopButton.DOWN) RightRingBtnDown.Source = img;
            } else
            {
                if (b == LoopButton.CENTER) LeftRingBtnCenter.Source = img;
                else if (b == LoopButton.FWD) LeftRingBtnFwd.Source = img;
                else if (b == LoopButton.BACK) LeftRingBtnBack.Source = img;
                else if (b == LoopButton.UP) LeftRingBtnUp.Source = img;
                else if (b == LoopButton.DOWN) LeftRingBtnDown.Source = img;
            }
        }

        public void SelectLeapProfile(String name)
        {
            LeapProfileSelector.SelectedIndex =
                LeapProfileSelector.Items.Cast<ComboBoxItem>()
                    .Select(c => (string)c.Content)
                    .ToList()
                    .IndexOf(name);
        }

        private void Swap(object sender, RoutedEventArgs e)
        {
            
            fingers.SwapRings(); // Not threadsafe

            ImageSource img = (ImageSource)FindResource("IndicatorOff");
            LeftRingBtnCenter.Source = img;
            LeftRingBtnFwd.Source = img;
            LeftRingBtnBack.Source = img;
            LeftRingBtnUp.Source = img;
            LeftRingBtnDown.Source = img;
            RightRingBtnCenter.Source = img;
            RightRingBtnFwd.Source = img;
            RightRingBtnBack.Source = img;
            RightRingBtnUp.Source = img;
            RightRingBtnDown.Source = img;
        }

        private void LeapProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            String text = (e.AddedItems[0] as ComboBoxItem).Content.ToString();

            // If fingers is null this is getting called from fingers' constructor as it
            // loads the previous data, so we don't need/want to call back to it
            if (text != null && fingers != null)
                fingers.SetLeapProfile(text, false);
        }
    }
}
