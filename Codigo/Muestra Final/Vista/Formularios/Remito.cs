using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ClassConexion;

namespace Vista
{
    public partial class Remito : Form
    {
        Conexion Cs = new Conexion();
        bool reimprimir = false;

        DataTable DT = new DataTable();
        string Datos;
        string DirEnt;
        private string datos;
        private DataTable dt;
        private string DirEntrega;
        private string CodClient;
        private string DirClient;
        private string LocalidadClient;
        private string Cuit;
        private string cliente;

        //  Ultimo nro de mov de laboratorio
        private string MovLabNumero;
        private string MovlabRenglon;
        private string txtTrabajo;



             
        public Remito(string datos, DataTable dt, List<string> erroresLote, List<string> sinEnsayo, string DirEntrega, string CodClient, string DirClient, string LocalidadClient, string Cuit, string cliente)
        {
            InitializeComponent();
            AsignarDatos(datos);

            //reimprimir
            this.datos = datos;
            this.dt = dt;
            this.DirEntrega = DirEntrega;
            this.CodClient = CodClient;
            this.DirClient = DirClient;
            this.LocalidadClient = LocalidadClient;
            this.Cuit = Cuit;
            this.cliente = cliente;


            DGV_Remito.DataSource = dt;

            if (erroresLote.Count > 0 || sinEnsayo.Count > 0)
            {
                BTAceptar.Enabled = false;
                string errores = "Se han producido los siguientes errores:  \n";
                foreach (string s in erroresLote)
                {
                    errores += "La muestra " + s + " no posee numero de Lote en la tabla Pedido \n";
                }

                foreach (string s in sinEnsayo)
                {
                    errores += "La muestra " + s + " no posee ensayo \n";
                }
                MessageBox.Show(errores, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);  
            }


        }

        private void AsignarDatos(string datos)
        {            
            string[] datos_separados = datos.Split(';');

            TBCliente.Text = datos_separados[1];
            if (datos_separados[0] == "") TBNumRemito.Text = Cs.TraerRemitoMax();
            else {
                TBNumRemito.Text = datos_separados[0];
                reimprimir = true;
            }
            TBFecha.Text = DateTime.Now.ToString("dd/MM/yyyy"); //DateTime.Now.ToShortDateString();

            
        }
        
        private void BTCancelar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BTAceptar_Click(object sender, EventArgs e)
        {
            string _Clave, _Codigo, _Renglon, _Fecha, _CLiente, _Articulo, _Terminado, _Cantidad, _Fechaord, _Movi, _Tipo, _Tipomov, _Observaciones, _WDate, _Marca, _Lote;

            try
            {
                if (TBNumRemito.Text == "") throw new Exception("Se debe ingresar el numero de remito");
                if (reimprimir)
                {
                    ImpreRemito impre = new ImpreRemito(dt, DirEntrega, CodClient, DirClient, LocalidadClient, Cuit, cliente);
                    impre.ShowDialog();
                }
                else
                {

                    for (int i = 0; i < DGV_Remito.Rows.Count; i++)
                    {
                        string codigo = DGV_Remito.Rows[i].Cells[5].Value.ToString();
                        string aux = DGV_Remito.Rows[i].Cells[1].Value.ToString();
                        
                        aux = aux.Trim();

                        aux = aux.StartsWith(".") ? "0" + aux : aux;// Hay casos en lo que se cargo como por ejemplo ".1" en vez de "0.1"

                        double Cantidad = Convert.ToDouble(aux.Replace(".", ","));

                        if (codigo.StartsWith("PT") || codigo.StartsWith("DY") || codigo.StartsWith("YQ") || codigo.StartsWith("YF"))
                        {
                            Cs.ActualizarPedidoI(DGV_Remito.Rows[i]);
                        }

                        Cs.ActualizarMuestraRemito(DGV_Remito.Rows[i], TBNumRemito.Text);

                        Cs.BuscarTipoPedido(DGV_Remito.Rows[i]);

                        // buscar ultimo numero de movvlab
                        MovLabNumero = Cs.TraerMovlabMax();

                        //la siguiente linea no hace falta porque ya suma uno en la consulta SQL
                        //int NumMov = int.Parse(MovLabNumero) + 1;
                        //string MovLab = NumMov.ToString();

                        //Reviso que cantidad de caracteres tiene el codigo devuelto
                        int CantCar = MovLabNumero.Length;

                        //Con la variable i del modulo saco el orden del movimiento, como empieza por 0 
                        //le sumo 1
                        string orden = (i + 1).ToString();

                        //Consulto si el orden es de un solo digito, si es asi le pongo un 0 adelante
                        if (orden.Length == 1) orden = "0" + orden;

                        //Sumo el string del movimiento mas el orden
                        string Clave = MovLabNumero + orden;

                        //realizo el for para saber cuantos ceros van a anteceder a la clave obtenida
                        for (int u = 0; u < (8 - Clave.Length); u++)
                        {
                            Clave = "0" + Clave;
                        }

                        _Clave = Clave;
                        _Codigo = MovLabNumero;
                        _Fecha = TBFecha.Text.Trim();
                        
                        _Cantidad = Cantidad.ToString(); //DGV_Remito.Rows[i].Cells[3].Value.ToString();
                        _Fechaord = String.Join("", TBFecha.Text.Split("/".ToCharArray()).Reverse());
                        _WDate = DateTime.Now.ToString("MM-dd-yyyy");
                        _Lote = DGV_Remito.Rows[i].Cells[3].Value.ToString();
                        

                        //SE MODIFICA A CONTINUACION PORQUE AL SER ML NO SE ACTUALIZA NADA
                        if (codigo.StartsWith("DY")) //|| codigo.StartsWith("ML"))
                        {
                            Cs.ActualizarArticulo(DGV_Remito.Rows[i]);

                            _Tipo = "M";
                            _Terminado = "  -     -   ";
                            _Articulo = codigo;
                            
                            //No se en que parte se tiene la fecha
                            //Fila["OrdenFecha"] = Fila[2].ToString().Substring(6,4) + Fila[2].ToString().Substring(2, 4) + Fila[2].ToString().Substring(0, 2);

                            // Grabar registro
                            Cs.AltaMovlab(_Clave, _Codigo, _Articulo, _Terminado, _Tipo ,orden, _Fecha, _Fechaord, _Lote, TBCliente.Text, Cantidad);

                            if (Cs.BuscarEnLaudo(DGV_Remito.Rows[i])) Cs.RestarSaldoALaudo(DGV_Remito.Rows[i]);
                            else if (Cs.BuscarEnGuia_Art(DGV_Remito.Rows[i])) Cs.RestarSaldoAGuia_Art(DGV_Remito.Rows[i]);
                            else throw new Exception("No se encontro la Muestra en las tablas Laudo ni Guia");
                        }
                         //SE MODIFICO A ELSE IF PARA QUE ENTRE EN CASO QUE NO SEA DY NI ML ENTRE Y SI ES YF O YQ LA CANTIDAD DEBE SER MAYOR A 1
                            //SINO NO SE ACTUALIZA LOTE
                        else if ((codigo.StartsWith("YQ") && Cantidad > 1) || (codigo.StartsWith("YF") && Cantidad > 1) || codigo.StartsWith("PT"))
                        {

                            Cs.ActualizarTerminado(DGV_Remito.Rows[i]);

                            _Tipo = "T";
                            _Terminado = codigo;
                            _Articulo = "  -     -   ";

                            //// Grabar registro
                            Cs.AltaMovlab(_Clave, _Codigo, _Articulo, _Terminado, _Tipo, orden, _Fecha, _Fechaord, _Lote, TBCliente.Text, Cantidad);
                    
                            if (Cs.BuscarEnHoja(DGV_Remito.Rows[i])) Cs.RestarSaldoAHoja(DGV_Remito.Rows[i]);
                            else if (Cs.BuscarEnGuia_Ter(DGV_Remito.Rows[i])) Cs.RestarSaldoAGuia_Ter(DGV_Remito.Rows[i]);
                            else throw new Exception("No se encontro la Muestra en las tablas Hoja ni Guia");
                        }
                    }
                }

                ImpreRemito impre_1 = new ImpreRemito(dt, DirEntrega, CodClient, DirClient, LocalidadClient, Cuit, cliente);
                impre_1.ShowDialog();

                Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}