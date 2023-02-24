using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using ExcelDataReader;
using OfficeOpenXml;

namespace Generador
{
    public partial class Form1 : Form
    {
        static Form1()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Mostrar el cuadro de diálogo del explorador de carpetas
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                string folderPath = folderBrowserDialog1.SelectedPath;

                // Configurar el objeto de aplicación de Excel
                Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
                excelApp.Visible = true;
                Workbook excelWorkbook = excelApp.Workbooks.Add();
                Worksheet excelWorksheet = (Worksheet)excelWorkbook.Worksheets[1];
                excelWorksheet.Cells[1, 1] = "Folio";
                excelWorksheet.Cells[1, 2] = "Serie";
                excelWorksheet.Cells[1, 3] = "Total";
                excelWorksheet.Cells[1, 4] = "Fecha";
                excelWorksheet.Cells[1, 5] = "UUID";
                int rowNum = 2;

                // Buscar archivos .xml en la carpeta y subcarpetas
                string[] files = Directory.GetFiles(folderPath, "*.xml", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    // Cargar el archivo .xml en un objeto XmlDocument
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(file);

                    // Obtener la información del cfdi:Comprobante y el cfdi:Complemento
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                    nsmgr.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
                    XmlNode comprobanteNode = xmlDoc.SelectSingleNode("//cfdi:Comprobante", nsmgr);
                    string folio = comprobanteNode.Attributes["Folio"].Value;
                    string serie = comprobanteNode.Attributes["Serie"].Value;
                    string total = comprobanteNode.Attributes["Total"].Value;
                    string fecha = comprobanteNode.Attributes["Fecha"].Value;
                    XmlNode complementoNode = xmlDoc.SelectSingleNode("//cfdi:Complemento", nsmgr);
                    string uuid = complementoNode.FirstChild.Attributes["UUID"].Value;

                    // Agregar la información al informe de Excel y al ListBox
                    excelWorksheet.Cells[rowNum, 1] = folio;
                    excelWorksheet.Cells[rowNum, 2] = serie;
                    excelWorksheet.Cells[rowNum, 3] = total;
                    excelWorksheet.Cells[rowNum, 4] = fecha;
                    excelWorksheet.Cells[rowNum, 5] = uuid;
                    listBox1.Items.Add(file);

                    rowNum++;
                }


                // Guardar y cerrar el archivo de Excel
                string fileName = "Informe.xml.xlsx";
                string filePath = Path.Combine(folderPath, fileName);
                excelWorkbook.SaveAs(filePath);
                excelWorkbook.Close();
                excelApp.Quit();
                MessageBox.Show("Informe guardado en " + filePath);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Archivos de Excel (*.xlsx)|*.xlsx|Todos los archivos (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog2 = new OpenFileDialog();
            openFileDialog2.Filter = "Archivos de Excel (*.xlsx)|*.xlsx|Todos los archivos (*.*)|*.*";
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = openFileDialog2.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            listBox1.Items.Clear(); //Limpiar ListBox antes de mostrar las coincidencias

            //Obtener el contenido de la columna "UUID" del primer archivo
            string[] uuids = File.ReadAllLines(textBox1.Text);
            string[] uuidsArray = new string[uuids.Length - 1]; //Crear arreglo sin la primera línea (encabezado)
            Array.Copy(uuids, 1, uuidsArray, 0, uuids.Length - 1); //Copiar el contenido sin la primera línea
            string uuidsTexto = String.Join(",", uuidsArray); //Convertir a texto separado por comas para usarlo en el método Contains

            //Obtener el contenido de la columna "TIMBRE" del segundo archivo
            string[] timbres = File.ReadAllLines(textBox2.Text);
            string[] timbresArray = new string[timbres.Length - 1]; //Crear arreglo sin la primera línea (encabezado)
            Array.Copy(timbres, 1, timbresArray, 0, timbres.Length - 1); //Copiar el contenido sin la primera línea

            //Recorrer cada línea del segundo archivo y buscar coincidencias en el primer archivo
            for (int i = 0; i < timbresArray.Length; i++)
            {
                if (uuidsTexto.Contains(timbresArray[i].Split(',')[0])) //Buscar el UUID en la columna TIMBRE del segundo archivo
                {
                    listBox2.Items.Add($"Coincidencia en línea {i + 2} del archivo {textBox2.Text}"); //Agregar la coincidencia al ListBox
                }
            }
           
        }

        private System.Data.DataTable ReadExcelFile(string filePath)
        {
            System.Data.DataTable dt = new System.Data.DataTable();

            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        while (reader.Read())
                        {
                            DataRow row = dt.NewRow();
                            for (int i = 5; i < reader.FieldCount; i++)
                            {
                                row[i] = reader.GetValue(i);
                            }
                            dt.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return dt;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string nombreArchivo = textBox3.Text;
            string directorio = @"C:\";
            try
            {

                string[] archivos = Directory.GetFiles(directorio, nombreArchivo, SearchOption.AllDirectories);

                listBox3.Items.Clear();
                foreach (string archivo in archivos)
                {
                    listBox3.Items.Add(archivo);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"No se pudo acceder a la carpeta: {ex.Message}", "Error de acceso", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

