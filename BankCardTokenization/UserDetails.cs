using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankCardTokenization
{
    /// <summary>
    /// Class: UserDetails
    /// Description: Contains user account information
    /// </summary>
    public class UserDetails
    {
        #region Data Members
        string userName;
        string password;

        // User privileges
        bool canRegisterTokens;
        bool canRequestCardNumbers;
        #endregion

        #region Properties
        public string UserName
        {
            get { return userName; }
            set
            {
                if (value != null)
                {
                    userName = value;
                }
                else
                {
                    userName = "Undefined";
                }
            }
        }

        public string Password
        {
            get { return password; }
            set
            {
                if (value != null)
                {
                    password = value;
                }
                else
                {
                    password = "Undefined";
                }
            }
        }

        public bool CanRegisterTokens
        {
            get
            {
                return canRegisterTokens;
            }

            set
            {
                canRegisterTokens = value;
            }
        }

        public bool CanRequestCardNumbers
        {
            get
            {
                return canRequestCardNumbers;
            }

            set
            {
                canRequestCardNumbers = value;
            }
        }
        #endregion

        #region Constructors
        public UserDetails(string userName, string password,
           bool canRegisterTokens, bool canRequesCardNumbers)
        {
            UserName = userName;
            Password = password;
            CanRegisterTokens = canRegisterTokens;
            CanRequestCardNumbers = canRequesCardNumbers;
        }

        public UserDetails() { }
        #endregion

        #region Utility Methods
        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3}", userName, password, canRegisterTokens, canRequestCardNumbers);
        } 
        #endregion
    }
}
