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
    public partial class ImpreRemito : Form
    {
        private DataTable DT;
        private string DirEnt;
        private string Datos;
        private string CodClient;
        private string DirClient;
        private string LocalidadClient;
        private string Cuit;
        private string cliente;


       
        

        public ImpreRemito(DataTable DT, string DirEnt, string CodClient, string DirClient, string LocalidadClient, string Cuit, string cliente)
        {
            // TODO: Complete member initialization
            InitializeComponent();
            this.DT = DT;
            this.DirEnt = DirEnt;
            this.CodClient = CodClient;
            this.DirClient = DirClient;
            this.LocalidadClient = LocalidadClient;
            this.Cuit = Cuit;
            this.cliente = cliente;
        }

        private void ImpreRemito_Load(object sender, EventArgs e)
        {
            DSRemito1 Ds = new DSRemito1();
            Ds.Tables[0].Rows.Add
                (new object[]
                {
                    cliente,
                    DirClient,
                    LocalidadClient,
                    DirEnt,
                    CodClient,
                    Cuit,

                }
                );

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                DataRow dr = DT.Rows[i];
                Ds.Tables[i+1].Rows.Add
                    (new object[]
                    {
                        dr[0].ToString(),
                        dr[1].ToString() + "KG Netos",
                        dr["Peligroso"].ToString(),
                        dr["PeligrosoII"].ToString(),
                    }
                    );
            }

            CRVRemito.Visible = true;
            ImpRemito RImp = new ImpRemito();
            RImp.SetDataSource(Ds);
            CRVRemito.ReportSource = RImp;
            
            
        }


    }
}
