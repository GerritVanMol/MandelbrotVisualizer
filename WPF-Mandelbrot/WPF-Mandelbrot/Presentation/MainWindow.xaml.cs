using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using WPF_Mandelbrot.Models;
using WPF_Mandelbrot.Presentation;

namespace WPF_Mandelbrot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private readonly MainViewModel viewModel;
        private readonly int[] allIterableNumbers = { 50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
        private readonly string[] selectableColors = { "COLOR","BANDING", "GRAYSCALE" };

        public MainWindow(MainViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
            FillCombobox();
            viewModel = DataContext as MainViewModel;
            cbxColors.SelectedIndex = 0;
            cbxIterations.SelectedIndex = 0;
        }

        private void FillCombobox()
        {
            cbxIterations.ItemsSource = allIterableNumbers;
            cbxColors.ItemsSource = selectableColors;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            viewModel.GetMousePositon(e.Delta);
            DisplayResultAsync();
            base.OnMouseWheel(e);
        }
        
        private async void DisplayResultAsync()
        {
            double msToSeconds = viewModel.stopwatch.ElapsedMilliseconds;
            string timeElapsed = msToSeconds.ToString();
            executionTimeTxtBlock.Text = timeElapsed + " ms";
            viewModel.BitmapDisplay = await viewModel.GenerateBitmapAsync(viewModel.Coordinates, viewModel.StepSize, viewModel.OffsetX, viewModel.OffsetY);
        }

        private void ComboBox_IterationsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.MaxIterabeleNr = (int)cbxIterations.SelectedItem;
            iterationTxt.Text = cbxIterations.SelectedItem.ToString();
            DisplayResultAsync();
        }

        private void ComboBox_ColorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SelectedColor = (string)cbxColors.SelectedItem;
            DisplayResultAsync();
        }

        //Set new screen coordinates
        private void Image_MouseLeftButtonUpAsync(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(Image);
            viewModel.CalculatePanningPositionAsync(Image.ActualWidth, Image.ActualHeight, mousePos);
        }
        
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePos = e.GetPosition(Image);
            viewModel.CalculateRealCoordinates(Image.ActualWidth, Image.ActualHeight, mousePos);
            viewModel.CalculateMouseCornerCoords(Image.ActualWidth, Image.ActualHeight, mousePos);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            int parsedValue;
            if (e.Key == Key.Return)
            {
                if (int.TryParse(iterationTxt.Text, out parsedValue))
                {
                    viewModel.MaxIterabeleNr = Convert.ToInt32(iterationTxt.Text);
                    DisplayResultAsync();
                }
                else MessageBox.Show("Only numeric values allowed!"); iterationTxt.Text = cbxIterations.SelectedItem.ToString();
            }
        }
    }
}
