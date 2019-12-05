using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Data;
using System.Data.SqlClient;


//Pagina
//Valida_Login
//Autentica_Login
//Menutree_Esquerdo
//MenuNivel
//Generico
//Check_Sessao
//S_mensagem

namespace ConsoleAppWsREST
{
    public class Distribuidor
    {
        public static string Pagina(HttpListenerRequest request, string cDataBaseCFG, string cDataBase)
        {
            ArtLib MeuLib = new ArtLib();
            string cHtml = "ERRO: Html nao atribuido";
            string cDados = "";

            char[] charSeparators = new char[] { '/' };
            string[] MeuPath = request.Url.LocalPath.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            string cMeuPath = MeuPath[MeuPath.Length - 1];


            string connectionString = "server=.\\SQLEXPRESS; Database=TESTE;Integrated Security=SSPI;";
                // "Data Source=.\\SQLEXPRESS;AttachDbFilename=C:\\dados\\NORTHWND.MDF;Integrated Security=True; Connect Timeout=30;User Instance=True";
            SqlConnection sqlConn = new SqlConnection(connectionString);

            sqlConn.Open();

            if (!cMeuPath.Contains(".css") && !cMeuPath.Contains(".js"))
            {
                LogFile.Log(string.Format(" --- Distribuidor - {0} - {1}", DateTime.Now.ToString("dd-MM-yyyy h:mm:ss tt"), cMeuPath));
            }


            do
            {
                if (request.HttpMethod == "POST")
                {
                    //Conversao para Encoding.UTF8 resolveu o problema de retornar em 2 chars o (Ã) - \u00C3\u0081 = encodeURI(chr) = %C3%81
                    using (var reader = new StreamReader(request.InputStream, Encoding.UTF8))
                    {
                        cDados = reader.ReadToEnd();

                        if ((cDados.IndexOf("dados=") >= 0) && (cDados.IndexOf("dados=") <= 1))
                        {
                            cDados = HttpUtility.UrlDecode(cDados);
                        }

                        //if (cDados.IndexOf("\u00C3") >= 0)
                        //{
                        //    cDados = HttpUtility.HtmlDecode(cDados);
                        //}
                    }
                    /*
                    int nTam = 1024;
                    byte[] formData = new byte[nTam];
                    request.InputStream.Read(formData, 0, nTam);
                    cDados = Encoding.ASCII.GetString(formData);
                    if  ((cDados.IndexOf("dados=") >= 0) && (cDados.IndexOf("dados=") <= 1))
                    {
                        cDados = HttpUtility.UrlDecode(cDados.Replace("\0", ""));
                    }
                    else
                    {
                        cDados =cDados.Replace("\0", "");
                    }
                    */
                }

                /*
                if (request.Url.LocalPath.Contains("/login"))
                {
                    cHtml = Resources.ResourceManager.GetObject("Login").ToString();
                }
                else 
                */

                if ((cMeuPath == "obter_dados") && (request.HttpMethod == "GET"))
                {
                    cHtml = Obter_Dados(request, MeuLib, cDados, sqlConn);
                }
                else if ((cMeuPath == "retorna_status") && !(String.IsNullOrEmpty(cDados)) && (request.HttpMethod == "POST"))
                {
                    cHtml = Retorna_Status(request, MeuLib, cDados, sqlConn);
                }
                else if ((cMeuPath == "obter_banco") && (request.HttpMethod == "GET"))
                {
                    cHtml = Obter_Banco(request, MeuLib, cDados, sqlConn);
                }
                else
                {
                    cHtml = string.Format("ERRO: Solicitacao nao atendida! {0}", DateTime.Now);
                }

            } while (false);

            sqlConn.Close();

            return cHtml;

        }


        private static string Obter_Dados(HttpListenerRequest request, ArtLib MeuLib, string cDados, SqlConnection sqlConn)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            dynamic oLogin = serializer.Deserialize<dynamic>(cDados);
            string cXJSon = "";
            string cJSon = "";
            string cChave = "";
            string cValor = "";

            LogFile.Log(" --- Obter_Dados:");

            SqlCommand cmd = new SqlCommand("SELECT * FROM tech_titulos WHERE status = '1'", sqlConn);
            SqlDataReader dr = cmd.ExecuteReader();

            cJSon = "{\"titulos\" : [";
            

            //percorre o SqlDataReader para obter os dados
            while (dr.Read())
            {

                if  (cXJSon.Trim() != "")
                {
                    cJSon += ",";
                }

                cXJSon = "";

                if ( !("antecipacao|coobrigacao|cancelamento|conciliacao|prorrogacao|bonificacao|devolucao".Contains(dr["operacao"].ToString())) )
                {
                    LogFile.Log(string.Format(" --- Distribuidor -- Obter_Dados - Operacao invalida! Operacao {0} ", dr["operacao"].ToString()));
                    continue;
                }

                cXJSon = Resources.ResourceManager.GetObject("json_"+ dr["operacao"].ToString()).ToString();

                for (int i = 0; i < dr.FieldCount; i++)
                {
                    cChave = string.Format("|{0}|", dr.GetName(i));

                    cValor = dr[i].ToString();

                    Type t = dr[i].GetType();

                    if  (t.Equals(typeof(decimal)))
                    {
                        cValor = cValor.Replace(".", "").Replace(",", ".");
                    }

                    cXJSon = cXJSon.Replace(cChave, cValor);
                }

                cJSon += cXJSon;

            }

            cJSon += "]}";


            //cJSon = Resources.ResourceManager.GetObject("json_geral").ToString();

            LogFile.Log(".");
            LogFile.Log("cJson:");
            LogFile.Log(cJSon);
            LogFile.Log(".");


            LogFile.Log(" --- Fim Obter_Dados!");

            return cJSon;

        }

        
        private static string Retorna_Status(HttpListenerRequest request, ArtLib MeuLib, string cDados, SqlConnection sqlConn)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string cQueryString = HttpUtility.UrlDecode(request.Url.Query);
            dynamic oLogin = serializer.Deserialize<dynamic>(cDados);
            dynamic aRetorno = oLogin["retorno"];
            string cJSon = "{ \"processado\" : [{0}] }";
            string cRetorno = "";
            string cStatus = "";
            string cMensagem = "";
            string cProcessou = "";
            string cIdProcesso = "";
            bool lSemErro = true;

            LogFile.Log(" --- retorna_status:");

            if (!String.IsNullOrEmpty(cQueryString))
            {
                int nP = (cQueryString.Contains("dados=") ? 7 : 1);
                oLogin = serializer.Deserialize<dynamic>(cQueryString.Substring(nP));
            }

            for (int h = 0; h < aRetorno.Length; h++)
            {

                lSemErro = true;

                for (int i = 0; i < aRetorno[h]["parcelas"].Length; i++)
                {

                    if (aRetorno[h]["parcelas"][i]["proc_ok"] == "S")
                    {
                        cStatus = "2";
                        cMensagem = "";
                    }
                    else
                    {
                        cStatus = "9";
                        cMensagem = aRetorno[h]["parcelas"][i]["mensagem"];
                        lSemErro = false;
                    }

                    cIdProcesso = aRetorno[h]["parcelas"][i]["id_processo"];

                    LogFile.Log(string.Format(" --- Retorno id: {0} - parcela: {1} - status: {2} - erro_mensag: {3}", cIdProcesso, aRetorno[h]["parcelas"][i]["parcela"], cStatus, cMensagem));

                    cProcessou = "erro";

                    if (lSemErro)
                    {

                        SqlCommand sqlComm = new SqlCommand(string.Format("UPDATE tech_titulos SET status='{0}', erro_mensag='{1}' WHERE id={2}", cStatus, cMensagem, cIdProcesso), sqlConn);
                        cProcessou = (sqlComm.ExecuteNonQuery() > 0 ? "ok" : "erro");

                    }

                    cRetorno = (h > 0 ? "," : "") + "{ " + string.Format("\"id_processo\":\"{0}\", \"processado\":\"{1}\" ", cIdProcesso, cProcessou) + "}{0}";

                    cJSon = cJSon.Replace("{0}", cRetorno);
                }

            }//for(h)

            cJSon = cJSon.Replace("{0}", "");

            LogFile.Log(".");
            LogFile.Log("cJson:");
            LogFile.Log(cJSon);
            LogFile.Log(".");

            LogFile.Log(" --- Fim retorna_status!");

            return cJSon;
        }


        private static string Obter_Banco(HttpListenerRequest request, ArtLib MeuLib, string cDados, SqlConnection sqlConn)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            dynamic oLogin = serializer.Deserialize<dynamic>(cDados);
            string cJSon = "";
            string cChave = "";
            string cValor = "";

            LogFile.Log(" --- Obter_Banco:");

            SqlCommand cmd = new SqlCommand("SELECT * FROM tech_banco", sqlConn);
            SqlDataReader dr = cmd.ExecuteReader();

            //percorre o SqlDataReader para obter os dados
            while (dr.Read())
            {

                cJSon = Resources.ResourceManager.GetObject("json_banco").ToString();

                for (int i = 0; i < dr.FieldCount; i++)
                {
                    cChave = string.Format("|{0}|", dr.GetName(i));

                    cValor = dr[i].ToString();

                    Type t = dr[i].GetType();

                    if (t.Equals(typeof(decimal)))
                    {
                        cValor = cValor.Replace(".", "").Replace(",", ".");
                    }

                    cJSon = cJSon.Replace(cChave, cValor);
                }

                break;  //Somente o primeiro banco

            }

 
            LogFile.Log(".");
            LogFile.Log("cJson:");
            LogFile.Log(cJSon);
            LogFile.Log(".");


            LogFile.Log(" --- Fim Obter_Banco!");

            return cJSon;

        }


    }
}
