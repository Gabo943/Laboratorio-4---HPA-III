using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CRUD_Productos
{
    public partial class Form1 : Form
    {
        private List<Producto> listaProductos;
        private Dictionary<string, object> myProducto = new Dictionary<string, object>();
            
        public Form1()
        {
            InitializeComponent();
            listaProductos = new List<Producto>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cargarProductos();
        }
        private void cargarProductos (string filtro = "")
        {
            dgvProductos.Rows.Clear();
            dgvProductos.Refresh();
            listaProductos = GetProducts.GetProductos(filtro);

            foreach (var prod in listaProductos)
            {
                Image img = null;

                if (prod.Imagen != null && prod.Imagen.Length > 0)
                {
                    using (MemoryStream ms = new MemoryStream(prod.Imagen))
                    {
                        using (Bitmap bmp = new Bitmap(ms))
                        {
                            img = new Bitmap(bmp); // Esto clona la imagen y evita que falle
                        }
                    }
                }

                dgvProductos.Rows.Add(prod.Id, prod.Nombre, prod.Precio, prod.Cantidad, img);
            }
        }

        private void txtBusqueda_TextChanged(object sender, EventArgs e)
        {
            cargarProductos(txtBusqueda.Text.Trim());
        }

        private void pbImagen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Seleccionar la imagen del producto";
                ofd.Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pbImagen.Image = Image.FromFile(ofd.FileName);
                    pbImagen.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!datosCorrectos())
            {
                return; // No vamos hacer nada - se detine en este punto, puedes crear un punto de interrupcíon
            }

            CargarDatosProductos();

            if (GetProducts.InsertSeguro("productos", myProducto));
            {
                MessageBox.Show("Se ha guardado satisfactoriamente el registro");
                // Aquí refrescas el grid volviendo a consultar la base de datos
                cargarProductos();
            }//fin del if InsertSeguro
        }
        private void CargarDatosProductos()
        {
            myProducto["Cantidad"] = int.Parse(txtCantidad.Text.Trim());
            myProducto["Precio"] = decimal.Parse(txtPrecio.Text.Trim());
            myProducto["Nombre"] = txtNombre.Text.Trim();
            myProducto["Imagen"] = ImageToByteArray(pbImagen.Image);
        }
        private byte[] ImageToByteArray(Image image)
        {
            if (image == null)
                return null;

            using (MemoryStream mMemoryStream = new MemoryStream())
            {
                // Si el RawFormat no es válido o proviene de memoria, guardamos en formato PNG por defecto
                System.Drawing.Imaging.ImageFormat formato = image.RawFormat;

                if (formato.Guid == System.Drawing.Imaging.ImageFormat.MemoryBmp.Guid || formato.Guid == Guid.Empty)
                {
                    formato = System.Drawing.Imaging.ImageFormat.Png;
                }

                image.Save(mMemoryStream, formato);
                return mMemoryStream.ToArray();
            }
        }
        private bool datosCorrectos()
        {
            if (txtNombre.Text.Trim().Equals(""))
            {
                MessageBox.Show("Ingrese el Nombre del Producto");
                return false;
            }

            if (txtPrecio.Text.Trim().Equals(""))
            {
                MessageBox.Show("Ingrese el Precio");
                return false;
            }

            if (txtCantidad.Text.Trim().Equals(""))
            {
                MessageBox.Show("Ingrese la Cantidad");
                return false;
            }

            if (!decimal.TryParse(txtPrecio.Text.Trim(), out decimal precio))
            {
                MessageBox.Show("Ingrese un Precio correcto");
                return false;
            }

            if (!int.TryParse(txtCantidad.Text.Trim(), out int cantidad))
            {
                MessageBox.Show("Ingrese una cantidad correcto");
                return false;
            }

            return true;
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            // 1. Validar que se haya seleccionado un registro (Folio/ID no vacío)
            if (string.IsNullOrWhiteSpace(txtFolio.Text))
            {
                MessageBox.Show("Por favor, selecciona un producto del DataGridView para modificar.",
                                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Validar que las entradas sean válidas
            if (!datosCorrectos())
            {
                return;
            }

            // 3. Cargar los datos del formulario al Diccionario
            CargarDatosProductos();

            int idProducto = int.Parse(txtFolio.Text.Trim());

            // 4. Ejecutar la actualización en la Base de Datos
            if (GetProducts.UpdateSeguro("productos", myProducto, idProducto))
            {
                MessageBox.Show("El registro se ha actualizado correctamente.",
                                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refrescar el DataGridView y limpiar campos
                cargarProductos();
                btnLimpiar_Click(null, null);
            }
            else
            {
                MessageBox.Show("No se pudo actualizar el registro. Verifique los datos.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtBusqueda.Text = "";
            txtCantidad.Text = "";
            txtNombre.Text = "";
            txtPrecio.Text = "";
            txtFolio.Text = "";
            pbImagen.Image = pbImagen.InitialImage;
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void dgvProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow fila = dgvProductos.Rows[e.RowIndex];

                txtFolio.Text = fila.Cells[0].Value?.ToString();
                txtNombre.Text = fila.Cells[1].Value?.ToString();
                txtPrecio.Text = fila.Cells[2].Value?.ToString();
                txtCantidad.Text = fila.Cells[3].Value?.ToString();

                if (dgvProductos.Rows[e.RowIndex].Cells["Imagen"].Value is Image img)
                {
                    pbImagen.Image = img;
                    pbImagen.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                else
                {
                    pbImagen.Image = pbImagen.InitialImage;
                }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            // 1. Validar que se haya seleccionado un registro (Folio/ID no vacío)
            if (string.IsNullOrWhiteSpace(txtFolio.Text))
            {
                MessageBox.Show("Por favor, selecciona un producto del DataGridView para eliminar.",
                                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idProducto = int.Parse(txtFolio.Text.Trim());
            string nombreProducto = txtNombre.Text.Trim();

            // 2. Pedir confirmación al usuario antes de borrar
            DialogResult respuesta = MessageBox.Show(
                $"¿Estás seguro de que deseas eliminar el producto '{nombreProducto}' (Folio: {idProducto})?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (respuesta == DialogResult.Yes)
            {
                // 3. Ejecutar la eliminación en SQL Server
                if (GetProducts.DeleteSeguro("productos", idProducto))
                {
                    MessageBox.Show("El registro ha sido eliminado correctamente.",
                                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refrescar la grilla y limpiar los campos de texto
                    cargarProductos();
                    btnLimpiar_Click(null, null);
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el registro. Es posible que ya no exista en la base de datos.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
