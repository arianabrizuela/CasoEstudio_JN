using Microsoft.AspNetCore.Mvc;
using CasoEstudio2.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace CasoEstudio2.Controllers
{
    public class CasasController : Controller
    {
        private readonly string _connectionString;

        public CasasController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }


        // Lista todas las casas
        public IActionResult Consulta()
        {
            List<CasasModel> lista = new List<CasasModel>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("sp_ConsultarCasas", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new CasasModel
                    {
                        DescripcionCasa = dr["DescripcionCasa"].ToString(),
                        PrecioCasa = (decimal)dr["PrecioCasa"],
                        UsuarioAlquiler = dr["UsuarioAlquiler"]?.ToString(),
                        Estado = dr["Estado"].ToString(),
                        FechaAlquiler = dr["FechaAlquiler"].ToString()
                    });
                }
            }
            return View(lista);
        }

        // Mostrar casas disponibles para alquilar
        public IActionResult Alquiler()
        {
            List<CasasModel> casasDisponibles = new List<CasasModel>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("sp_CasasDisponibles", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    casasDisponibles.Add(new CasasModel
                    {
                        IdCasa = (long)dr["IdCasa"],
                        DescripcionCasa = dr["DescripcionCasa"].ToString(),
                        PrecioCasa = (decimal)dr["PrecioCasa"]
                    });
                }
            }
            return View(casasDisponibles);
        }

        // Procesar alquiler POST
        [HttpPost]
        public IActionResult Alquiler(long IdCasa, string UsuarioAlquiler)
        {
            if (string.IsNullOrEmpty(UsuarioAlquiler))
            {
                ViewBag.Error = "Debe ingresar el nombre del usuario";
                return Alquiler();
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("sp_AlquilarCasa", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdCasa", IdCasa);
                cmd.Parameters.AddWithValue("@UsuarioAlquiler", UsuarioAlquiler);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Consulta");
        }
    }
}
