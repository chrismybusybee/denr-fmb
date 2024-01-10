using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Utilities
{
    public static class ReferenceNumberGenerator
    {
        public static string GenerateTransactionReference(string legend, int  applicationID)
        {
            // You can use various strategies to generate a unique 16-digit reference number
            // For example, here's a simple approach using random numbers:

            Random random = new Random();
            StringBuilder reference = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                reference.Append(random.Next(0, 10)); // Appending random digits (0-9)
            
            }
            reference.Append(applicationID);

            //get 

            return "REF: "+ legend+"-"+reference.ToString();
        }
    }



}
