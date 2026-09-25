using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUD_Productos
{
    internal class GetProducts
    {
        public static List<Producto> GetProductos(string filtro)
        {
            List<Producto> listaProductos = new List<Producto>();
            string query = "SELECT id, nombre, precio, cantidad, imagen FROM productos";

            if (!string.IsNullOrEmpty(filtro))
            {
                query += " WHERE id LIKE @filtro OR nombre LIKE @filtro " +
                         " OR CAST(precio AS VARCHAR) LIKE @filtro OR CAST(cantidad AS VARCHAR) LIKE @filtro";
            }

            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                if (conn == null) return listaProductos;

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 30;
                    // Si hay filtro, agregamos el parámetro para evitar inyección SQL
                    if (!string.IsNullOrEmpty(filtro))
                    {
                        cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");
                    }

                    // Ejecutamos el lector de datos
                    using (SqlDataReader mReader = cmd.ExecuteReader())
                    {
                        // Recorremos el lector fila por fila mientras haya registros
                        while (mReader.Read())
                        {
                            Producto prod = new Producto();

                            // Mapeamos los campos de la base de datos a las propiedades de tu clase Producto
                            prod.Id = Convert.ToInt32(mReader["id"]);
                            prod.Nombre = mReader["nombre"].ToString();
                            prod.Precio = Convert.ToDecimal(mReader["precio"]);
                            prod.Cantidad = Convert.ToInt32(mReader["cantidad"]);

                            prod.Imagen = mReader["imagen"] != DBNull.Value ? (byte[])mReader["imagen"] : null;

                            // Agregamos el objeto listo a la lista genérica
                            listaProductos.Add(prod);
                        }
                        mReader.Close();
                    } // SqlDataReader
                } // SqlCommand
            } // SqlConnection

            return listaProductos;
        }
        public static bool InsertSeguro(string tbName, Dictionary<string, object> data)
        {
            var columns = string.Join(", ", data.Keys);
            var placeholders = "@" + string.Join(", @", data.Keys);

            string sql = $"INSERT INTO {tbName} ({columns}) VALUES ({placeholders})";

            try
            {
                // Pedimos la conexión usando nuestra clase externa
                using (SqlConnection conexion = Conexion.ObtenerConexion())
                {
                    if (conexion == null) return false;

                    using (SqlCommand stmt = new SqlCommand(sql, conexion))
                    {
                        foreach (var kvp in data)
                        {
                            stmt.Parameters.AddWithValue("@" + kvp.Key, kvp.Value ?? DBNull.Value);
                        }

                        stmt.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error en INSERT: " + ex.Message);
                return false;
            }
        }
        public static bool UpdateSeguro(string tbName, Dictionary<string, object> data, int id)
        {
            var setClause = string.Join(", ", data.Keys.Select(k => $"{k} = @{k}"));

            string sql = $"UPDATE {tbName} SET {setClause} WHERE id = @IdProducto";

            try
            {
                using (SqlConnection conexion = Conexion.ObtenerConexion())
                {
                    if (conexion == null) return false;

                    using (SqlCommand stmt = new SqlCommand(sql, conexion))
                    {
                        // Parámetros dinámicos
                        foreach (var kvp in data)
                        {
                            stmt.Parameters.AddWithValue("@" + kvp.Key, kvp.Value ?? DBNull.Value);
                        }

                        // Parámetro de la condición WHERE
                        stmt.Parameters.AddWithValue("@IdProducto", id);

                        int filasAfectadas = stmt.ExecuteNonQuery();
                        return filasAfectadas > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error en UPDATE: " + ex.Message);
                return false;
            }
        }
        public static bool DeleteSeguro(string tbName, int id)
        {
            string sql = $"DELETE FROM {tbName} WHERE id = @IdProducto";

            try
            {
                using (SqlConnection conexion = Conexion.ObtenerConexion())
                {
                    if (conexion == null) return false;

                    using (SqlCommand stmt = new SqlCommand(sql, conexion))
                    {
                        stmt.Parameters.AddWithValue("@IdProducto", id);

                        int filasAfectadas = stmt.ExecuteNonQuery();
                        return filasAfectadas > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error en DELETE: " + ex.Message);
                return false;
            }
        }
    }
}
