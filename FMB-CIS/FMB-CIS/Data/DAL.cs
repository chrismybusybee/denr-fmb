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

        public string selectFirstNameFromEmail(string email, string connectString)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectString))
            {

                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("SelectFirstNameFromEmail", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("email", email);

                return sqlCmd.ExecuteScalar().ToString();

            }
        }

    }
}
