using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bibliotec_mvc.Controllers
{
    [Route("[controller]")]
    public class LivroController : Controller
    {
        private readonly ILogger<LivroController> _logger;

        public LivroController(ILogger<LivroController> logger)
        {
            _logger = logger;
        }
        Context context = new Context();

        public IActionResult Index()
        {
            ViewBag.admin = HttpContext.Session.GetString("Admin");

            //Criar uma lista de livros
            List<Livro> Listalivros = context.Livro.ToList();

            // Verificar se o livro tem reserva ou não
            var livrosReservados = context.LivroReserva.ToDictionary(Livro => Livro.LivroID, livro => livro.DtReserva);

            ViewBag.Livros = Listalivros;
            ViewBag.LivrosComReserva = livrosReservados;

            return View();
        }
        [Route("Cadastro")]
        // Médoto que retorna a tela de Cadastro:

        public IActionResult Cadastro()
        {
            ViewBag.admin = HttpContext.Session.GetString("Admin");
            ViewBag.Categorias = context.Categoria.ToList();
            return View();
        }
        //Método para cadastrar livro:
        [Route("Cadastrar")]
        public IActionResult Cadastrar(IFormCollection form)
        {
            Livro novoLivro = new Livro();
            //O que o usuario escrever no formulario, sera atribuido no novoLivro:
            novoLivro.Nome = form["Nome"].ToString();
            novoLivro.Descricao = form["Descricao"].ToString();
            novoLivro.Editora = form["Editora"].ToString();
            novoLivro.Escritor = form["Escritor"].ToString();
            novoLivro.Idioma = form["Idioma"].ToString();

            //IMAGEM
            context.Livro.Add(novoLivro);


        }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}