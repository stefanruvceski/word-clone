using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Word_ByME
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string file;
        int saved = 0;
        int empty = 1;
        System.Windows.Media.Color c;
        Color r;
        int kolor = 7;
        PropertyInfo[] arr = typeof(Color).GetProperties();

        public MainWindow()
        {
            InitializeComponent();
            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            cmbFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            cmbFontColor.ItemsSource = typeof(Colors).GetProperties();
           

            //rtbEditor.FontFamily = new FontFamily("Sagoe UI");
            //cmbFontFamily.SelectedItem = new FontFamily("Segoe UI");
            //rtbEditor.FontSize = 20;
            //cmbFontSize.Text = "20";
            rtbEditor.Foreground = Brushes.Black;
            cmbFontColor.SelectedItem = typeof(Colors).GetProperties()[7];



            textBox.Text = "Word Count: 0";
            lblCursorPosition.Text = "Line: " + "1" + " Column: " + "1";

        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        public static List<Color> ColorStructToList()
        {
            return typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public)
                                .Select(c => (Color)c.GetValue(null, null))
                                .ToList();
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            string text = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd).Text;
             
            if (saved ==0 && !String.IsNullOrWhiteSpace(text))
            {
                if (MessageBox.Show("Do you want to save before closing?",
                    "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (empty != 1)
                    {
                        save.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
                    else
                    {
                        saveas.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
                }
                else
                {
                    // Do not close the window
                }
            }
            
            this.Close();
        }


        private void rtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // toggle buttons
            object temp = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
            btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));

            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontStyleProperty);
            btnItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));

            temp = rtbEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            btnUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));

            //comboboxs
            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            cmbFontFamily.SelectedItem = temp;
            

            temp = rtbEditor.Selection.GetPropertyValue(Inline.FontSizeProperty);
            cmbFontSize.Text = temp.ToString();

            temp = rtbEditor.Selection.GetPropertyValue(Inline.ForegroundProperty);

            var hexcolor = ((SolidColorBrush)temp).Color.ToString();
            SolidColorBrush sol = (SolidColorBrush)temp;


           
            saved = 0;

            string s = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd).Text;
            char[] delimiters = new char[] { ' ', '\r', '\n' };
            string[] pom = s.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            textBox.Text = "Word Count: " + pom.Length.ToString();

            TextPointer tp1 = rtbEditor.Selection.Start.GetLineStartPosition(0);
            TextPointer tp2 = rtbEditor.Selection.Start;

            int column = tp1.GetOffsetToPosition(tp2) +1;

            int someBigNumber = int.MaxValue;
            int lineMoved, currentLineNumber;
            rtbEditor.Selection.Start.GetLineStartPosition(-someBigNumber, out lineMoved);
            currentLineNumber = -lineMoved + 1;

            lblCursorPosition.Text = "Line: " + currentLineNumber.ToString() + " Column: " + column.ToString();

        }

        private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontFamily.SelectedItem != null)
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
        }

        

        private void cmbFontColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFontColor.SelectedItem != null)
            {
                kolor = cmbFontColor.SelectedIndex;
                var selectedItem = (PropertyInfo)cmbFontColor.SelectedItem;
                var color = (Color)selectedItem.GetValue(null, null);

                rtbEditor.Selection.ApplyPropertyValue(Inline.ForegroundProperty, color.ToString());
                 r =  (Color)System.Windows.Media.ColorConverter.ConvertFromString(color.ToString());
                
                

            }
        }

        private void open_Click(object sender, RoutedEventArgs e)
        {
            string text = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd).Text;

            if (saved == 0 && !String.IsNullOrWhiteSpace(text))
            {
                if (MessageBox.Show("Do you want to save before opening?",
                    "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (empty != 1)
                    {
                        save.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
                    else
                    {
                        saveas.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
                }
                else
                {
                    // Do not close the window
                }
            }
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(dlg.FileName, FileMode.Open);
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                range.Load(fileStream, DataFormats.Rtf);
                file = fileStream.Name;
                saved = 1;
                empty = 0;
                fileStream.Close();
                rtbEditor.FontFamily = new FontFamily("Sagoe UI");
                cmbFontFamily.SelectedItem = new FontFamily("Segoe UI");
                rtbEditor.FontSize = 20;
                cmbFontSize.Text = "20";
                rtbEditor.Foreground = Brushes.Black;
                cmbFontColor.SelectedItem = typeof(Colors).GetProperties()[7];
            }
        }

        private void saveas_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(dlg.FileName, FileMode.Create);
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                range.Save(fileStream, DataFormats.Rtf);
                file = fileStream.Name;
                saved = 1;
                empty = 0;
                fileStream.Close();
            }

        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (empty == 0)
            {
                icon.Spin = true; // nikad nece se vrteti jer brzo se sacuva

                FileStream fileStream = new FileStream(file, FileMode.Create);
                TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
                range.Save(fileStream, DataFormats.Rtf);
                file = fileStream.Name;
                //for (double i = 0; i < 1000000.0; i=i+0.1) { } simulacija za cuvanje NE RADI
                icon.Spin = false;
                saved = 1;
                fileStream.Close();
            }
            else
            {
                MessageBox.Show("You need to save as!");
                saveas.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            }
        }

        private void FRButton_Click(object sender, RoutedEventArgs e)
        {
            string keyword = find.Text;
            string newString = replace.Text;

            TextRange text = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
            TextPointer current = text.Start.GetInsertionPosition(LogicalDirection.Forward);

            while (current != null)
            {
                string textInRun = current.GetTextInRun(LogicalDirection.Forward);

                if (!string.IsNullOrWhiteSpace(textInRun))
                {
                    int index = textInRun.IndexOf(keyword);
                    if (index != -1)
                    {
                        TextPointer selectionStart = current.GetPositionAtOffset(index, LogicalDirection.Forward);
                        TextPointer selectionEnd = selectionStart.GetPositionAtOffset(keyword.Length, LogicalDirection.Forward);
                        TextRange selection = new TextRange(selectionStart, selectionEnd);
                        var font = selection.GetPropertyValue(FontFamilyProperty);
                        var bold = selection.GetPropertyValue(FontWeightProperty);
                        var italic = selection.GetPropertyValue(FontStyleProperty);
                        var size = selection.GetPropertyValue(FontSizeProperty);
                        var color = selection.GetPropertyValue(ForegroundProperty);
                        var underline = selection.GetPropertyValue(Inline.TextDecorationsProperty);
                        selection.Text = newString;
                        selection.ApplyPropertyValue(TextElement.FontFamilyProperty, font);
                        selection.ApplyPropertyValue(TextElement.FontWeightProperty, bold);
                        selection.ApplyPropertyValue(TextElement.FontStyleProperty, italic);
                        selection.ApplyPropertyValue(TextElement.FontSizeProperty, size);
                        selection.ApplyPropertyValue(TextElement.ForegroundProperty, color);
                        selection.ApplyPropertyValue(Inline.TextDecorationsProperty,underline);
                        rtbEditor.Selection.Select(selection.Start, selection.End);
                        rtbEditor.Focus();
                    }
                }
                current = current.GetNextContextPosition(LogicalDirection.Forward);
                find.Text = "";
                replace.Text = "";
            }
        }


        private void mouseDown(object sender, MouseEventArgs e)
        {
            find.Text = "";
        }

        private void mouseDown1(object sender, MouseEventArgs e)
        {
            replace.Text = "";
        }

        private void buttonDT_Click(object sender, RoutedEventArgs e)
        {
            string Datum = DateTime.Now.ToString("dd/MM/yyyy");
            string Vreme = DateTime.Now.ToString("hh:mm:ss tt");
            //DT.Text = Datum + ' ' + Vreme;


            rtbEditor.CaretPosition.InsertTextInRun(" "+ DateTime.Now.ToString() + " ");

            rtbEditor.CaretPosition = rtbEditor.Document.ContentEnd;

        }

        private void new_Click(object sender, RoutedEventArgs e)
        {
            string text = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd).Text;

            if (saved == 0 && !String.IsNullOrWhiteSpace(text))
            {
                if (MessageBox.Show("Do you want to save before new document?",
                    "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if(empty != 1)
                    {
                        save.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
                    else
                    {
                        saveas.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
                    
                }
                else
                {
                    // Do not close the window
                }
                empty = 1;
            }

            rtbEditor.Document.Blocks.Clear();
            saved = 0;
            empty = 1;
            textBox.Text = "";
            rtbEditor.FontFamily = new FontFamily("Sagoe UI");
            cmbFontFamily.SelectedItem = new FontFamily("Segoe UI");
            rtbEditor.FontSize = 20;
            cmbFontSize.Text = "20";
            rtbEditor.Foreground = Brushes.Black;
            cmbFontColor.SelectedItem = typeof(Colors).GetProperties()[7];
        }


        private void pressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.K)
            {
                MessageBox.Show("Do you want to save before opening?",
                      "Confirmation", MessageBoxButton.YesNo) ;
            }
                buttonDT.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }


        private void cmbFontSize_TextChanged(object sender, RoutedEventArgs e)
        {
            try {
                rtbEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text);
            }
            catch
            {
                cmbFontSize.Text = "b";
            }
        }
    }
}
