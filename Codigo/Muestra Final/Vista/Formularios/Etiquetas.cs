using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Vista
{
    public partial class Etiquetas : Form
    {
        DataTable DT = new DataTable();
        
        public Etiquetas(DataTable tabla)
        {
            InitializeComponent();
            DT = tabla;

            foreach (DataRow dr in tabla.Rows)
            {
                DGV_Etiquetas.Rows.Add(dr["Codigo"].ToString(), dr["DescriCliente"].ToString(), 1);
            }

            //DeterminarCantidadPosibleDeCorrimientos();

            CBTamañoEtiquetas.SelectedIndex = 0;
        }

        private void BTCancelar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BTAceptar_Click(object sender, EventArgs e)
        {
            try
            {
                // Forzamos la actualizacion de todas las cantidades.
                DGV_Etiquetas.EndEdit(DataGridViewDataErrorContexts.Commit);

                string TipoEtiqueta = DeterminarTipoEtiqueta();

                for (int RowIndex = 0; RowIndex < DGV_Etiquetas.RowCount; RowIndex++)
                {
                    if (CantidadInvalidaDeEtiquetas(RowIndex))
                    {
                        throw new Exception("Debe estar definidas todas las cantidades de Cada etiqueta.");
                    }
                }

                int[] CantidadesDeEtiquetas = DeterminarCantidadDeEtiquetas();
                int posicion = CBPosicion.SelectedIndex;

                ImpreEtiquetChic impre = new ImpreEtiquetChic(DT, CantidadesDeEtiquetas, TipoEtiqueta, posicion);

                impre.ShowDialog();

                Close();
                
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);  
            }    
        }

        private string DeterminarTipoEtiqueta()
        {
            switch (CBTamañoEtiquetas.SelectedIndex)
            {

                case 0: // Etiqueta Chica (8.5 x 4)

                    return "Chica";

                case 1: // Etiqueta Autoadhesiva (14 x 12.5)

                    return "Grande";

                case 2: // Etiqueta Mediana

                    return "Mediana";

                case 3: // Etiqueta P/Frascos

                    return "Frasco";

                default:

                    throw new Exception("Tipo de Etiqueta Inválida.");

            }
        }

        private void DeterminarCantidadPosibleDeCorrimientos()
        {
            // Limpiamos todos los items anteriores.
            CBPosicion.Items.Clear();

            int posiciones = 0;

            switch (DeterminarTipoEtiqueta())
            {
                case "Chica": // Etiqueta Chica (8.5 x 4)

                    posiciones = 8;
                    break;

                case "Grande": // Etiqueta Autoadhesiva (14 x 12.5)

                    posiciones = 1;
                    break;

                case "Mediana": // Etiqueta Mediana

                    posiciones = 4;
                    break;

                case "Frasco": // Etiqueta P/Frascos

                    posiciones = 7;
                    break;

                default:

                    throw new Exception("Tipo de Etiqueta Inválida.");
            }

            for (int i = 1; i < posiciones; i++)
            {
                CBPosicion.Items.Add(i);
            }

            CBPosicion.SelectedIndex = 0;
        }

        private int[] DeterminarCantidadDeEtiquetas()
        {
            int[] Cantidades = new int[DGV_Etiquetas.Rows.Count];

            for (int i = 0; i < DGV_Etiquetas.Rows.Count; i++)
            {
                Cantidades[i] = int.Parse(DGV_Etiquetas.Rows[i].Cells[2].Value.ToString());
            }

            return Cantidades;
        }

        private bool CantidadInvalidaDeEtiquetas(int Index)
        {
            return DeterminarCantidadDeEtiquetas().Length < 0;
        }

        private void CBTamañoEtiquetas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CBTamañoEtiquetas.SelectedIndex == 1) // Solo la desactivamos en el Caso de las Etiquetas Grandes.
            {
                CBPosicion.Enabled = false;
            }
            else
            {
                DeterminarCantidadPosibleDeCorrimientos();
                CBPosicion.Enabled = true;
                CBPosicion.Focus();
            }
        }
    }
}
