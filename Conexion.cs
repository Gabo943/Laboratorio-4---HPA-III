using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace CRUD_Productos
{
    public class Conexion
    {
        private static string cadanaConexion = "Data Source= DESKTOP-U5P8OI6\\SQLEXPRESS ;Initial Catalog=productosdb;Integrated Security=True;TrustServerCertificate=True";
        public static SqlConnection ObtenerConexion()
        {
            try
            {
                SqlConnection conexion = new SqlConnection(cadanaConexion);
                conexion.Open();
                return conexion;
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error al conectar: " + ex.Message);
                return null;
            }
        } 
    }
}
