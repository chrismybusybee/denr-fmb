using FMB_CIS.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlTypes;

namespace FMB_CIS.Data
{
    public class DAL
    {
        //private readonly IConfiguration _configuration;

        //public DAL(IConfiguration configuration)
        //{
        //    this._configuration = configuration;
        //}

        public bool emailExist(string email, string connectString)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {

                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("SelectExistingEmail", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("email", email);

                if (sqlCmd.ExecuteScalar() != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public string selectEncrPassFromEmail(string email, string connectString)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {

                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("SelectPassFromEmail", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("email", email);

                return sqlCmd.ExecuteScalar().ToString();

            }
        }

        public string selectFullNameFromEmail(string email, string connectString)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {

                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("SelectFullNameFromEmail", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("email", email);

                return sqlCmd.ExecuteScalar().ToString();

            }
        }

        public string selectUserIDFromEmail(string email, string connectString)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {

                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("SelectUserIDFromEmail", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("email", email);

                //return Convert.ToInt32(sqlCmd.ExecuteScalar());
                return sqlCmd.ExecuteScalar().ToString();

            }
        }

        public string selectUserRoleFromEmail(string email, string connectString)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {

                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("SelectUserTypeIDFromEmail", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("email", email);
                int usrTypeID = Convert.ToInt32(sqlCmd.ExecuteScalar());
                string userRole = "";
                switch (usrTypeID)
                {
                    case 1:
                        userRole = "Chainsaw Importer";
                        break;
                    case 2:
                        userRole = "Chainsaw Seller";
                        break;
                    case 3:
                        userRole = "Chainsaw Owner";
                        break;
                    case 4:
                        userRole = "Chainsaw Importer and Seller";
                        break;
                    case 5:
                        userRole = "Chainsaw Importer and Owner";
                        break;
                    case 6:
                        userRole = "Chainsaw Seller and Owner";
                        break;
                    case 7:
                        userRole = "Chainsaw Importer, Owner, and Seller";
                        break;
                }
                return userRole;
                //return Convert.ToInt32(sqlCmd.ExecuteScalar());
                //return sqlCmd.ExecuteScalar().ToString();

            }
        }

    }
}
