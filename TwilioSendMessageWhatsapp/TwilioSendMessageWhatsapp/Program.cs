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
            var isSendType = Convert.ToInt32(ConfigurationManager.AppSettings["IS_SEND_TYPE"]);
            var accountSid = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_SID"];
            var authToken = ConfigurationManager.AppSettings["TWILIO_AUTH_TOKEN"];
            var connectionString = ConfigurationManager.ConnectionStrings["cnxTwilioDB"].ToString();
            var addHours = Convert.ToDouble(ConfigurationManager.AppSettings["DIFERENCIA_HORAS"]);

            var messageMediaUrl = ConfigurationManager.AppSettings["MessageMediaUrl"].ToString();
            var messageMediaUrlVideoYouTube = ConfigurationManager.AppSettings["MessageMediaUrlVideoYouTube"].ToString();
            var messageMediaUrlWeb = ConfigurationManager.AppSettings["MessageMediaUrlWeb"].ToString();

            var phoneNumberWhatsappFrom = ConfigurationManager.AppSettings["TWILIO_PhoneNumberWhatsappFrom"].ToString();
            var phoneNumberWhatsappTo = ConfigurationManager.AppSettings["TEST_PhoneNumberWhatsappTo"].ToString();

            var templateMessageA = ConfigurationManager.AppSettings["TEST_TEMPLATE_MESSAGE_A"].ToString();
            templateMessageA = templateMessageA.Replace("\\n", "\n\r");

            var templateMessageB = ConfigurationManager.AppSettings["TEST_TEMPLATE_MESSAGE_B"].ToString();
            templateMessageB = templateMessageB.Replace("\\n", "\n\r");

            var fechaPrueba = ConfigurationManager.AppSettings["TEST_FechaNotificacion"].ToString();

            string name = ConfigurationManager.AppSettings["TEST_Name"].ToString();

            string isNewRegisterSetting = ConfigurationManager.AppSettings["IS_NEW_REGISTER"].ToString();


            // Una prueba de envío
            if (isSendType == 0)
            {

                TwilioClient.Init(accountSid, authToken);

                List<Uri> lstMediaUrl = new List<Uri>
                {
                    new Uri(messageMediaUrl)
                };

                var isNewRegister = Convert.ToBoolean(isNewRegisterSetting);

                var messageConditionalTest = (isNewRegister) ? string.Format(templateMessageA, name, messageMediaUrlWeb) : string.Format(templateMessageB, name, messageMediaUrlVideoYouTube, messageMediaUrlWeb);

                var message = MessageResource.Create(
                    body: messageConditionalTest,
                    from: new Twilio.Types.PhoneNumber("whatsapp:" + phoneNumberWhatsappFrom),
                    to: new Twilio.Types.PhoneNumber("whatsapp:+51" + phoneNumberWhatsappTo),
                    mediaUrl: (isNewRegister) ? lstMediaUrl : null
                );

                Console.WriteLine(message.Sid);
                Console.WriteLine(message.DateSent);
                Console.ReadLine();
                Console.WriteLine("Prueba de Ejecución");
                Console.ReadLine();

            // Producción
            } else {

                var dateNow = (isSendType == 2)? DateTime.Now.AddHours(addHours).ToString("yyyy-MM-dd") : fechaPrueba;

                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    // Connection
                    SqlCommand command = new SqlCommand();
                    command.Connection = sqlConnection;

                    // Defined Query
                    command.CommandText = "[dbo].[sp_ListarUsuariosParaNotificacion]";
                    command.CommandType = CommandType.StoredProcedure;

                    // Parameters
                    SqlParameter parameter = new SqlParameter();
                    parameter.ParameterName = "@fecha_notificacion";
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

                                // Log
                                name = reader[1].ToString().Trim();
                                int Id = Convert.ToInt32(reader[0]);
                                int State = -1;
                                string Message = "";
                                phoneNumberWhatsappTo = reader[2].ToString();

                                bool newRegister = Convert.ToBoolean(reader[3]);
                                var messageConditional = "";

                                try
                                {
                        
                                    messageConditional = (newRegister) ? string.Format(templateMessageA, name, messageMediaUrlWeb) : string.Format(templateMessageB, name, messageMediaUrlVideoYouTube, messageMediaUrlWeb);

                                    
                                    // -- Integration Twilio -- //
                                    TwilioClient.Init(accountSid, authToken);

                                    List<Uri> lstMediaUrl = new List<Uri>
                                    {
                                        new Uri(messageMediaUrl)
                                    };

                                    var message = MessageResource.Create(
                                        body: messageConditional,
                                        from: new Twilio.Types.PhoneNumber("whatsapp:" + phoneNumberWhatsappFrom),
                                        to: new Twilio.Types.PhoneNumber("whatsapp:+51" + phoneNumberWhatsappTo),
                                        mediaUrl: (newRegister)? lstMediaUrl : null
                                    );

                                    State = 1;
                                    Message = message.Sid;
                                    // -- End --// 

                                }
                                catch (Exception ex)
                                {
                                    State = 0;
                                    Message = "Ocurrió un problema en el envío" + ex.Message;
                                }

                                // Connection
                                SqlCommand update = new SqlCommand();
                                update.Connection = sqlConnection;

                                // Defined Query
                                update.CommandText = "[dbo].[sp_ActualizarUsuarioNotificacion]";
                                update.CommandType = CommandType.StoredProcedure;

                                // Parameters

                                // Id
                                SqlParameter parameterId = new SqlParameter();
                                parameterId.ParameterName = "@id";
                                parameterId.SqlDbType = SqlDbType.Int;
                                parameterId.Direction = ParameterDirection.Input;
                                parameterId.Value = Id;
                                update.Parameters.Add(parameterId);

                                // Estado
                                SqlParameter parameterState = new SqlParameter();
                                parameterState.ParameterName = "@estado";
                                parameterState.SqlDbType = SqlDbType.Int;
                                parameterState.Direction = ParameterDirection.Input;
                                parameterState.Value = State;
                                update.Parameters.Add(parameterState);

                                // Mensaje
                                SqlParameter parameterMessage = new SqlParameter();
                                parameterMessage.ParameterName = "@mensaje";
                                parameterMessage.SqlDbType = SqlDbType.VarChar;
                                parameterMessage.Direction = ParameterDirection.Input;
                                parameterMessage.Value = Message;
                                update.Parameters.Add(parameterMessage);

                                update.ExecuteNonQuery();

                                Console.WriteLine("{0}: {1:C}, {2:C}, {3:C}", reader[0], reader[1], reader[2], reader[3]);
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
