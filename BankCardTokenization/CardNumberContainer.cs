using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankCardTokenization
{
    /// <summary>
    /// Class: CardNumberContainer
    /// Description: Models a container for a bank card and its corresponding tokens
    /// </summary>
    public class CardNumberContainer
    {
        #region Data Members
        string cardNumber;
        List<string> tokens = new List<string>();
        #endregion

        #region Properties
        public string CardNumber
        {
            get { return cardNumber; }
            set
            {
                if (value != null)
                {
                    cardNumber = value;
                }
            }
        }

        public List<string> Tokens
        {
            get
            {
                List<string> temp = new List<string>();
                foreach (var item in tokens)
                {
                    temp.Add(item);
                }

                return temp;
            }
        }
        #endregion

        #region Constructors
        public CardNumberContainer(string cardNumber)
        {
            CardNumber = cardNumber;
        }

        public CardNumberContainer() { }
        #endregion

        #region Utility Methods
        public void Add(string value)
        {
            tokens.Add(value);
        }

        // A method to determine if a given token (string value) corresponds to this card number
        public bool Contains(string value)
        {
            foreach (var token in tokens)
            {
                if (value == token)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
