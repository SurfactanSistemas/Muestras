﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace ClassConexion
{
    public class Conexion
    {
        SqlConnection conexion;
        SqlDataAdapter adapter = new SqlDataAdapter();
        DataTable DT = new DataTable();

        string CS = ConfigurationManager.ConnectionStrings["SurfactanSA"].ConnectionString;

        //todo lo comun
        public void AbrirConexion()
        {
            conexion = new SqlConnection(ConfigurationManager.ConnectionStrings["SurfactanSA"].ConnectionString);
            conexion.Open();
        }

        //cs modificada
        public void AbrirConexion(string cs)
        {
            conexion = new SqlConnection(cs);
            conexion.Open();
        }

        public void CerrarConexion()
        {
            conexion.Close();
        }

        public DataTable TraerLista(string desde, string hasta)
        {
            AbrirConexion();

            // La fecha en la base de datos del cliente tiene cuatro digitos por lo que se debe colocar (0, 4)
            //Y FUNCIONAAAAAAAAAAAAAAAAA

            string des_año = desde.Substring(0, 4)+"0101";
            string hasta_año = hasta.Substring(0, 4)+"1231";

            //Modfico Razon por Cliente
            string str = "select M.Codigo as 'Id', M.Pedido, M.Fecha,";
            str += "Case When M.Ensayo <> '' Then M.Ensayo When M.Producto <> '' Then M.Producto When M.Articulo <> '' Then M.Articulo End as 'Codigo',";
            str += "M.Nombre, M.Cantidad, M.DescriCliente, M.Razon , M.Cliente, M.Observaciones, M.Fecha2 , M.Remito, M.HojaRuta,";
            str += "Case When M.Ensayo2 <> '' Then M.Ensayo2 When M.Producto2 <> '' Then M.Producto2 When M.Articulo2 <> '' Then M.Articulo2 End as 'CodigoConf',";
            str += "M.Nombre2, M.Lote2, M.Observaciones2, M.Cantidad2, ";
            str += "Case When M.Stock2 = '1' Then 'S' Else null End as 'ActualizarStock', M.OrdenTrabajo, M.Vendedor, (select distinct Lote1 from pedido p where p.pedido = m.pedido and (P.Terminado = M.Producto or P.Articulo = M.Articulo)) Lote1, V.Nombre as 'NombreVend'";
            //str += "Case When M.Stock2 = '1' Then 'S' Else null End as 'ActualizarStock', M.OrdenTrabajo, M.Vendedor, P.Lote1, V.Nombre as 'NombreVend'";
            str += "from Muestra M ";
            //str += "inner join Pedido P on P.Pedido = M.Pedido and (P.Terminado = M.Producto or P.Articulo = M.Articulo) ";
            //str += "inner join Pedido P on P.Pedido = M.Pedido ";
            str += "inner join Vendedor V on V.Vendedor = M.Vendedor ";
            //str += "where M.Fecha like '%/" + des_año.ToString() + "' ";
            str += "where M.OrdFecha between '" + des_año.ToString() + "' and '" + hasta_año.ToString() + "' ";
            str += "order by M.Pedido desc "; ;

            //string dada = "where M.OrdFecha between " + des_año.ToString() + " and " + hasta_año.ToString() + " ";




            //int des = int.Parse(desde);
            //int has = int.Parse(hasta);

            //int cantidad = (has - des);

            //if (cantidad > 0)
            //{
            //    for (int i = 1; i <= cantidad; i++)
            //    {
            //        int año = (int.Parse(des_año) + i);
            //        str += "union select  M.Codigo as 'Id', M.Pedido, M.Fecha,";
            //        str += "Case When M.Ensayo <> '' Then M.Ensayo When M.Producto <> '' Then M.Producto When M.Articulo <> '' Then M.Articulo End as 'Codigo',";
            //        str += "M.Nombre, M.Cantidad, M.DescriCliente, M.Razon , M.Cliente, M.Observaciones, M.Fecha2 , M.Remito, M.HojaRuta,";
            //        str += "Case When M.Ensayo2 <> '' Then M.Ensayo2 When M.Producto2 <> '' Then M.Producto2 When M.Articulo2 <> '' Then M.Articulo2 End as 'CodigoConf',";
            //        str += "M.Nombre2, M.Lote2, M.Observaciones2, M.Cantidad2, ";
            //        str += "Case When M.Stock2 = '1' Then 'S' Else null End as 'ActualizarStock', M.OrdenTrabajo, M.Vendedor, P.Lote1, V.Nombre as 'NombreVend'";
            //        str += "from Muestra M ";
            //        str += "inner join Pedido P on P.Pedido = M.Pedido and (P.Terminado = M.Producto or P.Articulo = M.Articulo) ";
            //        str += "inner join Vendedor V on V.Vendedor = M.Vendedor ";
            //        str += "where M.Fecha like '%/" + año.ToString() + "' order by M.Pedido desc "; ;
            //    }
            //}

            //str += "order by Pedido desc";
            //str += ") a left join Pedido P on P.Pedido = a.Pedido left join Vendedor V on V.Vendedor = a.Vendedor  order by a.Pedido desc";
            //where Lote1 <> 0

            adapter.SelectCommand = new SqlCommand(str, conexion);

            DT.Clear();

            adapter.Fill(DT);

            adapter.SelectCommand = new SqlCommand("");

            CerrarConexion();

            return DT;
        }

        private string _BuscarNombreML(string codigo, string pedido)
        {
            return this.BuscarNombreML(codigo.Trim(), pedido.Trim());
        }

        public DataRow TraerPeligroso(string codigo)
        {
            List<DataRow> filas = new List<DataRow>();
            DataTable tabla = new DataTable();

            AbrirConexion();

            string str = "select Clase, Intervencion, Naciones from Terminado where Codigo = '" + codigo + "'";

            adapter.SelectCommand = new SqlCommand(str, conexion);

            adapter.Fill(tabla);

            filas.Add(tabla.Rows[0]);

            CerrarConexion();

            return filas[0];
        }

        public void ActualizarMuestraLab(string str)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = str;

            AbrirConexion();

            cmd.Connection = conexion;
            cmd.ExecuteNonQuery();

            CerrarConexion();
        }


        public bool VerificarEnsayo2(string Id)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            bool tiene = false;

            string str = "select Ensayo2 from Muestra where Codigo = " + Id + "";

            cmd.CommandText = str;

            AbrirConexion();

            cmd.Connection = conexion;

            string value = System.Convert.ToString(cmd.ExecuteScalar());

            CerrarConexion();

            if (value == "") tiene = false;
            else { tiene = true; }

            return tiene;
        }

        public string BuscarLote1(string cod, string pedido)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            
            if (cod.StartsWith("DY"))
            {

                string[] auxi = cod.Split('-');
                int max = auxi[1].ToString().Length;

                for (int x = 0; x < 5 - max; x++)
                {

                    auxi[1] = "0" + auxi[1];

                }

                cod = string.Join("-", auxi).Trim();

            }

            string str = "select Lote1 from Pedido where Terminado = '" + cod + "' and Pedido = '" + pedido + "'";

            cmd.CommandText = str;

            AbrirConexion();

            cmd.Connection = conexion;

            string value = System.Convert.ToString(cmd.ExecuteScalar());

            CerrarConexion();

            if (value == "") value = "0";

            return value;
        }


        public string BuscarNombreML(string cod, string pedido)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            string str = "select DescriCliente from Muestra where Articulo = '" + cod + "' and Pedido = '" + pedido + "'";

            cmd.CommandText = str;

            AbrirConexion();

            cmd.Connection = conexion;

            string value = System.Convert.ToString(cmd.ExecuteScalar());

            CerrarConexion();

            return value;
        }

        public string TraerRemitoMax()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            string str = "select max(Remito) + 1 as Remito from Muestra";

            cmd.CommandText = str;

            AbrirConexion();

            cmd.Connection = conexion;

            string value = System.Convert.ToString(cmd.ExecuteScalar());

            CerrarConexion();

            return value;
        }

        public void ActualizarPedidoI(System.Windows.Forms.DataGridViewRow DGV)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            string cod = DGV.Cells[5].Value.ToString().Trim();

            if (cod.StartsWith("DY"))
            {

                string[] auxi = cod.Split('-');
                int max = auxi[1].ToString().Length;

                for (int x = 0; x < 5 - max; x++)
                {

                    auxi[1] = "0" + auxi[1];

                }

                cod = string.Join("-", auxi).Trim();

            }

            AbrirConexion();
            cmd.Connection = conexion;

            //Actualizo pedido
            cmd.CommandText = "update Pedido set Facturado = (Facturado + " + DGV.Cells[1].Value.ToString().Trim() + "),"
                    + " MarcaFactura = 0 where Pedido = '" + DGV.Cells[4].Value.ToString().Trim()
                    + "' and Terminado = '" + cod + "'";

            cmd.ExecuteNonQuery();

            CerrarConexion();
        }

        public void ActualizarArticulo(System.Windows.Forms.DataGridViewRow DGVRow)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            AbrirConexion(CS);
            cmd.Connection = conexion;

            cmd.CommandText = "update Articulo set Salidas = (Salidas + " + DGVRow.Cells[1].Value.ToString() + "),"
            + "Venta = (Venta - " + DGVRow.Cells[1].Value.ToString() + ") where Codigo = '" + DGVRow.Cells[6].Value.ToString() + "'";

            cmd.ExecuteNonQuery();

            CerrarConexion();
        }


        public void AltaMovlab(string txtClave, string txtCodigo, string txtArticulo, string txtTerminado, string txtTipo, string txtRenglon, string txtFecha, string txtFechaOrd, string txtLote, string txtCliente, double txtCantidad)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;


            AbrirConexion(CS);
            cmd.Connection = conexion;

            cmd.CommandText = "INSERT INTO  Movlab  ("
                            + "Clave,"
                            + "Codigo, "
                            + "Renglon, "
                            + "Fecha, "
                            + "Tipo, "
                            + "Articulo, "
                            + "Terminado, "
                            + "Cantidad, "
                            + "Fechaord, "
                            + "Movi, "
                            + "Tipomov, "
                            + "Observaciones, "
                            + "WDate, "
                            + "Marca, "
                            + "Lote "
                            + ") VALUES ("
                            + "'" + txtClave + "',"
                            + "'" + Convert.ToInt32(txtCodigo) + "',"
                            + "'" + Convert.ToInt32(txtRenglon) + "',"
                            + "'" + txtFecha + "',"
                            + "'" + txtTipo.Trim() + "',"
                            + "'" + txtArticulo.Trim() + "',"
                            + "'" + txtTerminado.Trim() + "',"
                            + "'" + txtCantidad.ToString().Replace(",", ".") + "',"
                            + "'" + txtFechaOrd + "',"
                            + "'" + "S" + "',"
                            + "'" + "1" + "',"
                            + "'" + ("Muestras a " + txtCliente.PadRight(50)).Substring(0,50) + "',"
                            + "'" + DateTime.Now.ToString("MM-dd-yyyy") + "',"
                            + "'',"
                            + "'" + Convert.ToInt32(txtLote.Trim()) + "'"
                            + ")";

            cmd.ExecuteNonQuery();

            CerrarConexion();
        }



        public bool BuscarEnLaudo(System.Windows.Forms.DataGridViewRow DGVRow)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            bool resultado;

            AbrirConexion(CS);
            cmd.Connection = conexion;

            cmd.CommandText = "select COUNT(*) from Laudo  where Laudo = " + DGVRow.Cells[3].Value.ToString()
                + " and Articulo = '" + DGVRow.Cells[6].Value.ToString() + "'";

            if (int.Parse(cmd.ExecuteScalar().ToString()) == 0)
            {
                resultado = false;
            }
            else { resultado = true; }

            CerrarConexion();
            return resultado;
        }

        public void RestarSaldoALaudo(System.Windows.Forms.DataGridViewRow DGVRow)
        {
            string cadena = "update Laudo set Saldo = (Saldo - " + DGVRow.Cells[1].Value.ToString() + ")"
            + "  where Laudo = " + DGVRow.Cells[3].Value.ToString()
                + " and Articulo = '" + DGVRow.Cells[6].Value.ToString() + "'";

            HacerUpdate(cadena);
        }

        private void HacerUpdate(string cadena)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            AbrirConexion(CS);
            cmd.Connection = conexion;

            cmd.CommandText = cadena;

            cmd.ExecuteNonQuery();

            CerrarConexion();
        }

        public bool BuscarEnGuia_Art(System.Windows.Forms.DataGridViewRow DGVRow)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            bool resultado;

            AbrirConexion(CS);
            cmd.Connection = conexion;

            cmd.CommandText = "select COUNT(*) from Guia  where Lote =" + DGVRow.Cells[3].Value.ToString()
                + " and  Articulo = '" + DGVRow.Cells[6].Value.ToString() + "'";

            if (int.Parse(cmd.ExecuteScalar().ToString()) == 0)
            {
                resultado = false;
            }
            else { resultado = true; }

            CerrarConexion();
            return resultado;
        }

        public void RestarSaldoAGuia_Art(System.Windows.Forms.DataGridViewRow DGVRow)
        {
            string cadena = "update Guia set Saldo = (Saldo - " + DGVRow.Cells[1].Value.ToString() + ")"
            + "  where Lote = " + DGVRow.Cells[3].Value.ToString()
                + " and Articulo = '" + DGVRow.Cells[6].Value.ToString() + "' and Saldo <> 0";

            HacerUpdate(cadena);
        }

        public void RestarSaldoAGuia_Ter(System.Windows.Forms.DataGridViewRow DGVRow)
        {
            string cadena = "update Guia set Saldo = (Saldo - " + DGVRow.Cells[1].Value.ToString() + ")"
            + "  where Lote = " + DGVRow.Cells[3].Value.ToString()
                + " and Terminado = '" + DGVRow.Cells[6].Value.ToString() + "' and Saldo <> 0";

            HacerUpdate(cadena);
        }

        public void ActualizarTerminado(System.Windows.Forms.DataGridViewRow DGVRow)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            AbrirConexion(CS);
            cmd.Connection = conexion;
            
            cmd.CommandText = "update Terminado set Salidas = (Salidas + " + DGVRow.Cells[1].Value.ToString().Trim() + "),"
            + "Pedido = (Pedido - " + DGVRow.Cells[1].Value.ToString().Trim() + ") where Codigo = '" + DGVRow.Cells[6].Value.ToString().Trim() + "'";

            cmd.ExecuteNonQuery();

            CerrarConexion();
        }

        public bool RemitoExistente(string numero_remito)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            bool resultado = false;

            AbrirConexion(CS);
            cmd.Connection = conexion;

            cmd.CommandText = "select count(Remito) from Muestra WHERE Remito = '" + numero_remito.Trim() + "'";

            if (int.Parse(cmd.ExecuteScalar().ToString()) != 0)
            {
                resultado = true;
            }

            CerrarConexion();
            return resultado;
        }

        public bool BuscarEnGuia_Ter(System.Windows.Forms.DataGridViewRow DGVRow)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            bool resultado;

            AbrirConexion(CS);
            cmd.Connection = conexion;

            cmd.CommandText = "select COUNT(*) from Guia  where Lote =" + DGVRow.Cells[3].Value.ToString()
                + " and  Terminado = '" + DGVRow.Cells[6].Value.ToString().Trim() + "'";

            if (int.Parse(cmd.ExecuteScalar().ToString()) == 0)
            {
                resultado = false;
            }
            else { resultado = true; }

            CerrarConexion();
            return resultado;
        }

        public bool BuscarEnHoja(System.Windows.Forms.DataGridViewRow DGVRow)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            bool resultado;

            AbrirConexion(CS);
            cmd.Connection = conexion;

            cmd.CommandText = "select COUNT(*) from Hoja  where Hoja = " + DGVRow.Cells[3].Value.ToString()
                + " and Producto = '" + DGVRow.Cells[6].Value.ToString().Trim() + "'";

            if (int.Parse(cmd.ExecuteScalar().ToString()) == 0) resultado = false;
            else { resultado = true; }

            CerrarConexion();
            return resultado;
        }

        public void RestarSaldoAHoja(System.Windows.Forms.DataGridViewRow DGVRow)
        {
            string cadena = "update Hoja set Saldo = (Saldo - " + DGVRow.Cells[1].Value.ToString() + ")"
            + "  where Hoja = " + DGVRow.Cells[3].Value.ToString()
                + " and Producto = '" + DGVRow.Cells[6].Value.ToString() + "'";

            HacerUpdate(cadena);
        }

        public void BuscarTipoPedido(System.Windows.Forms.DataGridViewRow DGVRow)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            AbrirConexion();

            cmd.Connection = conexion;

            string cod = DGVRow.Cells[5].Value.ToString().Trim();

            if (cod.StartsWith("DY"))
            {
                string[] auxi = cod.Split('-');
                int max = auxi[1].ToString().Length;

                for (int x = 0; x < 5 - max; x++)
                {

                    auxi[1] = "0" + auxi[1];

                }

                cod = string.Join("-", auxi).Trim();

            }


            //Busco el tipopedido (esto es lo primero que tengo que hacer, antes de actualizar en otro lado) modifico la connectionstring
            cmd.CommandText = "select TipoPedido from Pedido where  Pedido = '" + DGVRow.Cells[4].Value.ToString().Trim()
                + "' and Terminado = '" + cod + "'";

            string value = System.Convert.ToString(cmd.ExecuteScalar());

            if (value == "5" || value == "1") CS = ConfigurationManager.ConnectionStrings["SurfactanSA"].ConnectionString;
            else if (value == "4") CS = ConfigurationManager.ConnectionStrings["Surfactan_VII"].ConnectionString;
            else CS = ConfigurationManager.ConnectionStrings["Surfactan_V"].ConnectionString;

            CerrarConexion();
        }

        public void ActualizarMuestraRemito(System.Windows.Forms.DataGridViewRow DGV, string NroRemito)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            AbrirConexion();
            cmd.Connection = conexion;

            //Actualizo Muestras (no existe la columna PARTIDA)
            cmd.CommandText = "update Muestra set Remito = " + NroRemito.ToString().Trim() + ", Lote2 = " + DGV.Cells[3].Value.ToString().Trim() + " where Codigo = '" + DGV.Cells[2].Value.ToString().Trim() + "'";

            cmd.ExecuteNonQuery();

            CerrarConexion();
        }

        public DataRow BuscarCliente(string cliente)
        {
            List<DataRow> filas = new List<DataRow>();
            DataTable tabla = new DataTable();

            AbrirConexion();

            string str = "select Cliente, Direccion, Localidad, Cuit, DirEntrega From Cliente where Razon = '" + cliente + "'";

            adapter.SelectCommand = new SqlCommand(str, conexion);

            adapter.Fill(tabla);
            if (tabla.Rows.Count == 0) throw new Exception("No se encontro cliente o la muestra no posee cliente");
            filas.Add(tabla.Rows[0]);

            CerrarConexion();

            return filas[0];
        }

        public DataRow ListarPeligroso(int Id)
        {
            List<DataRow> filas = new List<DataRow>();
            DataTable tabla = new DataTable();

            AbrirConexion();

            string str = "select Peligroso, PeligrosoII from Muestra where Codigo = '" + Id + "'";

            adapter.SelectCommand = new SqlCommand(str, conexion);

            adapter.Fill(tabla);

            filas.Add(tabla.Rows[0]);

            CerrarConexion();

            return filas[0];
        }



        public DataTable BuscarListaRemito(string numero_remito, string cliente)
        {
            DataTable tabla = new DataTable();

            AbrirConexion();

            string str = "select DescriCliente, Cantidad, Peligroso, PeligrosoII, Articulo, Pedido from Muestra where Remito = '" + numero_remito.Trim() + "' and Cliente = '" + cliente.Trim() + "'";

            adapter.SelectCommand = new SqlCommand(str, conexion);

            tabla.Clear();

            adapter.Fill(tabla);

            adapter.SelectCommand = new SqlCommand("");

            CerrarConexion();

            return tabla;
        }


        public void EliminarMuestra(string Id)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            AbrirConexion();
            cmd.Connection = conexion;


            //elimino  Muestras 
            cmd.CommandText = "delete Muestra where codigo = '" + Id + "'";

            cmd.ExecuteNonQuery();

            CerrarConexion();
        }



        public int BuscarPedido(string Id)
        {
            int Pedido;

            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            AbrirConexion();
            cmd.Connection = conexion;


            //elimino  Muestras 
            cmd.CommandText = "Select Pedido from Muestra where codigo = '" + Id + "'";

            Pedido = int.Parse(cmd.ExecuteScalar().ToString());

            //cmd.ExecuteNonQuery();

            CerrarConexion();

            return Pedido;
        }


        public void ModificCantidad(int Pedido, string Producto)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            AbrirConexion();
            cmd.Connection = conexion;

            string cod = Producto;

            if (cod.StartsWith("DY"))
            {

                string[] auxi = cod.Split('-');
                int max = auxi[1].ToString().Length;

                for (int x = 0; x < 5 - max; x++)
                {

                    auxi[1] = "0" + auxi[1];

                }

                cod = string.Join("-", auxi).Trim();

            }

            //elimino  Muestras 
            cmd.CommandText = "update Pedido set Cantidad = " + 0 + " where Pedido = " + Pedido + " and Terminado = '" + cod + "'";

            cmd.ExecuteNonQuery();

            CerrarConexion();
        }

        public string TraerMovlabMax()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            string str = "select max(Codigo) + 1 as Codigo from Movlab";

            cmd.CommandText = str;

            AbrirConexion(CS);

            cmd.Connection = conexion;

            string value = System.Convert.ToString(cmd.ExecuteScalar());

            CerrarConexion();

            return value;
        }

    }
}