using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace TwilioSendMessageWhatsapp
{
    class Program
    {
        static void Main(string[] args)
        {
            var isDev = Convert.ToInt32(ConfigurationManager.AppSettings["IS_DEV"]);

            if(isDev == 1)
            {
                Console.WriteLine("Prueba de Ejecución");
                Console.ReadLine();

            } else
            {
                var accountSid = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_SID"];
                var authToken = ConfigurationManager.AppSettings["TWILIO_AUTH_TOKEN"];
                var connectionString = ConfigurationManager.ConnectionStrings["cnxTwilioDB"].ToString();
                var addHours = Convert.ToDouble(ConfigurationManager.AppSettings["DIFERENCIA_HORAS"]);

                var dateNow = DateTime.Now.AddHours(addHours).ToString("yyyy-MM-dd");

                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    // Connection
                    SqlCommand command = new SqlCommand();
                    command.Connection = sqlConnection;

                    // Defined Query
                    command.CommandText = "[dbo].[SP_USERS_LIST]";
                    command.CommandType = CommandType.StoredProcedure;

                    // Parameters
                    SqlParameter parameter = new SqlParameter();
                    parameter.ParameterName = "@FECHA";
                    parameter.SqlDbType = SqlDbType.VarChar;
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = dateNow;

                    command.Parameters.Add(parameter);

                    sqlConnection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // Integration Twilio
                                //TwilioClient.Init(accountSid, authToken);

                                //var message = MessageResource.Create(
                                //    body: "This is the ship that made the Kessel Run in fourteen parsecs?",
                                //    from: new Twilio.Types.PhoneNumber("+15017122661"),
                                //    to: new Twilio.Types.PhoneNumber("+15558675310")
                                //);

                                //Console.WriteLine(message.Sid);

                                //Console.WriteLine(accountSid + "-" + authToken);

                                //Console.WriteLine("{0}: {1:C}", reader[0], reader[1]);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No rows found.");
                        }
                        reader.Close();
                    }
                    sqlConnection.Close();
                }
            }
            

            
                

           
        }
    }
}
