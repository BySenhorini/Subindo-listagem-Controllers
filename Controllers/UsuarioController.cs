using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace Bibliotec_mvc.Controllers
{
    [Route("[controller]")]
    public class UsuarioController : Controller
    {
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(ILogger<UsuarioController> logger)
        {
            _logger = logger;

        }
        //Criando um obj da CLasse Context:
        Context context = new Context();

        // O método está retornando a View Usuario/Index.cshtml
        public IActionResult Index()
        {
            //Pegar as info da session que são necessárias para que apareça os detalhes do meu usuario.
            int id = int.Parse(HttpContext.Session.GetString("UsuarioID")!);
            ViewBag.admin = HttpContext.Session.GetString("Admin");

            // id = 1
            Usuario? usuarioEncontrado = context.Usuario.FirstOrDefault(usuario => usuario.UsuarioID == id)!;

            if (usuarioEncontrado == null)
            {
                return NotFound();
            }
            //Procurar o curso que meu usuario esta cadastrado
            Curso? cursoEncontrado = context.Curso.FirstOrDefault(Curso => Curso.CursoID == usuarioEncontrado.CursoID)!;

            //Verificar se o usuario possui ou nao o curso
            if (cursoEncontrado == null)
            {// O usuario nao possui o curso
                ViewBag.Curso = "O usuário não possui nenhum curso cadastrado.";
            }
            else
            {
                // o usuario possui o curso 123
                ViewBag.Curso = cursoEncontrado.Nome;
            }
            ViewBag.Nome = usuarioEncontrado.Nome;
            ViewBag.Email = usuarioEncontrado.Email;
            ViewBag.Contato = usuarioEncontrado.Contato;
            ViewBag.DtNasc = usuarioEncontrado.DtNascimento.ToString("dd/MM/yyyy");


            return View();
        }


        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}