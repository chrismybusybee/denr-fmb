using FMB_CIS.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlTypes;

namespace FMB_CIS.Data
{
    public class DAL
    {
        
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

            }
        }

        public bool isLinkValid(string tkncode, string connectString)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {

                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("spIsPasswordResetLinkValid", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("@GUID", tkncode);

                return Convert.ToBoolean(sqlCmd.ExecuteScalar());

            }
        }

        public bool changePasswordAndReturnIfChanged(string tkncode, string encrPass, string connectString)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("spChangePassword", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.Add("@GUID", SqlDbType.UniqueIdentifier).Value = new Guid (tkncode);
                sqlCmd.Parameters.AddWithValue("@password", encrPass);
                sqlCmd.ExecuteNonQuery();
                int returnedVal = Convert.ToInt32(sqlCmd.ExecuteScalar());

                if (Convert.ToInt32(sqlCmd.ExecuteScalar()) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
