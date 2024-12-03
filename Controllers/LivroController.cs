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
            var livrosReservados = context.LivroReserva.ToDictionary(livro => livro.LivroID, livror => livror.DtReserva);

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
            if (form.Files.Count > 0)
            {
                var arquivo = form.Files[0];
                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens/Livros");
                if (!Directory.Exists(pasta))
                {
                    Directory.CreateDirectory(pasta);
                }
                var caminho = Path.Combine(pasta, arquivo.FileName);
                using (var stream = new FileStream(caminho, FileMode.Create))
                {
                    arquivo.CopyTo(stream);
                }
                novoLivro.Imagem = arquivo.FileName;



            }
            else
            {
                novoLivro.Imagem = "padrao.png";
            }

            //IMAGEM
            context.Livro.Add(novoLivro);

            context.SaveChanges();

            List<LivroCategoria> listaLivroCategorias = new List<LivroCategoria>();
            string[] categoriasSelecionadas = form["Categoria"].ToString().Split(',');

            foreach (string categoria in categoriasSelecionadas)
            {
                LivroCategoria livroCategoria = new LivroCategoria();
                livroCategoria.CategoriaID = int.Parse(categoria);
                livroCategoria.LivroID = novoLivro.LivroID;
                listaLivroCategorias.Add(livroCategoria);

            }
            context.LivroCategoria.AddRange(listaLivroCategorias);
            context.SaveChanges();

            return LocalRedirect("/Livro/Cadastro");




        }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}