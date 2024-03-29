﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

//Cobrir
//DesCobrir
//CobrirHTML
//DesCobrirHTML
//HTMLAcento
//JSONAcento
//Base64Encode
//Base64Decode
//ValCombo
//FMatriz
//

namespace ConsoleAppWsREST
{
    public class ArtLib
    {

        public int nTPagina = 20;  //Total de registros por pagina no browser

        string cAlfabeto = " 1234567890ABCDEFGHIJKLMNOPQRSTUVXZWYabcdefghijklmnopqrstuvxzwy -_=/?\\|*+.,;:!@#$%&()[]{}<>~^\"ÁÉÍÓÚáéíóúÂÊÔâêôÃÕãõÜüÇç©³¡'+#13+#10+«»";
        string cAlfHTML = " -_=/?\\|*+.,;:!@#$%&()[]{}<>~^\"1234567890ABCDEFGHIJKLMNOPQRSTUVXZWYabcdefghijklmnopqrstuvxzwy";
        string cDecimal = "0123456789";

        public string Cobrir(string cCodigo)
        {
            string cL = "";
            string cS = "";
            char cP;
            int nPos = 0;

            for (int i = 0; i < cCodigo.Length; i++)
            {
                cL = cCodigo.Substring(i, 1);

                nPos = cAlfabeto.IndexOf(cL);

                if (nPos < 0)
                {
                    continue;
                }

                nPos += 71;
                cP = (char)nPos;

                int nTam = cS.Length;
                if (nTam > 9) { nTam = 9; }

                nPos = -1;

                if (cS.Substring(0, nTam).Contains(cP))
                {
                    nPos = cS.Substring(0, nTam).IndexOf(cP);
                }

                if (nPos >= 0)
                {
                    cS += nPos.ToString();

                }
                else
                {
                    cS += cP;
                }

            }

            return cS;
        }


        public string DesCobrir(string cCodigo)
        {
            string cL = "";
            string cS = "";
            char cP;
            int nPos = 0;

            for (int i = 0; i < cCodigo.Length; i++)
            {
                cL = cCodigo.Substring(i, 1);

                if (cDecimal.Contains(cL))
                {
                    nPos = Int32.Parse(cL);
                    cS += cS.Substring(nPos, 1);
                }
                else
                {
                    cP = cCodigo[i];
                    nPos = (int)cP - 71;
                    cS += cAlfabeto.Substring(nPos, 1);
                }
            }

            return cS;
        }


        public string CobrirHTML(string cCodigo)
        {
            string cL = "";
            string cS = "";
            int nPos = 0;
            char cP;

            for (int i = 0; i < cCodigo.Length; i++)
            {
                cL = cCodigo.Substring(i, 1);

                nPos = cAlfHTML.IndexOf(cL);

                if (nPos < 0)
                {
                    LogFile.Log("CobrirHTML: Caractere nao encontrado! char: " + cL);
                    continue;
                }

                nPos += 70;
                cP = (char)nPos;

                int nTam = cS.Length;
                if (nTam > 9) { nTam = 9; }

                nPos = -1;

                if (cS.Substring(0, nTam).Contains(cP))
                {
                    nPos = cS.Substring(0, nTam).IndexOf(cP);
                }

                if (nPos >= 0)
                {
                    cS += nPos.ToString();

                }
                else
                {
                    cS += cP;
                }

            }

            return cS;
        }


        public string DesCobrirHTML(string cCodigo)
        {
            string cL = "";
            string cS = "";
            int nPos = 0;
            char cP;

            for (int i = 0; i < cCodigo.Length; i++)
            {
                cL = cCodigo.Substring(i, 1);

                nPos = cDecimal.IndexOf(cL);

                if (nPos >= 0)
                {
                    cS += cS.Substring(nPos, 1);
                }
                else
                {
                    cP = cCodigo[i];
                    nPos = (int)cP - 70;
                    cS += cAlfHTML.Substring(nPos, 1);
                }
            }

            return cS;
        }


        public string HTMLAcento(string cCod)
        {
            string cAcento = "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸƒ";
            string[] aHTMLAcento = {"192", "193", "194", "195", "196", "197", "198", "199",
                                    "200", "201", "202", "203", "204", "205", "206", "207",
                                    "208", "209", "210", "211", "212", "213", "214", "216",
                                    "217", "218", "219", "220", "221", "222", "223", "224",
                                    "225", "226", "227", "228", "229", "230", "231", "232",
                                    "233", "234", "235", "236", "237", "238", "239", "240",
                                    "241", "242", "243", "244", "245", "246", "248", "249",
                                    "250", "251", "252", "253", "254", "255", "338", "339",
                                    "352", "353", "376", "402"};
            string cL = "";
            string cS = "";
            int nPos = 0;

            for (int i = 0; i < cCod.Length; i++)
            {
                cL = cCod.Substring(i, 1);
                nPos = cAcento.IndexOf(cL);

                if (nPos >= 0)
                {
                    cS += "&#" + aHTMLAcento[nPos] + ";";
                }
                else
                {
                    cS += cL;
                }

            }

            return cS;

        }


        public string JSONAcento(string cCod)
        {
            string cAcento = "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸƒ";
            /*string[] aJSONAcento = {"\xC0", "\xC1", "\xC2", "\xC3", "\xC4", "\xC5", "\xC6", "\xC7", "\xC8", "\xC9", "\xCA", "\xCB", "\xCC", "\xCD", "\xCF",
                                    "\xD0", "\xD1", "\xD2", "\xD3", "\xD4", "\xD5", "\xD6",         "\xD8", "\xD9", "\xDA", "\xDB", "\xDC", "\xDD", "\xDF",
                                    "\xE0", "\xE1", "\xE2", "\xE3", "\xE4", "\xE5", "\xE6", "\xE7", "\xE8", "\xE9", "\xEA", "\xEB", "\xEC", "\xED", "\xEF",
                                    "\xF0", "\xF1", "\xF2", "\xF3", "\xF4", "\xF5", "\xF6",         "\xF8", "\xF9", "\xFA", "\xFB", "\xFC", "\xFD", "\xFF",
                                    "\u0152","\u0160","\u0161","\u0178","\u0192"};*/
            string[] aJSONAcento = {"!!xC0", "!!xC1", "!!xC2", "!!xC3", "!!xC4", "!!xC5", "!!xC6", "!!xC7", "!!xC8", "!!xC9", "!!xCA", "!!xCB", "!!xCC", "!!xCD", "!!xCE", "!!xCF",
                                    "!!xD0", "!!xD1", "!!xD2", "!!xD3", "!!xD4", "!!xD5", "!!xD6",          "!!xD8", "!!xD9", "!!xDA", "!!xDB", "!!xDC", "!!xDD", "!!xDE", "!!xDF",
                                    "!!xE0", "!!xE1", "!!xE2", "!!xE3", "!!xE4", "!!xE5", "!!xE6", "!!xE7", "!!xE8", "!!xE9", "!!xEA", "!!xEB", "!!xEC", "!!xED", "!!xEE", "!!xEF",
                                    "!!xF0", "!!xF1", "!!xF2", "!!xF3", "!!xF4", "!!xF5", "!!xF6",          "!!xF8", "!!xF9", "!!xFA", "!!xFB", "!!xFC", "!!xFD", "!!xFE", "!!xFF",
                                    "!!u0152","!!u0153","!!u0160","!!u0161","!!u0178","!!u0192"};
            string cL = "";
            string cS = "";
            int nPos = 0;

            for (int i = 0; i < cCod.Length; i++)
            {
                cL = cCod.Substring(i, 1);
                nPos = cAcento.IndexOf(cL);

                if (nPos >= 0)
                {
                    cS += aJSONAcento[nPos];
                }
                else
                {
                    cS += cL;
                }

            }

            return cS;

        }


        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }


        public string Base64Decode(string inputStr)
        {
            byte[] decodedByteArray =
              Convert.FromBase64CharArray(inputStr.ToCharArray(),
                                            0, inputStr.Length);
            string plainText = Encoding.UTF8.GetString(decodedByteArray);
            return (plainText);
        }


        public string ValCombo(string cOpcoes, string cValor)
        {
            int nP = 0;
            string cOpcao = "";

            nP = cOpcoes.IndexOf(cValor + "=");
            nP = (nP < 0 ? cOpcoes.IndexOf(cValor + ";") : nP);

            cOpcao = cOpcoes.Substring(nP, cOpcoes.Length - nP);
            nP = cOpcao.IndexOf(";");
            nP = (nP <= 0 ? cOpcao.Length : nP);
            cOpcao = cOpcao.Substring(0, nP);
            nP = cOpcao.IndexOf("=");

            if (nP <= 0)
            {
                return cOpcao;
            }
            else
            {
                return cOpcao.Substring(nP + 1, cOpcao.Length - (nP + 1));
            }
        }

        /*
        public MultiValueDictionary<int, string> FMatriz(string cMatriz, char cSep1 = ';', char cSep2 = '=') //"0=Nenhum;1=Direita;2=Esquerda", ";", "="
        {
            MultiValueDictionary<int, string> aMatriz = new MultiValueDictionary<int, string>();

            var aMatriz1 = cMatriz.Split(cSep1);
            string[] aMatriz2 = { };

            for (int i = 0; i < aMatriz1.Length; i++)
            {
                aMatriz2 = aMatriz1[i].Split(cSep2);
                aMatriz.Add(i, aMatriz2[0]);
                aMatriz.Add(i, aMatriz2[1]);
            }

            return aMatriz;
        }
        */

        public Dictionary<string, string> DMatriz(string cMatriz, char cSep1 = ';', char cSep2 = '=') //"0=Nenhum;1=Direita;2=Esquerda", ";", "="
        {
            Dictionary<string, string> aMatriz = new Dictionary<string, string>();

            var aMatriz1 = cMatriz.Split(cSep1);

            for (int i = 0; i < aMatriz1.Length; i++)
            {
                int nP = aMatriz1[i].IndexOf('=');
                string cPart1 = aMatriz1[i].Substring(0, nP);
                string cPart2 = aMatriz1[i].Substring(nP + 1);

                aMatriz.Add(cPart1, cPart2);
            }

            return aMatriz;
        }

        /*
        public MultiValueDictionary<string, string> SMatriz(string cMatriz, char cSep1 = ';', char cSep2 = '=') //"0=Nenhum;1=Direita;2=Esquerda", ";", "="
        {
            MultiValueDictionary<string, string> aMatriz = new MultiValueDictionary<string, string>();

            var aMatriz1 = cMatriz.Split(cSep1);
            string[] aMatriz2 = { };

            for (int i = 0; i < aMatriz1.Length; i++)
            {
                aMatriz2 = aMatriz1[i].Split(cSep2);
                aMatriz.Add(aMatriz2[0], aMatriz2[1]);
            }

            return aMatriz;
        }
        */

        public void ECaracterControle(string sentence) //encontrar carcteres especiais de controle
        {
            for (int ctr = 0; ctr < sentence.Length; ctr++)
            {
                if (Char.IsControl(sentence, ctr))
                    Console.WriteLine("Control character \\U{0} found in position {1}.",
                      Convert.ToInt32(sentence[ctr]).ToString("X4"), ctr);

            }
        }

        public string LimpaCaracterControle(string sentence) //retira carcteres especiais de controle
        {
            string cRetorno = "";

            for (int ctr = 0; ctr < sentence.Length; ctr++)
            {
                if (!Char.IsControl(sentence, ctr) || "000D|000A".Contains(Convert.ToInt32(sentence[ctr]).ToString("X4")))
                {
                    cRetorno += sentence[ctr];
                }
            }

            return cRetorno;
        }
    }
}

