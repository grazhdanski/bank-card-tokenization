using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace TokenizationClient
{
    /// <summary>
    /// Represents a client that connects to the tokenization server
    /// </summary>
    public partial class MainWindow : Window
    {
        NetworkStream stream;
        BinaryWriter writer;
        BinaryReader reader;

        private string textToWrite = String.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RunClient(string operation)
        {
            TcpClient client = new TcpClient("127.0.0.1", 50000);

            try
            {
                stream = client.GetStream();
                reader = new BinaryReader(stream);
                writer = new BinaryWriter(stream);

                LogMessage("Connected");

                // Send user credentials to the server
                writer.Write((string)Dispatcher.Invoke(() => txtBoxUserName.Text));
                writer.Write((string)Dispatcher.Invoke(() => txtBoxPassword.Password));

                if (reader.ReadBoolean()) // If the user is valid
                {
                    writer.Write(operation); // Send the operation code

                    if (reader.ReadBoolean()) // If the user has accesss
                    {

                        if (operation == "register")
                        {
                            // Send the card number
                            writer.Write((string)Dispatcher.Invoke(() => txtBoxCardNumber.Text));
                        }
                        else if (operation == "request")
                        {
                            // Send the token
                            writer.Write((string)Dispatcher.Invoke(() => txtBoxToken.Text));
                        }

                        if (reader.ReadBoolean()) // If the operation is succsessful
                        {
                            if (operation == "register")
                            {
                                // Read the generated token and display it
                                Dispatcher.Invoke(() => { txtBoxToken.Text = reader.ReadString(); });
                            }
                            else if (operation == "request")
                            {
                                // Read the card number and display it
                                Dispatcher.Invoke(() => { txtBoxCardNumber.Text = reader.ReadString(); });
                            }
                            else if (operation == "fetchByCard" || operation == "fetchByToken")
                            {
                                lock (this)
                                {
                                    // Read the the string that is to be outputed to a file
                                    textToWrite = reader.ReadString();
                                }
                            }
                        }
                        else // Operation failed
                        {

                            if (operation == "register")
                            {
                                LogMessage("Invalid Card Number");
                            }
                            else if (operation == "request")
                            {
                                LogMessage("Invalid Token");
                            }
                        }
                    }
                    else // User does not have access
                    {
                        if (operation == "register")
                        {
                            LogMessage("This user cannot register tokens");
                        }
                        else if (operation == "request")
                        {
                            LogMessage("This user cannot request card numbers");
                        }
                    }
                }
                else // Invalid user credentials
                {
                    LogMessage("Invalid User");
                }
            }
            catch (Exception error)
            {
                LogMessage(error.Message.ToString());
            }
            finally
            {
                reader?.Close();
                writer?.Close();
                stream?.Close();
                client?.Close();

                LogMessage("Disconnected\n");
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private async void btnRegisterToken_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => RunClient("register"));
        }

        private async void btnRequestCardNumber_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => RunClient("request"));
        }

        private void LogMessage(string message)
        {
            if (message == null)
            {
                return;
            }

            if (Dispatcher.CheckAccess())
            {
                txtConnectionLog.Text += message + "\n";
            }
            else
            {
                Dispatcher.Invoke(() => { txtConnectionLog.Text += message + "\n"; });
            }
        }

        private async void btnSaveByCardNumber_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt"; // Allow only .txt files

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await Task.Run(() => RunClient("fetchByCard"));

                    // If the operation was successful(i.e the textToWrite is not empty),
                    // write to file
                    if (textToWrite != String.Empty)
                    {
                        File.WriteAllText(saveFileDialog.FileName, textToWrite);
                    }
                }
                catch (Exception error)
                {
                    LogMessage(error.Message.ToString());
                }
                finally
                {
                    textToWrite = string.Empty;
                }
            }
        }

        private async void btnSaveByToken_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt"; // Allow only .txt files

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await Task.Run(() => RunClient("fetchByToken"));

                    // If the operation was successful(i.e the textToWrite is not empty),
                    // write to file
                    if (textToWrite != String.Empty)
                    {
                        File.WriteAllText(saveFileDialog.FileName, textToWrite);
                    }
                }
                catch (Exception error)
                {
                    LogMessage(error.Message.ToString());
                }
                finally
                {
                    textToWrite = string.Empty;
                }
            }
        }
    }
}
