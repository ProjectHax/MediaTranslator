using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MediaTranslator
{
    public partial class MainWindow : Window
    {
        const string VERSION = "1.0.1";
        int src_index = 8, dest_index = 8;

        List<string> files = new List<string> {
            "textdata_equip&skill.txt",
            "textdata_object.txt",
            "textquest_otherstring.txt",
            "textquest_queststring.txt",
            "textquest_speech&name.txt",
            "textuisystem.txt",
            "textzonename.txt",
            "texthelp.txt",
            "textevent.txt"
        };

        public MainWindow()
        {
            InitializeComponent();
            Title = string.Format("{0} v{1}", Title, VERSION);
        }

        private Dictionary<string, string> Load(string path, int index)
        {
            var data = new Dictionary<string, string>();

            foreach (var f in files)
            {
                string text = File.ReadAllText(path + "/" + f);
                string[] lines = text.Split("\r\n");

                if (text.Contains(".txt"))
                {
                    text = string.Empty;

                    foreach (var l in lines)
                    {
                        if (l.Length > 0)
                            text += File.ReadAllText(path + "/" + l);
                    }

                    lines = text.Split("\r\n");
                }

                foreach (var l in lines)
                {
                    string[] split = l.Split("\t");
                    if (split.Length > index)
                    {
                        int i;
                        if (int.TryParse(split[1], out i))
                            data[split[2]] = split[index];
                        else
                            data[split[1]] = split[index];
                    }
                }
            }

            return data;
        }

        private void SourceOpen_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new CommonOpenFileDialog();
            ofd.IsFolderPicker = true;
            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Source.Text = string.Empty;

                foreach (var f in files)
                {
                    if (!File.Exists(ofd.FileName + "/" + f))
                    {
                        MessageBox.Show(string.Format("{0} does not exist in the source folder. Cannot continue.", f), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                Source.Text = ofd.FileName;
                Preview();
            }
        }

        private void DestOpen_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new CommonOpenFileDialog();
            ofd.IsFolderPicker = true;
            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Destination.Text = string.Empty;

                foreach (var f in files)
                {
                    if (!File.Exists(ofd.FileName + "/" + f))
                    {
                        MessageBox.Show(string.Format("{0} does not exist in the destination folder. Cannot continue.", f), "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                Destination.Text = ofd.FileName;
                Preview();
            }
        }

        private string ExtractPreview(string path)
        {
            var text = File.ReadAllText(path + "/" + files[0]);
            string[] lines = text.Split("\r\n");

            if (text.Contains(".txt"))
            {
                text = string.Empty;

                foreach (var l in lines)
                {
                    if (l.Length > 0)
                        text += File.ReadAllText(path + "/" + l);
                }

                lines = text.Split("\r\n");
            }

            if (lines.Length > 100)
            {
                string[] split = lines[100].Split("\t");
                if (split.Length > src_index)
                {
                    return split[src_index];
                }
            }

            return string.Empty;
        }

        private void Preview()
        {
            if (SourceIndex == null || DestIndex == null)
                return;

            try
            {
                src_index = Convert.ToInt32(SourceIndex.Text);
                dest_index = Convert.ToInt32(DestIndex.Text);
            }
            catch (Exception)
            {
                return;
            }

            if (Source.Text.Length > 0)
            {
                SourcePreview.Text = ExtractPreview(Source.Text);
            }

            if (Destination.Text.Length > 0)
            {
                DestPreview.Text = ExtractPreview(Destination.Text);
            }
        }

        private string Update(Dictionary<string, string> data, string[] lines)
        {
            string text = string.Empty;

            foreach (var l in lines)
            {
                var split = l.Split("\t");

                if (split.Length > dest_index)
                {
                    string? val = split[dest_index];

                    int i;
                    if (int.TryParse(split[1], out i))
                    {
                        data.TryGetValue(split[2], out val);
                    }
                    else
                    {
                        data.TryGetValue(split[1], out val);
                    }

                    if (val != null)
                        split[dest_index] = val;
                }

                text += string.Join('\t', split) + "\r\n";
            }

            return text;
        }

        private void Write(string path, string text)
        {
            using (StreamWriter sw = new StreamWriter(File.Open(path, FileMode.Create), Encoding.Unicode))
            {
                sw.Write(text);
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (Source.Text == Destination.Text)
            {
                MessageBox.Show("Source and destination folders cannot be the same.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Source.Text.Length == 0)
            {
                MessageBox.Show("Source path has not been set", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Destination.Text.Length == 0)
            {
                MessageBox.Show("Destination path has not been set.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Source.IsEnabled = false;
            Destination.IsEnabled = false;
            SourceOpen.IsEnabled = false;
            DestOpen.IsEnabled = false;

            var data = Load(Source.Text, src_index);

            foreach (var f in files)
            {
                var text = File.ReadAllText(Destination.Text + "/" + f);
                string[] lines = text.Split("\r\n");

                if (text.Contains(".txt"))
                {
                    text = string.Empty;

                    foreach (var l in lines)
                    {
                        if (l.Length > 0)
                        {
                            text = File.ReadAllText(Destination.Text + "/" + l);
                            lines = text.Split("\r\n");

                            string result = Update(data, lines);
                            Write(Destination.Text + "/" + l, result);
                        }
                    }
                }
                else
                {
                    string result = Update(data, lines);
                    Write(Destination.Text + "/" + f, result);
                }
            }

            MessageBox.Show("Copy has finished successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SourceIndex_TextChanged(object sender, TextChangedEventArgs e)
        {
            Preview();
        }

        private void DestIndex_TextChanged(object sender, TextChangedEventArgs e)
        {
            Preview();
        }
    }
}