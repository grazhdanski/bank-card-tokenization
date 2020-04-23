using System;
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
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Net;
using wox.serial;
using System.Collections;
using System.Text.RegularExpressions;

namespace BankCardTokenization
{
    /// <summary>
    /// Represents a multithreaded server that manages operations with
    /// bank card numbers and their corresponding tokens. Supports card number tokenization
    /// as well as token-to-card-number requests.
    /// </summary>

    public partial class MainWindow : Window
    {
        private const string USERS_PATH = "users.xml"; // Path to the users XML file
        private const string CARD_PATH = "cards.xml"; // Path to the cards/tokens XML file

        ArrayList users = new ArrayList(); // Contains UserDetails objects
        ArrayList cardToToken = new ArrayList(); // Contains CardNumberContainer objects

        private const byte CARD_NUMBER_LENGTH = 16; // Length of a valid bank card number
        private static Random randomDigitGenerator = new Random();

        // A valid token must start with one of these digits
        private static char[] validFirstDigitsToken = { '1', '2', '7', '8', '9' };

        // Regular expressions used to validate card numbers and tokens
        private static Regex validCardNumber = new Regex("([3456][0-9]{15})");
        private static Regex validToken = new Regex("[12789][0-9]{15}");

        public MainWindow()
        {
            InitializeComponent();

            // Load the users and cards/tokens from the corresponding XML files
            users = (ArrayList)Easy.load(USERS_PATH);
            if(users == null)
            {
                users = new ArrayList();
            }

            cardToToken = (ArrayList)Easy.load(CARD_PATH);
            if(users == null)
            {
                users = new ArrayList();
            }

            // Run the tokenization server
            Thread readThread = new Thread(new ThreadStart(RunServer));
            readThread.Start();
        }

        private void RunServer()
        {
            IPAddress local = IPAddress.Parse("127.0.0.1");
            TcpListener listener = new TcpListener(local, 50000);
            listener.Start();

            Socket connection;

            try
            {
                while (true)
                {
                    LogMessage("Waiting for connection...");
                    connection = listener.AcceptSocket();

                    // Service the new client in a separate thread from the Thread Pool
                    ThreadPool.QueueUserWorkItem(RunInThread, connection);
                }
            }
            catch (Exception error)
            {
                // An exception occurred, output it to the log
                LogMessage(error.Message.ToString());
            }
        }

        // Service a client
        private void RunInThread(object connection)
        {
            Socket currentConnection = connection as Socket;

            if (currentConnection != null)
            {
                NetworkStream stream = new NetworkStream(currentConnection);
                BinaryReader reader = new BinaryReader(stream);
                BinaryWriter writer = new BinaryWriter(stream);
                LogMessage("User Connected");

                try
                {

                    // Read  user credentials
                    string userName = reader.ReadString();
                    string password = reader.ReadString();

                    lock (this)
                    {
                        bool isValidUser = IsValidUser(userName, password);

                        // Inform the client wether the user is valid or not
                        writer.Write(isValidUser);

                        if (isValidUser)
                        {
                            // Find the user object
                            UserDetails currentUser = new UserDetails();
                            foreach (UserDetails user in users)
                            {
                                if (user.UserName == userName)
                                {
                                    currentUser = user;
                                    break;
                                }
                            }

                            // Read the operation code
                            string operation = reader.ReadString();

                            if (operation == "register")
                            {
                                // Inform the client wether the user has permission to register tokens or not
                                writer.Write(currentUser.CanRegisterTokens);

                                if (currentUser.CanRegisterTokens)
                                {

                                    // Read the card number
                                    string cardNumber = reader.ReadString();

                                    if (IsValidCardNumber(cardNumber))
                                    {
                                        writer.Write(true); // Operation will be attempted

                                        // Generate a unique token
                                        string generatedToken = "";
                                        do
                                        {
                                            generatedToken = Tokenize(cardNumber);

                                        } while (IsDuplicateToken(generatedToken));

                                        writer.Write(generatedToken); // Send the token back to the client

                                        // Find the container to insert the generated token into
                                        CardNumberContainer currentCardNumberContainer = new CardNumberContainer("undefined");

                                        foreach (CardNumberContainer container in cardToToken)
                                        {
                                            if (container.CardNumber == cardNumber)
                                            {
                                                currentCardNumberContainer = container;
                                                break;
                                            }
                                        }

                                        if (currentCardNumberContainer.CardNumber != "undefined")
                                        {
                                            // Container not found, add a new container
                                            currentCardNumberContainer.Add(generatedToken);
                                        }
                                        else
                                        {
                                            // Container already exists, add the new token into it
                                            CardNumberContainer newContainer = new CardNumberContainer(cardNumber);
                                            newContainer.Add(generatedToken);

                                            cardToToken.Add(newContainer);
                                        }
                                    }
                                    else // The card number is not valid
                                    {
                                        // Inform the client that the  requested operation failed
                                        writer.Write(false);
                                    }
                                }
                            }
                            else if (operation == "request")// The operation is "request"
                            {
                                // Inform the client wether the user has permission to request
                                // card numbers or not
                                writer.Write(currentUser.CanRequestCardNumbers);

                                if (currentUser.CanRequestCardNumbers)
                                {
                                    // Read the token
                                    string token = reader.ReadString();

                                    // Try to find the token
                                    bool tokenFound = false;
                                    string cardNumberToReturn = String.Empty;

                                    foreach (CardNumberContainer cardContainer in cardToToken)
                                    {
                                        foreach (string item in cardContainer.Tokens)
                                        {
                                            if (item == token)
                                            {
                                                tokenFound = true;
                                                cardNumberToReturn = cardContainer.CardNumber;
                                                break;
                                            }
                                        }
                                    }

                                    if (tokenFound)
                                    {
                                        // Operation is successful, inform user and return the card number
                                        writer.Write(true);
                                        writer.Write(cardNumberToReturn);
                                    }
                                    else
                                    {
                                        // Operation failed, token does not exist
                                        writer.Write(false);
                                    }
                                }
                            }
                            else if (operation == "fetchByCard" || operation == "fetchByToken")
                            {
                                // Access granted, every registered user can fetch entries
                                writer.Write(true);

                                string output = "";

                                lock (this)
                                {
                                    if (operation == "fetchByCard")
                                    {
                                        // Query the cardToToken ArrayList and order 
                                        //entries by their card number

                                        var entriesOrderedByCardNumber =
                                        from CardNumberContainer container in cardToToken
                                        orderby container.CardNumber
                                        select container;

                                        // Append the data of each entry to the output string
                                        foreach (CardNumberContainer item in entriesOrderedByCardNumber)
                                        {
                                            output += item.CardNumber + "   ";
                                            foreach (string token in item.Tokens)
                                            {
                                                output += token + " ";
                                            }
                                            output += "\n";
                                        }
                                    }
                                    else if (operation == "fetchByToken")
                                    {
                                        // Query the cardToToken ArrayList and order 
                                        //entries by their first token

                                        var entiresOrderedByFirstToken =
                                        from CardNumberContainer container in cardToToken
                                        orderby container.Tokens[0]
                                        select container;

                                        // Append the data of each entry to the output string
                                        foreach (CardNumberContainer item in entiresOrderedByFirstToken)
                                        {
                                            output += item.CardNumber + "   ";
                                            foreach (string token in item.Tokens)
                                            {
                                                output += token + " ";
                                            }
                                            output += "\n";
                                        }
                                    }
                                }

                                writer.Write(true); // Operation done
                                writer.Write(output); // Return the output string to the client
                            }
                        }
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

                    // Save the contents of users and cardToToken ArrayLists 
                    // to their corresponding XML file

                    Easy.save(users, USERS_PATH);
                    Easy.save(cardToToken, CARD_PATH);

                    LogMessage("User Disconnected\n");
                }
            }
        }

        // Validate a user login attempt
        private bool IsValidUser(string userName, string password)
        {
            bool foundUser = false;
            foreach (UserDetails user in users)
            {
                if (user.UserName == userName && user.Password == password)
                {
                    foundUser = true;
                    break;
                }
            }
            return foundUser;
        }

        private static bool IsValidCardNumber(string input)
        {

            if (!validCardNumber.IsMatch(input))
            {
                return false;
            }

            // Use Luhn algorithm to validate the card number
            int sum = 0;
            int currentDigit = 0;
            for (int i = 0; i < CARD_NUMBER_LENGTH; i++)
            {

                currentDigit = (int)input[i] - 48;
                if (i % 2 == 0) // On even indexes add the value of the digit * 2
                {
                    currentDigit *= 2;
                    if (currentDigit > 9)
                    {
                        currentDigit %= 10;
                        sum++;
                    }
                }
                sum += currentDigit;

            }

            return sum % 10 == 0;
        }

        private bool IsDuplicateToken(string input)
        {
            foreach (CardNumberContainer cardContainer in cardToToken)
            {
                if (cardContainer.Contains(input))
                {
                    return true;
                }
            }

            return false;
        }

        // Generate a token representation of a valid card number
        private static string Tokenize(string input)
        {
            char[] token = new char[CARD_NUMBER_LENGTH];

            token[0] = validFirstDigitsToken[randomDigitGenerator.Next(0, validFirstDigitsToken.Length)];

            for (int i = 1; i < CARD_NUMBER_LENGTH; i++)
            {
                if (i < CARD_NUMBER_LENGTH - 4) // Only replace the first 12 digits
                {
                    // Assign a new random digit, different from the one in the input
                    do
                    {
                        token[i] = (char)(48 + randomDigitGenerator.Next(0, 10));

                    } while (token[i] == input[i]);
                }
                else
                {
                    token[i] = input[i];
                }
            }

            // Calculate the sum of the token digits
            int digitSum = 0;
            for (int i = 0; i < CARD_NUMBER_LENGTH; i++)
            {
                digitSum += (int)token[i] - 48;
            }

            // Ensure that te sum is not divisible by 10
            if (digitSum % 10 == 0)
            {
                // Find the first digit that is not 9 and increment it
                for (int i = 0; i < CARD_NUMBER_LENGTH; i++)
                {
                    if (token[i] != '9')
                    {
                        token[i]++;
                        break;
                    }
                }
            }

            return new String(token);
        }

        private void LogMessage(string message)
        {
            if (Dispatcher.CheckAccess())
            {
                txtBoxServerLog.Text += message + "\n";
            }
            else
            {
                Dispatcher.Invoke(() => { txtBoxServerLog.Text += message + "\n"; });
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }
    }
}
