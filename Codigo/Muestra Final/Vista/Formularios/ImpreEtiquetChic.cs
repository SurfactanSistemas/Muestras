using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;

namespace Vista
{
    public partial class ImpreEtiquetChic : Form
    {
        private DataTable dt;
        int[] CantidadesDeEtiquetas;
        string tipo;
        int posicion;
        const Boolean _EnProduccion = true; // Dejar en TRUE cuando se está en producción
                                            // Cambiar a FALSE cuando se trabaje en desarrollo.

        public ImpreEtiquetChic(DataTable ds, int[] CantidadesDeEtiquetas, string Tipo, int Posicion = 0)
        {
            InitializeComponent();
            this.dt = ds;
            this.CantidadesDeEtiquetas = CantidadesDeEtiquetas;
            this.tipo = Tipo;
            this.posicion = Posicion;
        }

        private void CRVEtiquetas_Load(object sender, EventArgs e)
        {
            int NumEtiquetaActual = 0;
            int HojaActual = 0;

            DSEtiq1[] Dss = new DSEtiq1[CalcularCantidadDeHojas()];

            Dss[HojaActual] = new DSEtiq1();

            if (this.tipo != "Grande")
            {
                // Dejamos en blanco las primeras etiquetas en caso de seleccionar una posicion que no se la de por defecto.
                for (int j = 0; j < this.posicion; j++)
                {
                    DataRow dr = dt.Rows[0];
                    Dss[0].Tables[NumEtiquetaActual].Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");

                    NumEtiquetaActual++;
                }
            }

            for (int row = 0; row < dt.Rows.Count; row++)
            {
                for (int i = 0; i < CantidadesDeEtiquetas[row]; i++)
                {
                    DataRow dr = dt.Rows[row];


                    if (tipo == "Frasco")
                    {

                        // Extraemos los datos SGA del Producto en caso de que tenga alguno cargado.
                        string[] SGA = _ObtenerDatosSGA(dr[2].ToString());

                        Dss[HojaActual].Tables[NumEtiquetaActual].Rows.Add(
                            dr[4].ToString(), dr[1].ToString(), dr[9].ToString(),
                            dr[6].ToString(), dr[7].ToString(), dr[0].ToString(),
                            dr[10].ToString(), dr[8].ToString(), dr[5].ToString(),
                            SGA[0].ToString(), SGA[1].ToString(), SGA[2].ToString(),
                            SGA[3].ToString(), SGA[4].ToString(), SGA[5].ToString(),
                            SGA[6].ToString(), SGA[7].ToString(), SGA[8].ToString(),
                            SGA[9].ToString(), SGA[10].ToString(), SGA[11].ToString()
                        );
                    }else {

                        Dss[HojaActual].Tables[NumEtiquetaActual].Rows.Add(
                            dr[4].ToString(), dr[1].ToString(), dr[9].ToString(),
                            dr[6].ToString(), dr[7].ToString(), dr[0].ToString(),
                            dr[10].ToString(), dr[8].ToString(), dr[5].ToString()
                        );
                    }

                    NumEtiquetaActual++;

                    if (EsFinalDeHoja(NumEtiquetaActual))
                    {
                        NumEtiquetaActual = 0;

                        HojaActual++;

                        if (HojaActual < CalcularCantidadDeHojas())
                        {
                            Dss[HojaActual] = new DSEtiq1();
                        }
                    }
                }
            }
            
            CRVEtiquetas.Visible = true;
            
            foreach (DSEtiq1 ds in Dss)
            {
                DSEtiq1 _ds = ds;
                ReportClass ECImp = ObtenerRptSegunTipo();

                ECImp.SetDataSource(_ds);

                //CRVEtiquetas.ReportSource = ECImp;

                ECImp.PrintToPrinter(1, true, 0, 0);

                Close();
            }
        }

        private string _DeterminarTabla(string codigo)
        {
            return (codigo.Substring(0, 2) == "PT") ? "DatosEtiqueta" : "DatosEtiquetaMP";        
        }

        private string _DeterminarColumna(string codigo)
        {
            return (codigo.Substring(0, 2) == "PT") ? "Terminado" : "Articulo";
        }

        private string[] _ObtenerDatosSGA(string _Codigo)
        {
            string[] datos = new string[12];

            try
            {
                using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["SurfactanSA"].ToString()))
                {
                    cn.Open();

                    string Tabla = _DeterminarTabla(_Codigo);
                    string Columna = _DeterminarColumna(_Codigo);

                    SqlCommand cmd = new SqlCommand("Select * From " + Tabla + " WHERE Renglon = '1' AND " + Columna + " = '" + _Codigo + "'", cn);
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.HasRows)
                    {
                        dr.Read();

                        int renglon = 8;
                        int pictograma = 1;

                        datos[0] = dr["Frase1"].ToString();
                        datos[1] = dr["Frase2"].ToString();
                        datos[2] = dr["Frase3"].ToString();
                        datos[3] = dr["Frase4"].ToString();
                        datos[4] = dr["Frase5"].ToString();
                        datos[5] = dr["Frase6"].ToString();
                        datos[6] = dr["Frase7"].ToString();

                        datos[7] = dr["Frase8"].ToString();


                        while (pictograma <= 9 && renglon <= 12)
                        {
                            if (Int32.Parse(dr["Pictograma" + pictograma].ToString()) != 0)
                            {
                                datos[renglon] = _ObtenerRutaImagenSGA(dr["Pictograma" + pictograma].ToString());
                                renglon++;
                            }

                            pictograma++;
                        }

                        while (renglon < 12)
                        {
                            datos[renglon] = _ObtenerRutaImagenSGA("0"); // Llenamos los campos de pictogramas faltantes con el rombo tachado.
                            renglon++;
                        }

                    }
                    else
                    {
                        int i = 0;

                        while (i < 12)
                        {
                            datos[i] = i >= 8 ? _ObtenerRutaImagenSGA("0") : ""; // Llenamos con rombos tachados los pictogramas.

                            i++;
                        }
                    }
                        
                }
            }
            catch (Exception)
            {
                throw new Exception("Ocurrio un error al querer consultar las especificaciones SGA del siguiente producto: " + _Codigo);
            }

            return datos;
        }

        private string _ObtenerRutaImagenSGA(string numero) 
        {
            if (numero == "0")
            {
                return _EnProduccion ? Path.GetFullPath(@".\SGA\SGA0.JPG") : Path.GetFullPath(@"..\..\Resources\SGA0.JPG");
            }
            // Envio la ruta dependiendo si se encuentra en produccion o no, debido a que mientras se desarrolla la raiz del ejecutable se resetea cada vez que se ejecuta.
            return _EnProduccion ? Path.GetFullPath(@".\SGA\SGA" + numero + ".png") : Path.GetFullPath(@"..\..\Resources\SGA" + numero + ".png");
        }
        
        private bool EsFinalDeHoja(int NumeroDeEtiquetaActual)
        {
            return NumeroDeEtiquetaActual == CantidadDeEtiquetasPorHojaSegunTipo();
        }

        private int CalcularCantidadDeHojas()
        {
            int corrimiento = 0;

            if (this.tipo != "Grande")
            {
                corrimiento += this.posicion;
            }

            return Convert.ToInt32( // Paso a número entero
                        Math.Ceiling( // Redondeo al entero más alto y próximo.
                            (CantidadDeEtiquetasTotales() + corrimiento) / CantidadDeEtiquetasPorHojaSegunTipo() // Sumo la posición por un tema de corrimiento.
                        )
                   );
        }

        private double CantidadDeEtiquetasTotales()
        {
            double total = 0;

            foreach (int cant in this.CantidadesDeEtiquetas)
            {
                total += cant;
            }

            return total;
        }

        public int CantidadDeEtiquetasPorHojaSegunTipo()
        {
            switch (this.tipo)
            {

                case "Chica": // Etiqueta Chica (8.5 x 4)

                    return 8;

                case "Grande": // Etiqueta Autoadhesiva (14 x 12.5)

                    return 1;

                case "Mediana": // Etiqueta Mediana

                    return 4;

                case "Frasco": // Etiqueta P/Frascos

                    return 7;

                default:

                    throw new Exception("Tipo de Etiqueta Inválida.");

            }
        }

        private ReportClass ObtenerRptSegunTipo()
        {
            switch (this.tipo)
            {

                case "Chica": // Etiqueta Chica (8.5 x 4)

                    return new EtiquetC();

                case "Grande": // Etiqueta Autoadhesiva (14 x 12.5)

                    return new EtiquetaGrand();

                case "Mediana": // Etiqueta Mediana

                    return new EtiquetaMediana();

                case "Frasco": // Etiqueta P/Frascos

                    return new EtiquetaFrasco();

                default:

                    throw new Exception("Tipo de Etiqueta Inválida.");

            }
        }
    }
}
